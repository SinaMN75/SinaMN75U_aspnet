namespace SinaMN75U.Services;

public interface ITxnService {
	Task<UResponse<Guid?>> Create(TxnCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<TxnResponse>?>> Read(TxnReadParams p, CancellationToken ct);
	Task<UResponse> Update(TxnUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class TxnService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : ITxnService {
	public async Task<UResponse<Guid?>> Create(TxnCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		TxnEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJsonData(),
			Tags = p.Tags,
			Amount = p.Amount,
			TrackingNumber = p.TrackingNumber,
			InvoiceId = p.InvoiceId,
			UserId = userData.Id
		};
		
		await db.Set<TxnEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<TxnResponse>?>> Read(TxnReadParams p, CancellationToken ct) {
		IQueryable<TxnEntity> q = db.Set<TxnEntity>().ApplyReadParams<TxnEntity, TagTxn, BaseJsonData>(p);
		IQueryable<TxnResponse> projected = q.Select(Projections.TxnSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(TxnUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		TxnEntity e = (await db.Set<TxnEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;

		db.Set<TxnEntity>().Update(e.ApplyUpdateParam<TxnEntity,TagTxn, BaseJsonData>(p));
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<TxnEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}
}