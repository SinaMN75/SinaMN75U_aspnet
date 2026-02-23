namespace SinaMN75U.Services;

public interface IAddressService {
	Task<UResponse<AddressResponse?>> Create(AddressCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<AddressResponse>?>> Read(AddressReadParams p, CancellationToken ct);
	Task<UResponse<AddressResponse?>> Update(AddressUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct);
}

public class AddressService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IAddressService {
	public async Task<UResponse<AddressResponse?>> Create(AddressCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<AddressResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		EntityEntry<AddressEntity> e = await db.AddAsync(p.MapToEntity(), ct);

		await db.SaveChangesAsync(ct);
		return new UResponse<AddressResponse?>(e.Entity.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<AddressResponse>?>> Read(AddressReadParams p, CancellationToken ct) {
		IQueryable<AddressResponse> q = db.Set<AddressEntity>().Select(Projections.AddressSelector(p.SelectorArgs));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<AddressResponse?>> Update(AddressUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<AddressResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		AddressEntity? e = await db.Set<AddressEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<AddressResponse?>(null, Usc.NotFound, ls.Get("AddressNotFound"));
		p.MapToEntity(e);
		db.Update(p.MapToEntity(e));
		await db.SaveChangesAsync(ct);
		return new UResponse<AddressResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<AddressEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<AddressEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<AddressEntity>().Where(x => p.Id == x.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.DeletedAt, p.DateTime ?? DateTime.UtcNow), ct);
		return new UResponse();
	}
}