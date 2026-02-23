namespace SinaMN75U.Services;

public interface IITHubService {
	Task<UResponse<ITHubShahkarResponse?>> Shahkar(ITHubShahkarParams p, CancellationToken ct);
	Task<UResponse<ItHubPostalCodeToAddressDetailResponse?>> PostalCodeToAddressDetail(PostalCodeToAddressDetailParams p, CancellationToken ct);
}

public class ITHubService(IHttpClientService httpClient) : IITHubService {
	public async Task<UResponse<ITHubShahkarResponse?>> Shahkar(ITHubShahkarParams p, CancellationToken ct) {
		ITHubGetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<ITHubShahkarResponse?>(null, Usc.BadRequest, "توکن اعتبار ندارد.");

		HttpResponseMessage response = await httpClient.Post(
			uri: "https://gateway.itsaaz.ir/hub/api/v1/Shahkar/MixVerifyMobile",
			body: new {
				nationalCode = p.NationalCode,
				mobile = p.Mobile
			},
			headers: new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" } }
		);

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		Console.WriteLine("KLKLKLKLK");
		Console.WriteLine(responseBody);
		return new UResponse<ITHubShahkarResponse?>(responseBody.FromJson<ITHubShahkarResponse>());
	}

	public async Task<UResponse<ItHubPostalCodeToAddressDetailResponse?>> PostalCodeToAddressDetail(PostalCodeToAddressDetailParams p, CancellationToken ct) {
		ITHubGetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<ItHubPostalCodeToAddressDetailResponse?>(null, Usc.BadRequest, "توکن اعتبار ندارد.");

		var requestBody = new {
			postcode = p.PostCode,
			orderId = p.OrderId
		};

		Dictionary<string, string> headers = new() {
			{ "Authorization", $"Bearer {tokenResponse.AccessToken}" },
			{ "Accept", "application/json" }
		};

		HttpResponseMessage response = await httpClient.Post(
			uri: "https://gateway.itsaaz.ir/hub/api/v1/Address/DetailsTypeA",
			body: requestBody,
			headers: headers
		);

		string responseBody = await response.Content.ReadAsStringAsync(ct);

		ItHubBaseResponse<ItHubPostalCodeToAddressDetailResponse>? apiResponse = JsonSerializer.Deserialize<ItHubBaseResponse<ItHubPostalCodeToAddressDetailResponse>>(responseBody);
		return new UResponse<ItHubPostalCodeToAddressDetailResponse?>(apiResponse?.Data, message: apiResponse?.Error ?? "");
	}

	private async Task<ITHubGetAccessTokenResponse?> GetAccessToken(CancellationToken ct) {
		ItHub itHub = AppSettings.Instance.ItHub;
		HttpResponseMessage response = await httpClient.PostForm(
			uri: "https://gateway.itsaaz.ir/sts/connect/token",
			formData: new Dictionary<string, string> {
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