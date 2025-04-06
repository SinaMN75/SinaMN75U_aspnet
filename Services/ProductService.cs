namespace SinaMN75U.Services;

public interface IProductService {
	public Task<UResponse<ProductResponse?>> Create(ProductCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<ProductResponse>?>> Read(ProductReadParams p, CancellationToken ct);
	public Task<UResponse<ProductResponse?>> ReadById(IdParams p, CancellationToken ct);
	public Task<UResponse<ProductResponse?>> Update(ProductUpdateParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class ProductService(DbContext db, ITokenService ts, ILocalizationService ls) : IProductService {
	public async Task<UResponse<ProductResponse?>> Create(ProductCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ProductResponse?>(null, USC.UnAuthorized, ls.Get("AuthorizationRequired"));

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
			JsonDetail = new ProductJsonDetail {
				Details = p.Details,
				VisitCounts = [],
				RelatedProducts = []
			}
		};
		await db.Set<ProductEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<ProductResponse?>(e.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<ProductResponse>?>> Read(ProductReadParams p, CancellationToken ct) {
		IQueryable<ProductEntity> q = db.Set<ProductEntity>();
		if (p.ParentId.IsNotNullOrEmpty()) q = q.Where(x => x.ParentId == p.ParentId);
		if (p.Code.IsNotNullOrEmpty()) q = q.Where(x => x.Code.Contains(p.Code));
		if (p.Query.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Query) || (x.Description ?? "").Contains(p.Query) || (x.Subtitle ?? "").Contains(p.Query));
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title));
		if (p.UserId.IsNotNullOrEmpty()) q = q.Where(x => x.UserId == p.UserId);
		if (p.Ids.IsNotNullOrEmpty()) q = q.Where(x => p.Ids.Contains(x.UserId));

		if (p.User) q = q.Include(x => x.User);
		if (p.Media) q = q.Include(x => x.Media);
		if (p.Categories) q = q.Include(x => x.Categories);

		UResponse<List<ProductEntity>> result = await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);

		return new UResponse<IEnumerable<ProductResponse>?>(result.Result?.Select(x => x.MapToResponse())) {
			TotalCount = result.TotalCount,
			PageCount = result.PageCount,
			PageSize = result.PageSize
		};
	}

	public async Task<UResponse<ProductResponse?>> ReadById(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);

		ProductEntity? e = await db.Set<ProductEntity>().Select(x => x)
			.Include(x => x.Media)
			.Include(x => x.Categories)
			.Include(x => x.User)
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<ProductResponse?>(null, USC.NotFound, ls.Get("ProductNotFound"));

		VisitCount? visitCount = e.JsonDetail.VisitCounts.FirstOrDefault(v => v.UserId == (userData?.Id ?? Guid.Empty));

		if (visitCount != null) visitCount.Count++;
		else e.JsonDetail.VisitCounts.Add(new VisitCount { UserId = userData?.Id ?? Guid.Empty, Count = 1 });

		db.Set<ProductEntity>().Update(e);
		await db.SaveChangesAsync(ct);

		return new UResponse<ProductResponse?>(e.MapToResponse());
	}

	public async Task<UResponse<ProductResponse?>> Update(ProductUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ProductResponse?>(null, USC.UnAuthorized, ls.Get("AuthorizationRequired"));

		ProductEntity? e = await db.Set<ProductEntity>().FindAsync(p.Id, ct);
		if (e == null) return new UResponse<ProductResponse?>(null, USC.NotFound, ls.Get("ProductNotFound"));

		if (p.Code.IsNotNullOrEmpty()) e.Code = p.Code;
		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.Description.IsNotNullOrEmpty()) e.Description = p.Description;
		if (p.Subtitle.IsNotNullOrEmpty()) e.Subtitle = p.Subtitle;
		if (p.Latitude.IsNotNullOrEmpty()) e.Latitude = p.Latitude;
		if (p.Longitude.IsNotNullOrEmpty()) e.Longitude = p.Longitude;
		if (p.Price.IsNotNullOrEmpty()) e.Price = p.Price;
		if (p.ParentId.IsNotNullOrEmpty()) e.ParentId = p.ParentId;
		if (p.Stock.IsNotNullOrEmpty()) e.Stock = p.Stock;
		if (p.Details.IsNotNullOrEmpty()) e.JsonDetail.Details = p.Details;
		if (p.UserId.IsNotNullOrEmpty()) e.UserId = p.UserId ?? userData.Id;

		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRange(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => p.RemoveTags.Contains(x));

		if (p.AddRelatedProducts.IsNotNullOrEmpty()) e.JsonDetail.RelatedProducts.AddRange(p.AddRelatedProducts);
		if (p.RemoveRelatedProducts.IsNotNullOrEmpty()) e.JsonDetail.RelatedProducts.RemoveAll(x => p.RemoveRelatedProducts.Contains(x));

		db.Set<ProductEntity>().Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<ProductResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ProductResponse?>(null, USC.UnAuthorized, ls.Get("AuthorizationRequired"));

		int count = await db.Set<ProductEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return count > 0 ? new UResponse(USC.Deleted, ls.Get("ProductDeleted")) : new UResponse(USC.NotFound, ls.Get("ProductNotFound"));
	}
}