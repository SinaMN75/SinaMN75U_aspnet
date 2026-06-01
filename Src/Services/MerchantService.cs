namespace SinaMN75U.Services;

public interface IMerchantService {
	Task<UResponse<Guid?>> Create(MerchantCreateParams p, CancellationToken ct);
	Task<UResponse> Bind(MerchantBindParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<MerchantResponse>?>> Read(MerchantReadParams p, CancellationToken ct);
	Task<UResponse<MerchantResponse?>> ReadById(IdParams<MerchantSelectorArgs> p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class MerchantService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IHttpClientService http
) : IMerchantService {
	public async Task<UResponse<Guid?>> Create(MerchantCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		MerchantEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			UserId = p.UserId ?? userData.Id,
			ZipCode = p.ZipCode,
			Title = p.Title,
			CityCode = p.CityCode,
			PhoneNumber = p.PhoneNumber,
			Landline = p.Landline,
			NationalCode = p.NationalCode,
			BankAccountId = p.BankAccountId,
			Mcc = p.Mcc,
			JsonData = new MerchantJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				Address = p.Address,
				BusinessTitle = p.BusinessTitle,
				OwnerName = p.OwnerName,
				OwnerPhoneNumber = p.OwnerPhoneNumber
			}
		};
		await db.Set<MerchantEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse> Bind(MerchantBindParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		MerchantEntity? e = await db.Set<MerchantEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.MerchantId, ct);
		if (e == null) return new UResponse<MerchantResponse?>(null, Usc.NotFound, ls.Get("MerchantNotFound"));

		HttpResponseMessage? response = await http.Post(
			"https://oa.avreenco.com:8080/api/mms/ing/v2/addMerchant",
			new {
				accountId = e.BankAccountId,
				businessTitle = e.JsonData.BusinessTitle,
				cityCode = e.CityCode,
				mcc = e.Mcc,
				merchantAddress = e.JsonData.Address,
				merchantMobileNo = e.PhoneNumber,
				merchantName = e.Title,
				merchantOwnerName = e.JsonData.OwnerName,
				merchantPhone = e.Landline,
				nationalId = e.NationalCode,
				ownerMobileNo = e.JsonData.OwnerPhoneNumber,
				postalCode = e.ZipCode,
				definitionTemplate = 1,
				settlementCurrency = 364
			},
			new Dictionary<string, string> { { "Authorization", $"{Core.App.Avreen.AuthHeader}" }, { "Accept", "application/json" } }
		);

		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<Guid?>(null);
		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));

		e.InsId = data.GetStringOrNull("insId")!;
		e.MerchantId = data.GetStringOrNull("merchantId")!;
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<IEnumerable<MerchantResponse>?>> Read(MerchantReadParams p, CancellationToken ct) {
		IQueryable<MerchantEntity> q = db.Set<MerchantEntity>().ApplyReadParams<MerchantEntity, TagMerchant, MerchantJson>(p);

		if (p.UserId.IsNotNullOrEmpty()) q = q.Where(x => x.UserId == p.UserId);
		if (p.ZipCode.IsNotNullOrEmpty()) q = q.Where(x => x.ZipCode == p.ZipCode);
		if (p.BankAccountId.IsNotNullOrEmpty()) q = q.Where(x => x.BankAccountId == p.BankAccountId);
		if (p.NationalCode.IsNotNullOrEmpty()) q = q.Where(x => x.NationalCode == p.NationalCode);
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title == p.Title);
		if (p.CityCode.IsNotNullOrEmpty()) q = q.Where(x => x.CityCode == p.CityCode);
		if (p.InsId.IsNotNullOrEmpty()) q = q.Where(x => x.InsId == p.InsId);
		if (p.Landline.IsNotNullOrEmpty()) q = q.Where(x => x.Landline == p.Landline);
		if (p.PhoneNumber.IsNotNullOrEmpty()) q = q.Where(x => x.PhoneNumber == p.PhoneNumber);
		if (p.MerchantId.IsNotNullOrEmpty()) q = q.Where(x => x.MerchantId == p.MerchantId);
		if (p.Mcc.IsNotNullOrEmpty()) q = q.Where(x => x.Mcc == p.Mcc);

		IQueryable<MerchantResponse> projected = q.Select(Projections.MerchantSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<MerchantResponse?>> ReadById(IdParams<MerchantSelectorArgs> p, CancellationToken ct) {
		MerchantResponse? e = await db.Set<MerchantEntity>().Select(Projections.MerchantSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<MerchantResponse?>(null, Usc.NotFound, ls.Get("MerchantNotFound")) : new UResponse<MerchantResponse?>(e);
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<MerchantEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}
}