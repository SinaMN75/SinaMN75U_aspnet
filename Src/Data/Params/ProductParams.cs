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
	public decimal? Latitude { get; set; }
	public decimal? Longitude { get; set; }
	public int? Stock { get; set; }
	public int? Point { get; set; }
	public int? Order { get; set; }
	public decimal? Deposit { get; set; }
	public decimal? Rent { get; set; }

	public string? Details { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Address { get; set; }

	public IEnumerable<Guid>? Categories { get; set; }
	public IEnumerable<Guid>? RelatedProducts { get; set; }
	public ICollection<ProductCreateParams>? Children { get; set; }

	public Guid? ParentId { get; set; }
	public Guid? CreatorId { get; set; }

	public ICollection<Guid>? Media { get; set; }

	public ProductEntity MapToEntity() {
		return new ProductEntity {
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
			CreatorId = CreatorId ?? Guid.Empty,
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
}

public sealed class ProductUpdateParams : BaseUpdateParams<TagProduct> {
	public string? Title { get; set; }
	public string? Code { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Slug { get; set; }
	public string? Type { get; set; }
	public string? Content { get; set; }
	public decimal? Latitude { get; set; }
	public decimal? Longitude { get; set; }
	public int? Stock { get; set; }
	public int? Point { get; set; }
	public int? Order { get; set; }
	public decimal? Deposit { get; set; }
	public decimal? Rent { get; set; }
	public Guid? ParentId { get; set; }
	public Guid? CreatorId { get; set; }
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

	public bool UpdateInvoicesPrices { get; set; }
}

public sealed class ProductReadParams : BaseReadParams<TagProduct> {
	public string? Query { get; set; }
	public string? Title { get; set; }
	public string? Code { get; set; }
	public string? Slug { get; set; }
	public Guid? ParentId { get; set; }
	public Guid? CreatorId { get; set; }
	public int? MinStock { get; set; }
	public int? MaxStock { get; set; }
	public decimal? MinDeposit { get; set; }
	public decimal? MaxRent { get; set; }
	public bool OrderByOrder { get; set; }
	public bool OrderByOrderDesc { get; set; }
	public bool? HasActiveContract { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
	public ProductSelectorArgs SelectorArgs { get; set; } = new();
}