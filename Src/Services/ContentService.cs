using SinaMN75U.Data;

namespace SinaMN75U.Services;

public interface IContentService {
	Task<UResponse<ContentResponse?>> Create(ContentCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ContentResponse>?>> Read(ContentReadParams p, CancellationToken ct);
	Task<UResponse<ContentResponse?>> Update(ContentUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class ContentService(
	DbContext db, 
	ILocalizationService ls,
	ITokenService ts,
	ILocalStorageService cache
	) : IContentService {
	public async Task<UResponse<ContentResponse?>> Create(ContentCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ContentResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		EntityEntry<ContentEntity> e = await db.AddAsync(p.MapToEntity(), ct);
		
		cache.DeleteAllByPartialKey(RouteTags.Content);
		await db.SaveChangesAsync(ct);
		return new UResponse<ContentResponse?>(e.Entity.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<ContentResponse>?>> Read(ContentReadParams p, CancellationToken ct) {
		IQueryable<ContentResponse> q = db.Set<ContentEntity>().Select(Projections.ContentSelector(new ContentSelectorArgs {ShowMedia = true}));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ContentResponse?>> Update(ContentUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ContentResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		ContentEntity e = (await db.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (p.Title.HasValue()) e.JsonData.Title = p.Title;
		if (p.SubTitle.HasValue()) e.JsonData.SubTitle = p.SubTitle;
		if (p.Description.HasValue()) e.JsonData.Description = p.Description;
		if (p.Instagram.HasValue()) e.JsonData.Instagram = p.Instagram;
		if (p.Phone.HasValue()) e.JsonData.Phone = p.Phone;
		if (p.Telegram.HasValue()) e.JsonData.Telegram = p.Telegram;
		if (p.Whatsapp.HasValue()) e.JsonData.Whatsapp = p.Whatsapp;

		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));

		db.Update(e);
		await db.SaveChangesAsync(ct);
		
		cache.DeleteAllByPartialKey(RouteTags.Content);
		return new UResponse<ContentResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ContentEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<ContentEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);
		
		cache.DeleteAllByPartialKey(RouteTags.Content);
		return new UResponse();
	}
}