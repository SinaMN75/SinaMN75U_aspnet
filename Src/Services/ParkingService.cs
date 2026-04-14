namespace SinaMN75U.Services;

public interface IParkingService {
	Task<UResponse<Guid?>> CreateParking(ParkingCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ParkingResponse>?>> ReadParking(ParkingReadParams p, CancellationToken ct);
	Task<UResponse> UpdateParking(ParkingUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteParking(IdParams p, CancellationToken ct);

	Task<UResponse<Guid?>> CreateParkingReport(ParkingReportCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ParkingReportResponse>?>> ReadParkingReport(ParkingReportReadParams p, CancellationToken ct);
	Task<UResponse> UpdateParkingReport(ParkingReportUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteParkingReport(IdParams p, CancellationToken ct);
}

public class ParkingService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IParkingService {
	public async Task<UResponse<Guid?>> CreateParking(ParkingCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new GeneralJsonData { Title = p.Title },
			Tags = p.Tags,
			Title = p.Title,
			CreatorId = p.CreatorId ?? userData.Id,
			EntrancePrice = p.EntrancePrice,
			HourlyPrice = p.HourlyPrice,
			DailyPrice = p.DailyPrice
		};
		await db.Set<ParkingEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<ParkingResponse>?>> ReadParking(ParkingReadParams p, CancellationToken ct) {
		IQueryable<ParkingResponse> q = db.Set<ParkingEntity>().Select(Projections.ParkingSelector(p.SelectorArgs));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateParking(ParkingUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingEntity? e = await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ParkingNotFound"));
		
		if (p.Title.IsNotNull()) e.JsonData.Title = p.Title;
		if (p.EntrancePrice.IsNotNull()) e.EntrancePrice = p.EntrancePrice.Value;
		if (p.HourlyPrice.IsNotNull()) e.HourlyPrice = p.HourlyPrice.Value;
		if (p.DailyPrice.IsNotNull()) e.DailyPrice = p.DailyPrice.Value;
		if (p.Tags.IsNotNull()) e.Tags = p.Tags;
		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));
		db.Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<ParkingResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> DeleteParking(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<ParkingEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}
	
	public async Task<UResponse<Guid?>> CreateParkingReport(ParkingReportCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		VehicleEntity? vehicle = await db.Set<VehicleEntity>().FirstOrDefaultAsync(x => x.LicencePlate == p.NumberPlate, ct);
		if (vehicle == null) {
			EntityEntry<VehicleEntity> vEntity = await db.Set<VehicleEntity>().AddAsync(new VehicleEntity {
				Id = p.Id ?? Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				JsonData = new GeneralJsonData(),
				Tags = [TagVehicle.Test],
				LicencePlate = p.NumberPlate,
				CreatorId = p.CreatorId ?? userData.Id
			}, ct);
			vehicle = vEntity.Entity;
		}

		ParkingReportEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			StartDate = p.StartDate,
			CreatorId = p.CreatorId ?? userData.Id,
			VehicleId = vehicle.Id,
			ParkingId = p.ParkingId,
			JsonData = new GeneralJsonData(),
			Tags = [TagParkingReport.Test]
		};
		await db.Set<ParkingReportEntity>().AddAsync(e, ct);

		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<ParkingReportResponse>?>> ReadParkingReport(ParkingReportReadParams p, CancellationToken ct) {
		IQueryable<ParkingReportResponse> q = db.Set<ParkingReportEntity>().Select(Projections.ParkingReportSelector(p.SelectorArgs));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateParkingReport(ParkingReportUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingReportEntity? e = await db.Set<ParkingReportEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ParkingReportNotFound"));
		
		if (p.CreatorId.IsNotNull()) e.CreatorId = p.CreatorId.Value;
		if (p.VehicleId.IsNotNull()) e.VehicleId = p.VehicleId.Value;
		if (p.ParkingId.IsNotNull()) e.ParkingId = p.ParkingId.Value;
		if (p.StartDate != null) e.StartDate = p.StartDate.Value;
		if (p.EndDate != null) e.EndDate = p.EndDate;
		if (p.Amount.IsNotNull()) e.Amount = p.Amount.Value;
		if (p.Title.IsNotNull()) e.JsonData.Title = p.Title;
		if (p.Tags.IsNotNull()) e.Tags = p.Tags;
		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));
		db.Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteParkingReport(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<ParkingReportEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}
}