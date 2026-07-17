namespace SinaMN75U.Services;

public interface IMoadiService {
	Task<UResponse<Guid?>> Create(MoadiCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<MoadiResponse>?>> Read(MoadiReadParams p, CancellationToken ct);
	Task<UResponse<MoadiResponse?>> ReadById(IdParams<MoadiSelectorArgs> p, CancellationToken ct);
	Task<UResponse> Update(MoadiUpdateParams p, CancellationToken ct);
	Task<UResponse<MoadiResponse?>> Approve(IdParams p, CancellationToken ct);
	Task<UResponse> Reject(MoadiRejectParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class MoadiService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IHttpClientService httpClient
) : IMoadiService {
	public async Task<UResponse<Guid?>> Create(MoadiCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		if (await db.Set<MoadiEntity>().AnyAsync(x => x.EconomicCode == p.EconomicCode, ct))
			return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("MoadiEconomicCodeExists"));

		MoadiEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [TagMoadi.Pending],
			UserId = p.UserId ?? userData.Id,
			Name = p.Name,
			EconomicCode = p.EconomicCode,
			LegalEntity = p.LegalEntity,
			UniqueTaxCode = p.UniqueTaxCode,
			NationalCode = p.NationalCode,
			PostalCode = p.PostalCode,
			RegistrationDate = p.RegistrationDate,
			RegistrationNumber = p.RegistrationNumber,
			Address = p.Address,
			StartInvoiceNumber = p.StartInvoiceNumber,
			IntroductionCode = p.IntroductionCode,
			OwnerName = p.OwnerName,
			OwnerMobile = p.OwnerMobile,
			OwnerNationalCode = p.OwnerNationalCode,
			JsonData = new MoadiJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2
			}
		};
		await db.Set<MoadiEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<MoadiResponse>?>> Read(MoadiReadParams p, CancellationToken ct) {
		IQueryable<MoadiEntity> q = db.Set<MoadiEntity>().ApplyReadParams(p);

		if (p.UserId.IsNotNullOrEmpty()) q = q.Where(x => x.UserId == p.UserId);
		if (p.Name.IsNotNullOrEmpty()) q = q.Where(x => x.Name == p.Name);
		if (p.EconomicCode.IsNotNullOrEmpty()) q = q.Where(x => x.EconomicCode == p.EconomicCode);
		if (p.NationalCode.IsNotNullOrEmpty()) q = q.Where(x => x.NationalCode == p.NationalCode);
		if (p.UniqueTaxCode.IsNotNullOrEmpty()) q = q.Where(x => x.UniqueTaxCode == p.UniqueTaxCode);
		if (p.LegalEntity.IsNotNullOrEmpty()) q = q.Where(x => x.LegalEntity == p.LegalEntity);
		if (p.Uuid.IsNotNullOrEmpty()) q = q.Where(x => x.JsonData.Uuid == p.Uuid);

		IQueryable<MoadiResponse> projected = q.Select(Projections.MoadiSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<MoadiResponse?>> ReadById(IdParams<MoadiSelectorArgs> p, CancellationToken ct) {
		MoadiResponse? e = await db.Set<MoadiEntity>().Select(Projections.MoadiSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<MoadiResponse?>(null, Usc.NotFound, ls.Get("MoadiNotFound")) : new UResponse<MoadiResponse?>(e);
	}

	public async Task<UResponse> Update(MoadiUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		MoadiEntity? e = await db.Set<MoadiEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("MoadiNotFound"));
		if (!userData.CanManage(e.CreatorId, e.AdminUserIds)) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		if (p.Name.IsNotNullOrEmpty()) e.Name = p.Name;
		if (p.EconomicCode.IsNotNullOrEmpty()) e.EconomicCode = p.EconomicCode;
		if (p.LegalEntity.IsNotNullOrEmpty()) e.LegalEntity = p.LegalEntity;
		if (p.UniqueTaxCode.IsNotNullOrEmpty()) e.UniqueTaxCode = p.UniqueTaxCode;
		if (p.NationalCode.IsNotNull()) e.NationalCode = p.NationalCode;
		if (p.PostalCode.IsNotNull()) e.PostalCode = p.PostalCode;
		if (p.RegistrationDate.IsNotNull()) e.RegistrationDate = p.RegistrationDate;
		if (p.RegistrationNumber.IsNotNull()) e.RegistrationNumber = p.RegistrationNumber;
		if (p.Address.IsNotNull()) e.Address = p.Address;
		if (p.StartInvoiceNumber.IsNotNull()) e.StartInvoiceNumber = p.StartInvoiceNumber;
		if (p.IntroductionCode.IsNotNull()) e.IntroductionCode = p.IntroductionCode;
		if (p.OwnerName.IsNotNullOrEmpty()) e.OwnerName = p.OwnerName;
		if (p.OwnerMobile.IsNotNullOrEmpty()) e.OwnerMobile = p.OwnerMobile;
		if (p.OwnerNationalCode.IsNotNullOrEmpty()) e.OwnerNationalCode = p.OwnerNationalCode;
		if (p.Detail1.IsNotNull()) e.JsonData.Detail1 = p.Detail1!;
		if (p.Detail2.IsNotNull()) e.JsonData.Detail2 = p.Detail2!;

		if (p.Tags.IsNotNull()) e.Tags = p.Tags!;
		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags!);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveRangeIfExist(p.RemoveTags!);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<MoadiResponse?>> Approve(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<MoadiResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!userData.IsAdmin) return new UResponse<MoadiResponse?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		MoadiEntity? e = await db.Set<MoadiEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<MoadiResponse?>(null, Usc.NotFound, ls.Get("MoadiNotFound"));
		if (e.Tags.Contains(TagMoadi.Approved)) return new UResponse<MoadiResponse?>(null, Usc.Conflict, ls.Get("MoadiAlreadyApproved"));

		HttpResponseMessage? response = await httpClient.Get(BuildNamatUri(e), new Dictionary<string, string> {
			{ "Authorization", $"Bearer {Core.App.Namat.BranchToken}" },
			{ "Accept", "application/json" }
		});

		string body = response == null ? "" : await response.Content.ReadAsStringAsync(ct);
		if (response is null or { IsSuccessStatusCode: false }) {
			string message = ls.Get("MoadiRegistrationFailed");
			if (body.IsNotNullOrEmpty()) {
				JsonElement err = JsonSerializer.Deserialize<JsonElement>(body);
				message = err.GetStringOrNull("message") ?? message;
			}
			return new UResponse<MoadiResponse?>(null, Usc.ThirdPartyError, message);
		}

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(body);
		e.JsonData.Uuid = data.GetStringOrNull("uuid");
		e.JsonData.RegisterStep = data.GetIntOrNull("register_step");
		e.JsonData.CreatedType = data.GetStringOrNull("created_type");
		e.JsonData.OwnerId = data.GetIntOrNull("owner_id");
		e.JsonData.ActiveContract = data.GetBoolOrNull("activeContract") ?? false;
		e.JsonData.InvoicesCount = data.GetIntOrNull("invoices_count") ?? 0;
		e.JsonData.InvoicesSuccessCount = data.GetIntOrNull("invoices_success_count") ?? 0;
		if (data.TryGetProperty("lastContract", out JsonElement lastContract) && lastContract.ValueKind == JsonValueKind.Object)
			e.JsonData.LastContractStatus = lastContract.GetStringOrNull("status");
		e.JsonData.RejectReason = null;

		e.Tags = [TagMoadi.Approved];
		await db.SaveChangesAsync(ct);

		MoadiResponse result = await db.Set<MoadiEntity>().Select(Projections.MoadiSelector(new MoadiSelectorArgs())).FirstAsync(x => x.Id == e.Id, ct);
		return new UResponse<MoadiResponse?>(result);
	}

	public async Task<UResponse> Reject(MoadiRejectParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		MoadiEntity? e = await db.Set<MoadiEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("MoadiNotFound"));

		e.Tags = [TagMoadi.Rejected];
		e.JsonData.RejectReason = p.Reason;
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<MoadiEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	private static string BuildNamatUri(MoadiEntity e) {
		Dictionary<string, string?> q = new() {
			{ "owner[name]", e.OwnerName },
			{ "owner[mobile]", e.OwnerMobile },
			{ "owner[national_code]", e.OwnerNationalCode },
			{ "legal_entity", e.LegalEntity },
			{ "name", e.Name },
			{ "national_code", e.NationalCode },
			{ "economic_code", e.EconomicCode },
			{ "postal_code", e.PostalCode },
			{ "registration_date", e.RegistrationDate },
			{ "registration_number", e.RegistrationNumber },
			{ "address", e.Address },
			{ "ebill[unique_tax_code]", e.UniqueTaxCode },
			{ "start_invoice_number", e.StartInvoiceNumber?.ToString() },
			{ "introduction_code", e.IntroductionCode }
		};
		string query = string.Join("&", q.Where(x => x.Value.IsNotNullOrEmpty()).Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}"));
		return $"{Core.App.Namat.BaseUrl}api/branch/moadi?{query}";
	}
}
