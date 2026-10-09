namespace SinaMN75U.Services;

public interface IParkingService {
	Task<UResponse<ParkingSeedResponse?>> SeedParking(ParkingSeedParams p, CancellationToken ct);
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
	private readonly Dictionary<(Guid ParkingId, TagVehicle VehicleType), ParkingTariffEntity?> _tariffs = new();
	private readonly Dictionary<Guid, ParkingEntity?> _parkings = new();

	private IQueryable<Guid> ParkingsOf(Guid userId) {
		IQueryable<Guid> off = db.Set<ParkingStaffEntity>().Where(x => x.UserId == userId && x.Tags.Contains(TagParkingStaff.Disabled)).Select(x => x.ParkingId);
		return db.Set<ParkingEntity>().Where(x => (x.CreatorId == userId || x.AdminUserIds.Contains(userId)) && !off.Contains(x.Id)).Select(x => x.Id);
	}

	private IQueryable<Guid> NoReportsIn(Guid userId) => db.Set<ParkingStaffEntity>()
		.Where(x => x.UserId == userId && x.Tags.Count > (x.Tags.Contains(TagParkingStaff.Disabled) ? 1 : 0) && !x.Tags.Contains(TagParkingStaff.ViewFinancialReports))
		.Select(x => x.ParkingId);

	private async Task<(ParkingEntity? Parking, ParkingStaffEntity? Staff, UResponse? Error)> Access(JwtClaimData u, Guid? parkingId, CancellationToken ct, TagParkingStaff? permission = null, bool anyStaff = false) {
		ParkingEntity? parking = parkingId == null ? null : await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == parkingId, ct);
		if (parking == null) return (null, null, new UResponse(Usc.NotFound, ls.Get("parkingNotFound")));
		if (u.IsAdmin || parking.CreatorId == u.Id) return (parking, null, null);
		ParkingStaffEntity? staff = await db.Set<ParkingStaffEntity>().FirstOrDefaultAsync(x => x.ParkingId == parking.Id && x.UserId == u.Id, ct);
		bool allowed = staff == null
			? parking.AdminUserIds.Contains(u.Id)
			: !staff.Tags.Contains(TagParkingStaff.Disabled) && (anyStaff || permission != null && (staff.Tags.Count == 0 || staff.Tags.Contains(permission.Value)));
		return (parking, staff, allowed ? null : new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction")));
	}

	public async Task<UResponse<Guid?>> CreateParking(ParkingCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		ParkingEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new ParkingJson { Detail1 = p.Detail1, Detail2 = p.Detail2 },
			Tags = p.Tags,
			Title = p.Title,
			Address = p.Address,
			PhoneNumber = p.PhoneNumber,
			Capacity = p.Capacity,
			CreatorId = userData.IsAdmin ? p.CreatorId ?? userData.Id : userData.Id,
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
		if (userData == null) return new UResponse<IEnumerable<ParkingResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (!userData.IsAdmin) {
			IQueryable<Guid> mine = ParkingsOf(userData.Id);
			q = q.Where(x => mine.Contains(x.Id));
		}

		IQueryable<ParkingResponse> projected = q.Select(Projections.ParkingSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateParking(ParkingUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		ParkingEntity? e = await db.Set<ParkingEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("parkingNotFound"));
		
		if (!userData.IsAdmin && userData.Id != e.CreatorId) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		
		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.Address.IsNotNull()) e.Address = p.Address;
		if (p.PhoneNumber.IsNotNull()) e.PhoneNumber = p.PhoneNumber;
		if (p.Capacity.IsNotNull()) e.Capacity = p.Capacity.Value;
		if (p.EntrancePrice.IsNotNull()) e.EntrancePrice = p.EntrancePrice.Value;
		if (p.HourlyPrice.IsNotNull()) e.HourlyPrice = p.HourlyPrice.Value;
		if (p.DailyPrice.IsNotNull()) e.DailyPrice = p.DailyPrice.Value;
		e.ApplyUpdateParam<ParkingEntity,TagParking, ParkingJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteParking(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		if (!userData.IsAdmin && !await db.Set<ParkingEntity>().AnyAsync(x => x.Id == p.Id && x.CreatorId == userData.Id, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		await db.Set<ParkingEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateParkingUser(ParkingUserCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		ParkingEntity? parking = await db.Set<ParkingEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.ParkingId, ct);
		if (parking == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("parkingNotFound"));
		if ((await Access(userData, parking.Id, ct)).Error is { } accessError) return new UResponse<Guid?>(null, accessError.Status, accessError.Message);

		bool exists = await db.Set<UserEntity>().AnyAsync(x => x.UserName == p.UserName, ct);
		if (exists) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("thisUsernameIsAlreadyTaken"));

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

		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(userId, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<UserResponse>?>> ReadParkingUsers(ParkingUserReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<UserResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<UserResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		ParkingEntity? parking = await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == p.ParkingId, ct);
		if (parking == null) return new UResponse<IEnumerable<UserResponse>?>(null, Usc.NotFound, ls.Get("parkingNotFound"));
		if (!userData.CanAccess(parking.CreatorId, parking.AdminUserIds)) return new UResponse<IEnumerable<UserResponse>?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		List<UserResponse> users = await db.Set<UserEntity>()
			.Where(x => parking.AdminUserIds.Contains(x.Id))
			.Select(Projections.UserSelector(p.SelectorArgs))
			.ToListAsync(ct);
		return new UResponse<IEnumerable<UserResponse>?>(users);
	}

	public async Task<UResponse> RemoveParkingUser(ParkingUserDeleteParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		ParkingEntity? parking = await db.Set<ParkingEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.ParkingId, ct);
		if (parking == null) return new UResponse(Usc.NotFound, ls.Get("parkingNotFound"));
		if ((await Access(userData, parking.Id, ct)).Error is { } accessError) return accessError;

		parking.AdminUserIds.Remove(p.UserId);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateParkingReport(ParkingReportCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		if ((await Access(userData, p.ParkingId, ct)).Error is { } accessError) return new UResponse<Guid?>(null, accessError.Status, accessError.Message);

		VehicleEntity? vehicle = await db.Set<VehicleEntity>().FirstOrDefaultAsync(x => x.LicencePlate == p.NumberPlate, ct);
		if (vehicle == null) {
			EntityEntry<VehicleEntity> vEntity = await db.Set<VehicleEntity>().AddAsync(new VehicleEntity {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				JsonData = new VehicleJson(),
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
			JsonData = new ParkingReportJson(),
			Tags = [TagParkingReport.Test]
		};
		await db.Set<ParkingReportEntity>().AddAsync(e, ct);

		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<ParkingReportResponse>?>> ReadParkingReport(ParkingReportReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<ParkingReportResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<ParkingReportResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		IQueryable<ParkingReportEntity> q = db.Set<ParkingReportEntity>().ApplyReadParams(p);
		if (!userData.IsAdmin) {
			Guid uid = userData.Id;
			IQueryable<Guid> mine = ParkingsOf(uid), limited = NoReportsIn(uid);
			q = q.Where(x => mine.Contains(x.ParkingId) && (!limited.Contains(x.ParkingId) || x.CreatorId == uid));
		}

		if (p.EndDate.HasValue) q = q.Where(x => x.EndDate >= p.EndDate);
		if (p.StartDate.HasValue) q = q.Where(x => x.StartDate >= p.StartDate);
		if (p.ParkingId.IsNotNull()) q = q.Where(x => x.ParkingId == p.ParkingId);
		if (p.VehicleId.IsNotNull()) q = q.Where(x => x.VehicleId == p.VehicleId);
		
		IQueryable<ParkingReportResponse> projected = q.Select(Projections.ParkingReportSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateParkingReport(ParkingReportUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		ParkingReportEntity? e = await db.Set<ParkingReportEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("parkingReportNotFound"));
		if ((await Access(userData, e.ParkingId, ct)).Error is { } accessError) return accessError;
		if (p.ParkingId.IsNotNull() && p.ParkingId != e.ParkingId && (await Access(userData, p.ParkingId, ct)).Error is { } targetError) return targetError;

		if (p.CreatorId.IsNotNull()) e.CreatorId = p.CreatorId.Value;
		if (p.VehicleId.IsNotNull()) e.VehicleId = p.VehicleId.Value;
		if (p.ParkingId.IsNotNull()) e.ParkingId = p.ParkingId.Value;
		if (p.StartDate != null) e.StartDate = p.StartDate.Value;
		if (p.EndDate != null) e.EndDate = p.EndDate;
		if (p.Amount.IsNotNull()) e.Amount = p.Amount.Value;
		e.ApplyUpdateParam<ParkingReportEntity,TagParkingReport, ParkingReportJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteParkingReport(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		Guid? reportParking = await db.Set<ParkingReportEntity>().Where(x => x.Id == p.Id).Select(x => (Guid?)x.ParkingId).FirstOrDefaultAsync(ct);
		if (reportParking == null) return new UResponse(Usc.NotFound, ls.Get("parkingReportNotFound"));
		if ((await Access(userData, reportParking, ct)).Error is { } accessError) return accessError;
		await db.Set<ParkingReportEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateParkingTariff(ParkingTariffCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		if ((await Access(userData, p.ParkingId, ct, TagParkingStaff.ChangeTariff)).Error is { } accessError) return new UResponse<Guid?>(null, accessError.Status, accessError.Message);

		ParkingTariffEntity? existing = await db.Set<ParkingTariffEntity>().AsTracking().FirstOrDefaultAsync(x => x.ParkingId == p.ParkingId && x.VehicleType == p.VehicleType, ct);
		if (existing != null) {
			ApplyTariff(existing, p);
			await db.SaveChangesAsync(ct);
			return new UResponse<Guid?>(existing.Id);
		}

		ParkingTariffEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new ParkingTariffJson { Detail1 = p.Detail1, Detail2 = p.Detail2 },
			Tags = p.Tags,
			CreatorId = userData.Id,
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
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<ParkingTariffResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<ParkingTariffResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		IQueryable<ParkingTariffEntity> q = db.Set<ParkingTariffEntity>().ApplyReadParams(p);
		if (!userData.IsAdmin) {
			IQueryable<Guid> mine = ParkingsOf(userData.Id);
			q = q.Where(x => mine.Contains(x.ParkingId));
		}
		if (p.ParkingId.IsNotNull()) q = q.Where(x => x.ParkingId == p.ParkingId);
		if (p.VehicleType.IsNotNull()) q = q.Where(x => x.VehicleType == p.VehicleType);
		return await q.Select(Projections.ParkingTariffSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateParkingTariff(ParkingTariffUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		ParkingTariffEntity? e = await db.Set<ParkingTariffEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("parkingTariffNotFound"));
		if ((await Access(userData, e.ParkingId, ct, TagParkingStaff.ChangeTariff)).Error is { } accessError) return accessError;

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

		e.ApplyUpdateParam<ParkingTariffEntity, TagParkingTariff, ParkingTariffJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteParkingTariff(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		Guid? tariffParking = await db.Set<ParkingTariffEntity>().Where(x => x.Id == p.Id).Select(x => (Guid?)x.ParkingId).FirstOrDefaultAsync(ct);
		if (tariffParking == null) return new UResponse(Usc.NotFound, ls.Get("parkingTariffNotFound"));
		if ((await Access(userData, tariffParking, ct, TagParkingStaff.ChangeTariff)).Error is { } accessError) return accessError;
		await db.Set<ParkingTariffEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateParkingSubscription(ParkingSubscriptionCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		if ((await Access(userData, p.ParkingId, ct, TagParkingStaff.ManageSubscriptions)).Error is { } accessError) return new UResponse<Guid?>(null, accessError.Status, accessError.Message);

		VehicleEntity vehicle = await GetOrCreateVehicle(p.LicencePlate, p.VehicleType, userData.Id, ct);

		DateTime start = p.StartDate ?? DateTime.UtcNow;
		DateTime expiry = p.ExpiryDate ?? start.AddDays(DurationDays(p.Tags));

		ParkingSubscriptionEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new ParkingSubscriptionJson { Detail1 = p.Detail1, Detail2 = p.Detail2 },
			Tags = p.Tags,
			CreatorId = userData.Id,
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
			JsonData = new VehicleJson(),
			Tags = [vehicleType],
			LicencePlate = licencePlate,
			CreatorId = creatorId
		}, ct);
		return entry.Entity;
	}

	public async Task<UResponse<IEnumerable<ParkingSubscriptionResponse>?>> ReadParkingSubscription(ParkingSubscriptionReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<ParkingSubscriptionResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<ParkingSubscriptionResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		DateTime now = DateTime.UtcNow;
		IQueryable<ParkingSubscriptionEntity> q = db.Set<ParkingSubscriptionEntity>().ApplyReadParams(p);
		if (!userData.IsAdmin) {
			IQueryable<Guid> mine = ParkingsOf(userData.Id);
			q = q.Where(x => mine.Contains(x.ParkingId));
		}

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
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		ParkingSubscriptionEntity? e = await db.Set<ParkingSubscriptionEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("parkingSubscriptionNotFound"));
		if ((await Access(userData, e.ParkingId, ct, TagParkingStaff.ManageSubscriptions)).Error is { } accessError) return accessError;

		if (p.CustomerName.IsNotNull()) e.CustomerName = p.CustomerName;
		if (p.CustomerPhoneNumber.IsNotNull()) e.CustomerPhoneNumber = p.CustomerPhoneNumber;
		if (p.Price.IsNotNull()) e.Price = p.Price.Value;
		if (p.StartDate.IsNotNull()) e.StartDate = p.StartDate.Value;
		if (p.ExpiryDate.IsNotNull()) e.ExpiryDate = p.ExpiryDate.Value;
		if (p.DailyEntryLimit.IsNotNull()) e.DailyEntryLimit = p.DailyEntryLimit.Value;
		if (p.OfficeHoursOnly.IsNotNull()) e.OfficeHoursOnly = p.OfficeHoursOnly.Value;

		e.ApplyUpdateParam<ParkingSubscriptionEntity, TagParkingSubscription, ParkingSubscriptionJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteParkingSubscription(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		Guid? subscriptionParking = await db.Set<ParkingSubscriptionEntity>().Where(x => x.Id == p.Id).Select(x => (Guid?)x.ParkingId).FirstOrDefaultAsync(ct);
		if (subscriptionParking == null) return new UResponse(Usc.NotFound, ls.Get("parkingSubscriptionNotFound"));
		if ((await Access(userData, subscriptionParking, ct, TagParkingStaff.ManageSubscriptions)).Error is { } accessError) return accessError;
		await db.Set<ParkingSubscriptionEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateParkingPlateFlag(ParkingPlateFlagCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		if ((await Access(userData, p.ParkingId, ct, TagParkingStaff.RegisterEntryExit)).Error is { } accessError) return new UResponse<Guid?>(null, accessError.Status, accessError.Message);

		ParkingPlateFlagEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new ParkingPlateFlagJson { Detail1 = p.Detail1, Detail2 = p.Detail2 },
			Tags = p.Tags,
			CreatorId = userData.Id,
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
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<ParkingPlateFlagResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<ParkingPlateFlagResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		IQueryable<ParkingPlateFlagEntity> q = db.Set<ParkingPlateFlagEntity>().ApplyReadParams(p);
		if (!userData.IsAdmin) {
			IQueryable<Guid> mine = ParkingsOf(userData.Id);
			q = q.Where(x => mine.Contains(x.ParkingId));
		}
		if (p.ParkingId.IsNotNull()) q = q.Where(x => x.ParkingId == p.ParkingId);
		if (p.LicencePlate.IsNotNullOrEmpty()) q = q.Where(x => x.LicencePlate == p.LicencePlate);
		return await q.Select(Projections.ParkingPlateFlagSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateParkingPlateFlag(ParkingPlateFlagUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		ParkingPlateFlagEntity? e = await db.Set<ParkingPlateFlagEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("plateRecordNotFound"));
		if ((await Access(userData, e.ParkingId, ct, TagParkingStaff.RegisterEntryExit)).Error is { } accessError) return accessError;

		if (p.Reason.IsNotNull()) e.Reason = p.Reason;
		if (p.Amount.IsNotNull()) e.Amount = p.Amount;
		if (p.FromDate.IsNotNull()) e.FromDate = p.FromDate;
		if (p.ToDate.IsNotNull()) e.ToDate = p.ToDate;
		if (p.SpotNumber.IsNotNull()) e.SpotNumber = p.SpotNumber;

		e.ApplyUpdateParam<ParkingPlateFlagEntity, TagParkingPlateFlag, ParkingPlateFlagJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteParkingPlateFlag(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		Guid? flagParking = await db.Set<ParkingPlateFlagEntity>().Where(x => x.Id == p.Id).Select(x => (Guid?)x.ParkingId).FirstOrDefaultAsync(ct);
		if (flagParking == null) return new UResponse(Usc.NotFound, ls.Get("plateRecordNotFound"));
		if ((await Access(userData, flagParking, ct, TagParkingStaff.RegisterEntryExit)).Error is { } accessError) return accessError;
		await db.Set<ParkingPlateFlagEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateParkingStaff(ParkingStaffCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		ParkingEntity? parking = await db.Set<ParkingEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.ParkingId, ct);
		if (parking == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("parkingNotFound"));
		if ((await Access(userData, parking.Id, ct)).Error is { } accessError) return new UResponse<Guid?>(null, accessError.Status, accessError.Message);

		if (await db.Set<UserEntity>().AnyAsync(x => x.UserName == p.UserName, ct)) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("thisUsernameIsAlreadyTaken"));

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
			JsonData = new ParkingStaffJson { Detail1 = p.Detail1, Detail2 = p.Detail2 },
			Tags = p.Tags,
			CreatorId = userData.Id,
			ParkingId = p.ParkingId,
			UserId = userId,
			ShiftTitle = p.ShiftTitle,
			MaxDiscountPercent = p.MaxDiscountPercent
		};
		await db.Set<ParkingStaffEntity>().AddAsync(staff, ct);

		parking.AdminUserIds.Add(userId);

		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(staff.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<ParkingStaffResponse>?>> ReadParkingStaff(ParkingStaffReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<ParkingStaffResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<ParkingStaffResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		IQueryable<ParkingStaffEntity> q = db.Set<ParkingStaffEntity>().ApplyReadParams(p);
		if (!userData.IsAdmin) {
			IQueryable<Guid> mine = ParkingsOf(userData.Id);
			q = q.Where(x => mine.Contains(x.ParkingId));
		}
		if (p.ParkingId.IsNotNull()) q = q.Where(x => x.ParkingId == p.ParkingId);
		return await q.Select(Projections.ParkingStaffSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateParkingStaff(ParkingStaffUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		ParkingStaffEntity? e = await db.Set<ParkingStaffEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("parkingStaffMemberNotFound"));
		// Same rule as CreateParkingStaff; without it any signed-in user could reset any staff member's password.
		if ((await Access(userData, e.ParkingId, ct)).Error is { } accessError) return accessError;

		if (p.ShiftTitle.IsNotNull()) e.ShiftTitle = p.ShiftTitle;
		if (p.MaxDiscountPercent.IsNotNull()) e.MaxDiscountPercent = p.MaxDiscountPercent.Value;

		if (p.Password.IsNotNullOrEmpty()) {
			UserEntity? user = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == e.UserId, ct);
			if (user != null) {
				user.Password = UPasswordHasher.Hash(p.Password);
			}
		}

		e.ApplyUpdateParam<ParkingStaffEntity, TagParkingStaff, ParkingStaffJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteParkingStaff(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		ParkingStaffEntity? e = await db.Set<ParkingStaffEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("parkingStaffMemberNotFound"));

		if ((await Access(userData, e.ParkingId, ct)).Error is { } accessError) return accessError;
		ParkingEntity? parking = await db.Set<ParkingEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == e.ParkingId, ct);
		if (parking != null) {
			parking.AdminUserIds.Remove(e.UserId);
		}

		db.Set<ParkingStaffEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<ParkingShiftResponse?>> OpenParkingShift(ParkingShiftOpenParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingShiftResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		if ((await Access(userData, p.ParkingId, ct, TagParkingStaff.RegisterEntryExit)).Error is { } accessError) return new UResponse<ParkingShiftResponse?>(null, accessError.Status, accessError.Message);
		ParkingShiftEntity? open = await db.Set<ParkingShiftEntity>().FirstOrDefaultAsync(x => x.ParkingId == p.ParkingId && x.CreatorId == userData.Id && x.EndDate == null, ct);
		if (open == null) {
			open = new ParkingShiftEntity {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				JsonData = new ParkingShiftJson(),
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
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<ParkingShiftResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<ParkingShiftResponse>?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		IQueryable<ParkingShiftEntity> q = db.Set<ParkingShiftEntity>().ApplyReadParams(p);
		if (!userData.IsAdmin) {
			Guid uid = userData.Id;
			IQueryable<Guid> mine = ParkingsOf(uid), limited = NoReportsIn(uid);
			q = q.Where(x => mine.Contains(x.ParkingId) && (!limited.Contains(x.ParkingId) || x.CreatorId == uid));
		}
		if (p.ParkingId.IsNotNull()) q = q.Where(x => x.ParkingId == p.ParkingId);
		if (p.IsOpen == true) q = q.Where(x => x.EndDate == null);
		if (p.IsOpen == false) q = q.Where(x => x.EndDate != null);
		return await q.Select(Projections.ParkingShiftSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ParkingShiftResponse?>> CloseParkingShift(ParkingShiftCloseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingShiftResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		ParkingShiftEntity? e = await db.Set<ParkingShiftEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<ParkingShiftResponse?>(null, Usc.NotFound, ls.Get("parkingShiftNotFound"));
		if (e.CreatorId != userData.Id && (await Access(userData, e.ParkingId, ct)).Error is { } accessError) return new UResponse<ParkingShiftResponse?>(null, accessError.Status, accessError.Message);
		if (e.EndDate != null) return new UResponse<ParkingShiftResponse?>(null, Usc.Conflict, ls.Get("theShiftWasClosed"));

		e.EndDate = DateTime.UtcNow;
		e.CountedCash = p.CountedCash;
		e.Tags = [TagParkingShift.Closed];
		await db.SaveChangesAsync(ct);

		ParkingShiftResponse? result = await db.Set<ParkingShiftEntity>().Where(x => x.Id == e.Id).Select(Projections.ParkingShiftSelector(new ParkingShiftSelectorArgs())).FirstOrDefaultAsync(ct);
		return new UResponse<ParkingShiftResponse?>(result);
	}

	public async Task<UResponse<ParkingPlateStatusResponse?>> ReadParkingPlateStatus(ParkingPlateStatusParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingPlateStatusResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		if ((await Access(userData, p.ParkingId, ct, TagParkingStaff.RegisterEntryExit)).Error is { } accessError) return new UResponse<ParkingPlateStatusResponse?>(null, accessError.Status, accessError.Message);

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
		if (userData == null) return new UResponse<ParkingReportResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		if ((await Access(userData, p.ParkingId, ct, TagParkingStaff.RegisterEntryExit)).Error is { } accessError) return new UResponse<ParkingReportResponse?>(null, accessError.Status, accessError.Message);

		DateTime now = DateTime.UtcNow;
		DateTime start = p.StartDate ?? now;

		VehicleEntity vehicle = await GetOrCreateVehicle(p.LicencePlate, p.VehicleType, userData.Id, ct);

		bool alreadyInside = await db.Set<ParkingReportEntity>().AnyAsync(x => x.ParkingId == p.ParkingId && x.VehicleId == vehicle.Id && x.EndDate == null, ct);
		if (alreadyInside) return new UResponse<ParkingReportResponse?>(null, Usc.Conflict, ls.Get("thisVehicleIsAlreadyInsideTheParking"));

		ParkingSubscriptionEntity? subscription = await db.Set<ParkingSubscriptionEntity>()
			.FirstOrDefaultAsync(x => x.ParkingId == p.ParkingId && x.VehicleId == vehicle.Id && x.ExpiryDate > now && !x.Tags.Contains(TagParkingSubscription.Cancelled), ct);

		ParkingShiftEntity? shift = await db.Set<ParkingShiftEntity>().AsTracking().FirstOrDefaultAsync(x => x.ParkingId == p.ParkingId && x.CreatorId == userData.Id && x.EndDate == null, ct);
		if (shift != null) {
			shift.EntryCount++;
		}

		ParkingPlateFlagEntity? reservation = await db.Set<ParkingPlateFlagEntity>()
			.FirstOrDefaultAsync(x => x.ParkingId == p.ParkingId && x.LicencePlate == p.LicencePlate && x.Tags.Contains(TagParkingPlateFlag.Reservation) && (x.ToDate == null || x.ToDate > now), ct);

		ParkingReportEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = now,
			JsonData = new ParkingReportJson(),
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

	private async Task<string> NextReceiptNumber(Guid parkingId, DateTime now, CancellationToken ct) {
		DateTime dayStart = now.Date;
		DateTime dayEnd = dayStart.AddDays(1);
		int count = await db.Set<ParkingReportEntity>().CountAsync(x => x.ParkingId == parkingId && x.CreatedAt >= dayStart && x.CreatedAt < dayEnd, ct);
		return $"{now:yyyyMMdd}-{count + 1:D4}";
	}

	public async Task<UResponse<ParkingBillResponse?>> CalculateParkingExit(ParkingExitCalculateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingBillResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		ParkingReportEntity? report = await FindOpenReport(p.ReportId, p.ParkingId, p.LicencePlate, ct);
		if (report == null) return new UResponse<ParkingBillResponse?>(null, Usc.NotFound, ls.Get("parkingReportNotFound"));
		(_, ParkingStaffEntity? staff, UResponse? accessError) = await Access(userData, report.ParkingId, ct, TagParkingStaff.RegisterEntryExit);
		if (accessError != null) return new UResponse<ParkingBillResponse?>(null, accessError.Status, accessError.Message);
		if (DiscountError(staff, report, p.Discount, await BuildBill(report, p.CorrectedStartDate, p.EndDate, 0, ct)) is { } discountError) return new UResponse<ParkingBillResponse?>(null, Usc.Forbidden, discountError);

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

		// Memoized per request (the service is scoped): listing inside vehicles builds a bill per row with the same parking/tariffs.
		if (!_tariffs.TryGetValue((report.ParkingId, vehicleType), out ParkingTariffEntity? tariff)) {
			tariff = await db.Set<ParkingTariffEntity>().FirstOrDefaultAsync(x => x.ParkingId == report.ParkingId && x.VehicleType == vehicleType, ct);
			_tariffs[(report.ParkingId, vehicleType)] = tariff;
		}

		if (!_parkings.TryGetValue(report.ParkingId, out ParkingEntity? parking)) {
			parking = await db.Set<ParkingEntity>().FirstOrDefaultAsync(x => x.Id == report.ParkingId, ct);
			_parkings[report.ParkingId] = parking;
		}

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

	private string? DiscountError(ParkingStaffEntity? staff, ParkingReportEntity report, decimal discount, ParkingBillResponse gross) {
		if (discount < 0) return ls.Get("amountIsNotValid");
		if (discount == 0 || staff == null) return null;
		if (staff.Tags.Count > 0 && !staff.Tags.Contains(TagParkingStaff.ApplyManualDiscount)) return ls.Get("youDoNotHaveClearanceToDoThisAction");
		return discount > Math.Round(gross.Subtotal * staff.MaxDiscountPercent / 100m) ? ls.Get("discountIsMoreThanAllowed") : null;
	}

	private static bool IsWeekend(DateTime date) => date.DayOfWeek is DayOfWeek.Thursday or DayOfWeek.Friday;

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
		if (userData == null) return new UResponse<ParkingReportResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		ParkingReportEntity? report = await db.Set<ParkingReportEntity>().AsTracking().Include(x => x.Vehicle).FirstOrDefaultAsync(x => x.Id == p.ReportId, ct);
		if (report == null) return new UResponse<ParkingReportResponse?>(null, Usc.NotFound, ls.Get("parkingReportNotFound"));
		if (report.EndDate.IsNotNull()) return new UResponse<ParkingReportResponse?>(null, Usc.Conflict, ls.Get("thisEntryHasAlreadyBeenClosed"));
		(_, ParkingStaffEntity? staff, UResponse? accessError) = await Access(userData, report.ParkingId, ct, TagParkingStaff.RegisterEntryExit);
		if (accessError != null) return new UResponse<ParkingReportResponse?>(null, accessError.Status, accessError.Message);
		if (DiscountError(staff, report, p.Discount, await BuildBill(report, p.CorrectedStartDate, p.EndDate, 0, ct)) is { } discountError) return new UResponse<ParkingReportResponse?>(null, Usc.Forbidden, discountError);

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

		ParkingShiftEntity? shift = await db.Set<ParkingShiftEntity>().AsTracking().FirstOrDefaultAsync(x => x.ParkingId == report.ParkingId && x.CreatorId == userData.Id && x.EndDate == null, ct);
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
		}

		await db.SaveChangesAsync(ct);

		ParkingReportResponse? result = await db.Set<ParkingReportEntity>().Where(x => x.Id == report.Id)
			.Select(Projections.ParkingReportSelector(new ParkingReportSelectorArgs { Vehicle = new VehicleSelectorArgs(), Parking = new ParkingSelectorArgs() }))
			.FirstOrDefaultAsync(ct);
		return new UResponse<ParkingReportResponse?>(result);
	}

	public async Task<UResponse<ParkingDashboardResponse?>> ReadParkingDashboard(ParkingDashboardParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ParkingDashboardResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		(ParkingEntity? parking, _, UResponse? accessError) = await Access(userData, p.ParkingId, ct, anyStaff: true);
		if (accessError != null) return new UResponse<ParkingDashboardResponse?>(null, accessError.Status, accessError.Message);

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
			ParkingId = parking!.Id,
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
		if (userData == null) return new UResponse<IEnumerable<ParkingInsideVehicleResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		if ((await Access(userData, p.ParkingId, ct, TagParkingStaff.RegisterEntryExit)).Error is { } accessError) return new UResponse<IEnumerable<ParkingInsideVehicleResponse>?>(null, accessError.Status, accessError.Message);

		DateTime now = DateTime.UtcNow;
		IQueryable<ParkingReportEntity> q = db.Set<ParkingReportEntity>().Include(x => x.Vehicle).Where(x => x.ParkingId == p.ParkingId && x.EndDate == null);

		if (p.Query.IsNotNullOrEmpty()) q = q.Where(x => x.Vehicle.LicencePlate.Contains(p.Query!));
		if (p.LongerThanADay == true) q = q.Where(x => x.StartDate <= now.AddDays(-1));
		if (p.HasSubscription == true) q = q.Where(x => x.SubscriptionId != null);

		int pageSize = Math.Max(1, p.PageSize);
		int totalCount = await q.CountAsync(ct);
		List<ParkingReportEntity> reports = await q
			.OrderBy(x => x.StartDate)
			.Skip((Math.Max(1, p.PageNumber) - 1) * pageSize)
			.Take(pageSize)
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
			PageSize = pageSize,
			PageCount = (int)Math.Ceiling(totalCount / (decimal)pageSize)
		};
	}

	private const string SeedNamespace = "avapark-seed";
	private const string DefaultPassword = "Ava@12345";

	private static readonly string[] PlateLetters = ["ب", "ج", "د", "س", "ص", "ط", "ق", "ل", "ن", "و"];

	private static readonly (string Title, string Address, string Phone, int Capacity)[] Parkings = [
		("پارکینگ مرکزی ونک", "تهران، خیابان ولیعصر، بالاتر از میدان ونک، پلاک ۱۲۴۰", "02188776655", 180),
		("پارکینگ طبقاتی سعادت‌آباد", "تهران، میدان کاج، ضلع شمالی", "02122334455", 95),
		("پارکینگ نمایشگاه", "تهران، بزرگراه چمران، محل دائمی نمایشگاه‌ها", "02166778899", 240)
	];

	private static readonly (string UserName, string First, string Last, string Phone, string Shift, int Discount, TagParkingStaff[] Permissions)[] Operators = [
		("z.ahmadi", "زهرا", "احمدی", "09121112233", "شیفت عصر", 0, [TagParkingStaff.RegisterEntryExit]),
		("a.karami", "علی", "کرمی", "09122223344", "شیفت شب", 10,
			[TagParkingStaff.RegisterEntryExit, TagParkingStaff.ApplyManualDiscount, TagParkingStaff.ManageSubscriptions]),
		("h.nouri", "حسین", "نوری", "09123334455", "شیفت روز", 0, [TagParkingStaff.RegisterEntryExit, TagParkingStaff.Disabled])
	];

	private static readonly (TagVehicle Type, decimal Entrance, decimal Day, decimal Night, decimal Cap, decimal Weekly, decimal Monthly, decimal Quarterly)[] Tariffs = [
		(TagVehicle.Car, 15000, 12000, 8000, 140000, 280000, 950000, 2600000),
		(TagVehicle.Motorcycle, 6000, 5000, 3500, 60000, 120000, 400000, 1100000),
		(TagVehicle.Pickup, 18000, 14000, 9500, 165000, 340000, 1150000, 3200000),
		(TagVehicle.Van, 20000, 16000, 11000, 185000, 380000, 1300000, 3600000),
		(TagVehicle.Truck, 24000, 19000, 13000, 220000, 450000, 1550000, 4300000),
		(TagVehicle.Electric, 12000, 10000, 7000, 120000, 240000, 800000, 2200000)
	];

	/// Deterministic v5-style id so the same logical row keeps the same key between runs.
	private static Guid SeedId(string key) {
		byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes($"{SeedNamespace}:{key}"));
		return new Guid(hash);
	}

	/// One-call demo data for the AvaPark POS: two parkings, an owner, three operators, tariffs for
	/// every vehicle type, subscriptions in all three states, flagged plates, shifts and a week of traffic.
	/// Every row uses a fixed id derived from <see cref="SeedNamespace"/> so a re-run replaces its own data
	/// and never touches anything else in the database.
	public async Task<UResponse<ParkingSeedResponse?>> SeedParking(ParkingSeedParams p, CancellationToken ct) {
		if (ts.ExtractClaims(p.Token) is not { IsSystemAdmin: true }) return new UResponse<ParkingSeedResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		DateTime now = DateTime.UtcNow;
		Guid ownerId = SeedId("user:owner");
		List<Guid> parkingIds = [.. Enumerable.Range(0, Parkings.Length).Select(i => SeedId($"parking:{i}"))];
		List<Guid> operatorIds = [.. Operators.Select(o => SeedId($"user:{o.UserName}"))];

		if (p.Reset) await Wipe(parkingIds, ownerId, ct);

		ParkingSeedResponse result = new();
		Random random = new(20260827);

		await SeedAccounts(ownerId, operatorIds, now, result, ct);
		await SeedParkings(parkingIds, ownerId, operatorIds, now, ct);
		await SeedTariffs(parkingIds, ownerId, now, result, ct);
		List<VehicleEntity> vehicles = await SeedVehicles(ownerId, now, random, result, ct);
		await SeedSubscriptions(parkingIds[0], ownerId, vehicles, now, result, ct);
		await SeedPlateFlags(parkingIds[0], ownerId, vehicles, now, result, ct);
		await SeedStaff(parkingIds, ownerId, operatorIds, now, result, ct);
		List<Guid> shiftIds = await SeedShifts(parkingIds[0], ownerId, operatorIds, now, result, ct);
		await SeedReports(p, parkingIds[0], ownerId, operatorIds, vehicles, shiftIds, now, random, result, ct);

		await db.SaveChangesAsync(ct);

		result.Parkings = await db.Set<ParkingEntity>()
			.Where(x => parkingIds.Contains(x.Id))
			.Select(Projections.ParkingSelector(new ParkingSelectorArgs()))
			.ToListAsync(ct);

		return new UResponse<ParkingSeedResponse?>(result, Usc.Created);
	}

	/// Only parking-scoped rows and the seed's own vehicles are removed. The seed users are kept and
	/// updated in place — deleting them would break foreign keys from unrelated tables such as ApiLogs.
	private async Task Wipe(List<Guid> parkingIds, Guid ownerId, CancellationToken ct) {
		await db.Set<ParkingReportEntity>().Where(x => parkingIds.Contains(x.ParkingId)).ExecuteDeleteAsync(ct);
		await db.Set<ParkingSubscriptionEntity>().Where(x => parkingIds.Contains(x.ParkingId)).ExecuteDeleteAsync(ct);
		await db.Set<ParkingPlateFlagEntity>().Where(x => parkingIds.Contains(x.ParkingId)).ExecuteDeleteAsync(ct);
		await db.Set<ParkingStaffEntity>().Where(x => parkingIds.Contains(x.ParkingId)).ExecuteDeleteAsync(ct);
		await db.Set<ParkingShiftEntity>().Where(x => parkingIds.Contains(x.ParkingId)).ExecuteDeleteAsync(ct);
		await db.Set<ParkingTariffEntity>().Where(x => parkingIds.Contains(x.ParkingId)).ExecuteDeleteAsync(ct);
		await db.Set<ParkingEntity>().Where(x => parkingIds.Contains(x.Id)).ExecuteDeleteAsync(ct);
		await db.Set<VehicleEntity>().Where(x => x.CreatorId == ownerId).ExecuteDeleteAsync(ct);
	}

	/// Inserts the account on the first run and refreshes it on every later one, so re-seeding never
	/// has to delete a user other tables may already point at.
	private async Task UpsertUser(Guid id, string userName, string first, string last, string? phone, DateTime now, TagUser[] tags, CancellationToken ct) {
		UserEntity? existing = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == id, ct);
		if (existing == null) {
			await db.Set<UserEntity>().AddAsync(BuildUser(id, userName, first, last, phone, now, tags), ct);
			return;
		}

		existing.UserName = userName;
		existing.FirstName = first;
		existing.LastName = last;
		existing.PhoneNumber = phone;
		existing.Password = UPasswordHasher.Hash(DefaultPassword);
		existing.Tags = [.. tags];
		db.Set<UserEntity>().Update(existing);
	}

	private UserEntity BuildUser(Guid id, string userName, string first, string last, string? phone, DateTime now, TagUser[] tags) => new() {
		Id = id,
		CreatedAt = now,
		CreatorId = id,
		JsonData = new UserJson(),
		Tags = [.. tags],
		UserName = userName,
		Password = UPasswordHasher.Hash(DefaultPassword),
		RefreshToken = ts.GenerateRefreshToken(),
		FirstName = first,
		LastName = last,
		PhoneNumber = phone,
		Wallets = [new WalletEntity { Id = id, CreatorId = id, CreatedAt = now, JsonData = new WalletJson(), Tags = [TagWallet.Primary], Balance = 0 }]
	};

	private async Task SeedAccounts(Guid ownerId, List<Guid> operatorIds, DateTime now, ParkingSeedResponse result, CancellationToken ct) {
		await UpsertUser(ownerId, "m.rezaei", "مهدی", "رضایی", "09120001122", now, [TagUser.Verified, TagUser.SubAdmin], ct);
		result.Accounts.Add(new ParkingSeedAccountResponse { UserName = "m.rezaei", Password = DefaultPassword, FullName = "مهدی رضایی", Role = "Owner" });

		for (int i = 0; i < Operators.Length; i++) {
			(string userName, string first, string last, string phone, _, _, _) = Operators[i];
			await UpsertUser(operatorIds[i], userName, first, last, phone, now, [TagUser.Verified], ct);
			result.Accounts.Add(new ParkingSeedAccountResponse { UserName = userName, Password = DefaultPassword, FullName = $"{first} {last}", Role = "Operator" });
		}
	}

	private async Task SeedParkings(List<Guid> parkingIds, Guid ownerId, List<Guid> operatorIds, DateTime now, CancellationToken ct) {
		for (int i = 0; i < Parkings.Length; i++) {
			(string title, string address, string phone, int capacity) = Parkings[i];
			await db.Set<ParkingEntity>().AddAsync(new ParkingEntity {
				Id = parkingIds[i],
				CreatedAt = now,
				CreatorId = ownerId,
				JsonData = new ParkingJson(),
				Tags = i == 2 ? [TagParking.Disabled] : [TagParking.Active],
				Title = title,
				Address = address,
				PhoneNumber = phone,
				Capacity = capacity,
				EntrancePrice = 15000,
				HourlyPrice = 12000,
				DailyPrice = 140000,
				AdminUserIds = i == 0 ? [.. operatorIds] : [operatorIds[0]]
			}, ct);
		}
	}

	private async Task SeedTariffs(List<Guid> parkingIds, Guid ownerId, DateTime now, ParkingSeedResponse result, CancellationToken ct) {
		foreach (Guid parkingId in parkingIds) {
			foreach ((TagVehicle type, decimal entrance, decimal day, decimal night, decimal cap, decimal weekly, decimal monthly, decimal quarterly) in Tariffs) {
				await db.Set<ParkingTariffEntity>().AddAsync(new ParkingTariffEntity {
					Id = SeedId($"tariff:{parkingId}:{type}"),
					CreatedAt = now,
					CreatorId = ownerId,
					JsonData = new ParkingTariffJson(),
					Tags = [TagParkingTariff.Hourly, TagParkingTariff.Subscription],
					ParkingId = parkingId,
					VehicleType = type,
					EntrancePrice = entrance,
					DayHourlyPrice = day,
					NightHourlyPrice = night,
					DailyCap = cap,
					WeeklyPrice = weekly,
					MonthlyPrice = monthly,
					QuarterlyPrice = quarterly,
					FreeMinutes = 15,
					NightStartHour = 22,
					NightEndHour = 6,
					HolidayExtraPercent = 20,
					RoundToFullHour = false,
					PerMinuteAfterFirstHour = true,
					SubscriptionDailyEntryLimit = 4,
					SubscriptionOfficeHoursOnly = false,
					SubscriptionExpiryReminderDays = 5
				}, ct);
				result.Tariffs++;
			}
		}
	}

	private async Task<List<VehicleEntity>> SeedVehicles(Guid ownerId, DateTime now, Random random, ParkingSeedResponse result, CancellationToken ct) {
		List<VehicleEntity> vehicles = [];
		TagVehicle[] types = [.. Tariffs.Select(t => t.Type)];

		for (int i = 0; i < 60; i++) {
			TagVehicle type = types[i % types.Length];
			string plate = $"{random.Next(11, 99)}{PlateLetters[i % PlateLetters.Length]}{random.Next(100, 999)}{random.Next(10, 99)}";
			VehicleEntity vehicle = new() {
				Id = SeedId($"vehicle:{i}"),
				CreatedAt = now,
				CreatorId = ownerId,
				JsonData = new VehicleJson(),
				Tags = [type],
				LicencePlate = plate
			};
			vehicles.Add(vehicle);
			await db.Set<VehicleEntity>().AddAsync(vehicle, ct);
			result.Vehicles++;
		}
		return vehicles;
	}

	private async Task SeedSubscriptions(Guid parkingId, Guid ownerId, List<VehicleEntity> vehicles, DateTime now, ParkingSeedResponse result, CancellationToken ct) {
		// Spread across active, about-to-expire and already-expired so every filter tab has rows.
		(int VehicleIndex, TagParkingSubscription Duration, int DaysLeft, string Name, string Phone)[] rows = [
			(0, TagParkingSubscription.Monthly, 23, "رضا محمدی", "09121112233"),
			(1, TagParkingSubscription.Quarterly, 46, "شرکت آریا تجارت", "09122223344"),
			(2, TagParkingSubscription.Monthly, 12, "نگار سلطانی", "09123334455"),
			(3, TagParkingSubscription.Weekly, 3, "سعید کاظمی", "09124445566"),
			(4, TagParkingSubscription.Monthly, 4, "مریم حیدری", "09125556677"),
			(5, TagParkingSubscription.Weekly, -6, "بهرام یوسفی", "09126667788"),
			(6, TagParkingSubscription.Monthly, -21, "الهام رستمی", "09127778899")
		];

		foreach ((int vehicleIndex, TagParkingSubscription duration, int daysLeft, string name, string phone) in rows) {
			int periodDays = duration switch {
				TagParkingSubscription.Weekly => 7,
				TagParkingSubscription.Quarterly => 90,
				_ => 30
			};
			decimal price = duration switch {
				TagParkingSubscription.Weekly => Tariffs[0].Weekly,
				TagParkingSubscription.Quarterly => Tariffs[0].Quarterly,
				_ => Tariffs[0].Monthly
			};

			await db.Set<ParkingSubscriptionEntity>().AddAsync(new ParkingSubscriptionEntity {
				Id = SeedId($"subscription:{vehicleIndex}"),
				CreatedAt = now,
				CreatorId = ownerId,
				JsonData = new ParkingSubscriptionJson(),
				Tags = [duration],
				ParkingId = parkingId,
				VehicleId = vehicles[vehicleIndex].Id,
				CustomerName = name,
				CustomerPhoneNumber = phone,
				Price = price,
				StartDate = now.AddDays(daysLeft - periodDays),
				ExpiryDate = now.AddDays(daysLeft),
				DailyEntryLimit = 4,
				OfficeHoursOnly = false
			}, ct);
			result.Subscriptions++;
		}
	}

	private async Task SeedPlateFlags(Guid parkingId, Guid ownerId, List<VehicleEntity> vehicles, DateTime now, ParkingSeedResponse result, CancellationToken ct) {
		(int VehicleIndex, TagParkingPlateFlag Kind, string Reason, decimal? Amount, string? Spot)[] rows = [
			(10, TagParkingPlateFlag.Debt, "بدهی پرداخت‌نشده از تردد قبلی", 84000, null),
			(11, TagParkingPlateFlag.Banned, "خسارت به تجهیزات · مسدود دائم", null, null),
			(12, TagParkingPlateFlag.Warning, "۲ بار خروج بدون پرداخت · ورود با تأیید", null, null),
			(13, TagParkingPlateFlag.Debt, "عدم تسویه صورتحساب ماه گذشته", 42000, null),
			(0, TagParkingPlateFlag.Reservation, "رزرو روزانه مشترک ماهانه", null, "B-24"),
			(14, TagParkingPlateFlag.Reservation, "رزرو جلسه هیئت مدیره", null, "A-09")
		];

		foreach ((int vehicleIndex, TagParkingPlateFlag kind, string reason, decimal? amount, string? spot) in rows) {
			await db.Set<ParkingPlateFlagEntity>().AddAsync(new ParkingPlateFlagEntity {
				Id = SeedId($"flag:{vehicleIndex}:{kind}"),
				CreatedAt = now,
				CreatorId = ownerId,
				JsonData = new ParkingPlateFlagJson(),
				Tags = [kind],
				ParkingId = parkingId,
				LicencePlate = vehicles[vehicleIndex].LicencePlate,
				Reason = reason,
				Amount = amount,
				SpotNumber = spot,
				FromDate = kind == TagParkingPlateFlag.Reservation ? now.AddHours(-2) : null,
				ToDate = kind == TagParkingPlateFlag.Reservation ? now.AddHours(6) : null
			}, ct);
			result.PlateFlags++;
		}
	}

	private async Task SeedStaff(List<Guid> parkingIds, Guid ownerId, List<Guid> operatorIds, DateTime now, ParkingSeedResponse result, CancellationToken ct) {
		await db.Set<ParkingStaffEntity>().AddAsync(new ParkingStaffEntity {
			Id = SeedId("staff:owner"),
			CreatedAt = now,
			CreatorId = ownerId,
			JsonData = new ParkingStaffJson(),
			Tags = [
				TagParkingStaff.RegisterEntryExit, TagParkingStaff.ApplyManualDiscount, TagParkingStaff.ManageSubscriptions,
				TagParkingStaff.ChangeTariff, TagParkingStaff.ViewFinancialReports
			],
			ParkingId = parkingIds[0],
			UserId = ownerId,
			ShiftTitle = "شیفت صبح",
			MaxDiscountPercent = 100
		}, ct);
		result.Staff++;

		for (int i = 0; i < Operators.Length; i++) {
			(_, _, _, _, string shift, int discount, TagParkingStaff[] permissions) = Operators[i];
			await db.Set<ParkingStaffEntity>().AddAsync(new ParkingStaffEntity {
				Id = SeedId($"staff:{i}"),
				CreatedAt = now,
				CreatorId = ownerId,
				JsonData = new ParkingStaffJson(),
				Tags = [.. permissions],
				ParkingId = parkingIds[0],
				UserId = operatorIds[i],
				ShiftTitle = shift,
				MaxDiscountPercent = discount
			}, ct);
			result.Staff++;
		}
	}

	private async Task<List<Guid>> SeedShifts(Guid parkingId, Guid ownerId, List<Guid> operatorIds, DateTime now, ParkingSeedResponse result, CancellationToken ct) {
		List<Guid> shiftIds = [];

		// Three closed shifts over the past days, then one still open for the owner so the POS lands on a live shift.
		for (int i = 3; i >= 1; i--) {
			Guid id = SeedId($"shift:closed:{i}");
			shiftIds.Add(id);
			await db.Set<ParkingShiftEntity>().AddAsync(new ParkingShiftEntity {
				Id = id,
				CreatedAt = now.AddDays(-i),
				CreatorId = operatorIds[i % operatorIds.Count],
				JsonData = new ParkingShiftJson(),
				Tags = [TagParkingShift.Closed],
				ParkingId = parkingId,
				StartDate = now.AddDays(-i).Date.AddHours(8),
				EndDate = now.AddDays(-i).Date.AddHours(20),
				CashTotal = 1_240_000 + i * 90_000,
				CardTotal = 3_180_000 + i * 120_000,
				IpgTotal = 640_000 + i * 40_000,
				CountedCash = 1_240_000 + i * 90_000,
				EntryCount = 60 + i * 4,
				ExitCount = 58 + i * 4
			}, ct);
			result.Shifts++;
		}

		Guid openId = SeedId("shift:open");
		shiftIds.Add(openId);
		await db.Set<ParkingShiftEntity>().AddAsync(new ParkingShiftEntity {
			Id = openId,
			CreatedAt = now.AddHours(-4),
			CreatorId = ownerId,
			JsonData = new ParkingShiftJson(),
			Tags = [TagParkingShift.Open],
			ParkingId = parkingId,
			StartDate = now.AddHours(-4),
			CashTotal = 0,
			CardTotal = 0,
			IpgTotal = 0,
			EntryCount = 0,
			ExitCount = 0
		}, ct);
		result.Shifts++;

		return shiftIds;
	}

	private async Task SeedReports(
		ParkingSeedParams p,
		Guid parkingId,
		Guid ownerId,
		List<Guid> operatorIds,
		List<VehicleEntity> vehicles,
		List<Guid> shiftIds,
		DateTime now,
		Random random,
		ParkingSeedResponse result,
		CancellationToken ct) {
		TagParkingPayment[] methods = [TagParkingPayment.Card, TagParkingPayment.Cash, TagParkingPayment.Ipg];
		Guid openShiftId = shiftIds[^1];
		int receipt = 1;

		for (int i = 0; i < p.ClosedEntries; i++) {
			VehicleEntity vehicle = vehicles[random.Next(20, vehicles.Count)];
			DateTime start = now.AddDays(-random.Next(1, 7)).Date.AddHours(random.Next(7, 20)).AddMinutes(random.Next(0, 60));
			DateTime end = start.AddMinutes(random.Next(25, 900));
			decimal amount = Math.Round((decimal)(15000 + random.Next(1, 12) * 12000) / 1000, 0) * 1000;
			TagParkingPayment method = methods[random.Next(methods.Length)];

			await db.Set<ParkingReportEntity>().AddAsync(new ParkingReportEntity {
				Id = SeedId($"report:closed:{i}"),
				CreatedAt = start,
				CreatorId = operatorIds[random.Next(operatorIds.Count)],
				JsonData = new ParkingReportJson(),
				Tags = [TagParkingReport.Closed],
				ParkingId = parkingId,
				VehicleId = vehicle.Id,
				StartDate = start,
				EndDate = end,
				Amount = amount,
				PaidAmount = amount,
				PaymentMethod = method,
				TrackingCode = method == TagParkingPayment.Cash ? null : random.Next(10_000_000, 99_999_999).ToString(),
				ReceiptNumber = $"{start:yyyyMMdd}-{receipt++:D4}",
				SpotNumber = $"{(char)('A' + random.Next(0, 4))}-{random.Next(1, 60):D2}",
				ShiftId = shiftIds[random.Next(shiftIds.Count - 1)]
			}, ct);
			result.ClosedReports++;
		}

		for (int i = 0; i < p.OpenEntries; i++) {
			VehicleEntity vehicle = vehicles[i];
			DateTime start = now.AddMinutes(-random.Next(10, 1500));
			// The first few open entries belong to subscribed plates so the exit flow has free stays to settle.
			Guid? subscriptionId = i < 3 ? SeedId($"subscription:{i}") : null;

			await db.Set<ParkingReportEntity>().AddAsync(new ParkingReportEntity {
				Id = SeedId($"report:open:{i}"),
				CreatedAt = start,
				CreatorId = ownerId,
				JsonData = new ParkingReportJson(),
				Tags = [TagParkingReport.Open],
				ParkingId = parkingId,
				VehicleId = vehicle.Id,
				StartDate = start,
				ReceiptNumber = $"{start:yyyyMMdd}-{receipt++:D4}",
				SpotNumber = $"B-{i + 10:D2}",
				CustomerPhoneNumber = i % 4 == 0 ? "09121234567" : null,
				SubscriptionId = subscriptionId,
				ShiftId = openShiftId
			}, ct);
			result.OpenReports++;
		}
	}
}
