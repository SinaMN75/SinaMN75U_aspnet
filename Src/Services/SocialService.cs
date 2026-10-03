namespace SinaMN75U.Services;

public interface ISocialService {
	public Task<UResponse<Guid?>> CreatePost(PostCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<PostResponse>?>> ReadPosts(PostReadParams p, CancellationToken ct);
	public Task<UResponse<PostResponse?>> ReadPostById(IdParams<PostSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> UpdatePost(PostUpdateParams p, CancellationToken ct);
	public Task<UResponse> DeletePost(IdParams p, CancellationToken ct);
	public Task<UResponse> ReactPost(PostReactParams p, CancellationToken ct);
	public Task<UResponse> ViewStory(IdParams p, CancellationToken ct);

	public Task<UResponse<Guid?>> CreateReport(ReportCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<ReportResponse>?>> ReadReports(ReportReadParams p, CancellationToken ct);
	public Task<UResponse> UpdateReport(ReportUpdateParams p, CancellationToken ct);

	public Task<UResponse> Block(BlockCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<BlockResponse>?>> ReadBlocks(BlockReadParams p, CancellationToken ct);
	public Task<UResponse> Unblock(BlockCreateParams p, CancellationToken ct);
}

public class SocialService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IRealtimeService rt
) : ISocialService {
	// ---- Who sees and changes what ----
	// Posts are public or for followers; replies are posts with a ParentId; stories expire after a day.
	// Blocking works both ways: neither side sees the other's posts. Admins hide posts (Hidden) and handle reports.

	private static readonly TimeSpan StoryLife = TimeSpan.FromHours(24);

	/// <summary>Everybody the user blocked or was blocked by.</summary>
	private Task<List<Guid>> BlockedWith(Guid userId, CancellationToken ct) => db.Set<BlockEntity>()
		.Where(x => x.CreatorId == userId || x.BlockedUserId == userId)
		.Select(x => x.CreatorId == userId ? x.BlockedUserId : x.CreatorId)
		.ToListAsync(ct);

	private static void Fill(PostResponse post, Guid viewerId) {
		post.ReactionCount = post.JsonData.Reactions.Count;
		post.MyReaction = post.JsonData.Reactions.FirstOrDefault(x => x.UserId == viewerId)?.Tag;
		// Who saw a story is only for its author.
		if (post.CreatorId != viewerId) post.JsonData.ViewerIds = [];
		foreach (PostResponse child in post.Children ?? []) Fill(child, viewerId);
	}

	// ---------------- Post ----------------

	public async Task<UResponse<Guid?>> CreatePost(PostCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		PostEntity? parent = null;
		if (p.ParentId.HasValue) {
			parent = await db.Set<PostEntity>().FirstOrDefaultAsync(x => x.Id == p.ParentId, ct);
			if (parent == null || parent.Tags.Contains(TagPost.Hidden)) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("postNotFound"));
			if (await db.Set<BlockEntity>().AnyAsync(x => x.CreatorId == parent.CreatorId && x.BlockedUserId == userData.Id, ct)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		}

		TagPost kind = parent != null ? TagPost.Comment : p.Tags.Contains(TagPost.Story) ? TagPost.Story : TagPost.Post;
		PostEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [kind, p.Tags.Contains(TagPost.Followers) ? TagPost.Followers : TagPost.Public],
			Text = p.Text,
			ParentId = parent?.Id,
			ExpiresAt = kind == TagPost.Story ? DateTime.UtcNow + StoryLife : null,
			JsonData = new PostJson { Detail1 = p.Detail1, Detail2 = p.Detail2, LinkType = p.LinkType, LinkId = p.LinkId }
		};

		await db.Set<PostEntity>().AddAsync(e, ct);
		if (parent != null) await db.AddNotifications([parent.CreatorId], userData.Id, "notifNewReply", userData.FullName, "post", parent.Id, ct, TagNotification.Social);
		await db.SaveChangesAsync(ct);
		if (parent != null) await rt.ToUsers([parent.CreatorId], "notification");
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<PostResponse>?>> ReadPosts(PostReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		Guid uid = userData?.Id ?? Guid.Empty;
		List<Guid> blocked = userData == null ? [] : await BlockedWith(uid, ct);
		List<Guid> following = userData == null ? [] : await db.Set<FollowEntity>().Where(x => x.CreatorId == uid && x.UserId != null).Select(x => x.UserId!.Value).ToListAsync(ct);

		IQueryable<PostEntity> q = db.Set<PostEntity>().ApplyReadParams(p).Where(x => !blocked.Contains(x.CreatorId));
		if (userData is not { IsAdmin: true }) {
			q = q.Where(x => !x.Tags.Contains(TagPost.Hidden) || x.CreatorId == uid);
			// Followers-only posts: for the author's followers and the author.
			q = q.Where(x => x.Tags.Contains(TagPost.Public) || x.CreatorId == uid || following.Contains(x.CreatorId));
		}

		if (p.ParentId.HasValue) q = q.Where(x => x.ParentId == p.ParentId).OrderBy(x => x.CreatedAt);
		else {
			q = q.Where(x => x.ParentId == null);
			if (p.Stories) q = q.Where(x => x.Tags.Contains(TagPost.Story) && x.ExpiresAt > DateTime.UtcNow);
			else if (p.Tags.IsNullOrEmpty()) q = q.Where(x => x.Tags.Contains(TagPost.Post));
			if (p.Feed) q = q.Where(x => x.CreatorId == uid || following.Contains(x.CreatorId));
			q = q.OrderByDescending(x => x.CreatedAt);
		}

		if (p.UserId.HasValue) q = q.Where(x => x.CreatorId == p.UserId);

		UResponse<IEnumerable<PostResponse>?> result = await q.Select(Projections.PostSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
		foreach (PostResponse post in result.Result ?? []) Fill(post, uid);
		return result;
	}

	public async Task<UResponse<PostResponse?>> ReadPostById(IdParams<PostSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		Guid uid = userData?.Id ?? Guid.Empty;
		PostResponse? e = await db.Set<PostEntity>().Select(Projections.PostSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null || e.Tags.Contains(TagPost.Hidden) && userData is not { IsAdmin: true } && e.CreatorId != uid) return new UResponse<PostResponse?>(null, Usc.NotFound, ls.Get("postNotFound"));
		if (userData != null && e.CreatorId != null && (await BlockedWith(uid, ct)).Contains(e.CreatorId.Value)) return new UResponse<PostResponse?>(null, Usc.NotFound, ls.Get("postNotFound"));
		Fill(e, uid);
		return new UResponse<PostResponse?>(e);
	}

	public async Task<UResponse> UpdatePost(PostUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		PostEntity? e = await db.Set<PostEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("postNotFound"));
		if (!userData.IsAdmin && e.CreatorId != userData.Id) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		bool hidden = e.Tags.Contains(TagPost.Hidden);
		if (p.Text.IsNotNull()) e.Text = p.Text;
		e.ApplyUpdateParam<PostEntity, TagPost, PostJson>(p);
		// Only moderators hide or show a post.
		if (!userData.IsAdmin) e.Tags = e.Tags.Where(x => x != TagPost.Hidden).Concat(hidden ? [TagPost.Hidden] : []).ToList();
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeletePost(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		PostEntity? e = await db.Set<PostEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("postNotFound"));
		// The author, a moderator, or the author of the post a reply is under.
		bool parentAuthor = e.ParentId != null && await db.Set<PostEntity>().AnyAsync(x => x.Id == e.ParentId && x.CreatorId == userData.Id, ct);
		if (!userData.IsAdmin && e.CreatorId != userData.Id && !parentAuthor) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		List<Guid> ids = await db.Set<PostEntity>().Where(x => x.ParentId == e.Id).Select(x => x.Id).ToListAsync(ct);
		ids.Add(e.Id);
		await db.Set<MediaEntity>().Where(x => x.PostId != null && ids.Contains(x.PostId.Value)).ExecuteDeleteAsync(ct);
		await db.Set<PostEntity>().Where(x => x.ParentId == e.Id).ExecuteDeleteAsync(ct);
		await db.Set<PostEntity>().Where(x => x.Id == e.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> ReactPost(PostReactParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		PostEntity? e = await db.Set<PostEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null || e.Tags.Contains(TagPost.Hidden)) return new UResponse(Usc.NotFound, ls.Get("postNotFound"));

		bool isNew = e.JsonData.Reactions.All(x => x.UserId != userData.Id);
		e.JsonData.Reactions = e.JsonData.Reactions.Where(x => x.UserId != userData.Id).ToList();
		if (p.Tag.HasValue) e.JsonData.Reactions.Add(new PostReaction { UserId = userData.Id, Tag = p.Tag.Value });
		if (isNew && p.Tag.HasValue) await db.AddNotifications([e.CreatorId], userData.Id, "notifNewReaction", userData.FullName, "post", e.Id, ct, TagNotification.Social);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> ViewStory(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		PostEntity? e = await db.Set<PostEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("postNotFound"));
		if (e.CreatorId != userData.Id && !e.JsonData.ViewerIds.Contains(userData.Id)) {
			e.JsonData.ViewerIds.Add(userData.Id);
			await db.SaveChangesAsync(ct);
		}

		return new UResponse();
	}

	// ---------------- Report ----------------

	public async Task<UResponse<Guid?>> CreateReport(ReportCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		List<TagReport> kinds = p.Tags.Where(x => (int)x / 100 == 1).Distinct().ToList();
		if (kinds.Count != 1) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("reportTypeIsRequired"));

		ReportEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatorId = userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [kinds[0], TagReport.Pending],
			TargetId = p.TargetId,
			Reason = p.Reason,
			JsonData = new ReportJson { Detail1 = p.Detail1, Detail2 = p.Detail2 }
		};
		await db.Set<ReportEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created, ls.Get("thanksForYourReport"));
	}

	public async Task<UResponse<IEnumerable<ReportResponse>?>> ReadReports(ReportReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<ReportResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		IQueryable<ReportEntity> q = db.Set<ReportEntity>().ApplyReadParams(p);
		if (!userData.IsAdmin) {
			q = q.Where(x => x.CreatorId == userData.Id);
			p.SelectorArgs.Creator = null;
		}

		if (p.TargetId.HasValue) q = q.Where(x => x.TargetId == p.TargetId);
		return await q.Select(Projections.ReportSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateReport(ReportUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		ReportEntity? e = await db.Set<ReportEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("reportNotFound"));
		if (p.Note.IsNotNull()) e.JsonData.Note = p.Note;
		e.ApplyUpdateParam<ReportEntity, TagReport, ReportJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	// ---------------- Block ----------------

	public async Task<UResponse> Block(BlockCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (p.UserId == userData.Id) return new UResponse(Usc.BadRequest, ls.Get("youCannotBlockYourself"));
		if (!await db.Set<UserEntity>().AnyAsync(x => x.Id == p.UserId, ct)) return new UResponse(Usc.NotFound, ls.Get("userNotFound"));
		if (await db.Set<BlockEntity>().AnyAsync(x => x.CreatorId == userData.Id && x.BlockedUserId == p.UserId, ct)) return new UResponse();

		await db.Set<BlockEntity>().AddAsync(new BlockEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [TagBlock.User],
			BlockedUserId = p.UserId,
			JsonData = new BlockJson()
		}, ct);
		// Blocking also ends following in both directions.
		await db.Set<FollowEntity>().Where(x => x.CreatorId == userData.Id && x.UserId == p.UserId || x.CreatorId == p.UserId && x.UserId == userData.Id).ExecuteDeleteAsync(ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<IEnumerable<BlockResponse>?>> ReadBlocks(BlockReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<BlockResponse>?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		IQueryable<BlockEntity> q = db.Set<BlockEntity>().ApplyReadParams(p);
		if (!userData.IsAdmin || p.CreatorId == null) q = q.Where(x => x.CreatorId == userData.Id);
		return await q.Select(Projections.BlockSelector()).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Unblock(BlockCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		await db.Set<BlockEntity>().Where(x => x.CreatorId == userData.Id && x.BlockedUserId == p.UserId).ExecuteDeleteAsync(ct);
		return new UResponse();
	}
}
