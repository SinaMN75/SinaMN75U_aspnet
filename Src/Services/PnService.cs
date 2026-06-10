namespace SinaMN75U.Services;

public class PnUserStatusParams : BaseParams {
	public string PhoneNumber { get; set; } = null!;
}

public class PnUserInquiryResponse {
	public UserDetails User { get; set; } = null!;
	public List<MerchantDetails> Merchants { get; set; } = [];
	public bool IsProfileCompleted { get; set; }
}

public class UserDetails {
	public Guid Id { get; set; }
	public string? PhoneNumber { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? FatherName { get; set; }
	public string? NationalCode { get; set; }
	public string? Email { get; set; }
	public DateTime? Birthdate { get; set; }
	public bool HasNationalCardFront { get; set; }
	public bool HasNationalCardBack { get; set; }
	public bool HasBirthCertificateFirst { get; set; }
	public bool HasVisualAuthentication { get; set; }
	public bool HasESignature { get; set; }
}

public class MerchantDetails {
	public Guid Id { get; set; }
	public string Title { get; set; } = null!;
	public string? BusinessTitle { get; set; }
	public string PhoneNumber { get; set; } = null!;
	public string? Landline { get; set; }
	public string NationalCode { get; set; } = null!;
	public string Mcc { get; set; } = null!;
	public string? Address { get; set; }
	public string? ZipCode { get; set; }
	public string? CityCode { get; set; }
	public List<TerminalDetails> Terminals { get; set; } = [];
}

public class TerminalDetails {
	public Guid Id { get; set; }
	public string Serial { get; set; } = null!;
	public string SimCardSerial { get; set; } = null!;
	public string Imei { get; set; } = null!;
	public string? TerminalId { get; set; }
	public string? InsId { get; set; }
	public bool IsVerified { get; set; }
}

public class PnAuthParams : BaseParams {
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

public class PnMerchantCreateParams : BaseParams {
	public string UserPhoneNumber { get; set; } = null!;
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

public class PnTerminalCreateParams : BaseCreateParams<TagTerminal> {
	public string Serial { get; set; } = null!;
	public string SimCardSerial { get; set; } = null!;
	public string Imei { get; set; } = null!;
	public Guid MerchantId { get; set; }
}

public interface IPnService {
	Task<UResponse> Auth(PnAuthParams p, CancellationToken ct);
	Task<UResponse<Guid?>> CreateMerchant(PnMerchantCreateParams p, CancellationToken ct);
	Task<UResponse<Guid?>> CreateTerminal(PnTerminalCreateParams p, CancellationToken ct);
	Task<UResponse<PnUserInquiryResponse?>> InquiryUserByPhoneNumber(PnUserStatusParams p, CancellationToken ct);
}

public class PnService(
	ITokenService ts,
	ILocalizationService ls,
	DbContext db,
	IHttpClientService http
) : IPnService {
	private const string ApiKey = "123";

	public async Task<UResponse> Auth(PnAuthParams p, CancellationToken ct) {
		if (p.ApiKey != ApiKey) return new UResponse(Usc.UnAuthorized, ls.Get("InvalidAPIKey"));

		UserEntity? e = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.PhoneNumber == p.PhoneNumber, ct);

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
			await db.Set<UserEntity>().AddAsync(user, ct);
			await db.SaveChangesAsync(ct);
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

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateMerchant(PnMerchantCreateParams p, CancellationToken ct) {
		if (p.ApiKey != ApiKey) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("InvalidAPIKey"));

		UserResponse? user = await db.Set<UserEntity>().Select(Projections.UserSelector(new UserSelectorArgs())).FirstOrDefaultAsync(x => x.PhoneNumber == p.UserPhoneNumber, ct);
		if (user == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		MerchantEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatorId = UConstants.PnUserId,
			CreatedAt = DateTime.UtcNow,
			Tags = [TagMerchant.Normal],
			UserId = user.Id,
			ZipCode = p.ZipCode,
			Title = p.Title,
			CityCode = p.CityCode,
			PhoneNumber = p.PhoneNumber,
			Landline = p.Landline,
			NationalCode = p.NationalCode,
			BankAccountId = p.BankAccountId,
			Mcc = p.Mcc,
			JsonData = new MerchantJson {
				Address = p.Address,
				BusinessTitle = p.BusinessTitle,
				OwnerName = p.OwnerName,
				OwnerPhoneNumber = p.OwnerPhoneNumber
			}
		};
		await db.Set<MerchantEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<Guid?>> CreateTerminal(PnTerminalCreateParams p, CancellationToken ct) {
		if (p.ApiKey != ApiKey) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("InvalidAPIKey"));

		TerminalEntity? e = await db.Set<TerminalEntity>().FirstOrDefaultAsync(x => x.Serial == p.Serial && x.SimCardSerial == p.SimCardSerial, ct);
		if (e == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("TerminalNotFoundCheckDetails"));
		MerchantEntity? merchant = await db.Set<MerchantEntity>().FirstOrDefaultAsync(x => x.Id == p.MerchantId, ct);
		if (merchant == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("MerchantNotFound"));

		string agreement = await GenerateAgreement(merchant.User, e, merchant);
		e.MerchantId = p.MerchantId;
		e.Agreement = agreement.FromBase64();

		db.Set<TerminalEntity>().Update(e);
		await db.SaveChangesAsync(ct);

		HttpResponseMessage? response = await http.Post(
			"https://oa.avreenco.com:8080/api/mms/ing/v2/addMerchant",
			new {
				accountId = merchant.BankAccountId,
				businessTitle = merchant.JsonData.BusinessTitle,
				cityCode = merchant.CityCode,
				mcc = merchant.Mcc,
				merchantAddress = merchant.JsonData.Address,
				merchantMobileNo = merchant.PhoneNumber,
				merchantName = merchant.Title,
				merchantOwnerName = merchant.JsonData.OwnerName,
				merchantPhone = merchant.Landline,
				nationalId = merchant.NationalCode,
				ownerMobileNo = merchant.JsonData.OwnerPhoneNumber,
				postalCode = merchant.ZipCode,
				definitionTemplate = 1,
				settlementCurrency = 364
			},
			new Dictionary<string, string> { { "Authorization", $"{Core.App.Avreen.AuthHeader}" }, { "Accept", "application/json" } }
		);

		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<Guid?>(null);
		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));

		merchant.InsId = data.GetStringOrNull("insId")!;
		merchant.MerchantId = data.GetStringOrNull("merchantId")!;

		HttpResponseMessage? terminalResponse = await http.Post(
			$"{Core.App.Avreen.BaseUrl}api/mms/ing/v2/defineAndBindTerminal",
			new {
				definitionTemplate = 1,
				merchantId = merchant.MerchantId,
				project = "AvaPlus",
				terminalSerial = e.Serial,
				terminalSerial2 = e.SimCardSerial
			},
			new Dictionary<string, string> { { "Authorization", $"{Core.App.Avreen.AuthHeader}" }, { "Accept", "application/json" } }
		);

		if (terminalResponse is null or { IsSuccessStatusCode: false }) return new UResponse<Guid?>(null);

		JsonElement terminalData = JsonSerializer.Deserialize<JsonElement>(await terminalResponse.Content.ReadAsStringAsync(ct));

		e.Tags.Add(TagTerminal.Verified);
		e.Tags.Remove(TagTerminal.AwaitingVerification);
		e.TerminalId = terminalData.GetStringOrNull("terminalId");
		e.InsId = terminalData.GetStringOrNull("insId");

		db.Set<TerminalEntity>().Update(e);
		await db.SaveChangesAsync(ct);

		return new UResponse<Guid?>(e.Id);
	}

	private async Task<string> GenerateAgreement(UserEntity user, TerminalEntity terminal, MerchantEntity merchant) {
		return await WordPdfGenerator.GenerateWithTextsAndImagesAsync(
			texts: new Dictionary<string, string> {
				{ "day", PersianDateTime.Now.Day.ToString() },
				{ "month", PersianDateTime.Now.Month.ToString() },
				{ "number", "NUMBER" },
				{ "fullName", $"{user.FirstName ?? ""} {user.LastName ?? ""}" },
				{ "nationalCode", user.NationalCode ?? "" },
				{ "birthdate", PersianDateTime.FromDateTime(user.Birthdate ?? DateTime.Now).ToString("yyyy-MM-dd") },
				{ "address", "ADDRESS" },
				{ "postalCode", merchant.ZipCode ?? "" },
				{ "phoneNumber", user.PhoneNumber ?? "" },
				{ "landLine", user.LandLine ?? "" },
				{ "fatherName", user.JsonData.FatherName ?? "" }
			},
			imagesBase64: new Dictionary<string, string> {
				{ "customerSignature", user.ESignature!.ToBase64()! }
			},
			templatePath: Path.Combine(Directory.GetCurrentDirectory(), "Templates", "atmAgreement.docx")
		);
	}

	public async Task<UResponse<PnUserInquiryResponse?>> InquiryUserByPhoneNumber(PnUserStatusParams p, CancellationToken ct) {
		if (p.ApiKey != ApiKey) return new UResponse<PnUserInquiryResponse?>(null, Usc.UnAuthorized, ls.Get("InvalidAPIKey"));

		UserEntity? user = await db.Set<UserEntity>()
			.Include(x => x.Merchants)
			.ThenInclude(x => x.Terminals)
			.FirstOrDefaultAsync(x => x.PhoneNumber == p.PhoneNumber, ct);

		if (user == null)
			return new UResponse<PnUserInquiryResponse?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		PnUserInquiryResponse response = new() {
			User = new UserDetails {
				Id = user.Id,
				PhoneNumber = user.PhoneNumber,
				FirstName = user.FirstName,
				LastName = user.LastName,
				FatherName = user.JsonData?.FatherName,
				NationalCode = user.NationalCode,
				Email = user.Email,
				Birthdate = user.Birthdate,
				HasNationalCardFront = user.NationalCardFront.IsNotNullOrEmpty(),
				HasNationalCardBack = user.NationalCardBack.IsNotNullOrEmpty(),
				HasBirthCertificateFirst = user.BirthCertificateFirst.IsNotNullOrEmpty(),
				HasVisualAuthentication = user.VisualAuthentication.IsNotNullOrEmpty(),
				HasESignature = user.ESignature.IsNotNullOrEmpty()
			},
			Merchants = user.Merchants?.Select(m => new MerchantDetails {
				Id = m.Id,
				Title = m.Title,
				BusinessTitle = m.JsonData?.BusinessTitle,
				PhoneNumber = m.PhoneNumber,
				Landline = m.Landline,
				NationalCode = m.NationalCode,
				Mcc = m.Mcc,
				Address = m.JsonData?.Address,
				ZipCode = m.ZipCode,
				CityCode = m.CityCode,
				Terminals = m.Terminals.Select(t => new TerminalDetails {
					Id = t.Id,
					Serial = t.Serial,
					SimCardSerial = t.SimCardSerial,
					Imei = t.Imei,
					TerminalId = t.TerminalId,
					InsId = t.InsId,
					IsVerified = t.Tags?.Contains(TagTerminal.Verified) ?? false
				}).ToList() ?? []
			}).ToList() ?? []
		};

		// Check if user profile is completed
		response.IsProfileCompleted = IsUserProfileCompleted(response.User);

		return new UResponse<PnUserInquiryResponse?>(response);
	}

	private static bool IsUserProfileCompleted(UserDetails user) {
		List<bool> requiredFields = [
			!string.IsNullOrEmpty(user.FirstName),
			!string.IsNullOrEmpty(user.LastName),
			!string.IsNullOrEmpty(user.FatherName),
			!string.IsNullOrEmpty(user.NationalCode),
			!string.IsNullOrEmpty(user.Email),
			user.Birthdate.HasValue,
			user.HasNationalCardFront,
			user.HasNationalCardBack,
			user.HasBirthCertificateFirst,
			user.HasVisualAuthentication,
			user.HasESignature
		];

		return requiredFields.All(field => field);
	}
}