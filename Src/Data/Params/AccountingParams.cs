namespace SinaMN75U.Data.Params;

// Params for the accounting report. When UserId is set the report is per-user,
// otherwise it is computed system-wide across all wallets/transactions.
public sealed class AccountingReportParams : BaseParams {
	public Guid? UserId { get; set; }
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
}
