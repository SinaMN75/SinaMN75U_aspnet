namespace SinaMN75U.InnerServices;

/// <summary>
/// Live updates over SignalR at /hubs/u. Clients connect with ?apiKey=...&amp;access_token=... and are put in their own "user:{id}" group.
/// Events: "notification" (to a user), "message" (a chat message), "tournament" / "openMatch" (something changed, refetch it).
/// </summary>
public sealed class UHub(ITokenService ts, DbContext db) : Hub {
	private Guid? UserId => Context.Items.TryGetValue("userId", out object? id) ? id as Guid? : null;

	public override async Task OnConnectedAsync() {
		HttpContext? http = Context.GetHttpContext();
		string? apiKey = http?.Request.Query["apiKey"];
		if (Core.App.Middleware.RequireApiKey && apiKey != Core.App.ApiKey) {
			Context.Abort();
			return;
		}

		JwtClaimData? user = ts.ExtractClaims(http?.Request.Query["access_token"]);
		if (user is { IsExpired: false }) {
			Context.Items["userId"] = user.Id;
			await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.User(user.Id));
		}

		await base.OnConnectedAsync();
	}

	/// <summary>Live messages of a conversation the user is a member of.</summary>
	public async Task<bool> JoinConversation(Guid id) {
		Guid? userId = UserId;
		if (userId == null || !await db.Set<ConversationEntity>().AnyAsync(x => x.Id == id && x.Users.Any(u => u.Id == userId))) return false;
		await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.Conversation(id));
		return true;
	}

	public Task LeaveConversation(Guid id) => Groups.RemoveFromGroupAsync(Context.ConnectionId, RealtimeGroups.Conversation(id));

	/// <summary>Live scores of a tournament (public).</summary>
	public Task JoinTournament(Guid id) => Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.Tournament(id));

	public Task LeaveTournament(Guid id) => Groups.RemoveFromGroupAsync(Context.ConnectionId, RealtimeGroups.Tournament(id));
}

public static class RealtimeGroups {
	public static string User(Guid id) => $"user:{id}";
	public static string Conversation(Guid id) => $"conversation:{id}";
	public static string Tournament(Guid id) => $"tournament:{id}";
}

public interface IRealtimeService {
	Task ToUsers(IEnumerable<Guid> userIds, string method, object? payload = null);
	Task ToGroup(string group, string method, object? payload = null);
}

public sealed class RealtimeService(IHubContext<UHub> hub) : IRealtimeService {
	// A failed push must never fail the request that caused it.
	public async Task ToUsers(IEnumerable<Guid> userIds, string method, object? payload = null) {
		try {
			List<string> groups = userIds.Distinct().Select(RealtimeGroups.User).ToList();
			if (groups.Count > 0) await hub.Clients.Groups(groups).SendAsync(method, payload);
		}
		catch (Exception e) {
			ULog.Error(e, "Realtime push failed");
		}
	}

	public async Task ToGroup(string group, string method, object? payload = null) {
		try {
			await hub.Clients.Group(group).SendAsync(method, payload);
		}
		catch (Exception e) {
			ULog.Error(e, "Realtime push failed");
		}
	}
}

public static class NotificationExtensions {
	/// <summary>
	/// Queues in-app notifications (saved with the caller's SaveChanges). Detail1 is a message key the apps translate
	/// (e.g. "notifEntryApproved"), Detail2 the subject (a tournament's title, a name...).
	/// </summary>
	public static async Task AddNotifications(this DbContext db, IEnumerable<Guid> userIds, Guid creatorId, string key, string? subject, string? linkType, Guid? linkId, CancellationToken ct, TagNotification kind = TagNotification.Sport) {
		DateTime now = DateTime.UtcNow;
		foreach (Guid userId in userIds.Distinct().Where(x => x != creatorId))
			await db.Set<NotificationEntity>().AddAsync(new NotificationEntity {
				Id = Guid.CreateVersion7(),
				CreatedAt = now,
				CreatorId = creatorId,
				UserId = userId,
				Tags = [kind, TagNotification.Unread],
				JsonData = new NotificationJson { Detail1 = key, Detail2 = subject ?? "", LinkType = linkType, LinkId = linkId }
			}, ct);
	}
}
