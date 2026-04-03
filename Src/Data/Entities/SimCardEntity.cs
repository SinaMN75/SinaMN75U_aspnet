namespace SinaMN75U.Data.Entities;

[Table("SimCards")]
public sealed class SimCardEntity : BaseEntity<TagSimCard, SimCardJson> {
	[Required, MaxLength(20)]
	public required string Number { get; set; }

	[MaxLength(100)]
	public string? Serial { get; set; }

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;
}

public sealed class SimCardJson {
	public string? Description { get; set; }
}