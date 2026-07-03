namespace SinaMN75U.Data.Responses;

public sealed record SystemMetricsResponse(
	double CpuUsage,
	double MemoryUsage,
	double DiskUsage,
	double TotalMemory,
	double FreeMemory,
	double TotalDisk,
	double FreeDisk,
	DateTime Date
);

public sealed class DashboardResponse {
	public required int Categories { get; set; }
	public required int Comments { get; set; }
	public required int Contents { get; set; }
	public required int Media { get; set; }
	public required int Products { get; set; }
	public required int Users { get; set; }
	public required IEnumerable<UserResponse> NewUsers { get; set; }
	public required IEnumerable<CategoryResponse> NewCategories { get; set; }
	public required IEnumerable<CommentResponse> NewComments { get; set; }
	public required IEnumerable<ContentResponse> NewContents { get; set; }
	public required IEnumerable<MediaResponse> NewMedia { get; set; }
	public required IEnumerable<ProductResponse> NewProducts { get; set; }
}

// ===================== Shared small DTOs =====================

public sealed class RecentUserItem {
	public Guid Id { get; set; }
	public string DisplayName { get; set; } = "";
	public string? UserName { get; set; }
	public string? PhoneNumber { get; set; }
	public DateTime CreatedAt { get; set; }
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

// ===================== Property (Hotels/Dorms) Dashboard =====================

public sealed class PropertyDashboardResponse {
	public DateTime GeneratedAt { get; set; }

	public int UsersCount { get; set; }
	public int NewUsersCount { get; set; }

	public int HotelsCount { get; set; }
	public int HotelRoomsCount { get; set; }
	public int HotelRoomsAvailableCount { get; set; }
	public int HotelRoomsOccupiedCount { get; set; }
	public double HotelOccupancyRate { get; set; }

	public int DormsCount { get; set; }
	public int DormRoomsCount { get; set; }
	public int DormBedsCount { get; set; }
	public int DormBedsAvailableCount { get; set; }
	public int DormBedsOccupiedCount { get; set; }
	public double DormOccupancyRate { get; set; }

	public int ContractsCount { get; set; }
	public int ActiveContractsCount { get; set; }
	public int UpcomingContractsCount { get; set; }
	public int ExpiredContractsCount { get; set; }
	public int ExpiringSoonContractsCount { get; set; }

	public int InvoicesCount { get; set; }
	public int PaidInvoicesCount { get; set; }
	public int UnpaidInvoicesCount { get; set; }
	public int OverdueInvoicesCount { get; set; }

	public decimal TotalDebt { get; set; }
	public decimal TotalPaid { get; set; }
	public decimal TotalPenalty { get; set; }
	public decimal TotalOutstanding { get; set; }

	public List<DormBedInvoiceChartResponse> MonthlyRevenue { get; set; } = [];
	public List<ExpiringContractItem> ExpiringContracts { get; set; } = [];
	public List<OverdueInvoiceItem> OverdueInvoices { get; set; } = [];
	public List<RecentContractItem> RecentContracts { get; set; } = [];
	public List<RecentUserItem> RecentUsers { get; set; } = [];
	public List<PropertyBreakdownItem> HotelsByCity { get; set; } = [];
	public List<PropertyBreakdownItem> DormsByCity { get; set; } = [];
}

public sealed class ExpiringContractItem {
	public Guid Id { get; set; }
	public string? UserName { get; set; }
	public string BedTitle { get; set; } = "";
	public string DormTitle { get; set; } = "";
	public DateTime EndDate { get; set; }
	public decimal Rent { get; set; }
}

public sealed class OverdueInvoiceItem {
	public Guid Id { get; set; }
	public string? UserName { get; set; }
	public decimal DebtAmount { get; set; }
	public decimal PaidAmount { get; set; }
	public decimal PenaltyAmount { get; set; }
	public DateTime DueDate { get; set; }
	public int DaysOverdue { get; set; }
}

public sealed class RecentContractItem {
	public Guid Id { get; set; }
	public string? UserName { get; set; }
	public string BedTitle { get; set; } = "";
	public string DormTitle { get; set; } = "";
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public decimal Rent { get; set; }
	public DateTime CreatedAt { get; set; }
}

public sealed class PropertyBreakdownItem {
	public string Name { get; set; } = "";
	public int Count { get; set; }
}