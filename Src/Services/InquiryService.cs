namespace SinaMN75U.Services;

public interface IInquiryService {
	Task<bool> IsServiceConnected(int code, CancellationToken ct);
	Task<UResponse<bool?>> MobileAndNationalCodeVerification(VerifyNationalCodeAndPhoneNumber p, CancellationToken ct);
	Task<UResponse<ZipCodeToAddressDetailResponse?>> ZipCodeToAddressDetail(ZipCodeToAddressDetailParams p, CancellationToken ct);
	Task<UResponse<VehicleViolationDetailResponse?>> VehicleViolationsDetail(VehicleViolationDetailParams p, CancellationToken ct);
	Task<UResponse<DrivingLicenceStatusResponse?>> DrivingLicenceStatus(DrivingLicenceStatusParams p, CancellationToken ct);
	Task<UResponse<LicencePlateDetailResponse?>> LicencePlateDetail(LicencePlateInquiryParams p, CancellationToken ct);
	Task<UResponse<DrivingLicenceNegativePointResponse?>> DrivingLicenceNegativePoint(DrivingLicenceNegativePointParams p, CancellationToken ct);
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

	public async Task<bool> IsServiceConnected(int code, CancellationToken ct) {
		HttpResponseMessage? response = await httpClient.Get("https://gateway.itsaaz.ir/hub/api/v1/Hc/Hub");
		if (response == null) return false;

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		JsonElement.ArrayEnumerator data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").EnumerateArray();

		foreach (JsonElement item in data) {
			if (item.GetProperty("hubRequestType").GetInt32() == code) return item.GetProperty("isConnect").GetBoolean();
		}

		return false;
	}

	public async Task<UResponse<bool?>> MobileAndNationalCodeVerification(VerifyNationalCodeAndPhoneNumber p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<bool?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		bool? isRecordExist = await ReadMobileAndNationalCodeVerificationHistory(p.NationalCode, p.PhoneNumber, ct);
		if (isRecordExist != null) return new UResponse<bool?>(isRecordExist);

		bool hasEnoughBalance = await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.MobileAndNationalCodeVerification, ct);
		if (!hasEnoughBalance) return new UResponse<bool?>(false, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

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
		await walletService.Purchase(new WalletPurchaseParams { Tag = TagPurchase.MobileAndNationalCodeVerification, Token = p.Token }, ct);

		return new UResponse<bool?>(data);
	}

	public async Task<UResponse<ZipCodeToAddressDetailResponse?>> ZipCodeToAddressDetail(ZipCodeToAddressDetailParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		string? responseBody = await ReadZipCodeToAddressHistory(p, ct);

		if (responseBody == null) {
			bool hasEnoughBalance = await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.ZipCodeToAddressDetail, ct);
			if (!hasEnoughBalance) return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));
			
			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			HttpResponseMessage? response = await httpClient.Post(
				"https://gateway.itsaaz.ir/hub/api/v1/Address/DetailsTypeA",
				new { postcode = p.ZipCode, orderId = 1 },
				new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
			);
			if (response == null) return new UResponse<ZipCodeToAddressDetailResponse?>(null);

			responseBody = await response.Content.ReadAsStringAsync(ct);
			await CreateZipCodeToAddressHistory(responseBody, p, ct);
		}

		JsonElement json = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data");

		ZipCodeToAddressDetailResponse data = new() {
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
		};
		
		await walletService.Purchase(new WalletPurchaseParams { Tag = TagPurchase.ZipCodeToAddressDetail, Token = p.Token }, ct);

		return new UResponse<ZipCodeToAddressDetailResponse?>(data);
	}

	public async Task<UResponse<VehicleViolationDetailResponse?>> VehicleViolationsDetail(VehicleViolationDetailParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<VehicleViolationDetailResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		string? responseBody = await ReadVehicleViolationsDetailHistory(p, ct);

		if (responseBody == null) {
			bool hasEnoughBalance = await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.VehicleViolationsDetail, ct);
			if (!hasEnoughBalance) return new UResponse<VehicleViolationDetailResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			UResponse<TagTxnErrorCodes> purchaseState = await walletService.Purchase(new WalletPurchaseParams { Tag = TagPurchase.VehicleViolationsDetail, Token = p.Token }, ct);
			if (purchaseState.Result != TagTxnErrorCodes.Ok) return new UResponse<VehicleViolationDetailResponse?>(null, purchaseState.Status, purchaseState.Message);

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
			await CreateVehicleViolationsDetailHistory(responseBody, p, ct);
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagPurchase.VehicleViolationsDetail, Token = p.Token }, ct);
		}

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
				.Select(x => new VehicleViolationDetailItem {
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

	public async Task<UResponse<DrivingLicenceStatusResponse?>> DrivingLicenceStatus(DrivingLicenceStatusParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<DrivingLicenceStatusResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		string? responseBody = await ReadDrivingLicenceStatusHistory(p, ct);

		if (responseBody == null) {
			bool hasEnoughBalance = await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.DrivingLicenceStatus, ct);
			if (!hasEnoughBalance) return new UResponse<DrivingLicenceStatusResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			UResponse<TagTxnErrorCodes> purchaseState = await walletService.Purchase(new WalletPurchaseParams { Tag = TagPurchase.DrivingLicenceStatus, Token = p.Token }, ct);
			if (purchaseState.Result != TagTxnErrorCodes.Ok) return new UResponse<DrivingLicenceStatusResponse?>(null, purchaseState.Status, purchaseState.Message);

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<DrivingLicenceStatusResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			HttpResponseMessage? response = await httpClient.Post(
				"https://gateway.itsaaz.ir/hub/api/v1/CarServices/GavahinameStatusInquiry",
				new { nationalCode = p.NationalCode, cellphone = p.PhoneNumber },
				new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
			);
			if (response == null) return new UResponse<DrivingLicenceStatusResponse?>(null);

			responseBody = await response.Content.ReadAsStringAsync(ct);
			await CreateDrivingLicenceStatusHistory(responseBody, p, ct);
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagPurchase.DrivingLicenceStatus, Token = p.Token }, ct);
		}

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetProperty("body").EnumerateArray().First();

		return new UResponse<DrivingLicenceStatusResponse?>(new DrivingLicenceStatusResponse {
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

	public async Task<UResponse<LicencePlateDetailResponse?>> LicencePlateDetail(LicencePlateInquiryParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<LicencePlateDetailResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		string? responseBody = await ReadLicencePlateStatusHistory(p, ct);

		if (responseBody == null) {
			bool hasEnoughBalance = await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.LicencePlateDetail, ct);
			if (!hasEnoughBalance) return new UResponse<LicencePlateDetailResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			UResponse<TagTxnErrorCodes> purchaseState = await walletService.Purchase(new WalletPurchaseParams { Tag = TagPurchase.LicencePlateDetail, Token = p.Token }, ct);
			if (purchaseState.Result != TagTxnErrorCodes.Ok) return new UResponse<LicencePlateDetailResponse?>(null, purchaseState.Status, purchaseState.Message);

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
			await CreateLicencePlateStatusHistory(responseBody, p, ct);
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagPurchase.LicencePlateDetail, Token = p.Token }, ct);
		}

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetProperty("body");

		return new UResponse<LicencePlateDetailResponse?>(new LicencePlateDetailResponse {
			Status = data.GetStringOrNull("plateStatus"),
			TracePlate = data.GetStringOrNull("tracePlate"),
			Items = data.GetProperty("historyPlate")
				.EnumerateArray().Select(x => new LicencePlateHistoryItem {
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

		string? responseBody = await ReadDrivingLicenceNegativePointHistory(p, ct);

		if (responseBody == null) {
			bool hasEnoughBalance = await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.DrivingLicenceNegativePoint, ct);
			if (!hasEnoughBalance) return new UResponse<DrivingLicenceNegativePointResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			UResponse<TagTxnErrorCodes> purchaseState = await walletService.Purchase(new WalletPurchaseParams { Tag = TagPurchase.DrivingLicenceNegativePoint, Token = p.Token }, ct);
			if (purchaseState.Result != TagTxnErrorCodes.Ok) return new UResponse<DrivingLicenceNegativePointResponse?>(null, purchaseState.Status, purchaseState.Message);

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<DrivingLicenceNegativePointResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			HttpResponseMessage? response = await httpClient.Post(
				"https://api-ithub.itsaaz.ir/api/v1/CarServices/DriversLicensePointsInquiry",
				new { licenseNo = p.DrivingLicenceNumber, nationalCode = p.NationalCode, cellphone = p.PhoneNumber },
				new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
			);
			if (response == null) return new UResponse<DrivingLicenceNegativePointResponse?>(null);

			responseBody = await response.Content.ReadAsStringAsync(ct);
			await CreateDrivingLicenceNegativePointHistory(responseBody, p, ct);
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagPurchase.DrivingLicenceNegativePoint, Token = p.Token }, ct);
		}

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetProperty("body");

		return new UResponse<DrivingLicenceNegativePointResponse?>(new DrivingLicenceNegativePointResponse {
			Allowable = data.GetStringOrNull("allowable") == "1",
			Point = data.GetStringOrNull("negPoint"),
			RuleId = data.GetStringOrNull("ruleId")
		});
	}

	public async Task<UResponse<IBanToBankAccountDetailResponse?>> IBanToBankAccountDetail(IBanToBankAccountDetailParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IBanToBankAccountDetailResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		string? responseBody = await ReadIBanToBankAccountDetailHistory(p, ct);

		if (responseBody == null) {
			bool hasEnoughBalance = await walletService.HasEnoughBalance(userData.Id, Core.App.ApiCallCosts.IBanToBankAccountDetail, ct);
			if (!hasEnoughBalance) return new UResponse<IBanToBankAccountDetailResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

			UResponse<TagTxnErrorCodes> purchaseState = await walletService.Purchase(new WalletPurchaseParams { Tag = TagPurchase.IBanToBankAccountDetail, Token = p.Token }, ct);
			if (purchaseState.Result != TagTxnErrorCodes.Ok) return new UResponse<IBanToBankAccountDetailResponse?>(null, purchaseState.Status, purchaseState.Message);

			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<IBanToBankAccountDetailResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			HttpResponseMessage? response = await httpClient.Post(
				"https://api-ithub.itsaaz.ir/api/v1/CarServices/DriversLicensePointsInquiry",
				new { iban = p.IBan },
				new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
			);
			if (response == null) return new UResponse<IBanToBankAccountDetailResponse?>(null);

			responseBody = await response.Content.ReadAsStringAsync(ct);
			await CreateIBanToBankAccountDetailHistory(responseBody, p, ct);
			await walletService.Purchase(new WalletPurchaseParams { Tag = TagPurchase.IBanToBankAccountDetail, Token = p.Token }, ct);
		}

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

	private async Task<bool?> ReadMobileAndNationalCodeVerificationHistory(string nationalCode, string phoneNumber, CancellationToken ct) {
		InquiryHistoryEntity? e = await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.NationalCode == nationalCode && x.PhoneNumber == phoneNumber, ct);
		return e?.Tags.Contains(TagInquiryHistory.Verified);
	}

	private async Task CreateZipCodeToAddressHistory(string responseBody, ZipCodeToAddressDetailParams p, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJsonData(),
			Tags = [TagInquiryHistory.ItHub, TagInquiryHistory.ZipCodeToAddressDetail, TagInquiryHistory.Verified],
			ZipCode = p.ZipCode,
			Response = responseBody
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task<string?> ReadZipCodeToAddressHistory(ZipCodeToAddressDetailParams p, CancellationToken ct) {
		InquiryHistoryEntity? e = await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.ZipCode == p.ZipCode && x.Tags.Contains(TagInquiryHistory.ZipCodeToAddressDetail), ct);
		return e?.Response;
	}

	private async Task CreateVehicleViolationsDetailHistory(string responseBody, VehicleViolationDetailParams p, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJsonData(),
			Tags = [TagInquiryHistory.ItHub, TagInquiryHistory.VehicleViolationsDetail],
			PhoneNumber = p.PhoneNumber,
			LicencePlate = p.LicencePlate,
			NationalCode = p.NationalCode,
			Response = responseBody
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task<string?> ReadVehicleViolationsDetailHistory(VehicleViolationDetailParams p, CancellationToken ct) {
		InquiryHistoryEntity? e = await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.PhoneNumber == p.PhoneNumber && x.LicencePlate == p.LicencePlate && x.NationalCode == p.NationalCode && x.Tags.Contains(TagInquiryHistory.VehicleViolationsDetail), ct);
		return e?.Response;
	}

	private async Task CreateDrivingLicenceStatusHistory(string responseBody, DrivingLicenceStatusParams p, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJsonData(),
			Tags = [TagInquiryHistory.ItHub, TagInquiryHistory.DrivingLicenceStatus],
			PhoneNumber = p.PhoneNumber,
			NationalCode = p.NationalCode,
			Response = responseBody
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task<string?> ReadDrivingLicenceStatusHistory(DrivingLicenceStatusParams p, CancellationToken ct) {
		InquiryHistoryEntity? e = await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.PhoneNumber == p.PhoneNumber && x.NationalCode == p.NationalCode && x.Tags.Contains(TagInquiryHistory.LicencePlateStatus), ct);
		return e?.Response;
	}

	private async Task CreateLicencePlateStatusHistory(string responseBody, LicencePlateInquiryParams p, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJsonData(),
			Tags = [TagInquiryHistory.ItHub, TagInquiryHistory.LicencePlateStatus],
			LicencePlate = p.LicencePlate,
			NationalCode = p.NationalCode,
			Response = responseBody
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task<string?> ReadLicencePlateStatusHistory(LicencePlateInquiryParams p, CancellationToken ct) {
		InquiryHistoryEntity? e = await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.LicencePlate == p.LicencePlate && x.NationalCode == p.NationalCode && x.Tags.Contains(TagInquiryHistory.LicencePlateStatus), ct);
		return e?.Response;
	}

	private async Task CreateDrivingLicenceNegativePointHistory(string responseBody, DrivingLicenceNegativePointParams p, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJsonData(),
			Tags = [TagInquiryHistory.ItHub, TagInquiryHistory.DrivingLicenceNegativePoint],
			NationalCode = p.NationalCode,
			PhoneNumber = p.PhoneNumber,
			DrivingLicenceNumber = p.DrivingLicenceNumber,
			Response = responseBody
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task<string?> ReadDrivingLicenceNegativePointHistory(DrivingLicenceNegativePointParams p, CancellationToken ct) {
		InquiryHistoryEntity? e = await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.DrivingLicenceNumber == p.DrivingLicenceNumber && x.NationalCode == p.NationalCode && x.PhoneNumber == p.PhoneNumber && x.Tags.Contains(TagInquiryHistory.DrivingLicenceNegativePoint), ct);
		return e?.Response;
	}

	private async Task CreateIBanToBankAccountDetailHistory(string responseBody, IBanToBankAccountDetailParams p, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJsonData(),
			Tags = [TagInquiryHistory.ItHub, TagInquiryHistory.DrivingLicenceNegativePoint],
			IBan = p.IBan,
			Response = responseBody
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task<string?> ReadIBanToBankAccountDetailHistory(IBanToBankAccountDetailParams p, CancellationToken ct) {
		InquiryHistoryEntity? e = await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.IBan == p.IBan && x.Tags.Contains(TagInquiryHistory.IBanToBankAccountDetail), ct);
		return e?.Response;
	}
	
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

		return new GetAccessTokenResponse {
			AccessToken = data.GetStringOrNull("access_token"),
			ExpiresIn = data.GetIntOrNull("expires_in")
		};
	}
}