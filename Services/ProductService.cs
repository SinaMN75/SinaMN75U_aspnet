namespace SinaMN75U.Services;

public interface IProductService {
	public Task<UResponse<ProductEntity?>> Create(ProductCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<ProductEntity>?>> Read(ProductReadParams p, CancellationToken ct);
	public Task<UResponse<ProductEntity?>> ReadById(IdParams p, CancellationToken ct);
	public Task<UResponse<ProductEntity?>> Update(ProductUpdateParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdParams p, CancellationToken ct);
	public Task<UResponse> DeleteRange(IdListParams p, CancellationToken ct);
}

public class ProductService(DbContext db, ITokenService ts, ILocalizationService ls, ICategoryService categoryService) : IProductService {
	public async Task<UResponse<ProductEntity?>> Create(ProductCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ProductEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		List<CategoryEntity> categories = [];
		if (p.Categories.IsNotNullOrEmpty())
			categories = await categoryService.ReadEntity(new CategoryReadParams { Ids = p.Categories }, ct) ?? [];

		ProductEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			Title = p.Title,
			Code = p.Code ?? Random.Shared.Next(10000, 99999).ToString(),
			Subtitle = p.Subtitle,
			Description = p.Description,
			Latitude = p.Latitude,
			Longitude = p.Longitude,
			Stock = p.Stock,
			Price = p.Price,
			ParentId = p.ParentId,
			UserId = p.UserId ?? userData.Id,
			Tags = p.Tags,
			Categories = categories,
			Type = p.Type,
			Content = p.Content,
			Slug = p.Slug,
			JsonData = new ProductJson {
				Details = p.Details,
				ActionTitle = p.ActionTitle,
				ActionUri = p.ActionUri,
				ActionType = p.ActionType,
				VisitCounts = [],
				RelatedProducts = []
			}
		};
		await db.Set<ProductEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<ProductEntity?>(e);
	}


	public async Task<UResponse<IEnumerable<ProductEntity>?>> Read(ProductReadParams p, CancellationToken ct) {
		IQueryable<ProductEntity> q = db.Set<ProductEntity>();
		if (p.Code.IsNotNullOrEmpty()) q = q.Where(x => x.Code.Contains(p.Code));
		if (p.Query.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Query) || (x.Description ?? "").Contains(p.Query) || (x.Subtitle ?? "").Contains(p.Query));
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title));
		if (p.MinPrice.IsNotNullOrEmpty()) q = q.Where(x => x.Price >= p.MinPrice);
		if (p.MaxPrice.IsNotNullOrEmpty()) q = q.Where(x => x.Price <= p.MaxPrice);
		if (p.MinStock.IsNotNullOrEmpty()) q = q.Where(x => x.Stock >= p.MinStock);
		if (p.MaxStock.IsNotNullOrEmpty()) q = q.Where(x => x.Stock >= p.MaxStock);

		if (p.Ids.IsNotNullOrEmpty()) q = q.Where(x => p.Ids.Contains(x.Id));
		if (p.UserId.IsNotNullOrEmpty()) q = q.Where(x => x.UserId == p.UserId);
		if (p.ParentId.IsNotNullOrEmpty()) q = q.Where(x => x.ParentId == p.ParentId);
		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(u => u.Tags.Any(tag => p.Tags!.Contains(tag)));

		if (p.ShowUser) q = q.Include(x => x.User);
		if (p.ShowMedia) q = q.Include(x => x.Media);
		if (p.ShowCategories) q = q.Include(x => x.Categories);
		if (p.ShowChildren)
			q = q.Include(x => x.Children)!
				.ThenInclude(x => x.Children)!
				.ThenInclude(x => x.Children)!
				.ThenInclude(x => x.Children)!
				.ThenInclude(x => x.Children)!
				.ThenInclude(x => x.Children)!
				.ThenInclude(x => x.Children);

		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ProductEntity?>> ReadById(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);

		ProductEntity? e = await db.Set<ProductEntity>()
			.Include(x => x.Media)
			.Include(x => x.Categories)
			.Include(x => x.User)
			.Include(x => x.Children)
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<ProductEntity?>(null, Usc.NotFound, ls.Get("ProductNotFound"));

		VisitCount? visitCount = e.JsonData.VisitCounts.FirstOrDefault(v => v.UserId == (userData?.Id ?? Guid.Empty));

		if (visitCount != null) visitCount.Count++;
		else e.JsonData.VisitCounts.Add(new VisitCount { UserId = userData?.Id ?? Guid.Empty, Count = 1 });

		db.Set<ProductEntity>().Update(e);
		await db.SaveChangesAsync(ct);

		return new UResponse<ProductEntity?>(e);
	}

	public async Task<UResponse<ProductEntity?>> Update(ProductUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ProductEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ProductEntity? e = await db.Set<ProductEntity>().Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<ProductEntity?>(null, Usc.NotFound, ls.Get("ProductNotFound"));

		e.ApplyUpdates(p);
		// if (p.Code.IsNotNullOrEmpty()) e.Code = p.Code;
		// if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		// if (p.Description.IsNotNullOrEmpty()) e.Description = p.Description;
		// if (p.Subtitle.IsNotNullOrEmpty()) e.Subtitle = p.Subtitle;
		// if (p.Latitude.IsNotNullOrEmpty()) e.Latitude = p.Latitude;
		// if (p.Longitude.IsNotNullOrEmpty()) e.Longitude = p.Longitude;
		// if (p.Price.IsNotNullOrEmpty()) e.Price = p.Price;
		// if (p.ParentId.IsNotNullOrEmpty()) e.ParentId = p.ParentId;
		// if (p.Stock.IsNotNullOrEmpty()) e.Stock = p.Stock;
		// if (p.Details.IsNotNullOrEmpty()) e.JsonData.Details = p.Details;
		// if (p.UserId.IsNotNullOrEmpty()) e.UserId = p.UserId ?? userData.Id;

		// if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		// if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => p.RemoveTags.Contains(x));

		if (p.AddRelatedProducts.IsNotNullOrEmpty()) e.JsonData.RelatedProducts.AddRangeIfNotExist(p.AddRelatedProducts);
		if (p.RemoveRelatedProducts.IsNotNullOrEmpty()) e.JsonData.RelatedProducts.RemoveAll(x => p.RemoveRelatedProducts.Contains(x));

		if (p.AddCategories.IsNotNullOrEmpty()) {
			IEnumerable<CategoryEntity> newCategories = await categoryService.ReadEntity(new CategoryReadParams { Ids = p.AddCategories }, ct) ?? [];
			e.Categories.AddRangeIfNotExist(newCategories);
		}

		if (p.RemoveCategories.IsNotNullOrEmpty()) {
			IEnumerable<CategoryEntity> categoriesToRemove = await categoryService.ReadEntity(new CategoryReadParams { Ids = p.RemoveCategories }, ct) ?? [];
			e.Categories.AddRangeIfNotExist(categoriesToRemove);
		}

		db.Set<ProductEntity>().Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<ProductEntity?>(e);
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ProductEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		int count = await db.Set<ProductEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return count > 0 ? new UResponse(Usc.Deleted, ls.Get("ProductDeleted")) : new UResponse(Usc.NotFound, ls.Get("ProductNotFound"));
	}

	public async Task<UResponse> DeleteRange(IdListParams p, CancellationToken ct) {
		await db.Set<ProductEntity>().WhereIn(u => u.Id, p.Ids).ExecuteDeleteAsync(ct);
		return new UResponse();
	}
}