namespace SinaMN75U.Services;

public interface IContentService {
	Task<UResponse<Guid?>> Create(ContentCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ContentResponse>?>> Read(ContentReadParams p, CancellationToken ct);
	Task<UResponse> Update(ContentUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class ContentService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IContentService {
	public async Task<UResponse<Guid?>> Create(ContentCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ContentEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new ContentJson {
				Title = p.Title,
				SubTitle = p.SubTitle,
				Description = p.Description,
				Instagram = p.Instagram,
				Telegram = p.Telegram,
				Whatsapp = p.Whatsapp,
				Phone = p.Phone
			},
			Tags = p.Tags
		};
		
		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<ContentResponse>?>> Read(ContentReadParams p, CancellationToken ct) {
		IQueryable<ContentResponse> q = db.Set<ContentEntity>().Select(Projections.ContentSelector(p.SelectorArgs));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(ContentUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ContentEntity? e = await db.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ContentNotFound"));
		
		if (p.Title != null) e.JsonData.Title = p.Title;
		if (p.SubTitle != null) e.JsonData.SubTitle = p.SubTitle;
		if (p.Description != null) e.JsonData.Description = p.Description;
		if (p.Instagram != null) e.JsonData.Instagram = p.Instagram;
		if (p.Telegram != null) e.JsonData.Telegram = p.Telegram;
		if (p.Whatsapp != null) e.JsonData.Whatsapp = p.Whatsapp;
		if (p.Phone != null) e.JsonData.Phone = p.Phone;
		if (p.Tags != null) e.Tags = p.Tags;
		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));
		
		db.Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<ContentEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}
}