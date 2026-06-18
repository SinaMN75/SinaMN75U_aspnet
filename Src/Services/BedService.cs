namespace SinaMN75U.Services;

public interface IBedService {
	public Task<UResponse<Guid?>> Create(BedCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<BedResponse>?>> Read(BedReadParams p, CancellationToken ct);
	public Task<UResponse<BedResponse?>> ReadById(IdParams<BedSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> Update(BedUpdateParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class BedService(
	DbContext db,
	ITokenService ts,
	ILocalizationService ls
) : IBedService {
	public async Task<UResponse<Guid?>> Create(BedCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		BedEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson { Detail1 = p.Detail1, Detail2 = p.Detail2 },
			Tags = p.Tags,
			CreatorId = p.CreatorId ?? userData.Id,
			Deposit = p.Deposit,
			Rent = p.Rent,
			ParentId = p.ParentId
		};

		await db.Set<BedEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<BedResponse>?>> Read(BedReadParams p, CancellationToken ct) {
		IQueryable<BedEntity> q = db.Set<BedEntity>().Where(x => x.ParentId == null).ApplyReadParams<BedEntity, TagBed, BaseJson>(p);

		if (p.ParentId.HasValue) q = q.Where(x => x.ParentId == p.ParentId);

		UResponse<IEnumerable<BedResponse>?> list = await q.Select(Projections.BedSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
		return list;
	}

	public async Task<UResponse<BedResponse?>> ReadById(IdParams<BedSelectorArgs> p, CancellationToken ct) {
		BedResponse? e = await db.Set<BedEntity>().Select(Projections.BedSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<BedResponse?>(null, Usc.NotFound, ls.Get("MerchantNotFound")) : new UResponse<BedResponse?>(e);
	}

	public async Task<UResponse> Update(BedUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		BedEntity? e = await db.Set<BedEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ProductNotFound"));
		
		if (p.ParentId.IsNotNull()) e.ParentId = p.ParentId;

		db.Set<BedEntity>().Update(e.ApplyUpdateParam<BedEntity, TagBed, BaseJson>(p));
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		
		BedEntity? e = await db.Set<BedEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ProductNotFound"));
		
		if (!userData.IsAdmin && userData.Id != e.CreatorId) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		db.Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}
}