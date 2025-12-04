namespace SinaMN75U.Data;

public static class Projections {
	public static readonly Expression<Func<MediaEntity, MediaResponse>> MediaSelector =
		m => new MediaResponse {
			Tags = m.Tags,
			JsonData = m.JsonData,
			Path = m.Path,
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
			UserId = x.UserId
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
			Media = x.Media.AsQueryable().Select(MediaSelector).ToList(),
			Children = x.Children.AsQueryable()
				.Select(c => new CategoryResponse {
					Id = c.Id,
					CreatedAt = c.CreatedAt,
					UpdatedAt = c.UpdatedAt,
					DeletedAt = c.DeletedAt,
					Tags = c.Tags,
					JsonData = c.JsonData,
					Title = c.Title,
					Order = c.Order,
					Code = c.Code,
					ParentId = c.ParentId,
					Media = c.Media.AsQueryable().Select(MediaSelector).ToList(),
					Children = new List<CategoryResponse>()
				})
				.ToList(),
		};

	public static Expression<Func<ContentEntity, ContentResponse>> ContentSelector =
		x => new ContentResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			DeletedAt = x.DeletedAt,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Media = x.Media.AsQueryable().Select(MediaSelector).ToList(),
		};


	public static Expression<Func<CommentEntity, CommentResponse>> CommentSelector(
		CommentReadParams p) {
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
			User = p.ShowUser
				? UserSelector.Invoke(x.User)
				: null,
			TargetUser = p.ShowTargetUser && x.TargetUserId != null
				? UserSelector.Invoke(x.TargetUser!)
				: null,
			Product = p.ShowProduct && x.ProductId != null
				? ProductSelector.Invoke(x.Product!)
				: null,
			Media = p.ShowMedia
				? x.Media.AsQueryable().Select(MediaSelector).ToList()
				: null,
			Children = p.ShowChildren
				? x.Children.AsQueryable()
					.Select(CommentSelector(p))
					.ToList()
				: null
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
			User = UserSelector.Invoke(x.User),
			Creator = UserSelector.Invoke(x.Creator),
			Product = ProductSelector.Invoke(x.Product),
			Invoices = x.Invoices.AsQueryable()
				.Select(InvoiceSelector)
				.ToList(),

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