namespace SinaMN75U.Services;

public interface IParkingSeedService {
	Task<UResponse<ParkingSeedResponse?>> SeedParking(ParkingSeedParams p, CancellationToken ct);
}

/// One-call demo data for the AvaPark POS: two parkings, an owner, three operators, tariffs for
/// every vehicle type, subscriptions in all three states, flagged plates, shifts and a week of traffic.
/// Every row uses a fixed id derived from <see cref="SeedNamespace"/> so a re-run replaces its own data
/// and never touches anything else in the database.
public class ParkingSeedService(DbContext db, ITokenService ts) : IParkingSeedService {
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

	public async Task<UResponse<ParkingSeedResponse?>> SeedParking(ParkingSeedParams p, CancellationToken ct) {
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
				JsonData = new BaseJson(),
				// The third parking is disabled so the parking picker has a greyed-out card to render.
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
					JsonData = new BaseJson(),
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
				JsonData = new BaseJson(),
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
				JsonData = new BaseJson(),
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
				JsonData = new BaseJson(),
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
			JsonData = new BaseJson(),
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
				JsonData = new BaseJson(),
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
				JsonData = new BaseJson(),
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
			JsonData = new BaseJson(),
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
				JsonData = new BaseJson(),
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
				JsonData = new BaseJson(),
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
