namespace SinaMN75U.Services;

public interface ISmsNotificationService {
	Task<UResponse> SendSms(string mobileNumber, string template, string param1, string? param2 = null, string? param3 = null);
	Task<bool> SendOtpSms(UserEntity user);
}

public class SmsNotificationService(
	IHttpClientService http,
	ILocalStorageService cache
) : ISmsNotificationService {
	public async Task<UResponse> SendSms(
		string mobileNumber,
		string template,
		string param1,
		string? param2 = null,
		string? param3 = null
	) {
		switch (AppSettings.Instance.SmsPanel.Provider) {
			case "ghasedak": {
				await http.Post("https://api.ghasedak.me/v2/verification/send/simple", new {
						receptor = mobileNumber,
						type = 1,
						template = AppSettings.Instance.SmsPanel.Pattern,
						param1,
						param2,
						param3
					},
					new Dictionary<string, string> { { "apikey", AppSettings.Instance.SmsPanel.ApiKey } }
				);
				break;
			}
			case "kavenegar": {
				await http.Post($"https://api.kavenegar.com/v1/{AppSettings.Instance.SmsPanel.ApiKey}/verify/lookup.json", new {
						receptor = mobileNumber,
						template,
						token = param1,
						token2 = param2,
						token3 = param3
					},
					new Dictionary<string, string> { { "apikey", AppSettings.Instance.SmsPanel.ApiKey } }
				);
				break;
			}
		}

		return new UResponse();
	}

	public async Task<bool> SendOtpSms(UserEntity user) {
		string? cachedData = cache.Get($"otp_{user.Id}");
		if (cachedData != null) return false;

		string otp = Random.Shared.Next(1000, 9999).ToString();
		cache.Set("otp_" + user.Id, otp, TimeSpan.FromMinutes(5));
		
		if (user.PhoneNumber.IsNull()) return false;
		await SendSms(user.PhoneNumber, AppSettings.Instance.SmsPanel.OtpPattern, otp);
		return true;
	}
}