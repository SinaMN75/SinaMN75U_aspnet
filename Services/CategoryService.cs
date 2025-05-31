namespace SinaMN75U.Services;

public interface ICategoryService {
	Task<UResponse<CategoryResponse?>> Create(CategoryCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<CategoryResponse>?>> Read(CategoryReadParams p, CancellationToken ct);
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
			JsonData = new CategoryJson { Subtitle = p.Subtitle },
			Tags = p.Tags,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		db.Set<CategoryEntity>().Add(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<CategoryResponse?>(e.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<CategoryResponse>?>> Read(CategoryReadParams p, CancellationToken ct) {
		IQueryable<CategoryEntity> q = db.Set<CategoryEntity>().OrderByDescending(x => x.Id);

		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags.Any(tag => p.Tags!.Contains(tag)));
		if (p.Ids.IsNotNullOrEmpty()) q = q.Where(x => p.Ids.Contains(x.Id));

		return await q.Select(x => new CategoryResponse {
			Id = x.Id,
			Title = x.Title,
			JsonData = x.JsonData,
			Tags = x.Tags,
			ParentId = x.ParentId,
			Children = x.Children!.Select(c1 => new CategoryResponse {
				Id = c1.Id,
				Title = c1.Title,
				JsonData = c1.JsonData,
				Tags = c1.Tags,
				ParentId = x.ParentId,
				Media = p.ShowMedia
					? x.Media!.Select(m => new MediaResponse {
						Path = m.Path,
						Id = m.Id,
						Tags = m.Tags,
						JsonData = m.JsonData
					})
					: null,
				Children = x.Children!.Select(c2 => new CategoryResponse {
					Id = c2.Id,
					Title = c2.Title,
					JsonData = c2.JsonData,
					ParentId = x.ParentId,
					Tags = c2.Tags,
					Media = p.ShowMedia
						? x.Media!.Select(m => new MediaResponse {
							Path = m.Path,
							Id = m.Id,
							Tags = m.Tags,
							JsonData = m.JsonData
						})
						: null,
					Children = x.Children!.Select(c3 => new CategoryResponse {
						Id = c3.Id,
						Title = c3.Title,
						JsonData = c3.JsonData,
						Tags = c3.Tags,
						ParentId = x.ParentId,
						Media = p.ShowMedia
							? x.Media!.Select(m => new MediaResponse {
								Path = m.Path,
								Id = m.Id,
								Tags = m.Tags,
								JsonData = m.JsonData
							})
							: null,
						Children = x.Children!.Select(c4 => new CategoryResponse {
							Id = c4.Id,
							Title = c4.Title,
							JsonData = c4.JsonData,
							Tags = c4.Tags,
							ParentId = x.ParentId,
							Media = p.ShowMedia
								? x.Media!.Select(m => new MediaResponse {
									Path = m.Path,
									Id = m.Id,
									Tags = m.Tags,
									JsonData = m.JsonData
								})
								: null,
							Children = x.Children!.Select(c5 => new CategoryResponse {
								Id = c5.Id,
								Title = c5.Title,
								JsonData = c5.JsonData,
								Tags = c5.Tags,
								ParentId = x.ParentId,
								Media = p.ShowMedia
									? x.Media!.Select(m => new MediaResponse {
										Path = m.Path,
										Id = m.Id,
										Tags = m.Tags,
										JsonData = m.JsonData
									})
									: null,
								Children = x.Children!.Select(c6 => new CategoryResponse {
									Id = c6.Id,
									Title = c6.Title,
									JsonData = c6.JsonData,
									Tags = c6.Tags,
									ParentId = x.ParentId,
									Media = p.ShowMedia
										? x.Media!.Select(m => new MediaResponse {
											Path = m.Path,
											Id = m.Id,
											Tags = m.Tags,
											JsonData = m.JsonData
										})
										: null,
									Children = x.Children!.Select(c7 => new CategoryResponse {
										Id = c7.Id,
										Title = c7.Title,
										JsonData = c7.JsonData,
										Tags = c7.Tags,
										ParentId = x.ParentId,
										Media = p.ShowMedia
											? x.Media!.Select(m => new MediaResponse {
												Path = m.Path,
												Id = m.Id,
												Tags = m.Tags,
												JsonData = m.JsonData
											})
											: null
									})
								})
							})
						})
					})
				})
			}),
			Media = p.ShowMedia
				? x.Media!.Select(m => new MediaResponse {
					Path = m.Path,
					Id = m.Id,
					Tags = m.Tags,
					JsonData = m.JsonData
				})
				: null
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<CategoryResponse?>> Update(CategoryUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<CategoryResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		CategoryEntity? e = await db.Set<CategoryEntity>().FindAsync(p.Id, ct);
		if (e == null)
			return new UResponse<CategoryResponse?>(null, Usc.NotFound, "Category not found");

		e.UpdatedAt = DateTime.UtcNow;
		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.Subtitle.IsNotNullOrEmpty()) e.JsonData.Subtitle = p.Subtitle;

		if (p.AddTags != null) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags != null) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));

		db.Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<CategoryResponse?>(e.MapToResponse());
	}

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