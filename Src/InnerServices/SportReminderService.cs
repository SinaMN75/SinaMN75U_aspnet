namespace SinaMN75U.InnerServices;

/// <summary>
/// Every few minutes: notifies players of tournament matches, court bookings and open games that start within the hour (once each).
/// A host without the sport tables just skips it.
/// </summary>
public sealed class SportReminderService(IServiceScopeFactory scopeFactory) : BackgroundService {
	private static readonly TimeSpan Every = TimeSpan.FromMinutes(5);
	private static readonly TimeSpan Ahead = TimeSpan.FromHours(1);
	private bool _failureLogged;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
		using PeriodicTimer timer = new(Every);
		try {
			do {
				try {
					using IServiceScope scope = scopeFactory.CreateScope();
					await Run(scope.ServiceProvider.GetRequiredService<DbContext>(), scope.ServiceProvider.GetRequiredService<IRealtimeService>(), stoppingToken);
				}
				catch (OperationCanceledException) {
					throw;
				}
				catch (Exception e) {
					if (!_failureLogged) ULog.Error(e, "Sport reminders are off (are the sport tables migrated?)");
					_failureLogged = true;
				}
			} while (await timer.WaitForNextTickAsync(stoppingToken));
		}
		catch (OperationCanceledException) {
			// The server is stopping.
		}
	}

	private static async Task Run(DbContext db, IRealtimeService rt, CancellationToken ct) {
		DateTime now = DateTime.UtcNow, until = now + Ahead;
		Guid system = Core.App.Users.SystemAdmin.Id;
		HashSet<Guid> notified = [];

		List<TournamentMatchEntity> matches = await db.Set<TournamentMatchEntity>().AsTracking().Include(x => x.Tournament)
			.Where(x => x.ScheduledAt > now && x.ScheduledAt <= until && !x.Tags.Contains(TagTournamentMatch.Finished) && !x.Tags.Contains(TagTournamentMatch.Bye) && !x.JsonData.Reminded)
			.ToListAsync(ct);
		foreach (TournamentMatchEntity m in matches) {
			List<Guid> sides = new[] { m.EntryAId, m.EntryBId, m.PartnerAId, m.PartnerBId }.OfType<Guid>().ToList();
			List<Guid> players = await db.Set<TournamentEntryEntity>().Where(x => sides.Contains(x.Id)).SelectMany(x => x.Users.Select(u => u.Id)).ToListAsync(ct);
			await db.AddNotifications(players, system, "notifMatchSoon", m.Tournament.Title, "tournament", m.TournamentId, ct, TagNotification.Reminder);
			m.JsonData.Reminded = true;
			notified.UnionWith(players);
		}

		List<BookingEntity> bookings = await db.Set<BookingEntity>().AsTracking().Include(x => x.Venue)
			.Where(x => x.StartAt > now && x.StartAt <= until && x.Tags.Contains(TagBooking.Confirmed) && !x.JsonData.Reminded)
			.ToListAsync(ct);
		foreach (BookingEntity b in bookings) {
			await db.AddNotifications(b.ParticipantIds, system, "notifBookingSoon", b.Venue.Title, "booking", b.Id, ct, TagNotification.Reminder);
			b.JsonData.Reminded = true;
			notified.UnionWith(b.ParticipantIds);
		}

		List<OpenMatchEntity> games = await db.Set<OpenMatchEntity>().AsTracking().Include(x => x.Users)
			.Where(x => x.StartAt > now && x.StartAt <= until && (x.Tags.Contains(TagOpenMatch.Open) || x.Tags.Contains(TagOpenMatch.Full)) && !x.JsonData.Reminded)
			.ToListAsync(ct);
		foreach (OpenMatchEntity g in games) {
			List<Guid> players = g.Users.Select(x => x.Id).ToList();
			await db.AddNotifications(players, system, "notifGameSoon", g.JsonData.Title, "openMatch", g.Id, ct, TagNotification.Reminder);
			g.JsonData.Reminded = true;
			notified.UnionWith(players);
		}

		if (notified.Count == 0) return;
		await db.SaveChangesAsync(ct);
		await rt.ToUsers(notified, "notification");
	}
}
