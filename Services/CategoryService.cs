namespace SinaMN75U.Services;

public interface ICategoryService {
	Task<UResponse> BulkCreate(IEnumerable<CategoryCreateParams> p, CancellationToken ct);
	Task<UResponse<CategoryEntity?>> Create(CategoryCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<CategoryEntity>?>> Read(CategoryReadParams p, CancellationToken ct);
	Task<UResponse<CategoryEntity?>> ReadById(IdParams p, CancellationToken ct);
	Task<UResponse<CategoryEntity?>> Update(CategoryUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> DeleteRange(IdListParams p, CancellationToken ct);

	Task<List<CategoryEntity>?> ReadEntity(CategoryReadParams p, CancellationToken ct);
}

public class CategoryService(DbContext db, IMediaService mediaService, ILocalizationService ls, ITokenService ts) : ICategoryService {
	public async Task<UResponse> BulkCreate(IEnumerable<CategoryCreateParams> p, CancellationToken ct) {
		List<CategoryEntity> list = [];
		list.AddRange(p.Select(i => new CategoryEntity {
			Id = i.Id ?? Guid.CreateVersion7(),
			Title = i.Title,
			Tags = i.Tags,
			Order = i.Order,
			ParentId = i.ParentId,
			JsonData = new CategoryJson {
				Subtitle = i.Subtitle,
				Link = i.Link,
				Location = i.Location,
				Type = i.Type,
				RelatedProducts = i.RelatedProducts ?? []
			}
		}));
		await db.AddRangeAsync(list);
		await db.SaveChangesAsync();
		return new UResponse();
	}

	public async Task<UResponse<CategoryEntity?>> Create(CategoryCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<CategoryEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		CategoryEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			Title = p.Title,
			JsonData = new CategoryJson {
				Subtitle = p.Subtitle,
				Link = p.Link,
				Location = p.Location,
				Type = p.Type,
				RelatedProducts = p.RelatedProducts ?? []
			},
			Tags = p.Tags,
			Order = p.Order,
			ParentId = p.ParentId,
		};

		await db.Set<CategoryEntity>().AddAsync(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<CategoryEntity?>(e);
	}

	public async Task<UResponse<IEnumerable<CategoryEntity>?>> Read(CategoryReadParams p, CancellationToken ct) {
		IQueryable<CategoryEntity> q = db.Set<CategoryEntity>()
			.Include(x => x.Children)
			.Include(x => x.Media)
			.Where(x => x.ParentId == null)
			.OrderBy(x => x.Id);

		if (p.Tags.IsNotNullOrEmpty())
			q = q.Where(x => x.Tags.Any(tag => p.Tags!.Contains(tag)));

		if (p.Ids.IsNotNullOrEmpty())
			q = q.Where(x => p.Ids.Contains(x.Id));

		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<CategoryEntity?>> ReadById(IdParams p, CancellationToken ct) {
		CategoryEntity? e = await db.Set<CategoryEntity>().Select(x => new CategoryEntity {
			Title = x.Title,
			Order = x.Order,
			ParentId = x.ParentId,
			Media = x.Media.Select(y => new MediaEntity {
				Path = y.Path,
				Id = y.Id,
				Tags = y.Tags,
				JsonData = y.JsonData
			}).ToList(),
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			Tags = x.Tags,
			JsonData = x.JsonData
		}).FirstOrDefaultAsync(x => x.Id == p.Id);
		return e == null ? new UResponse<CategoryEntity?>(null, Usc.NotFound, ls.Get("CategoryNotFound")) : new UResponse<CategoryEntity?>(e);
	}

	public async Task<UResponse<CategoryEntity?>> Update(CategoryUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<CategoryEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		CategoryEntity? e = await db.Set<CategoryEntity>().FindAsync(p.Id, ct);
		if (e == null) return new UResponse<CategoryEntity?>(null, Usc.NotFound, ls.Get("CategoryNotFound"));

		e.UpdatedAt = DateTime.UtcNow;
		if (p.Title.IsNotNull()) e.Title = p.Title;
		if (p.ParentId.IsNotNullOrEmpty()) e.ParentId = p.ParentId;
		if (p.Order.IsNotNull()) e.Order = p.Order;

		if (p.Subtitle.IsNotNull()) e.JsonData.Subtitle = p.Subtitle;
		if (p.Link.IsNotNull()) e.JsonData.Link = p.Link;
		if (p.Location.IsNotNull()) e.JsonData.Location = p.Location;
		if (p.Type.IsNotNull()) e.JsonData.Type = p.Type;
		if (p.Subtitle.IsNotNull()) e.JsonData.Subtitle = p.Subtitle;
		if (p.Subtitle.IsNotNull()) e.JsonData.Subtitle = p.Subtitle;

		if (p.RelatedProducts.IsNotNull()) e.JsonData.RelatedProducts = p.RelatedProducts;
		if (p.AddRelatedProducts.IsNotNullOrEmpty()) e.JsonData.RelatedProducts.AddRangeIfNotExist(p.AddRelatedProducts);
		if (p.RemoveRelatedProducts.IsNotNullOrEmpty()) e.JsonData.RelatedProducts.RemoveRangeIfExist(p.RemoveRelatedProducts);

		db.Update(e);
		await db.SaveChangesAsync(ct);

		return new UResponse<CategoryEntity?>(e);
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null)
			return new UResponse<CategoryEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		CategoryEntity? category = await db.Set<CategoryEntity>()
			.Include(x => x.Media)
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);

		if (category == null)
			return new UResponse(Usc.NotFound, ls.Get("CategoryNotFound"));

		if (category.Media.IsNotNullOrEmpty())
			await mediaService.DeleteRange(category.Media.Select(x => x.Id), ct);

		db.Set<CategoryEntity>().Remove(category);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteRange(IdListParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null)
			return new UResponse<CategoryEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<CategoryEntity>().WhereIn(u => u.Id, p.Ids).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<List<CategoryEntity>?> ReadEntity(CategoryReadParams p, CancellationToken ct) {
		IQueryable<CategoryEntity> q = db.Set<CategoryEntity>().OrderByDescending(x => x.Id);
		if (p.Ids.IsNotNullOrEmpty()) q = q.Where(x => p.Ids.Contains(x.Id));
		return await q.ToListAsync(ct);
	}
}