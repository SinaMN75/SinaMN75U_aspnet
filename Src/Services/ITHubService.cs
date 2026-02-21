namespace SinaMN75U.Services;

public interface IITHubService {
	Task<UResponse<ITHubShahkarResponse>?> Shahkar(ITHubShahkarParams p, CancellationToken ct);
	Task<UResponse<ItHubPostalCodeToAddressDetailResponse?>> PostalCodeToAddressDetail(PostalCodeToAddressDetailParams p, CancellationToken ct);
}

public class ITHubService(IHttpClientService httpClient) : IITHubService {
	public async Task<UResponse<ITHubShahkarResponse>?> Shahkar(ITHubShahkarParams p, CancellationToken ct) {
		string responseBody = await httpClient.PostForm(
			uri: "https://gateway.itsaaz.ir/hub/api/v1/Shahkar/MixVerifyMobile",
			formData: new Dictionary<string, string> {
				{ "nationalCode", p.NationalCode },
				{ "mobile", p.Mobile }
			}
		);
		
		ITHubShahkarResponse response = responseBody.FromJson<ITHubShahkarResponse>();

		return new UResponse<ITHubShahkarResponse>(response);
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

		string responseBody = await httpClient.Post(
			uri: "https://gateway.itsaaz.ir/hub/api/v1/Address/DetailsTypeA",
			body: requestBody,
			headers: headers
		);
		
		ItHubBaseResponse<ItHubPostalCodeToAddressDetailResponse>? apiResponse = JsonSerializer.Deserialize<ItHubBaseResponse<ItHubPostalCodeToAddressDetailResponse>>(responseBody);
		return new UResponse<ItHubPostalCodeToAddressDetailResponse?>(apiResponse?.Data);
	}

	private async Task<ITHubGetAccessTokenResponse?> GetAccessToken(CancellationToken ct) {
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