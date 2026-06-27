namespace SinaMN75U.Data.Responses;

// Accounting report DTOs: a small books view of money-in vs money-out.
public sealed class AccountingReportResponse {
	// Total funds that came in within the range (credits).
	public decimal TotalIn { get; set; }

	// Total funds that went out within the range (debits/spending).
	public decimal TotalOut { get; set; }

	// Net = TotalIn - TotalOut.
	public decimal Net { get; set; }

	// Current liability the platform holds (sum of wallet balances). System-wide reports only.
	public decimal TotalWalletBalance { get; set; }

	// Number of wallet transactions counted in this report.
	public int WalletTxnCount { get; set; }

	// Number of gateway (Txn) records counted in this report.
	public int TxnCount { get; set; }

	// Breakdown of incoming money grouped by wallet-txn tag.
	public List<AccountingBreakdownItem> IncomeByType { get; set; } = [];

	// Breakdown of outgoing money grouped by wallet-txn tag.
	public List<AccountingBreakdownItem> SpendingByType { get; set; } = [];

	// Breakdown of gateway payments (TxnEntity) grouped by tag.
	public List<AccountingBreakdownItem> GatewayByType { get; set; } = [];

	// Daily in/out series for charting.
	public List<AccountingTimelineItem> Timeline { get; set; } = [];
}

// One row of a tag-grouped money breakdown.
public sealed class AccountingBreakdownItem {
	public int Tag { get; set; }
	public string TagName { get; set; } = "";
	public decimal Amount { get; set; }
	public int Count { get; set; }
}

// One day of aggregated in/out totals.
public sealed class AccountingTimelineItem {
	public DateTime Date { get; set; }
	public decimal In { get; set; }
	public decimal Out { get; set; }
}
