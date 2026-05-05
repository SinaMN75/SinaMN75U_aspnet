using System.Net;

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
}

public class InquiryService(
	DbContext db,
	IHttpClientService httpClient,
	ILocalizationService ls,
	ITokenService ts,
	IWalletService walletService
) : IInquiryService {
	private readonly ItHub _itHub = Core.App.ItHub;

	public async Task<UResponse<bool?>> MobileAndNationalCodeVerification(VerifyNationalCodeAndPhoneNumber p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<bool?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		InquiryHistoryEntity? inquiryHistory = await ReadMobileAndNationalCodeVerificationHistory(p.NationalCode, p.PhoneNumber, ct);
		if (inquiryHistory != null) return new UResponse<bool?>(inquiryHistory.Tags.Contains(TagInquiryHistory.Verified));

		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<bool?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			"https://gateway.itsaaz.ir/hub/api/v1/Shahkar/MixVerifyMobile",
			new { nationalCode = p.NationalCode, mobile = p.PhoneNumber },
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" } }
		);
		if (response == null) return new UResponse<bool?>(null);

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		bool data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetBoolean();

		await CreateMobileAndNationalCodeVerificationHistory(p.NationalCode, p.PhoneNumber, data, ct);
		await walletService.Purchase(new WalletPurchaseParams { Tag = TagWalletTxn.MobileAndNationalCodeVerification, Token = p.Token }, ct);

		return new UResponse<bool?>(data);
	}

	public async Task<UResponse<ZipCodeToAddressDetailResponse?>> ZipCodeToAddressDetail(ZipCodeToAddressDetailParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		InquiryHistoryEntity? inquiryHistory = await ReadZipCodeToAddressHistory(p, ct);
		string? responseBody = inquiryHistory?.Response;

		if (inquiryHistory == null || responseBody == null) {
			if (!await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.ZipCodeToAddressDetail, ct)) return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			HttpResponseMessage? response = await httpClient.Post(
				"https://gateway.itsaaz.ir/hub/api/v1/Address/DetailsTypeA",
				new { postcode = p.ZipCode, orderId = 1 },
				new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
			);

			if (response == null) return new UResponse<ZipCodeToAddressDetailResponse?>(null);
			responseBody = await response.Content.ReadAsStringAsync(ct);

			if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest) {
				string errorMessage = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("error").GetStringOrNull("customMessage") ?? ls.Get("ThirdPartyError");
				await CreateZipCodeToAddressHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.ZipCodeToAddressDetail, TagInquiryHistory.Verified, TagInquiryHistory.Error], errorMessage, p, ct);
				return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.ThirdPartyError, errorMessage);
			}

			if (!response.IsSuccessStatusCode) return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
			await CreateZipCodeToAddressHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.ZipCodeToAddressDetail, TagInquiryHistory.Verified], "", p, ct);
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagWalletTxn.ZipCodeToAddressDetail, Token = p.Token }, ct);
		}

		if (inquiryHistory?.Tags.Contains(TagInquiryHistory.Error) ?? false) return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.ThirdPartyError, inquiryHistory.JsonData.Detail1);

		JsonElement json = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data");

		return new UResponse<ZipCodeToAddressDetailResponse?>(new ZipCodeToAddressDetailResponse {
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

		InquiryHistoryEntity? inquiryHistory = await ReadVehicleViolationsDetailHistory(p, ct);
		string? responseBody = inquiryHistory?.Response;

		if (inquiryHistory == null || responseBody == null) {
			if (!await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.VehicleViolationsDetail, ct)) return new UResponse<VehicleViolationDetailResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<VehicleViolationDetailResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			HttpResponseMessage? response = await httpClient.Post(
				"https://api-ithub.itsaaz.ir/api/v1/CarServices/VehicleviolationsDetails",
				new {
					nationalCode = p.NationalCode,
					cellPhone = p.PhoneNumber,
					plk1 = p.LicencePlate[..2],
					plk2 = p.LicencePlate.Substring(2, 1),
					plk3 = p.LicencePlate.Substring(3, 3),
					plkSrl = p.LicencePlate.Substring(6, 2)
				},
				new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
			);

			if (response == null) return new UResponse<VehicleViolationDetailResponse?>(null);
			responseBody = await response.Content.ReadAsStringAsync(ct);
			JsonElement httpResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

			if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest) {
				string errorMessage = httpResponse.GetProperty("error").GetStringOrNull("customMessage") ?? ls.Get("ThirdPartyError");
				await CreateVehicleViolationsDetailHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.VehicleViolationsDetail, TagInquiryHistory.Verified, TagInquiryHistory.Error], errorMessage, p, ct);
				return new UResponse<VehicleViolationDetailResponse?>(null, Usc.ThirdPartyError, errorMessage);
			}

			if (!response.IsSuccessStatusCode) return new UResponse<VehicleViolationDetailResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
			await CreateVehicleViolationsDetailHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.VehicleViolationsDetail, TagInquiryHistory.Verified], "", p, ct);
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagWalletTxn.VehicleViolationsDetail, Token = p.Token }, ct);
		}

		if (inquiryHistory?.Tags.Contains(TagInquiryHistory.Error) ?? false) return new UResponse<VehicleViolationDetailResponse?>(null, Usc.ThirdPartyError, inquiryHistory.JsonData.Detail1);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetProperty("body");

		return new UResponse<VehicleViolationDetailResponse?>(new VehicleViolationDetailResponse {
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

		InquiryHistoryEntity? inquiryHistory = await ReadDrivingLicenceStatusHistory(p, ct);
		string? responseBody = inquiryHistory?.Response;

		if (inquiryHistory == null || responseBody == null) {
			if (!await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.DrivingLicenceStatus, ct)) return new UResponse<DrivingLicenceDetailResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<DrivingLicenceDetailResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			HttpResponseMessage? response = await httpClient.Post(
				"https://gateway.itsaaz.ir/hub/api/v1/CarServices/GavahinameStatusInquiry",
				new { nationalCode = p.NationalCode, cellphone = p.PhoneNumber },
				new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
			);

			if (response == null) return new UResponse<DrivingLicenceDetailResponse?>(null);
			responseBody = await response.Content.ReadAsStringAsync(ct);

			if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest) {
				string errorMessage = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("error").GetStringOrNull("customMessage") ?? ls.Get("ThirdPartyError");
				await CreateDrivingLicenceStatusHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.DrivingLicenceDetail, TagInquiryHistory.Verified, TagInquiryHistory.Error], errorMessage, p, ct);
				return new UResponse<DrivingLicenceDetailResponse?>(null, Usc.ThirdPartyError, errorMessage);
			}

			if (!response.IsSuccessStatusCode) return new UResponse<DrivingLicenceDetailResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
			await CreateDrivingLicenceStatusHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.DrivingLicenceDetail, TagInquiryHistory.Verified], "", p, ct);
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagWalletTxn.DrivingLicenceStatus, Token = p.Token }, ct);
		}

		if (inquiryHistory?.Tags.Contains(TagInquiryHistory.Error) ?? false) return new UResponse<DrivingLicenceDetailResponse?>(null, Usc.ThirdPartyError, inquiryHistory.JsonData.Detail1);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetProperty("body").EnumerateArray().First();

		return new UResponse<DrivingLicenceDetailResponse?>(new DrivingLicenceDetailResponse {
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

		InquiryHistoryEntity? inquiryHistory = await ReadLicencePlateStatusHistory(p, ct);
		string? responseBody = inquiryHistory?.Response;

		if (inquiryHistory == null || responseBody == null) {
			if (!await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.LicencePlateDetail, ct)) return new UResponse<LicencePlateDetailResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<LicencePlateDetailResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			HttpResponseMessage? response = await httpClient.Post(
				"https://api-ithub.itsaaz.ir/api/v1/CarServices/PlateHistoryInquiry",
				new {
					nationalCode = p.NationalCode,
					plk1 = p.LicencePlate[..2],
					plk2 = p.LicencePlate.Substring(2, 1),
					plk3 = p.LicencePlate.Substring(3, 3),
					plkSrl = p.LicencePlate.Substring(6, 2)
				},
				new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
			);

			if (response == null) return new UResponse<LicencePlateDetailResponse?>(null);
			responseBody = await response.Content.ReadAsStringAsync(ct);

			if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest) {
				string errorMessage = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("error").GetStringOrNull("customMessage") ?? ls.Get("ThirdPartyError");
				await CreateLicencePlateStatusHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.LicencePlateDetail, TagInquiryHistory.Verified, TagInquiryHistory.Error], errorMessage, p, ct);
				return new UResponse<LicencePlateDetailResponse?>(null, Usc.ThirdPartyError, errorMessage);
			}

			if (!response.IsSuccessStatusCode) return new UResponse<LicencePlateDetailResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
			await CreateLicencePlateStatusHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.LicencePlateDetail, TagInquiryHistory.Verified], "", p, ct);
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagWalletTxn.LicencePlateDetail, Token = p.Token }, ct);
		}

		if (inquiryHistory?.Tags.Contains(TagInquiryHistory.Error) ?? false) return new UResponse<LicencePlateDetailResponse?>(null, Usc.ThirdPartyError, inquiryHistory.JsonData.Detail1);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetProperty("body");

		return new UResponse<LicencePlateDetailResponse?>(new LicencePlateDetailResponse {
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

		InquiryHistoryEntity? inquiryHistory = await ReadDrivingLicenceNegativePointHistory(p, ct);
		string? responseBody = inquiryHistory?.Response;

		if (inquiryHistory == null || responseBody == null) {
			if (!await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.DrivingLicenceNegativePoint, ct)) return new UResponse<DrivingLicenceNegativePointResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<DrivingLicenceNegativePointResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			HttpResponseMessage? response = await httpClient.Post(
				"https://api-ithub.itsaaz.ir/api/v1/CarServices/DriversLicensePointsInquiry",
				new { licenseNo = p.DrivingLicenceNumber, nationalCode = p.NationalCode, cellphone = p.PhoneNumber },
				new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
			);

			if (response == null) return new UResponse<DrivingLicenceNegativePointResponse?>(null);
			responseBody = await response.Content.ReadAsStringAsync(ct);

			if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest) {
				string errorMessage = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("error").GetStringOrNull("customMessage") ?? ls.Get("ThirdPartyError");
				await CreateDrivingLicenceNegativePointHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.DrivingLicenceNegativePoint, TagInquiryHistory.Verified, TagInquiryHistory.Error], errorMessage, p, ct);
				return new UResponse<DrivingLicenceNegativePointResponse?>(null, Usc.ThirdPartyError, errorMessage);
			}

			if (!response.IsSuccessStatusCode) return new UResponse<DrivingLicenceNegativePointResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
			await CreateDrivingLicenceNegativePointHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.DrivingLicenceNegativePoint, TagInquiryHistory.Verified], "", p, ct);
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagWalletTxn.DrivingLicenceNegativePoint, Token = p.Token }, ct);
		}

		if (inquiryHistory?.Tags.Contains(TagInquiryHistory.Error) ?? false) return new UResponse<DrivingLicenceNegativePointResponse?>(null, Usc.ThirdPartyError, inquiryHistory.JsonData.Detail1);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetProperty("body");

		return new UResponse<DrivingLicenceNegativePointResponse?>(new DrivingLicenceNegativePointResponse {
			Allowable = data.GetStringOrNull("allowable") == "1",
			Point = data.GetStringOrNull("negPoint"),
			RuleId = data.GetStringOrNull("ruleId")
		});
	}

	public async Task<UResponse<FreewayTollsResponse?>> FreewayTolls(FreewayTollsParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<FreewayTollsResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		InquiryHistoryEntity? inquiryHistory = await ReadFreewayTollsHistory(p, ct);
		string? responseBody = inquiryHistory?.Response;

		if (inquiryHistory == null || responseBody == null) {
			if (!await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.IBanToBankAccountDetail, ct)) return new UResponse<FreewayTollsResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<FreewayTollsResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			HttpResponseMessage? response = await httpClient.Post(
				"https://api-ithub.itsaaz.ir/api/v1/CarServices/GetFreewayTollsQuery",
				new {
					requestId = "1",
					plk1 = p.LicencePlate[..2],
					plk2 = p.LicencePlate.Substring(2, 1),
					plk3 = p.LicencePlate.Substring(3, 3),
					plkSrl = p.LicencePlate.Substring(6, 2)
				},
				new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
			);

			if (response == null) return new UResponse<FreewayTollsResponse?>(null);
			responseBody = await response.Content.ReadAsStringAsync(ct);

			if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest) {
				string errorMessage = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("error").GetStringOrNull("customMessage") ?? ls.Get("ThirdPartyError");
				await CreateFreewayTollsHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.IBanToBankAccountDetail, TagInquiryHistory.Verified, TagInquiryHistory.Error], errorMessage, p, ct);
				return new UResponse<FreewayTollsResponse?>(null, Usc.ThirdPartyError, errorMessage);
			}

			if (!response.IsSuccessStatusCode) return new UResponse<FreewayTollsResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
			await CreateFreewayTollsHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.IBanToBankAccountDetail, TagInquiryHistory.Verified], "", p, ct);
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagWalletTxn.IBanToBankAccountDetail, Token = p.Token }, ct);
		}

		if (inquiryHistory?.Tags.Contains(TagInquiryHistory.Error) ?? false) return new UResponse<FreewayTollsResponse?>(null, Usc.ThirdPartyError, inquiryHistory.JsonData.Detail1);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody);

		return new UResponse<FreewayTollsResponse?>(new FreewayTollsResponse {
			TotalPrice = data.GetIntOrNull("total_price").ToString(),
			Items = data.GetProperty("items").EnumerateArray().Select(x => new FreewayTollsResponse.FreewayTollsItem {
				Id = x.GetStringOrNull("id"),
				Date = x.GetStringOrNull("date"),
				Price = x.GetIntOrNull("price").ToString(),
				Gateway = x.GetStringOrNull("gateway"),
				Freeway = x.GetStringOrNull("freeway"),
			})
		});
	}

	public async Task<UResponse<IBanToBankAccountDetailResponse?>> IBanToBankAccountDetail(IBanToBankAccountDetailParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IBanToBankAccountDetailResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		InquiryHistoryEntity? inquiryHistory = await ReadIBanToBankAccountDetailHistory(p, ct);
		string? responseBody = inquiryHistory?.Response;

		if (inquiryHistory == null || responseBody == null) {
			if (!await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.IBanToBankAccountDetail, ct)) return new UResponse<IBanToBankAccountDetailResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<IBanToBankAccountDetailResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			HttpResponseMessage? response = await httpClient.Post(
				"https://api-ithub.itsaaz.ir/api/v1/CarServices/DriversLicensePointsInquiry",
				new { iban = p.IBan },
				new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
			);

			if (response == null) return new UResponse<IBanToBankAccountDetailResponse?>(null);
			responseBody = await response.Content.ReadAsStringAsync(ct);

			if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest) {
				string errorMessage = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("error").GetStringOrNull("customMessage") ?? ls.Get("ThirdPartyError");
				await CreateIBanToBankAccountDetailHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.IBanToBankAccountDetail, TagInquiryHistory.Verified, TagInquiryHistory.Error], errorMessage, p, ct);
				return new UResponse<IBanToBankAccountDetailResponse?>(null, Usc.ThirdPartyError, errorMessage);
			}

			if (!response.IsSuccessStatusCode) return new UResponse<IBanToBankAccountDetailResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
			await CreateIBanToBankAccountDetailHistory(responseBody, [TagInquiryHistory.ItHub, TagInquiryHistory.IBanToBankAccountDetail, TagInquiryHistory.Verified], "", p, ct);
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagWalletTxn.IBanToBankAccountDetail, Token = p.Token }, ct);
		}

		if (inquiryHistory?.Tags.Contains(TagInquiryHistory.Error) ?? false) return new UResponse<IBanToBankAccountDetailResponse?>(null, Usc.ThirdPartyError, inquiryHistory.JsonData.Detail1);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data");

		return new UResponse<IBanToBankAccountDetailResponse?>(new IBanToBankAccountDetailResponse {
			DepositNumber = data.GetStringOrNull("depositNumber"),
			IBanType = data.GetStringOrNull("iBanType"),
			BankCode = data.GetStringOrNull("bankCode"),
			BankName = data.GetStringOrNull("bankName"),
			OwnerName = data.GetProperty("ownersInfo").EnumerateArray().Select(x => $"{x.GetStringOrNull("firstName")} {x.GetStringOrNull("lastName")}").First()
		});
	}

	private async Task CreateMobileAndNationalCodeVerificationHistory(string nationalCode, string phoneNumber, bool isVerified, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJsonData(),
			Tags = isVerified ? [TagInquiryHistory.ValidateNationalCodeAndPhoneNumber, TagInquiryHistory.Verified] : [TagInquiryHistory.ValidateNationalCodeAndPhoneNumber, TagInquiryHistory.NotVerified],
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
			JsonData = new BaseJsonData { Detail1 = message },
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
			JsonData = new BaseJsonData { Detail1 = message },
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
			JsonData = new BaseJsonData { Detail1 = message },
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
			JsonData = new BaseJsonData { Detail1 = message },
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
			JsonData = new BaseJsonData { Detail1 = message },
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
			JsonData = new BaseJsonData { Detail1 = message },
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
			JsonData = new BaseJsonData { Detail1 = message },
			Tags = tags,
			LicencePlate = p.LicencePlate,
			Response = responseBody
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task<InquiryHistoryEntity?> ReadMobileAndNationalCodeVerificationHistory(string nationalCode, string phoneNumber, CancellationToken ct) {
		InquiryHistoryEntity? e = await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.NationalCode == nationalCode && x.PhoneNumber == phoneNumber, ct);
		return e;
	}

	private async Task<InquiryHistoryEntity?> ReadDrivingLicenceStatusHistory(DrivingLicenceDetailParams p, CancellationToken ct) => await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.PhoneNumber == p.PhoneNumber && x.NationalCode == p.NationalCode && x.Tags.Contains(TagInquiryHistory.LicencePlateDetail), ct);
	private async Task<InquiryHistoryEntity?> ReadLicencePlateStatusHistory(LicencePlateDetailParams p, CancellationToken ct) => await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.LicencePlate == p.LicencePlate && x.NationalCode == p.NationalCode && x.Tags.Contains(TagInquiryHistory.LicencePlateDetail), ct);
	private async Task<InquiryHistoryEntity?> ReadDrivingLicenceNegativePointHistory(DrivingLicenceNegativePointParams p, CancellationToken ct) => await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.DrivingLicenceNumber == p.DrivingLicenceNumber && x.NationalCode == p.NationalCode && x.PhoneNumber == p.PhoneNumber && x.Tags.Contains(TagInquiryHistory.DrivingLicenceNegativePoint), ct);
	private async Task<InquiryHistoryEntity?> ReadIBanToBankAccountDetailHistory(IBanToBankAccountDetailParams p, CancellationToken ct) => await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.IBan == p.IBan && x.Tags.Contains(TagInquiryHistory.IBanToBankAccountDetail), ct);
	private async Task<InquiryHistoryEntity?> ReadFreewayTollsHistory(FreewayTollsParams p, CancellationToken ct) => await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.LicencePlate == p.LicencePlate && x.Tags.Contains(TagInquiryHistory.FreewayTolls), ct);
	private async Task<InquiryHistoryEntity?> ReadZipCodeToAddressHistory(ZipCodeToAddressDetailParams p, CancellationToken ct) => await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.ZipCode == p.ZipCode && x.Tags.Contains(TagInquiryHistory.ZipCodeToAddressDetail), ct);
	private async Task<InquiryHistoryEntity?> ReadVehicleViolationsDetailHistory(VehicleViolationDetailParams p, CancellationToken ct) => await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.PhoneNumber == p.PhoneNumber && x.LicencePlate == p.LicencePlate && x.NationalCode == p.NationalCode && x.Tags.Contains(TagInquiryHistory.VehicleViolationsDetail), ct);

	private async Task<GetAccessTokenResponse?> GetAccessToken(CancellationToken ct) {
		HttpResponseMessage? response = await httpClient.PostForm(
			"https://gateway.itsaaz.ir/sts/connect/token",
			new Dictionary<string, string> {
				{ "grant_type", "password" },
				{ "client_id", _itHub.ClientId },
				{ "Client_secret", _itHub.ClientSecret },
				{ "username", _itHub.UserName },
				{ "password", _itHub.Password }
			}
		);
		if (response == null) return null;

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody);

		return new GetAccessTokenResponse { AccessToken = data.GetStringOrNull("access_token"), ExpiresIn = data.GetIntOrNull("expires_in") };
	}
}