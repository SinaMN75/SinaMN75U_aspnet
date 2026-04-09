namespace SinaMN75U.Services;

public interface IInquiryService {
	Task<UResponse<bool?>> Shahkar(ITHubShahkarParams p, CancellationToken ct);
	Task<UResponse<PostalCodeToAddressDetailResponse?>> PostalCodeToAddressDetail(PostalCodeToAddressDetailParams p, CancellationToken ct);
}

public class InquiryService(
	IHttpClientService httpClient,
	ILocalizationService ls
) : IInquiryService {
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

	private async Task<GetAccessTokenResponse?> GetAccessToken(CancellationToken ct) {
		ItHub itHub = Core.App.ItHub;
		HttpResponseMessage? response = await httpClient.PostForm(
			"https://gateway.itsaaz.ir/sts/connect/token",
			new Dictionary<string, string> {
				{ "grant_type", "password" },
				{ "client_id", itHub.ClientId },
				{ "Client_secret", itHub.ClientSecret },
				{ "username", itHub.UserName },
				{ "password", itHub.Password }
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