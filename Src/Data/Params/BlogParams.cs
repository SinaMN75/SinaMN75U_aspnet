namespace SinaMN75U.Data.Params;

public sealed class BlogCreateParams : BaseCreateParams<TagBlog> {
	public string Title { get; set; } = null!;
	public string? Subtitle { get; set; }
	public string? Code { get; set; }
	public string? Description { get; set; }
	public string? Slug { get; set; }
	public string? Type { get; set; }
	public string? Content { get; set; }

	public string? MetaTitle { get; set; }
	public string? MetaDescription { get; set; }
	public string? Source { get; set; }

	public string? ActionType { get; set; }
	public string? ActionTitle { get; set; }
	public string? ActionUri { get; set; }

	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	
	public Guid? ParentId { get; set; }

	public ICollection<Guid>? Categories { get; set; }
	public ICollection<Guid>? RelatedBlogs { get; set; }
	public ICollection<BlogCreateParams>? Children { get; set; }
	public ICollection<Guid>? Media { get; set; }
}

public sealed class BlogUpdateParams : BaseUpdateParams<TagBlog> {
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Code { get; set; }
	public string? Description { get; set; }
	public string? Slug { get; set; }
	public string? Type { get; set; }
	public string? Content { get; set; }

	public string? MetaTitle { get; set; }
	public string? MetaDescription { get; set; }
	public string? Source { get; set; }

	public string? ActionType { get; set; }
	public string? ActionTitle { get; set; }
	public string? ActionUri { get; set; }

	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	
	public Guid? ParentId { get; set; }
	public Guid? CreatorId { get; set; }

	public ICollection<Guid>? RelatedBlogs { get; set; }
	public ICollection<Guid>? AddRelatedBlogs { get; set; }
	public ICollection<Guid>? RemoveRelatedBlogs { get; set; }

	public ICollection<Guid>? AddCategories { get; set; }
	public ICollection<Guid>? RemoveCategories { get; set; }
	public ICollection<Guid>? Categories { get; set; }

	public ICollection<Guid>? Media { get; set; }
}

public sealed class BlogReadParams : BaseReadParams<TagBlog> {
	public string? Query { get; set; }
	public string? Title { get; set; }
	public string? Code { get; set; }
	public string? Slug { get; set; }
	public Guid? ParentId { get; set; }
	public ICollection<Guid>? Categories { get; set; }
	public BlogSelectorArgs SelectorArgs { get; set; } = new();
}
