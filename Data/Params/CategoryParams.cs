namespace SinaMN75U.Data.Params;

public class CategoryCreateParams : BaseParams {
	[URequired("TitleRequired")]
	public required string Title { get; set; }

	[URequired("")]
	public string? Subtitle { get; set; }

	[UMinCollectionLength(1, "TagsRequired")]
	public required List<TagCategory> Tags { get; set; }
}

public class CategoryUpdateParams : BaseParams {
	[URequired("IdRequired")]
	public required Guid Id { get; set; }

	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public IEnumerable<TagCategory>? AddTags { get; set; }
	public IEnumerable<TagCategory>? RemoveTags { get; set; }
}

public class CategoryReadParams : BaseParams {
	public IEnumerable<Guid>? Ids { get; set; }
	public IEnumerable<TagCategory>? Tags { get; set; }
	public bool Media { get; set; } = false;
}