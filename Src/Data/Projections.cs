namespace SinaMN75U.Data;

public static class Projections {
	public static Expression<Func<MediaEntity, MediaResponse>> MediaSelector() =>
		x => new MediaResponse {
			Tags = x.Tags,
			JsonData = x.JsonData,
			Path = x.Path
		};

	static UserResponse MapUser(UserEntity x) => new() {
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
		Birthdate = x.Birthdate
	};

	public static Expression<Func<UserEntity, UserResponse>> UserSelector(bool categories = false, bool media = false) => x => new UserResponse {
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
		Categories = categories ? x.Categories.AsQueryable().Select(CategorySelector()).ToList() : null,
		Media = media ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null
	};

	static ProductResponse MapProduct(ProductEntity x) => new() {
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

	public static Expression<Func<ProductEntity, ProductResponse>> ProductSelector(bool user = false, bool categories = false, bool children = false) => x => new ProductResponse {
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
		User = user ? MapUser(x.User) : null,
		Categories = categories ? x.Categories.AsQueryable().Select(CategorySelector()).ToList() : null,
		Children = children ? x.Children.AsQueryable().Select(ProductSelector()).ToList() : null
	};

	static CategoryResponse MapCategory(CategoryEntity x) => new() {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		UpdatedAt = x.UpdatedAt,
		DeletedAt = x.DeletedAt,
		Tags = x.Tags,
		JsonData = x.JsonData,
		Title = x.Title,
		Order = x.Order,
		Code = x.Code,
		ParentId = x.ParentId
	};

	public static Expression<Func<CategoryEntity, CategoryResponse>> CategorySelector(bool media = false, bool parent = false, bool children = false) => x => new CategoryResponse {
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
		Parent = parent ? MapCategory(x.Parent) : null,
		Children = children ? x.Children.AsQueryable().Select(CategorySelector()).ToList() : null
	};

	static ContentResponse MapContent(ContentEntity x) => new() {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		UpdatedAt = x.UpdatedAt,
		DeletedAt = x.DeletedAt,
		Tags = x.Tags,
		JsonData = x.JsonData
	};

	public static Expression<Func<ContentEntity, ContentResponse>> ContentSelector(bool media = false) => x => new ContentResponse {
		Id = x.Id,
		CreatedAt = x.CreatedAt,
		UpdatedAt = x.UpdatedAt,
		DeletedAt = x.DeletedAt,
		Tags = x.Tags,
		JsonData = x.JsonData,
		Media = media ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null
	};

	static CommentResponse MapComment(CommentEntity x) => new() {
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
		Description = x.Description
	};

	public static Expression<Func<CommentEntity, CommentResponse>> CommentSelector(bool user = false, bool targetUser = false, bool product = false, bool media = false, bool children = false) => x => new CommentResponse {
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
		User = user ? MapUser(x.User) : null,
		TargetUser = targetUser ? MapUser(x.TargetUser) : null,
		Product = product ? MapProduct(x.Product) : null,
		Media = media ? x.Media.AsQueryable().Select(MediaSelector()).ToList() : null,
		Children = children ? x.Children.AsQueryable().Select(CommentSelector()).ToList() : null
	};

	static ContractResponse MapContract(ContractEntity x) => new() {
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
		ProductId = x.ProductId
	};

	public static Expression<Func<ContractEntity, ContractResponse>> ContractSelector(bool user = false, bool creator = false, bool product = false, bool invoices = false) => x => new ContractResponse {
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
		User = user ? MapUser(x.User) : null,
		Creator = creator ? MapUser(x.Creator) : null,
		Product = product ? MapProduct(x.Product) : null,
		Invoices = invoices ? x.Invoices.AsQueryable().Select(InvoiceSelector()).ToList() : null
	};

	static InvoiceResponse MapInvoice(InvoiceEntity x) => new() {
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
		TrackingNumber = x.TrackingNumber
	};

	public static Expression<Func<InvoiceEntity, InvoiceResponse>> InvoiceSelector(bool user = false, bool contracts = false) =>
		x => new InvoiceResponse {
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
			User = user ? MapUser(x.User) : null,
			Contract = contracts ? MapContract(x.Contract) : null
		};
}