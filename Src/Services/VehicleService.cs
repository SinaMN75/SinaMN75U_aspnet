namespace SinaMN75U.Services;

public interface IVehicleService {
	Task<UResponse<VehicleResponse?>> Create(VehicleCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<VehicleResponse>?>> Read(VehicleReadParams p, CancellationToken ct);
	Task<UResponse<VehicleResponse?>> Update(VehicleUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct);
}

public class VehicleService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	ILocalStorageService cache
) : IVehicleService {
	public async Task<UResponse<VehicleResponse?>> Create(VehicleCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<VehicleResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		EntityEntry<VehicleEntity> e = await db.AddAsync(p.MapToEntity(), ct);

		cache.DeleteAllByPartialKey(RouteTags.Vehicle);
		await db.SaveChangesAsync(ct);
		return new UResponse<VehicleResponse?>(e.Entity.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<VehicleResponse>?>> Read(VehicleReadParams p, CancellationToken ct) {
		IQueryable<VehicleResponse> q = db.Set<VehicleEntity>().Select(Projections.VehicleSelector(p.SelectorArgs));
		if (p.Brand.IsNotNullOrEmpty()) q = q.Where(x => x.Brand == p.Brand);
		if (p.NumberPlate.IsNotNullOrEmpty()) q = q.Where(x => x.NumberPlate == p.NumberPlate);
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title == p.Title);
		if (p.Color.IsNotNullOrEmpty()) q = q.Where(x => x.Color == p.Color);
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<VehicleResponse?>> Update(VehicleUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<VehicleResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		VehicleEntity? e = await db.Set<VehicleEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<VehicleResponse?>(null, Usc.NotFound, ls.Get("VehicleNotFound"));
		p.MapToEntity(e);
		db.Update(p.MapToEntity(e));
		await db.SaveChangesAsync(ct);
		cache.DeleteAllByPartialKey(RouteTags.Vehicle);
		return new UResponse<VehicleResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<VehicleEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<VehicleEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Vehicle);
		return new UResponse();
	}

	public async Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<VehicleEntity>().Where(x => p.Id == x.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.DeletedAt, p.DateTime ?? DateTime.UtcNow), ct);
		cache.DeleteAllByPartialKey(RouteTags.Vehicle);
		return new UResponse();
	}
}