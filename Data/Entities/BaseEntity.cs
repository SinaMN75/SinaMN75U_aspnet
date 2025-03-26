namespace SinaMN75U.Data.Entities;

[Index(nameof(Id), IsUnique = true, Name = "IX_Users_Id")]
[Index(nameof(CreatedAt), Name = "IX_Users_CreatedAt")]
public class BaseEntity {
	[Key]
	public required Guid Id { get; set; }

	[Required]
	public required DateTime CreatedAt { get; set; }

	[Required]
	public required DateTime UpdatedAt { get; set; }
}