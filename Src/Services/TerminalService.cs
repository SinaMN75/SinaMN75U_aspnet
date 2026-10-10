namespace SinaMN75U.Services;

public interface ITerminalService {
	Task<UResponse<Guid?>> CreateMerchant(MerchantCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<MerchantResponse>?>> ReadMerchants(MerchantReadParams p, CancellationToken ct);
	Task<UResponse<MerchantResponse?>> ReadMerchantById(IdParams<MerchantSelectorArgs> p, CancellationToken ct);
	Task<UResponse> DeleteMerchant(IdParams p, CancellationToken ct);
	Task<UResponse<FinancialOpsDashboardResponse?>> ReadFinancialOpsDashboard(DashboardRangeParams p, CancellationToken ct);
	Task<UResponse<Guid?>> Create(TerminalCreateParams p, CancellationToken ct);
	Task<UResponse> BulkCreate(TerminalBulkCreateParams p, CancellationToken ct);
	Task<UResponse<TerminalImportResponse?>> Import(TerminalImportParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<TerminalResponse>?>> Read(TerminalReadParams p, CancellationToken ct);
	Task<UResponse> Update(TerminalUpdateParams p, CancellationToken ct);
	Task<UResponse<TerminalAvailabilityResponse?>> CheckAvailability(TerminalAssignParams p, CancellationToken ct);
	Task<UResponse<TerminalResponse?>> Assign(TerminalAssignParams p, CancellationToken ct);
	Task<UResponse<TerminalResponse?>> Approve(IdParams p, CancellationToken ct);
	Task<UResponse> Reject(TerminalRejectParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse<TerminalSupportPasswordResponse?>> ReadSupportPassword(IdParams p, CancellationToken ct);

	Task<UResponse<Guid?>> CreateBrand(TerminalBrandCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<TerminalBrandResponse>?>> ReadBrand(TerminalBrandReadParams p, CancellationToken ct);
	Task<UResponse> UpdateBrand(TerminalBrandUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteBrand(IdParams p, CancellationToken ct);

	Task<UResponse<Guid?>> CreateBroker(TerminalBrokerCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<TerminalBrokerResponse>?>> ReadBroker(TerminalBrokerReadParams p, CancellationToken ct);
	Task<UResponse> UpdateBroker(TerminalBrokerUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteBroker(IdParams p, CancellationToken ct);
}

public class TerminalService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IHttpClientService http,
	ISmsNotificationService sms,
	IWebHostEnvironment env
) : ITerminalService {
	private static readonly TagWalletTxn[] SpendingTags = [
		TagWalletTxn.MobileAndNationalCodeVerification, TagWalletTxn.ZipCodeToAddressDetail,
		TagWalletTxn.VehicleViolationsDetail, TagWalletTxn.DrivingLicenceStatus, TagWalletTxn.LicencePlateDetail,
		TagWalletTxn.DrivingLicenceNegativePoint, TagWalletTxn.IBanToBankAccountDetail, TagWalletTxn.FreewayTolls,
		TagWalletTxn.MerchantCreationFee, TagWalletTxn.ChargeSimPin, TagWalletTxn.ChargeSimTopup, TagWalletTxn.InternetSim
	];

	public async Task<UResponse<FinancialOpsDashboardResponse?>> ReadFinancialOpsDashboard(DashboardRangeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<FinancialOpsDashboardResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<FinancialOpsDashboardResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.HasPermission(TagUser.PermissionViewDashboard)) return new UResponse<FinancialOpsDashboardResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		DateTime to = p.ToDate ?? DateTime.UtcNow;
		DateTime from = p.FromDate ?? to.AddDays(-30);

		int usersCount = await db.Set<UserEntity>().CountAsync(ct);
		int newUsersCount = await db.Set<UserEntity>().CountAsync(x => x.CreatedAt >= from && x.CreatedAt <= to, ct);

		int merchantsCount = await db.Set<MerchantEntity>().CountAsync(ct);
		int newMerchantsCount = await db.Set<MerchantEntity>().CountAsync(x => x.CreatedAt >= from && x.CreatedAt <= to, ct);

		int terminalsCount = await db.Set<TerminalEntity>().CountAsync(ct);
		int terminalsAssignedCount = await db.Set<TerminalEntity>().CountAsync(x => x.MerchantId != null, ct);

		int txnCount = await db.Set<TxnEntity>().CountAsync(ct);
		int newTxnCount = await db.Set<TxnEntity>().CountAsync(x => x.CreatedAt >= from && x.CreatedAt <= to, ct);

		int walletsCount = await db.Set<WalletEntity>().CountAsync(ct);
		decimal totalWalletBalance = await db.Set<WalletEntity>().SumAsync(x => (decimal?)x.Balance, ct) ?? 0;

		List<WalletTxnEntity> walletRows = await db.Set<WalletTxnEntity>()
			.Where(x => x.CreatedAt >= from && x.CreatedAt <= to)
			.ToListAsync(ct);

		decimal totalIn = walletRows.Where(x => x.Tags.Contains(TagWalletTxn.Charge)).Sum(x => x.Amount);
		decimal totalOut = walletRows.Where(x => x.Tags.Any(t => SpendingTags.Contains(t))).Sum(x => x.Amount);

		List<TxnEntity> txnRows = await db.Set<TxnEntity>()
			.Where(x => x.CreatedAt >= from && x.CreatedAt <= to)
			.ToListAsync(ct);

		List<AccountingBreakdownItem> txnByStatus = txnRows
			.SelectMany(x => x.Tags.Where(t => t is TagTxn.Pending or TagTxn.Paid or TagTxn.Failed or TagTxn.Refunded).Select(t => (Tag: t, x.Amount)))
			.GroupBy(x => x.Tag)
			.Select(g => new AccountingBreakdownItem { Tag = (int)g.Key, TagName = g.Key.ToString(), Amount = g.Sum(i => i.Amount), Count = g.Count() })
			.OrderByDescending(x => x.Amount).ToList();

		List<AccountingBreakdownItem> txnByMethod = txnRows
			.SelectMany(x => x.Tags.Where(t => t is TagTxn.CreditCard or TagTxn.Cash).Select(t => (Tag: t, x.Amount)))
			.GroupBy(x => x.Tag)
			.Select(g => new AccountingBreakdownItem { Tag = (int)g.Key, TagName = g.Key.ToString(), Amount = g.Sum(i => i.Amount), Count = g.Count() })
			.OrderByDescending(x => x.Amount).ToList();

		List<TerminalEntity> terminalTagRows = await db.Set<TerminalEntity>().ToListAsync(ct);

		List<AccountingBreakdownItem> terminalsByType = terminalTagRows
			.SelectMany(x => x.Tags)
			.Where(t => t is not (TagTerminal.PendingApproval or TagTerminal.Approved or TagTerminal.Rejected))
			.GroupBy(t => t)
			.Select(g => new AccountingBreakdownItem { Tag = (int)g.Key, TagName = g.Key.ToString(), Amount = 0, Count = g.Count() })
			.OrderByDescending(x => x.Count).ToList();

		List<AccountingTimelineItem> dailyTimeline = walletRows
			.GroupBy(x => x.CreatedAt.Date)
			.Select(g => new AccountingTimelineItem {
				Date = g.Key,
				In = g.Where(x => x.Tags.Contains(TagWalletTxn.Charge)).Sum(x => x.Amount),
				Out = g.Where(x => x.Tags.Any(t => SpendingTags.Contains(t))).Sum(x => x.Amount)
			})
			.OrderBy(x => x.Date).ToList();

		List<TopMerchantItem> topMerchants = await db.Set<MerchantEntity>()
			.OrderByDescending(x => x.Terminals.Count)
			.Take(5)
			.Select(x => new TopMerchantItem { Id = x.Id, Title = x.Title, City = x.CityCode, TerminalCount = x.Terminals.Count, CreatedAt = x.CreatedAt })
			.ToListAsync(ct);

		List<TxnEntity> recentTxnEntities = await db.Set<TxnEntity>().Include(x => x.User)
			.OrderByDescending(x => x.CreatedAt).Take(10).ToListAsync(ct);
		List<RecentTxnItem> recentTransactions = recentTxnEntities.Select(x => new RecentTxnItem {
			Id = x.Id, Amount = x.Amount, TrackingNumber = x.TrackingNumber, UserName = x.User.UserName,
			Tags = x.Tags.Select(t => t.ToString()).ToList(), CreatedAt = x.CreatedAt
		}).ToList();

		List<MerchantEntity> recentMerchantEntities = await db.Set<MerchantEntity>()
			.OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync(ct);
		List<RecentMerchantItem> recentMerchants = recentMerchantEntities.Select(x => new RecentMerchantItem {
			Id = x.Id, Title = x.Title, CityCode = x.CityCode, TerminalCount = 0, CreatedAt = x.CreatedAt
		}).ToList();

		List<UserEntity> recentUserEntities = await db.Set<UserEntity>()
			.OrderByDescending(x => x.CreatedAt).Take(5).ToListAsync(ct);
		List<RecentUserItem> recentUsers = recentUserEntities.Select(x => new RecentUserItem {
			Id = x.Id, DisplayName = $"{x.FirstName} {x.LastName}".Trim() is { Length: > 0 } n ? n : x.UserName,
			UserName = x.UserName, PhoneNumber = x.PhoneNumber, CreatedAt = x.CreatedAt
		}).ToList();

		return new UResponse<FinancialOpsDashboardResponse?>(new FinancialOpsDashboardResponse {
			GeneratedAt = DateTime.UtcNow,
			FromDate = from,
			ToDate = to,
			UsersCount = usersCount,
			NewUsersCount = newUsersCount,
			MerchantsCount = merchantsCount,
			NewMerchantsCount = newMerchantsCount,
			TerminalsCount = terminalsCount,
			TerminalsAssignedCount = terminalsAssignedCount,
			TerminalsUnassignedCount = terminalsCount - terminalsAssignedCount,
			TxnCount = txnCount,
			NewTxnCount = newTxnCount,
			WalletsCount = walletsCount,
			TotalWalletBalance = totalWalletBalance,
			TotalIn = totalIn,
			TotalOut = totalOut,
			Net = totalIn - totalOut,
			TxnByStatus = txnByStatus,
			TxnByMethod = txnByMethod,
			TerminalsByType = terminalsByType,
			DailyTimeline = dailyTimeline,
			TopMerchants = topMerchants,
			RecentTransactions = recentTransactions,
			RecentMerchants = recentMerchants,
			RecentUsers = recentUsers
		});
	}

	public async Task<UResponse<Guid?>> Create(TerminalCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.HasPermission(TagUser.PermissionManageTerminals)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!await db.Set<TerminalBrandEntity>().AnyAsync(x => x.Id == p.TerminalBrandId, ct)) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("terminalBrandNotFound"));
		if (!await db.Set<TerminalBrokerEntity>().AnyAsync(x => x.Id == p.TerminalBrokerId, ct)) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("terminalBrokerNotFound"));

		TerminalEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			Serial = p.Serial,
			CreatedAt = DateTime.UtcNow,
			JsonData = new TerminalJson(),
			Tags = p.Tags,
			CreatorId = p.CreatorId ?? userData.Id,
			SimCardNumber = p.SimCardNumber,
			SimCardSerial = p.SimCardSerial,
			Imei = p.Imei,
			TerminalId = p.TerminalId,
			MerchantId = p.MerchantId,
			InsId = p.InsId,
			TerminalBrandId = p.TerminalBrandId,
			TerminalBrokerId = p.TerminalBrokerId
		};

		await db.Set<TerminalEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse> Update(TerminalUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.HasPermission(TagUser.PermissionManageTerminals)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		TerminalEntity? e = await db.Set<TerminalEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("terminalNotFound"));

		if (p.Serial.IsNotNullOrEmpty()) e.Serial = p.Serial;
		if (p.Imei.IsNotNullOrEmpty()) e.Imei = p.Imei;
		if (p.InsId.IsNotNullOrEmpty()) e.InsId = p.InsId;
		if (p.SimCardNumber.IsNotNullOrEmpty()) e.SimCardNumber = p.SimCardNumber;
		if (p.SimCardSerial.IsNotNullOrEmpty()) e.SimCardSerial = p.SimCardSerial;
		if (p.TerminalId.IsNotNullOrEmpty()) e.TerminalId = p.TerminalId;
		if (p.MerchantId.IsNotNullOrEmpty()) e.MerchantId = p.MerchantId;

		if (p.TerminalBrandId.IsNotNullOrEmpty()) {
			if (!await db.Set<TerminalBrandEntity>().AnyAsync(x => x.Id == p.TerminalBrandId, ct)) return new UResponse(Usc.NotFound, ls.Get("terminalBrandNotFound"));
			e.TerminalBrandId = p.TerminalBrandId!.Value;
		}

		if (p.TerminalBrokerId.IsNotNullOrEmpty()) {
			if (!await db.Set<TerminalBrokerEntity>().AnyAsync(x => x.Id == p.TerminalBrokerId, ct)) return new UResponse(Usc.NotFound, ls.Get("terminalBrokerNotFound"));
			e.TerminalBrokerId = p.TerminalBrokerId!.Value;
		}

		e.ApplyUpdateParam<TerminalEntity, TagTerminal, TerminalJson>(p);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<TerminalAvailabilityResponse?>> CheckAvailability(TerminalAssignParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalAvailabilityResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<TerminalAvailabilityResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		(TerminalEntity? terminal, MerchantEntity? merchant, TerminalBrandEntity? brand, TerminalBrokerEntity? broker, Usc status, string message) = await ResolveAssignable(userData, p, ct);
		if (terminal == null || merchant == null || brand == null || broker == null) return new UResponse<TerminalAvailabilityResponse?>(null, status, message);

		if (brand.JsonData.Agreement.IsNullOrEmpty()) return new UResponse<TerminalAvailabilityResponse?>(new TerminalAvailabilityResponse { Id = terminal.Id, Serial = terminal.Serial });

		byte[]? pdf = await GenerateAgreement(merchant.User, merchant, terminal, brand, broker);
		if (pdf == null) return new UResponse<TerminalAvailabilityResponse?>(null, Usc.InternalServerError, ls.Get("generatingTheAgreementFailed"));
		string agreement = UserFileStore.SaveBytes(env.WebRootPath, userData.Id, $"terminal-{terminal.Serial}.pdf", pdf);
		terminal.Agreement = agreement;
		await db.SaveChangesAsync(ct);

		return new UResponse<TerminalAvailabilityResponse?>(new TerminalAvailabilityResponse {
			Id = terminal.Id,
			Serial = terminal.Serial,
			Agreement = Core.App.BaseUrl + "/Media/" + agreement
		});
	}

	public async Task<UResponse<TerminalResponse?>> Assign(TerminalAssignParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<TerminalResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		(TerminalEntity? terminal, MerchantEntity? merchant, TerminalBrandEntity? brand, TerminalBrokerEntity? broker, Usc status, string message) = await ResolveAssignable(userData, p, ct);
		if (terminal == null || merchant == null || brand == null || broker == null) return new UResponse<TerminalResponse?>(null, status, message);

		string? agreement = null;
		if (brand.JsonData.Agreement.IsNotNullOrEmpty()) {
			if (!p.AcceptedAgreement) return new UResponse<TerminalResponse?>(null, Usc.BadRequest, ls.Get("youHaveToAcceptTheAgreementToContinue"));
			byte[]? pdf = await GenerateAgreement(merchant.User, merchant, terminal, brand, broker);
			if (pdf == null) return new UResponse<TerminalResponse?>(null, Usc.InternalServerError, ls.Get("generatingTheAgreementFailed"));
			agreement = UserFileStore.SaveBytes(env.WebRootPath, userData.Id, $"terminal-{terminal.Serial}.pdf", pdf);
		}

		terminal.JsonData.Detail1 = p.Title;
		terminal.JsonData.Detail2 = "";
		terminal.MerchantId = merchant.Id;
		terminal.Agreement = agreement;
		terminal.Tags.RemoveRangeIfExist([TagTerminal.PendingApproval, TagTerminal.Approved, TagTerminal.Rejected]);
		terminal.Tags.AddRangeIfNotExist([TagTerminal.PendingApproval]);

		await db.SaveChangesAsync(ct);

		return new UResponse<TerminalResponse?>(new TerminalResponse {
			Id = terminal.Id,
			CreatedAt = terminal.CreatedAt,
			JsonData = terminal.JsonData,
			Tags = terminal.Tags,
			CreatorId = terminal.CreatorId,
			Serial = terminal.Serial,
			SimCardNumber = terminal.SimCardNumber,
			SimCardSerial = terminal.SimCardSerial,
			Imei = terminal.Imei,
			TerminalId = terminal.TerminalId,
			Agreement = agreement == null ? null : Core.App.BaseUrl + "/Media/" + agreement,
			MerchantId = terminal.MerchantId,
			TerminalBrandId = terminal.TerminalBrandId,
			TerminalBrokerId = terminal.TerminalBrokerId,
		}, Usc.Success, ls.Get("yourRequestHasBeenSubmittedAndIsAwaitingApproval"));
	}

	public async Task<UResponse<TerminalResponse?>> Approve(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<TerminalResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.HasPermission(TagUser.PermissionManageTerminals)) return new UResponse<TerminalResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		TerminalEntity? terminal = await db.Set<TerminalEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (terminal == null) return new UResponse<TerminalResponse?>(null, Usc.NotFound, ls.Get("terminalNotFound"));
		if (terminal.Tags.Contains(TagTerminal.Approved)) return new UResponse<TerminalResponse?>(null, Usc.Conflict, ls.Get("thisTerminalRequestIsAlreadyApproved"));

		if (terminal.TerminalId.IsNullOrEmpty()) {
			MerchantEntity? merchant = await db.Set<MerchantEntity>().AsTracking().Include(x => x.User).FirstOrDefaultAsync(x => x.Id == terminal.MerchantId, ct);
			if (merchant == null) return new UResponse<TerminalResponse?>(null, Usc.NotFound, ls.Get("merchantNotFound"));

			if (merchant.MerchantId.IsNullOrEmpty()) {
				HttpResponseMessage? merchantResponse = await http.Post(
					$"{Core.App.Avreen.BaseUrl}api/mms/ing/v2/addMerchant",
					new {
						accountId = merchant.BankAccountId,
						businessTitle = merchant.JsonData.BusinessTitle,
						cityCode = merchant.CityCode,
						mcc = merchant.Mcc,
						merchantAddress = merchant.JsonData.Address,
						merchantMobileNo = merchant.PhoneNumber,
						merchantName = merchant.Title,
						merchantOwnerName = merchant.JsonData.OwnerName,
						merchantPhone = merchant.Landline,
						nationalId = merchant.NationalCode,
						ownerMobileNo = merchant.JsonData.OwnerPhoneNumber,
						postalCode = merchant.ZipCode,
						definitionTemplate = 1,
						settlementCurrency = 364
					},
					new Dictionary<string, string> { { "Authorization", $"{Core.App.Avreen.AuthHeader}" }, { "Accept", "application/json" } }
				);

				if (merchantResponse is null or { IsSuccessStatusCode: false }) return new UResponse<TerminalResponse?>(null, Usc.ThirdPartyError, ls.Get("failedToRegisterMerchantInAvreen"));
				JsonElement merchantData = JsonSerializer.Deserialize<JsonElement>(await merchantResponse.Content.ReadAsStringAsync(ct));
				string? merchantInsId = merchantData.GetStringOrNull("insId");
				string? merchantId = merchantData.GetStringOrNull("merchantId");
			
				if (merchantInsId == null || merchantId == null) return new UResponse<TerminalResponse?>(null, Usc.ThirdPartyError, ls.Get("merchantRegistrationSucceededButMerchantIdentifierWasNotReturnedByAvreen"));

				merchant.InsId = merchantInsId;
				merchant.MerchantId = merchantId;
			}

			HttpResponseMessage? terminalResponse = await http.Post(
				$"{Core.App.Avreen.BaseUrl}api/mms/ing/v2/defineAndBindTerminal",
				new {
					definitionTemplate = 1,
					merchantId = merchant.MerchantId,
					project = "AvaPlus",
					terminalSerial = terminal.Serial,
					terminalSerial2 = terminal.SimCardSerial
				},
				new Dictionary<string, string> { { "Authorization", $"{Core.App.Avreen.AuthHeader}" }, { "Accept", "application/json" } }
			);

			if (terminalResponse is null or { IsSuccessStatusCode: false }) return new UResponse<TerminalResponse?>(null, Usc.ThirdPartyError, ls.Get("failedToBindTerminalToMerchantInAvreen"));
			JsonElement terminalData = JsonSerializer.Deserialize<JsonElement>(await terminalResponse.Content.ReadAsStringAsync(ct));
			string? terminalInsId = terminalData.GetStringOrNull("insId");
			string? terminalId = terminalData.GetStringOrNull("merchantId");
		
			if (terminalId == null || terminalInsId == null) return new UResponse<TerminalResponse?>(null, Usc.ThirdPartyError, ls.Get("failedToBindTerminalToMerchantInAvreen"));

			terminal.TerminalId = terminalId;
			terminal.InsId = terminalInsId;
		}

		terminal.Tags.RemoveRangeIfExist([TagTerminal.PendingApproval, TagTerminal.Approved, TagTerminal.Rejected]);
		terminal.Tags.AddRangeIfNotExist([TagTerminal.Approved]);

		await db.SaveChangesAsync(ct);

		return new UResponse<TerminalResponse?>(new TerminalResponse {
			Id = terminal.Id,
			CreatedAt = terminal.CreatedAt,
			JsonData = terminal.JsonData,
			Tags = terminal.Tags,
			CreatorId = terminal.CreatorId,
			Serial = terminal.Serial,
			SimCardNumber = terminal.SimCardNumber,
			SimCardSerial = terminal.SimCardSerial,
			Imei = terminal.Imei,
			TerminalId = terminal.TerminalId,
			Agreement = terminal.Agreement,
			MerchantId = terminal.MerchantId,
			TerminalBrandId = terminal.TerminalBrandId,
			TerminalBrokerId = terminal.TerminalBrokerId
		});
	}

	public async Task<UResponse> Reject(TerminalRejectParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.HasPermission(TagUser.PermissionManageTerminals)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		TerminalEntity? terminal = await db.Set<TerminalEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (terminal == null) return new UResponse(Usc.NotFound, ls.Get("terminalNotFound"));
		if (terminal.Tags.Contains(TagTerminal.Approved)) return new UResponse(Usc.Conflict, ls.Get("thisTerminalRequestIsAlreadyApproved"));

		terminal.JsonData.Detail2 = p.Reason ?? "";
		terminal.Tags.RemoveRangeIfExist([TagTerminal.PendingApproval, TagTerminal.Approved, TagTerminal.Rejected]);
		terminal.Tags.AddRangeIfNotExist([TagTerminal.Rejected]);
		
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	private async Task<(TerminalEntity? Terminal, MerchantEntity? Merchant, TerminalBrandEntity? Brand, TerminalBrokerEntity? Broker, Usc Status, string Message)> ResolveAssignable(
		JwtClaimData userData,
		TerminalAssignParams p,
		CancellationToken ct
	) {
		TerminalEntity? terminal = await db.Set<TerminalEntity>().AsTracking()
			.Include(x => x.TerminalBrand)
			.Include(x => x.TerminalBroker)
			.FirstOrDefaultAsync(x => x.Serial == p.Serial && x.TerminalBrandId == p.TerminalBrandId && x.TerminalBrokerId == p.TerminalBrokerId, ct);

		if (terminal is not { MerchantId: null, TerminalId: null } || terminal.TerminalBrand.Tags.Contains(TagTerminalBrand.SimCard) && terminal.SimCardSerial != p.SimCardSerial) 
			return (null, null, null, null, Usc.NotFound, ls.Get("terminalNotFoundCheckYourDetails"));

		MerchantEntity? merchant = await db.Set<MerchantEntity>().AsTracking().Include(x => x.User).FirstOrDefaultAsync(x => x.Id == p.MerchantId, ct);
		if (merchant == null) return (null, null, null, null, Usc.NotFound, ls.Get("merchantNotFound"));
		if (!userData.HasPermission(TagUser.PermissionManageTerminals) && merchant.UserId != userData.Id) return (null, null, null, null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (terminal.Tags.Contains(TagTerminal.Approved)) return (null, null, null, null, Usc.Conflict, ls.Get("terminalIsAlreadyAssignedToAMerchant"));
		if (terminal.Tags.Contains(TagTerminal.PendingApproval)) return (null, null, null, null, Usc.Conflict, ls.Get("thisTerminalRequestIsWaitingForApproval"));
		if (terminal.MerchantId.IsNotNullOrEmpty() && terminal.MerchantId != merchant.Id) return (null, null, null, null, Usc.Conflict, ls.Get("terminalIsAlreadyAssignedToAMerchant"));

		return (terminal, merchant, terminal.TerminalBrand, terminal.TerminalBroker, Usc.Success, "");
	}
	
	public async Task<UResponse> BulkCreate(TerminalBulkCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.HasPermission(TagUser.PermissionManageTerminals)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		List<Guid> brandIds = p.List.Select(x => x.TerminalBrandId).Distinct().ToList();
		List<Guid> brokerIds = p.List.Select(x => x.TerminalBrokerId).Distinct().ToList();
		if (await db.Set<TerminalBrandEntity>().CountAsync(x => brandIds.Contains(x.Id), ct) != brandIds.Count) return new UResponse(Usc.NotFound, ls.Get("terminalBrandNotFound"));
		if (await db.Set<TerminalBrokerEntity>().CountAsync(x => brokerIds.Contains(x.Id), ct) != brokerIds.Count) return new UResponse(Usc.NotFound, ls.Get("terminalBrokerNotFound"));

		List<TerminalEntity> entities = [];

		entities.AddRange(p.List.Select(x => new TerminalEntity {
			Serial = x.Serial,
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new TerminalJson(),
			Tags = x.Tags,
			CreatorId = userData.Id,
			TerminalBrandId = x.TerminalBrandId,
			TerminalBrokerId = x.TerminalBrokerId
		}));

		await db.Set<TerminalEntity>().AddRangeAsync(entities, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse<IEnumerable<TerminalResponse>?>> Read(TerminalReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<TerminalResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<TerminalResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		IQueryable<TerminalEntity> q = db.Set<TerminalEntity>().ApplyReadParams(p);

		if (p.Serial.IsNotNullOrEmpty()) q = q.Where(x => x.Serial == p.Serial);
		if (p.TerminalId.IsNotNullOrEmpty()) q = q.Where(x => x.TerminalId == p.TerminalId);
		if (p.MerchantId.IsNotNullOrEmpty()) q = q.Where(x => x.MerchantId == p.MerchantId);
		if (p.Imei.IsNotNullOrEmpty()) q = q.Where(x => x.Imei == p.Imei);
		if (p.SimCardNumber.IsNotNullOrEmpty()) q = q.Where(x => x.SimCardNumber == p.SimCardNumber);
		if (p.SimCardSerial.IsNotNullOrEmpty()) q = q.Where(x => x.SimCardSerial == p.SimCardSerial);
		if (p.TerminalBrandId.IsNotNullOrEmpty()) q = q.Where(x => x.TerminalBrandId == p.TerminalBrandId);
		if (p.TerminalBrokerId.IsNotNullOrEmpty()) q = q.Where(x => x.TerminalBrokerId == p.TerminalBrokerId);

		IQueryable<TerminalResponse> projected = q.Select(Projections.TerminalSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.HasPermission(TagUser.PermissionDeleteTerminals)) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		await db.Set<TerminalEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse<TerminalSupportPasswordResponse?>> ReadSupportPassword(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		TerminalEntity? e = await db.Set<TerminalEntity>().Select(x => new TerminalEntity {
			Serial = x.Serial,
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			JsonData = x.JsonData,
			Tags = x.Tags,
			CreatorId = x.CreatorId,
			InsId = x.InsId,
			TerminalId = x.TerminalId,
			TerminalBrandId = x.TerminalBrandId,
			TerminalBrokerId = x.TerminalBrokerId,
			Merchant = new MerchantEntity {
				ZipCode = "",
				CityCode = "",
				PhoneNumber = "",
				Title = "",
				Landline = "",
				NationalCode = "",
				Mcc = "",
				UserId = x.Merchant!.UserId,
				Id = x.Merchant.Id,
				CreatedAt = x.Merchant.CreatedAt,
				JsonData = x.Merchant.JsonData,
				Tags = x.Merchant.Tags,
				CreatorId = x.Merchant.CreatorId,
				InsId = x.InsId,
				MerchantId = x.Merchant.MerchantId
			}
		}).FirstOrDefaultAsync(x => x.Id == p.Id, ct);

		if (e == null) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.NotFound, ls.Get("terminalNotFound"));
		if (e.Merchant == null) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.NotFound, ls.Get("merchantNotFound"));

		HttpResponseMessage? response = await http.Post(
			$"{Core.App.Avreen.BaseUrl}api/mms/ing/v2/generateSupportPassword",
			new {
				insId = e.InsId,
				merchantId = e.Merchant!.MerchantId,
				terminalId = e.TerminalId,
				terminalSerial = e.Serial,
				terminalSerial2 = e.SimCardSerial
			},
			new Dictionary<string, string> { { "Authorization", $"{Core.App.Avreen.AuthHeader}" }, { "Accept", "application/json" } }
		);

		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<TerminalSupportPasswordResponse?>(null);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));

		await sms.SendSms(new SmsNotificationParams {
			Mobile = userData.PhoneNumber!,
			Template = Core.App.SmsPanel.SupportPasswordOtp,
			Text = "12345"
		});

		return new UResponse<TerminalSupportPasswordResponse?>(new TerminalSupportPasswordResponse { Password = data.GetStringOrNull("supportPassword") });
	}

	public async Task<UResponse<TerminalImportResponse?>> Import(TerminalImportParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalImportResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<TerminalImportResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.HasPermission(TagUser.PermissionManageTerminals)) return new UResponse<TerminalImportResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.File.IsNullOrEmpty()) return new UResponse<TerminalImportResponse?>(null, Usc.BadRequest, ls.Get("FileRequired"));

		List<Dictionary<string, string>> rows;
		try {
			byte[] bytes = Convert.FromBase64String(StripDataUri(p.File));
			using MemoryStream ms = new(bytes);
			rows = ParseSheet(ms);
		}
		catch {
			return new UResponse<TerminalImportResponse?>(null, Usc.BadRequest, ls.Get("InvalidFileFormat"));
		}

		var existing = await db.Set<TerminalEntity>().Select(x => new { x.Serial, x.Imei, x.SimCardSerial, x.TerminalId }).ToListAsync(ct);
		HashSet<string> serials = existing.Select(x => x.Serial).ToHashSet(StringComparer.OrdinalIgnoreCase);
		HashSet<string> imeis = existing.Where(x => x.Imei != null).Select(x => x.Imei!).ToHashSet(StringComparer.OrdinalIgnoreCase);
		HashSet<string> simSerials = existing.Where(x => x.SimCardSerial != null).Select(x => x.SimCardSerial!).ToHashSet(StringComparer.OrdinalIgnoreCase);
		HashSet<string> terminalIds = existing.Where(x => x.TerminalId != null).Select(x => x.TerminalId!).ToHashSet(StringComparer.OrdinalIgnoreCase);

		Dictionary<string, Guid> brands = await db.Set<TerminalBrandEntity>().ToDictionaryAsync(x => x.Code, x => x.Id, StringComparer.OrdinalIgnoreCase, ct);
		Dictionary<string, Guid> brokers = await db.Set<TerminalBrokerEntity>().ToDictionaryAsync(x => x.Code, x => x.Id, StringComparer.OrdinalIgnoreCase, ct);

		TerminalImportResponse result = new() { TotalRows = rows.Count };
		List<TerminalEntity> toAdd = [];

		foreach (Dictionary<string, string> row in rows) {
			string serial = Val(row, "Serial");
			if (serial.IsNullOrEmpty()) {
				result.SkippedSerials.Add("(empty serial)");
				continue;
			}

			string? imei = Val(row, "Imei").NullIfEmpty();
			string? simSerial = Val(row, "SimCardSerial").NullIfEmpty();
			string? terminalId = Val(row, "TerminalId").NullIfEmpty();
			string brandCode = Val(row, "BrandCode");
			string brokerCode = Val(row, "BrokerCode");

			if (brandCode.IsNullOrEmpty() || !brands.TryGetValue(brandCode, out Guid brand)) {
				result.SkippedSerials.Add(brandCode.IsNullOrEmpty() ? $"{serial} (missing BrandCode)" : $"{serial} (unknown BrandCode '{brandCode}')");
				continue;
			}

			if (brokerCode.IsNullOrEmpty() || !brokers.TryGetValue(brokerCode, out Guid broker)) {
				result.SkippedSerials.Add(brokerCode.IsNullOrEmpty() ? $"{serial} (missing BrokerCode)" : $"{serial} (unknown BrokerCode '{brokerCode}')");
				continue;
			}

			if (serials.Contains(serial) ||
			    (imei != null && imeis.Contains(imei)) ||
			    (simSerial != null && simSerials.Contains(simSerial)) ||
			    (terminalId != null && terminalIds.Contains(terminalId))) {
				result.SkippedSerials.Add($"{serial} (duplicate)");
				continue;
			}

			List<TagTerminal> tags = TryParseTag(Val(row, "Tag1"), out TagTerminal tag1) ? [tag1] : [TagTerminal.NoAssigned];
			if (TryParseTag(Val(row, "Tag2"), out TagTerminal tag2) && !tags.Contains(tag2)) tags.Add(tag2);

			toAdd.Add(new TerminalEntity {
				Id = Guid.CreateVersion7(),
				Serial = serial,
				CreatedAt = DateTime.UtcNow,
				JsonData = new TerminalJson(),
				Tags = tags,
				CreatorId = userData.Id,
				SimCardNumber = Val(row, "SimCardNumber").NullIfEmpty(),
				SimCardSerial = simSerial,
				Imei = imei,
				TerminalId = terminalId,
				InsId = Val(row, "InsId").NullIfEmpty(),
				TerminalBrandId = brand,
				TerminalBrokerId = broker
			});

			serials.Add(serial);
			if (imei != null) imeis.Add(imei);
			if (simSerial != null) simSerials.Add(simSerial);
			if (terminalId != null) terminalIds.Add(terminalId);
		}

		if (toAdd.Count > 0) {
			await db.Set<TerminalEntity>().AddRangeAsync(toAdd, ct);
			await db.SaveChangesAsync(ct);
		}

		result.Imported = toAdd.Count;
		result.Skipped = result.TotalRows - result.Imported;
		return new UResponse<TerminalImportResponse?>(result, Usc.Success, ls.Get("ImportCompleted"));
	}

	public async Task<UResponse<Guid?>> CreateBrand(TerminalBrandCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.HasPermission(TagUser.PermissionManageTerminals)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		string code = p.Code.Trim();
		if (await db.Set<TerminalBrandEntity>().AnyAsync(x => x.Code == code, ct)) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("thisCodeAlreadyExists"));

		TerminalBrandEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new TerminalBrandJson { Agreement = p.Agreement.NullIfEmpty() },
			Tags = p.Tags,
			CreatorId = p.CreatorId ?? userData.Id,
			Code = code,
			Title = p.Title,
			Model = p.Model
		};

		await db.Set<TerminalBrandEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<TerminalBrandResponse>?>> ReadBrand(TerminalBrandReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<TerminalBrandResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<TerminalBrandResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		IQueryable<TerminalBrandEntity> q = db.Set<TerminalBrandEntity>().ApplyReadParams(p);

		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title == p.Title);
		if (p.Model.IsNotNullOrEmpty()) q = q.Where(x => x.Model == p.Model);
		if (p.Code.IsNotNullOrEmpty()) q = q.Where(x => x.Code == p.Code);

		IQueryable<TerminalBrandResponse> projected = q.Select(Projections.TerminalBrandSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateBrand(TerminalBrandUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.HasPermission(TagUser.PermissionManageTerminals)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		TerminalBrandEntity? e = await db.Set<TerminalBrandEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("terminalBrandNotFound"));

		if (p.Code.IsNotNullOrEmpty()) {
			string code = p.Code!.Trim();
			if (await db.Set<TerminalBrandEntity>().AnyAsync(x => x.Code == code && x.Id != e.Id, ct)) return new UResponse(Usc.Conflict, ls.Get("thisCodeAlreadyExists"));
			e.Code = code;
		}

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.Model.IsNotNullOrEmpty()) e.Model = p.Model;
		if (p.Agreement != null) e.JsonData.Agreement = p.Agreement.NullIfEmpty();

		e.ApplyUpdateParam<TerminalBrandEntity, TagTerminalBrand, TerminalBrandJson>(p);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteBrand(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.HasPermission(TagUser.PermissionDeleteTerminals)) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (await db.Set<TerminalEntity>().AnyAsync(x => x.TerminalBrandId == p.Id, ct)) return new UResponse(Usc.Conflict, ls.Get("thisBrandHasTerminalsAndCannotBeDeleted"));
		await db.Set<TerminalBrandEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateBroker(TerminalBrokerCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.HasPermission(TagUser.PermissionManageTerminals)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		string code = p.Code.Trim();
		if (await db.Set<TerminalBrokerEntity>().AnyAsync(x => x.Code == code, ct)) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("thisCodeAlreadyExists"));

		TerminalBrokerEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new TerminalBrokerJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				RegistrationNumber = p.RegistrationNumber,
				NationalCode = p.NationalCode,
				Representative = p.Representative,
				Address = p.Address,
				PostalCode = p.PostalCode,
				PhoneNumber = p.PhoneNumber,
				Sign1Base64 = p.Sign1Base64,
				Sign1Owner = p.Sign1Owner,
				Sign2Base64 = p.Sign2Base64,
				Sign2Owner = p.Sign2Owner,
				LogoBase64 = p.LogoBase64
			},
			Tags = p.Tags,
			CreatorId = p.CreatorId ?? userData.Id,
			Code = code,
			Title = p.Title,
		};

		await db.Set<TerminalBrokerEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<TerminalBrokerResponse>?>> ReadBroker(TerminalBrokerReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<TerminalBrokerResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<TerminalBrokerResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		IQueryable<TerminalBrokerEntity> q = db.Set<TerminalBrokerEntity>().ApplyReadParams(p);

		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title == p.Title);
		if (p.Code.IsNotNullOrEmpty()) q = q.Where(x => x.Code == p.Code);

		IQueryable<TerminalBrokerResponse> projected = q.Select(Projections.TerminalBrokerSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateBroker(TerminalBrokerUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.HasPermission(TagUser.PermissionManageTerminals)) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		TerminalBrokerEntity? e = await db.Set<TerminalBrokerEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("terminalBrokerNotFound"));

		if (p.Code.IsNotNullOrEmpty()) {
			string code = p.Code!.Trim();
			if (await db.Set<TerminalBrokerEntity>().AnyAsync(x => x.Code == code && x.Id != e.Id, ct)) return new UResponse(Usc.Conflict, ls.Get("thisCodeAlreadyExists"));
			e.Code = code;
		}

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;

		if (p.RegistrationNumber.IsNotNullOrEmpty()) e.JsonData.RegistrationNumber = p.RegistrationNumber;
		if (p.NationalCode.IsNotNullOrEmpty()) e.JsonData.NationalCode = p.NationalCode;
		if (p.Representative.IsNotNullOrEmpty()) e.JsonData.Representative = p.Representative;
		if (p.Address.IsNotNullOrEmpty()) e.JsonData.Address = p.Address;
		if (p.PostalCode.IsNotNullOrEmpty()) e.JsonData.PostalCode = p.PostalCode;
		if (p.PhoneNumber.IsNotNullOrEmpty()) e.JsonData.PhoneNumber = p.PhoneNumber;
		if (p.Sign1Base64.IsNotNullOrEmpty()) e.JsonData.Sign1Base64 = p.Sign1Base64;
		if (p.Sign1Owner.IsNotNullOrEmpty()) e.JsonData.Sign1Owner = p.Sign1Owner;
		if (p.Sign2Base64.IsNotNullOrEmpty()) e.JsonData.Sign2Base64 = p.Sign2Base64;
		if (p.Sign2Owner.IsNotNullOrEmpty()) e.JsonData.Sign2Owner = p.Sign2Owner;
		if (p.LogoBase64.IsNotNullOrEmpty()) e.JsonData.LogoBase64 = p.LogoBase64;

		e.ApplyUpdateParam<TerminalBrokerEntity, TagTerminalBroker, TerminalBrokerJson>(p);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteBroker(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.HasPermission(TagUser.PermissionDeleteTerminals)) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (await db.Set<TerminalEntity>().AnyAsync(x => x.TerminalBrokerId == p.Id, ct)) return new UResponse(Usc.Conflict, ls.Get("thisBrokerHasTerminalsAndCannotBeDeleted"));

		await db.Set<TerminalBrokerEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	private static List<Dictionary<string, string>> ParseSheet(Stream stream) {
		List<Dictionary<string, string>> rows = [];
		using SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, false);
		WorkbookPart wb = doc.WorkbookPart!;
		Sheet firstSheet = wb.Workbook!.Sheets!.Elements<Sheet>().First();
		WorksheetPart wsPart = (WorksheetPart)wb.GetPartById(firstSheet.Id!.Value!);
		SharedStringTablePart? sst = wb.SharedStringTablePart;

		Row[] sheetRows = wsPart.Worksheet!.GetFirstChild<SheetData>()!.Elements<Row>().ToArray();
		if (sheetRows.Length == 0) return rows;

		Dictionary<string, string> headers = new(StringComparer.OrdinalIgnoreCase);
		foreach (Cell c in sheetRows[0].Elements<Cell>()) {
			string text = CellText(c, sst).Trim();
			if (text.Length > 0) headers[ColumnLetter(c.CellReference!.Value!)] = text;
		}

		foreach (Row r in sheetRows.Skip(1)) {
			Dictionary<string, string> map = new(StringComparer.OrdinalIgnoreCase);
			foreach (Cell c in r.Elements<Cell>()) {
				string col = ColumnLetter(c.CellReference!.Value!);
				if (headers.TryGetValue(col, out string? header)) map[header] = CellText(c, sst).Trim();
			}

			if (map.Values.Any(v => v.Length > 0)) rows.Add(map);
		}

		return rows;
	}

	private static string CellText(Cell cell, SharedStringTablePart? sst) {
		string raw = cell.CellValue?.InnerText ?? "";
		if (cell.DataType?.Value == CellValues.SharedString && sst != null && int.TryParse(raw, out int idx))
			return sst.SharedStringTable!.Elements<SharedStringItem>().ElementAt(idx).InnerText;
		return cell.DataType?.Value == CellValues.InlineString ? cell.InnerText : raw;
	}

	private static string ColumnLetter(string cellRef) {
		int i = 0;
		while (i < cellRef.Length && char.IsLetter(cellRef[i])) i++;
		return cellRef[..i];
	}

	private static string StripDataUri(string s) {
		int i = s.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
		return i >= 0 ? s[(i + 7)..].Trim() : s.Trim();
	}

	private static string Val(Dictionary<string, string> row, string key) => row.GetValueOrDefault(key, "");

	private static bool TryParseTag(string raw, out TagTerminal tag) {
		tag = default;
		if (!int.TryParse(raw.Trim(), out int n) || !Enum.IsDefined(typeof(TagTerminal), n)) return false;
		tag = (TagTerminal)n;
		return true;
	}

	private async Task<byte[]?> GenerateAgreement(
		UserEntity user,
		MerchantEntity merchant,
		TerminalEntity terminal,
		TerminalBrandEntity brand,
		TerminalBrokerEntity broker
	) {
		try {
			string htmlPath = Path.Combine(AppContext.BaseDirectory, "Templates", brand.JsonData.Agreement!);
			if (!File.Exists(htmlPath)) {
				ULog.Error($"Agreement template not found at {htmlPath}");
				return null;
			}

			HtmlTemplate template = await HtmlTemplate.FromFile(htmlPath);
			template.RemoveUnmatchedTokens = true;

			template
				.Set("broker_day", PersianDateTime.Now.Day.ToString())
				.Set("broker_month", PersianDateTime.Now.Month.ToString())
				.SetLtr("broker_contract_number", terminal.Serial)
				.SetImageBase64("broker_logo", broker.JsonData.LogoBase64, attributes: "alt=\"\"")

				.Set("broker_company_name", broker.Title)
				.SetLtr("broker_registration_number", broker.JsonData.RegistrationNumber ?? "---")
				.SetLtr("broker_national_id", broker.JsonData.NationalCode ?? "---")
				.Set("broker_representative_name", broker.JsonData.Representative ?? "---")
				.Set("broker_address", broker.JsonData.Address ?? "---")
				.SetLtr("broker_postal_code", broker.JsonData.PostalCode ?? "---")
				.SetLtr("broker_phone", broker.JsonData.PhoneNumber ?? "---")
				.SetLtr("broker_support_phone", broker.JsonData.PhoneNumber ?? "---")

				.Set("user_full_name", $"{user.FirstName ?? "---"} {user.LastName ?? "---"}")
				.Set("user_father_name", user.JsonData.FatherName ?? "---")
				.Set("user_id_number", user.NationalCode ?? "---")
				.SetLtr("user_national_code", user.NationalCode ?? "---")
				.SetLtr("user_birthdate", PersianDateTime.FromDateTime(user.Birthdate ?? DateTime.Now).ToString("yyyy-MM-dd"))
				.Set("user_address", merchant.JsonData.Address ?? "---")
				.SetLtr("user_postal_code", merchant.ZipCode)
				.SetLtr("user_mobile", user.PhoneNumber ?? "---")
				.SetLtr("user_landline", user.LandLine ?? "---")

				.Set("user_plaque_number", "---")
				.Set("user_main_plaque", "---")

				.Set("broker_name1", broker.JsonData.Sign1Owner ?? "---")
				.Set("broker_name2", broker.JsonData.Sign2Owner ?? "---")
				.SetImageBase64("broker_signatures1", broker.JsonData.Sign1Base64)
				.SetImageBase64("broker_signatures2", broker.JsonData.Sign2Base64)
				.SetImageBytes("user_signature", ReadSignature(user));

			if (brand.Tags.Contains(TagTerminalBrand.Atm)) {
				template
					.Clear("broker_signatures1")
					.Clear("broker_signatures2")
					.Clear("user_signature")
					.Set("printInstruction", ls.Get("printTheAgreementSignItAndSendItByPost"));
			}
			else {
				template.SetImageBase64("broker_signatures", broker.JsonData.Sign1Base64);
				template.SetImageBytes("user_signature", ReadSignature(user));
			}

			return await HtmlToPdf.ConvertAsync(template.Render());
		}
		catch (Exception ex) {
			ULog.Error(ex, $"Generating the agreement failed for terminal {terminal.Id}");
			return null;
		}
	}

	private byte[]? ReadSignature(UserEntity user) {
		if (user.ESignature.IsNullOrEmpty()) return null;
		string path = Path.Combine(env.WebRootPath, "Media", user.ESignature.TrimStart('/', '\\'));
		return File.Exists(path) ? File.ReadAllBytes(path) : null;
	}

	public async Task<UResponse<Guid?>> CreateMerchant(MerchantCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (p.UserId != null && p.UserId != userData.Id && !userData.HasPermission(TagUser.PermissionManageMerchants)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		
		MerchantEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			UserId = p.UserId ?? userData.Id,
			ZipCode = p.ZipCode,
			Title = p.Title,
			CityCode = p.CityCode,
			PhoneNumber = p.PhoneNumber,
			Landline = p.Landline,
			NationalCode = p.NationalCode,
			BankAccountId = p.BankAccountId,
			Mcc = p.Mcc,
			JsonData = new MerchantJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				Address = p.Address,
				BusinessTitle = p.BusinessTitle,
				OwnerName = p.OwnerName,
				OwnerPhoneNumber = p.OwnerPhoneNumber
			}
		};
		await db.Set<MerchantEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse<Guid?>(e.Id);
	}
	
	public async Task<UResponse<IEnumerable<MerchantResponse>?>> ReadMerchants(MerchantReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<MerchantResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<MerchantResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		IQueryable<MerchantEntity> q = db.Set<MerchantEntity>().ApplyReadParams(p);
		if (!userData.HasPermission(TagUser.PermissionManageMerchants)) {
			Guid uid = userData.Id;
			q = q.Where(x => x.UserId == uid || x.CreatorId == uid);
		}

		if (p.UserId.IsNotNullOrEmpty()) q = q.Where(x => x.UserId == p.UserId);
		if (p.ZipCode.IsNotNullOrEmpty()) q = q.Where(x => x.ZipCode == p.ZipCode);
		if (p.BankAccountId.IsNotNullOrEmpty()) q = q.Where(x => x.BankAccountId == p.BankAccountId);
		if (p.NationalCode.IsNotNullOrEmpty()) q = q.Where(x => x.NationalCode == p.NationalCode);
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title == p.Title);
		if (p.CityCode.IsNotNullOrEmpty()) q = q.Where(x => x.CityCode == p.CityCode);
		if (p.InsId.IsNotNullOrEmpty()) q = q.Where(x => x.InsId == p.InsId);
		if (p.Landline.IsNotNullOrEmpty()) q = q.Where(x => x.Landline == p.Landline);
		if (p.PhoneNumber.IsNotNullOrEmpty()) q = q.Where(x => x.PhoneNumber == p.PhoneNumber);
		if (p.MerchantId.IsNotNullOrEmpty()) q = q.Where(x => x.MerchantId == p.MerchantId);
		if (p.Mcc.IsNotNullOrEmpty()) q = q.Where(x => x.Mcc == p.Mcc);

		IQueryable<MerchantResponse> projected = q.Select(Projections.MerchantSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<MerchantResponse?>> ReadMerchantById(IdParams<MerchantSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<MerchantResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<MerchantResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		Guid uid = userData.Id;
		bool all = userData.HasPermission(TagUser.PermissionManageMerchants);
		MerchantResponse? e = await db.Set<MerchantEntity>().Where(x => all || x.UserId == uid || x.CreatorId == uid).Select(Projections.MerchantSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<MerchantResponse?>(null, Usc.NotFound, ls.Get("merchantNotFound")) : new UResponse<MerchantResponse?>(e);
	}

	public async Task<UResponse> DeleteMerchant(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		bool canDeleteAll = userData.HasPermission(TagUser.PermissionDeleteMerchants);
		await db.Set<MerchantEntity>().Where(x => x.Id == p.Id && (canDeleteAll || x.CreatorId == userData.Id || x.UserId == userData.Id)).ExecuteDeleteAsync(ct);

		return new UResponse();
	}
}
