namespace SinaMN75U.Data.Params;

public sealed class BlogCreateParams : BaseCreateParams<TagBlog> {
	public string Title { get; set; } = null!;
	public string? Subtitle { get; set; }
	public string? Slug { get; set; }
	public string? Content { get; set; }

	public string? MetaTitle { get; set; }
	public string? MetaDescription { get; set; }
	public string? Source { get; set; }
	public int? ReadingTimeMinutes { get; set; }

	public DateTime? PublishedAt { get; set; }

	public ICollection<Guid>? Categories { get; set; }
	public ICollection<Guid>? Media { get; set; }
}

public sealed class BlogUpdateParams : BaseUpdateParams<TagBlog> {
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Slug { get; set; }
	public string? Content { get; set; }

	public string? MetaTitle { get; set; }
	public string? MetaDescription { get; set; }
	public string? Source { get; set; }
	public int? ReadingTimeMinutes { get; set; }

	public DateTime? PublishedAt { get; set; }

	public ICollection<Guid>? AddCategories { get; set; }
	public ICollection<Guid>? RemoveCategories { get; set; }
	public ICollection<Guid>? Categories { get; set; }

	public ICollection<Guid>? Media { get; set; }
}

public sealed class BlogReadParams : BaseReadParams<TagBlog> {
	public string? Query { get; set; }
	public string? Title { get; set; }
	public string? Slug { get; set; }
	public bool? OnlyPublished { get; set; }
	public ICollection<Guid>? Categories { get; set; }
	public BlogSelectorArgs SelectorArgs { get; set; } = new();
}
