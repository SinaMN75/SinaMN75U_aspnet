using SinaMN75U.Data;

namespace SinaMN75U.Services;

public interface ICommentService {
	public Task<UResponse<CommentResponse?>> Create(CommentCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<CommentResponse>?>> Read(CommentReadParams p, CancellationToken ct);
	public Task<UResponse<CommentResponse?>> ReadById(IdParams p, CancellationToken ct);
	public Task<UResponse<CommentResponse?>> Update(CommentUpdateParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdParams p, CancellationToken ct);
	public Task<UResponse<int>> ReadProductCommentCount(IdParams p, CancellationToken ct);
	public Task<UResponse<int>> ReadUserCommentCount(IdParams p, CancellationToken ct);
}

public class CommentService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	ILocalStorageService cache
) : ICommentService {
	public async Task<UResponse<CommentResponse?>> Create(CommentCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<CommentResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		EntityEntry<CommentEntity> e = await db.Set<CommentEntity>().AddAsync(p.MapToEntity(), ct);
		await db.SaveChangesAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Comment);
		return new UResponse<CommentResponse?>(e.Entity.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<CommentResponse>?>> Read(CommentReadParams p, CancellationToken ct) {
		IQueryable<CommentEntity> q = db.Set<CommentEntity>();
		if (p.ProductId.HasValue()) q = q.Where(x => x.ProductId == p.ProductId);
		if (p.UserId.HasValue()) q = q.Where(x => x.UserId == p.UserId);
		if (p.TargetUserId.HasValue()) q = q.Where(x => x.TargetUserId == p.TargetUserId);
		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags.Any(tag => p.Tags!.Contains(tag)));

		IQueryable<CommentResponse> list = q.Select(Projections.CommentSelector(
				user: p.ShowUser,
				targetUser: p.ShowTargetUser,
				product: p.ShowProduct,
				media: p.ShowMedia,
				children: p.ShowChildren
			)
		);


		return await list.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<CommentResponse?>> ReadById(IdParams p, CancellationToken ct) {
		CommentResponse? e = await db.Set<CommentEntity>()
			.Select(Projections.CommentSelector( user: true, targetUser: true, product: true, media: true, children: true))
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<CommentResponse?>(null, Usc.NotFound, ls.Get("CommentNotFound")) : new UResponse<CommentResponse?>(e);
	}

	public async Task<UResponse<CommentResponse?>> Update(CommentUpdateParams p, CancellationToken ct) {
		CommentEntity? e = await db.Set<CommentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<CommentResponse?>(null, Usc.NotFound, ls.Get("CommentNotFound"));
		if (p.Score.IsNotNull()) e.Score = p.Score.Value;
		if (p.Description.HasValue()) e.Description = p.Description;
		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => p.RemoveTags.Contains(x));
		db.Set<CommentEntity>().Update(p.MapToEntity(e));
		await db.SaveChangesAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Comment);
		return new UResponse<CommentResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		await db.Set<CommentEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Comment);
		return new UResponse();
	}

	public async Task<UResponse<int>> ReadProductCommentCount(IdParams p, CancellationToken ct) {
		int count = await db.Set<CommentEntity>().Where(x => x.ProductId == p.Id).CountAsync(ct);
		return new UResponse<int>(count);
	}

	public async Task<UResponse<int>> ReadUserCommentCount(IdParams p, CancellationToken ct) {
		int count = await db.Set<CommentEntity>().Where(x => x.TargetUserId == p.Id).CountAsync(ct);
		return new UResponse<int>(count);
	}
}