namespace SinaMN75U.Constants;

public sealed class AppSettings {
	public required string BaseUrl { get; init; }
	public required string ApiKey { get; init; }
	public required bool Test { get; set; }
	public required ConnectionStrings ConnectionStrings { get; init; }
	public required Jwt Jwt { get; init; }
	public required Middleware Middleware { get; init; }
	public required SmsPanel SmsPanel { get; init; }
	public required ItHub ItHub { get; init; }
	public required Mobtakeran Mobtakeran { get; init; }
	public required BasicSettings BasicSettings { get; init; }
	public required Ipg Ipg { get; init; }
	public required Avreen Avreen { get; init; }
	public required Pn Pn { get; init; }
	public required DefaultUsers Users { get; init; }
	public required ApiCallCosts ApiCallCosts { get; set; }
	public required IEnumerable<ChargeInternet> ChargeInternet { get; set; }
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
	public required bool RequireRefreshToken { get; init; }
	public required bool Log { get; init; }
	public required bool LogSuccess { get; init; }
	public int LogRetentionDays { get; init; } = 30;
}

public sealed class SmsPanel {
	public required TagSmsPanel Tag { get; init; }
	public required string LoginOtpPattern { get; init; }
	public required string SupportPasswordOtp { get; init; }
	public required string ApiKey { get; init; }
}

public sealed class ItHub {
	public required string ClientId { get; init; }
	public required string ClientSecret { get; init; }
	public required string UserName { get; init; }
	public required string Password { get; set; }
}

public sealed class Mobtakeran {
	public required string UserName { get; set; }
	public required string Password { get; set; }
	public required string ApiKey { get; set; }
	public required string BaseUrl { get; set; }
}

public sealed class BasicSettings {
	public required string DefaultVerificationKey { get; set; }
	public required int VerificationCodeLenght { get; set; }
}

public sealed class Avreen {
	public required string AuthHeader { get; set; }
	public required string BaseUrl { get; set; }
}

public sealed class Pn {
	public required string ApiKey { get; init; }
}

public sealed class Ipg {
	public required Guid IpgUserId { get; set; }
	public required TagIpg Tag { get; set; }
	public required string Title { get; set; }
	public required string Token { get; set; }
	public required string CallBackUrl { get; set; }
}

public sealed class DefaultUsers {
	public required UserEntity SystemAdmin { get; set; }
	public required UserEntity ITHub { get; set; }
	public required UserEntity AvaPlus { get; set; }
	public required UserEntity Mobtakeran { get; set; }
}

public sealed class ApiCallCosts {
	public required decimal MobileAndNationalCodeVerification { get; set; }
	public required decimal ZipCodeToAddressDetail { get; set; }
	public required decimal VehicleViolationsDetail { get; set; }
	public required decimal DrivingLicenceStatus { get; set; }
	public required decimal FreewayToll { get; set; }
	public required decimal LicencePlateDetail { get; set; }
	public required decimal DrivingLicenceNegativePoint { get; set; }
	public required decimal IBanToBankAccountDetail { get; set; }
}

public sealed class ChargeInternet {
	public required TagSimOperator Operator { get; set; }
	public required string Title { get; set; }
	public required string Logo { get; set; }
	public required List<ChargeInternetPreDefinedAmounts> PreDefinedAmountsList { get; set; }
}

public sealed class ChargeInternetPreDefinedAmounts {
	public required string Title { get; set; }
	public required decimal Amount { get; set; }
}