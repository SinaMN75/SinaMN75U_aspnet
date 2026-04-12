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
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			JsonData = new TxnJson {
				GatewayName = p.GatewayName
			},
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
		IQueryable<TxnResponse> q = db.Set<TxnEntity>().Select(Projections.TxnSelector(p.SelectorArgs));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(TxnUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		TxnEntity e = (await db.Set<TxnEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;

		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));

		db.Update(e);
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