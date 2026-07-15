namespace SinaMN75U.Data.Entities;

// ---------------- Hotel ----------------

[Table("Hotels")]
public class HotelEntity : BaseEntity<TagHotel, HotelJson> {
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

	public ICollection<HotelRoomEntity> Rooms { get; set; } = [];
	public ICollection<HotelReservationEntity> Reservations { get; set; } = [];
	public ICollection<MediaEntity> Media { get; set; } = [];
}

public sealed class HotelJson : BaseJson {
	public string? Description { get; set; }
	public string? Policies { get; set; }
	public string? CheckInTime { get; set; }
	public string? CheckOutTime { get; set; }
	public List<string> Amenities { get; set; } = [];
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
	public List<string> Amenities { get; set; } = [];
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
}

// ---------------- Dorm ----------------

[Table("Dorms")]
public class DormEntity : BaseEntity<TagDorm, BaseJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	[Required, MaxLength(20)]
	public required string CityCode { get; set; }

	public ICollection<DormRoomEntity> Rooms { get; set; } = [];
	public ICollection<MediaEntity> Media { get; set; } = [];
}

[Table("DormRooms")]
public class DormRoomEntity : BaseEntity<TagDormRoom, BaseJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	public required Guid DormId { get; set; }
	public DormEntity Dorm { get; set; } = null!;

	public ICollection<DormBedEntity> Beds { get; set; } = [];
	public ICollection<MediaEntity> Media { get; set; } = [];
}

[Table("DormBeds")]
public class DormBedEntity : BaseEntity<TagDormBed, BaseJson> {
	[Required, MaxLength(4)]
	public required string Title { get; set; }
	
	[Required, Column(TypeName = "decimal(24,2)")]
	public required decimal Deposit { get; set; }

	[Required, Column(TypeName = "decimal(24,2)")]
	public required decimal MonthlyRent { get; set; }

	public required Guid RoomId { get; set; }
	public DormRoomEntity Room { get; set; } = null!;

	public ICollection<MediaEntity> Media { get; set; } = [];
	public ICollection<DormBedContractEntity> Contracts { get; set; } = [];
}

[Table("Contracts")]
public sealed class DormBedContractEntity : BaseEntity<TagDormBedContract, BaseJson> {
	public required DateTime StartDate { get; set; }
	public required DateTime EndDate { get; set; }

	[Required, Column(TypeName = "decimal(24,2)")]
	public required decimal Deposit { get; set; }

	[Required, Column(TypeName = "decimal(24,2)")]
	public required decimal Rent { get; set; }

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	public required Guid BedId { get; set; }
	public DormBedEntity Bed { get; set; } = null!;

	public ICollection<DormBedInvoiceEntity> Invoices { get; set; } = [];
}

[Table("Invoices")]
public sealed class DormBedInvoiceEntity : BaseEntity<TagDormBedInvoice, DormBedInvoiceJson> {
	public required decimal DebtAmount { get; set; }
	public required decimal CreditorAmount { get; set; }
	public required decimal PaidAmount { get; set; }
	public required decimal PenaltyAmount { get; set; }

	public required DateTime DueDate { get; set; }

	public Guid? ContractId { get; set; }
	public DormBedContractEntity? Contract { get; set; }
}

public sealed class DormBedInvoiceJson : BaseJson {
	public int PenaltyPrecentEveryDate { get; set; }
}
