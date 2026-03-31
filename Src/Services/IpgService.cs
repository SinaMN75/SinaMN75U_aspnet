namespace SinaMN75U.Services;

public interface IIpgService {
	Task<UResponse<string?>> GetSaleIpgLink(IpgSaleParams p, CancellationToken ct);
	Task CallBack(string base64AdditionalData, CancellationToken ct);
}

public class IpgService(IHttpClientService http, DbContext db) : IIpgService {
	public async Task<UResponse<string?>> GetSaleIpgLink(IpgSaleParams p, CancellationToken ct) {
		try {
			HttpResponseMessage response = await http.Post("https://pna.shaparak.ir/mhipg/api/Payment/NormalSale", new {
					CorporationPin = Core.App.Ipg.Token,
					Amount = p.Amount,
					OrderId = p.OrderId.ToInt(),
					CallBackUrl = Core.App.Ipg.CallBackUrl,
					AdditionalData = p.Base64AdditionalData,
					Originator = p.User
				}
			);

			string responseBody = await response.Content.ReadAsStringAsync(ct);
			JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody);
			string token = data.GetProperty("Token").GetString()!;
			return new UResponse<string?>($"https://pna.shaparak.ir/mhui/home/index/{token}");
		}
		catch (Exception) {
			return new UResponse<string?>("https://localhost:7110/api/Ipg/CallBack", Usc.InternalServerError);
		}
	}

	public async Task CallBack(string base64AdditionalData, CancellationToken ct) {
		IpgAdditionalData data = JsonSerializer.Deserialize<IpgAdditionalData>(Convert.FromBase64String(base64AdditionalData).ToString()!)!;

		await db.Set<TxnEntity>().AddAsync(new TxnEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			JsonData = new TxnJson { GatewayName = data.GatewayName },
			Tags = [TagTxn.ChargeWallet, TagTxn.Paid],
			Amount = data.Amount,
			TrackingNumber = data.TrackingNumber,
			UserId = Guid.Parse(data.UserId)
		}, ct);

		await db.Set<WalletTxnEntity>().AddAsync(new WalletTxnEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			JsonData = new WalletTxnJson { Description = "شارژ کیف پول" },
			Tags = [TagWalletTxn.Charge],
			SenderId = Core.App.Ipg.IpgUserId,
			ReceiverId = Guid.Parse(data.UserId),
			Amount = data.Amount
		}, ct);

		WalletEntity wallet = (await db.Set<WalletEntity>().FirstOrDefaultAsync(x => x.UserId == Guid.Parse(data.UserId), ct))!;
		wallet.Balance += data.Amount;
		db.Update(wallet);

		await db.SaveChangesAsync(ct);
	}
}