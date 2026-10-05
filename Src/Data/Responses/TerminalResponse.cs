namespace SinaMN75U.Data.Responses;

public class MerchantResponse : BaseResponse<TagMerchant, MerchantJson> {
	public required string ZipCode { get; set; }
	public required string CityCode { get; set; }
	public required string PhoneNumber { get; set; }
	public required string Title { get; set; }
	public required string Landline { get; set; }
	public required string NationalCode { get; set; }
	public required string Mcc { get; set; }
	public string? BankAccountId { get; set; }
	public string? MerchantId { get; set; }
	public string? InsId { get; set; }
	
	public required Guid UserId { get; set; }
	public UserResponse? User { get; set; }
	
	public ICollection<TerminalResponse>? Terminals { get; set; }
}

public class TerminalResponse : BaseResponse<TagTerminal, TerminalJson> {
	public required string Serial { get; set; }
	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }
	public string? TerminalId { get; set; }
	public string? Agreement { get; set; }

	public Guid? MerchantId { get; set; }
	public MerchantResponse? Merchant { get; set; }
	
	public required Guid TerminalBrandId { get; set; }
	public TerminalBrandResponse? TerminalBrand { get; set; }

	public required Guid TerminalBrokerId { get; set; }
	public TerminalBrokerResponse? TerminalBroker { get; set; }
}

public class TerminalAvailabilityResponse {
	public required Guid Id { get; set; }
	public required string Serial { get; set; }
	public string? Agreement { get; set; }
}

public class TerminalSupportPasswordResponse {
	public string? Password { get; set; }
}

public class TerminalImportResponse {
	public int TotalRows { get; set; }
	public int Imported { get; set; }
	public int Skipped { get; set; }
	public List<string> SkippedSerials { get; set; } = [];
}

public sealed class TerminalBrandResponse : BaseResponse<TagTerminalBrand, TerminalBrandJson> {
	public required string Code { get; set; }
	public required string Title { get; set; }
	public required string Model { get; set; }
}

public sealed class TerminalBrokerResponse : BaseResponse<TagTerminalBroker, TerminalBrokerJson> {
	public required string Code { get; set; }
	public required string Title { get; set; }
}

// ===================== Financial / Operations Dashboard =====================

public sealed class FinancialOpsDashboardResponse {
	public DateTime GeneratedAt { get; set; }
	public DateTime FromDate { get; set; }
	public DateTime ToDate { get; set; }

	public int UsersCount { get; set; }
	public int NewUsersCount { get; set; }

	public int MerchantsCount { get; set; }
	public int NewMerchantsCount { get; set; }

	public int TerminalsCount { get; set; }
	public int TerminalsAssignedCount { get; set; }
	public int TerminalsUnassignedCount { get; set; }

	public int TxnCount { get; set; }
	public int NewTxnCount { get; set; }

	public int WalletsCount { get; set; }
	public decimal TotalWalletBalance { get; set; }

	// Wallet money flow within [FromDate, ToDate].
	public decimal TotalIn { get; set; }
	public decimal TotalOut { get; set; }
	public decimal Net { get; set; }

	public List<AccountingBreakdownItem> TxnByStatus { get; set; } = [];
	public List<AccountingBreakdownItem> TxnByMethod { get; set; } = [];
	public List<AccountingBreakdownItem> TerminalsByType { get; set; } = [];
	public List<AccountingTimelineItem> DailyTimeline { get; set; } = [];

	public List<TopMerchantItem> TopMerchants { get; set; } = [];
	public List<RecentTxnItem> RecentTransactions { get; set; } = [];
	public List<RecentMerchantItem> RecentMerchants { get; set; } = [];
	public List<RecentUserItem> RecentUsers { get; set; } = [];
}

public sealed class TopMerchantItem {
	public Guid Id { get; set; }
	public string Title { get; set; } = "";
	public string City { get; set; } = "";
	public int TerminalCount { get; set; }
	public DateTime CreatedAt { get; set; }
}

public sealed class RecentTxnItem {
	public Guid Id { get; set; }
	public decimal Amount { get; set; }
	public string TrackingNumber { get; set; } = "";
	public string? UserName { get; set; }
	public List<string> Tags { get; set; } = [];
	public DateTime CreatedAt { get; set; }
}

public sealed class RecentMerchantItem {
	public Guid Id { get; set; }
	public string Title { get; set; } = "";
	public string CityCode { get; set; } = "";
	public int TerminalCount { get; set; }
	public DateTime CreatedAt { get; set; }
}
