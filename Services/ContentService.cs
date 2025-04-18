namespace SinaMN75U.Services;

public interface IContentService {
	Task<UResponse<ContentResponse?>> Create(ContentCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ContentResponse>?>> Read(ContentReadParams p, CancellationToken ct);
	Task<UResponse<ContentResponse?>> Update(ContentUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class ContentService(DbContext db, ILocalizationService ls, ITokenService ts) : IContentService {
	public async Task<UResponse<ContentResponse?>> Create(ContentCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ContentResponse?>(null, USC.UnAuthorized, ls.Get("AuthorizationRequired"));

		EntityEntry<ContentEntity> e = await db.AddAsync(new ContentEntity {
			Tags = p.Tags,
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			JsonData = new ContentJson {
				Instagram = p.Instagram,
				Description = p.Description,
				Title = p.Title,
				SubTitle = p.SubTitle
			}
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<ContentResponse?>(e.Entity.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<ContentResponse>?>> Read(ContentReadParams p, CancellationToken ct) {
		IQueryable<ContentEntity> q = db.Set<ContentEntity>();
		
		if (p.Tags != null) q = q.Where(u => u.Tags.Any(tag => p.Tags.Contains(tag)));
		
		return await q.Select(x => new ContentResponse {
			Id = x.Id,
			Tags = x.Tags,
			Json = x.JsonData,
			Media = p.ShowMedia ? x.Media!.Select(m => new MediaResponse { Path = m.Path, Id = m.Id, Tags = m.Tags, Json = m.JsonData}) : null
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ContentResponse?>> Update(ContentUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ContentResponse?>(null, USC.UnAuthorized, ls.Get("AuthorizationRequired"));

		ContentEntity e = (await db.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (p.Title.IsNotNullOrEmpty()) e.JsonData.Title = p.Title;
		if (p.SubTitle.IsNotNullOrEmpty()) e.JsonData.SubTitle = p.SubTitle;
		if (p.Description.IsNotNullOrEmpty()) e.JsonData.Description = p.Description;
		if (p.Instagram.IsNotNullOrEmpty()) e.JsonData.Instagram = p.Instagram;

		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));

		db.Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<ContentResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ContentResponse?>(null, USC.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<ContentEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync();
		return new UResponse();
	}
}