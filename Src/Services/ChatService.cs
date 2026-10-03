namespace SinaMN75U.Services;

public interface IChatService {
	public Task<UResponse<ConversationResponse?>> CreateConversation(ConversationCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<ConversationResponse>?>> ReadConversations(ConversationReadParams p, CancellationToken ct);
	public Task<UResponse<ConversationResponse?>> ReadConversationById(IdParams<ConversationSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdateConversation(ConversationUpdateParams p, CancellationToken ct);
	public Task<UResponse> LeaveConversation(IdParams p, CancellationToken ct);
	public Task<UResponse> MarkConversationRead(IdParams p, CancellationToken ct);

	public Task<UResponse<MessageResponse?>> CreateMessage(MessageCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<MessageResponse>?>> ReadMessages(MessageReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateMessage(MessageUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeleteMessage(IdParams p, CancellationToken ct);
}

public class ChatService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IRealtimeService rt
) : IChatService {
	// ---- Who sees and changes what ----
	// Only members read and write a conversation. One direct conversation per pair of users; a blocked user can't start or write in one.
	// Group conversations are managed by their creator. New messages are pushed live to the conversation's group and to every member.

	private static string DirectKeyOf(Guid a, Guid b) => string.CompareOrdinal(a.ToString(), b.ToString()) < 0 ? $"{a}:{b}" : $"{b}:{a}";

	private Task<bool> IsMember(Guid conversationId, Guid userId, CancellationToken ct) =>
		db.Set<ConversationEntity>().AnyAsync(x => x.Id == conversationId && x.Users.Any(u => u.Id == userId), ct);

	private Task<bool> AreBlocked(Guid a, Guid b, CancellationToken ct) =>
		db.Set<BlockEntity>().AnyAsync(x => x.CreatorId == a && x.BlockedUserId == b || x.CreatorId == b && x.BlockedUserId == a, ct);

	/// <summary>Unread messages of each conversation for the user (after the time they last read it).</summary>
	private async Task FillUnread(IEnumerable<ConversationResponse> items, Guid userId, CancellationToken ct) {
		foreach (ConversationResponse c in items) {
			DateTime readAt = c.JsonData.Reads.FirstOrDefault(x => x.UserId == userId)?.At ?? DateTime.MinValue;
			c.UnreadCount = await db.Set<MessageEntity>().CountAsync(x => x.ConversationId == c.Id && x.CreatorId != userId && x.CreatedAt > readAt && !x.Tags.Contains(TagMessage.Deleted), ct);
		}
	}

	public async Task<UResponse<ConversationResponse?>> CreateConversation(ConversationCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ConversationResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<ConversationResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		bool group = p.Tags.Contains(TagConversation.Group);
		List<Guid> others = p.UserIds.Where(x => x != userData.Id).Distinct().ToList();
		if (others.Count == 0 || !group && others.Count != 1) return new UResponse<ConversationResponse?>(null, Usc.BadRequest, ls.Get("userIsRequired"));
		List<UserEntity> members = await db.Set<UserEntity>().AsTracking().Where(x => others.Contains(x.Id) || x.Id == userData.Id).ToListAsync(ct);
		if (members.Count != others.Count + 1) return new UResponse<ConversationResponse?>(null, Usc.NotFound, ls.Get("userNotFound"));

		string? key = null;
		if (!group) {
			if (await AreBlocked(userData.Id, others[0], ct)) return new UResponse<ConversationResponse?>(null, Usc.Forbidden, ls.Get("youCannotMessageThisUser"));
			key = DirectKeyOf(userData.Id, others[0]);
			Guid? existing = await db.Set<ConversationEntity>().Where(x => x.DirectKey == key).Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
			if (existing != null) return await ReadConversationById(new IdParams<ConversationSelectorArgs> { Id = existing.Value, Token = p.Token, ApiKey = p.ApiKey, SelectorArgs = new ConversationSelectorArgs { Users = true } }, ct);
		}

		DateTime now = DateTime.UtcNow;
		ConversationEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = userData.Id,
			CreatedAt = now,
			Tags = [group ? TagConversation.Group : TagConversation.Direct],
			Title = group ? p.Title : null,
			DirectKey = key,
			LastMessageAt = now,
			Users = members,
			JsonData = new ConversationJson { Detail1 = p.Detail1, Detail2 = p.Detail2, Reads = [new ConversationRead { UserId = userData.Id, At = now }] }
		};
		await db.Set<ConversationEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return await ReadConversationById(new IdParams<ConversationSelectorArgs> { Id = e.Id, Token = p.Token, ApiKey = p.ApiKey, SelectorArgs = new ConversationSelectorArgs { Users = true } }, ct);
	}

	public async Task<UResponse<IEnumerable<ConversationResponse>?>> ReadConversations(ConversationReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<ConversationResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		IQueryable<ConversationEntity> q = db.Set<ConversationEntity>().ApplyReadParams(p).Where(x => x.Users.Any(u => u.Id == userData.Id)).OrderByDescending(x => x.LastMessageAt);
		UResponse<IEnumerable<ConversationResponse>?> result = await q.Select(Projections.ConversationSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
		await FillUnread(result.Result ?? [], userData.Id, ct);
		return result;
	}

	public async Task<UResponse<ConversationResponse?>> ReadConversationById(IdParams<ConversationSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ConversationResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		ConversationResponse? e = await db.Set<ConversationEntity>().Where(x => x.Users.Any(u => u.Id == userData.Id))
			.Select(Projections.ConversationSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<ConversationResponse?>(null, Usc.NotFound, ls.Get("conversationNotFound"));
		await FillUnread([e], userData.Id, ct);
		return new UResponse<ConversationResponse?>(e);
	}

	public async Task<UResponse> UpdateConversation(ConversationUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		ConversationEntity? e = await db.Set<ConversationEntity>().AsTracking().Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null || e.Users.All(x => x.Id != userData.Id)) return new UResponse(Usc.NotFound, ls.Get("conversationNotFound"));
		if (!e.Tags.Contains(TagConversation.Group) || e.CreatorId != userData.Id && !e.AdminUserIds.Contains(userData.Id)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (p.Title.IsNotNull()) e.Title = p.Title;
		if (p.AddUserIds != null) {
			List<Guid> ids = p.AddUserIds.Where(x => e.Users.All(u => u.Id != x)).ToList();
			e.Users = e.Users.Concat(await db.Set<UserEntity>().AsTracking().Where(x => ids.Contains(x.Id)).ToListAsync(ct)).ToList();
		}

		if (p.RemoveUserIds != null) e.Users = e.Users.Where(x => x.Id == e.CreatorId || !p.RemoveUserIds.Contains(x.Id)).ToList();
		e.ApplyUpdateParam<ConversationEntity, TagConversation, ConversationJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> LeaveConversation(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		ConversationEntity? e = await db.Set<ConversationEntity>().AsTracking().Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null || e.Users.All(x => x.Id != userData.Id)) return new UResponse(Usc.NotFound, ls.Get("conversationNotFound"));

		e.Users = e.Users.Where(x => x.Id != userData.Id).ToList();
		// Nobody left (or a direct conversation): it goes away with its messages.
		if (e.Users.Count == 0 || e.Tags.Contains(TagConversation.Direct)) {
			await db.Set<MessageEntity>().Where(x => x.ConversationId == e.Id).ExecuteDeleteAsync(ct);
			db.Set<ConversationEntity>().Remove(e);
		}

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> MarkConversationRead(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		ConversationEntity? e = await db.Set<ConversationEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id && x.Users.Any(u => u.Id == userData.Id), ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("conversationNotFound"));
		e.JsonData.Reads = e.JsonData.Reads.Where(x => x.UserId != userData.Id).Append(new ConversationRead { UserId = userData.Id, At = DateTime.UtcNow }).ToList();
		await db.SaveChangesAsync(ct);
		await rt.ToGroup(RealtimeGroups.Conversation(e.Id), "read", new { ConversationId = e.Id, UserId = userData.Id });
		return new UResponse();
	}

	public async Task<UResponse<MessageResponse?>> CreateMessage(MessageCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<MessageResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<MessageResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		ConversationEntity? c = await db.Set<ConversationEntity>().AsTracking().Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == p.ConversationId, ct);
		if (c == null || c.Users.All(x => x.Id != userData.Id)) return new UResponse<MessageResponse?>(null, Usc.NotFound, ls.Get("conversationNotFound"));
		List<Guid> others = c.Users.Select(x => x.Id).Where(x => x != userData.Id).ToList();
		if (c.Tags.Contains(TagConversation.Direct) && others.Count == 1 && await AreBlocked(userData.Id, others[0], ct)) return new UResponse<MessageResponse?>(null, Usc.Forbidden, ls.Get("youCannotMessageThisUser"));

		DateTime now = DateTime.UtcNow;
		MessageEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = userData.Id,
			CreatedAt = now,
			Tags = [p.LinkType.IsNotNullOrEmpty() ? TagMessage.Shared : TagMessage.Text],
			Text = p.Text.Trim(),
			ConversationId = c.Id,
			JsonData = new MessageJson { Detail1 = p.Detail1, Detail2 = p.Detail2, ReplyToId = p.ReplyToId, LinkType = p.LinkType, LinkId = p.LinkId }
		};
		await db.Set<MessageEntity>().AddAsync(e, ct);

		c.LastMessageAt = now;
		c.JsonData.LastMessageText = e.Text.Length > 120 ? e.Text[..120] : e.Text;
		c.JsonData.LastMessageUserId = userData.Id;
		c.JsonData.Reads = c.JsonData.Reads.Where(x => x.UserId != userData.Id).Append(new ConversationRead { UserId = userData.Id, At = now }).ToList();
		await db.SaveChangesAsync(ct);

		MessageResponse? created = await db.Set<MessageEntity>().Select(Projections.MessageSelector(new MessageSelectorArgs { User = true })).FirstOrDefaultAsync(x => x.Id == e.Id, ct);
		await rt.ToGroup(RealtimeGroups.Conversation(c.Id), "message", created);
		await rt.ToUsers(others, "conversation", c.Id);
		return new UResponse<MessageResponse?>(created, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<MessageResponse>?>> ReadMessages(MessageReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<MessageResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (!userData.IsAdmin && !await IsMember(p.ConversationId, userData.Id, ct)) return new UResponse<IEnumerable<MessageResponse>?>(null, Usc.NotFound, ls.Get("conversationNotFound"));

		IQueryable<MessageEntity> q = db.Set<MessageEntity>().ApplyReadParams(p).Where(x => x.ConversationId == p.ConversationId);
		if (p.Before.HasValue) q = q.Where(x => x.CreatedAt < p.Before);
		return await q.OrderByDescending(x => x.CreatedAt).Select(Projections.MessageSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateMessage(MessageUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		MessageEntity? e = await db.Set<MessageEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null || e.Tags.Contains(TagMessage.Deleted)) return new UResponse(Usc.NotFound, ls.Get("messageNotFound"));
		if (e.CreatorId != userData.Id) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.Text.IsNotNullOrEmpty()) {
			e.Text = p.Text!.Trim();
			e.Tags = e.Tags.Where(x => x != TagMessage.Edited).Append(TagMessage.Edited).ToList();
		}

		await db.SaveChangesAsync(ct);
		await rt.ToGroup(RealtimeGroups.Conversation(e.ConversationId), "messageChanged", e.Id);
		return new UResponse();
	}

	public async Task<UResponse> DeleteMessage(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		MessageEntity? e = await db.Set<MessageEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("messageNotFound"));
		if (e.CreatorId != userData.Id && !userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		// Kept as "deleted" so replies and the order stay intact.
		e.Tags = e.Tags.Where(x => x != TagMessage.Deleted).Append(TagMessage.Deleted).ToList();
		e.Text = "";
		await db.SaveChangesAsync(ct);
		await rt.ToGroup(RealtimeGroups.Conversation(e.ConversationId), "messageChanged", e.Id);
		return new UResponse();
	}
}
