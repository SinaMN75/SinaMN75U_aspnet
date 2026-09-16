namespace SinaMN75U.Services;

public sealed class IpgProviderPayParams {
	public required TagIpgPayment Kind { get; set; }
	public required long Amount { get; set; }
	public required long OrderId { get; set; }
	public required string CallBackUrl { get; set; }
	public required string AdditionalData { get; set; }
	public string? Originator { get; set; }
	public string? BillId { get; set; }
	public string? PaymentId { get; set; }
	public string? ChargeMobileNumber { get; set; }
	public TagSimOperator? TopUpType { get; set; }
	public IEnumerable<IpgMultiplexedAccountParams>? MultiplexedAccounts { get; set; }
}

public sealed class IpgProviderPayResult {
	public required bool Succeed { get; set; }
	public string? Url { get; set; }
	public string? Token { get; set; }
	public string? Message { get; set; }
}

public interface IIpgProvider {
	TagIpg Tag { get; }
	bool SupportsKind(TagIpgPayment kind);
	bool SupportsTopUpOperator(TagSimOperator operatorTag);
	bool RequiresConfirm(TagIpgPayment kind);
	Task<IpgProviderPayResult> Pay(IpgProviderPayParams p, CancellationToken ct);
	Task<bool> Confirm(string token, CancellationToken ct);
}

public class PnIpgProvider(IHttpClientService http) : IIpgProvider {
	public TagIpg Tag => TagIpg.Pn;

	public bool SupportsKind(TagIpgPayment kind) => true;

	public bool SupportsTopUpOperator(TagSimOperator operatorTag) => TopUpType(operatorTag) != null;

	public bool RequiresConfirm(TagIpgPayment kind) => kind is TagIpgPayment.NormalSale or TagIpgPayment.MultiplexedSale;

	public async Task<IpgProviderPayResult> Pay(IpgProviderPayParams p, CancellationToken ct) {
		HttpResponseMessage? response = await http.Post(
			Url(p.Kind),
			Body(p),
			headers: new Dictionary<string, string> { { "Referer", Core.App.BaseUrl } }
		);

		if (!(response?.IsSuccessStatusCode ?? false)) return new IpgProviderPayResult { Succeed = false };

		JsonElement e = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		if (Status(e) != 0) return new IpgProviderPayResult { Succeed = false, Message = Text(e, "message") };

		string token = Text(e, "token") ?? "---";
		return new IpgProviderPayResult { Succeed = true, Token = token, Url = $"https://pna.shaparak.ir/mhui/home/index/{token}" };
	}

	public async Task<bool> Confirm(string token, CancellationToken ct) {
		HttpResponseMessage? response = await http.Post("https://pna.shaparak.ir/mhipg/api/Payment/confirm", new {
			CorporationPin = Core.App.Ipg.Token,
			Token = token
		});
		if (!(response?.IsSuccessStatusCode ?? false)) return false;

		return Status(JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct))) == 0;
	}

	private static string Url(TagIpgPayment kind) => kind switch {
		TagIpgPayment.Bill => "https://pna.shaparak.ir/mhipg/api/Payment/bill",
		TagIpgPayment.TopUp => "https://pna.shaparak.ir/mhipg/api/Payment/topup",
		TagIpgPayment.MultiplexedSale => "https://pna.shaparak.ir/mhipg/api/Payment/OnlineMultiplexedSale",
		TagIpgPayment.NormalSale => "https://pna.shaparak.ir/mhipg/api/Payment/NormalSale",
		_ => throw new Exception()
	};

	private static object Body(IpgProviderPayParams p) => p.Kind switch {
		TagIpgPayment.Bill => new {
			BillId = p.BillId,
			PayId = p.PaymentId,
			CorporationPin = Core.App.Ipg.Token,
			Amount = p.Amount,
			OrderId = p.OrderId,
			CallBackUrl = p.CallBackUrl,
			AdditionalData = p.AdditionalData,
			Originator = p.Originator ?? ""
		},
		TagIpgPayment.TopUp => new {
			RequesterMobileNumber = p.Originator ?? "",
			ChargeMobileNumber = p.ChargeMobileNumber ?? "",
			TopupType = TopUpType(p.TopUpType),
			CorporationPin = Core.App.Ipg.Token,
			Amount = p.Amount,
			OrderId = p.OrderId,
			CallBackUrl = p.CallBackUrl,
			AdditionalData = p.AdditionalData,
			Originator = p.Originator ?? ""
		},
		TagIpgPayment.MultiplexedSale => new {
			CorporationPin = Core.App.Ipg.Token,
			Amount = p.Amount,
			OrderId = p.OrderId,
			CallBackUrl = p.CallBackUrl,
			AdditionalData = p.AdditionalData,
			Originator = p.Originator ?? "",
			MultiplexedAccounts = p.MultiplexedAccounts?.Select(x => new {
				IBAN = x.Iban,
				Amount = ((long)x.Amount).ToString(),
				Payid = x.PayId?.ToString()
			})
		},
		TagIpgPayment.NormalSale => new {
			CorporationPin = Core.App.Ipg.Token,
			Amount = p.Amount,
			OrderId = p.OrderId,
			CallBackUrl = p.CallBackUrl,
			AdditionalData = p.AdditionalData,
			Originator = p.Originator ?? ""
		},
		_ => throw new Exception()
	};

	private static string? TopUpType(TagSimOperator? operatorTag) => operatorTag switch {
		TagSimOperator.HamrahAvval => "919",
		TagSimOperator.IranCell => "935",
		TagSimOperator.Rigthel => "920",
		_ => null
	};

	private static short Status(JsonElement e) {
		if (!e.TryGetProperty("status", out JsonElement s) && !e.TryGetProperty("Status", out s)) return -1;
		if (s.ValueKind == JsonValueKind.Number) return s.GetInt16();
		return short.TryParse(s.GetString(), out short parsed) ? parsed : (short)-1;
	}

	private static string? Text(JsonElement e, string name) => e.GetStringOrNull(name) ?? e.GetStringOrNull(char.ToUpperInvariant(name[0]) + name[1..]);
}