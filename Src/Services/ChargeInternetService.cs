namespace SinaMN75U.Services;

public interface IChargeInternetService {
	Task<UResponse<ChargeInternetReserveResponse?>> Pin(ReserveChargeParams p, CancellationToken ct);
	Task<UResponse<ChargeInternetReserveResponse?>> Topup(TopupChargeParams p, CancellationToken ct);
	Task<UResponse> InternetList(TopupChargeParams p, CancellationToken ct);
}

public class ChargeInternetService(
	IHttpClientService httpClient,
	ILocalizationService ls,
	ITokenService ts
) : IChargeInternetService {
	private async Task<GetAccessTokenResponse?> GetAccessToken(CancellationToken ct) {
		HttpResponseMessage? response = await httpClient.Post(
			uri: $"{Core.App.Mobtakeran.BaseUrl}api/v2/login",
			body: new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Guid.NewGuid().ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
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
			Reserve = data.GetStringOrNull("reserve"),
			Status = data.GetBoolOrNull("status"),
			Code = data.GetIntOrNull("code"),
			Message = data.GetStringOrNull("message"),
		};
	}

	public async Task<UResponse<ChargeInternetReserveResponse?>> Pin(ReserveChargeParams p, CancellationToken ct) {
		// JwtClaimData? userData = ts.ExtractClaims(p.Token);
		// if (userData == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Pin/Reserve",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Guid.NewGuid().ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new {
					amount = p.Amount,
					operator_id = ((int)p.SimType).ToString(),
					device = "05"
				}
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);
		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<ChargeInternetReserveResponse?>(null);

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody);
		JsonElement attachment = data.GetProperty("attachments");

		return new UResponse<ChargeInternetReserveResponse?>(new ChargeInternetReserveResponse {
			Reserve = data.GetIntOrNull("reserve"),
			ServerDateTime = data.GetStringOrNull("serverDateTime"),
			Status = data.GetBoolOrNull("status"),
			Code = data.GetIntOrNull("code"),
			Message = data.GetStringOrNull("message"),
			Reference = attachment.GetStringOrNull("reference"),
			TraceId = attachment.GetStringOrNull("trace_id"),
			AffectiveAmount = attachment.GetIntOrNull("affective_amount"),
			Help = attachment.GetStringOrNull("help"),
			MessageSource = attachment.GetStringOrNull("message_source"),
		});
	}

	public async Task<UResponse<ChargeInternetReserveResponse?>> Topup(TopupChargeParams p, CancellationToken ct) {
		// JwtClaimData? userData = ts.ExtractClaims(p.Token);
		// if (userData == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Topup/Pin",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Guid.NewGuid().ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new {
					subscriber = p.PhoneNumber,
					amount = p.Amount,
					operator_id = ((int)p.SimType).ToString(),
					device = "05",
					type = "0"
				}
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);
		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<ChargeInternetReserveResponse?>(null);

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody);
		JsonElement attachment = data.GetProperty("attachments");

		return new UResponse<ChargeInternetReserveResponse?>(new ChargeInternetReserveResponse {
			Reserve = data.GetIntOrNull("reserve"),
			ServerDateTime = data.GetStringOrNull("serverDateTime"),
			Status = data.GetBoolOrNull("status"),
			Code = data.GetIntOrNull("code"),
			Message = data.GetStringOrNull("message"),
			Reference = attachment.GetStringOrNull("reference"),
			TraceId = attachment.GetStringOrNull("trace_id"),
			AffectiveAmount = attachment.GetIntOrNull("affective_amount"),
			Help = attachment.GetStringOrNull("help"),
			MessageSource = attachment.GetStringOrNull("message_source"),
		});
	}

	public async Task<UResponse> InternetList(TopupChargeParams p, CancellationToken ct) {
		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Internet/getlist",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Guid.NewGuid().ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new {
					operator_id = ((int)p.SimType).ToString(),
				}
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);

		return new UResponse();
	}
}