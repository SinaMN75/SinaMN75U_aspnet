namespace SinaMN75U.Data.Responses;

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


public sealed class HotelDashboardResponse {
	public DateTime GeneratedAt { get; set; }

	public int GuestsCount { get; set; }
	public int NewGuestsCount { get; set; }

	public int HotelsCount { get; set; }
	public int HotelRoomsCount { get; set; }
	public int HotelRoomsAvailableCount { get; set; }
	public int HotelRoomsOccupiedCount { get; set; }
	public double HotelOccupancyRate { get; set; }
	public int ReservationsCount { get; set; }

	public List<RecentUserItem> RecentGuests { get; set; } = [];
	public List<HotelCityItem> HotelsByCity { get; set; } = [];
}

public sealed class HotelCityItem {
	public string Name { get; set; } = "";
	public int Count { get; set; }
}

public sealed class HotelRateResponse : BaseResponse<TagHotelRate, HotelRateJson> {
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public decimal? Price { get; set; }
	public decimal? Percent { get; set; }
	public Guid HotelId { get; set; }
	public Guid? RoomId { get; set; }
	public string? RoomTitle { get; set; }
}

public sealed class HotelCalendarDay {
	public DateTime Date { get; set; }
	public decimal Price { get; set; }
	public int Booked { get; set; }
	public int Available { get; set; }
	public bool Closed { get; set; }
}

public sealed class HotelNightAuditResponse {
	public DateTime Date { get; set; }
	public DateTime? LastAuditDate { get; set; }
	public int TotalRooms { get; set; }
	public int OccupiedRooms { get; set; }
	public double Occupancy { get; set; }
	public decimal RoomRevenue { get; set; }
	public decimal OpenBalance { get; set; }
	public List<HotelAuditItem> Arrivals { get; set; } = [];
	public List<HotelAuditItem> Departures { get; set; } = [];
	public List<HotelAuditItem> InHouse { get; set; } = [];
	public List<HotelAuditItem> NoShows { get; set; } = [];
	public List<HotelRoomUnitItem> DirtyUnits { get; set; } = [];
}

public sealed class HotelAuditItem {
	public Guid ReservationId { get; set; }
	public string? ReservationCode { get; set; }
	public string? GuestName { get; set; }
	public string? RoomTitle { get; set; }
	public string? RoomNumber { get; set; }
	public DateTime CheckInDate { get; set; }
	public DateTime CheckOutDate { get; set; }
	public decimal Balance { get; set; }
}

public sealed class HotelRoomUnitItem {
	public Guid RoomId { get; set; }
	public string? RoomTitle { get; set; }
	public required string Number { get; set; }
	public TagHousekeeping Status { get; set; }
}

public sealed class HotelGuestExportItem {
	public string? ReservationCode { get; set; }
	public DateTime CheckInDate { get; set; }
	public DateTime CheckOutDate { get; set; }
	public string? RoomTitle { get; set; }
	public string? RoomNumber { get; set; }
	public required string FullName { get; set; }
	public string? NationalCode { get; set; }
	public string? Nationality { get; set; }
	public string? PassportNumber { get; set; }
	public DateTime? BirthDate { get; set; }
	public string? FatherName { get; set; }
	public string? Gender { get; set; }
	public string? PhoneNumber { get; set; }
}

