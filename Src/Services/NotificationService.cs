namespace SinaMN75U.Services;

public interface INotificationService {
	Task<UResponse<Guid?>> Create(NotificationCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<NotificationResponse>?>> Read(NotificationReadParams p, CancellationToken ct);
	Task<UResponse> Update(NotificationUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class NotificationService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : INotificationService {
	public async Task<UResponse<Guid?>> Create(NotificationCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		NotificationEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new GeneralJsonData { Title = p.Title, Description = p.Description, },
			Tags = p.Tags,
			CreatorId = p.CreatorId ?? userData.Id,
			Userd = p.UserId
		};

		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<NotificationResponse>?>> Read(NotificationReadParams p, CancellationToken ct) {
		IQueryable<NotificationEntity> q = db.Set<NotificationEntity>();

		if (p.OrderByCreatedAt) q = q.OrderBy(x => x.CreatedAt);
		if (p.OrderByCreatedAtDesc) q = q.OrderByDescending(x => x.CreatedAt);
		if (p.CreatorId != null) q = q.Where(x => x.CreatorId == p.CreatorId);
		if (p.UserId != null) q = q.Where(x => x.Userd == p.UserId);

		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => p.Tags.All(tag => x.Tags.Contains(tag)));
		if (p.Ids.IsNotNullOrEmpty()) q = q.Where(x => p.Ids.Contains(x.Id));

		IQueryable<NotificationResponse> projected = q.Select(Projections.NotificationSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(NotificationUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		NotificationEntity? e = await db.Set<NotificationEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("NotificationNotFound"));

		if (p.Title != null) e.JsonData.Title = p.Title;
		if (p.Description != null) e.JsonData.Description = p.Description;
		if (p.Tags != null) e.Tags = p.Tags;

		db.Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<NotificationEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}
}