namespace SinaMN75U.Services;

public interface IProductService {
	public Task<UResponse> BulkCreate(List<ProductCreateParams> p, CancellationToken ct);
	public Task<UResponse<ProductEntity?>> Create(ProductCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<ProductEntity>?>> Read(ProductReadParams p, CancellationToken ct);
	public Task<UResponse<ProductEntity?>> ReadById(IdParams p, CancellationToken ct);
	public Task<UResponse<ProductEntity?>> Update(ProductUpdateParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdParams p, CancellationToken ct);
	public Task<UResponse> DeleteRange(IdListParams p, CancellationToken ct);
}

public class ProductService(DbContext db, ITokenService ts, ILocalizationService ls, ICategoryService categoryService) : IProductService {
	public async Task<UResponse> BulkCreate(List<ProductCreateParams> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.First().Token);
		if (userData == null) return new UResponse<ProductEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		List<ProductEntity> list = [];

		foreach (ProductCreateParams i in p) {
			List<CategoryEntity> categories = [];
			if (i.Categories.IsNotNullOrEmpty()) categories = await categoryService.ReadEntity(new CategoryReadParams { Ids = i.Categories }, ct) ?? [];
			list.Add(new ProductEntity {
				Title = i.Title,
				Code = i.Code,
				Subtitle = i.Subtitle,
				Description = i.Description,
				Latitude = i.Latitude,
				Longitude = i.Longitude,
				Stock = i.Stock,
				Price = i.Price,
				ParentId = i.ParentId,
				UserId = i.UserId ?? userData.Id,
				Tags = i.Tags,
				Categories = categories,
				Type = i.Type,
				Content = i.Content,
				Slug = i.Slug,
				JsonData = new ProductJson {
					Details = i.Details,
					ActionTitle = i.ActionTitle,
					ActionUri = i.ActionUri,
					ActionType = i.ActionType,
					VisitCounts = [],
					RelatedProducts = i.RelatedProducts?.ToList() ?? []
				}
			});
		}

		await db.AddRangeAsync(list);
		await db.SaveChangesAsync();
		return new UResponse();
	}

	public async Task<UResponse<ProductEntity?>> Create(ProductCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ProductEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		List<CategoryEntity> categories = [];
		if (p.Categories.IsNotNullOrEmpty())
			categories = await categoryService.ReadEntity(new CategoryReadParams { Ids = p.Categories }, ct) ?? [];

		ProductEntity e = new() {
			Title = p.Title,
			Code = p.Code,
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
				RelatedProducts = p.RelatedProducts?.ToList() ?? []
			}
		};
		await db.Set<ProductEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<ProductEntity?>(e);
	}


	public async Task<UResponse<IEnumerable<ProductEntity>?>> Read(ProductReadParams p, CancellationToken ct) {
		IQueryable<ProductEntity> q = db.Set<ProductEntity>().Where(x => x.ParentId == null);

		if (p.Query.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Query!) || (x.Description ?? "").Contains(p.Query!) || (x.Subtitle ?? "").Contains(p.Query!));
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));
		if (p.Code.IsNotNullOrEmpty()) q = q.Where(x => (x.Code ?? "").Contains(p.Code!));
		if (p.ParentId.IsNotNullOrEmpty()) q = q.Where(x => x.ParentId == p.ParentId);
		if (p.UserId.IsNotNullOrEmpty()) q = q.Where(x => x.UserId == p.UserId);
		if (p.Ids.IsNotNullOrEmpty()) q = q.Where(x => p.Ids!.Contains(x.UserId));
		if (p.MinStock.IsNotNull()) q = q.Where(x => x.Stock >= p.MinStock);
		if (p.MaxStock.IsNotNull()) q = q.Where(x => x.Stock <= p.MaxStock);
		if (p.MinPrice.IsNotNull()) q = q.Where(x => x.Price >= p.MinPrice);
		if (p.MaxPrice.IsNotNull()) q = q.Where(x => x.Price <= p.MaxPrice);

		if (p.ShowMedia) q = q.Include(x => x.Media);

		if (p.ShowUser) {
			q = q.Include(x => x.User);
			if (p.ShowUserMedia) q = q.Include(x => x.User).ThenInclude(x => x.Media);
			if (p.ShowUserCategory) {
				if (p.ShowCategoriesMedia) q = q.Include(x => x.User).ThenInclude(x => x.Categories).ThenInclude(x => x.Media);
				else q = q.Include(x => x.User).ThenInclude(x => x.Categories);
			}
		}

		if (p.ShowCategories) {
			if (p.ShowCategoriesMedia) q = q.Include(x => x.Categories).ThenInclude(x => x.Media);
			else q = q.Include(x => x.Categories);
		}

		if (p.ShowChildren) {
			if (p.ShowMedia) q = q.Include(x => x.Children).Include(x => x.Media);

			if (p.ShowUser) {
				q = q.Include(x => x.Children).Include(x => x.User);
				if (p.ShowUserMedia) q = q.Include(x => x.Children).Include(x => x.User).ThenInclude(x => x.Media);
				if (p.ShowUserCategory) {
					if (p.ShowCategoriesMedia) q = q.Include(x => x.Children).ThenInclude(x => x.User).ThenInclude(x => x.Categories).ThenInclude(x => x.Media);
					else q = q.Include(x => x.Children).ThenInclude(x => x.User).ThenInclude(x => x.Categories);
				}
			}

			if (p.ShowCategories) {
				if (p.ShowCategoriesMedia) q = q.Include(x => x.Children).ThenInclude(x => x.Categories).ThenInclude(x => x.Media);
				else q = q.Include(x => x.Children).ThenInclude(x => x.Categories);
			}
		}

		if (p.ShowChildrenDepth)
			q = q.Include(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children).ThenInclude(x => x.Children);

		if (p.OrderByCreatedAt) q = q.OrderBy(x => x.CreatedAt);
		if (p.OrderByCreatedAt) q = q.OrderByDescending(x => x.CreatedAt);

		return await q.OrderBy(x => x.CreatedAt).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ProductEntity?>> ReadById(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);

		ProductEntity? e = await db.Set<ProductEntity>().AsTracking()
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

		ProductEntity? e = await db.Set<ProductEntity>().AsTracking().Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<ProductEntity?>(null, Usc.NotFound, ls.Get("ProductNotFound"));

		e.UpdatedAt = DateTime.UtcNow;
		if (p.Title.IsNotNull()) e.Title = p.Title;
		if (p.Code.IsNotNull()) e.Code = p.Code;
		if (p.Subtitle.IsNotNull()) e.Subtitle = p.Subtitle;
		if (p.Description.IsNotNull()) e.Description = p.Description;
		if (p.Slug.IsNotNull()) e.Slug = p.Slug;
		if (p.Type.IsNotNull()) e.Type = p.Type;
		if (p.Content.IsNotNull()) e.Content = p.Content;
		if (p.Latitude.IsNotNull()) e.Latitude = p.Latitude;
		if (p.Longitude.IsNotNull()) e.Longitude = p.Longitude;
		if (p.Stock.IsNotNull()) e.Stock = p.Stock;
		if (p.Price.IsNotNull()) e.Price = p.Price;
		if (p.ParentId.IsNotNullOrEmpty()) e.ParentId = p.ParentId;
		if (p.UserId.IsNotNull()) e.UserId = p.UserId.Value;
		if (p.ActionType.IsNotNull()) e.JsonData.ActionType = p.ActionType;
		if (p.ActionTitle.IsNotNull()) e.JsonData.ActionTitle = p.ActionTitle;
		if (p.ActionUri.IsNotNull()) e.JsonData.ActionUri = p.ActionUri;
		if (p.Details.IsNotNull()) e.JsonData.Details = p.Details;
		if (p.RelatedProducts.IsNotNull()) e.JsonData.RelatedProducts = p.RelatedProducts;
		if (p.AddRelatedProducts.IsNotNullOrEmpty()) e.JsonData.RelatedProducts.AddRangeIfNotExist(p.AddRelatedProducts);
		if (p.RemoveRelatedProducts.IsNotNullOrEmpty()) e.JsonData.RelatedProducts.RemoveRangeIfExist(p.RemoveRelatedProducts);

		if (p.AddCategories.IsNotNullOrEmpty()) {
			IEnumerable<CategoryEntity> newCategories = await categoryService.ReadEntity(new CategoryReadParams { Ids = p.AddCategories }, ct) ?? [];
			e.Categories.AddRangeIfNotExist(newCategories);
		}

		if (p.RemoveCategories.IsNotNullOrEmpty()) {
			IEnumerable<CategoryEntity> categoriesToRemove = await categoryService.ReadEntity(new CategoryReadParams { Ids = p.RemoveCategories }, ct) ?? [];
			e.Categories.RemoveRangeIfExist(categoriesToRemove);
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
		int count = await db.Set<ProductEntity>().WhereIn(u => u.Id, p.Ids).ExecuteDeleteAsync(ct);
		return count > 0 ? new UResponse(Usc.Deleted, ls.Get("ProductDeleted")) : new UResponse(Usc.NotFound, ls.Get("ProductNotFound"));
	}
}