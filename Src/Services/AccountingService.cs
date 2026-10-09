namespace SinaMN75U.Services;

public interface IAccountingService {
	Task<UResponse<AccountingReportResponse?>> Report(AccountingReportParams p, CancellationToken ct);
	Task TakeCommission(Guid? organizationId, decimal amount, string detail, List<KeyValue> keyValues, Guid sourceId, Guid? placeId, CancellationToken ct);
	Task Post(Guid? organizationId, TagVoucher source, Guid? sourceId, Guid? placeId, Guid? personId, string description, DateTime date, CancellationToken ct, params AccountingLeg[] legs);
	Task SyncCharge(Guid organizationId, Guid sourceId, Guid placeId, Guid personId, DateTime date, string description, Dictionary<TagAccount, decimal> target, CancellationToken ct, decimal vatPercent = 0);
	Task<decimal> VatPercentOf(Guid organizationId, CancellationToken ct);
	Task<UResponse<IEnumerable<TaxInvoiceItem>?>> ReadTaxInvoices(LedgerReportParams p, CancellationToken ct);
	Task<UResponse?> PostReceipt(Guid? organizationId, Guid invoiceId, Guid placeId, Guid personId, Guid? contractId, decimal amount, InvoiceReceiveParams p, string description, Guid registeredBy, CancellationToken ct);
	Task<UResponse<Guid?>> CreateAccount(AccountCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<AccountResponse>?>> ReadAccounts(AccountReadParams p, CancellationToken ct);
	Task<UResponse> UpdateAccount(AccountUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteAccount(IdParams p, CancellationToken ct);
	Task<UResponse<Guid?>> CreateVoucher(VoucherCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<VoucherResponse>?>> ReadVouchers(VoucherReadParams p, CancellationToken ct);
	Task<UResponse> DeleteVoucher(IdParams p, CancellationToken ct);
	Task<UResponse<LedgerResponse?>> ReadLedger(LedgerReadParams p, CancellationToken ct);
	Task<UResponse<LedgerReportResponse?>> ReadLedgerReport(LedgerReportParams p, CancellationToken ct);
	Task<UResponse<Guid?>> CreateCheck(CheckCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<CheckResponse>?>> ReadChecks(CheckReadParams p, CancellationToken ct);
	Task<UResponse> SetCheckStatus(CheckStatusParams p, CancellationToken ct);
	Task<UResponse> RequestOrganizationSettlement(OrganizationSettlementRequestParams p, CancellationToken ct);
	Task<UResponse> ProcessOrganizationSettlement(OrganizationSettlementProcessParams p, CancellationToken ct);
	Task RemindChecks(CancellationToken ct);
	Task<bool> IsAccountOf(Guid organizationId, Guid accountId, CancellationToken ct);
}

public interface IAccountingSource {
	Task ReopenPayment(Guid sourceId, decimal amount, CancellationToken ct);
	Task<List<AccountingDue>> Outstanding(Guid organizationId, DateTime now, CancellationToken ct);
}

public sealed record AccountingDue(Guid PersonId, DateTime DueDate, decimal Amount);

public sealed record AccountingLeg(TagAccount Role, decimal Amount, Guid? AccountId = null);

public class AccountingService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IWalletService ws,
	IOrganizationService os,
	IServiceProvider sp
) : IAccountingService {
	private static readonly TagWalletTxn[] SpendingTags = [
		TagWalletTxn.MobileAndNationalCodeVerification, TagWalletTxn.ZipCodeToAddressDetail,
		TagWalletTxn.VehicleViolationsDetail, TagWalletTxn.DrivingLicenceStatus, TagWalletTxn.LicencePlateDetail,
		TagWalletTxn.DrivingLicenceNegativePoint, TagWalletTxn.IBanToBankAccountDetail, TagWalletTxn.FreewayTolls,
		TagWalletTxn.MerchantCreationFee, TagWalletTxn.ChargeSimPin, TagWalletTxn.ChargeSimTopup, TagWalletTxn.InternetSim
	];

	public async Task<UResponse<AccountingReportResponse?>> Report(AccountingReportParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<AccountingReportResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<AccountingReportResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (p.UserId != userData.Id && !userData.HasPermission(TagUser.PermissionManageWallets) && !(await db.Set<OrganizationEntity>().AnyAsync(x => x.Id == p.UserId && x.OwnerId == userData.Id, ct)))
			return new UResponse<AccountingReportResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		DateTime from = p.FromDate ?? DateTime.UtcNow.AddMonths(-1);
		DateTime to = p.ToDate ?? DateTime.UtcNow;

		IQueryable<WalletTxnEntity> wq = db.Set<WalletTxnEntity>().AsNoTracking().Where(x => x.CreatedAt >= from && x.CreatedAt <= to);
		if (p.UserId != null) wq = wq.Where(x => x.SenderId == p.UserId || x.ReceiverId == p.UserId);
		List<WalletRow> walletRows = await wq.Select(x => new WalletRow(x.Amount, x.Tags, x.SenderId, x.ReceiverId, x.CreatedAt)).ToListAsync(ct);

		List<TxnRow> txnRows = await db.Set<TxnEntity>().AsNoTracking()
			.Where(x => x.CreatedAt >= from && x.CreatedAt <= to)
			.Where(x => p.UserId == null || x.UserId == p.UserId)
			.Select(x => new TxnRow(x.Amount, x.Tags))
			.ToListAsync(ct);

		AccountingReportResponse r = new() {
			WalletTxnCount = walletRows.Count,
			TxnCount = txnRows.Count
		};

		if (p.UserId != null) BuildPerUser(r, walletRows, p.UserId.Value);
		else BuildSystemWide(r, walletRows);

		r.GatewayByType = txnRows
			.SelectMany(x => x.Tags.Select(t => (Tag: t, x.Amount)))
			.GroupBy(x => x.Tag)
			.Select(g => new AccountingBreakdownItem { Tag = (int)g.Key, TagName = g.Key.ToString(), Amount = g.Sum(i => i.Amount), Count = g.Count() })
			.OrderByDescending(i => i.Amount).ToList();

		r.TotalWalletBalance = p.UserId == null
			? await db.Set<WalletEntity>().SumAsync(x => (decimal?)x.Balance, ct) ?? 0
			: await db.Set<WalletEntity>().Where(x => x.CreatorId == p.UserId).SumAsync(x => (decimal?)x.Balance, ct) ?? 0;

		BuildTimeline(r, walletRows, p.UserId);

		return new UResponse<AccountingReportResponse?>(r);
	}

	private static void BuildPerUser(AccountingReportResponse r, List<WalletRow> rows, Guid userId) {
		List<WalletRow> incoming = rows.Where(x => x.ReceiverId == userId).ToList();
		List<WalletRow> outgoing = rows.Where(x => x.SenderId == userId).ToList();

		r.TotalIn = incoming.Sum(x => x.Amount);
		r.TotalOut = outgoing.Sum(x => x.Amount);
		r.Net = r.TotalIn - r.TotalOut;
		r.IncomeByType = GroupByTag(incoming);
		r.SpendingByType = GroupByTag(outgoing);
	}

	private static void BuildSystemWide(AccountingReportResponse r, List<WalletRow> rows) {
		List<WalletRow> incoming = rows.Where(x => x.Tags.Contains(TagWalletTxn.Charge)).ToList();
		List<WalletRow> outgoing = rows.Where(x => x.Tags.Any(t => SpendingTags.Contains(t))).ToList();

		r.TotalIn = incoming.Sum(x => x.Amount);
		r.TotalOut = outgoing.Sum(x => x.Amount);
		r.Net = r.TotalIn - r.TotalOut;
		r.IncomeByType = GroupByTag(incoming);
		r.SpendingByType = GroupByTag(outgoing);
	}

	private static List<AccountingBreakdownItem> GroupByTag(List<WalletRow> rows) => rows
		.SelectMany(x => x.Tags.Select(t => (Tag: t, x.Amount)))
		.GroupBy(x => x.Tag)
		.Select(g => new AccountingBreakdownItem { Tag = (int)g.Key, TagName = g.Key.ToString(), Amount = g.Sum(i => i.Amount), Count = g.Count() })
		.OrderByDescending(i => i.Amount).ToList();

	private static void BuildTimeline(AccountingReportResponse r, List<WalletRow> rows, Guid? userId) {
		r.Timeline = rows
			.GroupBy(x => x.CreatedAt.Date)
			.Select(g => new AccountingTimelineItem {
				Date = g.Key,
				In = g.Where(x => userId != null ? x.ReceiverId == userId : x.Tags.Contains(TagWalletTxn.Charge)).Sum(x => x.Amount),
				Out = g.Where(x => userId != null ? x.SenderId == userId : x.Tags.Any(t => SpendingTags.Contains(t))).Sum(x => x.Amount)
			})
			.OrderBy(i => i.Date).ToList();
	}

	private sealed record WalletRow(decimal Amount, ICollection<TagWalletTxn> Tags, Guid SenderId, Guid ReceiverId, DateTime CreatedAt);

	private sealed record TxnRow(decimal Amount, ICollection<TagTxn> Tags);

	public async Task TakeCommission(Guid? organizationId, decimal amount, string detail, List<KeyValue> keyValues, Guid sourceId, Guid? placeId, CancellationToken ct) {
		if (organizationId == null) return;
		decimal percent = await db.Set<OrganizationEntity>().Where(x => x.Id == organizationId).Select(x => x.JsonData.CommissionPercent).FirstOrDefaultAsync(ct);
		decimal commission = Math.Round(amount * percent / 100);
		if (commission <= 0) return;
		await ws.Transfer(new WalletTransferParams {
			SenderId = organizationId.Value,
			ReceiverId = Core.App.Users.SystemAdmin.Id,
			Amount = commission,
			Detail1 = detail,
			KeyValues = keyValues,
			TagWalletTxn = [TagWalletTxn.PlatformCommission],
			AllowOverdraft = true
		}, ct);
		await Post(organizationId, TagVoucher.Commission, sourceId, placeId, null, ls.Get("platformCommission", "fa"), DateTime.UtcNow, ct, new AccountingLeg(TagAccount.CommissionExpense, commission), new AccountingLeg(TagAccount.Wallet, -commission));
	}

	private async Task AddNotification(Guid userId, TagNotification tag, string title, string body, CancellationToken ct) =>
		await db.Set<NotificationEntity>().AddAsync(new NotificationEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = userId,
			UserId = userId,
			Tags = [tag, TagNotification.Unread],
			JsonData = new NotificationJson { Detail1 = title, Detail2 = body }
		}, ct);

	private static readonly (string Code, string Title, TagAccount[] Tags)[] DefaultAccounts = [
		("1101", "صندوق", [TagAccount.Asset, TagAccount.Cash]),
		("1102", "بانک", [TagAccount.Asset, TagAccount.Bank]),
		("1103", "تنخواه", [TagAccount.Asset, TagAccount.PettyCash]),
		("1104", "کیف پول سامانه", [TagAccount.Asset, TagAccount.Wallet]),
		("1105", "وجوه در راه", [TagAccount.Asset, TagAccount.InTransit]),
		("1201", "بدهکاران (ساکنان و مهمانان)", [TagAccount.Asset, TagAccount.Receivable]),
		("1202", "اسناد دریافتنی", [TagAccount.Asset, TagAccount.ChecksReceivable]),
		("1203", "مالیات بر ارزش افزوده‌ی خرید", [TagAccount.Asset, TagAccount.VatReceivable]),
		("1301", "موجودی کالا", [TagAccount.Asset, TagAccount.Inventory]),
		("2101", "ودیعه‌ی ساکنان", [TagAccount.Liability, TagAccount.DepositsHeld]),
		("2102", "اسناد پرداختنی", [TagAccount.Liability, TagAccount.ChecksPayable]),
		("2103", "بستانکاران", [TagAccount.Liability, TagAccount.Payables]),
		("2104", "مالیات بر ارزش افزوده‌ی فروش", [TagAccount.Liability, TagAccount.VatPayable]),
		("3101", "سرمایه", [TagAccount.Equity, TagAccount.Capital]),
		("4101", "درآمد اجاره‌ی خوابگاه", [TagAccount.Income, TagAccount.RentIncome]),
		("4102", "درآمد اقامت هتل", [TagAccount.Income, TagAccount.HotelIncome]),
		("4103", "درآمد خدمات", [TagAccount.Income, TagAccount.ServiceIncome]),
		("4104", "درآمد جریمه‌ی دیرکرد", [TagAccount.Income, TagAccount.PenaltyIncome]),
		("4105", "درآمد خسارت و کسورات", [TagAccount.Income, TagAccount.DamageIncome]),
		("5101", "کمیسیون سامانه", [TagAccount.Expense, TagAccount.CommissionExpense]),
		("5201", "حقوق و دستمزد", [TagAccount.Expense]),
		("5202", "آب، برق و گاز", [TagAccount.Expense]),
		("5203", "تعمیرات و نگهداری", [TagAccount.Expense]),
		("5204", "مواد غذایی", [TagAccount.Expense]),
		("5205", "نظافت و بهداشت", [TagAccount.Expense]),
		("5206", "اجاره‌ی ساختمان", [TagAccount.Expense]),
		("5207", "مصرف کالا و ملزومات", [TagAccount.Expense, TagAccount.ConsumptionExpense]),
		("5299", "سایر هزینه‌ها", [TagAccount.Expense])
	];

	private static readonly TagAccount[] ChargeRoles = [TagAccount.DepositsHeld, TagAccount.RentIncome, TagAccount.ServiceIncome, TagAccount.PenaltyIncome, TagAccount.HotelIncome, TagAccount.DamageIncome, TagAccount.VatPayable];

	private sealed record Entry(Guid AccountId, decimal Debit, decimal Credit, Guid? PersonId, string? Description);

	private readonly Dictionary<Guid, List<AccountEntity>> _accounts = [];

	private readonly List<VoucherEntity> _pending = [];

	private static bool IsRole(TagAccount t) => (int)t >= 300;

	private static bool IsMoneyBox(AccountEntity a) => a.Tags.Contains(TagAccount.Cash) || a.Tags.Contains(TagAccount.Bank) || a.Tags.Contains(TagAccount.PettyCash);

	private async Task<(JwtClaimData? User, UResponse? Error)> BooksUser(string? token, Guid organizationId, TagUser permission, CancellationToken ct) {
		JwtClaimData? u = ts.ExtractClaims(token);
		if (u == null) return (null, new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue")));
		if (u.IsExpired) return (null, new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired")));
		if (!await db.Set<OrganizationEntity>().AnyAsync(x => x.Id == organizationId, ct)) return (null, new UResponse(Usc.NotFound, ls.Get("organizationNotFound")));
		bool allowed = OrganizationService.IsFull(u) && await os.HasModule(u, organizationId, TagModule.Accounting, ct) ||
		               await os.HasOrganizationPermission(u, organizationId, permission, ct) ||
		               permission == TagUser.PermissionViewAccounting && await os.HasOrganizationPermission(u, organizationId, TagUser.PermissionManageAccounting, ct);
		return allowed ? (u, null) : (null, new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction")));
	}

	private async Task<List<AccountEntity>> AccountsOf(Guid organizationId, CancellationToken ct) {
		if (_accounts.TryGetValue(organizationId, out List<AccountEntity>? list)) return list;
		list = await db.Set<AccountEntity>().Where(x => x.OrganizationId == organizationId).ToListAsync(ct);
		List<AccountEntity> current = list;
		DateTime now = DateTime.UtcNow;
		List<AccountEntity> missing = DefaultAccounts
			.Where(a => current.Count == 0 || a.Tags.Any(IsRole) && !a.Tags.Where(IsRole).Any(r => current.Any(x => x.Tags.Contains(r))))
			.Select(a => new AccountEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = organizationId,
				CreatedAt = now,
				Tags = a.Tags.ToList(),
				Code = current.Any(x => x.Code == a.Code) ? a.Code + "9" : a.Code,
				Title = a.Title,
				OrganizationId = organizationId,
				JsonData = new AccountJson()
			}).ToList();
		if (missing.Count != 0) {
			await db.Set<AccountEntity>().AddRangeAsync(missing, ct);
			list = [..list, ..missing];
		}

		_accounts[organizationId] = list;
		return list;
	}

	private List<VoucherEntity> PendingVouchers() => _pending.Where(x => db.Entry(x).State == EntityState.Added).ToList();

	private async Task<Guid> AddVoucher(Guid organizationId, ICollection<TagVoucher> tags, Guid? sourceId, Guid? placeId, DateTime date, string? description, Guid? registeredBy, IEnumerable<Entry> lines, CancellationToken ct) {
		Guid id = Guid.CreateVersion7();
		DateTime now = DateTime.UtcNow;
		int last = await db.Set<VoucherEntity>().Where(x => x.OrganizationId == organizationId).MaxAsync(x => (int?)x.Number, ct) ?? 0;
		VoucherEntity v = new() {
			Id = id,
			CreatorId = organizationId,
			CreatedAt = now,
			Tags = tags.Distinct().ToList(),
			Number = last + PendingVouchers().Count(x => x.OrganizationId == organizationId) + 1,
			Date = date,
			PlaceId = placeId,
			SourceId = sourceId,
			OrganizationId = organizationId,
			JsonData = new VoucherJson { Detail1 = description ?? "", RegisteredBy = registeredBy },
			Lines = lines.Select(l => new VoucherLineEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = organizationId,
				CreatedAt = now,
				Tags = tags.Distinct().ToList(),
				VoucherId = id,
				AccountId = l.AccountId,
				Debit = l.Debit,
				Credit = l.Credit,
				PersonId = l.PersonId,
				Description = l.Description
			}).ToList()
		};
		await db.Set<VoucherEntity>().AddAsync(v, ct);
		_pending.Add(v);
		return id;
	}

	public async Task Post(Guid? organizationId, TagVoucher source, Guid? sourceId, Guid? placeId, Guid? personId, string description, DateTime date, CancellationToken ct, params AccountingLeg[] legs) {
		if (organizationId == null) return;
		List<AccountEntity> accounts = await AccountsOf(organizationId.Value, ct);
		List<Entry> lines = legs
			.GroupBy(x => x.AccountId ?? accounts.First(a => a.Tags.Contains(x.Role)).Id)
			.Select(g => (AccountId: g.Key, Amount: Math.Round(g.Sum(x => x.Amount), 2)))
			.Where(x => x.Amount != 0)
			.Select(x => new Entry(x.AccountId, Math.Max(0, x.Amount), Math.Max(0, -x.Amount), personId, null))
			.ToList();
		if (lines.Count == 0) return;
		await AddVoucher(organizationId.Value, [TagVoucher.Auto, source], sourceId, placeId, date, description, null, lines, ct);
	}

	public async Task<decimal> VatPercentOf(Guid organizationId, CancellationToken ct) =>
		await db.Set<OrganizationEntity>().Where(x => x.Id == organizationId).Select(x => x.JsonData.VatPercent).FirstOrDefaultAsync(ct);

	public async Task SyncCharge(Guid organizationId, Guid sourceId, Guid placeId, Guid personId, DateTime date, string description, Dictionary<TagAccount, decimal> target, CancellationToken ct, decimal vatPercent = 0) {
		List<AccountEntity> accounts = await AccountsOf(organizationId, ct);
		if (vatPercent > 0)
			foreach (TagAccount role in target.Keys.Where(x => x != TagAccount.DepositsHeld && x != TagAccount.VatPayable).ToList()) {
				decimal vat = Math.Round(target[role] * vatPercent / (100 + vatPercent));
				target[role] -= vat;
				target[TagAccount.VatPayable] = target.GetValueOrDefault(TagAccount.VatPayable) + vat;
			}

		List<(Guid AccountId, decimal Amount)> posted = (await db.Set<VoucherLineEntity>()
				.Where(x => x.Voucher.SourceId == sourceId && x.Tags.Contains(TagVoucher.Invoice))
				.GroupBy(x => x.AccountId)
				.Select(g => new { g.Key, Amount = g.Sum(x => x.Credit - x.Debit) })
				.ToListAsync(ct))
			.Select(x => (x.Key, x.Amount))
			.Concat(PendingVouchers().Where(x => x.SourceId == sourceId && x.Tags.Contains(TagVoucher.Invoice)).SelectMany(x => x.Lines).Select(x => (x.AccountId, x.Credit - x.Debit)))
			.ToList();

		List<AccountingLeg> legs = [];
		foreach (TagAccount role in ChargeRoles) {
			Guid id = accounts.First(a => a.Tags.Contains(role)).Id;
			decimal delta = Math.Round(target.GetValueOrDefault(role) - posted.Where(x => x.AccountId == id).Sum(x => x.Amount), 2);
			if (delta == 0) continue;
			legs.Add(new AccountingLeg(role, -delta));
			legs.Add(new AccountingLeg(TagAccount.Receivable, delta));
		}

		await Post(organizationId, TagVoucher.Invoice, sourceId, placeId, personId, description, date, ct, legs.ToArray());
	}

	public async Task<UResponse?> PostReceipt(Guid? organizationId, Guid invoiceId, Guid placeId, Guid personId, Guid? contractId, decimal amount, InvoiceReceiveParams p, string description, Guid registeredBy, CancellationToken ct) {
		if (organizationId == null) return null;

		DateTime now = DateTime.UtcNow;
		if (p.Check != null) {
			if (p.Check.Number.IsNullOrEmpty()) return new UResponse(Usc.BadRequest, ls.Get("numberRequired"));
			Guid checkId = Guid.CreateVersion7();
			await db.Set<CheckEntity>().AddAsync(new CheckEntity {
				Id = checkId,
				CreatorId = organizationId.Value,
				CreatedAt = now,
				Tags = [TagCheck.Received, TagCheck.Pending],
				Amount = amount,
				DueDate = p.Check.DueDate,
				Number = p.Check.Number,
				Bank = p.Check.Bank,
				PersonId = personId,
				ContractId = contractId,
				PlaceId = placeId,
				OrganizationId = organizationId.Value,
				JsonData = new CheckJson { SayadId = p.Check.SayadId, Drawer = p.Check.Drawer, InvoiceId = invoiceId, RegisteredBy = registeredBy }
			}, ct);
			await Post(organizationId, TagVoucher.Check, checkId, placeId, personId, description, now, ct, new AccountingLeg(TagAccount.ChecksReceivable, amount), new AccountingLeg(TagAccount.Receivable, -amount));
			return null;
		}

		AccountEntity? box = (await AccountsOf(organizationId.Value, ct)).FirstOrDefault(x => x.Id == p.AccountId && IsMoneyBox(x));
		if (box == null) return new UResponse(Usc.BadRequest, ls.Get("selectACashOrBankAccount"));
		await Post(organizationId, TagVoucher.Receipt, invoiceId, placeId, personId, description, now, ct, new AccountingLeg(TagAccount.Cash, amount, box.Id), new AccountingLeg(TagAccount.Receivable, -amount));
		return null;
	}

	public async Task<UResponse<Guid?>> CreateAccount(AccountCreateParams p, CancellationToken ct) {
		(JwtClaimData? userData, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionManageAccounting, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);

		List<AccountEntity> accounts = await AccountsOf(p.OrganizationId, ct);
		if (accounts.Any(x => x.Code == p.Code)) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("accountCodeExists"));
		List<TagAccount> tags = p.Tags.Where(x => !IsRole(x)).Distinct().ToList();
		if (tags.Count(x => (int)x < 200) != 1) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("tagsIsRequired"));

		Guid id = Guid.CreateVersion7();
		await db.Set<AccountEntity>().AddAsync(new AccountEntity {
			Id = id,
			CreatorId = p.OrganizationId,
			CreatedAt = DateTime.UtcNow,
			Tags = tags,
			Code = p.Code,
			Title = p.Title,
			OrganizationId = p.OrganizationId,
			JsonData = new AccountJson { Detail1 = p.Detail1, Detail2 = p.Detail2, RegisteredBy = userData!.Id }
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<AccountResponse>?>> ReadAccounts(AccountReadParams p, CancellationToken ct) {
		(_, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionViewAccounting, ct);
		if (error != null) return new UResponse<IEnumerable<AccountResponse>?>(null, error.Status, error.Message);

		await AccountsOf(p.OrganizationId, ct);
		await db.SaveChangesAsync(ct);

		IQueryable<VoucherLineEntity> lines = db.Set<VoucherLineEntity>().Where(l =>
			l.Voucher.OrganizationId == p.OrganizationId &&
			(p.FromDate == null || l.Voucher.Date >= p.FromDate) &&
			(p.ToDate == null || l.Voucher.Date <= p.ToDate) &&
			(p.PlaceId == null || l.Voucher.PlaceId == p.PlaceId));

		return await db.Set<AccountEntity>().Where(x => x.OrganizationId == p.OrganizationId).ApplyReadParams(p).OrderBy(x => x.Code).Select(x => new AccountResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Code = x.Code,
			Title = x.Title,
			OrganizationId = x.OrganizationId,
			Debit = lines.Where(l => l.AccountId == x.Id).Sum(l => (decimal?)l.Debit) ?? 0,
			Credit = lines.Where(l => l.AccountId == x.Id).Sum(l => (decimal?)l.Credit) ?? 0,
			Balance = lines.Where(l => l.AccountId == x.Id).Sum(l => (decimal?)(l.Debit - l.Credit)) ?? 0
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateAccount(AccountUpdateParams p, CancellationToken ct) {
		AccountEntity? e = await db.Set<AccountEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ledgerAccountNotFound"));
		(_, UResponse? error) = await BooksUser(p.Token, e.OrganizationId, TagUser.PermissionManageAccounting, ct);
		if (error != null) return error;

		if (p.Code.IsNotNullOrEmpty() && p.Code != e.Code) {
			if (await db.Set<AccountEntity>().AnyAsync(x => x.OrganizationId == e.OrganizationId && x.Code == p.Code, ct)) return new UResponse(Usc.Conflict, ls.Get("accountCodeExists"));
			e.Code = p.Code;
		}

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		List<TagAccount> fixedTags = e.Tags.Where(x => (int)x < 200 || IsRole(x)).ToList();
		e.ApplyUpdateParam<AccountEntity, TagAccount, AccountJson>(p);
		e.Tags = fixedTags.Concat(e.Tags.Where(x => (int)x is >= 200 and < 300)).Distinct().ToList();
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteAccount(IdParams p, CancellationToken ct) {
		AccountEntity? e = await db.Set<AccountEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ledgerAccountNotFound"));
		(_, UResponse? error) = await BooksUser(p.Token, e.OrganizationId, TagUser.PermissionManageAccounting, ct);
		if (error != null) return error;
		if (e.Tags.Any(IsRole) || await db.Set<VoucherLineEntity>().AnyAsync(x => x.AccountId == e.Id, ct)) return new UResponse(Usc.Conflict, ls.Get("accountHasEntries"));

		await db.Set<AccountEntity>().Where(x => x.Id == e.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateVoucher(VoucherCreateParams p, CancellationToken ct) {
		(JwtClaimData? userData, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionManageAccounting, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);

		List<AccountEntity> accounts = await AccountsOf(p.OrganizationId, ct);
		if (p.Lines.Count < 2 || p.Lines.Any(x => x.Debit < 0 || x.Credit < 0 || x.Debit > 0 == x.Credit > 0 || !accounts.Any(a => a.Id == x.AccountId && !a.Tags.Contains(TagAccount.Inactive))))
			return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("amountIsNotValid"));
		if (p.Lines.Sum(x => x.Debit) != p.Lines.Sum(x => x.Credit)) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("voucherIsNotBalanced"));
		if (p.PlaceId != null && !await os.IsPlaceOf(p.OrganizationId, p.PlaceId.Value, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		Guid id = await AddVoucher(p.OrganizationId, [TagVoucher.Manual, ..p.Tags.Where(x => x != TagVoucher.Auto)], null, p.PlaceId, p.Date ?? DateTime.UtcNow, p.Detail1, userData!.Id,
			p.Lines.Select(x => new Entry(x.AccountId, x.Debit, x.Credit, x.PersonId, x.Description)), ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<VoucherResponse>?>> ReadVouchers(VoucherReadParams p, CancellationToken ct) {
		(_, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionViewAccounting, ct);
		if (error != null) return new UResponse<IEnumerable<VoucherResponse>?>(null, error.Status, error.Message);

		IQueryable<VoucherEntity> q = db.Set<VoucherEntity>().Where(x => x.OrganizationId == p.OrganizationId).ApplyReadParams(p);
		if (p.FromDate != null) q = q.Where(x => x.Date >= p.FromDate);
		if (p.ToDate != null) q = q.Where(x => x.Date <= p.ToDate);
		if (p.PlaceId != null) q = q.Where(x => x.PlaceId == p.PlaceId);
		if (p.SourceId != null) q = q.Where(x => x.SourceId == p.SourceId);
		if (p.PersonId != null) q = q.Where(x => x.Lines.Any(l => l.PersonId == p.PersonId));
		if (p.AccountId != null) q = q.Where(x => x.Lines.Any(l => l.AccountId == p.AccountId));

		return await q.OrderByDescending(x => x.Date).ThenByDescending(x => x.Number).Select(x => new VoucherResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Number = x.Number,
			Date = x.Date,
			PlaceId = x.PlaceId,
			SourceId = x.SourceId,
			OrganizationId = x.OrganizationId,
			Total = x.Lines.Sum(l => l.Debit),
			Lines = x.Lines.OrderByDescending(l => l.Debit).Select(l => new VoucherLineResponse {
				Id = l.Id,
				AccountId = l.AccountId,
				AccountCode = l.Account.Code,
				AccountTitle = l.Account.Title,
				PersonId = l.PersonId,
				PersonName = db.Set<UserEntity>().Where(u => u.Id == l.PersonId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault(),
				Debit = l.Debit,
				Credit = l.Credit,
				Description = l.Description
			}).ToList()
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> DeleteVoucher(IdParams p, CancellationToken ct) {
		VoucherEntity? e = await db.Set<VoucherEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("voucherNotFound"));
		(_, UResponse? error) = await BooksUser(p.Token, e.OrganizationId, TagUser.PermissionManageAccounting, ct);
		if (error != null) return error;
		if (!e.Tags.Contains(TagVoucher.Manual)) return new UResponse(Usc.Conflict, ls.Get("onlyManualVouchersCanBeDeleted"));

		await db.Set<VoucherEntity>().Where(x => x.Id == e.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<LedgerResponse?>> ReadLedger(LedgerReadParams p, CancellationToken ct) {
		(_, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionViewAccounting, ct);
		if (error != null) return new UResponse<LedgerResponse?>(null, error.Status, error.Message);

		IQueryable<VoucherLineEntity> q = db.Set<VoucherLineEntity>().Where(x => x.Voucher.OrganizationId == p.OrganizationId);
		if (p.AccountId != null) q = q.Where(x => x.AccountId == p.AccountId);
		if (p.AccountTags.IsNotNullOrEmpty()) q = q.Where(x => x.Account.Tags.Any(t => p.AccountTags!.Contains(t)));
		if (p.PersonId != null) q = q.Where(x => x.PersonId == p.PersonId);
		if (p.PlaceId != null) q = q.Where(x => x.Voucher.PlaceId == p.PlaceId);

		decimal opening = p.FromDate == null ? 0 : await q.Where(x => x.Voucher.Date < p.FromDate).SumAsync(x => (decimal?)(x.Debit - x.Credit), ct) ?? 0;
		if (p.FromDate != null) q = q.Where(x => x.Voucher.Date >= p.FromDate);
		if (p.ToDate != null) q = q.Where(x => x.Voucher.Date <= p.ToDate);

		List<LedgerLineResponse> lines = await q.OrderBy(x => x.Voucher.Date).ThenBy(x => x.Voucher.Number).Select(x => new LedgerLineResponse {
			VoucherId = x.VoucherId,
			Number = x.Voucher.Number,
			Date = x.Voucher.Date,
			Tags = x.Tags,
			Description = x.Description ?? x.Voucher.JsonData.Detail1,
			AccountId = x.AccountId,
			AccountTitle = x.Account.Title,
			PersonId = x.PersonId,
			PersonName = db.Set<UserEntity>().Where(u => u.Id == x.PersonId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault(),
			Debit = x.Debit,
			Credit = x.Credit
		}).ToListAsync(ct);

		decimal balance = opening;
		foreach (LedgerLineResponse l in lines) {
			balance += l.Debit - l.Credit;
			l.Balance = balance;
		}

		return new UResponse<LedgerResponse?>(new LedgerResponse {
			Opening = opening,
			TotalDebit = lines.Sum(x => x.Debit),
			TotalCredit = lines.Sum(x => x.Credit),
			Closing = balance,
			Lines = lines
		});
	}

	public async Task<UResponse<LedgerReportResponse?>> ReadLedgerReport(LedgerReportParams p, CancellationToken ct) {
		(_, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionViewAccounting, ct);
		if (error != null) return new UResponse<LedgerReportResponse?>(null, error.Status, error.Message);

		List<AccountEntity> accounts = await AccountsOf(p.OrganizationId, ct);
		await db.SaveChangesAsync(ct);

		var sums = await db.Set<VoucherLineEntity>()
			.Where(x => x.Voucher.OrganizationId == p.OrganizationId && (p.FromDate == null || x.Voucher.Date >= p.FromDate) && (p.ToDate == null || x.Voucher.Date <= p.ToDate))
			.GroupBy(x => new { x.AccountId, x.Voucher.PlaceId })
			.Select(g => new { g.Key.AccountId, g.Key.PlaceId, Debit = g.Sum(x => x.Debit), Credit = g.Sum(x => x.Credit) })
			.ToListAsync(ct);
		Dictionary<Guid, decimal> opening = p.FromDate == null
			? []
			: await db.Set<VoucherLineEntity>()
				.Where(x => x.Voucher.OrganizationId == p.OrganizationId && x.Voucher.Date < p.FromDate)
				.GroupBy(x => x.AccountId)
				.Select(g => new { g.Key, Amount = g.Sum(x => x.Debit - x.Credit) })
				.ToDictionaryAsync(x => x.Key, x => x.Amount, ct);

		LedgerReportResponse r = new();
		foreach (AccountEntity a in accounts.OrderBy(x => x.Code)) {
			decimal debit = sums.Where(x => x.AccountId == a.Id).Sum(x => x.Debit);
			decimal credit = sums.Where(x => x.AccountId == a.Id).Sum(x => x.Credit);
			if (a.Tags.Contains(TagAccount.Income) && credit != debit) r.Income.Add(new LedgerReportItem { AccountId = a.Id, Code = a.Code, Title = a.Title, Amount = credit - debit });
			if (a.Tags.Contains(TagAccount.Expense) && credit != debit) r.Expense.Add(new LedgerReportItem { AccountId = a.Id, Code = a.Code, Title = a.Title, Amount = debit - credit });
			if (!IsMoneyBox(a) && !a.Tags.Contains(TagAccount.Wallet)) continue;
			decimal o = opening.GetValueOrDefault(a.Id);
			r.MoneyBoxes.Add(new LedgerMoneyBoxItem { AccountId = a.Id, Title = a.Title, Tags = a.Tags, Opening = o, In = debit, Out = credit, Closing = o + debit - credit });
		}

		r.NetProfit = r.Income.Sum(x => x.Amount) - r.Expense.Sum(x => x.Amount);
		r.VatSales = accounts.Where(x => x.Tags.Contains(TagAccount.VatPayable)).Sum(a => sums.Where(x => x.AccountId == a.Id).Sum(x => x.Credit - x.Debit));
		r.VatPurchases = accounts.Where(x => x.Tags.Contains(TagAccount.VatReceivable)).Sum(a => sums.Where(x => x.AccountId == a.Id).Sum(x => x.Debit - x.Credit));
		r.VatDue = r.VatSales - r.VatPurchases;

		HashSet<Guid> incomeIds = accounts.Where(x => x.Tags.Contains(TagAccount.Income)).Select(x => x.Id).ToHashSet();
		HashSet<Guid> expenseIds = accounts.Where(x => x.Tags.Contains(TagAccount.Expense)).Select(x => x.Id).ToHashSet();
		Dictionary<Guid, string> titles = await os.PlaceTitles(p.OrganizationId, ct);
		r.Places = sums
			.Where(x => incomeIds.Contains(x.AccountId) || expenseIds.Contains(x.AccountId))
			.GroupBy(x => x.PlaceId)
			.Select(g => new LedgerPlaceItem {
				PlaceId = g.Key,
				Title = g.Key != null ? titles.GetValueOrDefault(g.Key.Value, "") : "",
				Income = g.Where(x => incomeIds.Contains(x.AccountId)).Sum(x => x.Credit - x.Debit),
				Expense = g.Where(x => expenseIds.Contains(x.AccountId)).Sum(x => x.Debit - x.Credit)
			}).ToList();

		DateTime now = DateTime.UtcNow;
		List<AccountingDue> due = [];
		foreach (IAccountingSource s in sp.GetServices<IAccountingSource>()) due.AddRange(await s.Outstanding(p.OrganizationId, now, ct));
		due = due.Where(x => x.Amount > 0).ToList();
		List<Guid> personIds = due.Select(x => x.PersonId).Distinct().ToList();
		var people = await db.Set<UserEntity>().Where(x => personIds.Contains(x.Id)).Select(x => new { x.Id, x.FirstName, x.LastName, x.PhoneNumber }).ToDictionaryAsync(x => x.Id, ct);
		r.Aging = due.GroupBy(x => x.PersonId).Select(g => {
			decimal Bucket(int min, int max) => g.Where(x => (now - x.DueDate).Days >= min && (now - x.DueDate).Days <= max).Sum(x => x.Amount);
			var person = people.GetValueOrDefault(g.Key);
			return new LedgerAgingItem {
				PersonId = g.Key,
				PersonName = person == null ? null : $"{person.FirstName} {person.LastName}".Trim(),
				PhoneNumber = person?.PhoneNumber,
				Days0 = Bucket(0, 30),
				Days30 = Bucket(31, 60),
				Days60 = Bucket(61, 90),
				Days90 = Bucket(91, int.MaxValue),
				Total = g.Sum(x => x.Amount)
			};
		}).OrderByDescending(x => x.Total).ToList();

		return new UResponse<LedgerReportResponse?>(r);
	}

	public async Task<UResponse<IEnumerable<TaxInvoiceItem>?>> ReadTaxInvoices(LedgerReportParams p, CancellationToken ct) {
		(_, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionViewAccounting, ct);
		if (error != null) return new UResponse<IEnumerable<TaxInvoiceItem>?>(null, error.Status, error.Message);

		List<AccountEntity> accounts = await AccountsOf(p.OrganizationId, ct);
		await db.SaveChangesAsync(ct);
		HashSet<Guid> incomeIds = accounts.Where(x => x.Tags.Contains(TagAccount.Income)).Select(x => x.Id).ToHashSet();
		HashSet<Guid> vatIds = accounts.Where(x => x.Tags.Contains(TagAccount.VatPayable)).Select(x => x.Id).ToHashSet();
		List<Guid> ids = await db.Set<VoucherEntity>()
			.Where(x => x.OrganizationId == p.OrganizationId && x.SourceId != null && x.Tags.Contains(TagVoucher.Invoice))
			.GroupBy(x => x.SourceId!.Value)
			.Where(g => (p.FromDate == null || g.Min(x => x.Date) >= p.FromDate) && (p.ToDate == null || g.Min(x => x.Date) <= p.ToDate))
			.Select(g => g.Key)
			.ToListAsync(ct);
		var lines = await db.Set<VoucherLineEntity>()
			.Where(x => x.Voucher.OrganizationId == p.OrganizationId && x.Tags.Contains(TagVoucher.Invoice) && ids.Contains(x.Voucher.SourceId!.Value))
			.Select(x => new { SourceId = x.Voucher.SourceId!.Value, x.Voucher.Number, x.Voucher.Date, x.Voucher.PlaceId, x.PersonId, Description = x.Voucher.JsonData.Detail1, x.AccountId, Amount = x.Credit - x.Debit })
			.ToListAsync(ct);

		string? serviceId = (await db.Set<OrganizationEntity>().Where(x => x.Id == p.OrganizationId).Select(x => x.JsonData.TaxServiceId).FirstOrDefaultAsync(ct));
		Dictionary<Guid, string> titles = await os.PlaceTitles(p.OrganizationId, ct);
		List<Guid> personIds = lines.Where(x => x.PersonId != null).Select(x => x.PersonId!.Value).Distinct().ToList();
		var people = await db.Set<UserEntity>().Where(x => personIds.Contains(x.Id)).Select(x => new { x.Id, x.FirstName, x.LastName, x.NationalCode, x.PhoneNumber }).ToDictionaryAsync(x => x.Id, ct);
		List<TaxInvoiceItem> items = lines.GroupBy(x => x.SourceId).Select(g => {
			var first = g.OrderBy(x => x.Number).First();
			var person = first.PersonId == null ? null : people.GetValueOrDefault(first.PersonId.Value);
			decimal amount = g.Where(x => incomeIds.Contains(x.AccountId)).Sum(x => x.Amount);
			decimal vat = g.Where(x => vatIds.Contains(x.AccountId)).Sum(x => x.Amount);
			return new TaxInvoiceItem {
				SourceId = g.Key,
				Number = first.Number,
				Date = first.Date,
				PersonId = first.PersonId,
				PersonName = person == null ? null : $"{person.FirstName} {person.LastName}".Trim(),
				NationalCode = person?.NationalCode,
				PhoneNumber = person?.PhoneNumber,
				PlaceTitle = first.PlaceId == null ? null : titles.GetValueOrDefault(first.PlaceId.Value),
				Description = first.Description,
				ServiceId = serviceId,
				Amount = amount,
				VatPercent = amount > 0 ? Math.Round(vat * 100 / amount) : 0,
				Vat = vat,
				Total = amount + vat
			};
		}).Where(x => x.Total > 0).OrderBy(x => x.Date).ToList();
		return new UResponse<IEnumerable<TaxInvoiceItem>?>(items);
	}

	public async Task<UResponse<Guid?>> CreateCheck(CheckCreateParams p, CancellationToken ct) {
		(JwtClaimData? userData, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionManageAccounting, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);

		List<TagCheck> kind = p.Tags.Where(x => x is TagCheck.Received or TagCheck.Issued or TagCheck.Guarantee).Distinct().ToList();
		if (kind.Count != 1) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("tagsIsRequired"));
		if (p.Amount <= 0) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("amountIsNotValid"));
		if (p.AccountId != null && (await AccountsOf(p.OrganizationId, ct)).All(x => x.Id != p.AccountId)) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("ledgerAccountNotFound"));
		if (p.PlaceId != null && !await os.IsPlaceOf(p.OrganizationId, p.PlaceId.Value, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		Guid id = Guid.CreateVersion7();
		DateTime now = DateTime.UtcNow;
		await db.Set<CheckEntity>().AddAsync(new CheckEntity {
			Id = id,
			CreatorId = p.OrganizationId,
			CreatedAt = now,
			Tags = [kind[0], TagCheck.Pending],
			Amount = p.Amount,
			DueDate = p.DueDate,
			Number = p.Number,
			Bank = p.Bank,
			PersonId = p.PersonId,
			ContractId = p.ContractId,
			PlaceId = p.PlaceId,
			OrganizationId = p.OrganizationId,
			JsonData = new CheckJson { Detail1 = p.Detail1, Detail2 = p.Detail2, SayadId = p.SayadId, Drawer = p.Drawer, AccountId = p.AccountId, RegisteredBy = userData!.Id }
		}, ct);

		if (kind[0] == TagCheck.Received)
			await Post(p.OrganizationId, TagVoucher.Check, id, p.PlaceId, p.PersonId, $"{ls.Get("receivedCheck", "fa")} {p.Number}", now, ct,
				new AccountingLeg(TagAccount.ChecksReceivable, p.Amount), new AccountingLeg(TagAccount.Receivable, -p.Amount, p.AccountId));
		if (kind[0] == TagCheck.Issued)
			await Post(p.OrganizationId, TagVoucher.Check, id, p.PlaceId, p.PersonId, $"{ls.Get("issuedCheck", "fa")} {p.Number}", now, ct,
				new AccountingLeg(TagAccount.Payables, p.Amount, p.AccountId), new AccountingLeg(TagAccount.ChecksPayable, -p.Amount));

		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<CheckResponse>?>> ReadChecks(CheckReadParams p, CancellationToken ct) {
		(_, UResponse? error) = await BooksUser(p.Token, p.OrganizationId, TagUser.PermissionViewAccounting, ct);
		if (error != null) return new UResponse<IEnumerable<CheckResponse>?>(null, error.Status, error.Message);

		IQueryable<CheckEntity> q = db.Set<CheckEntity>().Where(x => x.OrganizationId == p.OrganizationId).ApplyReadParams(p);
		if (p.PersonId != null) q = q.Where(x => x.PersonId == p.PersonId);
		if (p.ContractId != null) q = q.Where(x => x.ContractId == p.ContractId);
		if (p.FromDueDate != null) q = q.Where(x => x.DueDate >= p.FromDueDate);
		if (p.ToDueDate != null) q = q.Where(x => x.DueDate <= p.ToDueDate);

		return await q.OrderBy(x => x.DueDate).Select(x => new CheckResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Amount = x.Amount,
			DueDate = x.DueDate,
			Number = x.Number,
			Bank = x.Bank,
			PersonId = x.PersonId,
			PersonName = db.Set<UserEntity>().Where(u => u.Id == x.PersonId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault(),
			ContractId = x.ContractId,
			PlaceId = x.PlaceId,
			OrganizationId = x.OrganizationId
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> SetCheckStatus(CheckStatusParams p, CancellationToken ct) {
		CheckEntity? e = await db.Set<CheckEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("checkNotFound"));
		(_, UResponse? error) = await BooksUser(p.Token, e.OrganizationId, TagUser.PermissionManageAccounting, ct);
		if (error != null) return error;
		if (!e.Tags.Contains(TagCheck.Pending) || p.Status is not (TagCheck.Cleared or TagCheck.Bounced or TagCheck.Returned))
			return new UResponse(Usc.Conflict, ls.Get("checkIsNotPending"));

		bool cleared = p.Status == TagCheck.Cleared;
		AccountEntity? box = (await AccountsOf(e.OrganizationId, ct)).FirstOrDefault(x => x.Id == p.AccountId && IsMoneyBox(x));
		if (cleared && box == null) return new UResponse(Usc.BadRequest, ls.Get("selectACashOrBankAccount"));

		DateTime date = p.Date ?? DateTime.UtcNow;
		string description = $"{ls.Get(cleared ? "checkCleared" : p.Status == TagCheck.Bounced ? "checkBounced" : "checkReturned", "fa")} {e.Number}";
		Guid? counter = e.JsonData.AccountId;
		if (e.Tags.Contains(TagCheck.Received)) {
			if (cleared) await Post(e.OrganizationId, TagVoucher.Check, e.Id, e.PlaceId, e.PersonId, description, date, ct, new AccountingLeg(TagAccount.Cash, e.Amount, box!.Id), new AccountingLeg(TagAccount.ChecksReceivable, -e.Amount));
			else {
				await Post(e.OrganizationId, TagVoucher.Check, e.Id, e.PlaceId, e.PersonId, description, date, ct, new AccountingLeg(TagAccount.Receivable, e.Amount, counter), new AccountingLeg(TagAccount.ChecksReceivable, -e.Amount));
				if (e.JsonData.InvoiceId != null)
					foreach (IAccountingSource s in sp.GetServices<IAccountingSource>()) await s.ReopenPayment(e.JsonData.InvoiceId.Value, e.Amount, ct);
			}
		}
		else if (e.Tags.Contains(TagCheck.Issued)) {
			if (cleared) await Post(e.OrganizationId, TagVoucher.Check, e.Id, e.PlaceId, e.PersonId, description, date, ct, new AccountingLeg(TagAccount.ChecksPayable, e.Amount), new AccountingLeg(TagAccount.Cash, -e.Amount, box!.Id));
			else await Post(e.OrganizationId, TagVoucher.Check, e.Id, e.PlaceId, e.PersonId, description, date, ct, new AccountingLeg(TagAccount.ChecksPayable, e.Amount), new AccountingLeg(TagAccount.Payables, -e.Amount, counter));
		}
		else if (cleared) await Post(e.OrganizationId, TagVoucher.Check, e.Id, e.PlaceId, e.PersonId, description, date, ct, new AccountingLeg(TagAccount.Cash, e.Amount, box!.Id), new AccountingLeg(TagAccount.Receivable, -e.Amount));

		e.Tags = [..e.Tags.Where(x => x != TagCheck.Pending), p.Status];
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> RequestOrganizationSettlement(OrganizationSettlementRequestParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (p.Amount <= 0) return new UResponse(Usc.BadRequest, ls.Get("amountIsNotValid"));

		OrganizationEntity? e = await db.Set<OrganizationEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.OrganizationId, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("organizationNotFound"));
		if (!userData.IsSystemAdmin && e.OwnerId != userData.Id) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		Guid id = Guid.CreateVersion7();
		UResponse<WalletTxnResponse?> transfer = await ws.Transfer(new WalletTransferParams {
			SenderId = e.Id,
			ReceiverId = Core.App.Users.SystemAdmin.Id,
			Amount = p.Amount,
			Detail1 = ls.Get("organizationSettlement"),
			KeyValues = [new KeyValue { Key = "iban", Value = p.Iban }, new KeyValue { Key = "settlementId", Value = id.ToString() }],
			TagWalletTxn = [TagWalletTxn.OrganizationSettlement]
		}, ct);
		if (transfer.Result == null) return new UResponse(transfer.Status, transfer.Message);

		e.JsonData.Settlements = [..e.JsonData.Settlements, new OrganizationSettlement { Id = id, Amount = p.Amount, Iban = p.Iban, CreatedAt = DateTime.UtcNow }];
		await Post(e.Id, TagVoucher.Payout, id, null, null, ls.Get("organizationSettlement", "fa"), DateTime.UtcNow, ct, new AccountingLeg(TagAccount.InTransit, p.Amount), new AccountingLeg(TagAccount.Wallet, -p.Amount));
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> ProcessOrganizationSettlement(OrganizationSettlementProcessParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (!userData.IsSystemAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		OrganizationEntity? e = await db.Set<OrganizationEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.OrganizationId, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("organizationNotFound"));
		OrganizationSettlement? s = e.JsonData.Settlements.FirstOrDefault(x => x.Id == p.SettlementId && x.Approved == null);
		if (s == null) return new UResponse(Usc.NotFound, ls.Get("settlementNotFound"));

		if (!p.Approve) {
			UResponse<WalletTxnResponse?> transfer = await ws.Transfer(new WalletTransferParams {
				SenderId = Core.App.Users.SystemAdmin.Id,
				ReceiverId = e.Id,
				Amount = s.Amount,
				Detail1 = ls.Get("organizationSettlement"),
				KeyValues = [new KeyValue { Key = "settlementId", Value = s.Id.ToString() }],
				TagWalletTxn = [TagWalletTxn.OrganizationSettlementRefund],
				AllowOverdraft = true
			}, ct);
			if (transfer.Result == null) return new UResponse(transfer.Status, transfer.Message);
		}

		e.JsonData.Settlements = e.JsonData.Settlements.Select(x => x.Id != s.Id ? x : new OrganizationSettlement {
			Id = x.Id, Amount = x.Amount, Iban = x.Iban, CreatedAt = x.CreatedAt, ProcessedAt = DateTime.UtcNow, Approved = p.Approve, Note = p.Note
		}).ToList();
		await Post(e.Id, TagVoucher.Payout, s.Id, null, null, ls.Get("organizationSettlement", "fa"), DateTime.UtcNow, ct, new AccountingLeg(p.Approve ? TagAccount.Bank : TagAccount.Wallet, s.Amount), new AccountingLeg(TagAccount.InTransit, -s.Amount));
		await AddNotification(e.OwnerId, TagNotification.General, ls.Get("organizationSettlement"), p.Approve ? s.Amount.ToIntString() : p.Note ?? "", ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<bool> IsAccountOf(Guid organizationId, Guid accountId, CancellationToken ct) => (await AccountsOf(organizationId, ct)).Any(x => x.Id == accountId);

	public async Task RemindChecks(CancellationToken ct) {
		DateTime soon = DateTime.UtcNow.AddDays(3);
		List<CheckEntity> checks = await db.Set<CheckEntity>().AsTracking().Include(x => x.Organization)
			.Where(x => x.Tags.Contains(TagCheck.Pending) && !x.JsonData.DueReminded && x.DueDate <= soon)
			.ToListAsync(ct);
		foreach (CheckEntity c in checks) {
			await AddNotification(c.Organization.OwnerId, TagNotification.General, ls.Get("checkIsDueSoon", "fa"), $"{c.Number} - {c.Amount.ToIntString()}", ct);
			c.JsonData.DueReminded = true;
		}

		await db.SaveChangesAsync(ct);
	}
}

public sealed class AccountingReminderService(IServiceScopeFactory scopeFactory) : BackgroundService {
	private bool _failureLogged;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
		using PeriodicTimer timer = new(TimeSpan.FromHours(1));
		try {
			do {
				try {
					using IServiceScope scope = scopeFactory.CreateScope();
					await scope.ServiceProvider.GetRequiredService<IAccountingService>().RemindChecks(stoppingToken);
				}
				catch (OperationCanceledException) {
					throw;
				}
				catch (Exception e) {
					if (!_failureLogged) ULog.Error(e, "Check reminders are off (are the accounting tables migrated?)");
					_failureLogged = true;
				}
			} while (await timer.WaitForNextTickAsync(stoppingToken));
		}
		catch (OperationCanceledException) {
		}
	}
}
