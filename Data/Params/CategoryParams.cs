namespace SinaMN75U.Data.Params;

public class CategoryCreateParams : BaseParams {
	[UValidationRequired("TitleRequired")]
	public required string Title { get; set; }

	public string? Subtitle { get; set; }

	[UValidationMinCollectionLength(1, "TagsRequired")]
	public required List<TagCategory> Tags { get; set; }

	public Guid? ParentId { get; set; }

	public int? Order { get; set; }
	public string? Location { get; set; }
	public string? Type { get; set; }
	public string? Link { get; set; }
	public List<Guid>? RelatedProducts { get; set; }
}

public class CategoryUpdateParams : BaseUpdateParams<TagCategory> {
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Link { get; set; }
	public string? Location { get; set; }
	public string? Type { get; set; }
	public int? Order { get; set; }
	public Guid? ParentId { get; set; }
	public ICollection<Guid>? RelatedProducts { get; set; }
	public ICollection<Guid>? AddRelatedProducts { get; set; }
	public ICollection<Guid>? RemoveRelatedProducts { get; set; }
}

public class CategoryReadParams : BaseReadParams<TagCategory> {
	public IEnumerable<Guid>? Ids { get; set; }
	public bool ShowMedia { get; set; } = false;
}