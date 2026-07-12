using Microsoft.EntityFrameworkCore.Storage;

namespace SinaMN75U.Services;

public class PnUserStatusResponse {
	public PnUserStatusItem NationalCardFront { get; set; } = null!;
	public PnUserStatusItem NationalCardBack { get; set; } = null!;
	public PnUserStatusItem BirthCertificateFirst { get; set; } = null!;
	public PnUserStatusItem VisualAuthentication { get; set; } = null!;
	public PnUserStatusItem ESignature { get; set; } = null!;
	public IEnumerable<MerchantResponse> Merchants { get; set; } = [];
}

public class PnPhoneNumberParams : BaseParams {
	[UValidationRequired("PhoneNumberRequired")]
	[UValidationRegex(@"^09\d{9}$", "InvalidPhoneNumber")]
	public string PhoneNumber { get; set; } = null!;
}

public class PnUserStatusItem {
	public bool IsUploaded { get; set; }
	public bool IsVerified { get; set; }
	public string? RejectionReason { get; set; }
}

public class PnAuthParams : BaseParams {
	[UValidationRequired("PhoneNumberRequired")]
	[UValidationRegex(@"^09\d{9}$", "InvalidPhoneNumber")]
	public string PhoneNumber { get; set; } = null!;
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? FatherName { get; set; }
	[UValidationStringLength(10, 10, "InvalidNationalCode")]
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
	[UValidationRequired("PhoneNumberRequired")]
	[UValidationRegex(@"^09\d{9}$", "InvalidPhoneNumber")]
	public string UserPhoneNumber { get; set; } = null!;

	[UValidationRequired("ZipCodeRequired")]
	[UValidationRegex(@"^\d{10}$", "InvalidZipCode")]
	public string ZipCode { get; set; } = null!;

	[UValidationRequired("CityCodeRequired")]
	public string CityCode { get; set; } = null!;

	[UValidationRequired("PhoneNumberRequired")]
	[UValidationRegex(@"^09\d{9}$", "InvalidPhoneNumber")]
	public string PhoneNumber { get; set; } = null!;

	[UValidationRequired("TitleRequired")]
	public string Title { get; set; } = null!;

	[UValidationRequired("LandlineRequired")]
	[UValidationStringLength(6, 12, "InvalidLandline")]
	public string Landline { get; set; } = null!;

	[UValidationRequired("NationalCodeRequired")]
	[UValidationRegex(@"^\d{10}$", "InvalidNationalCode")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("PhoneNumberRequired")]
	[UValidationRegex(@"^09\d{9}$", "InvalidPhoneNumber")]
	public string OwnerPhoneNumber { get; set; } = null!;

	[UValidationRequired("OwnerNameRequired")]
	public string OwnerName { get; set; } = null!;

	[UValidationRequired("MccRequired")]
	public string Mcc { get; set; } = null!;

	[UValidationRequired("AddressRequired")]
	public string Address { get; set; } = null!;

	public string? BusinessTitle { get; set; }
	public string? BankAccountId { get; set; }
}

public class PnTerminalCreateParams : BaseParams {
	[UValidationRequired("SerialRequired")]
	public string Serial { get; set; } = null!;

	[UValidationRequired("SimCardSerialRequired")]
	public string SimCardSerial { get; set; } = null!;

	[UValidationRequired("ImeiRequired")]
	[UValidationRegex(@"^\d{15}$", "InvalidImei")]
	public string Imei { get; set; } = null!;

	public Guid MerchantId { get; set; }
}

public class PnTerminalSupportPasswordParams : BaseParams {
	[UValidationRequired("TerminalIdRequired")]
	public Guid TerminalId { get; set; }
}

public class PnZipCodeParams : BaseParams {
	[UValidationRequired("ZipCodeRequired")]
	[UValidationStringLength(10, 10, "ZipCodeInvalid")]
	public string ZipCode { get; set; } = null!;
}

public interface IPnService {
	Task<UResponse> Auth(PnAuthParams p, CancellationToken ct);
	Task<UResponse<Guid?>> CreateMerchant(PnMerchantCreateParams p, CancellationToken ct);
	Task<UResponse<Guid?>> CreateTerminal(PnTerminalCreateParams p, CancellationToken ct);
	Task<UResponse<PnUserStatusResponse?>> UserStatus(PnPhoneNumberParams p, CancellationToken ct);
	Task<UResponse<TerminalSupportPasswordResponse?>> ReadTerminalSupportPassword(PnTerminalSupportPasswordParams p, CancellationToken ct);
	Task<UResponse<ZipCodeToAddressDetailResponse?>> ZipCodeToAddress(PnZipCodeParams p, CancellationToken ct);
}

public class PnService(
	ILocalizationService ls,
	DbContext db,
	IHttpClientService http,
	IWebHostEnvironment env
) : IPnService {

	public async Task<UResponse> Auth(PnAuthParams p, CancellationToken ct) {
		ULog.Info($"Auth method called for phone number: {p.PhoneNumber}");
		ULog.Debug($"Auth params - FirstName: {p.FirstName}, LastName: {p.LastName}");

		// API Key validation
		if (p.ApiKey != Core.App.Pn.ApiKey) {
			ULog.Warning($"Auth failed: Invalid API key provided for phone {p.PhoneNumber}");
			return new UResponse(Usc.UnAuthorized, ls.Get("InvalidAPIKey"));
		}

		ULog.Debug("API key validation passed");

		// Search for existing user
		ULog.Debug($"Searching for existing user with phone: {p.PhoneNumber}");
		UserEntity? e = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.PhoneNumber == p.PhoneNumber, ct);

		if (e == null) {
			ULog.Info($"No existing user found for {p.PhoneNumber}, creating new user");

			// Create new user
			UserEntity user = new() {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				CreatorId = UConstants.SystemAdminId,
				RefreshToken = Guid.NewGuid().ToString(),
				Password = UPasswordHasher.Hash(p.PhoneNumber),
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

			ULog.Debug($"New user created with ID: {user.Id}");
			ULog.Debug($"Image compression applied - BirthCertificate: {p.BirthCertificateFirst != null}, NationalCardFront: {p.NationalCardFront != null}, NationalCardBack: {p.NationalCardBack != null}, ESignature: {p.ESignature != null}, VisualAuth: {p.VisualAuthentication != null}");

			await db.Set<UserEntity>().AddAsync(user, ct);
			await db.SaveChangesAsync(ct);
			ULog.Success($"User {p.PhoneNumber} created and saved successfully");
			return new UResponse();
		}

		ULog.Info($"Existing user found for {p.PhoneNumber} with ID: {e.Id}");

		// Update user fields
		bool updated = false;

		if (p.FirstName.IsNotNullOrEmpty()) {
			ULog.Debug($"Updating FirstName from '{e.FirstName}' to '{p.FirstName}'");
			e.FirstName = p.FirstName;
			updated = true;
		}

		if (p.LastName.IsNotNullOrEmpty()) {
			ULog.Debug($"Updating LastName from '{e.LastName}' to '{p.LastName}'");
			e.LastName = p.LastName;
			updated = true;
		}

		if (p.PhoneNumber.IsNotNullOrEmpty()) {
			ULog.Debug($"Updating PhoneNumber from '{e.PhoneNumber}' to '{p.PhoneNumber}'");
			e.PhoneNumber = p.PhoneNumber;
			updated = true;
		}

		if (p.Email.IsNotNullOrEmpty()) {
			ULog.Debug($"Updating Email from '{e.Email}' to '{p.Email}'");
			e.Email = p.Email;
			updated = true;
		}

		if (p.Birthdate.HasValue) {
			ULog.Debug($"Updating Birthdate from '{e.Birthdate}' to '{p.Birthdate}'");
			e.Birthdate = p.Birthdate;
			updated = true;
		}

		if (p.NationalCode.IsNotNullOrEmpty()) {
			ULog.Debug($"Updating NationalCode from '{e.NationalCode}' to '{p.NationalCode}'");
			e.NationalCode = p.NationalCode;
			updated = true;
		}

		if (p.FatherName.IsNotNullOrEmpty()) {
			ULog.Debug($"Updating FatherName from '{e.JsonData.FatherName}' to '{p.FatherName}'");
			e.JsonData.FatherName = p.FatherName;
			updated = true;
		}

		// FIX: Re-uploading a verified field resets its verification tag so admin must re-approve
		if (p.NationalCardFront.IsNotNullOrEmpty()) {
			ULog.Warning($"NationalCardFront re-uploaded - removing verification tag for user {e.Id}");
			e.NationalCardFront = ImageCompressor.CompressBase64(p.NationalCardFront);
			e.Tags.Remove(TagUser.NationalCardFrontVerified);
			e.JsonData.NationalCardFrontRejectionReason = null;
			updated = true;
			ULog.Debug("NationalCardFront updated and verification reset");
		}

		if (p.NationalCardBack.IsNotNullOrEmpty()) {
			ULog.Warning($"NationalCardBack re-uploaded - removing verification tag for user {e.Id}");
			e.NationalCardBack = ImageCompressor.CompressBase64(p.NationalCardBack);
			e.Tags.Remove(TagUser.NationalCardBackVerified);
			e.JsonData.NationalCardBackRejectionReason = null;
			updated = true;
			ULog.Debug("NationalCardBack updated and verification reset");
		}

		if (p.BirthCertificateFirst.IsNotNullOrEmpty()) {
			ULog.Warning($"BirthCertificateFirst re-uploaded - removing verification tag for user {e.Id}");
			e.BirthCertificateFirst = ImageCompressor.CompressBase64(p.BirthCertificateFirst);
			e.Tags.Remove(TagUser.BirthCertificateFirstVerified);
			e.JsonData.BirthCertificateFirstRejectionReason = null;
			updated = true;
			ULog.Debug("BirthCertificateFirst updated and verification reset");
		}

		if (p.VisualAuthentication.IsNotNullOrEmpty()) {
			ULog.Warning($"VisualAuthentication re-uploaded - removing verification tag for user {e.Id}");
			e.VisualAuthentication = p.VisualAuthentication.FromBase64();
			e.Tags.Remove(TagUser.VisualAuthenticationVerified);
			e.JsonData.VisualAuthenticationRejectionReason = null;
			updated = true;
			ULog.Debug("VisualAuthentication updated and verification reset");
		}

		if (p.ESignature.IsNotNullOrEmpty()) {
			ULog.Warning($"ESignature re-uploaded - removing verification tag for user {e.Id}");
			e.ESignature = ImageCompressor.CompressBase64(p.ESignature, 10);
			e.Tags.Remove(TagUser.ESignatureVerified);
			e.JsonData.ESignatureRejectionReason = null;
			updated = true;
			ULog.Debug("ESignature updated and verification reset");
		}

		if (updated) {
			ULog.Info($"Saving updates for user {e.Id}");
			await db.SaveChangesAsync(ct);
			ULog.Success($"User {p.PhoneNumber} updated successfully");
		}
		else {
			ULog.Debug($"No updates needed for user {p.PhoneNumber}");
		}

		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateMerchant(PnMerchantCreateParams p, CancellationToken ct) {
		ULog.Info($"CreateMerchant called for user phone: {p.UserPhoneNumber}");
		ULog.Debug($"Merchant params - Title: {p.Title}, NationalCode: {p.NationalCode}, Mcc: {p.Mcc}, CityCode: {p.CityCode}");

		// API Key validation
		if (p.ApiKey != Core.App.Pn.ApiKey) {
			ULog.Warning($"CreateMerchant failed: Invalid API key for user {p.UserPhoneNumber}");
			return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("InvalidAPIKey"));
		}

		ULog.Debug("API key validation passed");

		// Find user
		ULog.Debug($"Fetching user with phone: {p.UserPhoneNumber}");
		UserResponse? user = await db.Set<UserEntity>().Select(Projections.UserSelector(new UserSelectorArgs())).FirstOrDefaultAsync(x => x.PhoneNumber == p.UserPhoneNumber, ct);
		if (user == null) {
			ULog.Warning($"CreateMerchant failed: User not found for phone {p.UserPhoneNumber}");
			return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("UserNotFound"));
		}

		ULog.Debug($"User found with ID: {user.Id}, Tags: {string.Join(", ", user.Tags)}");

		// Check if user is fully verified
		bool fullyVerified =
			user.Tags.Contains(TagUser.NationalCardFrontVerified) &&
			user.Tags.Contains(TagUser.NationalCardBackVerified) &&
			user.Tags.Contains(TagUser.BirthCertificateFirstVerified) &&
			user.Tags.Contains(TagUser.VisualAuthenticationVerified) &&
			user.Tags.Contains(TagUser.ESignatureVerified);

		ULog.Debug($"User fully verified status: {fullyVerified}");
		if (!fullyVerified) {
			ULog.Warning($"CreateMerchant failed: User {user.Id} is not fully verified");
			return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("UserNotFullyVerified"));
		}

		// Check for duplicate merchant
		ULog.Debug($"Checking for duplicate merchant with NationalCode: {p.NationalCode}");
		bool duplicate = await db.Set<MerchantEntity>().AnyAsync(x => x.UserId == user.Id && x.NationalCode == p.NationalCode && x.ZipCode == p.ZipCode, ct);
		if (duplicate) {
			ULog.Warning($"CreateMerchant failed: Merchant already exists for user {user.Id} with NationalCode {p.NationalCode}");
			return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("MerchantAlreadyExists"));
		}

		// Create new merchant
		ULog.Info($"Creating new merchant for user {user.Id}");
		MerchantEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatorId = UConstants.SystemAdminId,
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

		ULog.Debug($"Merchant created with ID: {e.Id}");
		await db.Set<MerchantEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		ULog.Success($"Merchant {e.Id} created successfully for user {user.Id}");

		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<Guid?>> CreateTerminal(PnTerminalCreateParams p, CancellationToken ct) {
		ULog.Info($"CreateTerminal called for MerchantId: {p.MerchantId}");
		ULog.Debug($"Terminal params - Serial: {p.Serial}, SimCardSerial: {p.SimCardSerial}, Imei: {p.Imei}");

		// API Key validation
		if (p.ApiKey != Core.App.Pn.ApiKey) {
			ULog.Warning($"CreateTerminal failed: Invalid API key for merchant {p.MerchantId}");
			return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("InvalidAPIKey"));
		}

		ULog.Debug("API key validation passed");

		// Find terminal
		ULog.Debug($"Searching for terminal with Serial: {p.Serial}, SimCardSerial: {p.SimCardSerial}");
		TerminalEntity? terminal = await db.Set<TerminalEntity>().AsTracking().FirstOrDefaultAsync(x => x.Serial == p.Serial && x.SimCardSerial == p.SimCardSerial, ct);
		if (terminal == null) {
			ULog.Warning($"CreateTerminal failed: Terminal not found with Serial {p.Serial}, SimCardSerial {p.SimCardSerial}");
			return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("TerminalNotFoundCheckDetails"));
		}

		ULog.Debug($"Terminal found with ID: {terminal.Id}, Current MerchantId: {terminal.MerchantId}");

		if (terminal.MerchantId != null) {
			ULog.Warning($"CreateTerminal failed: Terminal {terminal.Id} is already bound to Merchant {terminal.MerchantId}");
			return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("TerminalAlreadyBound"));
		}

		// Find merchant
		ULog.Debug($"Fetching merchant with ID: {p.MerchantId}");
		MerchantEntity? merchant = await db.Set<MerchantEntity>().AsTracking().Include(x => x.User).FirstOrDefaultAsync(x => x.Id == p.MerchantId, ct);
		if (merchant == null) {
			ULog.Warning($"CreateTerminal failed: Merchant not found with ID {p.MerchantId}");
			return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("MerchantNotFound"));
		}

		ULog.Debug($"Merchant found: ID {merchant.Id}, NationalCode {merchant.NationalCode}, MerchantId: {merchant.MerchantId}");

		if (merchant.MerchantId.IsNotNullOrEmpty()) {
			ULog.Warning($"CreateTerminal failed: Merchant {merchant.Id} already registered with Avreen, MerchantId: {merchant.MerchantId}");
			return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("MerchantAlreadyRegisteredWithAvreen"));
		}

		if (merchant.User.ESignature == null) {
			ULog.Warning($"CreateTerminal failed: User {merchant.User.Id} has no ESignature");
			return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("UserESignatureMissing"));
		}

		ULog.Debug("User has ESignature present");

		// Generate agreement before making any external calls — fail fast if template is missing
		ULog.Info($"Generating agreement for merchant {merchant.Id}");
		string agreement;
		try {
			agreement = await GenerateAgreement(merchant.User, merchant, env.ContentRootPath);
		}
		catch (Exception ex) {
			ULog.Error(ex, $"Agreement generation failed for merchant {merchant.Id}");
			return new UResponse<Guid?>(null, Usc.InternalServerError, ls.Get("AgreementGenerationFailed"));
		}

		ULog.Debug($"Agreement generated for terminal {terminal.Id}");

		// Start transaction for Avreen integration
		ULog.Info($"Starting transaction for Avreen integration for terminal {terminal.Id}");
		await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(ct);
		try {
			terminal.MerchantId = p.MerchantId;
			terminal.Agreement = agreement.FromBase64();
			// Register merchant with Avreen
			ULog.Debug("Calling Avreen API to add merchant...");
			HttpResponseMessage? response = await http.Post(
				$"{Core.App.Avreen.BaseUrl}api/mms/ing/v2/addMerchant",
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

			if (response is null or { IsSuccessStatusCode: false }) {
				ULog.Error($"Avreen addMerchant API call failed. Response null or unsuccessful");
				await transaction.RollbackAsync(ct);
				ULog.Warning($"Transaction rolled back for terminal {terminal.Id}");
				return new UResponse<Guid?>(null, Usc.InternalServerError, ls.Get("AvreenAddMerchantFailed"));
			}

			ULog.Success("Avreen addMerchant API call successful");

			string responseContent = await response.Content.ReadAsStringAsync(ct);
			JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseContent);
			merchant.InsId = data.GetStringOrNull("insId")!;
			merchant.MerchantId = data.GetStringOrNull("merchantId")!;
			ULog.Debug($"Avreen response - InsId: {merchant.InsId}, MerchantId: {merchant.MerchantId}");

			if (merchant.MerchantId.IsNullOrEmpty()) {
				ULog.Error($"Avreen addMerchant response missing merchantId. Response: {responseContent}");
				await transaction.RollbackAsync(ct);
				ULog.Warning($"Transaction rolled back for terminal {terminal.Id}");
				return new UResponse<Guid?>(null, Usc.InternalServerError, ls.Get("AvreenMerchantIdMissing"));
			}

			// Bind terminal with Avreen
			ULog.Debug("Calling Avreen API to define and bind terminal...");
			HttpResponseMessage? terminalResponse = await http.Post(
				$"{Core.App.Avreen.BaseUrl}api/mms/ing/v2/defineAndBindTerminal",
				new {
					definitionTemplate = 1,
					merchantId = merchant.MerchantId,
					project = "AvaPlus",
					terminalSerial = terminal.Serial,
					terminalSerial2 = terminal.SimCardSerial
				},
				new Dictionary<string, string> { { "Authorization", $"{Core.App.Avreen.AuthHeader}" }, { "Accept", "application/json" } }
			);

			if (terminalResponse is null or { IsSuccessStatusCode: false }) {
				ULog.Error($"Avreen defineAndBindTerminal API call failed");
				await transaction.RollbackAsync(ct);
				ULog.Warning($"Transaction rolled back for terminal {terminal.Id}");
				return new UResponse<Guid?>(null, Usc.InternalServerError, ls.Get("AvreenBindTerminalFailed"));
			}

			ULog.Success("Avreen defineAndBindTerminal API call successful");

			string terminalResponseContent = await terminalResponse.Content.ReadAsStringAsync(ct);
			JsonElement terminalData = JsonSerializer.Deserialize<JsonElement>(terminalResponseContent);
			
			terminal.TerminalId = terminalData.GetStringOrNull("terminalId");
			terminal.InsId = terminalData.GetStringOrNull("insId");
			ULog.Debug($"Terminal updated - TerminalId: {terminal.TerminalId}, InsId: {terminal.InsId}, Tags: Verified added, AwaitingVerification removed");

			db.Set<TerminalEntity>().Update(terminal);
			await db.SaveChangesAsync(ct);
			await transaction.CommitAsync(ct);

			ULog.Success($"Terminal {terminal.Id} successfully bound to merchant {merchant.Id} via Avreen");
			return new UResponse<Guid?>(terminal.Id);
		}
		catch (Exception ex) {
			ULog.Error(ex, $"Exception during Avreen integration for terminal {terminal.Id}");
			await transaction.RollbackAsync(ct);
			ULog.Warning($"Transaction rolled back for terminal {terminal.Id} due to exception");
			return new UResponse<Guid?>(null, Usc.InternalServerError, ls.Get("UnexpectedError"));
		}
	}

	public async Task<UResponse<PnUserStatusResponse?>> UserStatus(PnPhoneNumberParams p, CancellationToken ct) {
		ULog.Info($"UserStatus called for phone number: {p.PhoneNumber}");

		// API Key validation
		if (p.ApiKey != Core.App.Pn.ApiKey) {
			ULog.Warning($"UserStatus failed: Invalid API key for phone {p.PhoneNumber}");
			return new UResponse<PnUserStatusResponse?>(null, Usc.UnAuthorized, ls.Get("InvalidAPIKey"));
		}

		ULog.Debug("API key validation passed");

		PnUserStatusResponse response = new();

		// Find user with all related data
		ULog.Debug($"Fetching user data for phone: {p.PhoneNumber}");
		UserResponse? e = await db.Set<UserEntity>()
			.Select(Projections.UserSelector(new UserSelectorArgs {
				Merchant = new MerchantSelectorArgs { Terminal = new TerminalSelectorArgs() },
				NationalCardFront = true,
				NationalCardBack = true,
				BirthCertificateFirst = true,
				VisualAuthentication = true,
				ESignature = true
			}))
			.FirstOrDefaultAsync(x => x.PhoneNumber == p.PhoneNumber, ct);

		if (e == null) {
			ULog.Warning($"UserStatus failed: User not found for phone {p.PhoneNumber}");
			return new UResponse<PnUserStatusResponse?>(null, Usc.NotFound, ls.Get("UserNotFound"));
		}

		ULog.Debug($"User found with ID: {e.Id}");

		// Build status response
		response.BirthCertificateFirst = new PnUserStatusItem {
			IsUploaded = e.BirthCertificateFirst.IsNotNullOrEmpty(),
			IsVerified = e.Tags.Contains(TagUser.BirthCertificateFirstVerified),
			RejectionReason = e.JsonData.BirthCertificateFirstRejectionReason
		};
		response.NationalCardFront = new PnUserStatusItem {
			IsUploaded = e.NationalCardFront.IsNotNullOrEmpty(),
			IsVerified = e.Tags.Contains(TagUser.NationalCardFrontVerified),
			RejectionReason = e.JsonData.NationalCardFrontRejectionReason
		};
		response.NationalCardBack = new PnUserStatusItem {
			IsUploaded = e.NationalCardBack.IsNotNullOrEmpty(),
			IsVerified = e.Tags.Contains(TagUser.NationalCardBackVerified),
			RejectionReason = e.JsonData.NationalCardBackRejectionReason
		};
		response.VisualAuthentication = new PnUserStatusItem {
			IsUploaded = e.VisualAuthentication.IsNotNullOrEmpty(),
			IsVerified = e.Tags.Contains(TagUser.VisualAuthenticationVerified),
			RejectionReason = e.JsonData.VisualAuthenticationRejectionReason
		};
		response.ESignature = new PnUserStatusItem {
			IsUploaded = e.ESignature.IsNotNullOrEmpty(),
			IsVerified = e.Tags.Contains(TagUser.ESignatureVerified),
			RejectionReason = e.JsonData.ESignatureRejectionReason
		};

		response.Merchants = e.Merchants ?? [];

		ULog.Debug($"User status summary - BirthCertificate: Uploaded={response.BirthCertificateFirst.IsUploaded}, Verified={response.BirthCertificateFirst.IsVerified}");
		ULog.Debug($"User status summary - NationalCardFront: Uploaded={response.NationalCardFront.IsUploaded}, Verified={response.NationalCardFront.IsVerified}");
		ULog.Debug($"User status summary - NationalCardBack: Uploaded={response.NationalCardBack.IsUploaded}, Verified={response.NationalCardBack.IsVerified}");
		ULog.Debug($"User status summary - VisualAuthentication: Uploaded={response.VisualAuthentication.IsUploaded}, Verified={response.VisualAuthentication.IsVerified}");
		ULog.Debug($"User status summary - ESignature: Uploaded={response.ESignature.IsUploaded}, Verified={response.ESignature.IsVerified}");
		ULog.Debug($"User has {response.Merchants.Count()} merchants");

		ULog.Success($"UserStatus completed for phone {p.PhoneNumber}");
		return new UResponse<PnUserStatusResponse?>(response);
	}

	public async Task<UResponse<TerminalSupportPasswordResponse?>> ReadTerminalSupportPassword(PnTerminalSupportPasswordParams p, CancellationToken ct) {
		ULog.Info($"Pn ReadTerminalSupportPassword called for terminal: {p.TerminalId}");

		if (p.ApiKey != Core.App.Pn.ApiKey) {
			ULog.Warning($"ReadTerminalSupportPassword failed: Invalid API key for terminal {p.TerminalId}");
			return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.UnAuthorized, ls.Get("InvalidAPIKey"));
		}

		TerminalEntity? e = await db.Set<TerminalEntity>().Include(x => x.Merchant).FirstOrDefaultAsync(x => x.Id == p.TerminalId, ct);
		if (e == null) {
			ULog.Warning($"ReadTerminalSupportPassword failed: Terminal not found with Id {p.TerminalId}");
			return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.NotFound, ls.Get("TerminalNotFound"));
		}

		if (e.Merchant == null) {
			ULog.Warning($"ReadTerminalSupportPassword failed: Terminal {e.Id} has no bound merchant");
			return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.NotFound, ls.Get("MerchantNotFound"));
		}

		HttpResponseMessage? response = await http.Post(
			$"{Core.App.Avreen.BaseUrl}api/mms/ing/v2/generateSupportPassword",
			new {
				insId = e.InsId,
				merchantId = e.Merchant.MerchantId,
				terminalId = e.TerminalId,
				terminalSerial = e.Serial,
				terminalSerial2 = e.SimCardSerial
			},
			new Dictionary<string, string> { { "Authorization", $"{Core.App.Avreen.AuthHeader}" }, { "Accept", "application/json" } }
		);

		if (response is null or { IsSuccessStatusCode: false }) {
			ULog.Error($"Avreen generateSupportPassword failed for terminal {e.Id}");
			return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
		}

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		ULog.Success($"Support password generated for terminal {e.Id}");
		return new UResponse<TerminalSupportPasswordResponse?>(new TerminalSupportPasswordResponse { Password = data.GetStringOrNull("supportPassword") });
	}

	public async Task<UResponse<ZipCodeToAddressDetailResponse?>> ZipCodeToAddress(PnZipCodeParams p, CancellationToken ct) {
		ULog.Info($"Pn ZipCodeToAddress called for zipCode: {p.ZipCode}");

		if (p.ApiKey != Core.App.Pn.ApiKey) {
			ULog.Warning($"ZipCodeToAddress failed: Invalid API key for zipCode {p.ZipCode}");
			return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.UnAuthorized, ls.Get("InvalidAPIKey"));
		}

		GetAccessTokenResponse? tokenResponse = await GetAccessToken(ct);
		if (tokenResponse?.AccessToken == null) {
			ULog.Error("ZipCodeToAddress failed: could not obtain ItHub access token");
			return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));
		}

		HttpResponseMessage? response = await http.Post(
			"https://gateway.itsaaz.ir/hub/api/v1/Address/DetailsTypeA",
			new { postcode = p.ZipCode, orderId = 1 },
			new Dictionary<string, string> { { "Authorization", $"Bearer {tokenResponse.AccessToken}" }, { "Accept", "application/json" } }
		);

		if (response == null) return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		if (!response.IsSuccessStatusCode) {
			string errorMessage = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("error").GetStringOrNull("customMessage") ?? ls.Get("ThirdPartyError");
			ULog.Warning($"ZipCodeToAddress third-party error for zipCode {p.ZipCode}: {errorMessage}");
			return new UResponse<ZipCodeToAddressDetailResponse?>(null, Usc.ThirdPartyError, errorMessage);
		}

		JsonElement json = JsonSerializer.Deserialize<JsonElement>(responseBody).GetProperty("data");
		ULog.Success($"ZipCodeToAddress resolved for zipCode {p.ZipCode}");
		return new UResponse<ZipCodeToAddressDetailResponse?>(new ZipCodeToAddressDetailResponse {
			BuildingName = json.GetStringOrNull("BuildingName"),
			Description = json.GetStringOrNull("description"),
			Floor = json.GetStringOrNull("floor"),
			HouseNumber = json.GetStringOrNull("houseNumber"),
			LocalityName = json.GetStringOrNull("localityName"),
			LocalityType = json.GetStringOrNull("localityType"),
			ZipCode = json.GetStringOrNull("zipCode"),
			Province = json.GetStringOrNull("province"),
			SideFloor = json.GetStringOrNull("sideFloor"),
			Street = json.GetStringOrNull("street"),
			Street2 = json.GetStringOrNull("street2"),
			SubLocality = json.GetStringOrNull("subLocality"),
			TownShip = json.GetStringOrNull("townShip"),
			TraceId = json.GetStringOrNull("traceId"),
			Village = json.GetStringOrNull("village")
		});
	}

	private async Task<GetAccessTokenResponse?> GetAccessToken(CancellationToken ct) {
		ItHub itHub = Core.App.ItHub;
		HttpResponseMessage? response = await http.PostForm(
			"https://gateway.itsaaz.ir/sts/connect/token",
			new Dictionary<string, string> {
				{ "grant_type", "password" },
				{ "client_id", itHub.ClientId },
				{ "Client_secret", itHub.ClientSecret },
				{ "username", itHub.UserName },
				{ "password", itHub.Password }
			}
		);
		if (response == null) return null;

		string responseBody = await response.Content.ReadAsStringAsync(ct);
		JsonElement data = JsonSerializer.Deserialize<JsonElement>(responseBody);
		return new GetAccessTokenResponse { AccessToken = data.GetStringOrNull("access_token"), ExpiresIn = data.GetIntOrNull("expires_in") };
	}

	private static async Task<string> GenerateAgreement(UserEntity user, MerchantEntity merchant, string contentRootPath) {
		ULog.Debug($"Generating agreement for user {user.Id}, merchant {merchant.Id}");
		return await WordPdfGenerator.GenerateWithTextsAndImagesAsync(
			texts: new Dictionary<string, string> {
				{ "day", PersianDateTime.Now.Day.ToString() },
				{ "month", PersianDateTime.Now.Month.ToString() },
				{ "number", "NUMBER" },
				{ "fullName", $"{user.FirstName ?? ""} {user.LastName ?? ""}" },
				{ "nationalCode", user.NationalCode ?? "" },
				{ "birthdate", PersianDateTime.FromDateTime(user.Birthdate ?? DateTime.Now).ToString("yyyy-MM-dd") },
				{ "address", "ADDRESS" },
				{ "postalCode", merchant.ZipCode },
				{ "phoneNumber", user.PhoneNumber ?? "" },
				{ "landLine", user.LandLine ?? "" },
				{ "fatherName", user.JsonData.FatherName ?? "" }
			},
			imagesBase64: new Dictionary<string, string> {
				{ "customerSignature", user.ESignature!.ToBase64()! }
			},
			templatePath: Path.Combine(contentRootPath, "Templates", "atmAgreement.docx")
		);
	}
}