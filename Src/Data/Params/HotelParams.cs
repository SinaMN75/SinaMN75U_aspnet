namespace SinaMN75U.Data.Params;

public sealed class HotelCreateParams : BaseCreateParams<TagHotel> {
	[UValidationRequired("titleIsRequired"), UValidationStringLength(2, 100, "TitleMinLength")]
	public string Title { get; set; } = null!;

	[UValidationRequired("CityRequired"), UValidationStringLength(2, 100, "CityMinLength")]
	public required string CityCode { get; set; }

	public int Stars { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }

	public string? Description { get; set; }
	public string? Policies { get; set; }
	public string? CheckInTime { get; set; }
	public string? CheckOutTime { get; set; }
	public List<string>? Highlights { get; set; }
	public List<string>? Rules { get; set; }
	public string? HowToGetThere { get; set; }
	public List<PlaceNearby>? Nearby { get; set; }
	public List<PlaceFaq>? Faqs { get; set; }
	public string? Website { get; set; }
	public string? Whatsapp { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int? CancellationFreeHours { get; set; }
	public int? CancellationPenaltyNights { get; set; }
	public Guid? OrganizationId { get; set; }
}

public sealed class HotelUpdateParams : BaseUpdateParams<TagHotel> {
	public string? Title { get; set; }
	public string? CityCode { get; set; }
	public int? Stars { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }

	public string? Description { get; set; }
	public string? Policies { get; set; }
	public string? CheckInTime { get; set; }
	public string? CheckOutTime { get; set; }
	public List<string>? Highlights { get; set; }
	public List<string>? Rules { get; set; }
	public string? HowToGetThere { get; set; }
	public List<PlaceNearby>? Nearby { get; set; }
	public List<PlaceFaq>? Faqs { get; set; }
	public string? Website { get; set; }
	public string? Whatsapp { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int? CancellationFreeHours { get; set; }
	public int? CancellationPenaltyNights { get; set; }
	public Guid? OrganizationId { get; set; }
}

public sealed class HotelReadParams : BaseReadParams<TagHotel> {
	public string? Title { get; set; }
	public string? CityCode { get; set; }
	public int? MinStars { get; set; }
	public decimal? MinPrice { get; set; }
	public decimal? MaxPrice { get; set; }
	public decimal? MinScore { get; set; }
	public Guid? OrganizationId { get; set; }

	public HotelSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class HotelRoomCreateParams : BaseCreateParams<TagRoom> {
	[UValidationRequired("titleIsRequired"), UValidationStringLength(2, 100, "TitleMinLength")]
	public string Title { get; set; } = null!;

	[UValidationRequired("CapacityRequired")]
	public int Capacity { get; set; }

	[UValidationRequired("PricePerNightRequired")]
	public decimal PricePerNight { get; set; }

	[UValidationRequired("HotelIdRequired")]
	public Guid HotelId { get; set; }

	public string? RoomNumber { get; set; }
	public int Quantity { get; set; } = 1;
	public bool IsAvailable { get; set; } = true;

	public string? Description { get; set; }
	public string? BedType { get; set; }
	public double? SizeSquareMeters { get; set; }
	public int? Floor { get; set; }
	public int? ExtraGuestCapacity { get; set; }
	public decimal? ExtraGuestPrice { get; set; }
}

public sealed class HotelRoomUpdateParams : BaseUpdateParams<TagRoom> {
	public string? Title { get; set; }
	public int? Capacity { get; set; }
	public decimal? PricePerNight { get; set; }
	public Guid? HotelId { get; set; }
	public string? RoomNumber { get; set; }
	public int? Quantity { get; set; }
	public bool? IsAvailable { get; set; }

	public string? Description { get; set; }
	public string? BedType { get; set; }
	public double? SizeSquareMeters { get; set; }
	public int? Floor { get; set; }
	public int? ExtraGuestCapacity { get; set; }
	public decimal? ExtraGuestPrice { get; set; }
}

public sealed class HotelRoomAvailabilityParams : BaseParams {
	public Guid? HotelId { get; set; }
	public Guid? RoomId { get; set; }

	[UValidationRequired("checkInDateIsRequired")]
	public DateTime CheckInDate { get; set; }

	[UValidationRequired("checkOutDateIsRequired")]
	public DateTime CheckOutDate { get; set; }

	public int GuestCount { get; set; } = 1;

	public HotelRoomSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class HotelRoomReadParams : BaseReadParams<TagRoom> {
	public string? Title { get; set; }
	public Guid? HotelId { get; set; }
	public int? MinCapacity { get; set; }
	public int? MaxCapacity { get; set; }
	public decimal? MinPrice { get; set; }
	public decimal? MaxPrice { get; set; }
	public bool? AvailableOnly { get; set; }

	public HotelRoomSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class HotelReservationCreateParams : BaseCreateParams<TagHotelReservation> {
	[UValidationRequired("checkInDateIsRequired")]
	public DateTime CheckInDate { get; set; }

	[UValidationRequired("checkOutDateIsRequired")]
	public DateTime CheckOutDate { get; set; }

	[UValidationRequired("guestCountIsRequired")]
	public int GuestCount { get; set; }

	[UValidationRequired("userIsRequired")]
	public Guid UserId { get; set; }

	[UValidationRequired("roomIsRequired")]
	public Guid RoomId { get; set; }

	public decimal? TotalPrice { get; set; }

	public string? GuestName { get; set; }
	public string? GuestPhone { get; set; }
	public string? Notes { get; set; }
	public List<ReservationGuestParams>? Guests { get; set; }
	public int PenaltyPrecentEveryDate { get; set; }
	public string? RoomNumber { get; set; }
	public string? GroupCode { get; set; }
	public string? GroupName { get; set; }
}

public sealed class ReservationGuestParams {
	[UValidationRequired("nameIsRequired")]
	public string FullName { get; set; } = null!;

	public string? NationalCode { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Nationality { get; set; }
	public string? PassportNumber { get; set; }
	public DateTime? BirthDate { get; set; }
	public string? FatherName { get; set; }
	public string? Gender { get; set; }
}

public sealed class HotelReservationBookParams : BaseParams {
	[UValidationRequired("roomIsRequired")]
	public Guid RoomId { get; set; }

	[UValidationRequired("checkInDateIsRequired")]
	public DateTime CheckInDate { get; set; }

	[UValidationRequired("checkOutDateIsRequired")]
	public DateTime CheckOutDate { get; set; }

	[UValidationRequired("guestCountIsRequired")]
	public int GuestCount { get; set; }

	public List<ReservationGuestParams>? Guests { get; set; }
	public string? GuestName { get; set; }
	public string? GuestPhone { get; set; }
	public string? Notes { get; set; }
	public bool PayFromWallet { get; set; }
}

public sealed class HotelReservationCancelParams : BaseParams {
	[UValidationRequired("reservationIsRequired")]
	public Guid Id { get; set; }

	public string? Reason { get; set; }
}

public sealed class HotelInvoicePayParams {
	public required Guid InvoiceId { get; set; }
	public required Guid UserId { get; set; }
}

public sealed class HotelReservationUpdateParams : BaseUpdateParams<TagHotelReservation> {
	public DateTime? CheckInDate { get; set; }
	public DateTime? CheckOutDate { get; set; }
	public int? GuestCount { get; set; }
	public decimal? TotalPrice { get; set; }
	public string? GuestName { get; set; }
	public string? GuestPhone { get; set; }
	public string? Notes { get; set; }
	public List<ReservationGuestParams>? Guests { get; set; }
	public string? RoomNumber { get; set; }
}

public sealed class HotelReservationReadParams : BaseReadParams<TagHotelReservation> {
	public Guid? UserId { get; set; }
	public string? UserName { get; set; }
	public Guid? RoomId { get; set; }
	public Guid? HotelId { get; set; }
	public DateTime? CheckInDate { get; set; }
	public DateTime? CheckOutDate { get; set; }
	public bool? ActiveOnly { get; set; }
	public bool? UpcomingOnly { get; set; }
	public bool? PastOnly { get; set; }
	public string? GroupCode { get; set; }

	public HotelReservationSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class HotelInvoiceCreateParams : BaseCreateParams<TagHotelInvoice> {
	[UValidationRequired("priceIsRequired")]
	public decimal DebtAmount { get; set; }

	public decimal CreditorAmount { get; set; }
	public decimal PaidAmount { get; set; }
	public decimal PenaltyAmount { get; set; }
	public int PenaltyPrecentEveryDate { get; set; }

	[UValidationRequired("reservationIsRequired")]
	public Guid ReservationId { get; set; }

	[UValidationRequired("dateIsRequired")]
	public DateTime DueDate { get; set; }
}

public sealed class HotelInvoiceUpdateParams : BaseUpdateParams<TagHotelInvoice> {
	public decimal? DebtAmount { get; set; }
	public decimal? CreditorAmount { get; set; }
	public decimal? PaidAmount { get; set; }
	public decimal? PenaltyAmount { get; set; }
	public int? PenaltyPrecentEveryDate { get; set; }
	public DateTime? DueDate { get; set; }
	public Guid? ReservationId { get; set; }
}

public sealed class HotelInvoiceReadParams : BaseReadParams<TagHotelInvoice> {
	public HotelInvoiceSelectorArgs SelectorArgs { get; set; } = new();

	public Guid? ReservationId { get; set; }
	public Guid? UserId { get; set; }
	public Guid? HotelId { get; set; }
	public bool? IsPaid { get; set; }
	public bool? IsOverdue { get; set; }
	public DateTime? MinDueDate { get; set; }
	public DateTime? MaxDueDate { get; set; }
	public decimal? MinDebtAmount { get; set; }
	public decimal? MaxDebtAmount { get; set; }
}

public sealed class HotelRateCreateParams : BaseCreateParams<TagHotelRate> {
	[UValidationRequired("HotelIdRequired")]
	public Guid HotelId { get; set; }

	public Guid? RoomId { get; set; }

	[UValidationRequired("startDateIsRequired")]
	public DateTime StartDate { get; set; }

	[UValidationRequired("endDateIsRequired")]
	public DateTime EndDate { get; set; }

	public decimal? Price { get; set; }
	public decimal? Percent { get; set; }
	public List<int>? Weekdays { get; set; }
	public int? MinNights { get; set; }
}

public sealed class HotelRateUpdateParams : BaseUpdateParams<TagHotelRate> {
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public decimal? Price { get; set; }
	public decimal? Percent { get; set; }
	public List<int>? Weekdays { get; set; }
	public int? MinNights { get; set; }
}

public sealed class HotelRateReadParams : BaseReadParams<TagHotelRate> {
	public Guid? HotelId { get; set; }
	public Guid? RoomId { get; set; }
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
}

public sealed class HotelRoomCalendarParams : BaseParams {
	[UValidationRequired("roomIsRequired")]
	public Guid RoomId { get; set; }

	public DateTime FromDate { get; set; }
	public int Days { get; set; } = 31;
}

public sealed class HotelHousekeepingParams : BaseParams {
	[UValidationRequired("roomIsRequired")]
	public Guid RoomId { get; set; }

	[UValidationRequired("numberRequired")]
	public string Number { get; set; } = null!;

	public TagHousekeeping Status { get; set; }
	public string? Note { get; set; }
}

public sealed class HotelGroupRoomParams {
	public Guid RoomId { get; set; }
	public int Count { get; set; } = 1;
	public int GuestCount { get; set; } = 1;
}

public sealed class HotelReservationGroupParams : BaseParams {
	[UValidationRequired("userIsRequired")]
	public Guid UserId { get; set; }

	[UValidationRequired("checkInDateIsRequired")]
	public DateTime CheckInDate { get; set; }

	[UValidationRequired("checkOutDateIsRequired")]
	public DateTime CheckOutDate { get; set; }

	[UValidationRequired("titleIsRequired")]
	public string GroupName { get; set; } = null!;

	[UValidationMinCollectionLength(1, "roomIsRequired")]
	public List<HotelGroupRoomParams> Rooms { get; set; } = [];

	public string? GuestPhone { get; set; }
	public string? Notes { get; set; }
	public int PenaltyPrecentEveryDate { get; set; }
}

public sealed class HotelReservationExtendParams : BaseParams {
	[UValidationRequired("reservationIsRequired")]
	public Guid Id { get; set; }

	[UValidationRequired("checkOutDateIsRequired")]
	public DateTime CheckOutDate { get; set; }
}

public sealed class HotelReservationChangeRoomParams : BaseParams {
	[UValidationRequired("reservationIsRequired")]
	public Guid Id { get; set; }

	[UValidationRequired("roomIsRequired")]
	public Guid RoomId { get; set; }

	public bool KeepPrice { get; set; }
	public string? RoomNumber { get; set; }
}

public sealed class HotelNightAuditParams : BaseParams {
	[UValidationRequired("HotelIdRequired")]
	public Guid HotelId { get; set; }

	public DateTime? Date { get; set; }
}

public sealed class HotelGuestExportParams : BaseParams {
	[UValidationRequired("HotelIdRequired")]
	public Guid HotelId { get; set; }

	public DateTime FromDate { get; set; }
	public DateTime ToDate { get; set; }
}

