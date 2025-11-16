namespace SinaMN75U.Data.Entities;

[Table("Media")]
[Index(nameof(UserId), Name = "IX_Media_UserId")]
[Index(nameof(ContentId), Name = "IX_Media_ContentId")]
[Index(nameof(CategoryId), Name = "IX_Media_CategoryId")]
[Index(nameof(ProductId), Name = "IX_Media_ProductId")]
public class MediaEntity : BaseEntity<TagMedia, MediaJson> {
	[Required]
	[MaxLength(200)]
	public required string Path { get; set; }

	[JsonIgnore]
	public Guid? UserId { get; set; }

	[JsonIgnore]
	public UserEntity? User { get; set; }

	[JsonIgnore]
	public Guid? ContentId { get; set; }

	[JsonIgnore]
	public ContentEntity? Content { get; set; }

	[JsonIgnore]
	public Guid? CategoryId { get; set; }

	[JsonIgnore]
	public CategoryEntity? Category { get; set; }

	[JsonIgnore]
	public Guid? CommentId { get; set; }

	[JsonIgnore]
	public CommentEntity? Comment { get; set; }

	[JsonIgnore]
	public Guid? ProductId { get; set; }

	[JsonIgnore]
	public ProductEntity? Product { get; set; }
	
	[NotMapped]
	public string Url => $"{Server.BaseUrl}/Media/{Path}";
}

public class MediaJson {
	public string? Title { get; set; }
	public string? Description { get; set; }
}