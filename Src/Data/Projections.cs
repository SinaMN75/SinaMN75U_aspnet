namespace SinaMN75U.Data;

using System.Linq.Expressions;

public static class Projections {
	public static Expression<Func<MediaEntity, MediaResponse>> MediaSelector() => m => new MediaResponse {
		Tags = m.Tags,
		JsonData = m.JsonData,
		Path = m.Path
	};

	public static Expression<Func<UserEntity, UserResponse>> UserSelector(
		bool categories = false,
		bool media = false
	) =>
		x => new UserResponse {
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
			Categories = categories ? x.Categories.Select(c => CategorySelector(false, false, false).Compile().Invoke(c)).ToList() : null,
			Media = media ? x.Media.Select(m => MediaSelector().Compile().Invoke(m)).ToList() : null
		};

	public static Expression<Func<ProductEntity, ProductResponse>> ProductSelector(
		bool user = false,
		bool categories = false,
		bool children = false
	) =>
		x => new ProductResponse {
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
			User = user ? UserSelector().Compile().Invoke(x.User) : null,
			Categories = categories ? x.Categories.Select(c => CategorySelector().Compile().Invoke(c)).ToList() : null,
			Children = children ? x.Children.Select(ch => ProductSelector().Compile().Invoke(ch)).ToList() : null
		};

	public static Expression<Func<CategoryEntity, CategoryResponse>> CategorySelector(
		bool media = false,
		bool parent = false,
		bool children = false
	) =>
		x => new CategoryResponse {
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
			Media = media ? x.Media.Select(m => MediaSelector().Compile().Invoke(m)).ToList() : null,
			Parent = parent ? CategorySelector().Compile().Invoke(x.Parent) : null,
			Children = children ? x.Children.Select(ch => CategorySelector().Compile().Invoke(ch)).ToList() : null
		};

	public static Expression<Func<ContentEntity, ContentResponse>> ContentSelector(
		bool media = false
	) =>
		x => new ContentResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			DeletedAt = x.DeletedAt,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Media = media ? x.Media.Select(m => MediaSelector().Compile().Invoke(m)).ToList() : null
		};

	public static Expression<Func<CommentEntity, CommentResponse>> CommentSelector(
		bool user = false,
		bool includeTargetUser = false,
		bool product = false,
		bool media = false,
		bool children = false
	) =>
		x => new CommentResponse {
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
			User = user ? UserSelector().Compile().Invoke(x.User) : null,
			TargetUser = includeTargetUser ? UserSelector().Compile().Invoke(x.TargetUser) : null,
			Product = product ? ProductSelector().Compile().Invoke(x.Product) : null,
			Media = media ? x.Media.Select(m => MediaSelector().Compile().Invoke(m)).ToList() : null,
			Children = children ? x.Children.Select(ch => CommentSelector().Compile().Invoke(ch)).ToList() : null
		};

	public static Expression<Func<ContractEntity, ContractResponse>> ContractSelector(
		bool user = false,
		bool creator = false,
		bool product = false,
		bool invoices = false
	) =>
		x => new ContractResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			DeletedAt = x.DeletedAt,
			Tags = x.Tags,
			StartDate = x.StartDate,
			EndDate = x.EndDate,
			Deposit = x.Deposit,
			Rent = x.Rent,
			JsonData = x.JsonData,
			UserId = x.UserId,
			CreatorId = x.CreatorId,
			ProductId = x.ProductId,
			User = user ? UserSelector().Compile().Invoke(x.User) : null,
			Creator = creator ? UserSelector().Compile().Invoke(x.Creator) : null,
			Product = product ? ProductSelector().Compile().Invoke(x.Product) : null,
			Invoices = invoices ? x.Invoices.Select(inv => InvoiceSelector().Compile().Invoke(inv)).ToList() : null
		};

	public static Expression<Func<InvoiceEntity, InvoiceResponse>> InvoiceSelector(
		bool user = false,
		bool contracts = false
	) =>
		x => new InvoiceResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			DeletedAt = x.DeletedAt,
			Tags = x.Tags,
			DebtAmount = x.DebtAmount,
			CreditorAmount = x.CreditorAmount,
			PaidAmount = x.PaidAmount,
			PenaltyAmount = x.PenaltyAmount,
			DueDate = x.DueDate,
			PaidDate = x.PaidDate,
			TrackingNumber = x.TrackingNumber,
			JsonData = x.JsonData,
			User = user ? UserSelector().Compile().Invoke(x.User) : null,
			Contract = contracts ? ContractSelector().Compile().Invoke(x.Contract) : null
		};
}