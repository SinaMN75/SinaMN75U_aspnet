namespace SinaMN75U.Services;

public interface IParkingService {
	Task<UResponse<Guid?>> CreateParking(ParkingCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ParkingResponse>?>> ReadParking(ParkingReadParams p, CancellationToken ct);
	Task<UResponse> UpdateParking(ParkingUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteParking(IdParams p, CancellationToken ct);

	Task<UResponse<Guid?>> CreateParkingUser(ParkingUserCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<UserResponse>?>> ReadParkingUsers(ParkingUserReadParams p, CancellationToken ct);
	Task<UResponse> RemoveParkingUser(ParkingUserDeleteParams p, CancellationToken ct);

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
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("TokenExpired"));

		ParkingEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson { Detail1 = p.Detail1, Detail2 = p.Detail2 },
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
		IQueryable<ParkingEntity> q = db.Set<ParkingEntity>().ApplyReadParams(p);

		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData is not null && !userData.IsAdmin) {
			Guid uid = userData.Id;
			q = q.Where(x => x.CreatorId == uid || x.AdminUserIds.Contains(uid));
		}

		IQueryable<ParkingResponse> projected = q.Select(Projections.ParkingSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateParking(ParkingUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingEntity? e = await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ParkingNotFound"));
		
		if (!userData.IsAdmin && userData.Id != e.CreatorId) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		
		if (p.EntrancePrice.IsNotNull()) e.EntrancePrice = p.EntrancePrice.Value;
		if (p.HourlyPrice.IsNotNull()) e.HourlyPrice = p.HourlyPrice.Value;
		if (p.DailyPrice.IsNotNull()) e.DailyPrice = p.DailyPrice.Value;
		db.Set<ParkingEntity>().Update(e.ApplyUpdateParam<ParkingEntity,TagParking, BaseJson>(p));
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteParking(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<ParkingEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateParkingUser(ParkingUserCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("TokenExpired"));

		ParkingEntity? parking = await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == p.ParkingId, ct);
		if (parking == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("ParkingNotFound"));
		if (!userData.CanManage(parking.CreatorId, parking.AdminUserIds)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		bool exists = await db.Set<UserEntity>().AnyAsync(x => x.UserName == p.UserName, ct);
		if (exists) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("UserNameAlreadyExists"));

		Guid userId = Guid.CreateVersion7();
		DateTime now = DateTime.UtcNow;
		UserEntity user = new() {
			Id = userId,
			CreatorId = userData.Id,
			CreatedAt = now,
			JsonData = new UserJson(),
			Tags = [TagUser.Verified],
			UserName = p.UserName,
			Password = UPasswordHasher.Hash(p.Password),
			RefreshToken = ts.GenerateRefreshToken(),
			PhoneNumber = p.PhoneNumber,
			FirstName = p.FirstName,
			LastName = p.LastName,
			Wallets = [new WalletEntity { Id = userId, CreatorId = userId, CreatedAt = now, JsonData = new WalletJson(), Tags = [TagWallet.Primary], Balance = 0 }]
		};
		await db.Set<UserEntity>().AddAsync(user, ct);

		parking.AdminUserIds.Add(userId);
		db.Set<ParkingEntity>().Update(parking);

		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(userId, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<UserResponse>?>> ReadParkingUsers(ParkingUserReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<UserResponse>?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (userData.IsExpired) return new UResponse<IEnumerable<UserResponse>?>(null, Usc.ExpiredToken, ls.Get("TokenExpired"));

		ParkingEntity? parking = await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == p.ParkingId, ct);
		if (parking == null) return new UResponse<IEnumerable<UserResponse>?>(null, Usc.NotFound, ls.Get("ParkingNotFound"));
		if (!userData.CanAccess(parking.CreatorId, parking.AdminUserIds)) return new UResponse<IEnumerable<UserResponse>?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		List<UserResponse> users = await db.Set<UserEntity>()
			.Where(x => parking.AdminUserIds.Contains(x.Id))
			.Select(Projections.UserSelector(p.SelectorArgs))
			.ToListAsync(ct);
		return new UResponse<IEnumerable<UserResponse>?>(users);
	}

	public async Task<UResponse> RemoveParkingUser(ParkingUserDeleteParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingEntity? parking = await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == p.ParkingId, ct);
		if (parking == null) return new UResponse(Usc.NotFound, ls.Get("ParkingNotFound"));
		if (!userData.CanManage(parking.CreatorId, parking.AdminUserIds)) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		parking.AdminUserIds.Remove(p.UserId);
		db.Set<ParkingEntity>().Update(parking);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateParkingReport(ParkingReportCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("TokenExpired"));

		VehicleEntity? vehicle = await db.Set<VehicleEntity>().FirstOrDefaultAsync(x => x.LicencePlate == p.NumberPlate, ct);
		if (vehicle == null) {
			EntityEntry<VehicleEntity> vEntity = await db.Set<VehicleEntity>().AddAsync(new VehicleEntity {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				JsonData = new BaseJson(),
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
			JsonData = new BaseJson(),
			Tags = [TagParkingReport.Test]
		};
		await db.Set<ParkingReportEntity>().AddAsync(e, ct);

		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<ParkingReportResponse>?>> ReadParkingReport(ParkingReportReadParams p, CancellationToken ct) {
		IQueryable<ParkingReportEntity> q = db.Set<ParkingReportEntity>().ApplyReadParams(p);
		
		if (p.EndDate.HasValue) q = q.Where(x => x.EndDate >= p.EndDate);
		if (p.StartDate.HasValue) q = q.Where(x => x.StartDate >= p.StartDate);
		if (p.ParkingId.IsNotNull()) q = q.Where(x => x.ParkingId == p.ParkingId);
		if (p.VehicleId.IsNotNull()) q = q.Where(x => x.VehicleId == p.VehicleId);
		
		IQueryable<ParkingReportResponse> projected = q.Select(Projections.ParkingReportSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
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
		db.Set<ParkingReportEntity>().Update(e.ApplyUpdateParam<ParkingReportEntity,TagParkingReport, BaseJson>(p));
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