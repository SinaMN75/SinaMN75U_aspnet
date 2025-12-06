namespace SinaMN75U.Data;

public sealed class CategorySelectorArgs {
	public bool ShowMedia { get; set; }
	public bool ShowChildren { get; set; }
	public bool ShowChildrenMedia { get; set; }
}

public sealed class UserSelectorArgs {
	public CategorySelectorArgs CategorySelectorArgs { get; set; } = new();
	public bool ShowCategories { get; set; }
	public bool ShowMedia { get; set; }
}

public sealed class ProductSelectorArgs {
	public ProductSelectorArgs? ChildrenSelectorArgs { get; set; }
	public CategorySelectorArgs CategorySelectorArgs { get; set; } = new();
	public UserSelectorArgs UserSelectorArgs { get; set; } = new();
	public bool ShowCategories { get; set; }
	public bool ShowMedia { get; set; }
	public bool ShowUser { get; set; }
	public bool ShowChildren { get; set; }
}

public static class Projections {
	public static Expression<Func<MediaEntity, MediaResponse>> MediaSelector() =>
		x => new MediaResponse {
			Tags = x.Tags,
			JsonData = x.JsonData,
			Path = x.Path
		};

	public static Expression<Func<UserEntity, UserResponse>> UserSelector(UserSelectorArgs args) => x => new UserResponse {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		UpdatedAt = x.UpdatedAt,
		DeletedAt = x.DeletedAt,
		Tags = x.Tags,
		JsonData = x.JsonData,
		UserName = x.UserName,
		PhoneNumber = x.PhoneNumber,
		Email = x.Email,
		FirstName = x.FirstName,
		LastName = x.LastName,
		Bio = x.Bio,
		Country = x.Country,
		State = x.State,
		City = x.City,
		Birthdate = x.Birthdate,
		Categories = args.ShowCategories ? x.Categories.AsQueryable().Select(CategoryMinSelector(args.CategorySelectorArgs)).ToList() : null,
		Media = args.ShowMedia ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null
	};

	public static Expression<Func<ProductEntity, ProductResponse>> ProductSelector(ProductSelectorArgs args) => x => new ProductResponse {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		UpdatedAt = x.UpdatedAt,
		DeletedAt = x.DeletedAt,
		Tags = x.Tags,
		JsonData = x.JsonData,
		Title = x.Title,
		Code = x.Code,
		Subtitle = x.Subtitle,
		Description = x.Description,
		Slug = x.Slug,
		Type = x.Type,
		Content = x.Content,
		Latitude = x.Latitude,
		Longitude = x.Longitude,
		Deposit = x.Deposit,
		Rent = x.Rent,
		Stock = x.Stock,
		Point = x.Point,
		Order = x.Order,
		ParentId = x.ParentId,
		UserId = x.UserId,
		User = args.ShowUser ? x.User.MapToResponse() : null,
		Categories = args.ShowCategories ? x.Categories.AsQueryable().Select(CategorySelector()).ToList() : null,
		Children = args.ShowChildren ? x.Children.AsQueryable().Select(ProductSelector(args.ChildrenSelectorArgs ?? new ProductSelectorArgs())).ToList() : null,
		Media = args.ShowMedia ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null
	};

	public static Expression<Func<CategoryEntity, CategoryResponse>> CategorySelector(
		bool media = false,
		bool parent = false,
		bool children = false,
		bool childrenMedia = false
	) => x => new CategoryResponse {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		UpdatedAt = x.UpdatedAt,
		DeletedAt = x.DeletedAt,
		Tags = x.Tags,
		JsonData = x.JsonData,
		Title = x.Title,
		Order = x.Order,
		Code = x.Code,
		ParentId = x.ParentId,
		Media = media ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null,
		Parent = parent ? x.Parent!.MapToResponse() : null,
		Children = children ? x.Children.AsQueryable().Select(CategorySelector(media: childrenMedia)).ToList() : null
	};

	public static Expression<Func<CategoryEntity, CategoryResponse>> CategoryMinSelector(CategorySelectorArgs args) => x => new CategoryResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		Title = x.Title,
		ParentId = x.ParentId,
		Media = args.ShowMedia ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null,
		Children = args.ShowChildren ? x.Children.AsQueryable().Select(CategorySelector(media: args.ShowChildrenMedia)).ToList() : null
	};

	public static Expression<Func<ContentEntity, ContentResponse>> ContentSelector(
		bool media = false
	) => x => new ContentResponse {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		UpdatedAt = x.UpdatedAt,
		DeletedAt = x.DeletedAt,
		Tags = x.Tags,
		JsonData = x.JsonData,
		Media = media ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null
	};

	public static Expression<Func<CommentEntity, CommentResponse>> CommentSelector(
		bool user = false,
		bool targetUser = false,
		bool product = false,
		bool media = false,
		bool children = false
	) => x => new CommentResponse {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		UpdatedAt = x.UpdatedAt,
		DeletedAt = x.DeletedAt,
		Tags = x.Tags,
		JsonData = x.JsonData,
		UserId = x.UserId,
		TargetUserId = x.TargetUserId,
		ProductId = x.ProductId,
		ParentId = x.ParentId,
		Score = x.Score,
		Description = x.Description,
		User = user ? x.User.MapToResponse() : null,
		TargetUser = targetUser ? x.TargetUser!.MapToResponse() : null,
		Product = product ? x.Product!.MapToResponse() : null,
		Media = media ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null,
		Children = children ? x.Children.AsQueryable().Select(CommentSelector()).ToList() : null
	};

	public static Expression<Func<ContractEntity, ContractResponse>> ContractSelector(
		bool user = false,
		bool creator = false,
		bool product = false,
		bool invoices = false
	) => x => new ContractResponse {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		UpdatedAt = x.UpdatedAt,
		DeletedAt = x.DeletedAt,
		Tags = x.Tags,
		JsonData = x.JsonData,
		StartDate = x.StartDate,
		EndDate = x.EndDate,
		Deposit = x.Deposit,
		Rent = x.Rent,
		UserId = x.UserId,
		CreatorId = x.CreatorId,
		ProductId = x.ProductId,
		User = user ? x.User.MapToResponse() : null,
		Creator = creator ? x.Creator.MapToResponse() : null,
		Product = product ? x.Product.MapToResponse() : null,
		Invoices = invoices ? x.Invoices.AsQueryable().Select(InvoiceSelector()).ToList() : null
	};

	public static Expression<Func<InvoiceEntity, InvoiceResponse>> InvoiceSelector(
		bool user = false,
		bool contracts = false
	) => x => new InvoiceResponse {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		UpdatedAt = x.UpdatedAt,
		DeletedAt = x.DeletedAt,
		Tags = x.Tags,
		JsonData = x.JsonData,
		DebtAmount = x.DebtAmount,
		CreditorAmount = x.CreditorAmount,
		PaidAmount = x.PaidAmount,
		PenaltyAmount = x.PenaltyAmount,
		DueDate = x.DueDate,
		PaidDate = x.PaidDate,
		TrackingNumber = x.TrackingNumber,
		User = user ? x.User.MapToResponse() : null,
		Contract = contracts ? x.Contract.MapToResponse() : null
	};
}