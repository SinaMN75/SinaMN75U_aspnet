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

public sealed class AccountResponse : BaseResponse<TagAccount, AccountJson> {
	public required string Code { get; set; }
	public required string Title { get; set; }
	public Guid OrganizationId { get; set; }
	public decimal Debit { get; set; }
	public decimal Credit { get; set; }
	public decimal Balance { get; set; }
}

public sealed class VoucherResponse : BaseResponse<TagVoucher, VoucherJson> {
	public int Number { get; set; }
	public DateTime Date { get; set; }
	public Guid? PlaceId { get; set; }
	public Guid? SourceId { get; set; }
	public Guid OrganizationId { get; set; }
	public decimal Total { get; set; }
	public List<VoucherLineResponse> Lines { get; set; } = [];
}

public sealed class VoucherLineResponse {
	public Guid Id { get; set; }
	public Guid AccountId { get; set; }
	public string AccountCode { get; set; } = "";
	public string AccountTitle { get; set; } = "";
	public Guid? PersonId { get; set; }
	public string? PersonName { get; set; }
	public decimal Debit { get; set; }
	public decimal Credit { get; set; }
	public string? Description { get; set; }
}

public sealed class LedgerResponse {
	public decimal Opening { get; set; }
	public decimal TotalDebit { get; set; }
	public decimal TotalCredit { get; set; }
	public decimal Closing { get; set; }
	public List<LedgerLineResponse> Lines { get; set; } = [];
}

public sealed class LedgerLineResponse {
	public Guid VoucherId { get; set; }
	public int Number { get; set; }
	public DateTime Date { get; set; }
	public ICollection<TagVoucher> Tags { get; set; } = [];
	public string? Description { get; set; }
	public Guid AccountId { get; set; }
	public string AccountTitle { get; set; } = "";
	public Guid? PersonId { get; set; }
	public string? PersonName { get; set; }
	public decimal Debit { get; set; }
	public decimal Credit { get; set; }
	public decimal Balance { get; set; }
}

public sealed class LedgerReportResponse {
	public List<LedgerReportItem> Income { get; set; } = [];
	public List<LedgerReportItem> Expense { get; set; } = [];
	public decimal NetProfit { get; set; }
	public List<LedgerMoneyBoxItem> MoneyBoxes { get; set; } = [];
	public List<LedgerPlaceItem> Places { get; set; } = [];
	public List<LedgerAgingItem> Aging { get; set; } = [];
}

public sealed class LedgerReportItem {
	public Guid AccountId { get; set; }
	public string Code { get; set; } = "";
	public string Title { get; set; } = "";
	public decimal Amount { get; set; }
}

public sealed class LedgerMoneyBoxItem {
	public Guid AccountId { get; set; }
	public string Title { get; set; } = "";
	public ICollection<TagAccount> Tags { get; set; } = [];
	public decimal Opening { get; set; }
	public decimal In { get; set; }
	public decimal Out { get; set; }
	public decimal Closing { get; set; }
}

public sealed class LedgerPlaceItem {
	public Guid? PlaceId { get; set; }
	public string Title { get; set; } = "";
	public decimal Income { get; set; }
	public decimal Expense { get; set; }
}

public sealed class LedgerAgingItem {
	public Guid PersonId { get; set; }
	public string? PersonName { get; set; }
	public string? PhoneNumber { get; set; }
	public decimal Days0 { get; set; }
	public decimal Days30 { get; set; }
	public decimal Days60 { get; set; }
	public decimal Days90 { get; set; }
	public decimal Total { get; set; }
}

public sealed class CheckResponse : BaseResponse<TagCheck, CheckJson> {
	public decimal Amount { get; set; }
	public DateTime DueDate { get; set; }
	public required string Number { get; set; }
	public string? Bank { get; set; }
	public Guid? PersonId { get; set; }
	public string? PersonName { get; set; }
	public Guid? ContractId { get; set; }
	public Guid? PlaceId { get; set; }
	public Guid OrganizationId { get; set; }
}
