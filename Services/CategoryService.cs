namespace SinaMN75U.Services;

public interface ICategoryService {
	Task<UResponse<CategoryResponse>> Create(CategoryCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<CategoryResponse>>> Read(CancellationToken ct);
	Task<UResponse<CategoryResponse?>> Update(CategoryUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> DeleteRange(IEnumerable<Guid> idList, CancellationToken ct);
}

public class CategoryService(DbContext db, ILocalizationService ls, ITokenService ts) : ICategoryService {
	public async Task<UResponse<CategoryResponse>> Create(CategoryCreateParams p, CancellationToken ct) {
		CategoryEntity e = new() {
			Id = Guid.CreateVersion7(),
			Title = p.Title,
			TitleTr1 = p.TitleTr1,
			TitleTr2 = p.TitleTr2,
			JsonDetail = new CategoryJsonDetail { Subtitle = p.Subtitle },
			Tags = p.Tags,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		db.Set<CategoryEntity>().Add(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<CategoryResponse>(e.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<CategoryResponse>>> Read(CancellationToken ct) {
		List<CategoryResponse> categories = await db.Set<CategoryEntity>()
			.Include(i => i.Children)
			.AsNoTracking()
			.Select(i => i.MapToResponse())
			.ToListAsync(ct);

		return new UResponse<IEnumerable<CategoryResponse>>(categories);
	}

	public async Task<UResponse<CategoryResponse?>> Update(CategoryUpdateParams p, CancellationToken ct) {
		CategoryEntity? e = await db.Set<CategoryEntity>()
			.Include(i => i.Tags)
			.FirstOrDefaultAsync(i => i.Id == p.Id);

		if (e == null)
			return new UResponse<CategoryResponse?>(null, USC.NotFound, "Category not found");

		if (!string.IsNullOrEmpty(p.Title)) e.Title = p.Title;
		if (!string.IsNullOrEmpty(p.TitleTr1)) e.TitleTr1 = p.TitleTr1;
		if (!string.IsNullOrEmpty(p.TitleTr2)) e.TitleTr2 = p.TitleTr2;
		if (!string.IsNullOrEmpty(p.Subtitle)) e.JsonDetail.Subtitle = p.Subtitle;

		if (p.AddTags != null) e.Tags.AddRange(p.AddTags);
		if (p.RemoveTags != null) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));

		await db.SaveChangesAsync(ct);
		return new UResponse<CategoryResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		CategoryEntity? category = await db.Set<CategoryEntity>().FindAsync(p.Id, ct);
		if (category == null)
			return new UResponse(USC.NotFound, ls.Get("CategoryNotFound"));

		db.Set<CategoryEntity>().Remove(category);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteRange(IEnumerable<Guid> idList, CancellationToken ct) {
		await db.Set<CategoryEntity>().Where(x => idList.Contains(x.Id)).ExecuteDeleteAsync();
		return new UResponse();
	}
}