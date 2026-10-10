namespace SinaMN75U.InnerServices;

public interface IEmailService {
	Task<bool> Send(string to, string subject, string body, CancellationToken ct);
}

/// <summary>Sends through AppSettings.SmtpConnection (STARTTLS). Without it the email is only logged.</summary>
public sealed class EmailService : IEmailService {
	public async Task<bool> Send(string to, string subject, string body, CancellationToken ct) {
		if (!Uri.TryCreate(Core.App.SmtpConnection, UriKind.Absolute, out Uri? smtp)) {
			ULog.Warning($"SmtpConnection is not set; email to {to} not sent: {subject} | {body}");
			return false;
		}

		try {
			string[] credentials = smtp.UserInfo.Split(':', 2);
			string user = Uri.UnescapeDataString(credentials[0]);
			string from = System.Web.HttpUtility.ParseQueryString(smtp.Query)["from"] ?? user;

			using MailMessage message = new(from, to, subject, body);
			using SmtpClient client = new(smtp.Host, smtp.IsDefaultPort ? 587 : smtp.Port);
			client.EnableSsl = true;
			client.Credentials = new NetworkCredential(user, credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : "");
			await client.SendMailAsync(message, ct);
			return true;
		}
		catch (Exception e) {
			ULog.Error(e, $"Sending email to {to} failed");
			return false;
		}
	}
}
