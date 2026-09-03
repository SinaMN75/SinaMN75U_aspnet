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

	Task<UResponse<Guid?>> CreateParkingTariff(ParkingTariffCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ParkingTariffResponse>?>> ReadParkingTariff(ParkingTariffReadParams p, CancellationToken ct);
	Task<UResponse> UpdateParkingTariff(ParkingTariffUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteParkingTariff(IdParams p, CancellationToken ct);

	Task<UResponse<Guid?>> CreateParkingSubscription(ParkingSubscriptionCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ParkingSubscriptionResponse>?>> ReadParkingSubscription(ParkingSubscriptionReadParams p, CancellationToken ct);
	Task<UResponse> UpdateParkingSubscription(ParkingSubscriptionUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteParkingSubscription(IdParams p, CancellationToken ct);

	Task<UResponse<Guid?>> CreateParkingPlateFlag(ParkingPlateFlagCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ParkingPlateFlagResponse>?>> ReadParkingPlateFlag(ParkingPlateFlagReadParams p, CancellationToken ct);
	Task<UResponse> UpdateParkingPlateFlag(ParkingPlateFlagUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteParkingPlateFlag(IdParams p, CancellationToken ct);

	Task<UResponse<Guid?>> CreateParkingStaff(ParkingStaffCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ParkingStaffResponse>?>> ReadParkingStaff(ParkingStaffReadParams p, CancellationToken ct);
	Task<UResponse> UpdateParkingStaff(ParkingStaffUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteParkingStaff(IdParams p, CancellationToken ct);

	Task<UResponse<ParkingShiftResponse?>> OpenParkingShift(ParkingShiftOpenParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ParkingShiftResponse>?>> ReadParkingShift(ParkingShiftReadParams p, CancellationToken ct);
	Task<UResponse<ParkingShiftResponse?>> CloseParkingShift(ParkingShiftCloseParams p, CancellationToken ct);

	Task<UResponse<ParkingPlateStatusResponse?>> ReadParkingPlateStatus(ParkingPlateStatusParams p, CancellationToken ct);
	Task<UResponse<ParkingReportResponse?>> RegisterParkingEntry(ParkingEntryParams p, CancellationToken ct);
	Task<UResponse<ParkingBillResponse?>> CalculateParkingExit(ParkingExitCalculateParams p, CancellationToken ct);
	Task<UResponse<ParkingReportResponse?>> RegisterParkingExit(ParkingExitParams p, CancellationToken ct);
	Task<UResponse<ParkingDashboardResponse?>> ReadParkingDashboard(ParkingDashboardParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ParkingInsideVehicleResponse>?>> ReadParkingInsideVehicles(ParkingInsideVehiclesParams p, CancellationToken ct);
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
			Address = p.Address,
			PhoneNumber = p.PhoneNumber,
			Capacity = p.Capacity,
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

		
		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.Address.IsNotNull()) e.Address = p.Address;
		if (p.PhoneNumber.IsNotNull()) e.PhoneNumber = p.PhoneNumber;
		if (p.Capacity.IsNotNull()) e.Capacity = p.Capacity.Value;
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
				Tags = [TagVehicle.Car],
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

	public async Task<UResponse<Guid?>> CreateParkingTariff(ParkingTariffCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingEntity? parking = await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == p.ParkingId, ct);
		if (parking == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("ParkingNotFound"));
		if (!userData.CanManage(parking.CreatorId, parking.AdminUserIds)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		// One tariff row per parking + vehicle type: creating an existing pair updates it instead of duplicating.
		ParkingTariffEntity? existing = await db.Set<ParkingTariffEntity>().FirstOrDefaultAsync(x => x.ParkingId == p.ParkingId && x.VehicleType == p.VehicleType, ct);
		if (existing != null) {
			ApplyTariff(existing, p);
			db.Set<ParkingTariffEntity>().Update(existing);
			await db.SaveChangesAsync(ct);
			return new UResponse<Guid?>(existing.Id);
		}

		ParkingTariffEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson { Detail1 = p.Detail1, Detail2 = p.Detail2 },
			Tags = p.Tags,
			CreatorId = p.CreatorId ?? userData.Id,
			ParkingId = p.ParkingId,
			VehicleType = p.VehicleType
		};
		ApplyTariff(e, p);
		await db.Set<ParkingTariffEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	private static void ApplyTariff(ParkingTariffEntity e, ParkingTariffCreateParams p) {
		e.EntrancePrice = p.EntrancePrice;
		e.DayHourlyPrice = p.DayHourlyPrice;
		e.NightHourlyPrice = p.NightHourlyPrice;
		e.DailyCap = p.DailyCap;
		e.WeeklyPrice = p.WeeklyPrice;
		e.MonthlyPrice = p.MonthlyPrice;
		e.QuarterlyPrice = p.QuarterlyPrice;
		e.FreeMinutes = p.FreeMinutes;
		e.NightStartHour = p.NightStartHour;
		e.NightEndHour = p.NightEndHour;
		e.HolidayExtraPercent = p.HolidayExtraPercent;
		e.RoundToFullHour = p.RoundToFullHour;
		e.PerMinuteAfterFirstHour = p.PerMinuteAfterFirstHour;
		e.SubscriptionDailyEntryLimit = p.SubscriptionDailyEntryLimit;
		e.SubscriptionOfficeHoursOnly = p.SubscriptionOfficeHoursOnly;
		e.SubscriptionExpiryReminderDays = p.SubscriptionExpiryReminderDays;
	}

	public async Task<UResponse<IEnumerable<ParkingTariffResponse>?>> ReadParkingTariff(ParkingTariffReadParams p, CancellationToken ct) {
		IQueryable<ParkingTariffEntity> q = db.Set<ParkingTariffEntity>().ApplyReadParams(p);
		if (p.ParkingId.IsNotNull()) q = q.Where(x => x.ParkingId == p.ParkingId);
		if (p.VehicleType.IsNotNull()) q = q.Where(x => x.VehicleType == p.VehicleType);
		return await q.Select(Projections.ParkingTariffSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateParkingTariff(ParkingTariffUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingTariffEntity? e = await db.Set<ParkingTariffEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ParkingTariffNotFound"));

		if (p.VehicleType.IsNotNull()) e.VehicleType = p.VehicleType.Value;
		if (p.EntrancePrice.IsNotNull()) e.EntrancePrice = p.EntrancePrice.Value;
		if (p.DayHourlyPrice.IsNotNull()) e.DayHourlyPrice = p.DayHourlyPrice.Value;
		if (p.NightHourlyPrice.IsNotNull()) e.NightHourlyPrice = p.NightHourlyPrice.Value;
		if (p.DailyCap.IsNotNull()) e.DailyCap = p.DailyCap.Value;
		if (p.WeeklyPrice.IsNotNull()) e.WeeklyPrice = p.WeeklyPrice.Value;
		if (p.MonthlyPrice.IsNotNull()) e.MonthlyPrice = p.MonthlyPrice.Value;
		if (p.QuarterlyPrice.IsNotNull()) e.QuarterlyPrice = p.QuarterlyPrice.Value;
		if (p.FreeMinutes.IsNotNull()) e.FreeMinutes = p.FreeMinutes.Value;
		if (p.NightStartHour.IsNotNull()) e.NightStartHour = p.NightStartHour.Value;
		if (p.NightEndHour.IsNotNull()) e.NightEndHour = p.NightEndHour.Value;
		if (p.HolidayExtraPercent.IsNotNull()) e.HolidayExtraPercent = p.HolidayExtraPercent.Value;
		if (p.RoundToFullHour.IsNotNull()) e.RoundToFullHour = p.RoundToFullHour.Value;
		if (p.PerMinuteAfterFirstHour.IsNotNull()) e.PerMinuteAfterFirstHour = p.PerMinuteAfterFirstHour.Value;
		if (p.SubscriptionDailyEntryLimit.IsNotNull()) e.SubscriptionDailyEntryLimit = p.SubscriptionDailyEntryLimit.Value;
		if (p.SubscriptionOfficeHoursOnly.IsNotNull()) e.SubscriptionOfficeHoursOnly = p.SubscriptionOfficeHoursOnly.Value;
		if (p.SubscriptionExpiryReminderDays.IsNotNull()) e.SubscriptionExpiryReminderDays = p.SubscriptionExpiryReminderDays.Value;

		db.Set<ParkingTariffEntity>().Update(e.ApplyUpdateParam<ParkingTariffEntity, TagParkingTariff, BaseJson>(p));
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteParkingTariff(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		await db.Set<ParkingTariffEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateParkingSubscription(ParkingSubscriptionCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingEntity? parking = await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == p.ParkingId, ct);
		if (parking == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("ParkingNotFound"));

		VehicleEntity vehicle = await GetOrCreateVehicle(p.LicencePlate, p.VehicleType, p.CreatorId ?? userData.Id, ct);

		DateTime start = p.StartDate ?? DateTime.UtcNow;
		DateTime expiry = p.ExpiryDate ?? start.AddDays(DurationDays(p.Tags));

		ParkingSubscriptionEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson { Detail1 = p.Detail1, Detail2 = p.Detail2 },
			Tags = p.Tags,
			CreatorId = p.CreatorId ?? userData.Id,
			ParkingId = p.ParkingId,
			VehicleId = vehicle.Id,
			CustomerName = p.CustomerName,
			CustomerPhoneNumber = p.CustomerPhoneNumber,
			Price = p.Price,
			StartDate = start,
			ExpiryDate = expiry,
			DailyEntryLimit = p.DailyEntryLimit,
			OfficeHoursOnly = p.OfficeHoursOnly
		};
		await db.Set<ParkingSubscriptionEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	private static int DurationDays(ICollection<TagParkingSubscription> tags) {
		if (tags.Contains(TagParkingSubscription.Quarterly)) return 90;
		if (tags.Contains(TagParkingSubscription.Weekly)) return 7;
		return 30;
	}

	private async Task<VehicleEntity> GetOrCreateVehicle(string licencePlate, TagVehicle vehicleType, Guid creatorId, CancellationToken ct) {
		VehicleEntity? vehicle = await db.Set<VehicleEntity>().FirstOrDefaultAsync(x => x.LicencePlate == licencePlate, ct);
		if (vehicle != null) return vehicle;

		EntityEntry<VehicleEntity> entry = await db.Set<VehicleEntity>().AddAsync(new VehicleEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson(),
			Tags = [vehicleType],
			LicencePlate = licencePlate,
			CreatorId = creatorId
		}, ct);
		return entry.Entity;
	}

	public async Task<UResponse<IEnumerable<ParkingSubscriptionResponse>?>> ReadParkingSubscription(ParkingSubscriptionReadParams p, CancellationToken ct) {
		DateTime now = DateTime.UtcNow;
		IQueryable<ParkingSubscriptionEntity> q = db.Set<ParkingSubscriptionEntity>().ApplyReadParams(p);

		if (p.ParkingId.IsNotNull()) q = q.Where(x => x.ParkingId == p.ParkingId);
		if (p.LicencePlate.IsNotNullOrEmpty()) q = q.Where(x => x.Vehicle.LicencePlate == p.LicencePlate);
		if (p.Query.IsNotNullOrEmpty())
			q = q.Where(x => x.Vehicle.LicencePlate.Contains(p.Query!) || (x.CustomerName != null && x.CustomerName.Contains(p.Query!)) || (x.CustomerPhoneNumber != null && x.CustomerPhoneNumber.Contains(p.Query!)));

		if (p.IsActive == true) q = q.Where(x => x.ExpiryDate > now && !x.Tags.Contains(TagParkingSubscription.Cancelled));
		if (p.IsExpired == true) q = q.Where(x => x.ExpiryDate <= now);
		if (p.IsExpiringSoon == true) {
			DateTime threshold = now.AddDays(p.ExpiringInDays);
			q = q.Where(x => x.ExpiryDate > now && x.ExpiryDate <= threshold);
		}

		return await q.Select(Projections.ParkingSubscriptionSelector(p.SelectorArgs, now)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateParkingSubscription(ParkingSubscriptionUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingSubscriptionEntity? e = await db.Set<ParkingSubscriptionEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ParkingSubscriptionNotFound"));

		if (p.CustomerName.IsNotNull()) e.CustomerName = p.CustomerName;
		if (p.CustomerPhoneNumber.IsNotNull()) e.CustomerPhoneNumber = p.CustomerPhoneNumber;
		if (p.Price.IsNotNull()) e.Price = p.Price.Value;
		if (p.StartDate.IsNotNull()) e.StartDate = p.StartDate.Value;
		if (p.ExpiryDate.IsNotNull()) e.ExpiryDate = p.ExpiryDate.Value;
		if (p.DailyEntryLimit.IsNotNull()) e.DailyEntryLimit = p.DailyEntryLimit.Value;
		if (p.OfficeHoursOnly.IsNotNull()) e.OfficeHoursOnly = p.OfficeHoursOnly.Value;

		db.Set<ParkingSubscriptionEntity>().Update(e.ApplyUpdateParam<ParkingSubscriptionEntity, TagParkingSubscription, BaseJson>(p));
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteParkingSubscription(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		await db.Set<ParkingSubscriptionEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateParkingPlateFlag(ParkingPlateFlagCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingEntity? parking = await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == p.ParkingId, ct);
		if (parking == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("ParkingNotFound"));

		ParkingPlateFlagEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson { Detail1 = p.Detail1, Detail2 = p.Detail2 },
			Tags = p.Tags,
			CreatorId = p.CreatorId ?? userData.Id,
			ParkingId = p.ParkingId,
			LicencePlate = p.LicencePlate,
			Reason = p.Reason,
			Amount = p.Amount,
			FromDate = p.FromDate,
			ToDate = p.ToDate,
			SpotNumber = p.SpotNumber
		};
		await db.Set<ParkingPlateFlagEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<ParkingPlateFlagResponse>?>> ReadParkingPlateFlag(ParkingPlateFlagReadParams p, CancellationToken ct) {
		IQueryable<ParkingPlateFlagEntity> q = db.Set<ParkingPlateFlagEntity>().ApplyReadParams(p);
		if (p.ParkingId.IsNotNull()) q = q.Where(x => x.ParkingId == p.ParkingId);
		if (p.LicencePlate.IsNotNullOrEmpty()) q = q.Where(x => x.LicencePlate == p.LicencePlate);
		return await q.Select(Projections.ParkingPlateFlagSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateParkingPlateFlag(ParkingPlateFlagUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingPlateFlagEntity? e = await db.Set<ParkingPlateFlagEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ParkingPlateFlagNotFound"));

		if (p.Reason.IsNotNull()) e.Reason = p.Reason;
		if (p.Amount.IsNotNull()) e.Amount = p.Amount;
		if (p.FromDate.IsNotNull()) e.FromDate = p.FromDate;
		if (p.ToDate.IsNotNull()) e.ToDate = p.ToDate;
		if (p.SpotNumber.IsNotNull()) e.SpotNumber = p.SpotNumber;

		db.Set<ParkingPlateFlagEntity>().Update(e.ApplyUpdateParam<ParkingPlateFlagEntity, TagParkingPlateFlag, BaseJson>(p));
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteParkingPlateFlag(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		await db.Set<ParkingPlateFlagEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateParkingStaff(ParkingStaffCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingEntity? parking = await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == p.ParkingId, ct);
		if (parking == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("ParkingNotFound"));
		if (!userData.CanManage(parking.CreatorId, parking.AdminUserIds)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		if (await db.Set<UserEntity>().AnyAsync(x => x.UserName == p.UserName, ct)) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("UserNameAlreadyExists"));

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

		ParkingStaffEntity staff = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = now,
			JsonData = new BaseJson { Detail1 = p.Detail1, Detail2 = p.Detail2 },
			Tags = p.Tags,
			CreatorId = userData.Id,
			ParkingId = p.ParkingId,
			UserId = userId,
			ShiftTitle = p.ShiftTitle,
			MaxDiscountPercent = p.MaxDiscountPercent
		};
		await db.Set<ParkingStaffEntity>().AddAsync(staff, ct);

		// Keep the parking's admin list in sync so the existing scoping rules see this operator.
		parking.AdminUserIds.Add(userId);
		db.Set<ParkingEntity>().Update(parking);

		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(staff.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<ParkingStaffResponse>?>> ReadParkingStaff(ParkingStaffReadParams p, CancellationToken ct) {
		IQueryable<ParkingStaffEntity> q = db.Set<ParkingStaffEntity>().ApplyReadParams(p);
		if (p.ParkingId.IsNotNull()) q = q.Where(x => x.ParkingId == p.ParkingId);
		return await q.Select(Projections.ParkingStaffSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateParkingStaff(ParkingStaffUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingStaffEntity? e = await db.Set<ParkingStaffEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ParkingStaffNotFound"));

		if (p.ShiftTitle.IsNotNull()) e.ShiftTitle = p.ShiftTitle;
		if (p.MaxDiscountPercent.IsNotNull()) e.MaxDiscountPercent = p.MaxDiscountPercent.Value;

		if (p.Password.IsNotNullOrEmpty()) {
			UserEntity? user = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == e.UserId, ct);
			if (user != null) {
				user.Password = UPasswordHasher.Hash(p.Password);
				db.Set<UserEntity>().Update(user);
			}
		}

		db.Set<ParkingStaffEntity>().Update(e.ApplyUpdateParam<ParkingStaffEntity, TagParkingStaff, BaseJson>(p));
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteParkingStaff(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingStaffEntity? e = await db.Set<ParkingStaffEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ParkingStaffNotFound"));

		ParkingEntity? parking = await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == e.ParkingId, ct);
		if (parking != null) {
			parking.AdminUserIds.Remove(e.UserId);
			db.Set<ParkingEntity>().Update(parking);
		}

		db.Set<ParkingStaffEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<ParkingShiftResponse?>> OpenParkingShift(ParkingShiftOpenParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingShiftResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingShiftEntity? open = await db.Set<ParkingShiftEntity>().FirstOrDefaultAsync(x => x.ParkingId == p.ParkingId && x.CreatorId == userData.Id && x.EndDate == null, ct);
		if (open == null) {
			open = new ParkingShiftEntity {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				JsonData = new BaseJson(),
				Tags = [TagParkingShift.Open],
				CreatorId = userData.Id,
				ParkingId = p.ParkingId,
				StartDate = DateTime.UtcNow
			};
			await db.Set<ParkingShiftEntity>().AddAsync(open, ct);
			await db.SaveChangesAsync(ct);
		}

		ParkingShiftResponse? result = await db.Set<ParkingShiftEntity>().Where(x => x.Id == open.Id).Select(Projections.ParkingShiftSelector(new ParkingShiftSelectorArgs())).FirstOrDefaultAsync(ct);
		return new UResponse<ParkingShiftResponse?>(result);
	}

	public async Task<UResponse<IEnumerable<ParkingShiftResponse>?>> ReadParkingShift(ParkingShiftReadParams p, CancellationToken ct) {
		IQueryable<ParkingShiftEntity> q = db.Set<ParkingShiftEntity>().ApplyReadParams(p);
		if (p.ParkingId.IsNotNull()) q = q.Where(x => x.ParkingId == p.ParkingId);
		if (p.IsOpen == true) q = q.Where(x => x.EndDate == null);
		if (p.IsOpen == false) q = q.Where(x => x.EndDate != null);
		return await q.Select(Projections.ParkingShiftSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ParkingShiftResponse?>> CloseParkingShift(ParkingShiftCloseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingShiftResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingShiftEntity? e = await db.Set<ParkingShiftEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<ParkingShiftResponse?>(null, Usc.NotFound, ls.Get("ParkingShiftNotFound"));

		e.EndDate = DateTime.UtcNow;
		e.CountedCash = p.CountedCash;
		e.Tags = [TagParkingShift.Closed];
		db.Set<ParkingShiftEntity>().Update(e);
		await db.SaveChangesAsync(ct);

		ParkingShiftResponse? result = await db.Set<ParkingShiftEntity>().Where(x => x.Id == e.Id).Select(Projections.ParkingShiftSelector(new ParkingShiftSelectorArgs())).FirstOrDefaultAsync(ct);
		return new UResponse<ParkingShiftResponse?>(result);
	}

	public async Task<UResponse<ParkingPlateStatusResponse?>> ReadParkingPlateStatus(ParkingPlateStatusParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingPlateStatusResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		DateTime now = DateTime.UtcNow;
		VehicleResponse? vehicle = await db.Set<VehicleEntity>().Where(x => x.LicencePlate == p.LicencePlate).Select(Projections.VehicleSelector(new VehicleSelectorArgs())).FirstOrDefaultAsync(ct);

		ParkingSubscriptionResponse? subscription = await db.Set<ParkingSubscriptionEntity>()
			.Where(x => x.ParkingId == p.ParkingId && x.Vehicle.LicencePlate == p.LicencePlate && x.ExpiryDate > now && !x.Tags.Contains(TagParkingSubscription.Cancelled))
			.OrderByDescending(x => x.ExpiryDate)
			.Select(Projections.ParkingSubscriptionSelector(new ParkingSubscriptionSelectorArgs { Vehicle = new VehicleSelectorArgs() }, now))
			.FirstOrDefaultAsync(ct);

		List<ParkingPlateFlagResponse> flags = await db.Set<ParkingPlateFlagEntity>()
			.Where(x => x.ParkingId == p.ParkingId && x.LicencePlate == p.LicencePlate)
			.Where(x => x.ToDate == null || x.ToDate > now)
			.Select(Projections.ParkingPlateFlagSelector(new ParkingPlateFlagSelectorArgs()))
			.ToListAsync(ct);

		ParkingReportResponse? openReport = await db.Set<ParkingReportEntity>()
			.Where(x => x.ParkingId == p.ParkingId && x.Vehicle.LicencePlate == p.LicencePlate && x.EndDate == null)
			.OrderByDescending(x => x.StartDate)
			.Select(Projections.ParkingReportSelector(new ParkingReportSelectorArgs { Vehicle = new VehicleSelectorArgs() }))
			.FirstOrDefaultAsync(ct);

		ParkingTariffResponse? tariff = await db.Set<ParkingTariffEntity>()
			.Where(x => x.ParkingId == p.ParkingId && x.VehicleType == p.VehicleType)
			.Select(Projections.ParkingTariffSelector(new ParkingTariffSelectorArgs()))
			.FirstOrDefaultAsync(ct);

		return new UResponse<ParkingPlateStatusResponse?>(new ParkingPlateStatusResponse {
			LicencePlate = p.LicencePlate,
			Vehicle = vehicle,
			Subscription = subscription,
			Reservation = flags.FirstOrDefault(x => x.Tags.Contains(TagParkingPlateFlag.Reservation)),
			Flags = flags.Where(x => !x.Tags.Contains(TagParkingPlateFlag.Reservation)).ToList(),
			OpenReport = openReport,
			Tariff = tariff,
			HasActiveSubscription = subscription != null,
			IsBanned = flags.Any(x => x.Tags.Contains(TagParkingPlateFlag.Banned)),
			IsInside = openReport != null
		});
	}

	public async Task<UResponse<ParkingReportResponse?>> RegisterParkingEntry(ParkingEntryParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingReportResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingEntity? parking = await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == p.ParkingId, ct);
		if (parking == null) return new UResponse<ParkingReportResponse?>(null, Usc.NotFound, ls.Get("ParkingNotFound"));

		DateTime now = DateTime.UtcNow;
		DateTime start = p.StartDate ?? now;

		VehicleEntity vehicle = await GetOrCreateVehicle(p.LicencePlate, p.VehicleType, userData.Id, ct);

		bool alreadyInside = await db.Set<ParkingReportEntity>().AnyAsync(x => x.ParkingId == p.ParkingId && x.VehicleId == vehicle.Id && x.EndDate == null, ct);
		if (alreadyInside) return new UResponse<ParkingReportResponse?>(null, Usc.Conflict, ls.Get("VehicleIsAlreadyInsideTheParking"));

		ParkingSubscriptionEntity? subscription = await db.Set<ParkingSubscriptionEntity>()
			.FirstOrDefaultAsync(x => x.ParkingId == p.ParkingId && x.VehicleId == vehicle.Id && x.ExpiryDate > now && !x.Tags.Contains(TagParkingSubscription.Cancelled), ct);

		ParkingShiftEntity? shift = await db.Set<ParkingShiftEntity>().FirstOrDefaultAsync(x => x.ParkingId == p.ParkingId && x.CreatorId == userData.Id && x.EndDate == null, ct);
		if (shift != null) {
			// The context is NoTracking, so a mutated entity has to be attached explicitly or the change is dropped.
			shift.EntryCount++;
			db.Set<ParkingShiftEntity>().Update(shift);
		}

		ParkingPlateFlagEntity? reservation = await db.Set<ParkingPlateFlagEntity>()
			.FirstOrDefaultAsync(x => x.ParkingId == p.ParkingId && x.LicencePlate == p.LicencePlate && x.Tags.Contains(TagParkingPlateFlag.Reservation) && (x.ToDate == null || x.ToDate > now), ct);

		ParkingReportEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = now,
			JsonData = new BaseJson(),
			Tags = p.IsOffline ? [TagParkingReport.Open, TagParkingReport.Offline] : [TagParkingReport.Open],
			CreatorId = userData.Id,
			ParkingId = p.ParkingId,
			VehicleId = vehicle.Id,
			StartDate = start,
			ReceiptNumber = await NextReceiptNumber(p.ParkingId, now, ct),
			SpotNumber = p.SpotNumber ?? reservation?.SpotNumber,
			CustomerPhoneNumber = p.CustomerPhoneNumber,
			SubscriptionId = subscription?.Id,
			ShiftId = shift?.Id
		};
		await db.Set<ParkingReportEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);

		ParkingReportResponse? result = await db.Set<ParkingReportEntity>().Where(x => x.Id == e.Id)
			.Select(Projections.ParkingReportSelector(new ParkingReportSelectorArgs { Vehicle = new VehicleSelectorArgs(), Parking = new ParkingSelectorArgs() }))
			.FirstOrDefaultAsync(ct);
		return new UResponse<ParkingReportResponse?>(result, Usc.Created);
	}

	/// Receipt numbers read as yyyyMMdd-NNNN and restart every day, per parking.
	private async Task<string> NextReceiptNumber(Guid parkingId, DateTime now, CancellationToken ct) {
		DateTime dayStart = now.Date;
		DateTime dayEnd = dayStart.AddDays(1);
		int count = await db.Set<ParkingReportEntity>().CountAsync(x => x.ParkingId == parkingId && x.CreatedAt >= dayStart && x.CreatedAt < dayEnd, ct);
		return $"{now:yyyyMMdd}-{count + 1:D4}";
	}

	public async Task<UResponse<ParkingBillResponse?>> CalculateParkingExit(ParkingExitCalculateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingBillResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingReportEntity? report = await FindOpenReport(p.ReportId, p.ParkingId, p.LicencePlate, ct);
		if (report == null) return new UResponse<ParkingBillResponse?>(null, Usc.NotFound, ls.Get("ParkingReportNotFound"));

		ParkingBillResponse bill = await BuildBill(report, p.CorrectedStartDate, p.EndDate, p.Discount, ct);
		return new UResponse<ParkingBillResponse?>(bill);
	}

	private async Task<ParkingReportEntity?> FindOpenReport(Guid? reportId, Guid? parkingId, string? licencePlate, CancellationToken ct) {
		IQueryable<ParkingReportEntity> q = db.Set<ParkingReportEntity>().Include(x => x.Vehicle).Where(x => x.EndDate == null);
		if (reportId.IsNotNull()) return await q.FirstOrDefaultAsync(x => x.Id == reportId, ct);
		if (parkingId.IsNotNull()) q = q.Where(x => x.ParkingId == parkingId);
		if (licencePlate.IsNotNullOrEmpty()) q = q.Where(x => x.Vehicle.LicencePlate == licencePlate);
		return await q.OrderByDescending(x => x.StartDate).FirstOrDefaultAsync(ct);
	}

	private async Task<ParkingBillResponse> BuildBill(ParkingReportEntity report, DateTime? correctedStart, DateTime? end, decimal discount, CancellationToken ct) {
		DateTime start = correctedStart ?? report.StartDate;
		DateTime finish = end ?? DateTime.UtcNow;
		if (finish < start) finish = start;

		TagVehicle vehicleType = report.Vehicle.Tags.FirstOrDefault();

		ParkingTariffEntity? tariff = await db.Set<ParkingTariffEntity>().FirstOrDefaultAsync(x => x.ParkingId == report.ParkingId && x.VehicleType == vehicleType, ct);
		ParkingEntity? parking = await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == report.ParkingId, ct);

		int totalMinutes = (int)(finish - start).TotalMinutes;
		ParkingBillResponse bill = new() {
			ReportId = report.Id,
			LicencePlate = report.Vehicle.LicencePlate,
			VehicleType = vehicleType,
			SpotNumber = report.SpotNumber,
			ReceiptNumber = report.ReceiptNumber,
			StartDate = start,
			EndDate = finish,
			TotalMinutes = totalMinutes,
			Discount = discount
		};

		// A live subscription makes the stay free; the exit receipt is still issued.
		if (report.SubscriptionId.IsNotNull() && await db.Set<ParkingSubscriptionEntity>().AnyAsync(x => x.Id == report.SubscriptionId && x.ExpiryDate > finish, ct)) {
			bill.IsSubscription = true;
			bill.Lines.Add(new ParkingBillLineResponse { Key = "Subscription", Amount = 0, IsFree = true });
			return bill;
		}

		decimal entrancePrice = tariff?.EntrancePrice ?? parking?.EntrancePrice ?? 0;
		decimal dayRate = tariff?.DayHourlyPrice ?? parking?.HourlyPrice ?? 0;
		decimal nightRate = tariff?.NightHourlyPrice ?? dayRate;
		decimal dailyCap = tariff?.DailyCap ?? parking?.DailyPrice ?? 0;
		int freeMinutes = tariff?.FreeMinutes ?? 0;
		int nightStart = tariff?.NightStartHour ?? 22;
		int nightEnd = tariff?.NightEndHour ?? 6;
		bool roundToFullHour = tariff?.RoundToFullHour ?? false;
		bool perMinute = tariff?.PerMinuteAfterFirstHour ?? true;

		if (freeMinutes > 0) bill.Lines.Add(new ParkingBillLineResponse { Key = "FreeMinutes", Amount = 0, Minutes = freeMinutes, IsFree = true });

		if (totalMinutes <= freeMinutes) {
			bill.Payable = 0;
			return bill;
		}

		DateTime billableFrom = start.AddMinutes(freeMinutes);
		int billableMinutes = totalMinutes - freeMinutes;
		decimal subtotal = entrancePrice;
		if (entrancePrice > 0) bill.Lines.Add(new ParkingBillLineResponse { Key = "EntrancePrice", Amount = entrancePrice });

		int firstHourMinutes = Math.Min(billableMinutes, 60);
		DateTime firstHourEnd = billableFrom.AddMinutes(firstHourMinutes);
		int firstHourNight = NightMinutes(billableFrom, firstHourEnd, nightStart, nightEnd);
		decimal firstHourPrice = firstHourNight * 2 >= firstHourMinutes ? nightRate : dayRate;
		if (firstHourPrice > 0) {
			subtotal += firstHourPrice;
			bill.Lines.Add(new ParkingBillLineResponse { Key = "FirstHour", Amount = firstHourPrice, Minutes = firstHourMinutes });
		}

		int restMinutes = billableMinutes - firstHourMinutes;
		if (restMinutes > 0) {
			int restNight = NightMinutes(firstHourEnd, finish, nightStart, nightEnd);
			int restDay = restMinutes - restNight;
			decimal restAmount;
			if (roundToFullHour || !perMinute) {
				int hours = (int)Math.Ceiling(restMinutes / 60m);
				decimal blended = (restDay * dayRate + restNight * nightRate) / restMinutes;
				restAmount = hours * blended;
			}
			else {
				restAmount = restDay * dayRate / 60m + restNight * nightRate / 60m;
			}
			restAmount = Math.Round(restAmount, 0, MidpointRounding.AwayFromZero);
			subtotal += restAmount;
			bill.Lines.Add(new ParkingBillLineResponse { Key = "AdditionalMinutes", Amount = restAmount, Minutes = restMinutes });
			if (restNight > 0) bill.IsNightRateApplied = true;
		}

		if (firstHourNight > 0) bill.IsNightRateApplied = true;

		int holidayExtra = tariff?.HolidayExtraPercent ?? 0;
		if (holidayExtra > 0 && IsWeekend(start)) {
			decimal extra = Math.Round(subtotal * holidayExtra / 100m, 0, MidpointRounding.AwayFromZero);
			subtotal += extra;
			bill.Lines.Add(new ParkingBillLineResponse { Key = "HolidayExtra", Amount = extra });
		}

		if (dailyCap > 0) {
			int days = Math.Max(1, (int)Math.Ceiling(totalMinutes / 1440m));
			decimal cap = dailyCap * days;
			bill.DailyCap = cap;
			if (subtotal > cap) {
				bill.Lines.Add(new ParkingBillLineResponse { Key = "DailyCapApplied", Amount = cap - subtotal });
				subtotal = cap;
				bill.IsCapped = true;
			}
		}

		bill.Subtotal = subtotal;
		if (discount > 0) bill.Lines.Add(new ParkingBillLineResponse { Key = "Discount", Amount = -discount });
		bill.Payable = Math.Max(0, subtotal - discount);
		return bill;
	}

	private static bool IsWeekend(DateTime date) => date.DayOfWeek is DayOfWeek.Thursday or DayOfWeek.Friday;

	/// Minutes of [from, to) that fall inside the nightly window, which may wrap past midnight.
	private static int NightMinutes(DateTime from, DateTime to, int nightStart, int nightEnd) {
		if (to <= from || nightStart == nightEnd) return 0;
		int total = 0;
		for (DateTime cursor = from.Date; cursor < to; cursor = cursor.AddDays(1)) {
			if (nightStart < nightEnd) {
				total += OverlapMinutes(from, to, cursor.AddHours(nightStart), cursor.AddHours(nightEnd));
			}
			else {
				total += OverlapMinutes(from, to, cursor.AddHours(nightStart), cursor.AddDays(1));
				total += OverlapMinutes(from, to, cursor, cursor.AddHours(nightEnd));
			}
		}
		return total;
	}

	private static int OverlapMinutes(DateTime aFrom, DateTime aTo, DateTime bFrom, DateTime bTo) {
		DateTime from = aFrom > bFrom ? aFrom : bFrom;
		DateTime to = aTo < bTo ? aTo : bTo;
		return to <= from ? 0 : (int)(to - from).TotalMinutes;
	}

	public async Task<UResponse<ParkingReportResponse?>> RegisterParkingExit(ParkingExitParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingReportResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingReportEntity? report = await db.Set<ParkingReportEntity>().Include(x => x.Vehicle).FirstOrDefaultAsync(x => x.Id == p.ReportId, ct);
		if (report == null) return new UResponse<ParkingReportResponse?>(null, Usc.NotFound, ls.Get("ParkingReportNotFound"));
		if (report.EndDate.IsNotNull()) return new UResponse<ParkingReportResponse?>(null, Usc.Conflict, ls.Get("ParkingReportIsAlreadyClosed"));

		ParkingBillResponse bill = await BuildBill(report, p.CorrectedStartDate, p.EndDate, p.Discount, ct);
		TagParkingPayment method = bill.IsSubscription ? TagParkingPayment.Subscription : bill.Payable <= 0 ? TagParkingPayment.Free : p.PaymentMethod;

		if (p.CorrectedStartDate.IsNotNull()) report.StartDate = p.CorrectedStartDate.Value;
		report.EndDate = bill.EndDate;
		report.Amount = bill.Payable;
		report.Discount = p.Discount;
		report.PaidAmount = bill.Payable;
		report.PaymentMethod = method;
		report.TrackingCode = p.TrackingCode;
		report.Tags = p.IsOffline ? [TagParkingReport.Closed, TagParkingReport.Offline] : [TagParkingReport.Closed];
		db.Set<ParkingReportEntity>().Update(report);

		ParkingShiftEntity? shift = await db.Set<ParkingShiftEntity>().FirstOrDefaultAsync(x => x.ParkingId == report.ParkingId && x.CreatorId == userData.Id && x.EndDate == null, ct);
		if (shift != null) {
			shift.ExitCount++;
			switch (method) {
				case TagParkingPayment.Cash:
					shift.CashTotal += bill.Payable;
					break;
				case TagParkingPayment.Card:
					shift.CardTotal += bill.Payable;
					break;
				case TagParkingPayment.Ipg:
					shift.IpgTotal += bill.Payable;
					break;
				case TagParkingPayment.Subscription:
				case TagParkingPayment.Free:
				default:
					break;
			}
			db.Set<ParkingShiftEntity>().Update(shift);
		}

		await db.SaveChangesAsync(ct);

		ParkingReportResponse? result = await db.Set<ParkingReportEntity>().Where(x => x.Id == report.Id)
			.Select(Projections.ParkingReportSelector(new ParkingReportSelectorArgs { Vehicle = new VehicleSelectorArgs(), Parking = new ParkingSelectorArgs() }))
			.FirstOrDefaultAsync(ct);
		return new UResponse<ParkingReportResponse?>(result);
	}

	public async Task<UResponse<ParkingDashboardResponse?>> ReadParkingDashboard(ParkingDashboardParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingDashboardResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ParkingEntity? parking = await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == p.ParkingId, ct);
		if (parking == null) return new UResponse<ParkingDashboardResponse?>(null, Usc.NotFound, ls.Get("ParkingNotFound"));

		int insideCount = await db.Set<ParkingReportEntity>().CountAsync(x => x.ParkingId == p.ParkingId && x.EndDate == null, ct);

		ParkingShiftResponse? shift = await db.Set<ParkingShiftEntity>()
			.Where(x => x.ParkingId == p.ParkingId && x.CreatorId == userData.Id && x.EndDate == null)
			.Select(Projections.ParkingShiftSelector(new ParkingShiftSelectorArgs()))
			.FirstOrDefaultAsync(ct);

		List<ParkingReportResponse> recent = await db.Set<ParkingReportEntity>()
			.Where(x => x.ParkingId == p.ParkingId)
			.OrderByDescending(x => x.EndDate ?? x.StartDate)
			.Take(p.RecentCount)
			.Select(Projections.ParkingReportSelector(new ParkingReportSelectorArgs { Vehicle = new VehicleSelectorArgs() }))
			.ToListAsync(ct);

		return new UResponse<ParkingDashboardResponse?>(new ParkingDashboardResponse {
			ParkingId = parking.Id,
			Title = parking.Title,
			Capacity = parking.Capacity,
			InsideCount = insideCount,
			ShiftRevenue = shift?.Total ?? 0,
			OpenShift = shift,
			RecentReports = recent
		});
	}

	public async Task<UResponse<IEnumerable<ParkingInsideVehicleResponse>?>> ReadParkingInsideVehicles(ParkingInsideVehiclesParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<ParkingInsideVehicleResponse>?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		DateTime now = DateTime.UtcNow;
		IQueryable<ParkingReportEntity> q = db.Set<ParkingReportEntity>().Include(x => x.Vehicle).Where(x => x.ParkingId == p.ParkingId && x.EndDate == null);

		if (p.Query.IsNotNullOrEmpty()) q = q.Where(x => x.Vehicle.LicencePlate.Contains(p.Query!));
		if (p.LongerThanADay == true) q = q.Where(x => x.StartDate <= now.AddDays(-1));
		if (p.HasSubscription == true) q = q.Where(x => x.SubscriptionId != null);

		int totalCount = await q.CountAsync(ct);
		List<ParkingReportEntity> reports = await q
			.OrderBy(x => x.StartDate)
			.Skip((Math.Max(1, p.PageNumber) - 1) * p.PageSize)
			.Take(p.PageSize)
			.ToListAsync(ct);

		List<ParkingInsideVehicleResponse> items = [];
		foreach (ParkingReportEntity report in reports) {
			ParkingBillResponse bill = await BuildBill(report, null, now, 0, ct);
			items.Add(new ParkingInsideVehicleResponse {
				ReportId = report.Id,
				LicencePlate = report.Vehicle.LicencePlate,
				VehicleType = bill.VehicleType,
				StartDate = report.StartDate,
				SpotNumber = report.SpotNumber,
				StayedMinutes = bill.TotalMinutes,
				EstimatedAmount = bill.Payable,
				HasSubscription = bill.IsSubscription,
				IsCapped = bill.IsCapped
			});
		}

		return new UResponse<IEnumerable<ParkingInsideVehicleResponse>?>(items) {
			TotalCount = totalCount,
			PageSize = p.PageSize,
			PageCount = (int)Math.Ceiling(totalCount / (decimal)p.PageSize)
		};
	}
}
