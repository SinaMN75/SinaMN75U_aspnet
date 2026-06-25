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
	public async Task<UResponse<ChargeInternetReserveResponse?>> Pin(ReserveChargeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!await walletService.HasEnoughBalance(userData.Id, p.Amount, ct)) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Pin/Reserve",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new {
					amount = p.Amount.ToIntString(),
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

		if (approveResponse != null) {
			await walletService.Purchase(new WalletPurchaseParams { ApiKey = p.ApiKey, Token = p.Token, Tag = TagWalletTxn.ChargeSimPin, Amount = p.Amount }, ct);
			await vs.Create(new VasCreateParams {
				Id = Guid.CreateVersion7(),
				ApiKey = p.ApiKey,
				Token = p.Token,
				Tags = [TagVas.ChargePin],
				CreatorId = userData.Id,
				Amount = p.Amount,
				AuthorizeCode = approveResponse.Reference?.ToString() ?? "",
				BillId = null,
				PaymentId = null,
				TxnId = null,
				WalletTxnId = null,
				ChargePin = approveResponse.Pin
			}, ct);
		}

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
		if (!await walletService.HasEnoughBalance(userData.Id, p.Amount, ct)) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Topup/Reserve",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new { subscriber = p.PhoneNumber, amount = p.Amount.ToIntString(), operator_id = p.OperatorId, device = "05", type = "0" }
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

		await walletService.Purchase(new WalletPurchaseParams { ApiKey = p.ApiKey, Token = p.Token, Tag = TagWalletTxn.ChargeSimTopup, Amount = p.Amount }, ct);

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
		if (!await walletService.HasEnoughBalance(userData.Id, p.Amount, ct)) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.BalanceIsLow, ls.Get("BalanceIsLow"));

		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) return new UResponse<ChargeInternetReserveResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

		HttpResponseMessage? response = await httpClient.Post(
			$"{Core.App.Mobtakeran.BaseUrl}api/v2/Internet/Reserve",
			new {
				apiKey = Core.App.Mobtakeran.ApiKey,
				reserve = Random.Shared.Next(999999).ToString(),
				localDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				attachments = new { subscriber = p.Subscriber, operator_id = p.OperatorId, package_id = p.PackageId, amount = p.Amount.ToIntString(), device = p.Device, bank = p.Bank }
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

		await walletService.Purchase(new WalletPurchaseParams {
			ApiKey = p.ApiKey,
			Token = p.Token,
			Tag = TagWalletTxn.InternetSim,
			Amount = p.Amount
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
			return new UResponse<InternetPackageResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));

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
			return new UResponse<InternetPackageResponse?>(null);

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

	private static string NormalizeDuration(string? raw) {
		if (string.IsNullOrWhiteSpace(raw)) return "UNKNOWN";
		string v = raw.Trim().ToUpperInvariant().Replace(" ", "");
		// "30" -> "30D", "7" -> "7D", "90" -> "3M"
		if (int.TryParse(v, out int num)) return num <= 31 ? $"{num}D" : $"{Math.Max(1, (int)Math.Round(num / 30.0))}M";
		// "W1" -> "1W", "M1" -> "1M"  
		if (v.Length == 2 && (v[0] == 'W' || v[0] == 'M' || v[0] == 'D')) return $"{v[1]}{v[0]}";
		// "W" -> "1W", "M" -> "1M"
		if (v == "W") return "1W";
		return v == "M" ? "1M" : v;
	}
}

public class ChargeInternetServiceFake : IChargeInternetService {
	public bool SimulateUnauthorized { get; set; }
	public bool SimulateLowBalance { get; set; }
	public bool SimulateTokenFailure { get; set; }
	public bool SimulateUpstreamFailure { get; set; }

	public List<ReserveChargeParams> PinCalls { get; } = [];
	public List<TopupChargeParams> TopupCalls { get; } = [];
	public List<InternetReserveParams> InternetReserveCalls { get; } = [];
	public List<InternetListParams> InternetListCalls { get; } = [];
	public List<GetStatusParams> GetStatusCalls { get; } = [];
	public int GetBalanceCalls { get; private set; }
	public int EchoCalls { get; private set; }

	public string FakePin { get; set; } = "1234567890123456";
	public int FakeBalance { get; set; } = 5_000_000;

	public Task<UResponse<ChargeInternetReserveResponse?>> Pin(ReserveChargeParams p, CancellationToken ct) {
		PinCalls.Add(p);
		if (SimulateUnauthorized) return Fail<ChargeInternetReserveResponse>(Usc.UnAuthorized, "AuthorizationRequired");
		if (SimulateLowBalance) return Fail<ChargeInternetReserveResponse>(Usc.BalanceIsLow, "BalanceIsLow");
		return SimulateTokenFailure ? Fail<ChargeInternetReserveResponse>(Usc.ShahkarException, "ShahkarIsNotAvailableAtThisTime") : Task.FromResult(SimulateUpstreamFailure ? new UResponse<ChargeInternetReserveResponse?>(null) : new UResponse<ChargeInternetReserveResponse?>(BuildReserve(p.Amount, FakePin)));
	}

	public Task<UResponse<ChargeInternetReserveResponse?>> Topup(TopupChargeParams p, CancellationToken ct) {
		TopupCalls.Add(p);
		if (SimulateUnauthorized) return Fail<ChargeInternetReserveResponse>(Usc.UnAuthorized, "AuthorizationRequired");
		if (SimulateLowBalance) return Fail<ChargeInternetReserveResponse>(Usc.BalanceIsLow, "BalanceIsLow");
		return SimulateTokenFailure ? Fail<ChargeInternetReserveResponse>(Usc.ShahkarException, "ShahkarIsNotAvailableAtThisTime") : Task.FromResult(SimulateUpstreamFailure ? new UResponse<ChargeInternetReserveResponse?>(null) : new UResponse<ChargeInternetReserveResponse?>(BuildReserve(p.Amount, null)));
	}

	public Task<UResponse<ChargeInternetReserveResponse?>> InternetReserve(InternetReserveParams p, CancellationToken ct) {
		InternetReserveCalls.Add(p);
		if (SimulateUnauthorized) return Fail<ChargeInternetReserveResponse>(Usc.UnAuthorized, "AuthorizationRequired");
		if (SimulateLowBalance) return Fail<ChargeInternetReserveResponse>(Usc.BalanceIsLow, "BalanceIsLow");
		return SimulateTokenFailure ? Fail<ChargeInternetReserveResponse>(Usc.ShahkarException, "ShahkarIsNotAvailableAtThisTime") : Task.FromResult(SimulateUpstreamFailure ? new UResponse<ChargeInternetReserveResponse?>(null) : new UResponse<ChargeInternetReserveResponse?>(BuildReserve(p.Amount, null)));
	}

	public Task<UResponse<InternetPackageResponse?>> InternetList(InternetListParams p, CancellationToken ct) {
		InternetListCalls.Add(p);
		if (SimulateTokenFailure) return Fail<InternetPackageResponse>(Usc.ShahkarException, "ShahkarIsNotAvailableAtThisTime");
		if (SimulateUpstreamFailure) return Task.FromResult(new UResponse<InternetPackageResponse?>(null));


		InternetPackageResponse result = new() {
			Status = true,
			Message = "OK",
			List = [
				new() { Id = "PKG-1", Title = "1GB - 1 Day", Amount = 30_000, SimType = 0, Duration = "1D", OfferCode = "OFF1", PackageDType = 1, Capacity = "1GB" },
				new() { Id = "PKG-2", Title = "10GB - 30 Day", Amount = 200_000, SimType = 1, Duration = "1M", OfferCode = "OFF2", PackageDType = 2, Capacity = "10GB" }
			]
		};
		return Task.FromResult(new UResponse<InternetPackageResponse?>(result));
	}

	public Task<UResponse<GetStatusResponse?>> GetStatus(GetStatusParams p, CancellationToken ct) {
		GetStatusCalls.Add(p);
		if (SimulateTokenFailure) return Fail<GetStatusResponse>(Usc.ShahkarException, "ShahkarIsNotAvailableAtThisTime");
		if (SimulateUpstreamFailure) return Task.FromResult(new UResponse<GetStatusResponse?>(null));

		return Task.FromResult(new UResponse<GetStatusResponse?>(new GetStatusResponse {
			Reserve = 111111,
			ServerDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
			Status = true,
			Code = 0,
			Message = "Success",
			Reference = 999999,
			Subscriber = "09120000000",
			Serial = "SER-0001",
			Pin = FakePin,
			TxnTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
			Help = null,
			MessageSource = "fake",
			ExtCode = "0"
		}));
	}

	public Task<UResponse<GetBalanceResponse?>> GetBalance(CancellationToken ct) {
		GetBalanceCalls++;
		if (SimulateTokenFailure) return Fail<GetBalanceResponse>(Usc.ShahkarException, "ShahkarIsNotAvailableAtThisTime");
		if (SimulateUpstreamFailure) return Task.FromResult(new UResponse<GetBalanceResponse?>(null));

		return Task.FromResult(new UResponse<GetBalanceResponse?>(new GetBalanceResponse {
			Reserve = 222222,
			ServerDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
			Status = true,
			Code = 0,
			Message = "Success",
			Balance = FakeBalance,
			Wallet = FakeBalance,
			Credit = 0,
			Limit = 0,
			Help = null,
			MessageSource = "fake",
			ExtCode = "0"
		}));
	}

	public Task<UResponse<EchoResponse?>> Echo(CancellationToken ct) {
		EchoCalls++;
		if (SimulateTokenFailure) return Fail<EchoResponse>(Usc.ShahkarException, "ShahkarIsNotAvailableAtThisTime");
		if (SimulateUpstreamFailure) return Task.FromResult(new UResponse<EchoResponse?>(null));
		return Task.FromResult(new UResponse<EchoResponse?>(new EchoResponse {
			Reserve = 333333,
			ServerDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
			Status = true,
			Code = 0,
			Message = "Success",
			MciTopup = true,
			Mtn = true,
			Rightel = true,
			Shatel = true,
			MciInternet = true
		}));
	}

	private static ChargeInternetReserveResponse BuildReserve(decimal amount, string? pin) => new() {
		Reserve = Random.Shared.Next(999999),
		ServerDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
		Status = true,
		Code = 0,
		Message = "Success",
		Reference = Guid.NewGuid().ToString("N"),
		TraceId = Guid.NewGuid().ToString("N"),
		AffectiveAmount = (int)amount,
		Help = null,
		MessageSource = "fake",
		Pin = pin
	};

	private static Task<UResponse<T?>> Fail<T>(Usc status, string messageKey) => Task.FromResult(new UResponse<T?>(default, status, messageKey));
}