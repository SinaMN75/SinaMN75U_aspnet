namespace SinaMN75U.Data.Responses;

public sealed class DormResponse : BaseResponse<TagDorm, DormJson> {
	public required string Title { get; set; }
	public Guid? OrganizationId { get; set; }
	public required string CityCode { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }

	public double AverageScore { get; set; }
	public int CommentCount { get; set; }
	public decimal? MinMonthlyRent { get; set; }
	public int BedCount { get; set; }
	public int AvailableBedCount { get; set; }

	public IEnumerable<DormRoomResponse>? Rooms { get; set; }
	public IEnumerable<DormBedResponse>? Beds { get; set; }
	public IEnumerable<CommentResponse>? Comments { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
}

public sealed class DormRoomResponse : BaseResponse<TagDormRoom, DormRoomJson> {
	public required string Title { get; set; }
	public int Capacity { get; set; }
	
	public Guid DormId { get; set; }
	public DormResponse? Dorm { get; set; }
	
	public IEnumerable<DormBedResponse>? Beds { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
}

public sealed class DormBedResponse : BaseResponse<TagDormBed, DormBedJson> {
	public required string Title { get; set; }
	public required decimal Deposit { get; set; }
	public required decimal MonthlyRent { get; set; }

	public required Guid RoomId { get; set; }
	public DormRoomResponse? Room { get; set; }

	public ICollection<MediaResponse>? Media { get; set; }
	public ICollection<DormBedContractResponse>? Contracts { get; set; }
}

public sealed class DormBedContractResponse : BaseResponse<TagDormBedContract, DormBedContractJson> {
	public required DateTime StartDate { get; set; }
	public required DateTime EndDate { get; set; }
	public required decimal Deposit { get; set; }
	public required decimal Rent { get; set; }

	public UserResponse? User { get; set; }
	public required Guid UserId { get; set; }

	public DormBedResponse? Bed { get; set; }
	public required Guid BedId { get; set; }

	public required bool IsActive { get; set; }

	public IEnumerable<DormBedInvoiceResponse>? Invoices { get; set; }
}

public sealed class DormBedInvoiceResponse : BaseResponse<TagDormBedInvoice, DormBedInvoiceJson> {
	public required decimal DebtAmount { get; set; }
	public required decimal CreditorAmount { get; set; }
	public required decimal PaidAmount { get; set; }
	public required decimal PenaltyAmount { get; set; }
	public required DateTime DueDate { get; set; }

	public DormBedContractResponse? Contract { get; set; }
}

public sealed class DormBedInvoiceChartResponse {
	public string Month { get; set; } = "";
	public decimal TotalDebt { get; set; }
	public decimal TotalPaid { get; set; }
	public decimal TotalPenalty { get; set; }
	public decimal TotalRemaining { get; set; }
	public int InvoiceCount { get; set; }
}

// ===================== Property (Hotels/Dorms) Dashboard =====================

public sealed class DormDashboardResponse {
	public DateTime GeneratedAt { get; set; }

	public int ResidentsCount { get; set; }
	public int NewResidentsCount { get; set; }

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
	public List<RecentUserItem> RecentResidents { get; set; } = [];
	public List<DormCityItem> DormsByCity { get; set; } = [];
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

public sealed class DormCityItem {
	public string Name { get; set; } = "";
	public int Count { get; set; }
}

public sealed class DormApplicationResponse : BaseResponse<TagDormApplication, DormApplicationJson> {
	public DateTime DesiredStartDate { get; set; }
	public DateTime? DesiredEndDate { get; set; }
	public Guid DormId { get; set; }
	public string? DormTitle { get; set; }
	public Guid UserId { get; set; }
	public string? UserName { get; set; }
	public string? PhoneNumber { get; set; }
	public int? WaitlistPosition { get; set; }
}

public sealed class DormRecordResponse : BaseResponse<TagDormRecord, DormRecordJson> {
	public required string Title { get; set; }
	public DateTime Date { get; set; }
	public DateTime? EndDate { get; set; }
	public Guid DormId { get; set; }
	public string? DormTitle { get; set; }
	public Guid? UserId { get; set; }
	public string? UserName { get; set; }
}

public sealed class DormMealResponse : BaseResponse<TagDormMeal, BaseJson> {
	public required string Title { get; set; }
	public DateTime Date { get; set; }
	public decimal Price { get; set; }
	public int? Capacity { get; set; }
	public int ReservedCount { get; set; }
	public bool ReservedByMe { get; set; }
	public Guid DormId { get; set; }
}

public sealed class DormBookingResponse : BaseResponse<TagDormBooking, DormBookingJson> {
	public DateTime StartAt { get; set; }
	public DateTime? EndAt { get; set; }
	public string? Resource { get; set; }
	public decimal Price { get; set; }
	public Guid DormId { get; set; }
	public Guid UserId { get; set; }
	public string? UserName { get; set; }
	public Guid? MealId { get; set; }
	public string? MealTitle { get; set; }
}

