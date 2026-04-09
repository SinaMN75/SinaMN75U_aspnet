namespace SinaMN75U.Services;

public interface IInquiryService {
	Task<UResponse<bool?>> Shahkar(ITHubShahkarParams p, CancellationToken ct);
	Task<UResponse<PostalCodeToAddressDetailResponse?>> PostalCodeToAddressDetail(PostalCodeToAddressDetailParams p, CancellationToken ct);
	Task<UResponse<VehicleViolationDetailResponse?>> GetVehicleViolationsDetail(VehicleViolationDetailParams p, CancellationToken ct);
	Task<UResponse<DrivingLicenceStatusResponse?>> GetDrivingLicenceStatus(DrivingLicenceStatusParams p, CancellationToken ct);
}

public class InquiryService(
	IHttpClientService httpClient,
	ILocalizationService ls
) : IInquiryService {
	private readonly ItHub _itHub = Core.App.ItHub;

	public async Task<UResponse<bool?>> Shahkar(ITHubShahkarParams p, CancellationToken ct) {
		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<bool?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			"https://gateway.itsaaz.ir/hub/api/v1/Shahkar/MixVerifyMobile",
			new { nationalCode = p.NationalCode, mobile = p.Mobile },
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" } }
		);
		if (response == null) return new UResponse<bool?>(null);

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		return new UResponse<bool?>(JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data").GetBoolean());
	}

	public async Task<UResponse<PostalCodeToAddressDetailResponse?>> PostalCodeToAddressDetail(PostalCodeToAddressDetailParams p, CancellationToken ct) {
		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<PostalCodeToAddressDetailResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			"https://gateway.itsaaz.ir/hub/api/v1/Address/DetailsTypeA",
			new { postcode = p.ZipCode, orderId = 1 },
			new Dictionary<string, string> {
				{ "Authorization", $"Bearer {tokenResponse.AccessToken}" },
				{ "Accept", "application/json" }
			}
		);
		if (response == null) return new UResponse<PostalCodeToAddressDetailResponse?>(null);

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data");

		return new UResponse<PostalCodeToAddressDetailResponse?>(new PostalCodeToAddressDetailResponse {
			BuildingName = data.GetStringOrNull("BuildingName"),
			Description = data.GetStringOrNull("description"),
			Floor = data.GetStringOrNull("floor"),
			HouseNumber = data.GetStringOrNull("houseNumber"),
			LocalityName = data.GetStringOrNull("localityName"),
			LocalityType = data.GetStringOrNull("localityType"),
			ZipCode = data.GetStringOrNull("zipCode"),
			Province = data.GetStringOrNull("province"),
			SideFloor = data.GetStringOrNull("sideFloor"),
			Street = data.GetStringOrNull("street"),
			Street2 = data.GetStringOrNull("street2"),
			SubLocality = data.GetStringOrNull("subLocality"),
			TownShip = data.GetStringOrNull("townShip"),
			TraceId = data.GetStringOrNull("traceId"),
			Village = data.GetStringOrNull("village"),
			LocalityCode = data.GetIntOrNull("localityCode")
		});
	}

	public async Task<UResponse<VehicleViolationDetailResponse?>> GetVehicleViolationsDetail(VehicleViolationDetailParams p, CancellationToken ct) {
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
				plkSrl = p.LicencePlate.Substring(6, 2),
				orderId = 1
			},
			new Dictionary<string, string> {
				{ "Authorization", $"Bearer {tokenResponse.AccessToken}" },
				{ "Accept", "application/json" }
			}
		);
		if (response == null) return new UResponse<VehicleViolationDetailResponse?>(null);

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data");

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
					ViolationOccureDate = x.GetStringOrNull("violationOccureDate"),
					Type = x.TryGetProperty("violationDeliveryType", out var vdt)
						? vdt.GetStringOrNull("violationDeliveryType")
						: null,
					Address = x.GetStringOrNull("violatoinAddress"),
					ViolationType = x.TryGetProperty("violationTypeDTO", out var vtd)
						? vtd.GetStringOrNull("violationType")
						: null,
					FinalPrice = x.GetStringOrNull("finalPrice"),
					PaperId = x.GetStringOrNull("paperId"),
					PaymentId = x.GetStringOrNull("paymentId"),
					WarningId = x.GetStringOrNull("warningId"),
					InvestigationAbility = x.GetStringOrNull("investigationAbility"),
					HasImage = x.GetStringOrNull("hasImage") == "1"
				})
				.ToList()
		});
	}

	public async Task<UResponse<DrivingLicenceStatusResponse?>> GetDrivingLicenceStatus(DrivingLicenceStatusParams p, CancellationToken ct) {
		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<DrivingLicenceStatusResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			"https://gateway.itsaaz.ir/hub/api/v1/CarServices/GavahinameStatusInquiry",
			new { nationalCode = p.NationalCode, cellphone = p.PhoneNumber },
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);
		if (response == null) return new UResponse<DrivingLicenceStatusResponse?>(null);

		string responseBody = await response.Content.ReadAsStringAsync(ct);

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
			ValidYears = data.GetStringOrNull("validYears"),
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
}