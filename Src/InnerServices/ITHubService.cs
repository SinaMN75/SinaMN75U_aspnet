namespace SinaMN75U.InnerServices;

public interface IITHubService {
	Task<ItHubBaseResponse<bool?>> Shahkar(ITHubShahkarParams p, CancellationToken ct);
	Task<ItHubBaseResponse<ItHubPostalCodeToAddressDetailResponse?>> PostalCodeToAddressDetail(PostalCodeToAddressDetailParams p, CancellationToken ct);
}

public class ITHubService(
	IHttpClientService httpClient,
	ILocalizationService ls
) : IITHubService {
	public async Task<ItHubBaseResponse<bool?>> Shahkar(ITHubShahkarParams p, CancellationToken ct) {
		ITHubGetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new ItHubBaseResponse<bool?> { Error = new Error { ErrorCode = 605, CustomMessage = ls.Get("ShahkarIsNotAvailableAtThisTime") } };

		HttpResponseMessage response = await httpClient.Post(
			"https://gateway.itsaaz.ir/hub/api/v1/Shahkar/MixVerifyMobile",
			new {
				nationalCode = p.NationalCode,
				mobile = p.Mobile
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" } }
		);

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		return responseBody.FromJson<ItHubBaseResponse<bool?>>();
	}

	public async Task<ItHubBaseResponse<ItHubPostalCodeToAddressDetailResponse?>> PostalCodeToAddressDetail(PostalCodeToAddressDetailParams p, CancellationToken ct) {
		ITHubGetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new ItHubBaseResponse<ItHubPostalCodeToAddressDetailResponse?> { Error = new Error { ErrorCode = 605, CustomMessage = ls.Get("ShahkarIsNotAvailableAtThisTime") } };

		HttpResponseMessage response = await httpClient.Post(
			"https://gateway.itsaaz.ir/hub/api/v1/Address/DetailsTypeA",
			new { postcode = p.PostCode, orderId = p.OrderId },
			new Dictionary<string, string> {
				{ "Authorization", $"Bearer {tokenResponse.AccessToken}" },
				{ "Accept", "application/json" }
			}
		);

		string responseBody = await response.Content.ReadAsStringAsync(ct);

		return responseBody.FromJson<ItHubBaseResponse<ItHubPostalCodeToAddressDetailResponse?>>();
	}

	private async Task<ITHubGetAccessTokenResponse?> GetAccessToken(CancellationToken ct) {
		ItHub itHub = Core.App.ItHub;
		HttpResponseMessage response = await httpClient.PostForm(
			"https://gateway.itsaaz.ir/sts/connect/token",
			new Dictionary<string, string> {
				{ "grant_type", "password" },
				{ "client_id", itHub.ClientId },
				{ "Client_secret", itHub.ClientSecret },
				{ "username", itHub.UserName },
				{ "password", itHub.Password }
			}
		);

		string responseBody = await response.Content.ReadAsStringAsync(ct);

		return JsonSerializer.Deserialize<ITHubGetAccessTokenResponse>(responseBody);
	}
}