namespace SinaMN75U.Services;

public interface ITicketService {
	Task<UResponse<Guid?>> Create(TicketCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<TicketResponse>?>> Read(TicketReadParams p, CancellationToken ct);
	Task<UResponse> Update(TicketUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class TicketService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : ITicketService {
	public async Task<UResponse<Guid?>> Create(TicketCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		TicketEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new TicketJson {
				Title = p.Title,
				Description = p.Description,
				Instagram = p.Instagram,
				Telegram = p.Telegram,
				Whatsapp = p.Whatsapp,
				Phone = p.Phone
			},
			Tags = p.Tags,
			CreatorId = userData.Id
		};

		await db.Set<TicketEntity>().AddAsync(e, ct);

		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<TicketResponse>?>> Read(TicketReadParams p, CancellationToken ct) {
		IQueryable<TicketEntity> q = db.Set<TicketEntity>().ApplyReadParams<TicketEntity, TagTicket, TicketJson>(p);
		
		IQueryable<TicketResponse> projected = q.Select(Projections.TicketSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(TicketUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		TicketEntity e = (await db.Set<TicketEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		if (p.Title.IsNotNullOrEmpty()) e.JsonData.Title = p.Title;
		if (p.Description.IsNotNullOrEmpty()) e.JsonData.Description = p.Description;
		if (p.Instagram.IsNotNullOrEmpty()) e.JsonData.Instagram = p.Instagram;
		if (p.Phone.IsNotNullOrEmpty()) e.JsonData.Phone = p.Phone;
		if (p.Telegram.IsNotNullOrEmpty()) e.JsonData.Telegram = p.Telegram;
		if (p.Whatsapp.IsNotNullOrEmpty()) e.JsonData.Whatsapp = p.Whatsapp;
		
		db.Set<TicketEntity>().Update(e.ApplyUpdateParam<TicketEntity,TagTicket, TicketJson>(p));
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<TicketEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}
}