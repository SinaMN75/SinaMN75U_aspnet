namespace SinaMN75U.Constants;

public sealed class AppSettings {
	public required string BaseUrl { get; init; }
	public required string ApiKey { get; init; }
	public required ConnectionStrings ConnectionStrings { get; init; }
	public required Jwt Jwt { get; init; }
	public required Middleware Middleware { get; init; }
	public required SmsPanel SmsPanel { get; init; }
	public required ItHub ItHub { get; init; }
	public required BasicSettings BasicSettings { get; init; }
	public required Ipg Ipg { get; init; }
	public ICollection<UserEntity> DefaultUsers { get; set; } = [
		
	];
}

public sealed class ConnectionStrings {
	public required string Server { get; init; }
}

public sealed class Jwt {
	public required string Key { get; init; }
	public required string Issuer { get; init; }
	public required string Audience { get; init; }
	public required string Expires { get; init; }
}

public sealed class Middleware {
	public required bool DecryptParams { get; init; }
	public required bool EncryptResponse { get; init; }
	public required bool RequireApiKey { get; init; }
	public required bool Log { get; init; }
	public required bool LogSuccess { get; init; }
}

public sealed class SmsPanel {
	public required TagSmsPanel Tag { get; init; }
	public required string Pattern { get; init; }
	public required string ApiKey { get; init; }
}

public sealed class ItHub {
	public required string ClientId { get; init; }
	public required string ClientSecret { get; init; }
	public required string UserName { get; init; }
	public required string Password { get; set; }
	public required string WalletOwnerUserName { get; set; }
	public required decimal ShahkarVerifyNationalCodeAndMobilePrice { get; set; }
	public required decimal ZipCodeToAddressDetailPrice { get; set; }
}

public sealed class BasicSettings {
	public required string DefaultVerificationKey { get; set; }
	public required int VerificationCodeLenght { get; set; }
}

public sealed class Ipg {
	public required Guid IpgUserId { get; set; }
	public required TagIpg Tag { get; set; }
	public required string Title { get; set; }
	public required string Token { get; set; }
	public required string CallBackUrl { get; set; }
}