namespace SinaMN75U.Services;

public interface IInquiryService {
	Task<UResponse<bool?>> Shahkar(ITHubShahkarParams p, CancellationToken ct);
	Task<UResponse<PostalCodeToAddressDetailResponse?>> PostalCodeToAddressDetail(PostalCodeToAddressDetailParams p, CancellationToken ct);
	Task<UResponse<VehicleViolationDetailResponse?>> GetVehicleViolationsDetail(VehicleViolationDetailParams p, CancellationToken ct);
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

		// HttpResponseMessage? response = await httpClient.Post(
		// 	"https://api-ithub.itsaaz.ir/api/v1/CarServices/VehicleviolationsDetails",
		// 	new {
		// 		nationalCode = p.NationalCode,
		// 		cellPhone = p.PhoneNumber,
		// 		plk1 = p.LicencePlate.Substring(0, 2),
		// 		plk2 = p.LicencePlate.Substring(2, 1),
		// 		plk3 = p.LicencePlate.Substring(3, 3),
		// 		plkSrl = p.LicencePlate.Substring(6, 2),
		// 		orderId = 1
		// 	},
		// 	new Dictionary<string, string> {
		// 		{ "Authorization", $"Bearer {tokenResponse.AccessToken}" },
		// 		{ "Accept", "application/json" }
		// 	}
		// );
		// if (response == null) return new UResponse<PostalCodeToAddressDetailResponse?>(null);

		// string responseBody = await response.Content.ReadAsStringAsync(ct);
		// JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data");


		const string responseBody =
			"{\n  \"data\": {\n    \"body\": {\n      \"errorCode\": \"0\",\n      \"errorDesc\": \"[NAJI-SERVICE][ErrCode:0]\",\n      \"warningDTOs\": [\n        {\n          \"serialNo\": \"042730115525\",\n          \"violationOccureDate\": \"1404/06/09 - 08:50\",\n          \"violationOccureTime\": \"08:50\",\n          \"violationDeliveryType\": {\n            \"violationDeliveryType\": \"دوربين\"\n          },\n          \"violatoinAddress\": \"تهران پل صدر ب از قيطريه غرب-شرق\",\n          \"violationTypeDTO\": {\n            \"violationTypeId\": \"2056\",\n            \"violationType\": \"تجاوز از سرعت مجاز تا 10 کيلومتر\\r\\n\"\n          },\n          \"finalPrice\": \"1000000\",\n          \"paperId\": \"3011552500299\",\n          \"paymentId\": \"100016414\",\n          \"hasImage\": \"1\",\n          \"warningId\": \"2490094854\",\n          \"investigationAbility\": \"0\"\n        },\n        {\n          \"serialNo\": \"042740234194\",\n          \"violationOccureDate\": \"1404/07/01 - 11:51\",\n          \"violationOccureTime\": \"11:51\",\n          \"violationDeliveryType\": {\n            \"violationDeliveryType\": \"دوربين\"\n          },\n          \"violatoinAddress\": \"اصفهان خ استانداري خروجي از فرشادي\",\n          \"violationTypeDTO\": {\n            \"violationTypeId\": \"2126\",\n            \"violationType\": \"تردد غير مجاز وسايل نقليه در محدوده هاي ممنوع مصوب\"\n          },\n          \"finalPrice\": \"2000000\",\n          \"paperId\": \"4023419400291\",\n          \"paymentId\": \"200018793\",\n          \"hasImage\": \"1\",\n          \"warningId\": \"2504306337\",\n          \"investigationAbility\": \"0\"\n        },\n        {\n          \"serialNo\": \"046171554342\",\n          \"violationOccureDate\": \"1404/07/07 - 14:00\",\n          \"violationOccureTime\": \"14:00\",\n          \"violationDeliveryType\": {\n            \"violationDeliveryType\": \"الصاقي\"\n          },\n          \"violatoinAddress\": \"مناطق تهران تهرانسر\",\n          \"violationTypeDTO\": {\n            \"violationTypeId\": \"2085\",\n            \"violationType\": \"توقف دوبله در معابر\"\n          },\n          \"finalPrice\": \"1800000\",\n          \"paperId\": \"7155434200295\",\n          \"paymentId\": \"180000327\",\n          \"hasImage\": \"0\",\n          \"warningId\": \"2509328404\",\n          \"investigationAbility\": \"0\"\n        },\n        {\n          \"serialNo\": \"042818093129\",\n          \"violationOccureDate\": \"1404/11/04 - 22:14\",\n          \"violationOccureTime\": \"22:14\",\n          \"violationDeliveryType\": {\n            \"violationDeliveryType\": \"دوربين\"\n          },\n          \"violatoinAddress\": \"تهران تهران-يادگار امام قبل ازحکيم جنوب به شمال\",\n          \"violationTypeDTO\": {\n            \"violationTypeId\": \"2056\",\n            \"violationType\": \"تجاوز از سرعت مجاز تا 10 کيلومتر\\r\\n\"\n          },\n          \"finalPrice\": \"1000000\",\n          \"paperId\": \"1809312900294\",\n          \"paymentId\": \"100000332\",\n          \"hasImage\": \"1\",\n          \"warningId\": \"2596786479\",\n          \"investigationAbility\": \"0\"\n        },\n        {\n          \"serialNo\": \"052850358423\",\n          \"violationOccureDate\": \"1405/01/05 - 17:48\",\n          \"violationOccureTime\": \"17:48\",\n          \"violationDeliveryType\": {\n            \"violationDeliveryType\": \"دوربين\"\n          },\n          \"violatoinAddress\": \"قم قم تهران ك 72\",\n          \"violationTypeDTO\": {\n            \"violationTypeId\": \"2056\",\n            \"violationType\": \"تجاوز از سرعت مجاز تا 10 کيلومتر\\r\\n\"\n          },\n          \"finalPrice\": \"1333000\",\n          \"paperId\": \"5035842300297\",\n          \"paymentId\": \"133302042\",\n          \"hasImage\": \"1\",\n          \"warningId\": \"2633918319\",\n          \"investigationAbility\": \"1\"\n        },\n        {\n          \"serialNo\": \"052851846871\",\n          \"violationOccureDate\": \"1405/01/13 - 18:13\",\n          \"violationOccureTime\": \"18:13\",\n          \"violationDeliveryType\": {\n            \"violationDeliveryType\": \"دوربين\"\n          },\n          \"violatoinAddress\": \"اصفهان اصفهان قم ك41455 كقم\",\n          \"violationTypeDTO\": {\n            \"violationTypeId\": \"2056\",\n            \"violationType\": \"تجاوز از سرعت مجاز تا 10 کيلومتر\\r\\n\"\n          },\n          \"finalPrice\": \"1333000\",\n          \"paperId\": \"5184687100290\",\n          \"paymentId\": \"133302042\",\n          \"hasImage\": \"1\",\n          \"warningId\": \"2636199453\",\n          \"investigationAbility\": \"1\"\n        }\n      ],\n      \"plateDictation\": \"چهل ودو ب پانصد وهفتاد وچهار -  ايران پنجاه \",\n      \"plateChar\": \" شخصي  ايران 50 ــ  574ب42\",\n      \"complaintStatus\": \"0\",\n      \"complaint\": \"شکایت ندارد\",\n      \"parmDate\": \"1405/01/20\",\n      \"sysDate\": \"1405/01/20\",\n      \"sysTime\": \"19:26\",\n      \"priceStatus\": \"1\",\n      \"pageCount\": 1.0,\n      \"traceId\": \"444428926\",\n      \"paperId\": \"6581150300198\",\n      \"paymentId\": \"846602056\",\n      \"warningPrice\": \"8466000\",\n      \"inquirePrice\": \"8466000\",\n      \"ejrInquireNo\": \"5265811503\",\n      \"warningId\": \"0\",\n      \"inquirePriceDictation\": \"هشت  ميليون و چهارصد و شصت و  شش  هزار\"\n    }\n  },\n  \"meta\": null,\n  \"error\": null\n}";
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