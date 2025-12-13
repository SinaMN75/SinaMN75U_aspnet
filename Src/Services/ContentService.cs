using SinaMN75U.Data;

namespace SinaMN75U.Services;

public interface IContentService {
	Task<UResponse<ContentResponse?>> Create(ContentCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ContentResponse>?>> Read(ContentReadParams p, CancellationToken ct);
	Task<UResponse<ContentResponse?>> Update(ContentUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct);
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
		IQueryable<ContentResponse> q = db.Set<ContentEntity>().SoftDeleteBehavior(p.SelectorArgs.SoftDeleteBehavior).Select(Projections.ContentSelector(p.SelectorArgs));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ContentResponse?>> Update(ContentUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ContentResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		ContentEntity? e = await db.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<ContentResponse?>(null, Usc.NotFound, ls.Get("ContentNotFound"));
		p.MapToEntity(e);
		db.Update(p.MapToEntity(e));
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

	public async Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<ContentEntity>().Where(x => p.Id == x.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.DeletedAt, p.DateTime ?? DateTime.UtcNow), ct);
		cache.DeleteAllByPartialKey(RouteTags.Content);
		return new UResponse();
	}
}