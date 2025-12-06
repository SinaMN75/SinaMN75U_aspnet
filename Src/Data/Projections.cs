namespace SinaMN75U.Data;

public sealed class CategorySelectorArgs {
	public CategorySelectorArgs? ChildrenSelectorArgs { get; set; }
	public bool ShowMedia { get; set; }
	public bool ShowChildren { get; set; }
	public bool ShowChildrenMedia { get; set; }
}

public sealed class ContentSelectorArgs {
	public bool ShowMedia { get; set; }
}

public sealed class UserSelectorArgs {
	public CategorySelectorArgs CategorySelectorArgs { get; set; } = new();
	public bool ShowCategories { get; set; }
	public bool ShowMedia { get; set; }
}

public sealed class ProductSelectorArgs {
	public JwtClaimData? UserData { get; set; }
	public ProductSelectorArgs? ChildrenSelectorArgs { get; set; }
	public CategorySelectorArgs CategorySelectorArgs { get; set; } = new();
	public UserSelectorArgs UserSelectorArgs { get; set; } = new();
	public bool ShowCategories { get; set; }
	public bool ShowMedia { get; set; }
	public bool ShowUser { get; set; }
	public bool ShowChildren { get; set; }
	public bool ShowChildrenCount { get; set; }
	public bool ShowCommentsCount { get; set; }
	public bool ShowIsFollowing { get; set; }
}

public sealed class ContractSelectorArgs {
	public bool ShowInvoices { get; set; }
	public bool ShowProduct { get; set; }
	public bool ShowUser { get; set; }
	public bool ShowCreator { get; set; }
}

public sealed class InvoiceSelectorArgs {
	public UserSelectorArgs UserSelectorArgs { get; set; } = new();
	public bool ShowUser { get; set; }
	public bool ShowCreator { get; set; }
	public bool ShowContract { get; set; }
}

public sealed class CommentSelectorArgs {
	public CommentSelectorArgs? ChildrenSelectorArgs { get; set; }
	public bool ShowMedia { get; set; }
	public bool ShowUser { get; set; }
	public bool ShowTargetUser { get; set; }
	public bool ShowProduct { get; set; }
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
		Categories = args.ShowCategories ? x.Categories.AsQueryable().Select(CategorySelector(args.CategorySelectorArgs)).ToList() : null,
		Children = args.ShowChildren ? x.Children.AsQueryable().Select(ProductSelector(args.ChildrenSelectorArgs ?? new ProductSelectorArgs())).ToList() : null,
		Media = args.ShowMedia ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null,
		CommentCount = args.ShowCommentsCount ? x.Comments.Count : null,
		ChildrenCount = args.ShowChildrenCount ? x.Children.Count : null,
		IsFollowing = args.ShowIsFollowing && args.UserData != null ? x.Followers.Any(f => f.UserId == args.UserData.Id) : null,
		User = args.ShowUser
			? new UserResponse {
				Id = x.User.Id,
				JsonData = x.User.JsonData,
				Tags = x.User.Tags,
				UserName = x.User.UserName,
				PhoneNumber = x.User.PhoneNumber,
				Email = x.User.Email,
				FirstName = x.User.FirstName,
				LastName = x.User.LastName,
				Categories = args.UserSelectorArgs.ShowCategories ? x.User.Categories.AsQueryable().Select(CategoryMinSelector(args.UserSelectorArgs.CategorySelectorArgs)).ToList() : null,
				Media = args.UserSelectorArgs.ShowMedia ? x.User.Media.AsQueryable().Select(MediaSelector()).ToList() : null
			}
			: null
	};

	public static Expression<Func<CategoryEntity, CategoryResponse>> CategorySelector(CategorySelectorArgs arg) => x => new CategoryResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		Title = x.Title,
		Order = x.Order,
		Code = x.Code,
		ParentId = x.ParentId,
		Media = arg.ShowMedia ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null,
		Children = arg.ShowChildren ? x.Children.AsQueryable().Select(CategorySelector(arg.ChildrenSelectorArgs ?? new CategorySelectorArgs())).ToList() : null
	};

	public static Expression<Func<CategoryEntity, CategoryResponse>> CategoryMinSelector(CategorySelectorArgs args) => x => new CategoryResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		Title = x.Title,
		ParentId = x.ParentId,
		Media = args.ShowMedia ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null,
		Children = args.ShowChildren ? x.Children.AsQueryable().Select(CategorySelector(args.ChildrenSelectorArgs ?? new CategorySelectorArgs())).ToList() : null
	};

	public static Expression<Func<ContentEntity, ContentResponse>> ContentSelector(ContentSelectorArgs args) => x => new ContentResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		Media = args.ShowMedia ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null
	};

	public static Expression<Func<CommentEntity, CommentResponse>> CommentSelector(CommentSelectorArgs args) => x => new CommentResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		UserId = x.UserId,
		TargetUserId = x.TargetUserId,
		ProductId = x.ProductId,
		ParentId = x.ParentId,
		Score = x.Score,
		Description = x.Description,
		User = args.ShowUser ? x.User.MapToResponse() : null,
		TargetUser = args.ShowTargetUser ? x.TargetUser!.MapToResponse() : null,
		Product = args.ShowProduct ? x.Product!.MapToResponse() : null,
		Media = args.ShowMedia ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null,
		Children = args.ShowChildren ? x.Children.AsQueryable().Select(CommentSelector(args.ChildrenSelectorArgs ?? new CommentSelectorArgs())).ToList() : null
	};

	public static Expression<Func<ContractEntity, ContractResponse>> ContractSelector(ContractSelectorArgs args) => x => new ContractResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		StartDate = x.StartDate,
		EndDate = x.EndDate,
		Deposit = x.Deposit,
		Rent = x.Rent,
		UserId = x.UserId,
		CreatorId = x.CreatorId,
		ProductId = x.ProductId,
		User = args.ShowUser ? x.User.MapToResponse() : null,
		Creator = args.ShowCreator ? x.Creator.MapToResponse() : null,
		Product = args.ShowProduct ? x.Product.MapToResponse() : null,
		Invoices = args.ShowInvoices ? x.Invoices.AsQueryable().Select(InvoiceSelector(new InvoiceSelectorArgs())).ToList() : null
	};

	public static Expression<Func<InvoiceEntity, InvoiceResponse>> InvoiceSelector(InvoiceSelectorArgs args) => x => new InvoiceResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		DebtAmount = x.DebtAmount,
		CreditorAmount = x.CreditorAmount,
		PaidAmount = x.PaidAmount,
		PenaltyAmount = x.PenaltyAmount,
		DueDate = x.DueDate,
		PaidDate = x.PaidDate,
		TrackingNumber = x.TrackingNumber,
		User = args.ShowUser
			? new UserResponse {
				Id = x.User.Id,
				JsonData = x.User.JsonData,
				Tags = x.User.Tags,
				UserName = x.User.UserName,
				PhoneNumber = x.User.PhoneNumber,
				Email = x.User.Email,
				FirstName = x.User.FirstName,
				LastName = x.User.LastName,
				Categories = args.UserSelectorArgs.ShowCategories ? x.User.Categories.AsQueryable().Select(CategoryMinSelector(args.UserSelectorArgs.CategorySelectorArgs)).ToList() : null,
				Media = args.UserSelectorArgs.ShowMedia ? x.User.Media.AsQueryable().Select(MediaSelector()).ToList() : null
			}
			: null,
		Contract = args.ShowContract
			? new ContractResponse {
				Id = x.Contract.Id,
				JsonData = x.Contract.JsonData,
				Tags = x.Contract.Tags,
				StartDate = x.Contract.StartDate,
				EndDate = x.Contract.EndDate,
				Deposit = x.Contract.Deposit,
				Rent = x.Contract.Rent,
				UserId = x.Contract.UserId,
				CreatorId = x.Contract.CreatorId,
				ProductId = x.Contract.ProductId,
			}
			: null
	};
}