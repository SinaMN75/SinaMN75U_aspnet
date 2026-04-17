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
			JsonData = new BaseJsonData { Detail1 = p.Detail1, Detail2 = p.Detail2 },
			Tags = p.Tags,
			CreatorId = p.CreatorId ?? userData.Id,
			Userd = p.UserId
		};

		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<NotificationResponse>?>> Read(NotificationReadParams p, CancellationToken ct) {
		IQueryable<NotificationEntity> q = db.Set<NotificationEntity>().ApplyReadParams<NotificationEntity, TagNotification, BaseJsonData>(p);
		
		if (p.UserId.IsNotNull()) q = q.Where(x => x.Userd == p.UserId);
		
		IQueryable<NotificationResponse> projected = q.Select(Projections.NotificationSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(NotificationUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		NotificationEntity? e = await db.Set<NotificationEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("NotificationNotFound"));

		db.Update(e.ApplyUpdateParam<NotificationEntity,TagNotification, BaseJsonData>(p));
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