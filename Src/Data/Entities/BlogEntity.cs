namespace SinaMN75U.Data.Entities;

[Table("Blogs")]
[Microsoft.EntityFrameworkCore.Index(nameof(Slug), IsUnique = true, Name = "IX_Blogs_Slug")]
[Microsoft.EntityFrameworkCore.Index(nameof(Code), IsUnique = true, Name = "IX_Blogs_Code")]
[Microsoft.EntityFrameworkCore.Index(nameof(CreatorId))]
public sealed class BlogEntity : BaseEntity<TagBlog, BlogJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	[MaxLength(100)]
	public string? Subtitle { get; set; }

	[MaxLength(100)]
	public string? Code { get; set; }

	[MaxLength(2000)]
	public string? Description { get; set; }

	[MaxLength(200)]
	public string? Slug { get; set; }

	[MaxLength(100)]
	public string? Type { get; set; }

	public string? Content { get; set; }

	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	
	public Guid? ParentId { get; set; }
	public BlogEntity? Parent { get; set; }

	[InverseProperty("Parent")]
	public ICollection<BlogEntity> Children { get; set; } = [];

	public ICollection<MediaEntity> Media { get; set; } = [];
	public ICollection<CategoryEntity> Categories { get; set; } = [];
	public ICollection<CommentEntity> Comments { get; set; } = [];
}

public sealed class BlogJson : BaseJson {
	public string? MetaTitle { get; set; }
	public string? MetaDescription { get; set; }
	public string? Source { get; set; }

	public string? ActionType { get; set; }
	public string? ActionTitle { get; set; }
	public string? ActionUri { get; set; }
	public ICollection<Guid> RelatedBlogs { get; set; } = [];
}
