namespace SinaMN75U.Data.Entities;

public class BaseEntity<T, TJ> where T : Enum where TJ : class {
	[Key]
	public required Guid Id { get; set; }

	[Required]
	public required DateTime CreatedAt { get; set; }
	
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