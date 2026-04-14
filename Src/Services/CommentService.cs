namespace SinaMN75U.Services;

public interface ICommentService {
	public Task<UResponse<Guid?>> Create(CommentCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<CommentResponse>?>> Read(CommentReadParams p, CancellationToken ct);
	public Task<UResponse<CommentResponse?>> ReadById(IdParams p, CancellationToken ct);
	public Task<UResponse> Update(CommentUpdateParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdParams p, CancellationToken ct);
	public Task<UResponse<int>> ReadProductCommentCount(IdParams p, CancellationToken ct);
	public Task<UResponse<int>> ReadUserCommentCount(IdParams p, CancellationToken ct);
}

public class CommentService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : ICommentService {
	public async Task<UResponse<Guid?>> Create(CommentCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		CommentEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new CommentJson(),
			Tags = p.Tags,
			Score = p.Score,
			Description = p.Description,
			CreatorId = p.CreatorId ?? userData.Id,
			UserId = p.UserId,
			ProductId = p.ProductId,
			ParentId = p.ParentId
		};

		await db.Set<CommentEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<CommentResponse>?>> Read(CommentReadParams p, CancellationToken ct) {
		IQueryable<CommentEntity> q = db.Set<CommentEntity>();

		if (p.ProductId.IsNotNull()) q = q.Where(x => x.ProductId == p.ProductId);
		if (p.CreatorId.IsNotNull()) q = q.Where(x => x.CreatorId == p.CreatorId);
		if (p.UserId.IsNotNull()) q = q.Where(x => x.UserId == p.UserId);
		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags.Any(tag => p.Tags!.Contains(tag)));

		IQueryable<CommentResponse> list = q.Select(Projections.CommentSelector(p.SelectorArgs));

		return await list.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<CommentResponse?>> ReadById(IdParams p, CancellationToken ct) {
		CommentResponse? e = await db.Set<CommentEntity>()
			.Select(Projections.CommentSelector(new CommentSelectorArgs()))
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<CommentResponse?>(null, Usc.NotFound, ls.Get("CommentNotFound")) : new UResponse<CommentResponse?>(e);
	}

	public async Task<UResponse> Update(CommentUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		CommentEntity? e = await db.Set<CommentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("CommentNotFound"));
		
		if (p.Score.IsNotNull()) e.Score = p.Score.Value;
		if (p.Description.IsNotNullOrEmpty()) e.Description = p.Description;
		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => p.RemoveTags.Contains(x));
		if (p.Tags != null) e.Tags = p.Tags;
		
		db.Set<CommentEntity>().Update(e);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<CommentEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);
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