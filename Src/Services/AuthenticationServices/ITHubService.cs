namespace SinaMN75U.Services.AuthenticationServices;

public class ITHubService(IHttpClientService httpClient) : IAuthenticationService {
	public async Task<UResponse<ITHubShahkarResponse>> Shahkar(ITHubShahkarParams p, CancellationToken ct) {
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

	public async Task<UResponse<ITHubPostalCodeToAddressDetailResponse>> PostalCodeToAddressDetail(ITHubPostalCodeToAddressDetailParams p, CancellationToken ct) {
		string responseBody = await httpClient.Post(
			uri: "https://gateway.itsaaz.ir/hub/api/v1/IdentityDataTypeA",
			body: new {
				postcode = p.PostCode,
				orderId = p.OrderId
			},
			headers: new Dictionary<string, string> { { "Content-Type", "application/x-www-form-urlencoded" } }
		);
		ITHubPostalCodeToAddressDetailResponse response = responseBody.FromJson<ITHubPostalCodeToAddressDetailResponse>();

		return new UResponse<ITHubPostalCodeToAddressDetailResponse>(response);
	}

	public async Task<UResponse<ITHubGetAccessTokenResponse>> GetAccessToken(ITHubGetAccessTokenParams p, CancellationToken ct) {
		string responseBody = await httpClient.PostForm(
			uri: "https://gateway.itsaaz.ir/sts/connect/token",
			formData: new Dictionary<string, string> {
				{ "grant_type", "password" },
				{ "client_id", "test" },
				{ "Client_secret", "test" },
				{ "username", "test" },
				{ "password", "test" }
			}
		);

		ITHubGetAccessTokenResponse? response = JsonSerializer.Deserialize<ITHubGetAccessTokenResponse>(responseBody);

		return new UResponse<ITHubGetAccessTokenResponse>(response);
	}

	public async Task<UResponse> GetManagementAccessToken(ITHubGetManagementAccessTokenParams p, CancellationToken ct) {
		await httpClient.Post(
			uri: "https://gateway.itsaaz.ir/management/api/v1/LegalPerson/AuthorizeByUserPassApi",
			body: new {
				username = p.UserName,
				password = p.Password
			},
			headers: new Dictionary<string, string> { { "Content-Type", "application/x-www-form-urlencoded" } }
		);
		return new UResponse();
	}

	public async Task<UResponse> RefreshManagementAccessToken(ITHubRefreshManagementAccessTokenParams p, CancellationToken ct) {
		await httpClient.Post(
			uri: "https://gateway.itsaaz.ir/management/api/v1/LegalPerson/RenewToken",
			body: new {
				refreshToken = p.RefreshToken
			},
			headers: new Dictionary<string, string> { { "Content-Type", "application/x-www-form-urlencoded" } }
		);
		return new UResponse();
	}
}