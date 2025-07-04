namespace SinaMN75U.Services;

public interface ICategoryService {
	Task<UResponse<CategoryResponse?>> Create(CategoryCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<CategoryResponse>?>> Read(CategoryReadParams p, CancellationToken ct);
	Task<UResponse<CategoryResponse?>> ReadById(IdParams p, CancellationToken ct);
	Task<UResponse<CategoryResponse?>> Update(CategoryUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> DeleteRange(IdListParams p, CancellationToken ct);

	Task<List<CategoryEntity>?> ReadEntity(CategoryReadParams p, CancellationToken ct);
}

public class CategoryService(
	DbContext db,
	IMediaService mediaService,
	ILocalizationService ls,
	ITokenService ts
) : ICategoryService {
	public async Task<UResponse<CategoryResponse?>> Create(CategoryCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<CategoryResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		CategoryEntity e = new() {
			Id = Guid.CreateVersion7(),
			Title = p.Title,
			JsonData = new CategoryJson { Subtitle = p.Subtitle, Link = p.Link },
			Tags = p.Tags,
			CreatedAt = DateTime.UtcNow,
			Location = p.Location,
			Type = p.Type,
			Order = p.Order,
			UpdatedAt = DateTime.UtcNow,
			ParentId = p.ParentId
		};

		db.Set<CategoryEntity>().Add(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<CategoryResponse?>(e.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<CategoryResponse>?>> Read(CategoryReadParams p, CancellationToken ct) {
		IQueryable<CategoryEntity> q = db.Set<CategoryEntity>()
			.Include(x => x.Children)
			.Include(x => x.Media)
			.Where(x => x.ParentId == null)
			.OrderBy(x => x.Id);

		if (p.Tags.IsNotNullOrEmpty())
			q = q.Where(x => x.Tags.Any(tag => p.Tags!.Contains(tag)));

		if (p.Ids.IsNotNullOrEmpty())
			q = q.Where(x => p.Ids.Contains(x.Id));

		return await q.Select(x => x.MapToResponse(p.ShowMedia)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<CategoryResponse?>> ReadById(IdParams p, CancellationToken ct) {
		CategoryResponse? e = await db.Set<CategoryEntity>().Select(x => new CategoryResponse {
			Title = x.Title,
			Order = x.Order,
			Location = x.Location,
			Type = x.Type,
			ParentId = x.ParentId,
			Media = x.Media!.Select(y => new MediaResponse {
				Path = y.Path,
				Id = y.Id,
				Tags = y.Tags,
				JsonData = y.JsonData
			}),
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			Tags = x.Tags,
			JsonData = x.JsonData
		}).FirstOrDefaultAsync(x => x.Id == p.Id);
		return e == null ? new UResponse<CategoryResponse?>(null, Usc.NotFound, ls.Get("CategoryNotFound")) : new UResponse<CategoryResponse?>(e);
	}

	public async Task<UResponse<CategoryResponse?>> Update(CategoryUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<CategoryResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		CategoryEntity? e = await db.Set<CategoryEntity>().FindAsync(p.Id, ct);
		if (e == null) return new UResponse<CategoryResponse?>(null, Usc.NotFound, ls.Get("CategoryNotFound"));

		e.UpdatedAt = DateTime.UtcNow;
		e.ApplyUpdates(p);

		db.Update(e);
		await db.SaveChangesAsync(ct);

		return new UResponse<CategoryResponse?>(e.MapToResponse());
	}

	// public async Task<UResponse<CategoryResponse?>> Update(CategoryUpdateParams p, CancellationToken ct) {
	// 	JwtClaimData? userData = ts.ExtractClaims(p.Token);
	// 	if (userData == null) return new UResponse<CategoryResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
	// 	CategoryEntity? e = await db.Set<CategoryEntity>().FindAsync(p.Id, ct);
	// 	if (e == null)
	// 		return new UResponse<CategoryResponse?>(null, Usc.NotFound, "Category not found");
	//
	// 	e.UpdatedAt = DateTime.UtcNow;
	// 	if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
	// 	if (p.Type.IsNotNullOrEmpty()) e.Type = p.Type;
	// 	if (p.Order != null) e.Order = p.Order;
	// 	if (p.Location.IsNotNullOrEmpty()) e.Location = p.Location;
	// 	if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
	// 	if (p.Subtitle.IsNotNullOrEmpty()) e.JsonData.Subtitle = p.Subtitle;
	// 	if (p.Link.IsNotNullOrEmpty()) e.JsonData.Link = p.Link;
	//
	// 	if (p.AddTags != null) e.Tags.AddRangeIfNotExist(p.AddTags);
	// 	if (p.RemoveTags != null) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));
	//
	// 	db.Update(e);
	// 	await db.SaveChangesAsync(ct);
	// 	return new UResponse<CategoryResponse?>(e.MapToResponse());
	// }

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null)
			return new UResponse<CategoryResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

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
			return new UResponse<CategoryResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<CategoryEntity>().WhereIn(u => u.Id, p.Ids).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<List<CategoryEntity>?> ReadEntity(CategoryReadParams p, CancellationToken ct) {
		IQueryable<CategoryEntity> q = db.Set<CategoryEntity>().OrderByDescending(x => x.Id);
		if (p.Ids.IsNotNullOrEmpty()) q = q.Where(x => p.Ids.Contains(x.Id));
		return await q.ToListAsync(ct);
	}
}