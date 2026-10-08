namespace SinaMN75U.Data.Entities;

[Table("Organizations")]
public sealed class OrganizationEntity : BaseEntity<TagOrganization, OrganizationJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	public required Guid OwnerId { get; set; }
	public UserEntity Owner { get; set; } = null!;
}

public sealed class OrganizationJson : BaseJson {
	public decimal CommissionPercent { get; set; }
	public List<OrganizationMember> Members { get; set; } = [];
	public List<OrganizationSettlement> Settlements { get; set; } = [];
	public OrganizationPlan? Plan { get; set; }
	public string? LogoUrl { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public string? NationalId { get; set; }
	public string? EconomicCode { get; set; }
	public decimal VatPercent { get; set; }
	public string? TaxServiceId { get; set; }
}

public sealed class OrganizationPlan {
	public string? Title { get; set; }
	public int? MaxPlaces { get; set; }
	public int? MaxRooms { get; set; }
	public int? MaxBeds { get; set; }
	public DateTime? ExpiresAt { get; set; }
}

public sealed class OrganizationSettlement {
	public Guid Id { get; set; }
	public decimal Amount { get; set; }
	public string Iban { get; set; } = "";
	public DateTime CreatedAt { get; set; }
	public DateTime? ProcessedAt { get; set; }
	public bool? Approved { get; set; }
	public string? Note { get; set; }
}

public sealed class OrganizationMember {
	public Guid UserId { get; set; }
	public List<TagUser> Permissions { get; set; } = [];
}

public interface IOrganizationScoped {
	Guid Id { get; }
	string Title { get; }
	Guid? OrganizationId { get; }
	ICollection<Guid> AdminUserIds { get; set; }
}

[Table("StaffShifts")]
public sealed class StaffShiftEntity : BaseEntity<TagStaffShift, StaffShiftJson> {
	public required DateTime StartAt { get; set; }
	public required DateTime EndAt { get; set; }
	public Guid? PlaceId { get; set; }

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	public required Guid OrganizationId { get; set; }
	public OrganizationEntity Organization { get; set; } = null!;
}

public sealed class StaffShiftJson : BaseJson {
	public DateTime? CheckedInAt { get; set; }
	public DateTime? CheckedOutAt { get; set; }
	public Guid? RegisteredBy { get; set; }
}

[Table("StaffTasks")]
public sealed class StaffTaskEntity : BaseEntity<TagStaffTask, StaffTaskJson> {
	[Required, MaxLength(200)]
	public required string Title { get; set; }

	public Guid? PlaceId { get; set; }
	public Guid? AssigneeId { get; set; }
	public DateTime? DueDate { get; set; }

	public required Guid OrganizationId { get; set; }
	public OrganizationEntity Organization { get; set; } = null!;
}

public sealed class StaffTaskJson : BaseJson {
	public string? Description { get; set; }
	public string? Location { get; set; }
	public Guid? RoomId { get; set; }
	public Guid? BedId { get; set; }
	public Guid? RequesterId { get; set; }
	public DateTime? DoneAt { get; set; }
	public string? DoneNote { get; set; }
	public decimal? Cost { get; set; }
}

[Table("OrganizationCustomers")]
public sealed class OrganizationCustomerEntity : BaseEntity<TagOrganizationCustomer, OrganizationCustomerJson> {
	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	public required Guid OrganizationId { get; set; }
	public OrganizationEntity Organization { get; set; } = null!;
}

public sealed class OrganizationCustomerJson : BaseJson {
	public string? Note { get; set; }
	public Guid? RegisteredBy { get; set; }
}

[Table("ActivityLogs")]
public sealed class ActivityLogEntity : BaseEntity<TagActivityLog, ActivityLogJson> {
	[Required, MaxLength(200)]
	public required string Path { get; set; }

	public Guid? OrganizationId { get; set; }
	public Guid? EntityId { get; set; }
}

public sealed class ActivityLogJson : BaseJson {
	public string? UserName { get; set; }
	public string? Body { get; set; }
}
