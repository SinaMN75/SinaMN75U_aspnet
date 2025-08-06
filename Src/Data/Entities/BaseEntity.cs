namespace SinaMN75U.Data.Entities;

[Index(nameof(Id), IsUnique = true, Name = "IX_Id")]
[Index(nameof(Tags), Name = "IX_Tags")]
public class BaseEntity<T, TJ> {
	[Key]
	public Guid Id { get; set; } = Guid.CreateVersion7();

	[Required]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	[Required]
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	[Required]
	public required TJ JsonData { get; set; }

	[Required]
	public required ICollection<T> Tags { get; set; }
}

public class VisitCount {
	public required Guid UserId { get; set; }
	public required int Count { get; set; } = 1;
}