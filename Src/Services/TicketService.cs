namespace SinaMN75U.Services;

public interface ITicketService {
	Task<UResponse<TicketResponse?>> Create(TicketCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<TicketResponse>?>> Read(TicketReadParams p, CancellationToken ct);
	Task<UResponse<TicketResponse?>> Update(TicketUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class TicketService(
	DbContext db, 
	ILocalizationService ls,
	ITokenService ts,
	ILocalStorageService cache
	) : ITicketService {
	public async Task<UResponse<TicketResponse?>> Create(TicketCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TicketResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		EntityEntry<TicketEntity> e = await db.AddAsync(p.MapToEntity(userData.Id), ct);
		
		cache.DeleteAllByPartialKey(RouteTags.Ticket);
		await db.SaveChangesAsync(ct);
		return new UResponse<TicketResponse?>(e.Entity.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<TicketResponse>?>> Read(TicketReadParams p, CancellationToken ct) {
		IQueryable<TicketResponse> q = db.Set<TicketEntity>().Select(Projections.TicketSelector(p.SelectorArgs));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<TicketResponse?>> Update(TicketUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TicketResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		TicketEntity e = (await db.Set<TicketEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (p.Title.IsNotNullOrEmpty()) e.JsonData.Title = p.Title;
		if (p.Description.IsNotNullOrEmpty()) e.JsonData.Description = p.Description;
		if (p.Instagram.IsNotNullOrEmpty()) e.JsonData.Instagram = p.Instagram;
		if (p.Phone.IsNotNullOrEmpty()) e.JsonData.Phone = p.Phone;
		if (p.Telegram.IsNotNullOrEmpty()) e.JsonData.Telegram = p.Telegram;
		if (p.Whatsapp.IsNotNullOrEmpty()) e.JsonData.Whatsapp = p.Whatsapp;

		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));

		db.Update(e);
		await db.SaveChangesAsync(ct);
		
		cache.DeleteAllByPartialKey(RouteTags.Ticket);
		return new UResponse<TicketResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TicketEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<TicketEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);
		
		cache.DeleteAllByPartialKey(RouteTags.Ticket);
		return new UResponse();
	}
}