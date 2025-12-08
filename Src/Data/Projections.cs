namespace SinaMN75U.Data;

public sealed class CategorySelectorArgs {
	public MediaSelectorArgs? Media { get; set; }
	public CategorySelectorArgs? Children { get; set; }
	public ProductSelectorArgs? Product { get; set; }
	public UserSelectorArgs? User { get; set; }
	public int ChildrenDebt { get; set; }
}

public sealed class MediaSelectorArgs {
	public MediaSelectorArgs? Children { get; set; }
}

public sealed class ContentSelectorArgs {
	public MediaSelectorArgs? Media { get; set; }
}

public sealed class UserSelectorArgs {
	public CategorySelectorArgs? Category { get; set; }
	public ContractSelectorArgs? Contract { get; set; }
	public MediaSelectorArgs? Media { get; set; }
	public InvoiceSelectorArgs? Invoice { get; set; }
}

public sealed class ProductSelectorArgs {
	public Guid? UserId { get; set; }
	public ProductSelectorArgs? Children { get; set; }
	public CategorySelectorArgs? Category { get; set; }
	public UserSelectorArgs? User { get; set; }
	public MediaSelectorArgs? Media { get; set; }
	public CommentSelectorArgs? Comment { get; set; }
	public bool ChildrenCount { get; set; }
	public bool CommentsCount { get; set; }
	public bool IsFollowing { get; set; }
}

public sealed class ContractSelectorArgs {
	public UserSelectorArgs? User { get; set; }
	public UserSelectorArgs? Creator { get; set; }
	public ProductSelectorArgs? Product { get; set; }
	public InvoiceSelectorArgs? Invoice { get; set; }
}

public sealed class InvoiceSelectorArgs {
	public ContractSelectorArgs? Contract { get; set; }
	public UserSelectorArgs? User { get; set; }
}

public sealed class CommentSelectorArgs {
	public CommentSelectorArgs? Children { get; set; }
	public UserSelectorArgs? TargetUser { get; set; }
	public UserSelectorArgs? User { get; set; }
	public ProductSelectorArgs? Product { get; set; }
	public MediaSelectorArgs? Media { get; set; }
}

public static class Projections {
	public static Expression<Func<MediaEntity, MediaResponse>> MediaSelector(MediaSelectorArgs args) =>
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
		Categories = args.Category == null ? null : x.Categories.AsQueryable().Select(CategorySelector(args.Category)).ToList(),
		Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector(args.Media)).ToList(),
		Contracts = args.Contract == null ? null : x.Contracts.AsQueryable().Select(ContractSelector(args.Contract)).ToList(),
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
		Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector(args.Media)).ToList(),
		Categories = args.Category == null ? null : x.Categories.AsQueryable().Select(CategorySelector(args.Category)).ToList(),
		Children = args.Children == null ? null : x.Children.AsQueryable().Select(ProductSelector(args.Children)).ToList(),
		CommentCount = args.CommentsCount ? x.Comments.Count : null,
		ChildrenCount = args.ChildrenCount ? x.Children.Count : null,
		IsFollowing = args.IsFollowing && args.UserId != null ? x.Followers.Any(f => f.UserId == args.UserId) : null,
		User = args.User == null
			? null
			: new UserResponse {
				Id = x.User.Id,
				JsonData = x.User.JsonData,
				Tags = x.User.Tags,
				UserName = x.User.UserName,
				PhoneNumber = x.User.PhoneNumber,
				Email = x.User.Email,
				FirstName = x.User.FirstName,
				LastName = x.User.LastName,
				Media = args.User.Media == null ? null : x.User.Media.AsQueryable().Select(MediaSelector(args.User.Media)).ToList(),
				Categories = args.User.Category == null ? null : x.User.Categories.AsQueryable().Select(CategorySelector(args.User.Category)).ToList(),
			}
	};

	public static Expression<Func<CategoryEntity, CategoryResponse>> CategorySelector(CategorySelectorArgs arg) {
		Expression<Func<CategoryEntity, CategoryResponse>>? childSelector = null;
		if (arg is { Children: not null, ChildrenDebt: > 0 and < 10 }) childSelector = CategorySelector(new CategorySelectorArgs{
			Media = arg.Media,
			Children = arg.Children,
			ChildrenDebt = arg.ChildrenDebt - 1,
		});
		return x => new CategoryResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			Order = x.Order,
			Code = x.Code,
			ParentId = x.ParentId,
			Media = arg.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector(arg.Media)).ToList(),
			Children = arg.Children != null && arg.ChildrenDebt > 0 ? x.Children.AsQueryable().Select(childSelector!).ToList() : null
		};
	}

	public static Expression<Func<ContentEntity, ContentResponse>> ContentSelector(ContentSelectorArgs args) => x => new ContentResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector(args.Media)).ToList()
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
		Media = args.Media == null ? null : x.Media.AsQueryable().Select(MediaSelector(args.Media)).ToList(),
		User = args.User == null ? null : x.User.MapToResponse(),
		TargetUser = args.TargetUser == null ? null : x.TargetUser!.MapToResponse(),
		Product = args.Product == null ? null : x.Product!.MapToResponse(),
		Children = args.Children == null ? null : x.Children.AsQueryable().Select(CommentSelector(args.Children)).ToList()
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
		Invoices = args.Invoice == null ? null : x.Invoices.AsQueryable().Select(InvoiceSelector(args.Invoice)).ToList(),
		User = args.User == null ? null : new UserResponse {
			Id = x.User.Id,
			JsonData = x.User.JsonData,
			Tags = x.User.Tags,
			UserName = x.User.UserName,
			PhoneNumber = x.User.PhoneNumber,
			Email = x.User.Email,
			FirstName = x.User.FirstName,
			LastName = x.User.LastName,
			Media = args.User.Media == null ? null : x.User.Media.AsQueryable().Select(MediaSelector(args.User.Media)).ToList(),
			Categories = args.User.Category == null ? null : x.User.Categories.AsQueryable().Select(CategorySelector(args.User.Category)).ToList(),
		},
		Creator = args.Creator == null ? null : new UserResponse {
			Id = x.Creator.Id,
			JsonData = x.Creator.JsonData,
			Tags = x.Creator.Tags,
			UserName = x.Creator.UserName,
			PhoneNumber = x.Creator.PhoneNumber,
			Email = x.Creator.Email,
			FirstName = x.Creator.FirstName,
			LastName = x.Creator.LastName,
			Media = args.Creator.Media == null ? null : x.Creator.Media.AsQueryable().Select(MediaSelector(args.Creator.Media)).ToList(),
			Categories = args.Creator.Category == null ? null : x.Creator.Categories.AsQueryable().Select(CategorySelector(args.Creator.Category)).ToList(),
		},
		Product = args.Product == null ? null : new ProductResponse {
			Id = x.Product.Id,
			JsonData = x.Product.JsonData,
			Tags = x.Product.Tags,
			Title = x.Product.Title,
			Code = x.Product.Code,
			Subtitle = x.Product.Subtitle,
			Description = x.Product.Description,
			Slug = x.Product.Slug,
			Type = x.Product.Type,
			Content = x.Product.Content,
			Latitude = x.Product.Latitude,
			Longitude = x.Product.Longitude,
			Deposit = x.Product.Deposit,
			Rent = x.Product.Rent,
			Stock = x.Product.Stock,
			Point = x.Product.Point,
			Order = x.Product.Order,
			UserId = x.Product.UserId,
			Categories = args.Product.Category == null ? null : x.Product.Categories.AsQueryable().Select(CategorySelector(args.Product.Category)).ToList(),
			Media = args.Product.Media == null ? null : x.Product.Media.AsQueryable().Select(MediaSelector(args.Product.Media)).ToList(),
		},
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
		User = args.User == null
			? null
			: new UserResponse {
				Id = x.User.Id,
				JsonData = x.User.JsonData,
				Tags = x.User.Tags,
				UserName = x.User.UserName,
				PhoneNumber = x.User.PhoneNumber,
				Email = x.User.Email,
				FirstName = x.User.FirstName,
				LastName = x.User.LastName,
				Categories = args.User.Category == null ? null : x.User.Categories.AsQueryable().Select(CategorySelector(args.User.Category)).ToList(),
				Media = args.User.Media == null ? null : x.User.Media.AsQueryable().Select(MediaSelector(args.User.Media)).ToList()
			},
		Contract = args.Contract == null
			? null
			: new ContractResponse {
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
				Creator = args.Contract.Creator == null
					? null
					: new UserResponse {
						Id = x.Contract.Creator.Id,
						JsonData = x.Contract.Creator.JsonData,
						Tags = x.Contract.Creator.Tags,
						UserName = x.Contract.Creator.UserName,
						PhoneNumber = x.Contract.Creator.PhoneNumber,
						Email = x.Contract.Creator.Email,
						FirstName = x.Contract.Creator.FirstName,
						LastName = x.Contract.Creator.LastName,
						Categories = args.Contract.Creator.Category == null ? null : x.Contract.Creator.Categories.AsQueryable().Select(CategorySelector(args.Contract.Creator.Category)).ToList(),
						Media = args.Contract.Creator.Media == null ? null : x.Contract.Creator.Media.AsQueryable().Select(MediaSelector(args.Contract.Creator.Media)).ToList()
					},
				User = args.Contract.User == null
					? null
					: new UserResponse {
						Id = x.Contract.User.Id,
						JsonData = x.Contract.User.JsonData,
						Tags = x.Contract.User.Tags,
						UserName = x.Contract.User.UserName,
						PhoneNumber = x.Contract.User.PhoneNumber,
						Email = x.Contract.User.Email,
						FirstName = x.Contract.User.FirstName,
						LastName = x.Contract.User.LastName,
						Categories = args.Contract.User.Category == null ? null : x.Contract.User.Categories.AsQueryable().Select(CategorySelector(args.Contract.User.Category)).ToList(),
						Media = args.Contract.User.Media == null ? null : x.Contract.User.Media.AsQueryable().Select(MediaSelector(args.Contract.User.Media)).ToList()
					},
			}
	};
}