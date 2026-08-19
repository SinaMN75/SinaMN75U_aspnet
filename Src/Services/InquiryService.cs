namespace SinaMN75U.Services;

public interface IInquiryService {
	Task<UResponse<bool?>> MobileAndNationalCodeVerification(VerifyNationalCodeAndPhoneNumber p, CancellationToken ct);
	Task<UResponse<ZipCodeToAddressDetailResponse?>> ZipCodeToAddressDetail(ZipCodeToAddressDetailParams p, CancellationToken ct);
	Task<UResponse<VehicleViolationDetailResponse?>> VehicleViolationsDetail(VehicleViolationDetailParams p, CancellationToken ct);
	Task<UResponse<DrivingLicenceDetailResponse?>> DrivingLicenceDetail(DrivingLicenceDetailParams p, CancellationToken ct);
	Task<UResponse<LicencePlateDetailResponse?>> LicencePlateDetail(LicencePlateDetailParams p, CancellationToken ct);
	Task<UResponse<DrivingLicenceNegativePointResponse?>> DrivingLicenceNegativePoint(DrivingLicenceNegativePointParams p, CancellationToken ct);
	Task<UResponse<FreewayTollsResponse?>> FreewayTolls(FreewayTollsParams p, CancellationToken ct);
	Task<UResponse<IBanToBankAccountDetailResponse?>> IBanToBankAccountDetail(IBanToBankAccountDetailParams p, CancellationToken ct);
	Task<UResponse<InquiryCacheStatusResponse?>> InquiryCacheStatus(InquiryCacheStatusParams p, CancellationToken ct);
	UResponse<BillInfoResponse?> BillInfo(BillInfoParams p, CancellationToken ct);
}

public class InquiryService(
	DbContext db,
	IHttpClientService httpClient,
	ILocalizationService ls,
	ITokenService ts,
	IWalletService walletService
) : IInquiryService {
	private readonly ItHub _itHub = Core.App.ItHub;
	
	public UResponse<BillInfoResponse?> BillInfo(BillInfoParams p, CancellationToken ct) {
		BillParser parser = new();
		try {
			return new UResponse<BillInfoResponse?>(parser.Parse(p.BillId, p.PaymentId));
		}
		catch (Exception e) {
			return new UResponse<BillInfoResponse?>(null, Usc.ThirdPartyError, e.Message);
		}
	}

	public async Task<UResponse<bool?>> MobileAndNationalCodeVerification(VerifyNationalCodeAndPhoneNumber p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<bool?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (userData.IsExpired) return new UResponse<bool?>(null, Usc.ExpiredToken, ls.Get("TokenExpired"));
		
		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<bool?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));
		
		HttpResponseMessage? response = await SendMobileAndNationalCodeVerification(p, tokenResponse.AccessToken, ct);
		if (response == null) return new UResponse<bool?>(null);

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		bool data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetBoolean();

		await CreateMobileAndNationalCodeVerificationHistory(p.NationalCode, p.PhoneNumber, data, ct);
		return new UResponse<bool?>(data);
	}

	public async Task<UResponse<ZipCodeToAddressDetailResponse?>> ZipCodeToAddressDetail(ZipCodeToAddressDetailParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (userData.IsExpired) return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.ExpiredToken, ls.Get("TokenExpired"));

		InquiryHistoryEntity? inquiryHistory = p.Refresh ? null : await ReadZipCodeToAddressHistory(p, ct);
		string? responseBody = inquiryHistory?.Response;

		if (inquiryHistory == null || responseBody == null) {
			if (!await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.ZipCodeToAddressDetail, ct)) return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			// Charge the wallet before the billable third-party call so any external hit is always paid for
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagWalletTxn.ZipCodeToAddressDetail, Token = p.Token }, ct);

			HttpResponseMessage? response = await SendZipCodeToAddressDetail(p, tokenResponse.AccessToken, ct);

			if (response == null) return new UResponse<ZipCodeToAddressDetailResponse?>(null);
			responseBody = await response.Content.ReadAsStringAsync(ct);

			if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest) {
				string errorMessage = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("error").GetStringOrNull("customMessage") ?? ls.Get("ThirdPartyError");
				await CreateZipCodeToAddressHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.ZipCodeToAddressDetail, TagInquiryHistory.Error], errorMessage, p, ct);
				return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.ThirdPartyError, errorMessage);
			}

			if (!response.IsSuccessStatusCode) return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
			await CreateZipCodeToAddressHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.ZipCodeToAddressDetail], "", p, ct);
		}

		if (inquiryHistory?.Tags.Contains(TagInquiryHistory.Error) ?? false) return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.ThirdPartyError, inquiryHistory.JsonData.Detail1);

		JsonElement json = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data");

		return new UResponse<ZipCodeToAddressDetailResponse?>(new ZipCodeToAddressDetailResponse {
			IsCached = inquiryHistory != null,
			CachedAt = inquiryHistory?.CreatedAt,
			CacheExpiresAt = inquiryHistory == null ? null : inquiryHistory.CreatedAt.AddDays(Core.App.InquiryCacheDurations.ZipCodeToAddressDetail),
			BuildingName = json.GetStringOrNull("BuildingName"),
			Description = json.GetStringOrNull("description"),
			Floor = json.GetStringOrNull("floor"),
			HouseNumber = json.GetStringOrNull("houseNumber"),
			LocalityName = json.GetStringOrNull("localityName"),
			LocalityType = json.GetStringOrNull("localityType"),
			ZipCode = json.GetStringOrNull("zipCode"),
			Province = json.GetStringOrNull("province"),
			SideFloor = json.GetStringOrNull("sideFloor"),
			Street = json.GetStringOrNull("street"),
			Street2 = json.GetStringOrNull("street2"),
			SubLocality = json.GetStringOrNull("subLocality"),
			TownShip = json.GetStringOrNull("townShip"),
			TraceId = json.GetStringOrNull("traceId"),
			Village = json.GetStringOrNull("village")
		});
	}

	public async Task<UResponse<VehicleViolationDetailResponse?>> VehicleViolationsDetail(VehicleViolationDetailParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<VehicleViolationDetailResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (userData.IsExpired) return new UResponse<VehicleViolationDetailResponse?>(null, Usc.ExpiredToken, ls.Get("TokenExpired"));

		InquiryHistoryEntity? inquiryHistory = p.Refresh ? null : await ReadVehicleViolationsDetailHistory(p, ct);
		string? responseBody = inquiryHistory?.Response;

		if (inquiryHistory == null || responseBody == null) {
			if (!p.Refresh) return new UResponse<VehicleViolationDetailResponse?>(null, Usc.InquiryNotCached, ls.Get("InquiryNotCached"));
			if (!await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.VehicleViolationsDetail, ct)) return new UResponse<VehicleViolationDetailResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<VehicleViolationDetailResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			// Charge the wallet before the billable third-party call so any external hit is always paid for
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagWalletTxn.VehicleViolationsDetail, Token = p.Token }, ct);

			HttpResponseMessage? response = await SendVehicleViolationsDetail(p, tokenResponse.AccessToken, ct);

			if (response == null) return new UResponse<VehicleViolationDetailResponse?>(null);
			responseBody = await response.Content.ReadAsStringAsync(ct);
			JsonElement httpResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

			if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest) {
				string errorMessage = httpResponse.GetProperty("error").GetStringOrNull("customMessage") ?? ls.Get("ThirdPartyError");
				await CreateVehicleViolationsDetailHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.VehicleViolationsDetail, TagInquiryHistory.Error], errorMessage, p, ct);
				return new UResponse<VehicleViolationDetailResponse?>(null, Usc.ThirdPartyError, errorMessage);
			}

			if (!response.IsSuccessStatusCode) return new UResponse<VehicleViolationDetailResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
			await CreateVehicleViolationsDetailHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.VehicleViolationsDetail], "", p, ct);
		}

		if (inquiryHistory?.Tags.Contains(TagInquiryHistory.Error) ?? false) return new UResponse<VehicleViolationDetailResponse?>(null, Usc.ThirdPartyError, inquiryHistory.JsonData.Detail1);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetProperty("body");

		return new UResponse<VehicleViolationDetailResponse?>(new VehicleViolationDetailResponse {
			IsCached = inquiryHistory != null,
			CachedAt = inquiryHistory?.CreatedAt,
			CacheExpiresAt = inquiryHistory == null ? null : inquiryHistory.CreatedAt.AddDays(Core.App.InquiryCacheDurations.VehicleViolationsDetail),
			PlateDictation = data.GetStringOrNull("plateDictation"),
			PlateChar = data.GetStringOrNull("plateChar"),
			ComplaintStatus = data.GetStringOrNull("complaintStatus"),
			Complaint = data.GetStringOrNull("complaint"),
			DateTime = data.GetStringOrNull("sysDate") + " " + data.GetStringOrNull("sysTime"),
			PriceStatus = data.GetStringOrNull("priceStatus"),
			TraceId = data.GetStringOrNull("traceId"),
			PaperId = data.GetStringOrNull("paperId"),
			PaymentId = data.GetStringOrNull("paymentId"),
			WarningPrice = data.GetStringOrNull("warningPrice"),
			InquirePrice = data.GetStringOrNull("inquirePrice"),
			EjrInquireNo = data.GetStringOrNull("ejrInquireNo"),
			WarningId = data.GetStringOrNull("warningId"),
			InquirePriceDictation = data.GetStringOrNull("inquirePriceDictation"),
			Items = data.GetProperty("warningDTOs")
				.EnumerateArray()
				.Select(x => new VehicleViolationDetailResponse.VehicleViolationDetailItem {
					SerialNo = x.GetStringOrNull("serialNo"),
					Date = x.GetStringOrNull("violationOccureDate"),
					Type = x.TryGetProperty("violationDeliveryType", out JsonElement vdt)
						? vdt.GetStringOrNull("violationDeliveryType")
						: null,
					Address = x.GetStringOrNull("violatoinAddress"),
					ViolationType = x.TryGetProperty("violationTypeDTO", out JsonElement vtd)
						? vtd.GetStringOrNull("violationType")
						: null,
					FinalPrice = x.GetStringOrNull("finalPrice"),
					PaperId = x.GetStringOrNull("paperId"),
					PaymentId = x.GetStringOrNull("paymentId"),
					WarningId = x.GetStringOrNull("warningId"),
					InvestigationAbility = x.GetStringOrNull("investigationAbility"),
					HasImage = x.GetStringOrNull("hasImage") == "1"
				})
		});
	}

	public async Task<UResponse<DrivingLicenceDetailResponse?>> DrivingLicenceDetail(DrivingLicenceDetailParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<DrivingLicenceDetailResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (userData.IsExpired) return new UResponse<DrivingLicenceDetailResponse?>(null, Usc.ExpiredToken, ls.Get("TokenExpired"));

		InquiryHistoryEntity? inquiryHistory = p.Refresh ? null : await ReadDrivingLicenceDetailHistory(p, ct);
		string? responseBody = inquiryHistory?.Response;

		if (inquiryHistory == null || responseBody == null) {
			if (!p.Refresh) return new UResponse<DrivingLicenceDetailResponse?>(null, Usc.InquiryNotCached, ls.Get("InquiryNotCached"));
			if (!await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.DrivingLicenceStatus, ct)) return new UResponse<DrivingLicenceDetailResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<DrivingLicenceDetailResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			// Charge the wallet before the billable third-party call so any external hit is always paid for
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagWalletTxn.DrivingLicenceStatus, Token = p.Token }, ct);

			HttpResponseMessage? response = await SendDrivingLicenceDetail(p, tokenResponse.AccessToken, ct);

			if (response == null) return new UResponse<DrivingLicenceDetailResponse?>(null);
			responseBody = await response.Content.ReadAsStringAsync(ct);

			if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest) {
				string errorMessage = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("error").GetStringOrNull("customMessage") ?? ls.Get("ThirdPartyError");
				await CreateDrivingLicenceStatusHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.DrivingLicenceDetail, TagInquiryHistory.Error], errorMessage, p, ct);
				return new UResponse<DrivingLicenceDetailResponse?>(null, Usc.ThirdPartyError, errorMessage);
			}

			if (!response.IsSuccessStatusCode) return new UResponse<DrivingLicenceDetailResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
			await CreateDrivingLicenceStatusHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.DrivingLicenceDetail], "", p, ct);
		}

		if (inquiryHistory?.Tags.Contains(TagInquiryHistory.Error) ?? false) return new UResponse<DrivingLicenceDetailResponse?>(null, Usc.ThirdPartyError, inquiryHistory.JsonData.Detail1);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetProperty("body").EnumerateArray().First();

		return new UResponse<DrivingLicenceDetailResponse?>(new DrivingLicenceDetailResponse {
			IsCached = inquiryHistory != null,
			CachedAt = inquiryHistory?.CreatedAt,
			CacheExpiresAt = inquiryHistory == null ? null : inquiryHistory.CreatedAt.AddDays(Core.App.InquiryCacheDurations.DrivingLicenceStatus),
			NationalCode = data.GetStringOrNull("nationalNo"),
			FirstName = data.GetStringOrNull("firstName"),
			LastName = data.GetStringOrNull("lastName"),
			RequestDate = data.GetStringOrNull("requestDate"),
			Title = data.GetStringOrNull("title"),
			ConfirmDate = data.GetStringOrNull("printConfirmDate"),
			RahvarStatus = data.GetStringOrNull("rahvarStatus"),
			PacketNo = data.GetStringOrNull("packetNo"),
			Barcode = data.GetStringOrNull("barcode"),
			PrintNnumber = data.GetStringOrNull("printNum"),
			PrintDate = data.GetStringOrNull("printLicDate"),
			ValidYears = data.GetStringOrNull("validYears")
		});
	}

	public async Task<UResponse<LicencePlateDetailResponse?>> LicencePlateDetail(LicencePlateDetailParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<LicencePlateDetailResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (userData.IsExpired) return new UResponse<LicencePlateDetailResponse?>(null, Usc.ExpiredToken, ls.Get("TokenExpired"));

		InquiryHistoryEntity? inquiryHistory = p.Refresh ? null : await ReadLicencePlateStatusHistory(p, ct);
		string? responseBody = inquiryHistory?.Response;

		if (inquiryHistory == null || responseBody == null) {
			if (!p.Refresh) return new UResponse<LicencePlateDetailResponse?>(null, Usc.InquiryNotCached, ls.Get("InquiryNotCached"));
			if (!await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.LicencePlateDetail, ct)) return new UResponse<LicencePlateDetailResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<LicencePlateDetailResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			// Charge the wallet before the billable third-party call so any external hit is always paid for
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagWalletTxn.LicencePlateDetail, Token = p.Token }, ct);

			HttpResponseMessage? response = await SendLicencePlateDetail(p, tokenResponse.AccessToken, ct);

			if (response == null) return new UResponse<LicencePlateDetailResponse?>(null);
			responseBody = await response.Content.ReadAsStringAsync(ct);

			if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest) {
				string errorMessage = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("error").GetStringOrNull("customMessage") ?? ls.Get("ThirdPartyError");
				await CreateLicencePlateStatusHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.LicencePlateDetail, TagInquiryHistory.Error], errorMessage, p, ct);
				return new UResponse<LicencePlateDetailResponse?>(null, Usc.ThirdPartyError, errorMessage);
			}

			if (!response.IsSuccessStatusCode) return new UResponse<LicencePlateDetailResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
			await CreateLicencePlateStatusHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.LicencePlateDetail], "", p, ct);
		}

		if (inquiryHistory?.Tags.Contains(TagInquiryHistory.Error) ?? false) return new UResponse<LicencePlateDetailResponse?>(null, Usc.ThirdPartyError, inquiryHistory.JsonData.Detail1);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetProperty("body");

		return new UResponse<LicencePlateDetailResponse?>(new LicencePlateDetailResponse {
			IsCached = inquiryHistory != null,
			CachedAt = inquiryHistory?.CreatedAt,
			CacheExpiresAt = inquiryHistory == null ? null : inquiryHistory.CreatedAt.AddDays(Core.App.InquiryCacheDurations.LicencePlateDetail),
			Status = data.GetStringOrNull("plateStatus"),
			TracePlate = data.GetStringOrNull("tracePlate"),
			Items = data.GetProperty("historyPlate")
				.EnumerateArray().Select(x => new LicencePlateDetailResponse.LicencePlateHistoryItem {
						Type = x.GetStringOrNull("type"),
						InstallDate = x.GetStringOrNull("installDate"),
						Model = x.GetStringOrNull("model"),
						System = x.GetStringOrNull("system")
					}
				)
		});
	}

	public async Task<UResponse<DrivingLicenceNegativePointResponse?>> DrivingLicenceNegativePoint(DrivingLicenceNegativePointParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<DrivingLicenceNegativePointResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (userData.IsExpired) return new UResponse<DrivingLicenceNegativePointResponse?>(null, Usc.ExpiredToken, ls.Get("TokenExpired"));

		InquiryHistoryEntity? inquiryHistory = p.Refresh ? null : await ReadDrivingLicenceNegativePointHistory(p, ct);
		string? responseBody = inquiryHistory?.Response;

		if (inquiryHistory == null || responseBody == null) {
			// Group A gate: without an explicit paid refresh, never auto-charge - signal the app to show the payment screen first
			if (!p.Refresh) return new UResponse<DrivingLicenceNegativePointResponse?>(null, Usc.InquiryNotCached, ls.Get("InquiryNotCached"));
			if (!await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.DrivingLicenceNegativePoint, ct)) return new UResponse<DrivingLicenceNegativePointResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<DrivingLicenceNegativePointResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			// Charge the wallet before the billable third-party call so any external hit is always paid for
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagWalletTxn.DrivingLicenceNegativePoint, Token = p.Token }, ct);

			HttpResponseMessage? response = await SendDrivingLicenceNegativePoint(p, tokenResponse.AccessToken, ct);

			if (response == null) return new UResponse<DrivingLicenceNegativePointResponse?>(null);
			responseBody = await response.Content.ReadAsStringAsync(ct);

			if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest) {
				string errorMessage = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("error").GetStringOrNull("customMessage") ?? ls.Get("ThirdPartyError");
				await CreateDrivingLicenceNegativePointHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.DrivingLicenceNegativePoint, TagInquiryHistory.Error], errorMessage, p, ct);
				return new UResponse<DrivingLicenceNegativePointResponse?>(null, Usc.ThirdPartyError, errorMessage);
			}

			if (!response.IsSuccessStatusCode) return new UResponse<DrivingLicenceNegativePointResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
			await CreateDrivingLicenceNegativePointHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.DrivingLicenceNegativePoint], "", p, ct);
		}

		if (inquiryHistory?.Tags.Contains(TagInquiryHistory.Error) ?? false) return new UResponse<DrivingLicenceNegativePointResponse?>(null, Usc.ThirdPartyError, inquiryHistory.JsonData.Detail1);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetProperty("body");

		return new UResponse<DrivingLicenceNegativePointResponse?>(new DrivingLicenceNegativePointResponse {
			IsCached = inquiryHistory != null,
			CachedAt = inquiryHistory?.CreatedAt,
			CacheExpiresAt = inquiryHistory == null ? null : inquiryHistory.CreatedAt.AddDays(Core.App.InquiryCacheDurations.DrivingLicenceNegativePoint),
			Allowable = data.GetStringOrNull("allowable") == "1",
			Point = data.GetStringOrNull("negPoint"),
			RuleId = data.GetStringOrNull("ruleId")
		});
	}

	public async Task<UResponse<FreewayTollsResponse?>> FreewayTolls(FreewayTollsParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<FreewayTollsResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (userData.IsExpired) return new UResponse<FreewayTollsResponse?>(null, Usc.ExpiredToken, ls.Get("TokenExpired"));

		InquiryHistoryEntity? inquiryHistory = p.Refresh ? null : await ReadFreewayTollsHistory(p, ct);
		string? responseBody = inquiryHistory?.Response;

		if (inquiryHistory == null || responseBody == null) {
			// Group A gate: without an explicit paid refresh, never auto-charge - signal the app to show the payment screen first
			if (!p.Refresh) return new UResponse<FreewayTollsResponse?>(null, Usc.InquiryNotCached, ls.Get("InquiryNotCached"));
			if (!await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.FreewayToll, ct)) return new UResponse<FreewayTollsResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<FreewayTollsResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			// Charge the wallet before the billable third-party call so any external hit is always paid for
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagWalletTxn.FreewayTolls, Token = p.Token }, ct);

			HttpResponseMessage? response = await SendFreewayTolls(p, tokenResponse.AccessToken, ct);

			if (response == null) return new UResponse<FreewayTollsResponse?>(null);
			responseBody = await response.Content.ReadAsStringAsync(ct);

			if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest) {
				string errorMessage = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("error").GetStringOrNull("customMessage") ?? ls.Get("ThirdPartyError");
				await CreateFreewayTollsHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.FreewayTolls, TagInquiryHistory.Error], errorMessage, p, ct);
				return new UResponse<FreewayTollsResponse?>(null, Usc.ThirdPartyError, errorMessage);
			}

			if (!response.IsSuccessStatusCode) return new UResponse<FreewayTollsResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
			await CreateFreewayTollsHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.FreewayTolls], "", p, ct);
		}

		if (inquiryHistory?.Tags.Contains(TagInquiryHistory.Error) ?? false) return new UResponse<FreewayTollsResponse?>(null, Usc.ThirdPartyError, inquiryHistory.JsonData.Detail1);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody);

		return new UResponse<FreewayTollsResponse?>(new FreewayTollsResponse {
			IsCached = inquiryHistory != null,
			CachedAt = inquiryHistory?.CreatedAt,
			CacheExpiresAt = inquiryHistory == null ? null : inquiryHistory.CreatedAt.AddDays(Core.App.InquiryCacheDurations.FreewayToll),
			TotalPrice = data.GetIntOrNull("total_price").ToString(),
			Items = data.GetProperty("items").EnumerateArray().Select(x => new FreewayTollsResponse.FreewayTollsItem {
				Id = x.GetStringOrNull("id"),
				Date = x.GetStringOrNull("date"),
				Price = x.GetIntOrNull("price").ToString(),
				Gateway = x.GetStringOrNull("gateway"),
				Freeway = x.GetStringOrNull("freeway")
			})
		});
	}

	public async Task<UResponse<IBanToBankAccountDetailResponse?>> IBanToBankAccountDetail(IBanToBankAccountDetailParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IBanToBankAccountDetailResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (userData.IsExpired) return new UResponse<IBanToBankAccountDetailResponse?>(null, Usc.ExpiredToken, ls.Get("TokenExpired"));

		InquiryHistoryEntity? inquiryHistory = p.Refresh ? null : await ReadIBanToBankAccountDetailHistory(p, ct);
		string? responseBody = inquiryHistory?.Response;

		if (inquiryHistory == null || responseBody == null) {
			if (!await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.IBanToBankAccountDetail, ct)) return new UResponse<IBanToBankAccountDetailResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<IBanToBankAccountDetailResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			// Charge the wallet before the billable third-party call so any external hit is always paid for
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagWalletTxn.IBanToBankAccountDetail, Token = p.Token }, ct);

			HttpResponseMessage? response = await SendIBanToBankAccountDetail(p, tokenResponse.AccessToken, ct);

			if (response == null) return new UResponse<IBanToBankAccountDetailResponse?>(null);
			responseBody = await response.Content.ReadAsStringAsync(ct);

			if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest) {
				string errorMessage = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("error").GetStringOrNull("customMessage") ?? ls.Get("ThirdPartyError");
				await CreateIBanToBankAccountDetailHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.IBanToBankAccountDetail, TagInquiryHistory.Error], errorMessage, p, ct);
				return new UResponse<IBanToBankAccountDetailResponse?>(null, Usc.ThirdPartyError, errorMessage);
			}

			if (!response.IsSuccessStatusCode) return new UResponse<IBanToBankAccountDetailResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
			await CreateIBanToBankAccountDetailHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.IBanToBankAccountDetail], "", p, ct);
		}

		if (inquiryHistory?.Tags.Contains(TagInquiryHistory.Error) ?? false) return new UResponse<IBanToBankAccountDetailResponse?>(null, Usc.ThirdPartyError, inquiryHistory.JsonData.Detail1);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data");

		return new UResponse<IBanToBankAccountDetailResponse?>(new IBanToBankAccountDetailResponse {
			IsCached = inquiryHistory != null,
			CachedAt = inquiryHistory?.CreatedAt,
			CacheExpiresAt = inquiryHistory == null ? null : inquiryHistory.CreatedAt.AddDays(Core.App.InquiryCacheDurations.IBanToBankAccountDetail),
			DepositNumber = data.GetStringOrNull("depositNumber"),
			IBanType = data.GetStringOrNull("iBanType"),
			BankCode = data.GetStringOrNull("bankCode"),
			BankName = data.GetStringOrNull("bankName"),
			OwnerName = data.GetProperty("ownersInfo").EnumerateArray().Select(x => $"{x.GetStringOrNull("firstName")} {x.GetStringOrNull("lastName")}").First()
		});
	}

	// Read-only: reports which vehicle inquiries are already cached (and their expiry) without touching the wallet or any third-party API.
	public async Task<UResponse<InquiryCacheStatusResponse?>> InquiryCacheStatus(InquiryCacheStatusParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<InquiryCacheStatusResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (userData.IsExpired) return new UResponse<InquiryCacheStatusResponse?>(null, Usc.ExpiredToken, ls.Get("TokenExpired"));

		InquiryHistoryEntity? violation = await ReadVehicleViolationsDetailHistory(new VehicleViolationDetailParams { NationalCode = p.NationalCode, PhoneNumber = p.PhoneNumber, LicencePlate = p.LicencePlate }, ct);
		InquiryHistoryEntity? licence = await ReadDrivingLicenceDetailHistory(new DrivingLicenceDetailParams { NationalCode = p.NationalCode, PhoneNumber = p.PhoneNumber }, ct);
		InquiryHistoryEntity? plate = await ReadLicencePlateStatusHistory(new LicencePlateDetailParams { NationalCode = p.NationalCode, LicencePlate = p.LicencePlate }, ct);
		InquiryHistoryEntity? freeway = await ReadFreewayTollsHistory(new FreewayTollsParams { LicencePlate = p.LicencePlate }, ct);

		return new UResponse<InquiryCacheStatusResponse?>(new InquiryCacheStatusResponse {
			VehicleViolation = Item(violation, Core.App.InquiryCacheDurations.VehicleViolationsDetail),
			DrivingLicence = Item(licence, Core.App.InquiryCacheDurations.DrivingLicenceStatus),
			LicencePlate = Item(plate, Core.App.InquiryCacheDurations.LicencePlateDetail),
			FreewayTolls = Item(freeway, Core.App.InquiryCacheDurations.FreewayToll)
		});

		static InquiryCacheStatusResponse.CacheStatusItem? Item(InquiryHistoryEntity? h, int days) =>
			h == null ? null : new InquiryCacheStatusResponse.CacheStatusItem { CachedAt = h.CreatedAt, CacheExpiresAt = h.CreatedAt.AddDays(days) };
	}

	private async Task CreateMobileAndNationalCodeVerificationHistory(string nationalCode, string phoneNumber, bool isVerified, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson(),
			Tags = isVerified ? [TagInquiryHistory.ValidateNationalCodeAndPhoneNumber] : [TagInquiryHistory.ValidateNationalCodeAndPhoneNumber, TagInquiryHistory.NotVerified],
			NationalCode = nationalCode,
			PhoneNumber = phoneNumber,
			Response = ""
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task CreateZipCodeToAddressHistory(string responseBody, ICollection<TagInquiryHistory> tags, string message, ZipCodeToAddressDetailParams p, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson { Detail1 = message },
			Tags = tags,
			ZipCode = p.ZipCode,
			Response = responseBody
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task CreateVehicleViolationsDetailHistory(string responseBody, ICollection<TagInquiryHistory> tags, string message, VehicleViolationDetailParams p, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson { Detail1 = message },
			Tags = tags,
			PhoneNumber = p.PhoneNumber,
			LicencePlate = p.LicencePlate,
			NationalCode = p.NationalCode,
			Response = responseBody
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task CreateDrivingLicenceStatusHistory(string responseBody, ICollection<TagInquiryHistory> tags, string message, DrivingLicenceDetailParams p, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson { Detail1 = message },
			Tags = tags,
			PhoneNumber = p.PhoneNumber,
			NationalCode = p.NationalCode,
			Response = responseBody
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task CreateLicencePlateStatusHistory(string responseBody, ICollection<TagInquiryHistory> tags, string message, LicencePlateDetailParams p, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson { Detail1 = message },
			Tags = tags,
			LicencePlate = p.LicencePlate,
			NationalCode = p.NationalCode,
			Response = responseBody
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task CreateDrivingLicenceNegativePointHistory(string responseBody, ICollection<TagInquiryHistory> tags, string message, DrivingLicenceNegativePointParams p, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson { Detail1 = message },
			Tags = tags,
			NationalCode = p.NationalCode,
			PhoneNumber = p.PhoneNumber,
			DrivingLicenceNumber = p.DrivingLicenceNumber,
			Response = responseBody
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task CreateIBanToBankAccountDetailHistory(string responseBody, ICollection<TagInquiryHistory> tags, string message, IBanToBankAccountDetailParams p, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson { Detail1 = message },
			Tags = tags,
			IBan = p.IBan,
			Response = responseBody
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task CreateFreewayTollsHistory(string responseBody, ICollection<TagInquiryHistory> tags, string message, FreewayTollsParams p, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson { Detail1 = message },
			Tags = tags,
			LicencePlate = p.LicencePlate,
			Response = responseBody
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task<InquiryHistoryEntity?> ReadDrivingLicenceDetailHistory(DrivingLicenceDetailParams p, CancellationToken ct) {
		DateTime minDate = DateTime.UtcNow.AddDays(-Core.App.InquiryCacheDurations.DrivingLicenceStatus);
		return await db.Set<InquiryHistoryEntity>()
			.Where(x => x.PhoneNumber == p.PhoneNumber && x.NationalCode == p.NationalCode && x.Tags.Contains(TagInquiryHistory.DrivingLicenceDetail) && !x.Tags.Contains(TagInquiryHistory.Error) && x.CreatedAt >= minDate)
			.OrderByDescending(x => x.CreatedAt)
			.FirstOrDefaultAsync(ct);
	}

	private async Task<InquiryHistoryEntity?> ReadLicencePlateStatusHistory(LicencePlateDetailParams p, CancellationToken ct) {
		DateTime minDate = DateTime.UtcNow.AddDays(-Core.App.InquiryCacheDurations.LicencePlateDetail);
		return await db.Set<InquiryHistoryEntity>()
			.Where(x => x.LicencePlate == p.LicencePlate && x.NationalCode == p.NationalCode && x.Tags.Contains(TagInquiryHistory.LicencePlateDetail) && !x.Tags.Contains(TagInquiryHistory.Error) && x.CreatedAt >= minDate)
			.OrderByDescending(x => x.CreatedAt)
			.FirstOrDefaultAsync(ct);
	}

	private async Task<InquiryHistoryEntity?> ReadDrivingLicenceNegativePointHistory(DrivingLicenceNegativePointParams p, CancellationToken ct) {
		DateTime minDate = DateTime.UtcNow.AddDays(-Core.App.InquiryCacheDurations.DrivingLicenceNegativePoint);
		return await db.Set<InquiryHistoryEntity>()
			.Where(x => x.DrivingLicenceNumber == p.DrivingLicenceNumber && x.NationalCode == p.NationalCode && x.PhoneNumber == p.PhoneNumber && x.Tags.Contains(TagInquiryHistory.DrivingLicenceNegativePoint) && !x.Tags.Contains(TagInquiryHistory.Error) && x.CreatedAt >= minDate)
			.OrderByDescending(x => x.CreatedAt)
			.FirstOrDefaultAsync(ct);
	}

	private async Task<InquiryHistoryEntity?> ReadIBanToBankAccountDetailHistory(IBanToBankAccountDetailParams p, CancellationToken ct) {
		DateTime minDate = DateTime.UtcNow.AddDays(-Core.App.InquiryCacheDurations.IBanToBankAccountDetail);
		return await db.Set<InquiryHistoryEntity>()
			.Where(x => x.IBan == p.IBan && x.Tags.Contains(TagInquiryHistory.IBanToBankAccountDetail) && !x.Tags.Contains(TagInquiryHistory.Error) && x.CreatedAt >= minDate)
			.OrderByDescending(x => x.CreatedAt)
			.FirstOrDefaultAsync(ct);
	}

	private async Task<InquiryHistoryEntity?> ReadFreewayTollsHistory(FreewayTollsParams p, CancellationToken ct) {
		DateTime minDate = DateTime.UtcNow.AddDays(-Core.App.InquiryCacheDurations.FreewayToll);
		return await db.Set<InquiryHistoryEntity>()
			.Where(x => x.LicencePlate == p.LicencePlate && x.Tags.Contains(TagInquiryHistory.FreewayTolls) && !x.Tags.Contains(TagInquiryHistory.Error) && x.CreatedAt >= minDate)
			.OrderByDescending(x => x.CreatedAt)
			.FirstOrDefaultAsync(ct);
	}

	private async Task<InquiryHistoryEntity?> ReadZipCodeToAddressHistory(ZipCodeToAddressDetailParams p, CancellationToken ct) {
		DateTime minDate = DateTime.UtcNow.AddDays(-Core.App.InquiryCacheDurations.ZipCodeToAddressDetail);
		return await db.Set<InquiryHistoryEntity>()
			.Where(x => x.ZipCode == p.ZipCode && x.Tags.Contains(TagInquiryHistory.ZipCodeToAddressDetail) && !x.Tags.Contains(TagInquiryHistory.Error) && x.CreatedAt >= minDate)
			.OrderByDescending(x => x.CreatedAt)
			.FirstOrDefaultAsync(ct);
	}

	private async Task<InquiryHistoryEntity?> ReadVehicleViolationsDetailHistory(VehicleViolationDetailParams p, CancellationToken ct) {
		DateTime minDate = DateTime.UtcNow.AddDays(-Core.App.InquiryCacheDurations.VehicleViolationsDetail);
		return await db.Set<InquiryHistoryEntity>()
			.Where(x => x.PhoneNumber == p.PhoneNumber && x.LicencePlate == p.LicencePlate && x.NationalCode == p.NationalCode && x.Tags.Contains(TagInquiryHistory.VehicleViolationsDetail) && !x.Tags.Contains(TagInquiryHistory.Error) && x.CreatedAt >= minDate)
			.OrderByDescending(x => x.CreatedAt)
			.FirstOrDefaultAsync(ct);
	}

	protected virtual Task<HttpResponseMessage?> SendMobileAndNationalCodeVerification(VerifyNationalCodeAndPhoneNumber p, string accessToken, CancellationToken ct) =>
		httpClient.Post(
			"https://gateway.itsaaz.ir/hub/api/v1/Shahkar/MixVerifyMobile",
			new { nationalCode = p.NationalCode, mobile = p.PhoneNumber },
			new Dictionary<string, string> { { "Authorization", $"Bearer {accessToken}" } }
		);

	protected virtual Task<HttpResponseMessage?> SendZipCodeToAddressDetail(ZipCodeToAddressDetailParams p, string accessToken, CancellationToken ct) =>
		httpClient.Post(
			"https://gateway.itsaaz.ir/hub/api/v1/Address/DetailsTypeA",
			new { postcode = p.ZipCode, orderId = 1 },
			new Dictionary<string, string> { { "Authorization", $"Bearer {accessToken}" }, { "Accept", "application/json" } }
		);

	protected virtual Task<HttpResponseMessage?> SendVehicleViolationsDetail(VehicleViolationDetailParams p, string accessToken, CancellationToken ct) =>
		httpClient.Post(
			"https://api-ithub.itsaaz.ir/api/v1/CarServices/VehicleviolationsDetails",
			new {
				nationalCode = p.NationalCode,
				cellPhone = p.PhoneNumber,
				plk1 = p.LicencePlate[..2],
				plk2 = p.LicencePlate.Substring(2, 1),
				plk3 = p.LicencePlate.Substring(3, 3),
				plkSrl = p.LicencePlate.Substring(6, 2)
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {accessToken}" }, { "Accept", "application/json" } }
		);

	protected virtual Task<HttpResponseMessage?> SendDrivingLicenceDetail(DrivingLicenceDetailParams p, string accessToken, CancellationToken ct) =>
		httpClient.Post(
			"https://gateway.itsaaz.ir/hub/api/v1/CarServices/GavahinameStatusInquiry",
			new { nationalCode = p.NationalCode, cellphone = p.PhoneNumber },
			new Dictionary<string, string> { { "Authorization", $"Bearer {accessToken}" }, { "Accept", "application/json" } }
		);

	protected virtual Task<HttpResponseMessage?> SendLicencePlateDetail(LicencePlateDetailParams p, string accessToken, CancellationToken ct) =>
		httpClient.Post(
			"https://api-ithub.itsaaz.ir/api/v1/CarServices/PlateHistoryInquiry",
			new {
				nationalCode = p.NationalCode,
				plk1 = p.LicencePlate[..2],
				plk2 = p.LicencePlate.Substring(2, 1),
				plk3 = p.LicencePlate.Substring(3, 3),
				plkSrl = p.LicencePlate.Substring(6, 2)
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {accessToken}" }, { "Accept", "application/json" } }
		);

	protected virtual Task<HttpResponseMessage?> SendDrivingLicenceNegativePoint(DrivingLicenceNegativePointParams p, string accessToken, CancellationToken ct) =>
		httpClient.Post(
			"https://api-ithub.itsaaz.ir/api/v1/CarServices/DriversLicensePointsInquiry",
			new { licenseNo = p.DrivingLicenceNumber, nationalCode = p.NationalCode, cellphone = p.PhoneNumber },
			new Dictionary<string, string> { { "Authorization", $"Bearer {accessToken}" }, { "Accept", "application/json" } }
		);

	protected virtual Task<HttpResponseMessage?> SendFreewayTolls(FreewayTollsParams p, string accessToken, CancellationToken ct) =>
		httpClient.Post(
			"https://api-ithub.itsaaz.ir/api/v1/CarServices/GetFreewayTollsQuery",
			new {
				requestId = "1",
				plk1 = p.LicencePlate[..2],
				plk2 = p.LicencePlate.Substring(2, 1),
				plk3 = p.LicencePlate.Substring(3, 3),
				plkSrl = p.LicencePlate.Substring(6, 2)
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {accessToken}" }, { "Accept", "application/json" } }
		);

	protected virtual Task<HttpResponseMessage?> SendIBanToBankAccountDetail(IBanToBankAccountDetailParams p, string accessToken, CancellationToken ct) =>
		httpClient.Post(
			"https://api-ithub.itsaaz.ir/api/v1/CarServices/DriversLicensePointsInquiry",
			new { iban = p.IBan },
			new Dictionary<string, string> { { "Authorization", $"Bearer {accessToken}" }, { "Accept", "application/json" } }
		);

	protected virtual async Task<GetAccessTokenResponse?> GetAccessToken(CancellationToken ct) {
		HttpResponseMessage? response = await httpClient.Post(
			"https://gateway.itsaaz.ir/sts/connect/token",
			new Dictionary<string, string> {
				{ "grant_type", "password" },
				{ "client_id", _itHub.ClientId },
				{ "client_secret", _itHub.ClientSecret },
				{ "username", _itHub.UserName },
				{ "password", _itHub.Password }
			}
		);
		if (response == null) {
			ULog.Error("ItHub token request failed: no response from gateway.itsaaz.ir/sts/connect/token");
			return null;
		}

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody);
		string? accessToken = data.GetStringOrNull("access_token");
		if (accessToken == null) {
			ULog.Error($"ItHub token request returned no access_token: HTTP {(int)response.StatusCode} - {responseBody}");
			return null;
		}

		return new GetAccessTokenResponse { AccessToken = accessToken, ExpiresIn = data.GetIntOrNull("expires_in") };
	}
}

public class InquiryServiceFake(
	DbContext db,
	IHttpClientService httpClient,
	ILocalizationService ls,
	ITokenService ts,
	IWalletService walletService
) : InquiryService(db, httpClient, ls, ts, walletService) {
	protected override Task<GetAccessTokenResponse?> GetAccessToken(CancellationToken ct) =>
		Task.FromResult<GetAccessTokenResponse?>(new GetAccessTokenResponse { AccessToken = "FAKE", ExpiresIn = 3600 });

	protected override Task<HttpResponseMessage?> SendMobileAndNationalCodeVerification(VerifyNationalCodeAndPhoneNumber p, string accessToken, CancellationToken ct) =>
		FakeOk("""{"data":true}""");

	protected override Task<HttpResponseMessage?> SendZipCodeToAddressDetail(ZipCodeToAddressDetailParams p, string accessToken, CancellationToken ct) =>
		FakeOk("""{"data":{"BuildingName":"برج آسمان","description":"واحد جنوبی","floor":"5","houseNumber":"18","localityName":"سعادت آباد","localityType":"محله","zipCode":"1998754312","province":"تهران","sideFloor":"راست","street":"بلوار سرو","street2":"کوچه گلستان","subLocality":"ناحیه ۲","townShip":"تهران","traceId":"ZIP2406300001","village":""}}""");

	protected override Task<HttpResponseMessage?> SendVehicleViolationsDetail(VehicleViolationDetailParams p, string accessToken, CancellationToken ct) =>
		FakeOk("""{"data":{"body":{"plateDictation":"21 الف 345 ایران 99","plateChar":"الف","complaintStatus":"ندارد","complaint":"0","sysDate":"1405/04/09","sysTime":"11:25","priceStatus":"پرداخت نشده","traceId":"VV240630001","paperId":"812345678901","paymentId":"456123789654","warningPrice":"3,150,000","inquirePrice":"2,850,000","ejrInquireNo":"EJR548796","warningId":"WRN874512","inquirePriceDictation":"دو میلیون و هشتصد و پنجاه هزار ریال","warningDTOs":[{"serialNo":"100001","violationOccureDate":"1405/03/28 14:20","violationDeliveryType":{"violationDeliveryType":"دوربین"},"violatoinAddress":"بزرگراه همت","violationTypeDTO":{"violationType":"سرعت غیرمجاز"},"finalPrice":"1,500,000","paperId":"100000001","paymentId":"500000001","warningId":"W100001","investigationAbility":"دارد","hasImage":"1"},{"serialNo":"100002","violationOccureDate":"1405/04/02 09:15","violationDeliveryType":{"violationDeliveryType":"مامور"},"violatoinAddress":"خیابان ولیعصر","violationTypeDTO":{"violationType":"توقف ممنوع"},"finalPrice":"1,350,000","paperId":"100000002","paymentId":"500000002","warningId":"W100002","investigationAbility":"ندارد","hasImage":"0"}]}}}""");

	protected override Task<HttpResponseMessage?> SendDrivingLicenceDetail(DrivingLicenceDetailParams p, string accessToken, CancellationToken ct) =>
		FakeOk("""{"data":{"body":[{"nationalNo":"0012345678","firstName":"علی","lastName":"محمدی","requestDate":"1404/10/15","title":"آماده تحویل","printConfirmDate":"1404/10/18","rahvarStatus":"تحویل به پست","packetNo":"PK987654321","barcode":"626123456789012345","printNum":"PR140500123","printLicDate":"1404/10/16","validYears":"10"}]}}""");

	protected override Task<HttpResponseMessage?> SendLicencePlateDetail(LicencePlateDetailParams p, string accessToken, CancellationToken ct) =>
		FakeOk("""{"data":{"body":{"plateStatus":"فعال","tracePlate":"21الف34599","historyPlate":[{"type":"شخصی","installDate":"1401/05/20","model":"پژو 207","system":"سواری"},{"type":"شخصی","installDate":"1398/11/15","model":"پژو 206","system":"سواری"}]}}}""");

	protected override Task<HttpResponseMessage?> SendDrivingLicenceNegativePoint(DrivingLicenceNegativePointParams p, string accessToken, CancellationToken ct) =>
		FakeOk("""{"data":{"body":{"allowable":"1","negPoint":"8","ruleId":"NP-1405-001"}}}""");

	protected override Task<HttpResponseMessage?> SendFreewayTolls(FreewayTollsParams p, string accessToken, CancellationToken ct) =>
		FakeOk("""{"total_price":780000,"items":[{"id":"T001","date":"1405/03/15 08:20","price":250000,"gateway":"عوارض تهران-قم","freeway":"آزادراه تهران قم"},{"id":"T002","date":"1405/03/20 18:40","price":280000,"gateway":"عوارض قم-کاشان","freeway":"آزادراه امیرکبیر"},{"id":"T003","date":"1405/03/25 12:05","price":250000,"gateway":"عوارض تهران-پردیس","freeway":"آزادراه پردیس"}]}""");

	protected override Task<HttpResponseMessage?> SendIBanToBankAccountDetail(IBanToBankAccountDetailParams p, string accessToken, CancellationToken ct) =>
		FakeOk("""{"data":{"depositNumber":"0101234567001","iBanType":"جاری","bankCode":"017","bankName":"بانک ملی ایران","ownersInfo":[{"firstName":"علی","lastName":"محمدی"}]}}""");

	private static Task<HttpResponseMessage?> FakeOk(string body) =>
		Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) });
}
