namespace SinaMN75U.Data.Responses;

public sealed class OrganizationResponse : BaseResponse<TagOrganization, OrganizationJson> {
	public required string Title { get; set; }
	public Guid OwnerId { get; set; }
	public decimal Balance { get; set; }
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

public sealed class HotelResponse : BaseResponse<TagHotel, HotelJson> {
	public required string Title { get; set; }
	public Guid? OrganizationId { get; set; }
	public required string CityCode { get; set; }
	public int Stars { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }

	public double AverageScore { get; set; }
	public int CommentCount { get; set; }
	public decimal? MinPricePerNight { get; set; }
	public int RoomCount { get; set; }

	public IEnumerable<HotelRoomResponse>? Rooms { get; set; }
	public IEnumerable<HotelReservationResponse>? Reservations { get; set; }
	public IEnumerable<CommentResponse>? Comments { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
}

public sealed class HotelRoomAvailabilityResponse {
	public required HotelRoomResponse Room { get; set; }
	public required int AvailableQuantity { get; set; }
	public required int NightCount { get; set; }
	public required decimal TotalPrice { get; set; }
	public required bool FitsGuestCount { get; set; }
}

public sealed class HotelRoomResponse : BaseResponse<TagRoom, HotelRoomJson> {
	public required string Title { get; set; }
	public int Capacity { get; set; }
	public decimal PricePerNight { get; set; }
	public string? RoomNumber { get; set; }
	public int Quantity { get; set; }
	public bool IsAvailable { get; set; }
	
	public Guid HotelId { get; set; }
	public HotelResponse? Hotel { get; set; }

	public IEnumerable<HotelReservationResponse>? Reservations { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
}

public sealed class HotelReservationResponse : BaseResponse<TagHotelReservation, HotelReservationJson> {
	public required DateTime CheckInDate { get; set; }
	public required DateTime CheckOutDate { get; set; }
	public int GuestCount { get; set; }
	public decimal TotalPrice { get; set; }

	public Guid UserId { get; set; }
	public UserResponse? User { get; set; }

	public Guid RoomId { get; set; }
	public HotelRoomResponse? Room { get; set; }

	public Guid HotelId { get; set; }
	public HotelResponse? Hotel { get; set; }

	public required bool IsActive { get; set; }

	public IEnumerable<HotelInvoiceResponse>? Invoices { get; set; }
}

public sealed class HotelInvoiceResponse : BaseResponse<TagHotelInvoice, HotelInvoiceJson> {
	public required decimal DebtAmount { get; set; }
	public required decimal CreditorAmount { get; set; }
	public required decimal PaidAmount { get; set; }
	public required decimal PenaltyAmount { get; set; }
	public required DateTime DueDate { get; set; }

	public Guid? ReservationId { get; set; }
	public HotelReservationResponse? Reservation { get; set; }
}

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
