namespace SinaMN75U.Data;

public sealed class CategorySelectorArgs {
	public CategorySelectorArgs? ChildrenSelectorArgs { get; set; }
	public bool Media { get; set; }
	public bool Children { get; set; }
	public bool ChildrenMedia { get; set; }
	public int ChildrenDebt { get; set; } = 0;
}

public sealed class ContentSelectorArgs {
	public bool Media { get; set; }
}

public sealed class UserSelectorArgs {
	public CategorySelectorArgs CategorySelectorArgs { get; set; } = new();
	public bool Categories { get; set; }
	public bool Media { get; set; }
}

public sealed class ProductSelectorArgs {
	public Guid? UserId { get; set; }
	public ProductSelectorArgs? ChildrenSelectorArgs { get; set; }
	public CategorySelectorArgs CategorySelectorArgs { get; set; } = new();
	public UserSelectorArgs UserSelectorArgs { get; set; } = new();
	public bool Categories { get; set; }
	public bool Media { get; set; }
	public bool User { get; set; }
	public bool Children { get; set; }
	public bool ChildrenCount { get; set; }
	public bool CommentsCount { get; set; }
	public bool IsFollowing { get; set; }
}

public sealed class ContractSelectorArgs {
	public bool Invoices { get; set; }
	public bool Product { get; set; }
	public bool User { get; set; }
	public bool Creator { get; set; }
}

public sealed class InvoiceSelectorArgs {
	public UserSelectorArgs UserSelectorArgs { get; set; } = new();
	public bool User { get; set; }
	public bool Creator { get; set; }
	public bool Contract { get; set; }
}

public sealed class CommentSelectorArgs {
	public CommentSelectorArgs? ChildrenSelectorArgs { get; set; }
	public bool Media { get; set; }
	public bool User { get; set; }
	public bool TargetUser { get; set; }
	public bool Product { get; set; }
	public bool Children { get; set; }
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
		Categories = args.Categories ? x.Categories.AsQueryable().Select(CategorySelector(args.CategorySelectorArgs)).ToList() : null,
		Media = args.Media ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null
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
		Categories = args.Categories ? x.Categories.AsQueryable().Select(CategorySelector(args.CategorySelectorArgs)).ToList() : null,
		Children = args.Children ? x.Children.AsQueryable().Select(ProductSelector(args.ChildrenSelectorArgs ?? new ProductSelectorArgs())).ToList() : null,
		Media = args.Media ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null,
		CommentCount = args.CommentsCount ? x.Comments.Count : null,
		ChildrenCount = args.ChildrenCount ? x.Children.Count : null,
		IsFollowing = args.IsFollowing && args.UserId != null ? x.Followers.Any(f => f.UserId == args.UserId) : null,
		User = args.User
			? new UserResponse {
				Id = x.User.Id,
				JsonData = x.User.JsonData,
				Tags = x.User.Tags,
				UserName = x.User.UserName,
				PhoneNumber = x.User.PhoneNumber,
				Email = x.User.Email,
				FirstName = x.User.FirstName,
				LastName = x.User.LastName,
				Categories = args.UserSelectorArgs.Categories ? x.User.Categories.AsQueryable().Select(CategorySelector(args.UserSelectorArgs.CategorySelectorArgs)).ToList() : null,
				Media = args.UserSelectorArgs.Media ? x.User.Media.AsQueryable().Select(MediaSelector()).ToList() : null
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
		Media = arg.Media ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null,
		Children = arg.Children ? x.Children.AsQueryable().Select(CategorySelector(arg.ChildrenSelectorArgs ?? new CategorySelectorArgs())).ToList() : null
	};

	public static Expression<Func<ContentEntity, ContentResponse>> ContentSelector(ContentSelectorArgs args) => x => new ContentResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		Media = args.Media ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null
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
		User = args.User ? x.User.MapToResponse() : null,
		TargetUser = args.TargetUser ? x.TargetUser!.MapToResponse() : null,
		Product = args.Product ? x.Product!.MapToResponse() : null,
		Media = args.Media ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null,
		Children = args.Children ? x.Children.AsQueryable().Select(CommentSelector(args.ChildrenSelectorArgs ?? new CommentSelectorArgs())).ToList() : null
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
		User = args.User ? x.User.MapToResponse() : null,
		Creator = args.Creator ? x.Creator.MapToResponse() : null,
		Product = args.Product ? x.Product.MapToResponse() : null,
		Invoices = args.Invoices ? x.Invoices.AsQueryable().Select(InvoiceSelector(new InvoiceSelectorArgs())).ToList() : null
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
		User = args.User
			? new UserResponse {
				Id = x.User.Id,
				JsonData = x.User.JsonData,
				Tags = x.User.Tags,
				UserName = x.User.UserName,
				PhoneNumber = x.User.PhoneNumber,
				Email = x.User.Email,
				FirstName = x.User.FirstName,
				LastName = x.User.LastName,
				Categories = args.UserSelectorArgs.Categories ? x.User.Categories.AsQueryable().Select(CategorySelector(args.UserSelectorArgs.CategorySelectorArgs)).ToList() : null,
				Media = args.UserSelectorArgs.Media ? x.User.Media.AsQueryable().Select(MediaSelector()).ToList() : null
			}
			: null,
		Contract = args.Contract
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