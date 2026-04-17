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

	public DefaultUsers Users { get; init; } = new() {
		SystemAdmin = new UserEntity {
			Id = UConstants.SystemAdminId,
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			UserName = "SinaMN75",
			Password = "SinaMN75",
			RefreshToken = "",
			PhoneNumber = "+989351902721",
			NationalCode = "0019246935",
			Email = "sinamn75@gmail.com",
			FirstName = "Sina",
			LastName = "MohammadZadeh",
			Bio = "SinaMN75",
			Birthdate = new DateTime(1996, 7, 21),
			Tags = [TagUser.Male, TagUser.SystemAdmin],
			JsonData = new UserJson { FcmToken = "", FatherName = "Davoud", Weight = 180, Height = 90 },
			Extra = new UserExtraEntity { CreatorId = Core.App.Users.SystemAdmin.Id, Id = UConstants.SystemAdminId, CreatedAt = DateTime.UtcNow, JsonData = new BaseJsonData(), Tags = [] },
			Wallets = [new WalletEntity { CreatorId = Core.App.Users.SystemAdmin.Id, Id = UConstants.SystemAdminId, CreatedAt = DateTime.UtcNow, JsonData = new BaseJsonData(), Tags = [TagWallet.Primary], Balance = 0 }]
		},
		ITHub = new UserEntity {
			Id = UConstants.ITHubUserId,
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			UserName = "ITHub",
			Password = "ITHub123!@#",
			RefreshToken = "",
			PhoneNumber = "+98123456789",
			NationalCode = "123456789",
			Email = "ithub@gmail.com",
			FirstName = "ITHub",
			LastName = "ITHub",
			Bio = "SinaMN75",
			Birthdate = DateTime.UtcNow,
			Tags = [TagUser.Unspecified, TagUser.SystemUser],
			JsonData = new UserJson(),
			Extra = new UserExtraEntity { CreatorId = Core.App.Users.SystemAdmin.Id, Id = UConstants.ITHubUserId, CreatedAt = DateTime.UtcNow, JsonData = new BaseJsonData(), Tags = [] },
			Wallets = [new WalletEntity { CreatorId = Core.App.Users.SystemAdmin.Id, Id = UConstants.ITHubUserId, CreatedAt = DateTime.UtcNow, JsonData = new BaseJsonData(), Tags = [TagWallet.Primary], Balance = 0 }]
		},
		AvaPlus = new UserEntity {
			Id = UConstants.AvaPlus,
			CreatorId = Core.App.Users.SystemAdmin.Id,
			CreatedAt = DateTime.UtcNow,
			UserName = "AvaPlus",
			Password = "AvaPlus123!@#",
			RefreshToken = "",
			PhoneNumber = "+98123456789",
			NationalCode = "987654321",
			Email = "avaplus@gmail.com",
			FirstName = "AvaPlus",
			LastName = "AvaPlus",
			Bio = "AvaPlus",
			Birthdate = DateTime.UtcNow,
			Tags = [TagUser.Unspecified, TagUser.SystemUser],
			JsonData = new UserJson(),
			Extra = new UserExtraEntity { CreatorId = Core.App.Users.SystemAdmin.Id, Id = UConstants.AvaPlus, CreatedAt = DateTime.UtcNow, JsonData = new BaseJsonData(), Tags = [] },
			Wallets = [new WalletEntity { CreatorId = Core.App.Users.SystemAdmin.Id, Id = UConstants.AvaPlus, CreatedAt = DateTime.UtcNow, JsonData = new BaseJsonData(), Tags = [TagWallet.Primary], Balance = 0 }]
		}
	};
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

public sealed class DefaultUsers {
	public required UserEntity SystemAdmin { get; set; }
	public required UserEntity ITHub { get; set; }
	public required UserEntity AvaPlus { get; set; }
}