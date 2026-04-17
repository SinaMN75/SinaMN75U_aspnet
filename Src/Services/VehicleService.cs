namespace SinaMN75U.Services;

public interface IVehicleService {
	Task<UResponse<Guid?>> Create(VehicleCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<VehicleResponse>?>> Read(VehicleReadParams p, CancellationToken ct);
	Task<UResponse> Update(VehicleUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class VehicleService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IVehicleService {
	public async Task<UResponse<Guid?>> Create(VehicleCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		VehicleEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJsonData { Detail1 = p.Detail1, Detail2 = p.Detail2 },
			Tags = p.Tags,
			LicencePlate = p.LicencePlate,
			Brand = p.Brand,
			Color = p.Color,
			CreatorId = p.CreatorId ?? userData.Id,
			Title = p.Title,
		};
		
		await db.Set<VehicleEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, message: ls.Get("VehicleCreated"));
	}

	public async Task<UResponse<IEnumerable<VehicleResponse>?>> Read(VehicleReadParams p, CancellationToken ct) {
		IQueryable<VehicleEntity> q = db.Set<VehicleEntity>().ApplyReadParams<VehicleEntity, TagVehicle, BaseJsonData>(p);
		if (p.Brand.IsNotNullOrEmpty()) q = q.Where(x => x.Brand == p.Brand);
		if (p.LicencePlate.IsNotNullOrEmpty()) q = q.Where(x => x.LicencePlate == p.LicencePlate);
		if (p.Color.IsNotNullOrEmpty()) q = q.Where(x => x.Color == p.Color);
		
		IQueryable<VehicleResponse> projected = q.Select(Projections.VehicleSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(VehicleUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		VehicleEntity? e = await db.Set<VehicleEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("VehicleNotFound"));
		
		if (p.LicencePlate.IsNotNull()) e.LicencePlate = p.LicencePlate;
		if (p.Brand.IsNotNull()) e.Brand = p.Brand;
		if (p.Color.IsNotNull()) e.Color = p.Color;
		
		db.Set<VehicleEntity>().Update(e.ApplyUpdateParam<VehicleEntity,TagVehicle, BaseJsonData>(p));
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<VehicleEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}
}