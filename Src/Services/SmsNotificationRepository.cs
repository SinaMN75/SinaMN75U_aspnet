namespace SinaMN75U.Services;

public interface ISmsNotificationService {
	Task<bool> SendOtpSms(UserResponse user);
	Task<bool> SendSms(SmsNotificationParams p);
}

public class SmsNotificationService(
	IHttpClientService http,
	ILocalStorageService cache
) : ISmsNotificationService {
	private async Task Send(
		string mobileNumber,
		string template,
		string param1,
		string? param2 = null,
		string? param3 = null
	) {
		try {
			SmsPanel sms = Core.App.SmsPanel;
			switch (sms.Tag) {
				case TagSmsPanel.Ghasedak: {
					await http.Post("https://api.ghasedak.me/v2/verification/send/simple", new {
							receptor = mobileNumber,
							type = 1,
							template = sms.LoginOtpPattern,
							param1,
							param2,
							param3
						},
						new Dictionary<string, string> { { "apikey", sms.ApiKey } }
					);
					break;
				}
				case TagSmsPanel.Kavenegar: {
					await http.Post($"https://api.kavenegar.com/v1/{sms.ApiKey}/verify/lookup.json?receptor={mobileNumber}&token={param1}&template={template}", new { });
					break;
				}
				case TagSmsPanel.NikSms:
				default: break;
			}
		}
		catch (Exception e) {
			Console.WriteLine(e);
		}
	}

	public async Task<bool> SendOtpSms(UserResponse user) {
		if (cache.Get($"otp_{user.Id}") != null) return false;
		int length = Core.App.BasicSettings.VerificationCodeLenght;

		string otp = Random.Shared.Next((int)Math.Pow(10, length - 1), (int)Math.Pow(10, length)).ToString();
		cache.Set("otp_" + user.Id, otp, TimeSpan.FromMinutes(1));

		if (user.PhoneNumber.IsNull()) return false;
		await Send(user.PhoneNumber, Core.App.SmsPanel.LoginOtpPattern, otp);
		return true;
	}

	public async Task<bool> SendSms(SmsNotificationParams p) {
		await Send(p.Mobile, p.Template, p.Text);
		return true;
	}
}

public class SmsNotificationServiceFake(
	IHttpClientService http,
	ILocalStorageService cache
) : ISmsNotificationService {
	private static async Task Send() {
		await Task.Delay(1000);
	}

	public async Task<bool> SendOtpSms(UserResponse user) {
		if (cache.Get($"otp_{user.Id}") != null) return false;
		int length = Core.App.BasicSettings.VerificationCodeLenght;

		string otp = Random.Shared.Next((int)Math.Pow(10, length - 1), (int)Math.Pow(10, length)).ToString();
		cache.Set("otp_" + user.Id, otp, TimeSpan.FromMinutes(1));

		if (user.PhoneNumber.IsNull()) return false;
		await Send();
		return true;
	}

	public async Task<bool> SendSms(SmsNotificationParams p) {
		await Send();
		return true;
	}
}