namespace SinaMN75U.Data.Params;

public sealed class ProductCreateParams : BaseCreateParams<TagProduct> {
	[UValidationRequired("TitleRequired")]
	public required string Title { get; set; }

	public string? Code { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? ActionType { get; set; }
	public string? ActionTitle { get; set; }
	public string? ActionUri { get; set; }
	public string? Slug { get; set; }
	public string? Type { get; set; }
	public string? Content { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int? Stock { get; set; }
	public int? Point { get; set; }
	public int? Order { get; set; }
	public double? Deposit { get; set; }
	public double? Rent { get; set; }

	public string? Details { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Address { get; set; }

	public IEnumerable<Guid>? Categories { get; set; }
	public IEnumerable<Guid>? RelatedProducts { get; set; }
	public ICollection<ProductCreateParams>? Children { get; set; }

	public Guid? ParentId { get; set; }
	public Guid? UserId { get; set; }

	public ICollection<Guid>? Media { get; set; }
	
	public ProductEntity MapToEntity() => new() {
		Title = Title,
		Code = Code,
		Subtitle = Subtitle,
		Description = Description,
		Slug = Slug,
		Type = Type,
		Content = Content,
		Latitude = Latitude,
		Longitude = Longitude,
		Stock = Stock ?? 0,
		Point = Point ?? 0,
		Order = Order ?? 0,
		Deposit = Deposit,
		Rent = Rent,
		ParentId = ParentId,
		UserId = UserId ?? Guid.Empty,
		JsonData = new ProductJson {
			ActionType = ActionType,
			ActionTitle = ActionTitle,
			ActionUri = ActionUri,
			Details = Details,
			PhoneNumber = PhoneNumber,
			Address = Address,
			RelatedProducts = RelatedProducts?.ToList() ?? []
		},
		Tags = Tags
	};

}

public sealed class ProductUpdateParams : BaseUpdateParams<TagProduct> {
	public string? Title { get; set; }
	public string? Code { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Slug { get; set; }
	public string? Type { get; set; }
	public string? Content { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int? Stock { get; set; }
	public int? Point { get; set; }
	public int? Order { get; set; }
	public double? Deposit { get; set; }
	public double? Rent { get; set; }
	public Guid? ParentId { get; set; }
	public Guid? UserId { get; set; }
	public string? ActionType { get; set; }
	public string? ActionTitle { get; set; }
	public string? ActionUri { get; set; }
	public string? Details { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Address { get; set; }
	public ICollection<Guid>? RelatedProducts { get; set; }
	public IEnumerable<Guid>? AddRelatedProducts { get; set; }
	public IEnumerable<Guid>? RemoveRelatedProducts { get; set; }
	public IEnumerable<Guid>? AddCategories { get; set; }
	public IEnumerable<Guid>? RemoveCategories { get; set; }
	public ICollection<Guid>? Categories { get; set; }
	
	public ICollection<Guid>? Media { get; set; }

	public bool UpdateInvoicesPrices { get; set; } = false;
	
	public void MapToEntity(ProductEntity e) {
		if (Title != null) e.Title = Title;
		if (Code != null) e.Code = Code;
		if (Subtitle != null) e.Subtitle = Subtitle;
		if (Description != null) e.Description = Description;
		if (Slug != null) e.Slug = Slug;
		if (Type != null) e.Type = Type;
		if (Content != null) e.Content = Content;
		if (Latitude.HasValue) e.Latitude = Latitude;
		if (Longitude.HasValue) e.Longitude = Longitude;
		if (Stock.HasValue) e.Stock = Stock.Value;
		if (Point.HasValue) e.Point = Point.Value;
		if (Order.HasValue) e.Order = Order.Value;
		if (Deposit.HasValue) e.Deposit = Deposit.Value;
		if (Rent.HasValue) e.Rent = Rent.Value;
		if (ParentId.HasValue) e.ParentId = ParentId;
		if (UserId.HasValue) e.UserId = UserId.Value;

		if (ActionType != null) e.JsonData.ActionType = ActionType;
		if (ActionTitle != null) e.JsonData.ActionTitle = ActionTitle;
		if (ActionUri != null) e.JsonData.ActionUri = ActionUri;
		if (Details != null) e.JsonData.Details = Details;
		if (PhoneNumber != null) e.JsonData.PhoneNumber = PhoneNumber;
		if (Address != null) e.JsonData.Address = Address;

		if (Categories != null) e.Categories = [];

		if (RelatedProducts != null) e.JsonData.RelatedProducts = RelatedProducts.ToList();
		if (AddRelatedProducts != null) e.JsonData.RelatedProducts.AddRangeIfNotExist(AddRelatedProducts);
		if (RemoveRelatedProducts != null) e.JsonData.RelatedProducts?.RemoveRangeIfExist(RemoveRelatedProducts);

		if (Tags != null) e.Tags = Tags;
	}
}

public sealed class ProductReadParams : BaseReadParams<TagProduct> {
	public string? Query { get; set; }
	public string? Title { get; set; }
	public string? Code { get; set; }
	public string? Slug { get; set; }
	public Guid? ParentId { get; set; }
	public Guid? UserId { get; set; }
	public int? MinStock { get; set; }
	public int? MaxStock { get; set; }
	public double? MinDeposit { get; set; }
	public double? MaxRent { get; set; }
	public bool OrderByOrder { get; set; }
	public bool OrderByOrderDesc { get; set; }
	public bool? HasActiveContract { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
	public ProductSelectorArgs SelectorArgs { get; set; } = new();
}