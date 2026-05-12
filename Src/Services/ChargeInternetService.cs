namespace SinaMN75U.Services;

public interface IChargeInternetService {
	Task<UResponse<ChargeInternetReserveResponse?>> Pin(ReserveChargeParams p, CancellationToken ct);
	Task<UResponse<ChargeInternetReserveResponse?>> Topup(TopupChargeParams p, CancellationToken ct);
	Task<UResponse<InternetPackageResponse?>> InternetList(InternetListParams p, CancellationToken ct);

	Task<UResponse<GetStatusResponse?>> GetStatus(GetStatusParams p, CancellationToken ct);
	Task<UResponse<GetBalanceResponse?>> GetBalance(CancellationToken ct);
	Task<UResponse<EchoResponse?>> Echo(CancellationToken ct);
	Task<UResponse<InternetPackageResponse?>> MCITopOffer(MCITopOfferParams p, CancellationToken ct);
	Task<UResponse<ChargeInternetReserveResponse?>> InternetReserve(InternetReserveParams p, CancellationToken ct);
}

public class ChargeInternetService(
	IHttpClientService httpClient,
	ILocalizationService ls,
	ITokenService ts,
	IWalletService walletService
) : IChargeInternetService {
	public async Task<UResponse<ChargeInternetReserveResponse?>> Pin(ReserveChargeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!await walletService.HasEnoughBalance(userData.Id, p.Amount.ToDecimal(), ct)) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));
		
		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Pin/Reserve",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new {
					amount = p.Amount,
					operator_id = p.SimType,
					device = "05"
				}
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);
		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<ChargeInternetReserveResponse?>(null);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		JsonElement attachment = data.GetProperty("attachments");

		ApproveResponse? approveResponse = await Approve(new ApproveParams {
			ApiKey = p.ApiKey,
			Token = p.Token,
			Reference = attachment.GetStringOrNull("reference")!,
			CardNumber = null,
			NationalCode = userData.NationalCode
		}, ct);

		if (approveResponse?.Pin != null) await walletService.Purchase(new WalletPurchaseParams { ApiKey = p.ApiKey, Token = p.Token, Tag = TagWalletTxn.ChargeSimPin }, ct);

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
			Pin = approveResponse?.Pin
		});
	}

	public async Task<UResponse<ChargeInternetReserveResponse?>> Topup(TopupChargeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!await walletService.HasEnoughBalance(userData.Id, p.Amount.ToDecimal(), ct)) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Topup/Reserve",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new { subscriber = p.PhoneNumber, amount = p.Amount, operator_id = p.OperatorId, device = "05", type = "0" }
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);
		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<ChargeInternetReserveResponse?>(null);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		JsonElement attachment = data.GetProperty("attachments");

		await Approve(new ApproveParams {
			ApiKey = p.ApiKey,
			Token = p.Token,
			Reference = attachment.GetStringOrNull("reference")!,
			CardNumber = null,
			NationalCode = userData.NationalCode
		}, ct);

		await walletService.Purchase(new WalletPurchaseParams { ApiKey = p.ApiKey, Token = p.Token, Tag = TagWalletTxn.ChargeSimTopup }, ct);

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
			MessageSource = attachment.GetStringOrNull("message_source")
		});
	}

	public async Task<UResponse<ChargeInternetReserveResponse?>> InternetReserve(InternetReserveParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!await walletService.HasEnoughBalance(userData.Id, p.Amount.ToDecimal(), ct)) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Internet/Reserve",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new { subscriber = p.Subscriber, operator_id = p.OperatorId, package_id = p.PackageId, amount = p.Amount, device = p.Device, bank = p.Bank }
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);

		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<ChargeInternetReserveResponse?>(null);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		JsonElement attachment = data.GetProperty("attachments");

		await Approve(new ApproveParams {
			ApiKey = p.ApiKey,
			Token = p.Token,
			Reference = attachment.GetStringOrNull("reference")!,
			CardNumber = null,
			NationalCode = userData.NationalCode
		}, ct);

		await walletService.Purchase(new WalletPurchaseParams { ApiKey = p.ApiKey, Token = p.Token, Tag = TagWalletTxn.InternetSim }, ct);

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
			MessageSource = attachment.GetStringOrNull("message_source")
		});
	}

	public async Task<UResponse<InternetPackageResponse?>> InternetList(InternetListParams p, CancellationToken ct) {
		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<InternetPackageResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Internet/getlist",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new { operator_id = p.OperatorId }
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);

		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<InternetPackageResponse?>(null);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		JsonElement attachment = data.GetProperty("attachments");

		string? listJson = attachment.GetStringOrNull("list");
		IEnumerable<InternetPackageItem> packages = [];

		if (!string.IsNullOrEmpty(listJson)) {
			JsonElement packagesArray = JsonSerializer.Deserialize<JsonElement>(listJson);
			packages = packagesArray.EnumerateArray()
				.Select(x => new InternetPackageItem {
					Amount = x.GetIntOrNull("Amount"),
					Id = x.GetStringOrNull("Id"),
					Title = x.GetStringOrNull("Title"),
					ShortTitle = x.GetStringOrNull("ShortTitle"),
					SimType = x.GetIntOrNull("SimType"),
					Duration = x.GetStringOrNull("Duration"),
					OfferCode = x.GetStringOrNull("OfferCode"),
					Price = x.GetDecimalOrNull("Price"),
					PackageDType = x.GetIntOrNull("PackageDType"),
					Capacity = x.GetStringOrNull("Capacity")
				})
				.ToList();
		}

		return new UResponse<InternetPackageResponse?>(new InternetPackageResponse {
			Reserve = data.GetIntOrNull("reserve"),
			ServerDateTime = data.GetStringOrNull("serverDateTime"),
			Status = data.GetBoolOrNull("status"),
			Code = data.GetIntOrNull("code"),
			Message = data.GetStringOrNull("message"),
			Reference = attachment.GetStringOrNull("reference"),
			TraceId = attachment.GetStringOrNull("trace_id"),
			Help = attachment.GetStringOrNull("help"),
			List = packages
		});
	}

	public async Task<UResponse<GetBalanceResponse?>> GetBalance(CancellationToken ct) {
		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<GetBalanceResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/GetBalance",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new { }
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);

		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<GetBalanceResponse?>(null);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		JsonElement attachment = data.TryGetProperty("attachments", out JsonElement a) ? a : default;

		return new UResponse<GetBalanceResponse?>(new GetBalanceResponse {
			Reserve = data.GetIntOrNull("reserve"),
			ServerDateTime = data.GetStringOrNull("serverDateTime"),
			Status = data.GetBoolOrNull("status"),
			Code = data.GetIntOrNull("code"),
			Message = data.GetStringOrNull("message"),
			Balance = attachment.GetIntOrNull("balance"),
			Wallet = attachment.GetIntOrNull("wallet"),
			Credit = attachment.GetIntOrNull("credit"),
			Limit = attachment.GetIntOrNull("limit"),
			Help = attachment.GetStringOrNull("help"),
			MessageSource = attachment.GetStringOrNull("message_source"),
			ExtCode = attachment.GetStringOrNull("ext_code")
		});
	}

	public async Task<UResponse<EchoResponse?>> Echo(CancellationToken ct) {
		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<EchoResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Echo",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);

		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<EchoResponse?>(null);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		JsonElement attachment = data.TryGetProperty("attachments", out JsonElement a) ? a : default;

		return new UResponse<EchoResponse?>(new EchoResponse {
			Reserve = data.GetIntOrNull("reserve"),
			ServerDateTime = data.GetStringOrNull("serverDateTime"),
			Status = data.GetBoolOrNull("status"),
			Code = data.GetIntOrNull("code"),
			Message = data.GetStringOrNull("message"),
			MciTopup = attachment.GetBoolOrNull("mci_topup"),
			Mtn = attachment.GetBoolOrNull("mtn"),
			Rightel = attachment.GetBoolOrNull("rightel"),
			Shatel = attachment.GetBoolOrNull("shatel"),
			MciInternet = attachment.GetBoolOrNull("mci_internet")
		});
	}

	public async Task<UResponse<InternetPackageResponse?>> MCITopOffer(MCITopOfferParams p, CancellationToken ct) {
		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<InternetPackageResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Internet/MCITopOffer",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new { subscriber = p.Subscriber }
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);

		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<InternetPackageResponse?>(null);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		JsonElement attachment = data.GetProperty("attachments");

		string? listJson = attachment.GetStringOrNull("list");
		IEnumerable<InternetPackageItem> packages = [];

		if (!string.IsNullOrEmpty(listJson)) {
			JsonElement packagesArray = JsonSerializer.Deserialize<JsonElement>(listJson);
			packages = packagesArray.EnumerateArray()
				.Select(x => new InternetPackageItem {
					Amount = x.GetIntOrNull("amount"),
					Id = x.GetStringOrNull("id"),
					Title = x.GetStringOrNull("title"),
					ShortTitle = null, // Not in this endpoint
					SimType = x.GetIntOrNull("simType"),
					Duration = x.GetStringOrNull("duration"),
					OfferCode = x.GetStringOrNull("offerCode"),
					Price = x.GetDecimalOrNull("price"),
					PackageDType = x.GetIntOrNull("packageDTYPE"),
					Capacity = x.GetStringOrNull("capacity")
				})
				.ToList();
		}

		return new UResponse<InternetPackageResponse?>(new InternetPackageResponse {
			Reserve = data.GetIntOrNull("reserve"),
			ServerDateTime = data.GetStringOrNull("serverDateTime"),
			Status = data.GetBoolOrNull("status"),
			Code = data.GetIntOrNull("code"),
			Message = data.GetStringOrNull("message"),
			Reference = attachment.GetStringOrNull("reference"),
			TraceId = attachment.GetStringOrNull("trace_id"),
			Help = attachment.GetStringOrNull("help"),
			List = packages
		});
	}

	private async Task<ApproveResponse?> Approve(ApproveParams p, CancellationToken ct) {
		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return null;

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Approve",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new { reference = p.Reference, nationalCode = p.NationalCode }
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);

		if (response is null or { IsSuccessStatusCode: false }) return null;

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		JsonElement attachment = data.TryGetProperty("attachments", out JsonElement a) ? a : default;

		return new ApproveResponse {
			Reserve = data.GetIntOrNull("reserve"),
			ServerDateTime = data.GetStringOrNull("serverDateTime"),
			Status = data.GetBoolOrNull("status"),
			Code = data.GetIntOrNull("code"),
			Message = data.GetStringOrNull("message"),
			Reference = attachment.GetIntOrNull("reference"),
			Serial = attachment.GetStringOrNull("serial"),
			Pin = attachment.GetStringOrNull("pin"),
			TraceId = attachment.GetStringOrNull("trace_id"),
			Help = attachment.GetStringOrNull("help"),
			MessageSource = attachment.GetStringOrNull("message_source"),
			ExtCode = attachment.GetStringOrNull("ext_code")
		};
	}

	public async Task<UResponse<GetStatusResponse?>> GetStatus(GetStatusParams p, CancellationToken ct) {
		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<GetStatusResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/GetStatus",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new { reference = p.Reference }
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);

		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<GetStatusResponse?>(null);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		JsonElement attachment = data.TryGetProperty("attachments", out JsonElement a) ? a : default;

		return new UResponse<GetStatusResponse?>(new GetStatusResponse {
			Reserve = data.GetIntOrNull("reserve"),
			ServerDateTime = data.GetStringOrNull("serverDateTime"),
			Status = data.GetBoolOrNull("status"),
			Code = data.GetIntOrNull("code"),
			Message = data.GetStringOrNull("message"),
			Reference = attachment.GetIntOrNull("reference"),
			Subscriber = attachment.GetStringOrNull("subscriber"),
			Serial = attachment.GetStringOrNull("serial"),
			Pin = attachment.GetStringOrNull("pin"),
			TxnTime = attachment.GetStringOrNull("txn_time"),
			Help = attachment.GetStringOrNull("help"),
			MessageSource = attachment.GetStringOrNull("message_source"),
			ExtCode = attachment.GetStringOrNull("ext_code")
		});
	}

	private async Task<GetAccessTokenResponse?> GetAccessToken(CancellationToken ct) {
		HttpResponseMessage? response = await httpClient.Post(
			uri: $"{Core.App.Mobtakeran.BaseUrl}api/v2/login",
			body: new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new { username = Core.App.Mobtakeran.UserName, password = Core.App.Mobtakeran.Password }
			}
		);
		if (response is null or { IsSuccessStatusCode: false }) return null;

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct)).GetProperty("attachments");

		return new GetAccessTokenResponse {
			AccessToken = data.GetStringOrNull("token"),
			Reserve = data.GetStringOrNull("reserve"),
			Status = data.GetBoolOrNull("status"),
			Code = data.GetIntOrNull("code"),
			Message = data.GetStringOrNull("message")
		};
	}
}