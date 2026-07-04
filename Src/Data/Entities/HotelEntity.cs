namespace SinaMN75U.Data.Entities;

[Table("Hotels")]
public class HotelEntity : BaseEntity<TagHotel, BaseJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	[Required, MaxLength(100)]
	public required string City { get; set; }

	[Required, MaxLength(100)]
	public required string Country { get; set; }

	public ICollection<HotelRoomEntity> Rooms { get; set; } = [];
	public ICollection<MediaEntity> Media { get; set; } = [];
}

[Table("HotelRooms")]
public class HotelRoomEntity : BaseEntity<TagRoom, BaseJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	public required int Capacity { get; set; }

	[Required, Column(TypeName = "decimal(24,2)")]
	public decimal PricePerNight { get; set; }
	
	public required Guid HotelId { get; set; }
	public HotelEntity Hotel { get; set; } = null!;

	public ICollection<MediaEntity> Media { get; set; } = [];
}

[Table("Dorms")]
public class DormEntity : BaseEntity<TagDorm, BaseJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	[Required, MaxLength(100)]
	public required string City { get; set; }

	[Required, MaxLength(100)]
	public required string Country { get; set; }

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