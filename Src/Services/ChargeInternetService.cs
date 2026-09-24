namespace SinaMN75U.Services;

public interface IChargeInternetService {
	Task<UResponse<ChargeInternetReserveResponse?>> Pin(ReserveChargeParams p, CancellationToken ct);
	Task<UResponse<ChargeInternetReserveResponse?>> Topup(TopupChargeParams p, CancellationToken ct);
	Task<UResponse<InternetPackageResponse?>> InternetList(InternetListParams p, CancellationToken ct);
	Task<UResponse<GetStatusResponse?>> GetStatus(GetStatusParams p, CancellationToken ct);
	Task<UResponse<GetBalanceResponse?>> GetBalance(CancellationToken ct);
	Task<UResponse<EchoResponse?>> Echo(CancellationToken ct);
	Task<UResponse<ChargeInternetReserveResponse?>> InternetReserve(InternetReserveParams p, CancellationToken ct);
}

public class ChargeInternetService(
	IHttpClientService httpClient,
	ILocalizationService ls,
	ITokenService ts,
	IWalletService walletService,
	IVasService vs
) : IChargeInternetService {
	private const int MobtakeranOk = 1;

	public static decimal? PayableAmount(string operatorId, decimal nominalAmount, bool isPin) {
		ChargeInternet? op = Core.App.ChargeInternet.FirstOrDefault(x => ((int)x.Operator).ToString() == operatorId);
		List<ChargeInternetPreDefinedAmounts>? amounts = isPin ? op?.PinAmountsList : op?.TopupAmountsList;
		if (amounts == null || amounts.All(x => x.Amount != nominalAmount)) return null;
		return Math.Round(nominalAmount * (100 + Core.App.ChargeInternetTaxPercent) / 100, 0, MidpointRounding.AwayFromZero);
	}
	
	public async Task<UResponse<ChargeInternetReserveResponse?>> Pin(ReserveChargeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		decimal? payableAmount = PayableAmount(p.SimType, p.Amount, true);
		if (payableAmount == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BadRequest, ls.Get("theSelectedChargeAmountIsNotOfferedByThisOperator"));
		if (!await walletService.HasEnoughBalance(userData.Id, payableAmount.Value, ct)) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BalanceIsLow, ls.Get("yourBalanceIsNotEnough"));

		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ShahkarException, ls.Get("shahkarIsNotAvailableAtThisTimePleaseTryAgainLater"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Pin/Reserve",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new {
					amount = payableAmount.Value.ToIntString(),
					operator_id = p.SimType,
					device = "05"
				}
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);
		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ThirdPartyError, ls.Get("thirdPartyServiceError"));

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		JsonElement attachment = data.GetProperty("attachments");

		if (data.GetIntOrNull("code") != MobtakeranOk) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ThirdPartyError, data.GetStringOrNull("message") ?? ls.Get("thirdPartyServiceError"));

		ApproveResponse? approveResponse = await Approve(new ApproveParams {
			ApiKey = p.ApiKey,
			Token = p.Token,
			Reference = attachment.GetStringOrNull("reference")!,
			CardNumber = null,
			NationalCode = userData.NationalCode
		}, ct);
		
		if (approveResponse is null || approveResponse.Code != MobtakeranOk)
			return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ThirdPartyError, approveResponse?.Message ?? ls.Get("thirdPartyServiceError"));

		await walletService.Purchase(new WalletPurchaseParams { AllowOverdraft = true,
			ApiKey = p.ApiKey,
			Token = p.Token,
			Tag = TagWalletTxn.ChargeSimPin,
			Amount = payableAmount.Value,
			KeyValues = [
				new KeyValue { Key = ULocalizedConstants.Operator, Value = p.SimType },
				new KeyValue { Key = ULocalizedConstants.Pin, Value = approveResponse.Pin ?? "---" },
				new KeyValue { Key = ULocalizedConstants.Reference, Value = approveResponse.Reference?.ToString() ?? "---" }
			]
		}, ct);
		await vs.Create(new VasCreateParams {
			Id = Guid.CreateVersion7(),
			ApiKey = p.ApiKey,
			Token = p.Token,
			Tags = [TagVas.ChargePin],
			CreatorId = userData.Id,
			Amount = payableAmount.Value,
			AuthorizeCode = approveResponse.Reference?.ToString() ?? "",
			BillId = null,
			PaymentId = null,
			TxnId = null,
			WalletTxnId = null,
			ChargePin = approveResponse.Pin
		}, ct);

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
			Pin = approveResponse.Pin
		});
	}

	public async Task<UResponse<ChargeInternetReserveResponse?>> Topup(TopupChargeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		decimal? payableAmount = PayableAmount(p.OperatorId, p.Amount, false);
		if (payableAmount == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BadRequest, ls.Get("theSelectedChargeAmountIsNotOfferedByThisOperator"));
		if (!await walletService.HasEnoughBalance(userData.Id, payableAmount.Value, ct)) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BalanceIsLow, ls.Get("yourBalanceIsNotEnough"));

		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ShahkarException, ls.Get("shahkarIsNotAvailableAtThisTimePleaseTryAgainLater"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Topup/Reserve",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new { subscriber = p.PhoneNumber, amount = payableAmount.Value.ToIntString(), operator_id = p.OperatorId, device = "05", type = "0" }
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);
		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ThirdPartyError, ls.Get("thirdPartyServiceError"));

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		JsonElement attachment = data.GetProperty("attachments");

		if (data.GetIntOrNull("code") != MobtakeranOk) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ThirdPartyError, data.GetStringOrNull("message") ?? ls.Get("thirdPartyServiceError"));

		ApproveResponse? approveResponse = await Approve(new ApproveParams {
			ApiKey = p.ApiKey,
			Token = p.Token,
			Reference = attachment.GetStringOrNull("reference")!,
			CardNumber = null,
			NationalCode = userData.NationalCode
		}, ct);
		if (approveResponse is null || approveResponse.Code != MobtakeranOk)
			return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ThirdPartyError, approveResponse?.Message ?? ls.Get("thirdPartyServiceError"));

		await walletService.Purchase(new WalletPurchaseParams { AllowOverdraft = true,
			ApiKey = p.ApiKey, 
			Token = p.Token,
			Tag = TagWalletTxn.ChargeSimTopup,
			Amount = payableAmount.Value,
			KeyValues = [
				new KeyValue { Key = ULocalizedConstants.PhoneNumber, Value = p.PhoneNumber },
				new KeyValue { Key = ULocalizedConstants.Operator, Value = p.OperatorId },
				new KeyValue { Key = ULocalizedConstants.Reference, Value = approveResponse.Reference?.ToString() ?? "---" }
			]
		}, ct);

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
		if (userData == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!await walletService.HasEnoughBalance(userData.Id, p.Amount, ct)) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BalanceIsLow, ls.Get("yourBalanceIsNotEnough"));

		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ShahkarException, ls.Get("shahkarIsNotAvailableAtThisTimePleaseTryAgainLater"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Internet/Reserve",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new { subscriber = p.Subscriber, operator_id = p.OperatorId, package_id = p.PackageId, amount = p.Amount.ToIntString(), device = p.Device}
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);
		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ThirdPartyError, ls.Get("thirdPartyServiceError"));

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		JsonElement attachment = data.GetProperty("attachments");

		if (data.GetIntOrNull("code") != MobtakeranOk) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ThirdPartyError, data.GetStringOrNull("message") ?? ls.Get("thirdPartyServiceError"));

		ApproveResponse? approveResponse = await Approve(new ApproveParams {
			ApiKey = p.ApiKey,
			Token = p.Token,
			Reference = attachment.GetStringOrNull("reference")!,
			CardNumber = null,
			NationalCode = userData.NationalCode
		}, ct);
		if (approveResponse is null || approveResponse.Code != MobtakeranOk)
			return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ThirdPartyError, approveResponse?.Message ?? ls.Get("thirdPartyServiceError"));

		await walletService.Purchase(new WalletPurchaseParams { AllowOverdraft = true,
			ApiKey = p.ApiKey,
			Token = p.Token,
			Tag = TagWalletTxn.InternetSim,
			Amount = p.Amount,
			KeyValues = [
				new KeyValue { Key = ULocalizedConstants.PhoneNumber, Value = p.Subscriber },
				new KeyValue { Key = ULocalizedConstants.Operator, Value = p.OperatorId },
				new KeyValue { Key = ULocalizedConstants.InternetPackage, Value = p.PackageId },
				new KeyValue { Key = ULocalizedConstants.Reference, Value = approveResponse.Reference?.ToString() ?? "---" }
			]
		}, ct);

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
		if (tokenResponse?.AccessToken == null)
			return new UResponse<InternetPackageResponse?>(null, Usc.ShahkarException, ls.Get("shahkarIsNotAvailableAtThisTimePleaseTryAgainLater"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Internet/getlist",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new { operator_id = p.OperatorId }
			},
			new Dictionary<string, string> {
				{ "Authorization", $"Bearer {tokenResponse.AccessToken}" },
				{ "Accept", "application/json" }
			}
		);

		if (response is null or { IsSuccessStatusCode: false })
			return new UResponse<InternetPackageResponse?>(null, Usc.ThirdPartyError, ls.Get("thirdPartyServiceError"));

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		JsonElement attachment = data.GetProperty("attachments");

		InternetPackageResponse result = new() {
			Status = data.GetBoolOrNull("status") ?? false,
			Message = data.GetStringOrNull("message") ?? ""
		};

		string? listJson = attachment.GetStringOrNull("list");
		if (!string.IsNullOrEmpty(listJson)) {
			JsonElement packagesArray = JsonSerializer.Deserialize<JsonElement>(listJson);
			result.List = packagesArray.EnumerateArray()
				.Select(x => new InternetPackageItem {
					Id = x.GetStringOrNull("Id") ?? "",
					Title = x.GetStringOrNull("Title") ?? "",
					Amount = x.GetIntOrNull("Amount") ?? 0,
					SimType = x.GetIntOrNull("SimType") ?? 0,
					Duration = NormalizeDuration(x.GetStringOrNull("Duration")),
					OfferCode = x.GetStringOrNull("OfferCode") ?? "",
					PackageDType = x.GetIntOrNull("PackageDType") ?? 0,
					Capacity = x.GetStringOrNull("Capacity") ?? ""
				})
				.ToList();
		}

		return new UResponse<InternetPackageResponse?>(result);
	}

	public async Task<UResponse<GetBalanceResponse?>> GetBalance(CancellationToken ct) {
		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<GetBalanceResponse?>(null, Usc.ShahkarException, ls.Get("shahkarIsNotAvailableAtThisTimePleaseTryAgainLater"));

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

		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<GetBalanceResponse?>(null, Usc.ThirdPartyError, ls.Get("thirdPartyServiceError"));

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
		if (tokenResponse?.AccessToken == null) return new UResponse<EchoResponse?>(null, Usc.ShahkarException, ls.Get("shahkarIsNotAvailableAtThisTimePleaseTryAgainLater"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Echo",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")
			},
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);

		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<EchoResponse?>(null, Usc.ThirdPartyError, ls.Get("thirdPartyServiceError"));

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
		if (tokenResponse?.AccessToken == null) return new UResponse<GetStatusResponse?>(null, Usc.ShahkarException, ls.Get("shahkarIsNotAvailableAtThisTimePleaseTryAgainLater"));

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

		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<GetStatusResponse?>(null, Usc.ThirdPartyError, ls.Get("thirdPartyServiceError"));

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

	private static string NormalizeDuration(string? raw) {
		if (string.IsNullOrWhiteSpace(raw)) return "UNKNOWN";
		string v = raw.Trim().ToUpperInvariant().Replace(" ", "");
		if (int.TryParse(v, out int num)) return num <= 31 ? $"{num}D" : $"{Math.Max(1, (int)Math.Round(num / 30.0))}M";
		if (v.Length == 2 && (v[0] == 'W' || v[0] == 'M' || v[0] == 'D')) return $"{v[1]}{v[0]}";
		if (v == "W") return "1W";
		return v == "M" ? "1M" : v;
	}
}

public class ChargeInternetServiceFake(
	ILocalizationService ls,
	ITokenService ts,
	IWalletService walletService,
	IVasService vs
) : IChargeInternetService {
	public bool SimulateUnauthorized { get; set; }
	public bool SimulateLowBalance { get; set; }
	public bool SimulateUpstreamFailure { get; set; }
	public string FakePin { get; set; } = "1234567890123456";
	public long FakeBalance { get; set; } = 5_000_000;

	public async Task<UResponse<ChargeInternetReserveResponse?>> Pin(ReserveChargeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null || SimulateUnauthorized) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		decimal? payableAmount = ChargeInternetService.PayableAmount(p.SimType, p.Amount, true);
		if (payableAmount == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BadRequest, ls.Get("theSelectedChargeAmountIsNotOfferedByThisOperator"));
		if (SimulateLowBalance || !await walletService.HasEnoughBalance(userData.Id, payableAmount.Value, ct)) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BalanceIsLow, ls.Get("yourBalanceIsNotEnough"));
		if (SimulateUpstreamFailure) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ThirdPartyError, ls.Get("thirdPartyServiceError"));

		string reference = Math.Abs(Guid.NewGuid().GetHashCode()).ToString();
		await walletService.Purchase(
			new WalletPurchaseParams { AllowOverdraft = true,
				ApiKey = p.ApiKey,
				Token = p.Token,
				Tag = TagWalletTxn.ChargeSimPin,
				Amount = payableAmount.Value,
				KeyValues = [
					new KeyValue { Key = ULocalizedConstants.Operator, Value = p.SimType },
					new KeyValue { Key = ULocalizedConstants.Pin, Value = FakePin },
					new KeyValue { Key = ULocalizedConstants.Reference, Value = reference }
				]
			}, ct);
		await vs.Create(new VasCreateParams {
			Id = Guid.CreateVersion7(),
			ApiKey = p.ApiKey,
			Token = p.Token,
			Tags = [TagVas.ChargePin],
			CreatorId = userData.Id,
			Amount = payableAmount.Value,
			AuthorizeCode = reference,
			ChargePin = FakePin
		}, ct);
		return new UResponse<ChargeInternetReserveResponse?>(BuildReserve(payableAmount.Value, FakePin, reference));
	}

	public async Task<UResponse<ChargeInternetReserveResponse?>> Topup(TopupChargeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null || SimulateUnauthorized) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		decimal? payableAmount = ChargeInternetService.PayableAmount(p.OperatorId, p.Amount, false);
		if (payableAmount == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BadRequest, ls.Get("theSelectedChargeAmountIsNotOfferedByThisOperator"));
		if (SimulateLowBalance || !await walletService.HasEnoughBalance(userData.Id, payableAmount.Value, ct)) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BalanceIsLow, ls.Get("yourBalanceIsNotEnough"));
		if (SimulateUpstreamFailure) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ThirdPartyError, ls.Get("thirdPartyServiceError"));

		await walletService.Purchase(
			new WalletPurchaseParams { AllowOverdraft = true,
				ApiKey = p.ApiKey, 
				Token = p.Token, 
				Tag = TagWalletTxn.ChargeSimTopup,
				Amount = payableAmount.Value,
				KeyValues = [
					new KeyValue { Key = ULocalizedConstants.PhoneNumber, Value = p.PhoneNumber },
					new KeyValue { Key = ULocalizedConstants.Operator, Value = p.OperatorId }
				]
			}, ct);
		return new UResponse<ChargeInternetReserveResponse?>(BuildReserve(payableAmount.Value, null, Math.Abs(Guid.NewGuid().GetHashCode()).ToString()));
	}

	public async Task<UResponse<ChargeInternetReserveResponse?>> InternetReserve(InternetReserveParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null || SimulateUnauthorized) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (SimulateLowBalance || !await walletService.HasEnoughBalance(userData.Id, p.Amount, ct)) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BalanceIsLow, ls.Get("yourBalanceIsNotEnough"));
		if (SimulateUpstreamFailure) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ThirdPartyError, ls.Get("thirdPartyServiceError"));

		await walletService.Purchase(
			new WalletPurchaseParams { AllowOverdraft = true,
				ApiKey = p.ApiKey,
				Token = p.Token,
				Tag = TagWalletTxn.InternetSim,
				Amount = p.Amount,
				KeyValues = [
					new KeyValue { Key = ULocalizedConstants.PhoneNumber, Value = p.Subscriber },
					new KeyValue { Key = ULocalizedConstants.Operator, Value = p.OperatorId },
					new KeyValue { Key = ULocalizedConstants.InternetPackage, Value = p.PackageId }
				]
			}, ct);
		return new UResponse<ChargeInternetReserveResponse?>(BuildReserve(p.Amount, null, Math.Abs(Guid.NewGuid().GetHashCode()).ToString()));
	}

	public Task<UResponse<InternetPackageResponse?>> InternetList(InternetListParams p, CancellationToken ct) {
		if (SimulateUpstreamFailure) return Task.FromResult(new UResponse<InternetPackageResponse?>(null, Usc.ThirdPartyError, ls.Get("thirdPartyServiceError")));
		return Task.FromResult(new UResponse<InternetPackageResponse?>(new InternetPackageResponse {
			Status = true,
			Message = "OK",
			List = [
				new InternetPackageItem { Id = "PKG-D1", Title = "روزانه ۱ گیگ", Amount = 30_000, SimType = 0, Duration = "1D", OfferCode = "OFFD1", PackageDType = 3, Capacity = "1GB" },
				new InternetPackageItem { Id = "PKG-W5", Title = "هفتگی ۵ گیگ", Amount = 90_000, SimType = 0, Duration = "1W", OfferCode = "OFFW5", PackageDType = 2, Capacity = "5GB" },
				new InternetPackageItem { Id = "PKG-M10", Title = "ماهانه ۱۰ گیگ", Amount = 200_000, SimType = 1, Duration = "1M", OfferCode = "OFFM10", PackageDType = 1, Capacity = "10GB" },
				new InternetPackageItem { Id = "PKG-M30", Title = "ماهانه ۳۰ گیگ", Amount = 450_000, SimType = 1, Duration = "1M", OfferCode = "OFFM30", PackageDType = 1, Capacity = "30GB" }
			]
		}));
	}

	public Task<UResponse<GetStatusResponse?>> GetStatus(GetStatusParams p, CancellationToken ct) {
		if (SimulateUpstreamFailure) return Task.FromResult(new UResponse<GetStatusResponse?>(null, Usc.ThirdPartyError, ls.Get("thirdPartyServiceError")));
		return Task.FromResult(new UResponse<GetStatusResponse?>(new GetStatusResponse {
			Reserve = Math.Abs(Guid.NewGuid().GetHashCode()),
			ServerDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
			Status = true,
			Code = 1,
			Message = "Success",
			Reference = Math.Abs(Guid.NewGuid().GetHashCode()),
			Subscriber = "09120000000",
			Serial = "SER-0001",
			Pin = FakePin,
			TxnTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
			Help = null,
			MessageSource = "fake",
			ExtCode = "0"
		}));
	}

	public Task<UResponse<GetBalanceResponse?>> GetBalance(CancellationToken ct) =>
		Task.FromResult(new UResponse<GetBalanceResponse?>(new GetBalanceResponse {
			Reserve = Math.Abs(Guid.NewGuid().GetHashCode()),
			ServerDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
			Status = true,
			Code = 1,
			Message = "Success",
			Balance = (int)FakeBalance,
			Wallet = (int)FakeBalance,
			Credit = 0,
			Limit = 0,
			Help = null,
			MessageSource = "fake",
			ExtCode = "0"
		}));

	public Task<UResponse<EchoResponse?>> Echo(CancellationToken ct) =>
		Task.FromResult(new UResponse<EchoResponse?>(new EchoResponse {
			Reserve = Math.Abs(Guid.NewGuid().GetHashCode()),
			ServerDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
			Status = true,
			Code = 1,
			Message = "Success",
			MciTopup = true,
			Mtn = true,
			Rightel = true,
			Shatel = true,
			MciInternet = true
		}));

	private static ChargeInternetReserveResponse BuildReserve(decimal amount, string? pin, string reference) => new() {
		Reserve = Math.Abs(Guid.NewGuid().GetHashCode()),
		ServerDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
		Status = true,
		Code = 1,
		Message = "Success",
		Reference = reference,
		TraceId = Guid.NewGuid().ToString("N"),
		AffectiveAmount = (long)amount,
		Help = null,
		MessageSource = "fake",
		Pin = pin
	};
}
