namespace SinaMN75U.Data.Entities;

[Index(nameof(Id), IsUnique = true, Name = "IX_Id")]
[Index(nameof(Tags), Name = "IX_Tags")]
public class BaseEntity<T, TJ> where T : Enum where TJ : class {
	[Key]
	public required Guid Id { get; set; }

	[Required]
	public required DateTime CreatedAt { get; set; }

	[Required]
	public required DateTime UpdatedAt { get; set; }

	public DateTime? DeletedAt { get; set; }

	[Required]
	public required TJ JsonData { get; set; }

	[Required]
	public required ICollection<T> Tags { get; set; }
}

public sealed class VisitCount {
	public required Guid UserId { get; set; }
	public required int Count { get; set; } = 1;
}

public sealed class GeneralJsonData {
	public string Title { get; set; } = "";
	public string Description { get; set; } = "";
}