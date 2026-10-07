namespace SinaMN75U.Data.Entities;

// ---------------- Hotel ----------------

[Table("Hotels")]
public class HotelEntity : BaseEntity<TagHotel, HotelJson>, IOrganizationScoped {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	[Required, MaxLength(20)]
	public required string CityCode { get; set; }

	public int Stars { get; set; }

	[MaxLength(500)]
	public string? Address { get; set; }

	[MaxLength(20)]
	public string? PhoneNumber { get; set; }

	[MaxLength(100)]
	public string? Email { get; set; }

	public Guid? OrganizationId { get; set; }
	public OrganizationEntity? Organization { get; set; }

	public ICollection<HotelRoomEntity> Rooms { get; set; } = [];
	public ICollection<HotelReservationEntity> Reservations { get; set; } = [];
	public ICollection<CommentEntity> Comments { get; set; } = [];
	public ICollection<MediaEntity> Media { get; set; } = [];
}

public sealed class HotelJson : BaseJson {
	public string? Description { get; set; }
	public string? Policies { get; set; }
	public string? CheckInTime { get; set; }
	public string? CheckOutTime { get; set; }
	public List<string> Highlights { get; set; } = [];
	public List<string> Rules { get; set; } = [];
	public string? HowToGetThere { get; set; }
	public List<PlaceNearby> Nearby { get; set; } = [];
	public List<PlaceFaq> Faqs { get; set; } = [];
	public string? Website { get; set; }
	public string? Whatsapp { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int CancellationFreeHours { get; set; } = 24;
	public int CancellationPenaltyNights { get; set; } = 1;
}

// ---------------- HotelRoom ----------------

[Table("HotelRooms")]
public class HotelRoomEntity : BaseEntity<TagRoom, HotelRoomJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	public required int Capacity { get; set; }

	[Required, Column(TypeName = "decimal(24,2)")]
	public decimal PricePerNight { get; set; }

	[MaxLength(20)]
	public string? RoomNumber { get; set; }

	public int Quantity { get; set; } = 1;

	public bool IsAvailable { get; set; } = true;

	public required Guid HotelId { get; set; }
	public HotelEntity Hotel { get; set; } = null!;

	public ICollection<HotelReservationEntity> Reservations { get; set; } = [];
	public ICollection<MediaEntity> Media { get; set; } = [];
}

public sealed class HotelRoomJson : BaseJson {
	public string? Description { get; set; }
	public string? BedType { get; set; }
	public double? SizeSquareMeters { get; set; }
	public int? Floor { get; set; }
	public int? ExtraGuestCapacity { get; set; }
	public decimal? ExtraGuestPrice { get; set; }
}

// ---------------- HotelReservation ----------------

[Table("HotelReservations")]
public sealed class HotelReservationEntity : BaseEntity<TagHotelReservation, HotelReservationJson> {
	public required DateTime CheckInDate { get; set; }
	public required DateTime CheckOutDate { get; set; }

	public required int GuestCount { get; set; }

	[Required, Column(TypeName = "decimal(24,2)")]
	public required decimal TotalPrice { get; set; }

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	public required Guid RoomId { get; set; }
	public HotelRoomEntity Room { get; set; } = null!;

	public required Guid HotelId { get; set; }
	public HotelEntity Hotel { get; set; } = null!;

	public ICollection<HotelInvoiceEntity> Invoices { get; set; } = [];
}

public sealed class HotelReservationJson : BaseJson {
	public string? GuestName { get; set; }
	public string? GuestPhone { get; set; }
	public string? Notes { get; set; }
	public int NightCount { get; set; }
	public string? ReservationCode { get; set; }
	public List<ReservationGuestJson> Guests { get; set; } = [];
	public DateTime? CancelledAt { get; set; }
	public string? CancelReason { get; set; }
	public decimal? CancellationPenalty { get; set; }
	public decimal? RefundAmount { get; set; }
}

public sealed class ReservationGuestJson {
	public required string FullName { get; set; }
	public string? NationalCode { get; set; }
	public string? PhoneNumber { get; set; }
}

// ---------------- HotelInvoice ----------------

[Table("HotelInvoices")]
public sealed class HotelInvoiceEntity : BaseEntity<TagHotelInvoice, HotelInvoiceJson> {
	public required decimal DebtAmount { get; set; }
	public required decimal CreditorAmount { get; set; }
	public required decimal PaidAmount { get; set; }
	public required decimal PenaltyAmount { get; set; }

	public required DateTime DueDate { get; set; }

	public Guid? ReservationId { get; set; }
	public HotelReservationEntity? Reservation { get; set; }
}

public sealed class HotelInvoiceJson : BaseJson {
	public int PenaltyPrecentEveryDate { get; set; }
	public bool Posted { get; set; }
}
