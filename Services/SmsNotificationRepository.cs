namespace SinaMN75U.Services;

public interface ISmsNotificationService {
	Task<UResponse> SendSms(string mobileNumber, string template, string param1, string? param2 = null, string? param3 = null);
	Task<bool> SendOtpSms(UserResponse user);
}

public class SmsNotificationService(
	IHttpClientService http,
	IConfiguration config,
	ILocalStorageService cache
) : ISmsNotificationService {
	public async Task<UResponse> SendSms(
		string mobileNumber,
		string template,
		string param1,
		string? param2 = null,
		string? param3 = null
	) {
		switch (config["sms.provider"]!) {
			case "ghasedak": {
				await http.Post("https://api.ghasedak.me/v2/verification/send/simple", new {
						receptor = mobileNumber,
						type = 1,
						template = config["sms.pattern"]!,
						param1,
						param2,
						param3,
					},
					new Dictionary<string, string> { { "apikey", config["sms.apiKey"]! } }
				);
				break;
			}
			case "kavenegar": {
				await http.Post($"https://api.kavenegar.com/v1/{config["sms.apiKey"]!}/verify/lookup.json", new {
						receptor = mobileNumber,
						template,
						token = param1,
						token2 = param2,
						token3 = param3,
					},
					new Dictionary<string, string> { { "apikey", config["sms.apiKey"]! } }
				);
				break;
			}
		}

		return new UResponse();
	}
	
	public async Task<bool> SendOtpSms(UserResponse user) {
		string? cachedData = cache.GetStringData($"otp_{user.Id}");
		if (cachedData != null) return false;

		string otp = Random.Shared.Next(1000, 9999).ToString();
		cache.SetStringData("otp_" + user.Id, otp, TimeSpan.FromMinutes(60));

		await SendSms(user.PhoneNumber, config["sms.otpPattern"]!, otp);
		return true;
	}
}