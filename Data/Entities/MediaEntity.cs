namespace SinaMN75U.Data.Entities;

[Table("Media")]
[Index(nameof(UserId), Name = "IX_Media_UserId")]
[Index(nameof(ContentId), Name = "IX_Media_ContentId")]
[Index(nameof(CategoryId), Name = "IX_Media_CategoryId")]
public class MediaEntity : BaseEntity<TagMedia, MediaJson> {
	[Required]
	[MaxLength(200)]
	public required string Path { get; set; }

	public Guid? UserId { get; set; }
	public UserEntity? User { get; set; }

	public Guid? ContentId { get; set; }
	public ContentEntity? Content { get; set; }

	public Guid? CategoryId { get; set; }
	public CategoryEntity? Category { get; set; }

	public Guid? CommentId { get; set; }
	public CommentEntity? Comment { get; set; }

	public MediaEntity MapToResponse() => new() {
		Id = Id,
		Path = $"{Server.ServerAddress}/Media/{Path}",
		Tags = Tags,
		JsonData = JsonData,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt
	};

	public MediaEntity MapToEntity() => new() {
		Path = $"{Server.ServerAddress}/Media/{Path}",
		Tags = Tags,
		CreatedAt = CreatedAt,
		UpdatedAt = CreatedAt,
		JsonData = JsonData,
		Id = Id,
		CategoryId = CategoryId,
		UserId = UserId,
		ContentId = ContentId
	};
}

public class MediaJson {
	public string? Title { get; set; }
	public string? Description { get; set; }
}