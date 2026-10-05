namespace SinaMN75U.Services;

public interface ICommentService {
	public Task<UResponse<Guid?>> Create(CommentCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<CommentResponse>?>> Read(CommentReadParams p, CancellationToken ct);
	public Task<UResponse<CommentResponse?>> ReadById(IdParams<CommentSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> Update(CommentUpdateParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdParams p, CancellationToken ct);
	public Task<UResponse<int>> ReadProductCommentCount(IdParams p, CancellationToken ct);
	public Task<UResponse<int>> ReadUserCommentCount(IdParams p, CancellationToken ct);
}

public class CommentService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IHotelService hs
) : ICommentService {
	// A comment is changed by its writer, a full admin, or an admin of the hotel/dorm it reviews.
	private async Task<bool> CanModerate(JwtClaimData u, CommentEntity e, CancellationToken ct) {
		if (u.IsAdmin || u.Id == e.CreatorId) return true;
		bool? place = await hs.CanActOnPlaceOf(u, e.HotelId, null, e.DormId, null, null, ct);
		if (place != null) return place.Value;
		return e.VenueId != null && await db.Set<VenueEntity>().AnyAsync(x => x.Id == e.VenueId && (x.CreatorId == u.Id || x.AdminUserIds.Contains(u.Id)), ct);
	}

	public async Task<UResponse<Guid?>> Create(CommentCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		Guid creatorId = userData.IsSuperAdmin ? p.CreatorId ?? userData.Id : userData.Id; // nobody else writes in another user's name
		CommentEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new CommentJson {
				Reacts = [new CommentReacts { Tag = p.Reaction ?? TagReaction.Like, UserId = creatorId }]
			},
			Tags = p.Tags,
			Score = p.Score,
			Description = p.Description,
			CreatorId = creatorId,
			UserId = p.UserId,
			ProductId = p.ProductId,
			BlogId = p.BlogId,
			HotelId = p.HotelId,
			DormId = p.DormId,
			VenueId = p.VenueId,
			ParentId = p.ParentId
		};

		await db.Set<CommentEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<CommentResponse>?>> Read(CommentReadParams p, CancellationToken ct) {
		IQueryable<CommentEntity> q = db.Set<CommentEntity>().ApplyReadParams(p);
		
		if (p.UserId.IsNotNull()) q = q.Where(x => x.UserId == p.UserId);
		if (p.ProductId.IsNotNull()) q = q.Where(x => x.ProductId == p.ProductId);
		if (p.BlogId.IsNotNull()) q = q.Where(x => x.BlogId == p.BlogId);
		if (p.HotelId.IsNotNull()) q = q.Where(x => x.HotelId == p.HotelId);
		if (p.DormId.IsNotNull()) q = q.Where(x => x.DormId == p.DormId);
		if (p.VenueId.IsNotNull()) q = q.Where(x => x.VenueId == p.VenueId);
		
		IQueryable<CommentResponse> projected = q.Select(Projections.CommentSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<CommentResponse?>> ReadById(IdParams<CommentSelectorArgs> p, CancellationToken ct) {
		CommentResponse? e = await db.Set<CommentEntity>()
			.Select(Projections.CommentSelector(p.SelectorArgs))
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<CommentResponse?>(null, Usc.NotFound, ls.Get("commentNotFound")) : new UResponse<CommentResponse?>(e);
	}

	public async Task<UResponse> Update(CommentUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		CommentEntity? e = await db.Set<CommentEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("commentNotFound"));
		if (!await CanModerate(userData, e, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (p.Score.IsNotNull()) e.Score = p.Score.Value;
		if (p.Description.IsNotNullOrEmpty()) e.Description = p.Description;

		e.ApplyUpdateParam<CommentEntity, TagComment, CommentJson>(p);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		
		CommentEntity? e = await db.Set<CommentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("commentNotFound"));
		if (!await CanModerate(userData, e, ct)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		db.Set<CommentEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		
		return new UResponse();
	}

	public async Task<UResponse<int>> ReadProductCommentCount(IdParams p, CancellationToken ct) {
		int count = await db.Set<CommentEntity>().Where(x => x.ProductId == p.Id).CountAsync(ct);
		return new UResponse<int>(count);
	}

	public async Task<UResponse<int>> ReadUserCommentCount(IdParams p, CancellationToken ct) {
		int count = await db.Set<CommentEntity>().Where(x => x.UserId == p.Id).CountAsync(ct);
		return new UResponse<int>(count);
	}
}