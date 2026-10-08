namespace SinaMN75U.Data.Entities;

// ---------------- Dorm ----------------

[Table("Dorms")]
public class DormEntity : BaseEntity<TagDorm, DormJson>, IOrganizationScoped {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	[Required, MaxLength(20)]
	public required string CityCode { get; set; }

	[MaxLength(500)]
	public string? Address { get; set; }

	[MaxLength(20)]
	public string? PhoneNumber { get; set; }

	public Guid? OrganizationId { get; set; }
	public OrganizationEntity? Organization { get; set; }

	public ICollection<DormRoomEntity> Rooms { get; set; } = [];
	public ICollection<CommentEntity> Comments { get; set; } = [];
	public ICollection<MediaEntity> Media { get; set; } = [];
}

public sealed class DormJson : BaseJson {
	public string? Description { get; set; }
	public string? Policies { get; set; }
	public List<string> Highlights { get; set; } = [];
	public List<string> Rules { get; set; } = [];
	public List<string> RequiredDocuments { get; set; } = [];
	public string? NearbyUniversity { get; set; }
	public int? UniversityWalkMinutes { get; set; }
	public string? VisitingHours { get; set; }
	public string? CurfewTime { get; set; }
	public int? MinimumStayMonths { get; set; }
	public string? HowToGetThere { get; set; }
	public List<PlaceNearby> Nearby { get; set; } = [];
	public List<PlaceFaq> Faqs { get; set; } = [];
	public string? Website { get; set; }
	public string? Whatsapp { get; set; }
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public List<string> LaundryMachines { get; set; } = [];
	public int? LaundrySlotMinutes { get; set; }
}

[Table("DormRooms")]
public class DormRoomEntity : BaseEntity<TagDormRoom, DormRoomJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	public int Capacity { get; set; }

	public required Guid DormId { get; set; }
	public DormEntity Dorm { get; set; } = null!;

	public ICollection<DormBedEntity> Beds { get; set; } = [];
	public ICollection<MediaEntity> Media { get; set; } = [];
}

public sealed class DormRoomJson : BaseJson {
	public string? Description { get; set; }
	public int? Floor { get; set; }
	public double? SizeSquareMeters { get; set; }
}

[Table("DormBeds")]
public class DormBedEntity : BaseEntity<TagDormBed, DormBedJson> {
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

public class DormBedJson : BaseJson {
	public string? Description { get; set; }
}

[Table("Contracts")]
public sealed class DormBedContractEntity : BaseEntity<TagDormBedContract, DormBedContractJson> {
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

public class DormBedContractJson : BaseJson {
	public DateTime? SettledAt { get; set; }
	public decimal? Deductions { get; set; }
	public string? DeductionReason { get; set; }
	public decimal? DepositRefund { get; set; }
	public List<ContractBedChange> BedHistory { get; set; } = [];
	public string? GuardianName { get; set; }
	public string? GuardianPhone { get; set; }
	public string? EmergencyName { get; set; }
	public string? EmergencyPhone { get; set; }
	public string? EmergencyRelation { get; set; }
	public Guid? ApplicationId { get; set; }
	public List<HandoverItem> CheckInChecklist { get; set; } = [];
	public List<HandoverItem> CheckOutChecklist { get; set; } = [];
	public Guid? DamageInvoiceId { get; set; }
}

public sealed class HandoverItem {
	public string Title { get; set; } = "";
	public bool Ok { get; set; }
	public string? Note { get; set; }
	public decimal Damage { get; set; }
	public List<string> PhotoUrls { get; set; } = [];
}

public sealed class ContractBedChange {
	public Guid BedId { get; set; }
	public DateTime From { get; set; }
	public DateTime To { get; set; }
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
	public bool DueReminded { get; set; }
	public bool OverdueReminded { get; set; }
	public bool Posted { get; set; }
}

[Table("DormApplications")]
public sealed class DormApplicationEntity : BaseEntity<TagDormApplication, DormApplicationJson> {
	public required DateTime DesiredStartDate { get; set; }
	public DateTime? DesiredEndDate { get; set; }

	public required Guid DormId { get; set; }
	public DormEntity Dorm { get; set; } = null!;

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;
}

public sealed class DormApplicationJson : BaseJson {
	public string? ReviewNote { get; set; }
	public Guid? ReviewedBy { get; set; }
	public DateTime? ReviewedAt { get; set; }
	public Guid? BedId { get; set; }
	public Guid? ContractId { get; set; }
	public List<DormApplicationDocument> Documents { get; set; } = [];
}

public sealed class DormApplicationDocument {
	public string Title { get; set; } = "";
	public string? Url { get; set; }
	public bool? Approved { get; set; }
}

[Table("DormRecords")]
public sealed class DormRecordEntity : BaseEntity<TagDormRecord, DormRecordJson> {
	[Required, MaxLength(200)]
	public required string Title { get; set; }

	public required DateTime Date { get; set; }
	public DateTime? EndDate { get; set; }

	public required Guid DormId { get; set; }
	public DormEntity Dorm { get; set; } = null!;

	public Guid? UserId { get; set; }
	public UserEntity? User { get; set; }
}

public sealed class DormRecordJson : BaseJson {
	public string? Body { get; set; }
	public string? VisitorName { get; set; }
	public string? VisitorPhone { get; set; }
	public string? VisitorNationalCode { get; set; }
	public string? Relation { get; set; }
	public Guid? RoomId { get; set; }
	public Guid? ReviewedBy { get; set; }
	public decimal? Penalty { get; set; }
	public Guid? InvoiceId { get; set; }
	public List<HandoverItem> Items { get; set; } = [];
}

[Table("DormMeals")]
public sealed class DormMealEntity : BaseEntity<TagDormMeal, BaseJson> {
	[Required, MaxLength(200)]
	public required string Title { get; set; }

	public required DateTime Date { get; set; }

	[Column(TypeName = "decimal(24,2)")]
	public decimal Price { get; set; }

	public int? Capacity { get; set; }

	public required Guid DormId { get; set; }
	public DormEntity Dorm { get; set; } = null!;

	public ICollection<DormBookingEntity> Bookings { get; set; } = [];
}

[Table("DormBookings")]
public sealed class DormBookingEntity : BaseEntity<TagDormBooking, DormBookingJson> {
	public required DateTime StartAt { get; set; }
	public DateTime? EndAt { get; set; }

	[MaxLength(50)]
	public string? Resource { get; set; }

	[Column(TypeName = "decimal(24,2)")]
	public decimal Price { get; set; }

	public required Guid DormId { get; set; }
	public DormEntity Dorm { get; set; } = null!;

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	public Guid? MealId { get; set; }
	public DormMealEntity? Meal { get; set; }
}

public sealed class DormBookingJson : BaseJson {
	public Guid? InvoiceId { get; set; }
}

