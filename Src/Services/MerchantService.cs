namespace SinaMN75U.Services;

public interface IMerchantService {
	Task<UResponse<Guid?>> Create(MerchantCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<MerchantResponse>?>> Read(MerchantReadParams p, CancellationToken ct);
	Task<UResponse<MerchantResponse?>> ReadById(IdParams<MerchantSelectorArgs> p, CancellationToken ct);
	Task<UResponse> Update(MerchantUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class MerchantService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IMediaService mediaService
) : IMerchantService {
	public async Task<UResponse<Guid?>> Create(MerchantCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		MerchantEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			UserId = p.UserId,
			ZipCode = p.ZipCode,
			JsonData = new MerchantJson {
				Detail1 = p.Detail1, 
				Detail2 = p.Detail2
			},
		};
		await db.Set<MerchantEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<MerchantResponse>?>> Read(MerchantReadParams p, CancellationToken ct) {
		IQueryable<MerchantEntity> q = db.Set<MerchantEntity>().ApplyReadParams<MerchantEntity, TagMerchant, MerchantJson>(p);
		
		if (p.UserId.IsNotNullOrEmpty()) q = q.Where(x => x.UserId == p.UserId); 
		if (p.ZipCode.IsNotNullOrEmpty()) q = q.Where(x => x.ZipCode == p.ZipCode);
		
		IQueryable<MerchantResponse> projected = q.Select(Projections.MerchantSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<MerchantResponse?>> ReadById(IdParams<MerchantSelectorArgs> p, CancellationToken ct) {
		MerchantResponse? e = await db.Set<MerchantEntity>().Select(Projections.MerchantSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<MerchantResponse?>(null, Usc.NotFound, ls.Get("MerchantNotFound")) : new UResponse<MerchantResponse?>(e);
	}

	public async Task<UResponse> Update(MerchantUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		MerchantEntity? e = await db.Set<MerchantEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("MerchantNotFound"));

		if (p.ZipCode != null) e.ZipCode = p.ZipCode;

		db.Set<MerchantEntity>().Update(e.ApplyUpdateParam<MerchantEntity,TagMerchant, MerchantJson>(p));
		await db.SaveChangesAsync(ct);
		
		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<MerchantEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}
}