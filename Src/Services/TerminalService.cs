namespace SinaMN75U.Services;

public interface ITerminalService {
	Task<UResponse<Guid?>> Create(TerminalCreateParams p, CancellationToken ct);
	Task<UResponse> BulkCreate(TerminalBulkCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<TerminalResponse>?>> Read(TerminalReadParams p, CancellationToken ct);
	Task<UResponse> Update(TerminalUpdateParams p, CancellationToken ct);
	Task<UResponse<TerminalResponse?>> Assign(TerminalAssignParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse<TerminalSupportPasswordResponse?>> ReadSupportPassword(IdParams p, CancellationToken ct);
}

public class TerminalService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IHttpClientService http,
	ISmsNotificationService sms
) : ITerminalService {
	public async Task<UResponse<Guid?>> Create(TerminalCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		TerminalEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			Serial = p.Serial,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson(),
			Tags = p.Tags,
			CreatorId = p.CreatorId ?? userData.Id,
			SimCardNumber = p.SimCardNumber,
			SimCardSerial = p.SimCardSerial,
			Imei = p.Imei,
			TerminalId = p.TerminalId,
			MerchantId = p.MerchantId,
			InsId = p.InsId
		};

		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse> Update(TerminalUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!userData.IsAdmin) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		TerminalEntity? e = await db.Set<TerminalEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("TerminalNotFound"));

		if (p.Serial.IsNotNullOrEmpty()) e.Serial = p.Serial;
		if (p.Imei.IsNotNullOrEmpty()) e.Imei = p.Imei;
		if (p.InsId.IsNotNullOrEmpty()) e.InsId = p.InsId;
		if (p.SimCardNumber.IsNotNullOrEmpty()) e.SimCardNumber = p.SimCardNumber;
		if (p.SimCardSerial.IsNotNullOrEmpty()) e.SimCardSerial = p.SimCardSerial;
		if (p.TerminalId.IsNotNullOrEmpty()) e.TerminalId = p.TerminalId;
		if (p.MerchantId.IsNotNullOrEmpty()) e.MerchantId = p.MerchantId;

		e.ApplyUpdateParam<TerminalEntity, TagTerminal, BaseJson>(p);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<TerminalResponse?>> Assign(TerminalAssignParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		TerminalEntity? terminal = await db.Set<TerminalEntity>().AsTracking().FirstOrDefaultAsync(x => x.Serial == p.Serial && x.SimCardSerial == p.SimCardSerial, ct);
		if (terminal == null) return new UResponse<TerminalResponse?>(null, Usc.NotFound, ls.Get("TerminalNotFoundCheckDetails"));
		MerchantEntity? merchant = await db.Set<MerchantEntity>().AsTracking().Include(x => x.User).FirstOrDefaultAsync(x => x.Id == p.MerchantId, ct);
		if (merchant == null) return new UResponse<TerminalResponse?>(null, Usc.NotFound, ls.Get("MerchantNotFound"));
		
		string agreement = await GenerateAgreement(merchant.User, terminal);

		terminal.JsonData.Detail1 = p.Title ?? "";
		terminal.MerchantId = p.MerchantId;
		terminal.Agreement = agreement.FromBase64();

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
		
		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<TerminalResponse?>(null);
		JsonElement merchantData = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		if (merchantData.GetStringOrNull("insId") == null) return new UResponse<TerminalResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
		
		merchant.InsId = merchantData.GetStringOrNull("insId")!;
		merchant.MerchantId = merchantData.GetStringOrNull("merchantId")!;

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

		if (terminalResponse is null or { IsSuccessStatusCode: false }) return new UResponse<TerminalResponse?>(null);
		JsonElement terminalData = JsonSerializer.Deserialize<JsonElement>(await terminalResponse.Content.ReadAsStringAsync(ct));
		if (terminalData.GetStringOrNull("insId") == null) return new UResponse<TerminalResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));

		terminal.TerminalId = terminalData.GetStringOrNull("terminalId");
		terminal.InsId = terminalData.GetStringOrNull("insId");
		
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

	public async Task<UResponse> BulkCreate(TerminalBulkCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		List<TerminalEntity> entities = [];

		entities.AddRange(p.List.Select(x => new TerminalEntity {
			Serial = x.Serial,
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson(),
			Tags = x.Tags,
			CreatorId = userData.Id
		}));

		await db.Set<TerminalEntity>().AddRangeAsync(entities, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse<IEnumerable<TerminalResponse>?>> Read(TerminalReadParams p, CancellationToken ct) {
		IQueryable<TerminalEntity> q = db.Set<TerminalEntity>().ApplyReadParams<TerminalEntity, TagTerminal, BaseJson>(p);

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
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<TerminalEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse<TerminalSupportPasswordResponse?>> ReadSupportPassword(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		TerminalEntity? e = await db.Set<TerminalEntity>().Select(x => new TerminalEntity {
			Serial = x.Serial,
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			JsonData = x.JsonData,
			Tags = x.Tags,
			CreatorId = x.CreatorId,
			InsId = x.InsId,
			TerminalId = x.TerminalId,
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

		if (e == null) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.NotFound, ls.Get("TerminalNotFound"));
		if (e.Merchant == null) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.NotFound, ls.Get("MerchantNotFound"));
		if (!userData.IsAdmin && userData.Id != e.CreatorId && userData.Id != e.Merchant?.UserId) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

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

		return new UResponse<TerminalSupportPasswordResponse?>(
			new TerminalSupportPasswordResponse { Password = data.GetStringOrNull("supportPassword") }
		);
	}

	private async Task<string> GenerateAgreement(UserEntity user, TerminalEntity terminal) {
		return await WordPdfGenerator.GenerateWithTextsAndImagesAsync(
			texts: new Dictionary<string, string> {
				{ "day", PersianDateTime.Now.Day.ToString() },
				{ "month", PersianDateTime.Now.Month.ToString() },
				{ "number", "NUMBER" },
				{ "fullName", $"{user.FirstName ?? ""} {user.LastName ?? ""}" },
				{ "nationalCode", user.NationalCode ?? "" },
				{ "birthdate", PersianDateTime.FromDateTime(user.Birthdate ?? DateTime.Now).ToString("yyyy-MM-dd") },
				{ "address", "ADDRESS" },
				{ "postalCode", terminal.Merchant?.ZipCode ?? "" },
				{ "phoneNumber", user.PhoneNumber ?? "" },
				{ "landLine", user.LandLine ?? "" },
				{ "fatherName", user.JsonData.FatherName ?? "" }
			},
			imagesBase64: new Dictionary<string, string> {
				{ "customerSignature", user.ESignature!.ToBase64()! }
			},
			templatePath: Path.Combine(Directory.GetCurrentDirectory(), "Templates", "atmAgreement.docx")
		);
	}
}