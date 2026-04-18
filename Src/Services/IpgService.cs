namespace SinaMN75U.Services;

public interface IIpgService {
	Task<UResponse<MpgSaleResponse?>> DirectPurchase(MpgSaleParams p, CancellationToken ct);
	Task<UResponse<string?>> GetSaleIpgLink(IpgSaleParams p, CancellationToken ct);
	Task IpgCallBack(string base64AdditionalData, CancellationToken ct);
}

public class IpgService(IHttpClientService http, DbContext db) : IIpgService {
	public async Task<UResponse<MpgSaleResponse?>> DirectPurchase(MpgSaleParams p, CancellationToken ct) {
		try {
			HttpResponseMessage? response = await http.Post("https://IPGPayment.pna.co.ir/Mhipg/api/DirectPay/DirectPurchase", new {
					corporationpin = Core.App.Mpg.Token,
					amount = p.Amount,
					orderId = p.OrderId,
					pan = p.Pan,
					pin2 = p.Pin2,
					cvv2 = p.Cvv2,
					expireMonth = p.ExpireMonth,
					expireYear = p.ExpireYear,
					additionalData = p.Base64AdditionalData,
					email = p.Email,
					originator = p.PhoneNumber,
					mobileNumber = p.PhoneNumber,
				}
			);
			if (response?.IsSuccessStatusCode ?? false) {
				string responseBody = await response.Content.ReadAsStringAsync(ct);
				JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody);
				return new UResponse<MpgSaleResponse?>(new MpgSaleResponse {
					Rrn = data.GetStringOrNull("rrn"),
					Trace = data.GetStringOrNull("traceNo"),
					OrderId = data.GetStringOrNull("orderId"),
					Token = data.GetStringOrNull("token"),
					Pan = data.GetStringOrNull("truncatedPAN"),
					PhoneNumber = data.GetStringOrNull("originator"),
					Email = data.GetStringOrNull("email"),
					Status = data.GetIntOrNull("status").ToString(),
					Message = data.GetStringOrNull("message"),
				});
			}

			return new UResponse<MpgSaleResponse?>(null);
		}
		catch (Exception) {
			return new UResponse<MpgSaleResponse?>(null, Usc.InternalServerError);
		}
	}

	public async Task<UResponse<string?>> GetSaleIpgLink(IpgSaleParams p, CancellationToken ct) {
		try {
			HttpResponseMessage? response = await http.Post("https://pna.shaparak.ir/mhipg/api/Payment/NormalSale", new {
					CorporationPin = Core.App.Ipg.Token,
					Amount = p.Amount,
					OrderId = p.OrderId.ToInt(),
					CallBackUrl = Core.App.Ipg.CallBackUrl,
					AdditionalData = p.Base64AdditionalData,
					Originator = p.User
				}
			);
			if (response?.IsSuccessStatusCode ?? false) {
				string responseBody = await response.Content.ReadAsStringAsync(ct);
				JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody);
				string token = data.GetProperty("Token").GetString()!;
				return new UResponse<string?>($"https://pna.shaparak.ir/mhui/home/index/{token}");
			}

			return new UResponse<string?>(null);
		}
		catch (Exception) {
			return new UResponse<string?>("https://localhost:7110/api/Ipg/CallBack", Usc.InternalServerError);
		}
	}

	public async Task IpgCallBack(string base64AdditionalData, CancellationToken ct) {
		IpgAdditionalData data = JsonSerializer.Deserialize<IpgAdditionalData>(Convert.FromBase64String(base64AdditionalData).ToString()!)!;

		await db.Set<TxnEntity>().AddAsync(new TxnEntity {
			CreatorId = Core.App.Users.SystemAdmin.Id,
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJsonData(),
			Tags = [TagTxn.ChargeWallet, TagTxn.Paid],
			Amount = data.Amount,
			TrackingNumber = data.TrackingNumber,
			UserId = Guid.Parse(data.UserId)
		}, ct);

		await db.Set<WalletTxnEntity>().AddAsync(new WalletTxnEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJsonData { Detail2 = "شارژ کیف پول" },
			Tags = [TagWalletTxn.Charge],
			SenderId = Core.App.Ipg.IpgUserId,
			ReceiverId = Guid.Parse(data.UserId),
			Amount = data.Amount
		}, ct);

		WalletEntity wallet = (await db.Set<WalletEntity>().FirstOrDefaultAsync(x => x.CreatorId == Guid.Parse(data.UserId), ct))!;
		wallet.Balance += data.Amount;
		db.Update(wallet);

		await db.SaveChangesAsync(ct);
	}
}