namespace SinaMN75U.Services;

public interface ITerminalService {
	Task<UResponse<Guid?>> Create(TerminalCreateParams p, CancellationToken ct);
	Task<UResponse> BulkCreate(TerminalBulkCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<TerminalResponse>?>> Read(TerminalReadParams p, CancellationToken ct);
	Task<UResponse> Update(TerminalUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class TerminalService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : ITerminalService {
	public async Task<UResponse<Guid?>> Create(TerminalCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		TerminalEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			Serial = p.Serial,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJsonData(),
			Tags = p.Tags,
			CreatorId = p.CreatorId ?? userData.Id,
			SimCardNumber = p.SimCardNumber,
			SimCardSerial = p.SimCardSerial,
			Imei = p.Imei,
			TerminalId = p.TerminalId,
			MerchantId =  p.MerchantId
		};

		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse> BulkCreate(TerminalBulkCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		
		List<TerminalEntity> entities = [];
		
		entities.AddRange(p.List.Select(x => new TerminalEntity {
			Serial = x.Serial,
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJsonData(),
			Tags = x.Tags,
			CreatorId = userData.Id
		}));
		
		await db.Set<TerminalEntity>().AddRangeAsync(entities, ct);
		await db.SaveChangesAsync(ct);
		
		return new UResponse();
	}

	public async Task<UResponse<IEnumerable<TerminalResponse>?>> Read(TerminalReadParams p, CancellationToken ct) {
		IQueryable<TerminalEntity> q = db.Set<TerminalEntity>().ApplyReadParams<TerminalEntity, TagTerminal, BaseJsonData>(p);

		if (p.Serial.IsNotNullOrEmpty()) q = q.Where(x => x.Serial == p.Serial);
		if (p.TerminalId.IsNotNullOrEmpty()) q = q.Where(x => x.TerminalId == p.TerminalId);
		if (p.MerchantId.IsNotNullOrEmpty()) q = q.Where(x => x.MerchantId == p.MerchantId);
		if (p.Imei.IsNotNullOrEmpty()) q = q.Where(x => x.Imei == p.Imei);
		if (p.SimCardNumber.IsNotNullOrEmpty()) q = q.Where(x => x.SimCardNumber == p.SimCardNumber);
		if (p.SimCardSerial.IsNotNullOrEmpty()) q = q.Where(x => x.SimCardSerial == p.SimCardSerial);
		
		IQueryable<TerminalResponse> projected = q.Select(Projections.TerminalSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(TerminalUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		TerminalEntity? e = await db.Set<TerminalEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("TerminalNotFound"));

		if (p.Serial.IsNotNullOrEmpty()) e.Serial = p.Serial;
		if (p.Imei.IsNotNullOrEmpty()) e.Imei = p.Imei;
		if (p.SimCardNumber.IsNotNullOrEmpty()) e.SimCardNumber = p.SimCardNumber;
		if (p.SimCardSerial.IsNotNullOrEmpty()) e.SimCardSerial = p.SimCardSerial;
		if (p.TerminalId.IsNotNullOrEmpty()) e.TerminalId = p.TerminalId;
		if (p.MerchantId.IsNotNullOrEmpty()) e.MerchantId = p.MerchantId;

		db.Set<TerminalEntity>().Update(e.ApplyUpdateParam<TerminalEntity,TagTerminal, BaseJsonData>(p));
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<TerminalEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}
}