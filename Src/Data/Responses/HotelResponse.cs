namespace SinaMN75U.Data.Responses;

public sealed class HotelResponse : BaseResponse<TagHotel, HotelJson> {
	public required string Title { get; set; }
	public required string CityCode { get; set; }
	public int Stars { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }

	public IEnumerable<HotelRoomResponse>? Rooms { get; set; }
	public IEnumerable<HotelReservationResponse>? Reservations { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
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

public sealed class DormResponse : BaseResponse<TagDorm, BaseJson> {
	public required string Title { get; set; }
	public required string CityCode { get; set; }

	public IEnumerable<DormRoomResponse>? Rooms { get; set; }
	public IEnumerable<DormBedResponse>? Beds { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
}

public sealed class DormRoomResponse : BaseResponse<TagDormRoom, BaseJson> {
	public required string Title { get; set; }
	
	public Guid DormId { get; set; }
	public DormResponse? Dorm { get; set; }
	
	public IEnumerable<DormBedResponse>? Beds { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
}

public sealed class DormBedResponse : BaseResponse<TagDormBed, BaseJson> {
	public required string Title { get; set; }
	public required decimal Deposit { get; set; }
	public required decimal MonthlyRent { get; set; }

	public required Guid RoomId { get; set; }
	public DormRoomResponse? Room { get; set; }

	public ICollection<MediaResponse>? Media { get; set; }
	public ICollection<DormBedContractResponse>? Contracts { get; set; }
}

public sealed class DormBedContractResponse : BaseResponse<TagDormBedContract, BaseJson> {
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