namespace SinaMN75U.Services;

public interface IContentService {
	Task<UResponse<ContentEntity?>> Create(ContentCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ContentEntity>?>> Read(ContentReadParams p, CancellationToken ct);
	Task<UResponse<ContentEntity?>> Update(ContentUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class ContentService(
	DbContext db, 
	ILocalizationService ls,
	ITokenService ts,
	ILocalStorageService cache
	) : IContentService {
	public async Task<UResponse<ContentEntity?>> Create(ContentCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ContentEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		EntityEntry<ContentEntity> e = await db.AddAsync(new ContentEntity {
			Tags = p.Tags,
			JsonData = new ContentJson {
				Description = p.Description,
				Title = p.Title,
				SubTitle = p.SubTitle,
				Instagram = p.Instagram,
				Phone = p.Phone,
				Telegram = p.Telegram,
				Whatsapp = p.Whatsapp
			}
		}, ct);
		
		cache.DeleteAllByPartialKey(RouteTags.Content);
		await db.SaveChangesAsync(ct);
		return new UResponse<ContentEntity?>(e.Entity);
	}

	public async Task<UResponse<IEnumerable<ContentEntity>?>> Read(ContentReadParams p, CancellationToken ct) {
		IQueryable<ContentEntity> q = db.Set<ContentEntity>().Include(x => x.Media);
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ContentEntity?>> Update(ContentUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ContentEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		ContentEntity e = (await db.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (p.Title.IsNotNullOrEmpty()) e.JsonData.Title = p.Title;
		if (p.SubTitle.IsNotNullOrEmpty()) e.JsonData.SubTitle = p.SubTitle;
		if (p.Description.IsNotNullOrEmpty()) e.JsonData.Description = p.Description;
		if (p.Instagram.IsNotNullOrEmpty()) e.JsonData.Instagram = p.Instagram;
		if (p.Phone.IsNotNullOrEmpty()) e.JsonData.Phone = p.Phone;
		if (p.Telegram.IsNotNullOrEmpty()) e.JsonData.Telegram = p.Telegram;
		if (p.Whatsapp.IsNotNullOrEmpty()) e.JsonData.Whatsapp = p.Whatsapp;

		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));

		db.Update(e);
		await db.SaveChangesAsync(ct);
		
		cache.DeleteAllByPartialKey(RouteTags.Content);
		return new UResponse<ContentEntity?>(e);
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ContentEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<ContentEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);
		
		cache.DeleteAllByPartialKey(RouteTags.Content);
		return new UResponse();
	}
}