namespace SinaMN75U.Data;

public static class Projections {
	public static readonly Expression<Func<MediaEntity, MediaResponse>> MediaSelector =
		m => new MediaResponse {
			Tags = m.Tags,
			JsonData = m.JsonData,
			Path = m.Path
		};

	public static readonly Expression<Func<UserEntity, UserResponse>> UserSelector =
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
			Categories = null,
			Media = null,
		};

	public static readonly Expression<Func<ProductEntity, ProductResponse>> ProductSelector =
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
			User = null,
			Categories = null,
			Children = null
		};

	public static Expression<Func<CategoryEntity, CategoryResponse>> CategorySelector =
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
			Media = null,
			Children = null,
			Parent = null
		};

	public static Expression<Func<ContentEntity, ContentResponse>> ContentSelector =
		x => new ContentResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			DeletedAt = x.DeletedAt,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Media = null,
		};


	public static Expression<Func<CommentEntity, CommentResponse>> CommentSelector() {
		return x => new CommentResponse {
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
			User = null,
			TargetUser = null,
			Product = null,
			Media = null,
			Children = null,
			Parent = null
		};
	}
	
	public static Expression<Func<ContractEntity, ContractResponse>> ContractSelector =
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
			User = null,
			Creator = null,
			Product = null,
			Invoices = null,
		};
	
	public static readonly Expression<Func<InvoiceEntity, InvoiceResponse>> InvoiceSelector =
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
			User = null,
			Contract = null,
		};
}