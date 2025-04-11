namespace SinaMN75U.Services;

public interface ICategoryService {
	Task<UResponse<CategoryResponse?>> Create(CategoryCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<CategoryResponse>?>> Read(CategoryReadParams p, CancellationToken ct);
	Task<UResponse<CategoryResponse?>> Update(CategoryUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> DeleteRange(IEnumerable<Guid> p, CancellationToken ct);
}

public class CategoryService(
	DbContext db,
	IMediaService mediaService,
	ILocalizationService ls,
	ITokenService ts
) : ICategoryService {
	public async Task<UResponse<CategoryResponse?>> Create(CategoryCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<CategoryResponse?>(null, USC.UnAuthorized, ls.Get("AuthorizationRequired"));
		CategoryEntity e = new() {
			Id = Guid.CreateVersion7(),
			Title = p.Title,
			JsonDetail = new CategoryJsonDetail { Subtitle = p.Subtitle },
			Tags = p.Tags,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		db.Set<CategoryEntity>().Add(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<CategoryResponse?>(e.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<CategoryResponse>?>> Read(CategoryReadParams p, CancellationToken ct) {
		IQueryable<CategoryEntity> qe = db.Set<CategoryEntity>().OrderByDescending(x => x.Id);

		if (p.Tags.IsNotNullOrEmpty()) qe = qe.Where(x => x.Tags.Any(tag => p.Tags!.Contains(tag)));
		if (p.Ids.IsNotNullOrEmpty()) qe = qe.Where(x => p.Ids.Contains(x.Id));
		if (p.ShowChildren) qe = qe.Include(x => x.Children);
		if (p.ShowMedia) qe = qe.Include(x => x.Media)
			.Include(x => x.Children!).ThenInclude(x => x.Media);

		IQueryable<CategoryResponse> qr = qe.ToResponse(p.ShowMedia, p.ShowChildren);

		return await qr.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<CategoryResponse?>> Update(CategoryUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<CategoryResponse?>(null, USC.UnAuthorized, ls.Get("AuthorizationRequired"));
		CategoryEntity? e = await db.Set<CategoryEntity>().FindAsync(p.Id, ct);
		if (e == null)
			return new UResponse<CategoryResponse?>(null, USC.NotFound, "Category not found");

		e.UpdatedAt = DateTime.UtcNow;
		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.Subtitle.IsNotNullOrEmpty()) e.JsonDetail.Subtitle = p.Subtitle;

		if (p.AddTags != null) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags != null) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));

		await db.SaveChangesAsync(ct);
		return new UResponse<CategoryResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null)
			return new UResponse<CategoryResponse?>(null, USC.UnAuthorized, ls.Get("AuthorizationRequired"));

		CategoryEntity? category = await db.Set<CategoryEntity>()
			.Include(x => x.Media)
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);

		if (category == null)
			return new UResponse(USC.NotFound, ls.Get("CategoryNotFound"));

		if (category.Media.IsNotNullOrEmpty())
			await mediaService.DeleteRange(category.Media.Select(x => x.Id), ct);

		db.Set<CategoryEntity>().Remove(category);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteRange(IEnumerable<Guid> p, CancellationToken ct) {
		await db.Set<CategoryEntity>().WhereIn(u => u.Id, p).ExecuteDeleteAsync(ct);
		return new UResponse();
	}
}