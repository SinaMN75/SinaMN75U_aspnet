namespace SinaMN75U.Data.Entities;

public class BaseEntity {
	[Key]
	public required Guid Id { get; set; }

	[Required]
	public required DateTime CreatedAt { get; set; }

	[Required]
	public required DateTime UpdatedAt { get; set; }
}