namespace SinaMN75U.Data.Entities;

[Table("Blogs")]
[Microsoft.EntityFrameworkCore.Index(nameof(Slug), IsUnique = true, Name = "IX_Blogs_Slug")]
public sealed class BlogEntity : BaseEntity<TagBlog, BlogJson> {
	[Required, MaxLength(200)]
	public required string Title { get; set; }

	[MaxLength(300)]
	public string? Subtitle { get; set; }

	[MaxLength(160)]
	public string? Slug { get; set; }

	// Full body (HTML/Markdown), unbounded like ProductEntity.Content.
	public string? Content { get; set; }

	public int ViewCount { get; set; }

	public DateTime? PublishedAt { get; set; }

	public ICollection<MediaEntity> Media { get; set; } = [];
	public ICollection<CategoryEntity> Categories { get; set; } = [];

	public ICollection<CommentEntity> Comments { get; set; } = [];
}

public sealed class BlogJson : BaseJson {
	public string? MetaTitle { get; set; }
	public string? MetaDescription { get; set; }
	public string? Source { get; set; }
	public int? ReadingTimeMinutes { get; set; }
}
