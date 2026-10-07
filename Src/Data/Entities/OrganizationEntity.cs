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
