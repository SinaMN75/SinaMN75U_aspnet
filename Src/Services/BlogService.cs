namespace SinaMN75U.Services;

public interface IBlogService {
	public Task<UResponse<Guid?>> Create(BlogCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<BlogResponse>?>> Read(BlogReadParams p, CancellationToken ct);
	public Task<UResponse<BlogResponse?>> ReadById(IdParams<BlogSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> Update(BlogUpdateParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdParams p, CancellationToken ct);
	public Task<UResponse> DeleteRange(IdListParams p, CancellationToken ct);
	public Task<UResponse> Publish(IdParams p, CancellationToken ct);
	public Task<UResponse> Unpublish(IdParams p, CancellationToken ct);
}

public class BlogService(
	DbContext db,
	ITokenService ts,
	ILocalizationService ls,
	IMediaService mediaService
) : IBlogService {
	public async Task<UResponse<Guid?>> Create(BlogCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		List<CategoryEntity> categories = p.Categories.IsNotNullOrEmpty()
			? await db.Set<CategoryEntity>().AsTracking().Where(x => p.Categories.Contains(x.Id)).OrderByDescending(x => x.Id).ToListAsync(ct)
			: [];

		BlogEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			Title = p.Title,
			Subtitle = p.Subtitle,
			Slug = p.Slug == "" ? null : p.Slug,
			Content = p.Content,
			PublishedAt = p.PublishedAt,
			Categories = categories,
			AdminUserIds = p.AdminUserIds ?? [],
			JsonData = new BlogJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				MetaTitle = p.MetaTitle,
				MetaDescription = p.MetaDescription,
				Source = p.Source,
				ReadingTimeMinutes = p.ReadingTimeMinutes
			}
		};

		await db.Set<BlogEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);

		await AddMedia(e.Id, p.Media ?? [], ct);

		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<BlogResponse>?>> Read(BlogReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<BlogEntity> q = db.Set<BlogEntity>().ApplyReadParams(p).ApplyAdminScope<BlogEntity, TagBlog>(userData);

		if (p.Query.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Query!) || (x.Subtitle ?? "").Contains(p.Query!) || (x.Content ?? "").Contains(p.Query!));
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));
		if (p.Slug.IsNotNullOrEmpty()) q = q.Where(x => (x.Slug ?? "") == p.Slug!);
		if (p.OnlyPublished == true) q = q.Where(x => x.PublishedAt != null && x.PublishedAt <= DateTime.UtcNow);
		if (p.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories.Any(y => p.Categories.Contains(y.Id)));

		IQueryable<BlogResponse> projected = q.Select(Projections.BlogSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<BlogResponse?>> ReadById(IdParams<BlogSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		BlogResponse? e = await db.Set<BlogEntity>().ApplyAdminScope<BlogEntity, TagBlog>(userData).Select(Projections.BlogSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<BlogResponse?>(null, Usc.NotFound, ls.Get("BlogNotFound"));

		await db.Set<BlogEntity>().Where(x => x.Id == p.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.ViewCount, x => x.ViewCount + 1), ct);

		return new UResponse<BlogResponse?>(e);
	}

	public async Task<UResponse> Update(BlogUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		BlogEntity? e = await db.Set<BlogEntity>().AsTracking().Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("BlogNotFound"));

		if (!userData.CanManage(e.CreatorId, e.AdminUserIds)) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.Subtitle.IsNotNull()) e.Subtitle = p.Subtitle;
		if (p.Slug.IsNotNull()) e.Slug = p.Slug;
		if (p.Content.IsNotNull()) e.Content = p.Content;
		if (p.PublishedAt.HasValue) e.PublishedAt = p.PublishedAt;
		if (p.MetaTitle.IsNotNull()) e.JsonData.MetaTitle = p.MetaTitle;
		if (p.MetaDescription.IsNotNull()) e.JsonData.MetaDescription = p.MetaDescription;
		if (p.Source.IsNotNull()) e.JsonData.Source = p.Source;
		if (p.ReadingTimeMinutes.IsNotNull()) e.JsonData.ReadingTimeMinutes = p.ReadingTimeMinutes;

		if (p.AddCategories.IsNotNullOrEmpty()) e.Categories.AddRangeIfNotExist(await db.Set<CategoryEntity>().AsTracking().Where(x => p.AddCategories.Contains(x.Id)).OrderByDescending(x => x.Id).ToListAsync(ct));
		if (p.RemoveCategories.IsNotNullOrEmpty()) e.Categories.RemoveRangeIfExist(await db.Set<CategoryEntity>().AsTracking().Where(x => p.RemoveCategories.Contains(x.Id)).OrderByDescending(x => x.Id).ToListAsync(ct));

		if (p.Categories.IsNotNull()) {
			if (p.Categories.Count == 0) e.Categories = [];
			else e.Categories = await db.Set<CategoryEntity>().AsTracking().Where(x => p.Categories.Contains(x.Id)).OrderByDescending(x => x.Id).ToListAsync(ct);
		}

		db.Set<BlogEntity>().Update(e.ApplyUpdateParam<BlogEntity, TagBlog, BlogJson>(p));
		await db.SaveChangesAsync(ct);
		await AddMedia(p.Id, p.Media ?? [], ct);

		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		BlogEntity? e = await db.Set<BlogEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("BlogNotFound"));

		if (!userData.CanManage(e.CreatorId, e.AdminUserIds)) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		db.Set<BlogEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse(Usc.Deleted, ls.Get("BlogDeleted"));
	}

	public async Task<UResponse> DeleteRange(IdListParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		int count = await db.Set<BlogEntity>().WhereIn(u => u.Id, p.Ids).ExecuteDeleteAsync(ct);

		return count > 0 ? new UResponse(Usc.Deleted, ls.Get("BlogDeleted")) : new UResponse(Usc.NotFound, ls.Get("BlogNotFound"));
	}

	public async Task<UResponse> Publish(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		BlogEntity? e = await db.Set<BlogEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("BlogNotFound"));
		if (!userData.CanManage(e.CreatorId, e.AdminUserIds)) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		e.PublishedAt = DateTime.UtcNow;
		e.Tags = [TagBlog.Published];
		db.Update(e);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> Unpublish(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		BlogEntity? e = await db.Set<BlogEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("BlogNotFound"));
		if (!userData.CanManage(e.CreatorId, e.AdminUserIds)) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		e.PublishedAt = null;
		e.Tags = [TagBlog.Draft];
		db.Update(e);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	private async Task AddMedia(Guid blogId, ICollection<Guid> ids, CancellationToken ct) {
		if (ids.IsNullOrEmpty()) return;
		List<MediaEntity> media = await mediaService.ReadEntity(new BaseReadParams<TagMedia> { Ids = ids }, ct) ?? [];
		if (media.Count == 0) return;
		foreach (MediaEntity i in media)
			await db.Set<MediaEntity>().Where(x => x.Id == i.Id).ExecuteUpdateAsync(
				u => u.SetProperty(y => y.BlogId, blogId),
				ct
			);
	}
}
