namespace SinaMN75U.Services;

public interface ISimCardService {
	Task<UResponse<Guid?>> Create(SimCardCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<SimCardResponse>?>> Read(SimCardReadParams p, CancellationToken ct);
	Task<UResponse> Update(SimCardUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct);
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
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			JsonData = new GeneralJsonData { Description = p.Description ?? "" },
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
		IQueryable<SimCardResponse> q = db.Set<SimCardEntity>().Select(Projections.SimCardSelector(p.SelectorArgs));

		if (p.OrderByCreatedAt) q = q.OrderBy(x => x.CreatedAt);
		if (p.OrderByCreatedAtDesc) q = q.OrderByDescending(x => x.CreatedAt);
		if (p.OrderByUpdatedAt) q = q.OrderBy(x => x.UpdatedAt);
		if (p.OrderByUpdatedAtDesc) q = q.OrderByDescending(x => x.UpdatedAt);
		if (p.CreatorId != null) q = q.Where(x => x.UserId == p.CreatorId);

		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => p.Tags.All(tag => x.Tags.Contains(tag)));
		if (p.Ids.IsNotNullOrEmpty()) q = q.Where(x => p.Ids.Contains(x.Id));

		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(SimCardUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		SimCardEntity? e = await db.Set<SimCardEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("SimCardNotFound"));

		if (p.Number != null) e.Number = p.Number;
		if (p.Serial != null) e.Serial = p.Serial;
		if (p.Description != null) e.JsonData.Description = p.Description;
		if (p.Tags != null) e.Tags = p.Tags;

		db.Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<SimCardEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<SimCardEntity>().Where(x => p.Id == x.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.DeletedAt, p.DateTime ?? DateTime.UtcNow), ct);
		return new UResponse();
	}
}