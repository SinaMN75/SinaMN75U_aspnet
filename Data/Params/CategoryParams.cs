namespace SinaMN75U.Data.Params;

public class CategoryCreateParams : BaseParams {
	[URequired("TitleRequired")]
	public required string Title { get; set; }

	[URequired("")]
	public string? Subtitle { get; set; }

	[UMinCollectionLength(1, "TagsRequired")]
	public required List<TagCategory> Tags { get; set; }
	
	public int? Order { get; set; }
	public string? Location { get; set; }
	public string? Type { get; set; }
	public string? Link { get; set; }
}

public class CategoryUpdateParams : BaseUpdateParams<TagCategory> {
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	
	public string? Link { get; set; }
	public int? Order { get; set; }
	public string? Location { get; set; }
	public string? Type { get; set; }
}

public class CategoryReadParams : BaseReadParams<TagCategory> {
	public IEnumerable<Guid>? Ids { get; set; }
	public bool ShowMedia { get; set; } = false;
}