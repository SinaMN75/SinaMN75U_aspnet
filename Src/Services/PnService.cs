namespace SinaMN75U.Services;

public class AuthParams : BaseParams {
	public string PhoneNumber { get; set; } = null!;
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? FatherName { get; set; }
	public string? NationalCode { get; set; }
	public string? NationalCardFront { get; set; }
	public string? NationalCardBack { get; set; }
	public string? BirthCertificateFirst { get; set; }
	public string? VisualAuthentication { get; set; }
	public string? ESignature { get; set; }
	public string? Email { get; set; }
	public DateTime? Birthdate { get; set; }
}

public class MerchantCreateParams {
	public string ZipCode { get; set; } = null!;
	public string CityCode { get; set; } = null!;
	public string PhoneNumber { get; set; } = null!;
	public string Title { get; set; } = null!;
	public string Landline { get; set; } = null!;
	public string NationalCode { get; set; } = null!;
	public string OwnerPhoneNumber { get; set; } = null!;
	public string OwnerName { get; set; } = null!;
	public string Mcc { get; set; } = null!;
	public string? BusinessTitle { get; set; }
	public string? BankAccountId { get; set; }
	public string? Address { get; set; }
}

public class TerminalCreateParams {
	public string Serial { get; set; } = null!;
	public string SimCardSerial { get; set; } = null!;
	public string Imei { get; set; } = null!;
	public Guid MerchantId { get; set; }
}

public interface IPnService {
	Task<UResponse> Auth(AuthParams p);
}

public class PnService(
	ITokenService ts,
	ILocalizationService ls,
	DbContext db
) : IPnService {
	public async Task<UResponse> Auth(AuthParams p) {
		if (p.ApiKey != "") return new UResponse(Usc.UnAuthorized, ls.Get("InvalidAPIKey"));

		UserEntity? e = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.PhoneNumber == p.PhoneNumber);

		if (e == null) {
			UserEntity user = new() {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				CreatorId = UConstants.PnUserId,
				RefreshToken = Guid.NewGuid().ToString(),
				Password = p.PhoneNumber,
				UserName = p.PhoneNumber,
				PhoneNumber = p.PhoneNumber,
				NationalCode = p.NationalCode,
				Email = p.Email,
				Birthdate = p.Birthdate,
				FirstName = p.FirstName,
				LastName = p.LastName,
				BirthCertificateFirst = p.BirthCertificateFirst == null ? null : ImageCompressor.CompressBase64(p.BirthCertificateFirst),
				NationalCardFront = p.NationalCardFront == null ? null : ImageCompressor.CompressBase64(p.NationalCardFront),
				NationalCardBack = p.NationalCardBack == null ? null : ImageCompressor.CompressBase64(p.NationalCardBack),
				ESignature = p.ESignature == null ? null : ImageCompressor.CompressBase64(p.ESignature),
				VisualAuthentication = p.VisualAuthentication?.FromBase64(),
				JsonData = new UserJson { FatherName = p.FatherName },
				Tags = [TagUser.SunUser]
			};
			await db.Set<UserEntity>().AddAsync(user);
			await db.SaveChangesAsync();
			return new UResponse();
		}

		if (p.FirstName.IsNotNullOrEmpty()) e.FirstName = p.FirstName;
		if (p.LastName.IsNotNullOrEmpty()) e.LastName = p.LastName;
		if (p.PhoneNumber.IsNotNullOrEmpty()) e.PhoneNumber = p.PhoneNumber;
		if (p.Email.IsNotNullOrEmpty()) e.Email = p.Email;
		if (p.Birthdate.HasValue) e.Birthdate = p.Birthdate;
		if (p.NationalCode.IsNotNullOrEmpty()) e.NationalCode = p.NationalCode;
		if (p.FatherName.IsNotNullOrEmpty()) e.JsonData.FatherName = p.FatherName;
		if (p.NationalCardFront.IsNotNullOrEmpty()) e.NationalCardFront = ImageCompressor.CompressBase64(p.NationalCardFront);
		if (p.NationalCardBack.IsNotNullOrEmpty()) e.NationalCardBack = ImageCompressor.CompressBase64(p.NationalCardBack);
		if (p.BirthCertificateFirst.IsNotNullOrEmpty()) e.BirthCertificateFirst = ImageCompressor.CompressBase64(p.BirthCertificateFirst);
		if (p.VisualAuthentication.IsNotNullOrEmpty()) e.VisualAuthentication = p.VisualAuthentication.FromBase64();
		if (p.ESignature.IsNotNullOrEmpty()) e.ESignature = ImageCompressor.CompressBase64(p.ESignature, 10);

		await db.SaveChangesAsync();
		return new UResponse();

	}
}