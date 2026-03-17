namespace SinaMN75U.Services;

public interface IInquiryService {
	Task<UResponse<bool?>> Shahkar(ITHubShahkarParams p, CancellationToken ct);
	Task<UResponse<ItHubPostalCodeToAddressDetailBaseResponse?>> PostalCodeToAddressDetail(PostalCodeToAddressDetailParams p, CancellationToken ct);
}

public class InquiryService(
	IHttpClientService httpClient,
	ILocalizationService ls
) : IInquiryService {
	public async Task<UResponse<bool?>> Shahkar(ITHubShahkarParams p, CancellationToken ct) {
		ITHubGetAccessTokenBaseResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.Data?.AccessToken == null) return new UResponse<bool?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage response = await httpClient.Post(
			"https://gateway.itsaaz.ir/hub/api/v1/Shahkar/MixVerifyMobile",
			new { nationalCode = p.NationalCode, mobile = p.Mobile },
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.Data?.AccessToken}" } }
		);

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		return new UResponse<bool?>(responseBody.FromJson<ITHubShahkarBaseResponse>().Data);
	}

	public async Task<UResponse<ItHubPostalCodeToAddressDetailBaseResponse?>> PostalCodeToAddressDetail(PostalCodeToAddressDetailParams p, CancellationToken ct) {
		ITHubGetAccessTokenBaseResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.Data?.AccessToken == null) return new UResponse<ItHubPostalCodeToAddressDetailBaseResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage response = await httpClient.Post(
			"https://gateway.itsaaz.ir/hub/api/v1/Address/DetailsTypeA",
			new { postcode = p.PostCode, orderId = p.OrderId },
			new Dictionary<string, string> {
				{ "Authorization", $"Bearer {tokenResponse.Data.AccessToken}" },
				{ "Accept", "application/json" }
			}
		);

		string responseBody = await response.Content.ReadAsStringAsync(ct);

		return new UResponse<ItHubPostalCodeToAddressDetailBaseResponse?>(responseBody.FromJson<ItHubPostalCodeToAddressDetailBaseResponse>());
	}

	private async Task<ITHubGetAccessTokenBaseResponse?> GetAccessToken(CancellationToken ct) {
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

		return JsonSerializer.Deserialize<ITHubGetAccessTokenBaseResponse>(responseBody);
	}
}