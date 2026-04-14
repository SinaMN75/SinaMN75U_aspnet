namespace SinaMN75U.Services;

public interface IInquiryService {
	Task<UResponse<bool?>> Shahkar(VerifyNationalCodeAndPhoneNumber p, CancellationToken ct);
	Task<UResponse<PostalCodeToAddressDetailResponse?>> PostalCodeToAddressDetail(PostalCodeToAddressDetailParams p, CancellationToken ct);
	Task<UResponse<VehicleViolationDetailResponse?>> GetVehicleViolationsDetail(VehicleViolationDetailParams p, CancellationToken ct);
	Task<UResponse<DrivingLicenceStatusResponse?>> GetDrivingLicenceStatus(DrivingLicenceStatusParams p, CancellationToken ct);
	Task<UResponse<LicencePlateInquiryResponse?>> InquiryLicencePlate(LicencePlateInquiryParams p, CancellationToken ct);
	Task<UResponse<DrivingLicenceNegativePointResponse?>> DrivingLicenceNegativePoint(DrivingLicenceNegativePointParams p, CancellationToken ct);
}

public class InquiryService(
	DbContext db,
	IHttpClientService httpClient,
	ILocalizationService ls
) : IInquiryService {
	private readonly ItHub _itHub = Core.App.ItHub;

	public async Task<UResponse<bool?>> Shahkar(VerifyNationalCodeAndPhoneNumber p, CancellationToken ct) {
		bool? isRecordExist = await ReadShahkarHistory(p.NationalCode, p.Mobile, ct);
		if (isRecordExist != null) return new UResponse<bool?>(isRecordExist);

		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<bool?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			"https://gateway.itsaaz.ir/hub/api/v1/Shahkar/MixVerifyMobile",
			new { nationalCode = p.NationalCode, mobile = p.Mobile },
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" } }
		);
		if (response == null) return new UResponse<bool?>(null);

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		bool data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetBoolean();

		await CreateShahkarHistory(p.NationalCode, p.Mobile, data, ct);

		return new UResponse<bool?>(data);
	}

	public async Task<UResponse<PostalCodeToAddressDetailResponse?>> PostalCodeToAddressDetail(PostalCodeToAddressDetailParams p, CancellationToken ct) {
		string? responseBody = await ReadZipCodeToAddressHistory(p, ct);

		if (responseBody == null) {
			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<PostalCodeToAddressDetailResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));
			HttpResponseMessage? response = await httpClient.Post(
				"https://gateway.itsaaz.ir/hub/api/v1/Address/DetailsTypeA",
				new { postcode = p.ZipCode, orderId = 1 },
				new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
			);
			if (response == null) return new UResponse<PostalCodeToAddressDetailResponse?>(null);

			responseBody = await response.Content.ReadAsStringAsync(ct);
			await CreateZipCodeToAddressHistory(responseBody, p, ct);
		}

		JsonElement json = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data");

		PostalCodeToAddressDetailResponse data = new() {
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

		return new UResponse<PostalCodeToAddressDetailResponse?>(data);
	}

	public async Task<UResponse<VehicleViolationDetailResponse?>> GetVehicleViolationsDetail(VehicleViolationDetailParams p, CancellationToken ct) {
		string? responseBody = await ReadVehicleViolationsDetailHistory(p, ct);

		if (responseBody == null) {
			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<VehicleViolationDetailResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			HttpResponseMessage? response = await httpClient.Post(
				"https://api-ithub.itsaaz.ir/api/v1/CarServices/VehicleviolationsDetails",
				new {
					nationalCode = p.NationalCode,
					cellPhone = p.PhoneNumber,
					plk1 = p.LicencePlate.Substring(0, 2),
					plk2 = p.LicencePlate.Substring(2, 1),
					plk3 = p.LicencePlate.Substring(3, 3),
					plkSrl = p.LicencePlate.Substring(6, 2)
				},
				new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
			);
			if (response == null) return new UResponse<VehicleViolationDetailResponse?>(null);

			responseBody = await response.Content.ReadAsStringAsync(ct);
			Console.WriteLine("NNNNNNNNN");
			Console.WriteLine(responseBody);
			await CreateVehicleViolationsDetailHistory(responseBody, p, ct);
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

	public async Task<UResponse<DrivingLicenceStatusResponse?>> GetDrivingLicenceStatus(DrivingLicenceStatusParams p, CancellationToken ct) {
		string? responseBody = await ReadDrivingLicenceStatusHistory(p, ct);

		if (responseBody == null) {
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

	public async Task<UResponse<LicencePlateInquiryResponse?>> InquiryLicencePlate(LicencePlateInquiryParams p, CancellationToken ct) {
		string? responseBody = await ReadLicencePlateStatusHistory(p, ct);

		if (responseBody == null) {
			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<LicencePlateInquiryResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			HttpResponseMessage? response = await httpClient.Post(
				"https://api-ithub.itsaaz.ir/api/v1/CarServices/PlateHistoryInquiry",
				new {
					nationalCode = p.NationalCode,
					plk1 = p.LicencePlate.Substring(0, 2),
					plk2 = p.LicencePlate.Substring(2, 1),
					plk3 = p.LicencePlate.Substring(3, 3),
					plkSrl = p.LicencePlate.Substring(6, 2)
				},
				new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
			);
			if (response == null) return new UResponse<LicencePlateInquiryResponse?>(null);

			responseBody = await response.Content.ReadAsStringAsync(ct);
			await CreateLicencePlateStatusHistory(responseBody, p, ct);
		}

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetProperty("body");

		return new UResponse<LicencePlateInquiryResponse?>(new LicencePlateInquiryResponse {
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
		string? responseBody = await ReadDrivingLicenceNegativePointHistory(p, ct);

		if (responseBody == null) {
			GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
			if (tokenResponse?.AccessToken == null) return new UResponse<DrivingLicenceNegativePointResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

			HttpResponseMessage? response = await httpClient.Post(
				"https://api-ithub.itsaaz.ir/api/v1/CarServices/DriversLicensePointsInquiry",
				new {
					licenseNo = p.DrivingLicenceNumber,
					nationalCode = p.NationalCode,
					cellphone = p.PhoneNumber
				},
				new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
			);
			if (response == null) return new UResponse<DrivingLicenceNegativePointResponse?>(null);

			responseBody = await response.Content.ReadAsStringAsync(ct);
			await CreateDrivingLicenceNegativePointHistory(responseBody, p, ct);
		}

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetProperty("body");

		return new UResponse<DrivingLicenceNegativePointResponse?>(new DrivingLicenceNegativePointResponse {
			Allowable = data.GetStringOrNull("allowable") == "1",
			Point = data.GetStringOrNull("negPoint"),
			RuleId = data.GetStringOrNull("ruleId")
		});
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

	private async Task CreateShahkarHistory(string nationalCode, string phoneNumber, bool isVerified, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new GeneralJsonData { Description = "" },
			Tags = isVerified ? [TagInquiryHistory.ValidateNationalCodeAndPhoneNumber, TagInquiryHistory.Verified] : [TagInquiryHistory.ValidateNationalCodeAndPhoneNumber, TagInquiryHistory.NotVerified],
			NationalCode = nationalCode,
			PhoneNumber = phoneNumber,
			Response = ""
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task<bool?> ReadShahkarHistory(string nationalCode, string phoneNumber, CancellationToken ct) {
		InquiryHistoryEntity? e = await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.NationalCode == nationalCode && x.PhoneNumber == phoneNumber, ct);
		return e?.Tags.Contains(TagInquiryHistory.Verified);
	}

	private async Task CreateZipCodeToAddressHistory(string responseBody, PostalCodeToAddressDetailParams p, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new GeneralJsonData { Description = "" },
			Tags = [TagInquiryHistory.ItHub, TagInquiryHistory.ZipCodeToAddressDetail, TagInquiryHistory.Verified],
			ZipCode = p.ZipCode,
			Response = responseBody
		}, ct);
		await db.SaveChangesAsync(ct);
	}

	private async Task<string?> ReadZipCodeToAddressHistory(PostalCodeToAddressDetailParams p, CancellationToken ct) {
		InquiryHistoryEntity? e = await db.Set<InquiryHistoryEntity>().FirstOrDefaultAsync(x => x.ZipCode == p.ZipCode && x.Tags.Contains(TagInquiryHistory.ZipCodeToAddressDetail), ct);
		return e?.Response;
	}

	private async Task CreateVehicleViolationsDetailHistory(string responseBody, VehicleViolationDetailParams p, CancellationToken ct) {
		await db.Set<InquiryHistoryEntity>().AddAsync(new InquiryHistoryEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new GeneralJsonData { Description = "" },
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
			CreatedAt = DateTime.UtcNow,
			JsonData = new GeneralJsonData { Description = "" },
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
			CreatedAt = DateTime.UtcNow,
			JsonData = new GeneralJsonData { Description = "" },
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
			CreatedAt = DateTime.UtcNow,
			JsonData = new GeneralJsonData { Description = "" },
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
}