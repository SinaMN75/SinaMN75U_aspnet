namespace SinaMN75U.Services;

public interface IContentService {
	Task<UResponse> BulkCreate(List<ContentCreateParams> p, CancellationToken ct);
	Task<UResponse<Guid?>> Create(ContentCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ContentResponse>?>> Read(ContentReadParams p, CancellationToken ct);
	Task<UResponse<ContentResponse?>> ReadById(IdParams<ContentSelectorArgs> p, CancellationToken ct);
	Task<UResponse> Update(ContentUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> DeleteRange(IdListParams p, CancellationToken ct);
}

public class ContentService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IMediaService mediaService
) : IContentService {
	public async Task<UResponse> BulkCreate(List<ContentCreateParams> p, CancellationToken ct) {
		foreach (ContentCreateParams param in p) await Create(param, ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> Create(ContentCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("TokenExpired"));

		ContentEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			AdminUserIds = p.AdminUserIds ?? [],
			JsonData = new ContentJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				Title = p.Title,
				SubTitle = p.SubTitle,
				Description = p.Description,
				ImageBase64 = p.ImageBase64,
				IconBase64 = p.IconBase64,
				ButtonText = p.ButtonText,
				ButtonLink = p.ButtonLink,
				Link = p.Link,
				Order = p.Order,
				Instagram = p.Instagram,
				Telegram = p.Telegram,
				Whatsapp = p.Whatsapp,
				Phone = p.Phone,
				Links = p.Links,
				Items = p.Items
			}
		};

		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		await AddMedia(e.Id, p.Media ?? [], ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<ContentResponse>?>> Read(ContentReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		IQueryable<ContentEntity> q = db.Set<ContentEntity>().ApplyReadParams(p).ApplyAdminScope<ContentEntity, TagContent>(userData);

		if (p.Query.IsNotNullOrEmpty()) q = q.Where(x =>
			(x.JsonData.Title ?? "").Contains(p.Query!) ||
			(x.JsonData.SubTitle ?? "").Contains(p.Query!) ||
			(x.JsonData.Description ?? "").Contains(p.Query!));

		IQueryable<ContentResponse> projected = q.Select(Projections.ContentSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ContentResponse?>> ReadById(IdParams<ContentSelectorArgs> p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		ContentResponse? e = await db.Set<ContentEntity>().ApplyAdminScope<ContentEntity, TagContent>(userData)
			.Select(Projections.ContentSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<ContentResponse?>(null, Usc.NotFound, ls.Get("ContentNotFound")) : new UResponse<ContentResponse?>(e);
	}

	public async Task<UResponse> Update(ContentUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ContentEntity? e = await db.Set<ContentEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ContentNotFound"));
		if (!userData.CanManage(e.CreatorId, e.AdminUserIds)) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		if (p.Title.IsNotNull()) e.JsonData.Title = p.Title;
		if (p.SubTitle.IsNotNull()) e.JsonData.SubTitle = p.SubTitle;
		if (p.Description.IsNotNull()) e.JsonData.Description = p.Description;
		if (p.ImageBase64.IsNotNull()) e.JsonData.ImageBase64 = p.ImageBase64;
		if (p.IconBase64.IsNotNull()) e.JsonData.IconBase64 = p.IconBase64;
		if (p.ButtonText.IsNotNull()) e.JsonData.ButtonText = p.ButtonText;
		if (p.ButtonLink.IsNotNull()) e.JsonData.ButtonLink = p.ButtonLink;
		if (p.Link.IsNotNull()) e.JsonData.Link = p.Link;
		if (p.Order.IsNotNull()) e.JsonData.Order = p.Order;
		if (p.Instagram.IsNotNull()) e.JsonData.Instagram = p.Instagram;
		if (p.Telegram.IsNotNull()) e.JsonData.Telegram = p.Telegram;
		if (p.Whatsapp.IsNotNull()) e.JsonData.Whatsapp = p.Whatsapp;
		if (p.Phone.IsNotNull()) e.JsonData.Phone = p.Phone;
		if (p.Links.IsNotNull()) e.JsonData.Links = p.Links!;
		if (p.Items.IsNotNull()) e.JsonData.Items = p.Items!;
		if (p.CreatorId.IsNotNull()) e.CreatorId = p.CreatorId!.Value;

		db.Set<ContentEntity>().Update(e.ApplyUpdateParam<ContentEntity, TagContent, ContentJson>(p));
		await db.SaveChangesAsync(ct);
		await AddMedia(p.Id, p.Media ?? [], ct);
		return new UResponse(Usc.Success, ls.Get("ContentUpdated"));
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ContentEntity? e = await db.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("ContentNotFound"));
		if (!userData.CanManage(e.CreatorId, e.AdminUserIds)) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		db.Set<ContentEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse(Usc.Deleted, ls.Get("ContentDeleted"));
	}

	public async Task<UResponse> DeleteRange(IdListParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		int count = await db.Set<ContentEntity>().WhereIn(u => u.Id, p.Ids).ExecuteDeleteAsync(ct);
		return count > 0 ? new UResponse(Usc.Deleted, ls.Get("ContentDeleted")) : new UResponse(Usc.NotFound, ls.Get("ContentNotFound"));
	}

	private async Task AddMedia(Guid contentId, ICollection<Guid> ids, CancellationToken ct) {
		if (ids.IsNullOrEmpty()) return;
		List<MediaEntity> media = await mediaService.ReadEntity(new BaseReadParams<TagMedia> { Ids = ids }, ct) ?? [];
		if (media.Count == 0) return;
		foreach (MediaEntity i in media)
			await db.Set<MediaEntity>().Where(x => x.Id == i.Id).ExecuteUpdateAsync(u => u.SetProperty(y => y.ContentId, contentId), ct);
	}
}
