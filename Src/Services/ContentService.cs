namespace SinaMN75U.Services;

public interface IContentService {
	Task<UResponse<Guid?>> Create(ContentCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ContentResponse>?>> Read(ContentReadParams p, CancellationToken ct);
	Task<UResponse> Update(ContentUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct);
}

public class ContentService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IContentService {
	public async Task<UResponse<Guid?>> Create(ContentCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		ContentEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			JsonData = new ContentJson {
				Title = p.Title,
				SubTitle = p.SubTitle,
				Description = p.Description,
				Instagram = p.Instagram,
				Telegram = p.Telegram,
				Whatsapp = p.Whatsapp,
				Phone = p.Phone
			},
			Tags = p.Tags
		};
		
		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<ContentResponse>?>> Read(ContentReadParams p, CancellationToken ct) {
		IQueryable<ContentResponse> q = db.Set<ContentEntity>().Select(Projections.ContentSelector(p.SelectorArgs));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(ContentUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		ContentEntity? e = await db.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ContentNotFound"));
		p.MapToEntity(e);
		db.Update(p.MapToEntity(e));
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<ContentEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<ContentEntity>().Where(x => p.Id == x.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.DeletedAt, p.DateTime ?? DateTime.UtcNow), ct);
		return new UResponse();
	}
}