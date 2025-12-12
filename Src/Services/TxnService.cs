using SinaMN75U.Data;

namespace SinaMN75U.Services;

public interface ITxnService {
	Task<UResponse<TxnResponse?>> Create(TxnCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<TxnResponse>?>> Read(TxnReadParams p, CancellationToken ct);
	Task<UResponse<TxnResponse?>> Update(TxnUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class TxnService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	ILocalStorageService cache
) : ITxnService {
	public async Task<UResponse<TxnResponse?>> Create(TxnCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TxnResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		EntityEntry<TxnEntity> e = await db.AddAsync(p.MapToEntity(userData.Id), ct);

		cache.DeleteAllByPartialKey(RouteTags.Txn);
		await db.SaveChangesAsync(ct);
		return new UResponse<TxnResponse?>(e.Entity.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<TxnResponse>?>> Read(TxnReadParams p, CancellationToken ct) {
		IQueryable<TxnResponse> q = db.Set<TxnEntity>().Select(Projections.TxnSelector(p.SelectorArgs));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<TxnResponse?>> Update(TxnUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TxnResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		TxnEntity e = (await db.Set<TxnEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;

		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));

		db.Update(e);
		await db.SaveChangesAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Txn);
		return new UResponse<TxnResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TxnEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<TxnEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Txn);
		return new UResponse();
	}
}