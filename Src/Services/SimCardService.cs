namespace SinaMN75U.Services;

public interface ISimCardService {
	Task<UResponse<Guid?>> Create(SimCardCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<SimCardResponse>?>> Read(SimCardReadParams p, CancellationToken ct);
	Task<UResponse> Update(SimCardUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class SimCardService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : ISimCardService {
	public async Task<UResponse<Guid?>> Create(SimCardCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		SimCardEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJsonData { Detail2 = p.Detail2, Detail1 = p.Detail1 },
			Tags = p.Tags,
			UserId = p.CreatorId ?? userData.Id,
			Number = p.Number,
			Serial = p.Serial
		};

		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<SimCardResponse>?>> Read(SimCardReadParams p, CancellationToken ct) {
		IQueryable<SimCardEntity> q = db.Set<SimCardEntity>().ApplyReadParams<SimCardEntity, TagSimCard, BaseJsonData>(p);
		IQueryable<SimCardResponse> projected = q.Select(Projections.SimCardSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(SimCardUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		SimCardEntity? e = await db.Set<SimCardEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("SimCardNotFound"));

		if (p.Number != null) e.Number = p.Number;
		if (p.Serial != null) e.Serial = p.Serial;

		db.Set<SimCardEntity>().Update(e.ApplyUpdateParam<SimCardEntity,TagSimCard, BaseJsonData>(p));
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<SimCardEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}
}