namespace SinaMN75U.Services;

public interface IAccountingService {
	Task<UResponse<AccountingReportResponse?>> Report(AccountingReportParams p, CancellationToken ct);
}

public class AccountingService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IAccountingService {
	private static readonly TagWalletTxn[] SpendingTags = [
		TagWalletTxn.MobileAndNationalCodeVerification, TagWalletTxn.ZipCodeToAddressDetail,
		TagWalletTxn.VehicleViolationsDetail, TagWalletTxn.DrivingLicenceStatus, TagWalletTxn.LicencePlateDetail,
		TagWalletTxn.DrivingLicenceNegativePoint, TagWalletTxn.IBanToBankAccountDetail, TagWalletTxn.FreewayTolls,
		TagWalletTxn.MerchantCreationFee, TagWalletTxn.ChargeSimPin, TagWalletTxn.ChargeSimTopup, TagWalletTxn.InternetSim
	];

	public async Task<UResponse<AccountingReportResponse?>> Report(AccountingReportParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<AccountingReportResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (userData.IsExpired) return new UResponse<AccountingReportResponse?>(null, Usc.ExpiredToken, ls.Get("TokenExpired"));
		if (p.UserId == null && !userData.IsAdmin) return new UResponse<AccountingReportResponse?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		DateTime from = p.FromDate ?? DateTime.UtcNow.AddMonths(-1);
		DateTime to = p.ToDate ?? DateTime.UtcNow;

		IQueryable<WalletTxnEntity> wq = db.Set<WalletTxnEntity>().AsNoTracking().Where(x => x.CreatedAt >= from && x.CreatedAt <= to);
		if (p.UserId != null) wq = wq.Where(x => x.SenderId == p.UserId || x.ReceiverId == p.UserId);
		List<WalletTxnEntity> walletRows = await wq.ToListAsync(ct);

		List<TxnEntity> txnRows = await db.Set<TxnEntity>().AsNoTracking()
			.Where(x => x.CreatedAt >= from && x.CreatedAt <= to)
			.Where(x => p.UserId == null || x.UserId == p.UserId)
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

	private static void BuildPerUser(AccountingReportResponse r, List<WalletTxnEntity> rows, Guid userId) {
		List<WalletTxnEntity> incoming = rows.Where(x => x.ReceiverId == userId).ToList();
		List<WalletTxnEntity> outgoing = rows.Where(x => x.SenderId == userId).ToList();

		r.TotalIn = incoming.Sum(x => x.Amount);
		r.TotalOut = outgoing.Sum(x => x.Amount);
		r.Net = r.TotalIn - r.TotalOut;
		r.IncomeByType = GroupByTag(incoming);
		r.SpendingByType = GroupByTag(outgoing);
	}

	private static void BuildSystemWide(AccountingReportResponse r, List<WalletTxnEntity> rows) {
		List<WalletTxnEntity> incoming = rows.Where(x => x.Tags.Contains(TagWalletTxn.Charge)).ToList();
		List<WalletTxnEntity> outgoing = rows.Where(x => x.Tags.Any(t => SpendingTags.Contains(t))).ToList();

		r.TotalIn = incoming.Sum(x => x.Amount);
		r.TotalOut = outgoing.Sum(x => x.Amount);
		r.Net = r.TotalIn - r.TotalOut;
		r.IncomeByType = GroupByTag(incoming);
		r.SpendingByType = GroupByTag(outgoing);
	}

	private static List<AccountingBreakdownItem> GroupByTag(List<WalletTxnEntity> rows) => rows
		.SelectMany(x => x.Tags.Select(t => (Tag: t, x.Amount)))
		.GroupBy(x => x.Tag)
		.Select(g => new AccountingBreakdownItem { Tag = (int)g.Key, TagName = g.Key.ToString(), Amount = g.Sum(i => i.Amount), Count = g.Count() })
		.OrderByDescending(i => i.Amount).ToList();

	private static void BuildTimeline(AccountingReportResponse r, List<WalletTxnEntity> rows, Guid? userId) {
		r.Timeline = rows
			.GroupBy(x => x.CreatedAt.Date)
			.Select(g => new AccountingTimelineItem {
				Date = g.Key,
				In = g.Where(x => userId != null ? x.ReceiverId == userId : x.Tags.Contains(TagWalletTxn.Charge)).Sum(x => x.Amount),
				Out = g.Where(x => userId != null ? x.SenderId == userId : x.Tags.Any(t => SpendingTags.Contains(t))).Sum(x => x.Amount)
			})
			.OrderBy(i => i.Date).ToList();
	}
}
