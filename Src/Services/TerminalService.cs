using DocumentFormat.OpenXml.Spreadsheet;

namespace SinaMN75U.Services;

public interface ITerminalService {
	Task<UResponse<Guid?>> Create(TerminalCreateParams p, CancellationToken ct);
	Task<UResponse> BulkCreate(TerminalBulkCreateParams p, CancellationToken ct);
	Task<UResponse<TerminalImportResponse?>> Import(TerminalImportParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<TerminalResponse>?>> Read(TerminalReadParams p, CancellationToken ct);
	Task<UResponse> Update(TerminalUpdateParams p, CancellationToken ct);
	Task<UResponse<TerminalAvailabilityResponse?>> CheckAvailability(TerminalAssignParams p, CancellationToken ct);
	Task<UResponse<TerminalResponse?>> Assign(TerminalAssignParams p, CancellationToken ct);
	Task<UResponse<TerminalResponse?>> Approve(IdParams p, CancellationToken ct);
	Task<UResponse> Reject(TerminalRejectParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse<TerminalSupportPasswordResponse?>> ReadSupportPassword(IdParams p, CancellationToken ct);

	Task<UResponse<Guid?>> CreateBrand(TerminalBrandCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<TerminalBrandResponse>?>> ReadBrand(TerminalBrandReadParams p, CancellationToken ct);
	Task<UResponse> UpdateBrand(TerminalBrandUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteBrand(IdParams p, CancellationToken ct);

	Task<UResponse<Guid?>> CreateBroker(TerminalBrokerCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<TerminalBrokerResponse>?>> ReadBroker(TerminalBrokerReadParams p, CancellationToken ct);
	Task<UResponse> UpdateBroker(TerminalBrokerUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteBroker(IdParams p, CancellationToken ct);
}

public class TerminalService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IHttpClientService http,
	ISmsNotificationService sms,
	IWebHostEnvironment env
) : ITerminalService {
	public async Task<UResponse<Guid?>> Create(TerminalCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		TerminalEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			Serial = p.Serial,
			CreatedAt = DateTime.UtcNow,
			JsonData = new TerminalJson(),
			Tags = p.Tags,
			CreatorId = p.CreatorId ?? userData.Id,
			SimCardNumber = p.SimCardNumber,
			SimCardSerial = p.SimCardSerial,
			Imei = p.Imei,
			TerminalId = p.TerminalId,
			MerchantId = p.MerchantId,
			InsId = p.InsId,
			TerminalBrandId = p.TerminalBrandId,
			TerminalBrokerId = p.TerminalBrokerId
		};

		await db.Set<TerminalEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse> Update(TerminalUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		TerminalEntity? e = await db.Set<TerminalEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("terminalNotFound"));

		if (p.Serial.IsNotNullOrEmpty()) e.Serial = p.Serial;
		if (p.Imei.IsNotNullOrEmpty()) e.Imei = p.Imei;
		if (p.InsId.IsNotNullOrEmpty()) e.InsId = p.InsId;
		if (p.SimCardNumber.IsNotNullOrEmpty()) e.SimCardNumber = p.SimCardNumber;
		if (p.SimCardSerial.IsNotNullOrEmpty()) e.SimCardSerial = p.SimCardSerial;
		if (p.TerminalId.IsNotNullOrEmpty()) e.TerminalId = p.TerminalId;
		if (p.MerchantId.IsNotNullOrEmpty()) e.MerchantId = p.MerchantId;

		if (p.TerminalBrandId.IsNotNullOrEmpty()) {
			if (!await db.Set<TerminalBrandEntity>().AnyAsync(x => x.Id == p.TerminalBrandId, ct)) return new UResponse(Usc.NotFound, ls.Get("terminalBrandNotFound"));
			e.TerminalBrandId = p.TerminalBrandId;
		}

		if (p.TerminalBrokerId.IsNotNullOrEmpty()) {
			if (!await db.Set<TerminalBrokerEntity>().AnyAsync(x => x.Id == p.TerminalBrokerId, ct)) return new UResponse(Usc.NotFound, ls.Get("terminalBrokerNotFound"));
			e.TerminalBrokerId = p.TerminalBrokerId;
		}

		e.ApplyUpdateParam<TerminalEntity, TagTerminal, TerminalJson>(p);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<TerminalAvailabilityResponse?>> CheckAvailability(TerminalAssignParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalAvailabilityResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<TerminalAvailabilityResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		(TerminalEntity? terminal, MerchantEntity? merchant, TerminalBrandEntity? brand, TerminalBrokerEntity? broker, Usc status, string message) = await ResolveAssignable(userData, p, ct);
		if (terminal == null || merchant == null || brand == null || broker == null) return new UResponse<TerminalAvailabilityResponse?>(null, status, message);

		string? pdf = await GenerateAgreement(merchant.User, merchant, terminal, brand, broker);
		if (pdf == null) return new UResponse<TerminalAvailabilityResponse?>(null, Usc.InternalServerError, ls.Get("generatingTheAgreementFailed"));

		return new UResponse<TerminalAvailabilityResponse?>(new TerminalAvailabilityResponse {
			Id = terminal.Id,
			Serial = terminal.Serial,
			Agreement = pdf
		});
	}

	public async Task<UResponse<TerminalResponse?>> Assign(TerminalAssignParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<TerminalResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!p.AcceptedAgreement) return new UResponse<TerminalResponse?>(null, Usc.BadRequest, ls.Get("youHaveToAcceptTheAgreementToContinue"));

		(TerminalEntity? terminal, MerchantEntity? merchant, TerminalBrandEntity? brand, TerminalBrokerEntity? broker, Usc status, string message) =
			await ResolveAssignable(userData, p, ct);
		if (terminal == null || merchant == null || brand == null || broker == null) return new UResponse<TerminalResponse?>(null, status, message);

		string? agreement = await GenerateAgreement(merchant.User, merchant, terminal, brand, broker);
		if (agreement == null) return new UResponse<TerminalResponse?>(null, Usc.InternalServerError, ls.Get("generatingTheAgreementFailed"));

		terminal.JsonData.Detail1 = p.Title ?? "";
		terminal.JsonData.Detail2 = "";
		terminal.MerchantId = merchant.Id;
		// terminal.Agreement = agreement;
		terminal.AgreementHtml = agreement;
		SetStatus(terminal, TagTerminal.PendingApproval);

		await db.SaveChangesAsync(ct);

		return new UResponse<TerminalResponse?>(new TerminalResponse {
			Id = terminal.Id,
			CreatedAt = terminal.CreatedAt,
			JsonData = terminal.JsonData,
			Tags = terminal.Tags,
			CreatorId = terminal.CreatorId,
			Serial = terminal.Serial,
			SimCardNumber = terminal.SimCardNumber,
			SimCardSerial = terminal.SimCardSerial,
			Imei = terminal.Imei,
			TerminalId = terminal.TerminalId,
			Agreement = terminal.Agreement.ToBase64(),
			MerchantId = terminal.MerchantId,
			TerminalBrandId = terminal.TerminalBrandId,
			TerminalBrokerId = terminal.TerminalBrokerId,
		}, Usc.Success, ls.Get("yourRequestHasBeenSubmittedAndIsAwaitingApproval"));
	}

	public async Task<UResponse<TerminalResponse?>> Approve(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<TerminalResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<TerminalResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		TerminalEntity? terminal = await db.Set<TerminalEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (terminal == null) return new UResponse<TerminalResponse?>(null, Usc.NotFound, ls.Get("terminalNotFound"));
		if (terminal.Tags.Contains(TagTerminal.Approved)) return new UResponse<TerminalResponse?>(null, Usc.Conflict, ls.Get("thisTerminalRequestIsAlreadyApproved"));
		if (!terminal.Tags.Contains(TagTerminal.PendingApproval)) return new UResponse<TerminalResponse?>(null, Usc.BadRequest, ls.Get("thisTerminalRequestIsNotWaitingForApproval"));

		MerchantEntity? merchant = await db.Set<MerchantEntity>().AsTracking().Include(x => x.User).FirstOrDefaultAsync(x => x.Id == terminal.MerchantId, ct);
		if (merchant == null) return new UResponse<TerminalResponse?>(null, Usc.NotFound, ls.Get("merchantNotFound"));

		if (merchant.MerchantId.IsNullOrEmpty()) {
			HttpResponseMessage? response = await http.Post(
				$"{Core.App.Avreen.BaseUrl}api/mms/ing/v2/addMerchant",
				new {
					accountId = merchant.BankAccountId,
					businessTitle = merchant.JsonData.BusinessTitle,
					cityCode = merchant.CityCode,
					mcc = merchant.Mcc,
					merchantAddress = merchant.JsonData.Address,
					merchantMobileNo = merchant.PhoneNumber,
					merchantName = merchant.Title,
					merchantOwnerName = merchant.JsonData.OwnerName,
					merchantPhone = merchant.Landline,
					nationalId = merchant.NationalCode,
					ownerMobileNo = merchant.JsonData.OwnerPhoneNumber,
					postalCode = merchant.ZipCode,
					definitionTemplate = 1,
					settlementCurrency = 364
				},
				new Dictionary<string, string> { { "Authorization", $"{Core.App.Avreen.AuthHeader}" }, { "Accept", "application/json" } }
			);

			if (response is null or { IsSuccessStatusCode: false }) return new UResponse<TerminalResponse?>(null, Usc.ThirdPartyError, ls.Get("failedToRegisterMerchantInAvreen"));
			JsonElement merchantData = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
			if (merchantData.GetStringOrNull("insId") == null)
				return new UResponse<TerminalResponse?>(null, Usc.ThirdPartyError, ls.Get("merchantRegistrationSucceededButMerchantIdentifierWasNotReturnedByAvreen"));

			merchant.InsId = merchantData.GetStringOrNull("insId")!;
			merchant.MerchantId = merchantData.GetStringOrNull("merchantId")!;
		}

		HttpResponseMessage? terminalResponse = await http.Post(
			$"{Core.App.Avreen.BaseUrl}api/mms/ing/v2/defineAndBindTerminal",
			new {
				definitionTemplate = 1,
				merchantId = merchant.MerchantId,
				project = "AvaPlus",
				terminalSerial = terminal.Serial,
				terminalSerial2 = terminal.SimCardSerial
			},
			new Dictionary<string, string> { { "Authorization", $"{Core.App.Avreen.AuthHeader}" }, { "Accept", "application/json" } }
		);

		if (terminalResponse is null or { IsSuccessStatusCode: false }) return new UResponse<TerminalResponse?>(null, Usc.ThirdPartyError, ls.Get("failedToBindTerminalToMerchantInAvreen"));
		JsonElement terminalData = JsonSerializer.Deserialize<JsonElement>(await terminalResponse.Content.ReadAsStringAsync(ct));
		if (terminalData.GetStringOrNull("insId") == null) return new UResponse<TerminalResponse?>(null, Usc.ThirdPartyError, ls.Get("failedToBindTerminalToMerchantInAvreen"));

		terminal.TerminalId = terminalData.GetStringOrNull("terminalId");
		terminal.InsId = terminalData.GetStringOrNull("insId");
		SetStatus(terminal, TagTerminal.Approved);

		await db.SaveChangesAsync(ct);

		return new UResponse<TerminalResponse?>(new TerminalResponse {
			Id = terminal.Id,
			CreatedAt = terminal.CreatedAt,
			JsonData = terminal.JsonData,
			Tags = terminal.Tags,
			CreatorId = terminal.CreatorId,
			Serial = terminal.Serial,
			SimCardNumber = terminal.SimCardNumber,
			SimCardSerial = terminal.SimCardSerial,
			Imei = terminal.Imei,
			TerminalId = terminal.TerminalId,
			Agreement = terminal.Agreement.ToBase64(),
			MerchantId = terminal.MerchantId
		});
	}

	public async Task<UResponse> Reject(TerminalRejectParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		TerminalEntity? terminal = await db.Set<TerminalEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (terminal == null) return new UResponse(Usc.NotFound, ls.Get("terminalNotFound"));
		if (terminal.Tags.Contains(TagTerminal.Approved)) return new UResponse(Usc.Conflict, ls.Get("thisTerminalRequestIsAlreadyApproved"));

		terminal.JsonData.Detail2 = p.Reason ?? "";
		SetStatus(terminal, TagTerminal.Rejected);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	private async Task<(TerminalEntity? Terminal, MerchantEntity? Merchant, TerminalBrandEntity? Brand, TerminalBrokerEntity? Broker, Usc Status, string Message)> ResolveAssignable(
		JwtClaimData userData,
		TerminalAssignParams p,
		CancellationToken ct
	) {
		if (p.TerminalBrandId == null) return (null, null, null, null, Usc.BadRequest, ls.Get("terminalBrandIsRequired"));
		if (p.TerminalBrokerId == null) return (null, null, null, null, Usc.BadRequest, ls.Get("terminalBrokerIsRequired"));

		TerminalEntity? terminal = await db.Set<TerminalEntity>().AsTracking()
			.Include(x => x.TerminalBrand)
			.Include(x => x.TerminalBroker)
			.FirstOrDefaultAsync(x => x.Serial == p.Serial && x.TerminalBrandId == p.TerminalBrandId && x.TerminalBrokerId == p.TerminalBrokerId, ct);

		if (terminal?.TerminalBrand == null || terminal.TerminalBroker == null || terminal.TerminalBrand.Tags.Contains(TagTerminalBrand.SimCard) && terminal.SimCardSerial != p.SimCardSerial) return (null, null, null, null, Usc.NotFound, ls.Get("terminalNotFoundCheckYourDetails"));

		MerchantEntity? merchant = await db.Set<MerchantEntity>().AsTracking().Include(x => x.User).FirstOrDefaultAsync(x => x.Id == p.MerchantId, ct);
		if (merchant == null) return (null, null, null, null, Usc.NotFound, ls.Get("merchantNotFound"));
		if (!userData.IsAdmin && merchant.UserId != userData.Id) return (null, null, null, null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (terminal.Tags.Contains(TagTerminal.Approved)) return (null, null, null, null, Usc.Conflict, ls.Get("terminalIsAlreadyAssignedToAMerchant"));
		if (terminal.Tags.Contains(TagTerminal.PendingApproval)) return (null, null, null, null, Usc.Conflict, ls.Get("thisTerminalRequestIsWaitingForApproval"));
		if (terminal.MerchantId.IsNotNullOrEmpty() && terminal.MerchantId != merchant.Id) return (null, null, null, null, Usc.Conflict, ls.Get("terminalIsAlreadyAssignedToAMerchant"));

		return (terminal, merchant, terminal.TerminalBrand, terminal.TerminalBroker, Usc.Success, "");
	}

	private static void SetStatus(TerminalEntity terminal, TagTerminal status) =>
		terminal.Tags = terminal.Tags.Where(x => x is not (TagTerminal.PendingApproval or TagTerminal.Approved or TagTerminal.Rejected)).Append(status).ToList();

	public async Task<UResponse> BulkCreate(TerminalBulkCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		List<TerminalEntity> entities = [];

		entities.AddRange(p.List.Select(x => new TerminalEntity {
			Serial = x.Serial,
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new TerminalJson(),
			Tags = x.Tags,
			CreatorId = userData.Id,
			TerminalBrandId = x.TerminalBrandId,
			TerminalBrokerId = x.TerminalBrokerId
		}));

		await db.Set<TerminalEntity>().AddRangeAsync(entities, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse<IEnumerable<TerminalResponse>?>> Read(TerminalReadParams p, CancellationToken ct) {
		IQueryable<TerminalEntity> q = db.Set<TerminalEntity>().ApplyReadParams(p);

		if (p.Serial.IsNotNullOrEmpty()) q = q.Where(x => x.Serial == p.Serial);
		if (p.TerminalId.IsNotNullOrEmpty()) q = q.Where(x => x.TerminalId == p.TerminalId);
		if (p.MerchantId.IsNotNullOrEmpty()) q = q.Where(x => x.MerchantId == p.MerchantId);
		if (p.Imei.IsNotNullOrEmpty()) q = q.Where(x => x.Imei == p.Imei);
		if (p.SimCardNumber.IsNotNullOrEmpty()) q = q.Where(x => x.SimCardNumber == p.SimCardNumber);
		if (p.SimCardSerial.IsNotNullOrEmpty()) q = q.Where(x => x.SimCardSerial == p.SimCardSerial);

		IQueryable<TerminalResponse> projected = q.Select(Projections.TerminalSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		await db.Set<TerminalEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse<TerminalSupportPasswordResponse?>> ReadSupportPassword(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		TerminalEntity? e = await db.Set<TerminalEntity>().Select(x => new TerminalEntity {
			Serial = x.Serial,
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			JsonData = x.JsonData,
			Tags = x.Tags,
			CreatorId = x.CreatorId,
			InsId = x.InsId,
			TerminalId = x.TerminalId,
			TerminalBrandId = x.TerminalBrandId,
			TerminalBrokerId = x.TerminalBrokerId,
			Merchant = new MerchantEntity {
				ZipCode = "",
				CityCode = "",
				PhoneNumber = "",
				Title = "",
				Landline = "",
				NationalCode = "",
				Mcc = "",
				UserId = x.Merchant!.UserId,
				Id = x.Merchant.Id,
				CreatedAt = x.Merchant.CreatedAt,
				JsonData = x.Merchant.JsonData,
				Tags = x.Merchant.Tags,
				CreatorId = x.Merchant.CreatorId,
				InsId = x.InsId,
				MerchantId = x.Merchant.MerchantId
			}
		}).FirstOrDefaultAsync(x => x.Id == p.Id, ct);

		if (e == null) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.NotFound, ls.Get("terminalNotFound"));
		if (e.Merchant == null) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.NotFound, ls.Get("merchantNotFound"));
		if (!userData.IsAdmin && userData.Id != e.CreatorId && userData.Id != e.Merchant?.UserId) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		HttpResponseMessage? response = await http.Post(
			$"{Core.App.Avreen.BaseUrl}api/mms/ing/v2/generateSupportPassword",
			new {
				insId = e.InsId,
				merchantId = e.Merchant!.MerchantId,
				terminalId = e.TerminalId,
				terminalSerial = e.Serial,
				terminalSerial2 = e.SimCardSerial
			},
			new Dictionary<string, string> { { "Authorization", $"{Core.App.Avreen.AuthHeader}" }, { "Accept", "application/json" } }
		);

		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<TerminalSupportPasswordResponse?>(null);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));

		await sms.SendSms(new SmsNotificationParams {
			Mobile = userData.PhoneNumber!,
			Template = Core.App.SmsPanel.SupportPasswordOtp,
			Text = "12345"
		});

		return new UResponse<TerminalSupportPasswordResponse?>(new TerminalSupportPasswordResponse { Password = data.GetStringOrNull("supportPassword") });
	}

	public async Task<UResponse<TerminalImportResponse?>> Import(TerminalImportParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalImportResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<TerminalImportResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (p.File.IsNullOrEmpty()) return new UResponse<TerminalImportResponse?>(null, Usc.BadRequest, ls.Get("FileRequired"));

		List<Dictionary<string, string>> rows;
		try {
			byte[] bytes = Convert.FromBase64String(StripDataUri(p.File));
			using MemoryStream ms = new(bytes);
			rows = ParseSheet(ms);
		}
		catch {
			return new UResponse<TerminalImportResponse?>(null, Usc.BadRequest, ls.Get("InvalidFileFormat"));
		}

		HashSet<string> serials = (await db.Set<TerminalEntity>().Select(x => x.Serial).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
		HashSet<string> imeis = (await db.Set<TerminalEntity>().Where(x => x.Imei != null).Select(x => x.Imei!).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
		HashSet<string> simSerials = (await db.Set<TerminalEntity>().Where(x => x.SimCardSerial != null).Select(x => x.SimCardSerial!).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
		HashSet<string> terminalIds = (await db.Set<TerminalEntity>().Where(x => x.TerminalId != null).Select(x => x.TerminalId!).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);

		HashSet<Guid> brands = (await db.Set<TerminalBrandEntity>().Select(x => x.Id).ToListAsync(ct)).ToHashSet();
		HashSet<Guid> brokers = (await db.Set<TerminalBrokerEntity>().Select(x => x.Id).ToListAsync(ct)).ToHashSet();

		TerminalImportResponse result = new() { TotalRows = rows.Count };
		List<TerminalEntity> toAdd = [];

		foreach (Dictionary<string, string> row in rows) {
			string serial = Val(row, "Serial");
			if (serial.IsNullOrEmpty()) {
				result.SkippedSerials.Add("(empty serial)");
				continue;
			}

			string? imei = Val(row, "Imei").NullIfEmpty();
			string? simSerial = Val(row, "SimCardSerial").NullIfEmpty();
			string? terminalId = Val(row, "TerminalId").NullIfEmpty();
			string? brandId = Val(row, "BrandId").NullIfEmpty();
			string? brokerId = Val(row, "BrokerId").NullIfEmpty();

			if (!Guid.TryParse(brandId, out Guid brand) || !brands.Contains(brand)) {
				result.SkippedSerials.Add($"{serial} (missing/invalid BrandId)");
				continue;
			}

			if (!Guid.TryParse(brokerId, out Guid broker) || !brokers.Contains(broker)) {
				result.SkippedSerials.Add($"{serial} (missing/invalid BrokerId)");
				continue;
			}

			if (serials.Contains(serial) ||
			    (imei != null && imeis.Contains(imei)) ||
			    (simSerial != null && simSerials.Contains(simSerial)) ||
			    (terminalId != null && terminalIds.Contains(terminalId))) {
				result.SkippedSerials.Add($"{serial} (duplicate)");
				continue;
			}

			// The device type lives on the brand now, so an imported row only carries its status.
			List<TagTerminal> tags = TryParseTag(Val(row, "Tag1"), out TagTerminal tag1) ? [tag1] : [TagTerminal.NoAssigned];
			if (TryParseTag(Val(row, "Tag2"), out TagTerminal tag2) && !tags.Contains(tag2)) tags.Add(tag2);

			toAdd.Add(new TerminalEntity {
				Id = Guid.CreateVersion7(),
				Serial = serial,
				CreatedAt = DateTime.UtcNow,
				JsonData = new TerminalJson(),
				Tags = tags,
				CreatorId = userData.Id,
				SimCardNumber = Val(row, "SimCardNumber").NullIfEmpty(),
				SimCardSerial = simSerial,
				Imei = imei,
				TerminalId = terminalId,
				InsId = Val(row, "InsId").NullIfEmpty(),
				TerminalBrandId = brand,
				TerminalBrokerId = broker
			});

			serials.Add(serial);
			if (imei != null) imeis.Add(imei);
			if (simSerial != null) simSerials.Add(simSerial);
			if (terminalId != null) terminalIds.Add(terminalId);
		}

		if (toAdd.Count > 0) {
			await db.Set<TerminalEntity>().AddRangeAsync(toAdd, ct);
			await db.SaveChangesAsync(ct);
		}

		result.Imported = toAdd.Count;
		result.Skipped = result.TotalRows - result.Imported;
		return new UResponse<TerminalImportResponse?>(result, Usc.Success, ls.Get("ImportCompleted"));
	}

	public async Task<UResponse<Guid?>> CreateBrand(TerminalBrandCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		TerminalBrandEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new TerminalBrandJson(),
			Tags = p.Tags,
			CreatorId = p.CreatorId ?? userData.Id,
			Title = p.Title,
			Model = p.Model
		};

		await db.Set<TerminalBrandEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<TerminalBrandResponse>?>> ReadBrand(TerminalBrandReadParams p, CancellationToken ct) {
		IQueryable<TerminalBrandEntity> q = db.Set<TerminalBrandEntity>().ApplyReadParams(p);

		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title == p.Title);

		IQueryable<TerminalBrandResponse> projected = q.Select(Projections.TerminalBrandSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateBrand(TerminalBrandUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		TerminalBrandEntity? e = await db.Set<TerminalBrandEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("terminalNotFound"));

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;

		e.ApplyUpdateParam<TerminalBrandEntity, TagTerminalBrand, TerminalBrandJson>(p);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteBrand(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		await db.Set<TerminalBrandEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateBroker(TerminalBrokerCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		TerminalBrokerEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new TerminalBrokerJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				RegistrationNumber = p.RegistrationNumber,
				NationalCode = p.NationalCode,
				Representative = p.Representative,
				Address = p.Address,
				PostalCode = p.PostalCode,
				PhoneNumber = p.PhoneNumber,
				Sign1Base64 = p.Sign1Base64,
				Sign1Owner = p.Sign1Owner,
				Sign2Base64 = p.Sign2Base64,
				Sign2Owner = p.Sign2Owner,
				LogoBase64 = p.LogoBase64
			},
			Tags = p.Tags,
			CreatorId = p.CreatorId ?? userData.Id,
			Title = p.Title,
		};

		await db.Set<TerminalBrokerEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<TerminalBrokerResponse>?>> ReadBroker(TerminalBrokerReadParams p, CancellationToken ct) {
		IQueryable<TerminalBrokerEntity> q = db.Set<TerminalBrokerEntity>().ApplyReadParams(p);

		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title == p.Title);

		IQueryable<TerminalBrokerResponse> projected = q.Select(Projections.TerminalBrokerSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateBroker(TerminalBrokerUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		TerminalBrokerEntity? e = await db.Set<TerminalBrokerEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("terminalNotFound"));

		if (p.RegistrationNumber.IsNotNullOrEmpty()) e.JsonData.RegistrationNumber = p.RegistrationNumber;
		if (p.NationalCode.IsNotNullOrEmpty()) e.JsonData.NationalCode = p.NationalCode;
		if (p.Representative.IsNotNullOrEmpty()) e.JsonData.Representative = p.Representative;
		if (p.Address.IsNotNullOrEmpty()) e.JsonData.Address = p.Address;
		if (p.PostalCode.IsNotNullOrEmpty()) e.JsonData.PostalCode = p.PostalCode;
		if (p.PhoneNumber.IsNotNullOrEmpty()) e.JsonData.PhoneNumber = p.PhoneNumber;
		if (p.Sign1Base64.IsNotNullOrEmpty()) e.JsonData.Sign1Base64 = p.Sign1Base64;
		if (p.Sign1Owner.IsNotNullOrEmpty()) e.JsonData.Sign1Owner = p.Sign1Owner;
		if (p.Sign2Base64.IsNotNullOrEmpty()) e.JsonData.Sign2Base64 = p.Sign2Base64;
		if (p.Sign2Owner.IsNotNullOrEmpty()) e.JsonData.Sign2Owner = p.Sign2Owner;
		if (p.LogoBase64.IsNotNullOrEmpty()) e.JsonData.LogoBase64 = p.LogoBase64;

		e.ApplyUpdateParam<TerminalBrokerEntity, TagTerminalBroker, TerminalBrokerJson>(p);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteBroker(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		await db.Set<TerminalBrokerEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	private static List<Dictionary<string, string>> ParseSheet(Stream stream) {
		List<Dictionary<string, string>> rows = [];
		using SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, false);
		WorkbookPart wb = doc.WorkbookPart!;
		WorksheetPart wsPart = wb.WorksheetParts.First();
		SharedStringTablePart? sst = wb.SharedStringTablePart;

		Row[] sheetRows = wsPart.Worksheet!.GetFirstChild<SheetData>()!.Elements<Row>().ToArray();
		if (sheetRows.Length == 0) return rows;

		Dictionary<string, string> headers = new(StringComparer.OrdinalIgnoreCase);
		foreach (Cell c in sheetRows[0].Elements<Cell>()) {
			string text = CellText(c, sst).Trim();
			if (text.Length > 0) headers[ColumnLetter(c.CellReference!.Value!)] = text;
		}

		foreach (Row r in sheetRows.Skip(1)) {
			Dictionary<string, string> map = new(StringComparer.OrdinalIgnoreCase);
			foreach (Cell c in r.Elements<Cell>()) {
				string col = ColumnLetter(c.CellReference!.Value!);
				if (headers.TryGetValue(col, out string? header)) map[header] = CellText(c, sst).Trim();
			}

			if (map.Values.Any(v => v.Length > 0)) rows.Add(map);
		}

		return rows;
	}

	private static string CellText(Cell cell, SharedStringTablePart? sst) {
		string raw = cell.CellValue?.InnerText ?? "";
		if (cell.DataType?.Value == CellValues.SharedString && sst != null && int.TryParse(raw, out int idx))
			return sst.SharedStringTable!.Elements<SharedStringItem>().ElementAt(idx).InnerText;
		return cell.DataType?.Value == CellValues.InlineString ? cell.InnerText : raw;
	}

	private static string ColumnLetter(string cellRef) {
		int i = 0;
		while (i < cellRef.Length && char.IsLetter(cellRef[i])) i++;
		return cellRef[..i];
	}

	private static string StripDataUri(string s) {
		int i = s.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
		return i >= 0 ? s[(i + 7)..].Trim() : s.Trim();
	}

	private static string Val(Dictionary<string, string> row, string key) => row.GetValueOrDefault(key, "");

	private static bool TryParseTag(string raw, out TagTerminal tag) {
		tag = default;
		if (!int.TryParse(raw.Trim(), out int n) || !Enum.IsDefined(typeof(TagTerminal), n)) return false;
		tag = (TagTerminal)n;
		return true;
	}

	private async Task<string?> GenerateAgreement(
		UserEntity user,
		MerchantEntity merchant,
		TerminalEntity terminal,
		TerminalBrandEntity brand,
		TerminalBrokerEntity broker
	) {
		try {
			string htmlPath = Path.Combine(AppContext.BaseDirectory, "Templates", brand.Tags.Contains(TagTerminalBrand.Atm) ? "atmAgreemenlToPrint.html" : "atmAgreement.html");
			if (!File.Exists(htmlPath)) {
				ULog.Error($"Agreement template not found at {htmlPath}");
				return null;
			}

			HtmlTemplate template = await HtmlTemplate.FromFile(htmlPath);
			template.RemoveUnmatchedTokens = true;

			template
				// ---- Header ----
				.Set("broker_day", PersianDateTime.Now.Day.ToString())
				.Set("broker_month", PersianDateTime.Now.Month.ToString())
				.SetLtr("broker_contract_number", terminal.Serial)
				.SetImageBase64("broker_logo", broker.JsonData.LogoBase64, attributes: "alt=\"\"")

				// Broker side
				.Set("broker_company_name", broker.Title)
				.SetLtr("broker_registration_number", broker.JsonData.RegistrationNumber ?? "---")
				.SetLtr("broker_national_id", broker.JsonData.NationalCode ?? "---")
				.Set("broker_representative_name", broker.JsonData.Representative ?? "---")
				.Set("broker_representative_title", broker.JsonData.Representative ?? "---")
				.Set("broker_address", broker.JsonData.Address ?? "---")
				.SetLtr("broker_postal_code", broker.JsonData.PostalCode ?? "---")
				.SetLtr("broker_phone", broker.JsonData.PhoneNumber ?? "---")
				.SetLtr("broker_support_phone", broker.JsonData.PhoneNumber ?? "---")

				// User / acceptor side
				.Set("user_full_name", $"{user.FirstName ?? "---"} {user.LastName ?? "---"}")
				.Set("user_father_name", user.JsonData.FatherName ?? "---")
				.Set("user_id_number", user.NationalCode ?? "---")
				.SetLtr("user_national_code", user.NationalCode ?? "---")
				.SetLtr("user_birthdate", PersianDateTime.FromDateTime(user.Birthdate ?? DateTime.Now).ToString("yyyy-MM-dd"))
				.Set("user_address", merchant.JsonData.Address ?? "---")
				.SetLtr("user_postal_code", merchant.ZipCode)
				.SetLtr("user_mobile", user.PhoneNumber ?? "---")
				.SetLtr("user_landline", user.LandLine ?? "---")

				// Property plaque
				.Set("user_plaque_number", "---")
				.Set("user_main_plaque", "---")

				// signature box
				.Set("broker_name1", broker.JsonData.Sign1Owner ?? "---")
				.Set("broker_name2", broker.JsonData.Sign1Owner ?? "---")
				.SetImageBase64("broker_signatures1", broker.JsonData.Sign1Base64)
				.SetImageBase64("broker_signatures2", broker.JsonData.Sign1Base64)
				.SetImageBytes("user_signature", ReadSignature(user));

			if (brand.Tags.Contains(TagTerminalBrand.Atm)) {
				template
					.Clear("broker_signatures1")
					.Clear("broker_signatures2")
					.Clear("user_signature")
					.Set("printInstruction", ls.Get("printTheAgreementSignItAndSendItByPost"));
			}
			else {
				template.SetImageBase64("broker_signatures", broker.JsonData.Sign1Base64);
				template.SetImageBytes("user_signature", ReadSignature(user));
			}


			// template
			// 	.Set("day", PersianDateTime.Now.Day.ToString())
			// 	.Set("month", PersianDateTime.Now.Month.ToString())
			// 	.SetLtr("number", terminal.Serial)
			// 	.Set("fullName", $"{user.FirstName ?? "---"} {user.LastName ?? "---"}")
			// 	.Set("address", merchant.JsonData.Address ?? "---")
			// 	.Set("fatherName", user.JsonData.FatherName ?? "---")
			// 	.SetLtr("nationalCode", user.NationalCode ?? "---")
			// 	.SetLtr("birthdate", PersianDateTime.FromDateTime(user.Birthdate ?? DateTime.Now).ToString("yyyy-MM-dd"))
			// 	.SetLtr("postalCode", merchant.ZipCode)
			// 	.SetLtr("phoneNumber", user.PhoneNumber ?? "---")
			// 	.SetLtr("landLine", user.LandLine ?? "---")
			// 	.Set("brokerTitle", broker.Title)
			// 	.Set("brokerRepresentative", broker.JsonData.Representative ?? "---")
			// 	.Set("brokerAddress", broker.JsonData.Address ?? "---")
			// 	.Set("brokerSign1Owner", broker.JsonData.Sign1Owner ?? "")
			// 	.Set("brokerSign2Owner", broker.JsonData.Sign2Owner ?? "")
			// 	.SetLtr("brokerRegistrationNumber", broker.JsonData.RegistrationNumber ?? "---")
			// 	.SetLtr("brokerNationalCode", broker.JsonData.NationalCode ?? "---")
			// 	.SetLtr("brokerPostalCode", broker.JsonData.PostalCode ?? "---")
			// 	.SetLtr("brokerPhoneNumber", broker.JsonData.PhoneNumber ?? "---")
			// 	.SetImageBase64("brokerLogo", broker.JsonData.LogoBase64, attributes: "alt=\"\"");
			//
			// if (brand.Tags.Contains(TagTerminalBrand.Atm)) {
			// 	template
			// 		.Clear("brokerSign1")
			// 		.Clear("brokerSign2")
			// 		.Clear("customerSignature")
			// 		.Set("printInstruction", ls.Get("printTheAgreementSignItAndSendItByPost"));
			// }
			// else {
			// 	template.SetImageBase64("brokerSign1", broker.JsonData.Sign1Base64);
			// 	template.SetImageBase64("brokerSign2", broker.JsonData.Sign2Base64);
			// 	template.SetImageBytes("customerSignature", ReadSignature(user));
			// }

			return template.Render();
		}
		catch (Exception ex) {
			ULog.Error(ex, $"Generating the agreement failed for terminal {terminal.Id}");
			return null;
		}
	}

	private byte[]? ReadSignature(UserEntity user) {
		if (user.ESignature.IsNullOrEmpty()) return null;
		string path = Path.Combine(env.WebRootPath, "Media", user.ESignature.TrimStart('/', '\\'));
		return File.Exists(path) ? File.ReadAllBytes(path) : null;
	}
}