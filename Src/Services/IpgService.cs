using System.Collections.Specialized;

namespace SinaMN75U.Services;

public interface IIpgService {
	Task<UResponse<string?>> GetSaleIpgLink(IpgSaleParams p, CancellationToken ct);
	Task IpgCallBack(string token, short status, string? cardNumberMasked, long? rrn, CancellationToken ct);
}

public class IpgService(IHttpClientService http, DbContext db) : IIpgService {
	public async Task<UResponse<string?>> GetSaleIpgLink(IpgSaleParams p, CancellationToken ct) {
		try {
			HttpResponseMessage? response = await http.Post("https://pna.shaparak.ir/mhipg/api/Payment/NormalSale", new {
					CorporationPin = Core.App.Ipg.Token,
					Amount = (long)p.Amount,
					OrderId = p.OrderId.ToInt(),
					CallBackUrl = $"{Core.App.Ipg.CallBackUrl}?additionalData={p.Base64AdditionalData}",
					AdditionalData = p.Base64AdditionalData,
					Originator = p.User
				}
			);
			if (response?.IsSuccessStatusCode ?? false) {
				string responseBody = await response.Content.ReadAsStringAsync(ct);
				JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody);
				short status = data.GetProperty("Status").GetInt16();
				if (status == 0) {
					string token = data.GetProperty("Token").GetString()!;
					return new UResponse<string?>($"https://pna.shaparak.ir/mhui/home/index/{token}");
				}
			}

			return new UResponse<string?>(null);
		}
		catch (Exception) {
			return new UResponse<string?>(null, Usc.InternalServerError);
		}
	}

	public async Task IpgCallBack(string token, short status, string? cardNumberMasked, long? rrn, CancellationToken ct) {
		if (status != 0) return;

		HttpResponseMessage? confirmResponse = await http.Post("https://pna.shaparak.ir/mhipg/api/Payment/confirm", new {
			CorporationPin = Core.App.Ipg.Token,
			Token = token
		});

		if (!(confirmResponse?.IsSuccessStatusCode ?? false)) return;

		string confirmBody = await confirmResponse.Content.ReadAsStringAsync(ct);
		JsonElement confirmData = JsonSerializer.Deserialize<JsonElement>(confirmBody);
		short confirmStatus = confirmData.GetProperty("Status").GetInt16();
		if (confirmStatus != 0) return;

		NameValueCollection queryParams = System.Web.HttpUtility.ParseQueryString(new Uri(Core.App.Ipg.CallBackUrl).Query);
		string? additionalData = queryParams["additionalData"];
		if (string.IsNullOrEmpty(additionalData)) return;

		string jsonString = Encoding.UTF8.GetString(Convert.FromBase64String(additionalData));
		IpgAdditionalData data = JsonSerializer.Deserialize<IpgAdditionalData>(jsonString)!;

		await db.Set<TxnEntity>().AddAsync(new TxnEntity {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson { Detail2 = $"RRN:{rrn}", Detail1 = $"Card:{cardNumberMasked}" },
			Tags = [TagTxn.Paid],
			CreatorId = Guid.Parse(data.UserId),
			Amount = data.Amount,
			TrackingNumber = data.TrackingNumber,
			UserId = Guid.Parse(data.UserId)
		}, ct);

		await db.Set<TxnEntity>().AddAsync(new TxnEntity {
			CreatorId = Core.App.Users.SystemAdmin.Id,
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson { Detail2 = $"RRN:{rrn}" },
			Tags = [TagTxn.ChargeWallet, TagTxn.Paid],
			Amount = data.Amount,
			TrackingNumber = data.TrackingNumber,
			UserId = Guid.Parse(data.UserId)
		}, ct);

		await db.Set<WalletTxnEntity>().AddAsync(new WalletTxnEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson { Detail2 = "شارژ کیف پول", Detail1 = $"RRN:{rrn}" },
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