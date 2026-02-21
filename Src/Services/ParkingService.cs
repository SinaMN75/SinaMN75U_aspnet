namespace SinaMN75U.Services;

public interface IParkingService {
	Task<UResponse<ParkingResponse?>> CreateParking(ParkingCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ParkingResponse>?>> ReadParking(ParkingReadParams p, CancellationToken ct);
	Task<UResponse<ParkingResponse?>> UpdateParking(ParkingUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteParking(IdParams p, CancellationToken ct);
	Task<UResponse> SoftDeleteParking(SoftDeleteParams p, CancellationToken ct);
	
	Task<UResponse<ParkingReportResponse?>> CreateParkingReport(ParkingReportCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ParkingReportResponse>?>> ReadParkingReport(ParkingReportReadParams p, CancellationToken ct);
	Task<UResponse<ParkingReportResponse?>> UpdateParkingReport(ParkingReportUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteParkingReport(IdParams p, CancellationToken ct);
	Task<UResponse> SoftDeleteParkingReport(SoftDeleteParams p, CancellationToken ct);
}

public class ParkingService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	ILocalStorageService cache
) : IParkingService {
	public async Task<UResponse<ParkingResponse?>> CreateParking(ParkingCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		EntityEntry<ParkingEntity> e = await db.AddAsync(p.MapToEntity(), ct);

		cache.DeleteAllByPartialKey(RouteTags.Parking);
		await db.SaveChangesAsync(ct);
		return new UResponse<ParkingResponse?>(e.Entity.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<ParkingResponse>?>> ReadParking(ParkingReadParams p, CancellationToken ct) {
		IQueryable<ParkingResponse> q = db.Set<ParkingEntity>().Select(Projections.ParkingSelector(p.SelectorArgs));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ParkingResponse?>> UpdateParking(ParkingUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		ParkingEntity? e = await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<ParkingResponse?>(null, Usc.NotFound, ls.Get("ParkingNotFound"));
		p.MapToEntity(e);
		db.Update(p.MapToEntity(e));
		await db.SaveChangesAsync(ct);
		cache.DeleteAllByPartialKey(RouteTags.Parking);
		return new UResponse<ParkingResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> DeleteParking(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<ParkingEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Parking);
		return new UResponse();
	}

	public async Task<UResponse> SoftDeleteParking(SoftDeleteParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<ParkingEntity>().Where(x => p.Id == x.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.DeletedAt, p.DateTime ?? DateTime.UtcNow), ct);
		cache.DeleteAllByPartialKey(RouteTags.Parking);
		return new UResponse();
	}
	public async Task<UResponse<ParkingReportResponse?>> CreateParkingReport(ParkingReportCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingReportResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));
		
		VehicleEntity? vehicle = await db.Set<VehicleEntity>().FirstOrDefaultAsync(x => x.NumberPlate == p.NumberPlate, ct);
		if (vehicle == null) {
			EntityEntry<VehicleEntity> vEntity = await db.Set<VehicleEntity>().AddAsync(new VehicleEntity {
				JsonData = new VehicleJson(),
				Tags = [TagVehicle.Test],
				NumberPlate = p.NumberPlate
			}, ct);
			vehicle = vEntity.Entity;
		}

		EntityEntry<ParkingReportEntity> e = await db.AddAsync(new ParkingReportEntity {
			StartDate = p.StartDate,
			CreatorId = p.CreatorId ?? userData.Id,
			VehicleId = vehicle.Id,
			ParkingId = p.ParkingId,
			JsonData = new ParkingReportJson(),
			Tags = [TagParkingReport.Test]
		}, ct);

		cache.DeleteAllByPartialKey(RouteTags.Parking);
		await db.SaveChangesAsync(ct);
		return new UResponse<ParkingReportResponse?>(e.Entity.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<ParkingReportResponse>?>> ReadParkingReport(ParkingReportReadParams p, CancellationToken ct) {
		IQueryable<ParkingReportResponse> q = db.Set<ParkingReportEntity>().Select(Projections.ParkingReportSelector(p.SelectorArgs));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ParkingReportResponse?>> UpdateParkingReport(ParkingReportUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingReportResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		ParkingReportEntity? e = await db.Set<ParkingReportEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<ParkingReportResponse?>(null, Usc.NotFound, ls.Get("ParkingReportNotFound"));
		p.MapToEntity(e);
		db.Update(p.MapToEntity(e));
		await db.SaveChangesAsync(ct);
		cache.DeleteAllByPartialKey(RouteTags.Parking);
		return new UResponse<ParkingReportResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> DeleteParkingReport(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingReportEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<ParkingReportEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Parking);
		return new UResponse();
	}

	public async Task<UResponse> SoftDeleteParkingReport(SoftDeleteParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<ParkingReportEntity>().Where(x => p.Id == x.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.DeletedAt, p.DateTime ?? DateTime.UtcNow), ct);
		cache.DeleteAllByPartialKey(RouteTags.Parking);
		return new UResponse();
	}
}