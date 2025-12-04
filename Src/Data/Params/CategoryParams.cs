namespace SinaMN75U.Data.Params;

public sealed class CategoryCreateParams : BaseParams {
	public Guid? Id { get; set; }

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
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Code { get; set; }
	public List<Guid>? RelatedProducts { get; set; }

	public IEnumerable<CategoryCreateParams> Children { get; set; } = [];
	public ICollection<Guid>? Media { get; set; }
	
	public CategoryEntity MapToEntity() => new() {
		Id = Id ?? Guid.CreateVersion7(),
		Title = Title,
		ParentId = ParentId,
		Order = Order,
		Code = Code,
		JsonData = new CategoryJson {
			Subtitle = Subtitle,
			Location = Location,
			Type = Type,
			Link = Link,
			Address = Address,
			PhoneNumber = PhoneNumber,
			RelatedProducts = RelatedProducts ?? []
		},
		Tags = Tags
	};

}

public sealed class CategoryUpdateParams : BaseUpdateParams<TagCategory> {
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Link { get; set; }
	public string? Location { get; set; }
	public string? Type { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Code { get; set; }
	public int? Order { get; set; }
	public Guid? ParentId { get; set; }
	public ICollection<Guid>? RelatedProducts { get; set; }
	public IEnumerable<Guid>? AddRelatedProducts { get; set; }
	public IEnumerable<Guid>? RemoveRelatedProducts { get; set; }
	public ICollection<Guid>? Media { get; set; }

	public double? ProductDeposit { get; set; }
	public double? ProductRent { get; set; }
	public bool UpdateInvoicesRent { get; set; } = false;
	
	public CategoryEntity MapToEntity(CategoryEntity e) {
		if (Title != null) e.Title = Title;
		if (Subtitle != null) e.JsonData.Subtitle = Subtitle;
		if (Link != null) e.JsonData.Link = Link;
		if (Location != null) e.JsonData.Location = Location;
		if (Type != null) e.JsonData.Type = Type;
		if (Address != null) e.JsonData.Address = Address;
		if (PhoneNumber != null) e.JsonData.PhoneNumber = PhoneNumber;
		if (Code != null) e.Code = Code;
		if (Order != null) e.Order = Order;
		if (ParentId != null) e.ParentId = ParentId;
		if (RelatedProducts != null) e.JsonData.RelatedProducts = RelatedProducts;
		if (Tags != null) e.Tags = Tags;
		return e;
	}
}

public sealed class CategoryReadParams : BaseReadParams<TagCategory> {
	public bool ShowMedia { get; set; }
	public bool ShowChildren { get; set; }
	public bool ShowChildrenMedia { get; set; }
	public bool OrderByOrder { get; set; }
	public bool OrderByOrderDesc { get; set; }
}