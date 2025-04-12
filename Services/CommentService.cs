namespace SinaMN75U.Services;

public interface ICommentService {
	public Task<UResponse<CommentResponse?>> Create(CommentCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<CommentResponse>?>> Read(CommentReadParams p, CancellationToken ct);
	public Task<UResponse<CommentResponse?>> ReadById(IdParams p, CancellationToken ct);
	public Task<UResponse<CommentResponse?>> Update(CommentUpdateParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class CommentService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : ICommentService {
	public async Task<UResponse<CommentResponse?>> Create(CommentCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<CommentResponse?>(null, USC.UnAuthorized, ls.Get("AuthorizationRequired"));

		EntityEntry<CommentEntity> e = await db.Set<CommentEntity>().AddAsync(new CommentEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			Score = p.Score,
			Json = new CommentJson(),
			Description = p.Description,
			UserId = p.UserId ?? userData.Id,
			Tags = p.Tags,
			ProductId = p.ProductId,
			ParentId = p.ParentId,
			TargetUserId = p.TargetUserId
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<CommentResponse?>(e.Entity.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<CommentResponse>?>> Read(CommentReadParams p, CancellationToken ct) {
		IQueryable<CommentEntity> q = db.Set<CommentEntity>();
		if (p.ProductId.IsNotNullOrEmpty()) q = q.Where(x => x.ProductId == p.ProductId);
		if (p.UserId.IsNotNullOrEmpty()) q = q.Where(x => x.UserId == p.UserId);
		if (p.TargetUserId.IsNotNullOrEmpty()) q = q.Where(x => x.TargetUserId == p.TargetUserId);
		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags.Any(tag => p.Tags!.Contains(tag)));

		return await q.Select(x => x.MapToResponse(p.ShowMedia)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<CommentResponse?>> ReadById(IdParams p, CancellationToken ct) {
		CommentEntity? e = await db.Set<CommentEntity>().Select(x => x.MapToEntity(true)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<CommentResponse?>(null, USC.NotFound, ls.Get("CommentNotFound")) : new UResponse<CommentResponse?>(e.MapToResponse(true));
	}

	public async Task<UResponse<CommentResponse?>> Update(CommentUpdateParams p, CancellationToken ct) {
		CommentEntity? e = await db.Set<CommentEntity>().Select(x => x.MapToEntity(false)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<CommentResponse?>(null, USC.NotFound, ls.Get("CommentNotFound"));
		if (p.Score.IsNotNullOrEmpty()) e.Score = p.Score.Value;
		if (p.Description.IsNotNullOrEmpty()) e.Description = p.Description;
		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => p.RemoveTags.Contains(x));
		db.Set<CommentEntity>().Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<CommentResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		await db.Set<CommentEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync();
		return new UResponse();
	}
}