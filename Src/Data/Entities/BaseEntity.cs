namespace SinaMN75U.Data.Entities;

public class BaseEntity<T, TJ> where T : Enum where TJ : BaseJsonData {
	[Key]
	public required Guid Id { get; set; }

	[Required]
	public required DateTime CreatedAt { get; set; }
	
	[Required]
	public required TJ JsonData { get; set; }

	[Required]
	public required ICollection<T> Tags { get; set; }
	
	public required Guid CreatorId { get; set; }
	public UserEntity Creator { get; set; } = null!;
}

public class BaseJsonData {
	public string Detail1 { get; set; } = "";
	public string Detail2 { get; set; } = "";
}