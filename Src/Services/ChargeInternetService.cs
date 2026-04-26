namespace SinaMN75U.Services;

public class IChargeInternetService {
	
	
}

public class ChargeInternetService(IHttpClientService httpClient) {
	private async Task<GetAccessTokenResponse?> GetAccessToken(CancellationToken ct) {
		HttpResponseMessage? response = await httpClient.Post(
			uri: $"{Core.App.Mobtakeran.BaseUrl}/api/v2/login",
			body: new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = "",
				localDateTime = "",
				attachments = new {
					username = Core.App.Mobtakeran.UserName,
					password = Core.App.Mobtakeran.Password
				}
			}
		);
		if (response == null) return null;

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("attachments");

		return new GetAccessTokenResponse {
			AccessToken = data.GetStringOrNull("token"),
			ExpiresIn = 100
		};
	}
}