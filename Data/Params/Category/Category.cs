namespace SinaMN75U.Data.Params.Category;

public class CategoryCreateParams: BaseParams {
	public required string Title { get; set; }
	public string? TitleTr1 { get; set; }
	public string? TitleTr2 { get; set; }
	public string? Subtitle { get; set; }
	public required List<TagCategory> Tags { get; set; }
}

public class CategoryUpdateParams: BaseParams {
	public required Guid Id { get; set; }
	public string? Title { get; set; }
	public string? TitleTr1 { get; set; }
	public string? TitleTr2 { get; set; }
	public string? Subtitle { get; set; }
	public List<TagCategory>? AddTags { get; set; }
	public List<TagCategory>? RemoveTags { get; set; }
}

public class CategoryFilterParams: BaseParams {
	public List<Guid>? Ids { get; set; }
	public List<TagCategory>? Tags { get; set; }
	public bool ShowMedia { get; set; } = false;
}