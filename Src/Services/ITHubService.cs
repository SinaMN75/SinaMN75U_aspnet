namespace SinaMN75U.Services;

public interface IITHubService {
	Task<ITHubGetAccessTokenResponse?> GetAccessToken(CancellationToken ct);
	Task<UResponse<ITHubShahkarResponse>?> Shahkar(ITHubShahkarParams p, CancellationToken ct);
	Task<UResponse<ItHubPostalCodeToAddressDetailResponse?>> PostalCodeToAddressDetail(PostalCodeToAddressDetailParams p, CancellationToken ct);
}

public class ITHubService(IHttpClientService httpClient) : IITHubService {
	public async Task<UResponse<ITHubShahkarResponse>?> Shahkar(ITHubShahkarParams p, CancellationToken ct) {
		string responseBody = await httpClient.Post(
			uri: "https://gateway.itsaaz.ir/hub/api/v1/Shahkar/MixVerifyMobile",
			body: new {
				nationalCode = p.NationalCode,
				mobile = p.Mobile
			},
			headers: new Dictionary<string, string> { { "Content-Type", "application/x-www-form-urlencoded" } }
		);
		ITHubShahkarResponse response = responseBody.FromJson<ITHubShahkarResponse>();

		return new UResponse<ITHubShahkarResponse>(response);
	}

	public async Task<UResponse<ItHubPostalCodeToAddressDetailResponse?>> PostalCodeToAddressDetail(PostalCodeToAddressDetailParams p, CancellationToken ct) {
		ITHubGetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) {
			return new UResponse<ItHubPostalCodeToAddressDetailResponse?>(null, Usc.BadRequest);
		}

		var requestBody = new {
			postcode = p.PostCode,
			orderId = p.OrderId
		};

		Dictionary<string, string> headers = new Dictionary<string, string> {
			{ "Authorization", $"Bearer {tokenResponse.AccessToken}" },
			{ "Accept", "application/json" }
		};

		try {
			string responseBody = await httpClient.Post(
				uri: "https://gateway.itsaaz.ir/hub/api/v1/Address/DetailsTypeA",
				body: requestBody,
				headers: headers
			);

			ItHubBaseResponse<ItHubPostalCodeToAddressDetailResponse>? apiResponse = JsonSerializer.Deserialize<ItHubBaseResponse<ItHubPostalCodeToAddressDetailResponse>>(responseBody);

			if (apiResponse == null) {
				return new UResponse<ItHubPostalCodeToAddressDetailResponse?>(null, Usc.BadRequest);
			}

			if (apiResponse.Error != null) {
				return new UResponse<ItHubPostalCodeToAddressDetailResponse?>(null, Usc.BadRequest);
			}

			return new UResponse<ItHubPostalCodeToAddressDetailResponse?>(apiResponse.Data);
		}
		catch {
			return new UResponse<ItHubPostalCodeToAddressDetailResponse?>(null, Usc.BadRequest);
		}
	}

	public async Task<ITHubGetAccessTokenResponse?> GetAccessToken(CancellationToken ct) {
		ItHub itHub = AppSettings.Instance.ItHub;
		string responseBody = await httpClient.PostForm(
			uri: "https://gateway.itsaaz.ir/sts/connect/token",
			formData: new Dictionary<string, string> {
				{ "grant_type", "password" },
				{ "client_id", itHub.ClientId },
				{ "Client_secret", itHub.ClientSecret },
				{ "username", itHub.UserName },
				{ "password", itHub.Password }
			}
		);

		return JsonSerializer.Deserialize<ITHubGetAccessTokenResponse>(responseBody);
	}
}