namespace SinaMN75U.Data;

public class BaseSelectorsArgs {
	public SoftDeleteBehavior SoftDeleteBehavior { get; set; } = SoftDeleteBehavior.IgnoreDeleted;
}

public sealed class CategorySelectorArgs : BaseSelectorsArgs {
	public MediaSelectorArgs? Media { get; set; }
	public CategorySelectorArgs? Children { get; set; }
	public ProductSelectorArgs? Product { get; set; }
	public UserSelectorArgs? User { get; set; }
	public int ChildrenDebt { get; set; }
}

public sealed class MediaSelectorArgs : BaseSelectorsArgs {
	public MediaSelectorArgs? Children { get; set; }
}

public sealed class ContentSelectorArgs : BaseSelectorsArgs {
	public MediaSelectorArgs? Media { get; set; }
}

public sealed class TicketSelectorArgs : BaseSelectorsArgs {
	public MediaSelectorArgs? Media { get; set; }
	public UserSelectorArgs? User { get; set; }
}

public sealed class UserSelectorArgs : BaseSelectorsArgs {
	public CategorySelectorArgs? Category { get; set; }
	public ContractSelectorArgs? Contract { get; set; }
	public MediaSelectorArgs? Media { get; set; }
	public InvoiceSelectorArgs? Invoice { get; set; }
}

public sealed class ProductSelectorArgs : BaseSelectorsArgs {
	public Guid? UserId { get; set; }
	public ProductSelectorArgs? Children { get; set; }
	public CategorySelectorArgs? Category { get; set; }
	public UserSelectorArgs? User { get; set; }
	public MediaSelectorArgs? Media { get; set; }
	public CommentSelectorArgs? Comment { get; set; }
	public bool ChildrenCount { get; set; }
	public bool CommentsCount { get; set; }
	public bool IsFollowing { get; set; }
	public int ChildrenDebt { get; set; }
}

public sealed class ContractSelectorArgs : BaseSelectorsArgs {
	public UserSelectorArgs? User { get; set; }
	public UserSelectorArgs? Creator { get; set; }
	public ProductSelectorArgs? Product { get; set; }
	public InvoiceSelectorArgs? Invoice { get; set; }
}

public sealed class InvoiceSelectorArgs : BaseSelectorsArgs {
	public ContractSelectorArgs? Contract { get; set; }
}

public sealed class CommentSelectorArgs: BaseSelectorsArgs {
	public CommentSelectorArgs? Children { get; set; }
	public UserSelectorArgs? User { get; set; }
	public UserSelectorArgs? Creator { get; set; }
	public ProductSelectorArgs? Product { get; set; }
	public MediaSelectorArgs? Media { get; set; }
}

public sealed class TxnSelectorArgs : BaseSelectorsArgs {
	public UserSelectorArgs? User { get; set; }
	public InvoiceSelectorArgs? Invoice { get; set; }
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
		Categories = args.Category == null ? null : x.Categories.AsQueryable().SoftDeleteBehavior(args.Category.SoftDeleteBehavior).Select(CategorySelector(args.Category)).ToList(),
		Media = args.Media == null ? null : x.Media.AsQueryable().SoftDeleteBehavior(args.Media.SoftDeleteBehavior).Select(MediaSelector(args.Media)).ToList(),
		Contracts = args.Contract == null ? null : x.Contracts.AsQueryable().SoftDeleteBehavior(args.Contract.SoftDeleteBehavior).Select(ContractSelector(args.Contract)).ToList(),
	};

	public static Expression<Func<ProductEntity, ProductResponse>> ProductSelector(ProductSelectorArgs args) {
		Expression<Func<ProductEntity, ProductResponse>>? childSelector = null;
		if (args is { Children: not null, ChildrenDebt: > 0 and < 10 })
			childSelector = ProductSelector(new ProductSelectorArgs {
				UserId = args.UserId,
				Media = args.Media,
				Comment = args.Comment,
				ChildrenCount = args.ChildrenCount,
				CommentsCount = args.CommentsCount,
				IsFollowing = args.IsFollowing,
				Children = args.Children,
				Category = args.Category,
				User = args.User,
				ChildrenDebt = args.ChildrenDebt - 1,
				SoftDeleteBehavior = args.SoftDeleteBehavior
			});
		return x => new ProductResponse {
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
			Media = args.Media == null ? null : x.Media.AsQueryable().SoftDeleteBehavior(args.Media.SoftDeleteBehavior).Select(MediaSelector(args.Media)).ToList(),
			Categories = args.Category == null ? null : x.Categories.AsQueryable().SoftDeleteBehavior(args.Category.SoftDeleteBehavior).Select(CategorySelector(args.Category)).ToList(),
			Children = args.Children != null && args.ChildrenDebt > 0 ? x.Children.AsQueryable().SoftDeleteBehavior(args.Children.SoftDeleteBehavior).Select(childSelector!).ToList() : null,
			CommentCount = args.CommentsCount ? x.Comments.Count : null,
			ChildrenCount = args.ChildrenCount ? x.Children.Count : null,
			IsFollowing = args.IsFollowing && args.UserId != null ? x.Followers.Any(f => f.CreatorId == args.UserId) : null,
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
					Media = args.User.Media == null ? null : x.User.Media.AsQueryable().SoftDeleteBehavior(args.User.Media.SoftDeleteBehavior).Select(MediaSelector(args.User.Media)).ToList(),
					Categories = args.User.Category == null ? null : x.User.Categories.AsQueryable().SoftDeleteBehavior(args.User.Category.SoftDeleteBehavior).Select(CategorySelector(args.User.Category)).ToList(),
				}
		};
	}

	public static Expression<Func<CategoryEntity, CategoryResponse>> CategorySelector(CategorySelectorArgs args) {
		Expression<Func<CategoryEntity, CategoryResponse>>? childSelector = null;
		if (args is { Children: not null, ChildrenDebt: > 0 and < 10 })
			childSelector = CategorySelector(new CategorySelectorArgs {
					Media = args.Media,
					Children = args.Children,
					ChildrenDebt = args.ChildrenDebt - 1,
					Product = args.Product,
					User = args.User,
					SoftDeleteBehavior = args.SoftDeleteBehavior
				}
			);
		return x => new CategoryResponse {
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			Order = x.Order,
			Code = x.Code,
			ParentId = x.ParentId,
			Users = args.User == null ? null : x.Users.AsQueryable().SoftDeleteBehavior(args.SoftDeleteBehavior).Select(UserSelector(args.User)).ToList(),
			Products = args.Product == null ? null : x.Products.AsQueryable().SoftDeleteBehavior(args.SoftDeleteBehavior).Select(ProductSelector(args.Product)).ToList(),
			Media = args.Media == null ? null : x.Media.AsQueryable().SoftDeleteBehavior(args.SoftDeleteBehavior).Select(MediaSelector(args.Media)).ToList(),
			Children = args.Children != null && args.ChildrenDebt > 0 ? x.Children.AsQueryable().SoftDeleteBehavior(args.Children.SoftDeleteBehavior).Select(childSelector!).ToList() : null
		};
	}

	public static Expression<Func<ContentEntity, ContentResponse>> ContentSelector(ContentSelectorArgs args) => x => new ContentResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		Media = args.Media == null ? null : x.Media.AsQueryable().SoftDeleteBehavior(args.Media.SoftDeleteBehavior).Select(MediaSelector(args.Media)).ToList()
	};

	public static Expression<Func<TxnEntity, TxnResponse>> TxnSelector(TxnSelectorArgs args) => x => new TxnResponse {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		UpdatedAt = x.UpdatedAt,
		DeletedAt = x.DeletedAt,
		Tags = x.Tags,
		Amount = x.Amount,
		TrackingNumber = x.TrackingNumber,
		JsonData = x.JsonData,
		UserId = x.UserId,
		InvoiceId = x.InvoiceId,
		Invoice = args.Invoice == null
			? null
			: new InvoiceResponse {
				DebtAmount = x.Invoice.DebtAmount,
				CreditorAmount = x.Invoice.CreditorAmount,
				PaidAmount = x.Invoice.PaidAmount,
				PenaltyAmount = x.Invoice.PenaltyAmount,
				DueDate = x.Invoice.DueDate,
				JsonData = x.Invoice.JsonData,
				Tags = x.Invoice.Tags,
				Contract = args.Invoice.Contract == null
					? null
					: new ContractResponse {
						Id = x.Invoice.Contract.Id,
						JsonData = x.Invoice.Contract.JsonData,
						Tags = x.Invoice.Contract.Tags,
						StartDate = x.Invoice.Contract.StartDate,
						EndDate = x.Invoice.Contract.EndDate,
						Deposit = x.Invoice.Contract.Deposit,
						Rent = x.Invoice.Contract.Rent,
						UserId = x.Invoice.Contract.UserId,
						CreatorId = x.Invoice.Contract.CreatorId,
						ProductId = x.Invoice.Contract.ProductId,
					}
			},
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
				Categories = args.User.Category == null ? null : x.User.Categories.AsQueryable().SoftDeleteBehavior(args.User.Category.SoftDeleteBehavior).Select(CategorySelector(args.User.Category)).ToList(),
				Media = args.User.Media == null ? null : x.User.Media.AsQueryable().SoftDeleteBehavior(args.User.Media.SoftDeleteBehavior).Select(MediaSelector(args.User.Media)).ToList()
			},
	};

	public static Expression<Func<TicketEntity, TicketResponse>> TicketSelector(TicketSelectorArgs args) => x => new TicketResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		UserId = x.UserId,
		Media = args.Media == null ? null : x.Media.AsQueryable().SoftDeleteBehavior(args.Media.SoftDeleteBehavior).Select(MediaSelector(args.Media)).ToList(),
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
				Categories = args.User.Category == null ? null : x.User.Categories.AsQueryable().SoftDeleteBehavior(args.User.Category.SoftDeleteBehavior).Select(CategorySelector(args.User.Category)).ToList(),
				Media = args.User.Media == null ? null : x.User.Media.AsQueryable().SoftDeleteBehavior(args.User.Media.SoftDeleteBehavior).Select(MediaSelector(args.User.Media)).ToList()
			},
	};

	public static Expression<Func<CommentEntity, CommentResponse>> CommentSelector(CommentSelectorArgs args) => x => new CommentResponse {
		Id = x.Id,
		Tags = x.Tags,
		JsonData = x.JsonData,
		CreatorId = x.CreatorId,
		UserId = x.UserId,
		ProductId = x.ProductId,
		ParentId = x.ParentId,
		Score = x.Score,
		Description = x.Description,
		Media = args.Media == null ? null : x.Media.AsQueryable().SoftDeleteBehavior(args.Media.SoftDeleteBehavior).Select(MediaSelector(args.Media)).ToList(),
		Creator = args.Creator == null ? null : x.Creator.MapToResponse(),
		User = args.User == null ? null : x.User!.MapToResponse(),
		Product = args.Product == null ? null : x.Product!.MapToResponse(),
		Children = args.Children == null ? null : x.Children.AsQueryable().SoftDeleteBehavior(args.Children.SoftDeleteBehavior).Select(CommentSelector(args.Children)).ToList()
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
		Invoices = args.Invoice == null ? null : x.Invoices.AsQueryable().SoftDeleteBehavior(args.Invoice.SoftDeleteBehavior).Select(InvoiceSelector(args.Invoice)).ToList(),
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
				Media = args.User.Media == null ? null : x.User.Media.AsQueryable().SoftDeleteBehavior(args.User.Media.SoftDeleteBehavior).Select(MediaSelector(args.User.Media)).ToList(),
				Categories = args.User.Category == null ? null : x.User.Categories.AsQueryable().SoftDeleteBehavior(args.User.Category.SoftDeleteBehavior).Select(CategorySelector(args.User.Category)).ToList(),
			},
		Creator = args.Creator == null
			? null
			: new UserResponse {
				Id = x.Creator.Id,
				JsonData = x.Creator.JsonData,
				Tags = x.Creator.Tags,
				UserName = x.Creator.UserName,
				PhoneNumber = x.Creator.PhoneNumber,
				Email = x.Creator.Email,
				FirstName = x.Creator.FirstName,
				LastName = x.Creator.LastName,
				Media = args.Creator.Media == null ? null : x.Creator.Media.AsQueryable().SoftDeleteBehavior(args.Creator.Media.SoftDeleteBehavior).Select(MediaSelector(args.Creator.Media)).ToList(),
				Categories = args.Creator.Category == null ? null : x.Creator.Categories.AsQueryable().SoftDeleteBehavior(args.Creator.Category.SoftDeleteBehavior).Select(CategorySelector(args.Creator.Category)).ToList(),
			},
		Product = args.Product == null
			? null
			: new ProductResponse {
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
				Categories = args.Product.Category == null ? null : x.Product.Categories.AsQueryable().SoftDeleteBehavior(args.Product.Category.SoftDeleteBehavior).Select(CategorySelector(args.Product.Category)).ToList(),
				Media = args.Product.Media == null ? null : x.Product.Media.AsQueryable().SoftDeleteBehavior(args.Product.Media.SoftDeleteBehavior).Select(MediaSelector(args.Product.Media)).ToList(),
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
						Categories = args.Contract.Creator.Category == null ? null : x.Contract.Creator.Categories.AsQueryable().SoftDeleteBehavior(args.Contract.Creator.Category.SoftDeleteBehavior).Select(CategorySelector(args.Contract.Creator.Category)).ToList(),
						Media = args.Contract.Creator.Media == null ? null : x.Contract.Creator.Media.AsQueryable().SoftDeleteBehavior(args.Contract.Creator.Media.SoftDeleteBehavior).Select(MediaSelector(args.Contract.Creator.Media)).ToList()
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
						Categories = args.Contract.User.Category == null ? null : x.Contract.User.Categories.AsQueryable().SoftDeleteBehavior(args.Contract.User.Category.SoftDeleteBehavior).Select(CategorySelector(args.Contract.User.Category)).ToList(),
						Media = args.Contract.User.Media == null ? null : x.Contract.User.Media.AsQueryable().SoftDeleteBehavior(args.Contract.User.Media.SoftDeleteBehavior).Select(MediaSelector(args.Contract.User.Media)).ToList()
					},
				Product = args.Contract.Product == null
					? null
					: new ProductResponse {
						Id = x.Contract.Product.Id,
						JsonData = x.Contract.Product.JsonData,
						Tags = x.Contract.Product.Tags,
						Title = x.Contract.Product.Title,
						Code = x.Contract.Product.Code,
						Deposit = x.Contract.Product.Deposit,
						Rent = x.Contract.Product.Rent,
						UserId = x.Contract.Product.UserId,
						Categories = args.Contract.Product.Category == null ? null : x.Contract.Product.Categories.AsQueryable().SoftDeleteBehavior(args.Contract.Product.Category.SoftDeleteBehavior).Select(CategorySelector(args.Contract.Product.Category)).ToList(),
						Media = args.Contract.Product.Media == null ? null : x.Contract.Product.Media.AsQueryable().SoftDeleteBehavior(args.Contract.Product.Media.SoftDeleteBehavior).Select(MediaSelector(args.Contract.Product.Media)).ToList(),
					},
			}
	};
}