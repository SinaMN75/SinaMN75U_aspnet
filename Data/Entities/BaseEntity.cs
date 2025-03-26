namespace SinaMN75U.Data.Entities;

[Index(nameof(Id), IsUnique = true, Name = "IX_Id")]
[Index(nameof(CreatedAt), Name = "IX_CreatedAt")]
[Index(nameof(Tags), Name = "IX_Tags")]
public class BaseEntity<T> {
	[Key]
	public required Guid Id { get; set; }

	[Required]
	public required DateTime CreatedAt { get; set; }

	[Required]
	public required DateTime UpdatedAt { get; set; }
	
	[Required]
	public required List<T> Tags { get; set; }
}