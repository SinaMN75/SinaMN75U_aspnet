namespace SinaMN75U.Services;

public interface IVenueService {
	public Task<UResponse<Guid?>> CreateVenue(VenueCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<VenueResponse>?>> ReadVenues(VenueReadParams p, CancellationToken ct);
	public Task<UResponse<VenueResponse?>> ReadVenueById(IdParams<VenueSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateVenue(VenueUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteVenue(IdParams p, CancellationToken ct);
	public Task<UResponse<VenueStatsResponse?>> ReadVenueStats(VenueStatsParams p, CancellationToken ct);

	public Task<UResponse<Guid?>> CreateCourt(CourtCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<CourtResponse>?>> ReadCourts(CourtReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateCourt(CourtUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteCourt(IdParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<CourtAvailabilityResponse>?>> ReadCourtAvailability(CourtAvailabilityParams p, CancellationToken ct);

	public Task<UResponse<BookingResponse?>> CreateBooking(BookingCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<BookingResponse>?>> ReadBookings(BookingReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateBooking(BookingUpdateParams p, CancellationToken ct);
	public Task<UResponse> CancelBooking(BookingCancelParams p, CancellationToken ct);
	public Task<UResponse> PayBookingShare(IdParams p, CancellationToken ct);
}

public class VenueService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IWalletService ws,
	IRealtimeService rt
) : IVenueService {
	// ---- Who sees and changes what ----
	// Anybody signed in adds a venue; it is listed once an admin approves it.
	// The creator owns it, AdminUserIds are its staff: they manage its courts and bookings.
	// Booking money is held by the platform and paid to the owner when the booking is completed (or a no-show).

	private static bool CanManage(JwtClaimData u, VenueEntity v) => u.IsAdmin || v.CreatorId == u.Id || v.AdminUserIds.Contains(u.Id);

	private static readonly TagBooking[] ActiveBookings = [TagBooking.Pending, TagBooking.Confirmed, TagBooking.Completed, TagBooking.NoShow];

	private static TimeZoneInfo ZoneOf(VenueEntity v) {
		try {
			return TimeZoneInfo.FindSystemTimeZoneById(v.JsonData.TimeZone);
		}
		catch {
			return TimeZoneInfo.Utc;
		}
	}

	private static DateTime ToUtc(DateTime local, TimeZoneInfo zone) => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), zone);

	private static DateTime ToLocal(DateTime utc, TimeZoneInfo zone) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone);

	private static TimeSpan ParseTime(string value) => value == "24:00" ? TimeSpan.FromHours(24) : TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out TimeSpan t) ? t : TimeSpan.Zero;

	/// <summary>The open periods of a local day; no opening hours = open all day.</summary>
	private static List<(DateTime From, DateTime To)> OpenPeriods(VenueEntity v, DateTime localDay) {
		if (v.JsonData.OpeningHours.Count == 0) return [(localDay, localDay.AddDays(1))];
		return v.JsonData.OpeningHours
			.Where(x => x.Day == (int)localDay.DayOfWeek)
			.Select(x => (localDay + ParseTime(x.Open), localDay + ParseTime(x.Close)))
			.Where(x => x.Item2 > x.Item1)
			.OrderBy(x => x.Item1)
			.ToList();
	}

	/// <summary>The price of a court from a local start time for some minutes; a matching price rule wins over the base price.</summary>
	private static decimal PriceOf(CourtEntity c, DateTime localStart, int minutes) {
		TimeSpan time = localStart.TimeOfDay;
		CourtPriceRule? rule = c.JsonData.PriceRules.FirstOrDefault(r =>
			(r.Days.Count == 0 || r.Days.Contains((int)localStart.DayOfWeek)) && time >= ParseTime(r.From) && time < ParseTime(r.To));
		return Math.Round((rule?.PricePerHour ?? c.PricePerHour) * minutes / 60m, 2);
	}

	private static bool IsClosed(VenueEntity v, DateTime startUtc, DateTime endUtc) => v.JsonData.Closures.Any(x => x.From < endUtc && x.To > startUtc);

	private static double DistanceKm(double lat1, double lng1, double lat2, double lng2) {
		double dLat = (lat2 - lat1) * Math.PI / 180, dLng = (lng2 - lng1) * Math.PI / 180;
		double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
		return 6371 * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
	}

	// ---------------- Venue ----------------

	public async Task<UResponse<Guid?>> CreateVenue(VenueCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (p.Latitude is < -90 or > 90 || p.Longitude is < -180 or > 180) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("locationIsNotValid"));

		// Admins may publish directly; everybody else waits for approval.
		TagVenue status = userData.IsAdmin && p.Tags.Any(x => (int)x / 100 == 2) ? p.Tags.First(x => (int)x / 100 == 2) : TagVenue.Pending;
		TagVenue kind = p.Tags.Contains(TagVenue.Shop) ? TagVenue.Shop : TagVenue.Club;
		VenueEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = userData.IsAdmin ? p.CreatorId ?? userData.Id : userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [kind, status, ..p.Tags.Where(x => x == TagVenue.PayAtVenue)],
			Title = p.Title,
			Latitude = p.Latitude,
			Longitude = p.Longitude,
			Address = p.Address,
			PhoneNumber = p.PhoneNumber,
			Country = p.Country?.ToUpperInvariant(),
			City = p.City,
			AdminUserIds = p.AdminUserIds ?? [],
			JsonData = new VenueJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				Description = p.Description,
				Website = p.Website,
				Instagram = p.Instagram,
				Whatsapp = p.Whatsapp,
				TimeZone = p.TimeZone.IsNotNullOrEmpty() ? p.TimeZone! : "UTC",
				Currency = p.Currency,
				Amenities = p.Amenities ?? [],
				OpeningHours = p.OpeningHours ?? [],
				CancellationFreeHours = p.CancellationFreeHours ?? 24,
				CancellationPenaltyPercent = Math.Clamp(p.CancellationPenaltyPercent ?? 100, 0, 100)
			}
		};

		await db.Set<VenueEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<VenueResponse>?>> ReadVenues(VenueReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		Guid uid = userData?.Id ?? Guid.Empty;
		IQueryable<VenueEntity> q = db.Set<VenueEntity>().ApplyReadParams(p);
		if (p.Mine) q = q.Where(x => x.CreatorId == uid || x.AdminUserIds.Contains(uid));
		else if (userData is not { IsAdmin: true }) q = q.Where(x => x.Tags.Contains(TagVenue.Approved));
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.ToLower().Contains(p.Title!.ToLower()));
		if (p.SportId.HasValue) q = q.Where(x => x.Courts.Any(c => c.SportId == p.SportId && c.Tags.Contains(TagCourt.Active)));
		if (p.Country.IsNotNullOrEmpty()) q = q.Where(x => x.Country == p.Country!.ToUpperInvariant());
		if (p.City.IsNotNullOrEmpty()) q = q.Where(x => x.City == p.City);

		if (p is { Latitude: not null, Longitude: not null }) {
			double lat = p.Latitude.Value, lng = p.Longitude.Value, cos = Math.Max(0.01, Math.Cos(lat * Math.PI / 180));
			if (p.RadiusKm.HasValue) {
				double dLat = p.RadiusKm.Value / 111.0, dLng = p.RadiusKm.Value / (111.0 * cos);
				q = q.Where(x => x.Latitude >= lat - dLat && x.Latitude <= lat + dLat && x.Longitude >= lng - dLng && x.Longitude <= lng + dLng);
			}

			// Nearest first (a flat approximation is enough for ordering).
			q = q.OrderBy(x => (x.Latitude - lat) * (x.Latitude - lat) + (x.Longitude - lng) * cos * ((x.Longitude - lng) * cos));
		}

		if (userData is not { IsAdmin: true }) p.SelectorArgs.Creator = null;
		UResponse<IEnumerable<VenueResponse>?> result = await q.Select(Projections.VenueSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
		if (p is { Latitude: not null, Longitude: not null })
			foreach (VenueResponse v in result.Result ?? [])
				v.DistanceKm = Math.Round(DistanceKm(p.Latitude.Value, p.Longitude.Value, v.Latitude, v.Longitude), 2);
		return result;
	}

	public async Task<UResponse<VenueResponse?>> ReadVenueById(IdParams<VenueSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		Guid uid = userData?.Id ?? Guid.Empty;
		IQueryable<VenueEntity> q = db.Set<VenueEntity>();
		if (userData is not { IsAdmin: true }) {
			q = q.Where(x => x.Tags.Contains(TagVenue.Approved) || x.CreatorId == uid || x.AdminUserIds.Contains(uid));
			p.SelectorArgs.Creator = null;
		}

		VenueResponse? e = await q.Select(Projections.VenueSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<VenueResponse?>(null, Usc.NotFound, ls.Get("venueNotFound")) : new UResponse<VenueResponse?>(e);
	}

	public async Task<UResponse> UpdateVenue(VenueUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		VenueEntity? e = await db.Set<VenueEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("venueNotFound"));
		if (!CanManage(userData, e)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.Latitude is < -90 or > 90 || p.Longitude is < -180 or > 180) return new UResponse(Usc.BadRequest, ls.Get("locationIsNotValid"));

		List<TagVenue> statusBefore = e.Tags.Where(x => (int)x / 100 == 2).ToList();
		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title!;
		if (p.Latitude.HasValue) e.Latitude = p.Latitude.Value;
		if (p.Longitude.HasValue) e.Longitude = p.Longitude.Value;
		if (p.Address.IsNotNull()) e.Address = p.Address;
		if (p.PhoneNumber.IsNotNull()) e.PhoneNumber = p.PhoneNumber;
		if (p.Country.IsNotNull()) e.Country = p.Country.IsNotNullOrEmpty() ? p.Country!.ToUpperInvariant() : null;
		if (p.City.IsNotNull()) e.City = p.City;
		if (p.Description.IsNotNull()) e.JsonData.Description = p.Description;
		if (p.Website.IsNotNull()) e.JsonData.Website = p.Website;
		if (p.Instagram.IsNotNull()) e.JsonData.Instagram = p.Instagram;
		if (p.Whatsapp.IsNotNull()) e.JsonData.Whatsapp = p.Whatsapp;
		if (p.TimeZone.IsNotNullOrEmpty()) e.JsonData.TimeZone = p.TimeZone!;
		if (p.Currency.IsNotNull()) e.JsonData.Currency = p.Currency;
		if (p.Amenities != null) e.JsonData.Amenities = p.Amenities;
		if (p.OpeningHours != null) e.JsonData.OpeningHours = p.OpeningHours;
		if (p.Closures != null) e.JsonData.Closures = p.Closures;
		if (p.CancellationFreeHours.HasValue) e.JsonData.CancellationFreeHours = Math.Max(0, p.CancellationFreeHours.Value);
		if (p.CancellationPenaltyPercent.HasValue) e.JsonData.CancellationPenaltyPercent = Math.Clamp(p.CancellationPenaltyPercent.Value, 0, 100);
		if (userData.IsAdmin && p.RejectionReason.IsNotNull()) e.JsonData.RejectionReason = p.RejectionReason;
		e.ApplyUpdateParam<VenueEntity, TagVenue, VenueJson>(p);

		// Only admins change the status; a rejected venue that its owner edits is sent for review again.
		List<TagVenue> status = e.Tags.Where(x => (int)x / 100 == 2).ToList();
		if (!userData.IsAdmin) status = statusBefore.Contains(TagVenue.Rejected) ? [TagVenue.Pending] : statusBefore;
		if (status.Count != 1) status = [status.FirstOrDefault(TagVenue.Pending)];
		e.Tags = e.Tags.Where(x => (int)x / 100 != 2).Concat(status).Distinct().ToList();
		if (e.Tags.Count(x => (int)x / 100 == 1) != 1) e.Tags = e.Tags.Where(x => (int)x / 100 != 1).Append(TagVenue.Club).ToList();

		if (!statusBefore.SequenceEqual(status) && status[0] is TagVenue.Approved or TagVenue.Rejected or TagVenue.Suspended)
			await db.AddNotifications([e.CreatorId], userData.Id, status[0] switch {
				TagVenue.Approved => "notifVenueApproved",
				TagVenue.Rejected => "notifVenueRejected",
				_ => "notifVenueSuspended"
			}, e.Title, "venue", e.Id, ct, TagNotification.General);

		await db.SaveChangesAsync(ct);
		await rt.ToUsers([e.CreatorId], "notification");
		return new UResponse();
	}

	public async Task<UResponse> DeleteVenue(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		VenueEntity? e = await db.Set<VenueEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("venueNotFound"));
		if (!userData.IsAdmin && e.CreatorId != userData.Id) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (await db.Set<BookingEntity>().AnyAsync(x => x.VenueId == e.Id && x.EndAt > DateTime.UtcNow && (x.Tags.Contains(TagBooking.Pending) || x.Tags.Contains(TagBooking.Confirmed)), ct))
			return new UResponse(Usc.Conflict, ls.Get("thisVenueHasUpcomingBookings"));

		await db.Set<OpenMatchEntity>().Where(x => x.VenueId == e.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.VenueId, (Guid?)null), ct);
		await db.Set<MediaEntity>().Where(x => x.VenueId == e.Id).ExecuteDeleteAsync(ct);
		await db.Set<CommentEntity>().Where(x => x.VenueId == e.Id).ExecuteDeleteAsync(ct);
		await db.Set<BookingEntity>().Where(x => x.VenueId == e.Id).ExecuteDeleteAsync(ct);
		await db.Set<CourtEntity>().Where(x => x.VenueId == e.Id).ExecuteDeleteAsync(ct);
		await db.Set<VenueEntity>().Where(x => x.Id == e.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<VenueStatsResponse?>> ReadVenueStats(VenueStatsParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<VenueStatsResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		VenueEntity? v = await db.Set<VenueEntity>().Include(x => x.Courts).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (v == null) return new UResponse<VenueStatsResponse?>(null, Usc.NotFound, ls.Get("venueNotFound"));
		if (!CanManage(userData, v)) return new UResponse<VenueStatsResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		DateTime now = DateTime.UtcNow;
		DateTime from = p.From ?? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
		DateTime to = p.To ?? from.AddMonths(1);
		if ((to - from).TotalDays > 366) to = from.AddDays(366);

		List<BookingEntity> bookings = await db.Set<BookingEntity>().Where(x => x.VenueId == v.Id && x.StartAt < to && x.EndAt > from).ToListAsync(ct);
		List<BookingEntity> active = bookings.Where(x => x.Tags.Any(ActiveBookings.Contains)).ToList();

		// Open minutes of every active court in the period, from the opening hours.
		TimeZoneInfo zone = ZoneOf(v);
		int courts = v.Courts.Count(x => x.Tags.Contains(TagCourt.Active));
		double openMinutes = 0;
		for (DateTime day = ToLocal(from, zone).Date; day < ToLocal(to, zone); day = day.AddDays(1))
			openMinutes += OpenPeriods(v, day).Sum(x => (x.To - x.From).TotalMinutes) * courts;
		double bookedMinutes = active.Sum(x => (x.EndAt - x.StartAt).TotalMinutes);

		return new UResponse<VenueStatsResponse?>(new VenueStatsResponse {
			Bookings = active.Count,
			Upcoming = active.Count(x => x.StartAt > now && !x.Tags.Contains(TagBooking.Completed)),
			Completed = active.Count(x => x.Tags.Contains(TagBooking.Completed)),
			Cancelled = bookings.Count(x => x.Tags.Contains(TagBooking.Cancelled)),
			Revenue = active.Where(x => x.Tags.Contains(TagBooking.Completed) || x.Tags.Contains(TagBooking.NoShow)).Sum(x => x.Price)
			          + bookings.Where(x => x.Tags.Contains(TagBooking.Cancelled)).Sum(x => x.JsonData.Penalty),
			OccupancyPercent = openMinutes <= 0 ? 0 : (int)Math.Round(Math.Min(100, 100 * bookedMinutes / openMinutes))
		});
	}

	// ---------------- Court ----------------

	public async Task<UResponse<Guid?>> CreateCourt(CourtCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		VenueEntity? v = await db.Set<VenueEntity>().FirstOrDefaultAsync(x => x.Id == p.VenueId, ct);
		if (v == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("venueNotFound"));
		if (!CanManage(userData, v)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.PricePerHour < 0 || p.SlotMinutes is < 15 or > 240) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("priceOrSlotIsNotValid"));
		if (p.SportId.HasValue && !await db.Set<SportEntity>().AnyAsync(x => x.Id == p.SportId, ct)) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("sportNotFound"));

		CourtEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [p.Tags.Contains(TagCourt.Outdoor) ? TagCourt.Outdoor : TagCourt.Indoor, p.Tags.Contains(TagCourt.Inactive) ? TagCourt.Inactive : TagCourt.Active],
			Title = p.Title,
			PricePerHour = p.PricePerHour,
			SlotMinutes = p.SlotMinutes,
			VenueId = v.Id,
			SportId = p.SportId,
			JsonData = new CourtJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				Description = p.Description,
				Surface = p.Surface,
				Players = p.Players ?? 4,
				PriceRules = p.PriceRules ?? []
			}
		};

		await db.Set<CourtEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<CourtResponse>?>> ReadCourts(CourtReadParams p, CancellationToken ct) {
		IQueryable<CourtEntity> q = db.Set<CourtEntity>().ApplyReadParams(p);
		if (p.VenueId.HasValue) q = q.Where(x => x.VenueId == p.VenueId);
		if (p.SportId.HasValue) q = q.Where(x => x.SportId == p.SportId);
		p.SelectorArgs.Creator = null;
		return await q.Select(Projections.CourtSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateCourt(CourtUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		CourtEntity? e = await db.Set<CourtEntity>().AsTracking().Include(x => x.Venue).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("courtNotFound"));
		if (!CanManage(userData, e.Venue)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.PricePerHour < 0 || p.SlotMinutes is < 15 or > 240) return new UResponse(Usc.BadRequest, ls.Get("priceOrSlotIsNotValid"));

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title!;
		if (p.SportId.HasValue) e.SportId = p.SportId == Guid.Empty ? null : p.SportId;
		if (p.PricePerHour.HasValue) e.PricePerHour = p.PricePerHour.Value;
		if (p.SlotMinutes.HasValue) e.SlotMinutes = p.SlotMinutes.Value;
		if (p.Description.IsNotNull()) e.JsonData.Description = p.Description;
		if (p.Surface.IsNotNull()) e.JsonData.Surface = p.Surface;
		if (p.Players.HasValue) e.JsonData.Players = p.Players.Value;
		if (p.PriceRules != null) e.JsonData.PriceRules = p.PriceRules;
		e.ApplyUpdateParam<CourtEntity, TagCourt, CourtJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteCourt(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		CourtEntity? e = await db.Set<CourtEntity>().Include(x => x.Venue).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("courtNotFound"));
		if (!CanManage(userData, e.Venue)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		// Bookings are the venue's history; a court that has them is deactivated instead.
		if (await db.Set<BookingEntity>().AnyAsync(x => x.CourtId == e.Id, ct)) return new UResponse(Usc.Conflict, ls.Get("courtHasBookingsDeactivateItInstead"));

		await db.Set<CourtEntity>().Where(x => x.Id == e.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<IEnumerable<CourtAvailabilityResponse>?>> ReadCourtAvailability(CourtAvailabilityParams p, CancellationToken ct) {
		IQueryable<CourtEntity> q = db.Set<CourtEntity>().Include(x => x.Venue).Where(x => x.Tags.Contains(TagCourt.Active) && x.Venue.Tags.Contains(TagVenue.Approved));
		if (p.CourtId.HasValue) q = q.Where(x => x.Id == p.CourtId);
		else if (p.VenueId.HasValue) q = q.Where(x => x.VenueId == p.VenueId);
		else return new UResponse<IEnumerable<CourtAvailabilityResponse>?>(null, Usc.BadRequest, ls.Get("idIsRequired"));
		if (p.SportId.HasValue) q = q.Where(x => x.SportId == p.SportId);

		List<CourtEntity> courts = await q.OrderBy(x => x.Title).ToListAsync(ct);
		if (courts.Count == 0) return new UResponse<IEnumerable<CourtAvailabilityResponse>?>([]);
		VenueEntity venue = courts[0].Venue;
		TimeZoneInfo zone = ZoneOf(venue);
		DateTime day = p.Date.Date;
		DateTime dayStart = ToUtc(day, zone), dayEnd = ToUtc(day.AddDays(1), zone);

		List<Guid> courtIds = courts.Select(x => x.Id).ToList();
		List<BookingEntity> booked = await db.Set<BookingEntity>()
			.Where(x => courtIds.Contains(x.CourtId) && x.StartAt < dayEnd && x.EndAt > dayStart && !x.Tags.Contains(TagBooking.Cancelled))
			.ToListAsync(ct);
		Dictionary<Guid, CourtResponse> projected = await db.Set<CourtEntity>().Where(x => courtIds.Contains(x.Id))
			.Select(Projections.CourtSelector(new CourtSelectorArgs { Sport = new SportSelectorArgs() })).ToDictionaryAsync(x => x.Id, ct);

		DateTime now = DateTime.UtcNow;
		List<CourtAvailabilityResponse> result = [];
		foreach (CourtEntity court in courts) {
			CourtAvailabilityResponse item = new() { Court = projected[court.Id] };
			foreach ((DateTime from, DateTime to) in OpenPeriods(venue, day))
				for (DateTime local = from; local.AddMinutes(court.SlotMinutes) <= to; local = local.AddMinutes(court.SlotMinutes)) {
					DateTime start = ToUtc(local, zone), end = start.AddMinutes(court.SlotMinutes);
					bool free = start > now && !IsClosed(venue, start, end) && !booked.Any(b => b.CourtId == court.Id && b.StartAt < end && b.EndAt > start);
					item.Slots.Add(new CourtSlotResponse { StartAt = start, EndAt = end, Price = PriceOf(court, local, court.SlotMinutes), Available = free });
				}

			result.Add(item);
		}

		return new UResponse<IEnumerable<CourtAvailabilityResponse>?>(result);
	}

	// ---------------- Booking ----------------

	private async Task<UResponse<WalletTxnResponse?>> Pay(Guid from, Guid to, decimal amount, TagWalletTxn tag, string detailKey, VenueEntity v, CourtEntity c, BookingEntity b, CancellationToken ct) =>
		await ws.Transfer(new WalletTransferParams {
			SenderId = from,
			ReceiverId = to,
			Amount = amount,
			Detail1 = ls.Get(detailKey),
			KeyValues = [
				new KeyValue { Key = ULocalizedConstants.Venue, Value = v.Title },
				new KeyValue { Key = ULocalizedConstants.Court, Value = c.Title },
				new KeyValue { Key = ULocalizedConstants.StartAt, Value = b.StartAt.ToString("O") },
				new KeyValue { Key = ULocalizedConstants.BookingId, Value = b.Id.ToString() }
			],
			TagWalletTxn = [tag]
		}, ct);

	public async Task<UResponse<BookingResponse?>> CreateBooking(BookingCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<BookingResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<BookingResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		CourtEntity? court = await db.Set<CourtEntity>().Include(x => x.Venue).FirstOrDefaultAsync(x => x.Id == p.CourtId, ct);
		if (court == null || !court.Tags.Contains(TagCourt.Active)) return new UResponse<BookingResponse?>(null, Usc.NotFound, ls.Get("courtNotFound"));
		VenueEntity venue = court.Venue;
		if (!venue.Tags.Contains(TagVenue.Approved)) return new UResponse<BookingResponse?>(null, Usc.Conflict, ls.Get("thisVenueIsNotTakingBookings"));
		if (!p.PayFromWallet && !venue.Tags.Contains(TagVenue.PayAtVenue)) return new UResponse<BookingResponse?>(null, Usc.BadRequest, ls.Get("thisVenueOnlyTakesOnlinePayment"));
		if (p.DurationMinutes <= 0 || p.DurationMinutes % court.SlotMinutes != 0 || p.DurationMinutes > 360) return new UResponse<BookingResponse?>(null, Usc.BadRequest, ls.Get("durationIsNotValid"));

		DateTime start = p.StartAt, end = start.AddMinutes(p.DurationMinutes);
		if (start <= DateTime.UtcNow) return new UResponse<BookingResponse?>(null, Usc.BadRequest, ls.Get("theStartTimeIsInThePast"));

		// Inside one open period of the venue's day, on the court's slot grid, not during a closure.
		TimeZoneInfo zone = ZoneOf(venue);
		DateTime localStart = ToLocal(start, zone), localEnd = ToLocal(end, zone);
		(DateTime From, DateTime To)? period = OpenPeriods(venue, localStart.Date).Cast<(DateTime From, DateTime To)?>().FirstOrDefault(x => x!.Value.From <= localStart && localEnd <= x.Value.To);
		if (period == null || (localStart - period.Value.From).TotalMinutes % court.SlotMinutes != 0 || IsClosed(venue, start, end))
			return new UResponse<BookingResponse?>(null, Usc.BadRequest, ls.Get("theVenueIsClosedAtThisTime"));
		if (await db.Set<BookingEntity>().AnyAsync(x => x.CourtId == court.Id && x.StartAt < end && x.EndAt > start && !x.Tags.Contains(TagBooking.Cancelled), ct))
			return new UResponse<BookingResponse?>(null, Usc.Conflict, ls.Get("thisTimeIsAlreadyBooked"));

		decimal price = 0;
		for (DateTime local = localStart; local < localEnd; local = local.AddMinutes(court.SlotMinutes)) price += PriceOf(court, local, court.SlotMinutes);

		List<Guid> sharing = p.SplitWithUserIds.Where(x => x != userData.Id).Distinct().ToList();
		if (sharing.Count > 0 && await db.Set<UserEntity>().CountAsync(x => sharing.Contains(x.Id), ct) != sharing.Count) return new UResponse<BookingResponse?>(null, Usc.NotFound, ls.Get("userNotFound"));
		decimal share = Math.Round(price / (sharing.Count + 1), 2);

		BookingEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatorId = userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [TagBooking.Confirmed, p.PayFromWallet ? TagBooking.PaidFromWallet : TagBooking.PayAtVenue],
			StartAt = start,
			EndAt = end,
			Price = price,
			UserId = userData.Id,
			ParticipantIds = [userData.Id, ..sharing],
			CourtId = court.Id,
			VenueId = venue.Id,
			JsonData = new BookingJson {
				Code = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)),
				Notes = p.Notes,
				// The booker's share is the rest, so the shares always add up to the price.
				Participants = [
					new BookingParticipant { UserId = userData.Id, Share = price - share * sharing.Count },
					..sharing.Select(x => new BookingParticipant { UserId = x, Share = share })
				]
			}
		};

		if (p.PayFromWallet && price > 0) {
			UResponse<WalletTxnResponse?> paid = await Pay(userData.Id, Core.App.Users.SystemAdmin.Id, e.JsonData.Participants[0].Share, TagWalletTxn.CourtBooking, "courtBookingPayment", venue, court, e, ct);
			if (paid.Result == null) return new UResponse<BookingResponse?>(null, paid.Status, paid.Message);
			e.JsonData.Participants[0].Paid = true;
			e.JsonData.PaidAmount = e.JsonData.Participants[0].Share;
		}

		await db.Set<BookingEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);

		// Two requests for the same time can both pass the check above; the later one gives way.
		if (await db.Set<BookingEntity>().AnyAsync(x => x.Id != e.Id && x.CourtId == court.Id && x.StartAt < end && x.EndAt > start && !x.Tags.Contains(TagBooking.Cancelled) && x.CreatedAt <= e.CreatedAt, ct)) {
			if (e.JsonData.PaidAmount > 0) await Pay(Core.App.Users.SystemAdmin.Id, userData.Id, e.JsonData.PaidAmount, TagWalletTxn.CourtBookingRefund, "courtBookingRefund", venue, court, e, ct);
			await db.Set<BookingEntity>().Where(x => x.Id == e.Id).ExecuteDeleteAsync(ct);
			return new UResponse<BookingResponse?>(null, Usc.Conflict, ls.Get("thisTimeIsAlreadyBooked"));
		}

		List<Guid> staff = [venue.CreatorId, ..venue.AdminUserIds];
		await db.AddNotifications(staff, userData.Id, "notifNewBooking", venue.Title, "booking", e.Id, ct, TagNotification.Booking);
		await db.AddNotifications(sharing, userData.Id, "notifBookingShare", venue.Title, "booking", e.Id, ct, TagNotification.Booking);
		await db.SaveChangesAsync(ct);
		await rt.ToUsers([..staff, ..sharing], "notification");

		BookingResponse? created = await db.Set<BookingEntity>()
			.Select(Projections.BookingSelector(new BookingSelectorArgs { Court = new CourtSelectorArgs(), Venue = new VenueSelectorArgs() }))
			.FirstOrDefaultAsync(x => x.Id == e.Id, ct);
		return new UResponse<BookingResponse?>(created, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<BookingResponse>?>> ReadBookings(BookingReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<BookingResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		Guid uid = userData.Id;

		IQueryable<BookingEntity> q = db.Set<BookingEntity>().ApplyReadParams(p);
		// Players see their own bookings; staff see their venues'.
		if (p.Mine || p.VenueId == null && p.CourtId == null && !userData.IsAdmin) q = q.Where(x => x.ParticipantIds.Contains(uid));
		else if (!userData.IsAdmin) q = q.Where(x => x.Venue.CreatorId == uid || x.Venue.AdminUserIds.Contains(uid));
		if (p.VenueId.HasValue) q = q.Where(x => x.VenueId == p.VenueId);
		if (p.CourtId.HasValue) q = q.Where(x => x.CourtId == p.CourtId);
		if (p.From.HasValue) q = q.Where(x => x.EndAt > p.From);
		if (p.To.HasValue) q = q.Where(x => x.StartAt < p.To);
		q = p.From.HasValue ? q.OrderBy(x => x.StartAt) : q.OrderByDescending(x => x.StartAt);

		p.SelectorArgs.Creator = null;
		return await q.Select(Projections.BookingSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	/// <summary>Pays what the platform holds for a booking out to the venue owner (once).</summary>
	private async Task Settle(BookingEntity e, CancellationToken ct) {
		decimal amount = e.JsonData.PaidAmount - e.JsonData.RefundAmount;
		if (e.JsonData.Settled || amount <= 0) return;
		UResponse<WalletTxnResponse?> paid = await Pay(Core.App.Users.SystemAdmin.Id, e.Venue.CreatorId, amount, TagWalletTxn.CourtBookingSettlement, "courtBookingSettlement", e.Venue, e.Court, e, ct);
		if (paid.Result != null) e.JsonData.Settled = true;
	}

	public async Task<UResponse> UpdateBooking(BookingUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		BookingEntity? e = await db.Set<BookingEntity>().AsTracking().Include(x => x.Venue).Include(x => x.Court).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("bookingNotFound"));
		if (!CanManage(userData, e.Venue)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (e.Tags.Contains(TagBooking.Cancelled)) return new UResponse(Usc.Conflict, ls.Get("thisBookingIsCancelled"));

		// Staff only move a booking to Completed / NoShow here; cancelling has its own refunds.
		List<TagBooking> payment = e.Tags.Where(x => (int)x / 100 == 2).ToList();
		e.ApplyUpdateParam<BookingEntity, TagBooking, BookingJson>(p);
		TagBooking status = e.Tags.Contains(TagBooking.NoShow) ? TagBooking.NoShow : e.Tags.Contains(TagBooking.Completed) ? TagBooking.Completed : TagBooking.Confirmed;
		e.Tags = [status, ..payment];
		if (p.Notes.IsNotNull()) e.JsonData.Notes = p.Notes;
		if (status is TagBooking.Completed or TagBooking.NoShow) await Settle(e, ct);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> CancelBooking(BookingCancelParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		BookingEntity? e = await db.Set<BookingEntity>().AsTracking().Include(x => x.Venue).Include(x => x.Court).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("bookingNotFound"));
		bool staff = CanManage(userData, e.Venue);
		if (!staff && e.UserId != userData.Id) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!e.Tags.Contains(TagBooking.Confirmed) && !e.Tags.Contains(TagBooking.Pending)) return new UResponse(Usc.Conflict, ls.Get("thisBookingCannotBeCancelled"));
		if (!staff && e.StartAt <= DateTime.UtcNow) return new UResponse(Usc.Conflict, ls.Get("thisBookingCannotBeCancelled"));

		// Claim the cancellation first so two requests can't both refund.
		List<TagBooking> cancelled = [TagBooking.Cancelled, ..e.Tags.Where(x => (int)x / 100 == 2)];
		int claimed = await db.Set<BookingEntity>().Where(x => x.Id == e.Id && !x.Tags.Contains(TagBooking.Cancelled)).ExecuteUpdateAsync(u => u.SetProperty(x => x.Tags, cancelled), ct);
		if (claimed == 0) return new UResponse(Usc.Conflict, ls.Get("thisBookingCannotBeCancelled"));
		e.Tags = cancelled;

		// A late cancellation by the player keeps a part of the price for the venue; the venue cancelling refunds everything.
		bool late = (e.StartAt - DateTime.UtcNow).TotalHours < e.Venue.JsonData.CancellationFreeHours;
		decimal penaltyRate = !staff && late ? e.Venue.JsonData.CancellationPenaltyPercent / 100m : 0;
		decimal refunded = 0;
		foreach (BookingParticipant participant in e.JsonData.Participants.Where(x => x.Paid)) {
			decimal refund = Math.Round(participant.Share * (1 - penaltyRate), 2);
			if (refund <= 0) continue;
			UResponse<WalletTxnResponse?> paid = await Pay(Core.App.Users.SystemAdmin.Id, participant.UserId, refund, TagWalletTxn.CourtBookingRefund, "courtBookingRefund", e.Venue, e.Court, e, ct);
			if (paid.Result != null) refunded += refund;
		}

		e.JsonData.RefundAmount = refunded;
		e.JsonData.Penalty = e.JsonData.PaidAmount - refunded;
		e.JsonData.CancelReason = p.Reason;
		await Settle(e, ct);

		List<Guid> notify = staff ? e.ParticipantIds.ToList() : [e.Venue.CreatorId, ..e.Venue.AdminUserIds, ..e.ParticipantIds];
		await db.AddNotifications(notify, userData.Id, "notifBookingCancelled", e.Venue.Title, "booking", e.Id, ct, TagNotification.Booking);
		await db.SaveChangesAsync(ct);
		await rt.ToUsers(notify, "notification");
		return new UResponse();
	}

	public async Task<UResponse> PayBookingShare(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		BookingEntity? e = await db.Set<BookingEntity>().AsTracking().Include(x => x.Venue).Include(x => x.Court).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("bookingNotFound"));
		if (!e.Tags.Contains(TagBooking.Confirmed)) return new UResponse(Usc.Conflict, ls.Get("thisBookingIsCancelled"));
		BookingParticipant? me = e.JsonData.Participants.FirstOrDefault(x => x.UserId == userData.Id);
		if (me == null) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (me.Paid) return new UResponse(Usc.Conflict, ls.Get("yourShareIsAlreadyPaid"));

		UResponse<WalletTxnResponse?> paid = await Pay(userData.Id, Core.App.Users.SystemAdmin.Id, me.Share, TagWalletTxn.CourtBooking, "courtBookingPayment", e.Venue, e.Court, e, ct);
		if (paid.Result == null) return new UResponse(paid.Status, paid.Message);
		me.Paid = true;
		e.JsonData.PaidAmount += me.Share;
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}
}
