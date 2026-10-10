namespace SinaMN75U.Services;

public interface IAuthService {
	Task<UResponse<LoginResponse?>> Register(RegisterParams p, CancellationToken ct);
	Task<UResponse> CompleteProfile(AuthCompleteProfileParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> Login(LoginParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> RefreshToken(RefreshTokenParams p, CancellationToken ct);
	Task<UResponse> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> VerifyCodeForLogin(VerifyMobileForLoginParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> LoginOrRegister(RegisterParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> LoginWithGoogle(GoogleLoginParams p, CancellationToken ct);
	Task<UResponse> ForgotPassword(ForgotPasswordParams p, CancellationToken ct);
	Task<UResponse> ResetPassword(ResetPasswordParams p, CancellationToken ct);
}

public class AuthService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	ISmsNotificationService smsNotificationService,
	ILocalStorageService cache,
	IInquiryService inquiryService,
	IGoogleAuthService google,
	IEmailService email
) : IAuthService {
	public async Task<UResponse<LoginResponse?>> Register(RegisterParams p, CancellationToken ct) {
		bool isUserExists = await db.Set<UserEntity>().AnyAsync(x => x.UserName == p.UserName, ct);
		if (isUserExists) return new UResponse<LoginResponse?>(null, Usc.Conflict, ls.Get("accountAlreadyExistsWouldYouLikeToLogIn"));

		Guid userId = Guid.CreateVersion7();
		DateTime now = DateTime.UtcNow;
		string? referralCode = p.ReferralCode?.Trim().ToUpperInvariant();
		Guid? referrerId = referralCode.IsNullOrEmpty() ? null : await db.Set<UserEntity>().Where(x => x.JsonData.ReferralCode == referralCode).Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
		UserEntity e = new() {
			Id = userId,
			CreatorId = userId,
			CreatedAt = now,
			UserName = p.UserName,
			Email = p.Email,
			PhoneNumber = p.PhoneNumber,
			Password = UPasswordHasher.Hash(p.Password),
			RefreshToken = ts.GenerateRefreshToken(),
			RefreshTokenExpiresAt = ts.RefreshTokenExpiry(),
			Tags = p.Tags.Where(t => !JwtClaimData.IsRoleTag(t)).ToList(),
			FirstName = p.FirstName,
			LastName = p.LastName,
			NationalCode = p.NationalCode,
			JsonData = new UserJson { ReferrerId = referrerId },
			Wallets = [new WalletEntity { Id = userId, CreatorId = userId, CreatedAt = now, JsonData = new WalletJson(), Tags = [TagWallet.Primary], Balance = 0 }]
		};

		await db.Set<UserEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse<LoginResponse?>(new LoginResponse {
			Token = ts.GenerateJwt(e),
			RefreshToken = e.RefreshToken,
			RefreshTokenExpiresAt = e.RefreshTokenExpiresAt,
			User = e.MapToResponse()
		});
	}

	public async Task<UResponse> CompleteProfile(AuthCompleteProfileParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<bool?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		UserEntity? e = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == userData.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("accountNotFound"));
		
		if (!userData.IsAdmin && userData.Id != e.Id) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		
		UResponse<bool?> shahkarResponse = await inquiryService.MobileAndNationalCodeVerification(new VerifyNationalCodeAndPhoneNumber {
			ApiKey = p.ApiKey,
			Token = p.Token,
			NationalCode = p.NationalCode,
			PhoneNumber = e.PhoneNumber!
		}, ct);

		if (shahkarResponse.Result == null) return new UResponse(Usc.ShahkarException, ls.Get("shahkarIsNotAvailableAtThisTimePleaseTryAgainLater"));
		if (shahkarResponse.Result == false) return new UResponse(Usc.ShahkarError, ls.Get("nationalCodeIsNotMatchWithPhoneNumberOwner"));

		e.NationalCode = p.NationalCode;
		if (p.FirstName.IsNotNullOrEmpty()) e.FirstName = p.FirstName;
		if (p.LastName.IsNotNullOrEmpty()) e.LastName = p.LastName;

		await db.SaveChangesAsync(ct);
		
		return new UResponse<UserResponse?>(e.MapToResponse(), message: ls.Get("yourDetailSubmittedSuccessfully"));
	}

	public async Task<UResponse<LoginResponse?>> Login(LoginParams p, CancellationToken ct) {
		if (p.Email.IsNullOrEmpty() && p.UserName.IsNullOrEmpty()) return new UResponse<LoginResponse?>(null, Usc.NotFound, ls.Get("loginInformationIsWrong"));

		string lockKey = "lockout_login_" + (p.UserName.IsNotNullOrEmpty() ? p.UserName : p.Email);
		if (IsLockedOut(lockKey)) return new UResponse<LoginResponse?>(null, Usc.TooManyRequests, ls.Get("tooManyFailedAttemptsPleaseTryAgainLater"));

		IQueryable<UserEntity> users = db.Set<UserEntity>().AsTracking();
		if (p.UserName.IsNotNullOrEmpty() && p.Email.IsNotNullOrEmpty()) users = users.Where(x => x.UserName == p.UserName || x.Email == p.Email);
		else if (p.UserName.IsNotNullOrEmpty()) users = users.Where(x => x.UserName == p.UserName);
		else users = users.Where(x => x.Email == p.Email);
		UserEntity? user = await users.FirstOrDefaultAsync(ct);
		if (user == null && p.UserName.IsNotNullOrEmpty() && PhoneForms(p.UserName).Count > 1) user = await ByPhone(db.Set<UserEntity>().AsTracking(), p.UserName).FirstOrDefaultAsync(ct);
		bool otpDefault = user?.Email != null && user.Email == user.PhoneNumber && p.Password == user.PhoneNumber;
		if (user == null || otpDefault || !UPasswordHasher.Verify(p.Password, user.Password)) {
			RegisterFailedAttempt(lockKey);
			return new UResponse<LoginResponse?>(null, Usc.NotFound, ls.Get("loginInformationIsWrong"));
		}

		ResetFailedAttempts(lockKey);
		user.RefreshToken = ts.GenerateRefreshToken();
		user.RefreshTokenExpiresAt = ts.RefreshTokenExpiry();
		await db.SaveChangesAsync(ct);

		return new UResponse<LoginResponse?>(new LoginResponse {
			Token = ts.GenerateJwt(user),
			RefreshToken = user.RefreshToken,
			RefreshTokenExpiresAt = user.RefreshTokenExpiresAt,
			User = user.MapToResponse()
		});
	}

	public async Task<UResponse<LoginResponse?>> RefreshToken(RefreshTokenParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<LoginResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		UserEntity? user = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(u => u.RefreshToken == p.RefreshToken && u.Id == userData.Id, ct);
		if (user == null) return new UResponse<LoginResponse?>(null, Usc.UnAuthorized, ls.Get("accountNotFound"));
		if (user.RefreshTokenExpiresAt.HasValue && user.RefreshTokenExpiresAt.Value.ToUniversalTime() < DateTime.UtcNow)
			return new UResponse<LoginResponse?>(null, Usc.ExpiredRefreshToken, ls.Get("yourSessionHasExpiredPleaseSignInAgain"));

		user.RefreshTokenExpiresAt = ts.RefreshTokenExpiry();
		await db.SaveChangesAsync(ct);

		return new UResponse<LoginResponse?>(new LoginResponse {
			Token = ts.GenerateJwt(user),
			RefreshToken = user.RefreshToken,
			RefreshTokenExpiresAt = user.RefreshTokenExpiresAt,
			User = user.MapToResponse()
		});
	}

	private static List<string> PhoneForms(string? phone) {
		if (phone.IsNullOrEmpty()) return [];
		string digits = new(phone.Where(char.IsAsciiDigit).ToArray());
		string local = digits.StartsWith("0098") ? digits[4..] : digits.StartsWith("98") && digits.Length == 12 ? digits[2..] : digits.StartsWith('0') ? digits[1..] : digits;
		return local.Length == 10 && local.StartsWith('9') ? [phone, "0" + local, "+98" + local, "98" + local] : [phone];
	}

	private IQueryable<UserEntity> ByPhone(IQueryable<UserEntity> q, string? phone) {
		List<string> forms = PhoneForms(phone);
		return q.Where(x => x.PhoneNumber != null && forms.Contains(x.PhoneNumber)).OrderByDescending(x => x.PhoneNumber == phone);
	}

	public async Task<UResponse> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginParams p, CancellationToken ct) {
		UserResponse? existingUser = await ByPhone(db.Set<UserEntity>(), p.PhoneNumber).Select(x => new UserResponse {
			Id = x.Id,
			PhoneNumber = x.PhoneNumber,
			JsonData = x.JsonData,
			Tags = x.Tags
		}).FirstOrDefaultAsync(ct);

		if (existingUser != null) {
			if (!await smsNotificationService.SendOtpSms(existingUser)) return new UResponse(Usc.MaximumLimitReached, ls.Get("tooManyOTPRequestsPleaseWaitAndTryAgain"));
			return new UResponse(message: ls.Get("verificationCodeSent"));
		}

		Guid userId = Guid.CreateVersion7();
		DateTime now = DateTime.UtcNow;

		UserEntity e = new() {
			Id = userId,
			CreatedAt = now,
			UserName = p.PhoneNumber,
			Password = UPasswordHasher.Hash(Guid.NewGuid().ToString()),
			RefreshToken = ts.GenerateRefreshToken(),
			RefreshTokenExpiresAt = ts.RefreshTokenExpiry(),
			PhoneNumber = p.PhoneNumber,
			JsonData = new UserJson(),
			Tags = [],
			CreatorId = Core.App.Users.SystemAdmin.Id,
			Wallets = [new WalletEntity { Id = userId, CreatorId = userId, CreatedAt = now, JsonData = new WalletJson(), Tags = [TagWallet.Primary], Balance = 0 }]
		};

		await db.Set<UserEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		if (!await smsNotificationService.SendOtpSms(e.MapToResponse())) return new UResponse(Usc.MaximumLimitReached, ls.Get("tooManyOTPRequestsPleaseWaitAndTryAgain"));

		return new UResponse();
	}

	public async Task<UResponse<LoginResponse?>> VerifyCodeForLogin(VerifyMobileForLoginParams p, CancellationToken ct) {
		string lockKey = "lockout_otp_" + PhoneForms(p.PhoneNumber).Last();
		if (IsLockedOut(lockKey)) return new UResponse<LoginResponse?>(null, Usc.TooManyRequests, ls.Get("tooManyFailedAttemptsPleaseTryAgainLater"));

		UserEntity? user = await ByPhone(db.Set<UserEntity>().AsTracking(), p.PhoneNumber).FirstOrDefaultAsync(ct);
		if (user == null) return new UResponse<LoginResponse?>(null, Usc.UserNotFound, ls.Get("accountNotFound"));

		if (p.Otp != Core.App.BasicSettings.DefaultVerificationKey && p.Otp != cache.Get("otp_" + user.Id)) {
			RegisterFailedAttempt(lockKey);
			return new UResponse<LoginResponse?>(null, Usc.WrongVerificationCode, ls.Get("enteredOtpIsNotValidOrExpiredPleaseTryAgain"));
		}

		ResetFailedAttempts(lockKey);
		cache.Set("otp_" + user.Id, "", TimeSpan.FromSeconds(1));
		user.RefreshToken = ts.GenerateRefreshToken();
		user.RefreshTokenExpiresAt = ts.RefreshTokenExpiry();
		await db.SaveChangesAsync(ct);

		return new UResponse<LoginResponse?>(new LoginResponse {
			Token = ts.GenerateJwt(user),
			RefreshToken = user.RefreshToken,
			RefreshTokenExpiresAt = user.RefreshTokenExpiresAt,
			User = user.MapToResponse()
		});
	}

	public async Task<UResponse<LoginResponse?>> LoginOrRegister(RegisterParams p, CancellationToken ct) {
		if (p.PhoneNumber.IsNullOrEmpty() || p.NationalCode.IsNullOrEmpty()) return new UResponse<LoginResponse?>(null, Usc.BadRequest, ls.Get("loginInformationIsWrong"));
		UserEntity? user = await ByPhone(db.Set<UserEntity>().AsTracking().Where(x => x.NationalCode == p.NationalCode), p.PhoneNumber).FirstOrDefaultAsync(ct);
		if (user == null) return await Register(p, ct);
		if (JwtClaimData.Rank(user.Tags) > 0) return new UResponse<LoginResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		user.RefreshToken = ts.GenerateRefreshToken();
		user.RefreshTokenExpiresAt = ts.RefreshTokenExpiry();
		await db.SaveChangesAsync(ct);

		return new UResponse<LoginResponse?>(new LoginResponse {
			Token = ts.GenerateJwt(user),
			RefreshToken = user.RefreshToken,
			RefreshTokenExpiresAt = user.RefreshTokenExpiresAt,
			User = user.MapToResponse()
		});
	}

	public async Task<UResponse<LoginResponse?>> LoginWithGoogle(GoogleLoginParams p, CancellationToken ct) {
		if (!google.IsConfigured) return new UResponse<LoginResponse?>(null, Usc.BadRequest, ls.Get("googleSignInIsNotConfigured"));

		GoogleUser? g = await google.Validate(p.IdToken, ct);
		if (g == null) return new UResponse<LoginResponse?>(null, Usc.UnAuthorized, ls.Get("googleTokenIsInvalid"));
		if (!g.EmailVerified) return new UResponse<LoginResponse?>(null, Usc.Forbidden, ls.Get("googleEmailIsNotVerified"));

		UserEntity? user = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.JsonData.GoogleId == g.Id, ct)
		                   ?? await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Email == g.Email, ct);

		if (user == null) {
			Guid userId = Guid.CreateVersion7();
			DateTime now = DateTime.UtcNow;
			bool userNameTaken = await db.Set<UserEntity>().AnyAsync(x => x.UserName == g.Email, ct);
			user = new UserEntity {
				Id = userId,
				CreatorId = userId,
				CreatedAt = now,
				UserName = userNameTaken ? $"google_{g.Id}" : g.Email,
				Email = g.Email,
				FirstName = g.FirstName,
				LastName = g.LastName,
				Password = UPasswordHasher.Hash(ts.GenerateRefreshToken()),
				RefreshToken = ts.GenerateRefreshToken(),
				RefreshTokenExpiresAt = ts.RefreshTokenExpiry(),
				Tags = [TagUser.Unspecified],
				JsonData = new UserJson { GoogleId = g.Id },
				Wallets = [new WalletEntity { Id = userId, CreatorId = userId, CreatedAt = now, JsonData = new WalletJson(), Tags = [TagWallet.Primary], Balance = 0 }]
			};
			await db.Set<UserEntity>().AddAsync(user, ct);
		}
		else {
			user.JsonData.GoogleId ??= g.Id;
			if (user.FirstName.IsNullOrEmpty()) user.FirstName = g.FirstName;
			if (user.LastName.IsNullOrEmpty()) user.LastName = g.LastName;
			user.RefreshToken = ts.GenerateRefreshToken();
			user.RefreshTokenExpiresAt = ts.RefreshTokenExpiry();
		}

		await db.SaveChangesAsync(ct);

		return new UResponse<LoginResponse?>(new LoginResponse {
			Token = ts.GenerateJwt(user),
			RefreshToken = user.RefreshToken,
			RefreshTokenExpiresAt = user.RefreshTokenExpiresAt,
			User = user.MapToResponse()
		});
	}

	public async Task<UResponse> ForgotPassword(ForgotPasswordParams p, CancellationToken ct) {
		// Same answer whether or not the email exists, so the endpoint can't be used to discover accounts.
		UResponse sent = new(message: ls.Get("ifAnAccountExistsAResetCodeWasSent"));

		UserEntity? user = await FindByEmail(p.Email, ct);
		if (user == null || user.Email.IsNullOrEmpty()) return sent;
		if (cache.Get("pwreset_sent_" + user.Id) != null) return sent;

		string code = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
		cache.Set("pwreset_" + user.Id, code, PasswordResetCodeLifetime);
		cache.Set("pwreset_sent_" + user.Id, "1", TimeSpan.FromMinutes(1));

		await email.Send(user.Email, ls.Get("passwordResetEmailSubject"), string.Format(ls.Get("passwordResetEmailBody"), code, PasswordResetCodeLifetime.TotalMinutes), ct);
		return sent;
	}

	public async Task<UResponse> ResetPassword(ResetPasswordParams p, CancellationToken ct) {
		string lockKey = "lockout_pwreset_" + p.Email.Trim().ToLowerInvariant();
		if (IsLockedOut(lockKey)) return new UResponse(Usc.TooManyRequests, ls.Get("tooManyFailedAttemptsPleaseTryAgainLater"));

		UserEntity? user = await FindByEmail(p.Email, ct, tracking: true);
		string? expected = user == null ? null : cache.Get("pwreset_" + user.Id);
		if (user == null || expected.IsNullOrEmpty() || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(p.Code.Trim()))) {
			RegisterFailedAttempt(lockKey);
			return new UResponse(Usc.WrongVerificationCode, ls.Get("passwordResetCodeIsInvalidOrExpired"));
		}

		ResetFailedAttempts(lockKey);
		cache.Set("pwreset_" + user.Id, "", TimeSpan.FromSeconds(1));
		user.Password = UPasswordHasher.Hash(p.NewPassword);
		user.RefreshToken = ts.GenerateRefreshToken(); // signs out other sessions
		user.RefreshTokenExpiresAt = ts.RefreshTokenExpiry();
		await db.SaveChangesAsync(ct);
		return new UResponse(message: ls.Get("passwordChangedSuccessfully"));
	}

	private async Task<UserEntity?> FindByEmail(string email, CancellationToken ct, bool tracking = false) {
		string trimmed = email.Trim();
		string lower = trimmed.ToLowerInvariant();
		IQueryable<UserEntity> users = tracking ? db.Set<UserEntity>().AsTracking() : db.Set<UserEntity>();
		return await users.FirstOrDefaultAsync(x => x.Email == trimmed || x.Email == lower, ct);
	}

	private static readonly TimeSpan PasswordResetCodeLifetime = TimeSpan.FromMinutes(15);
	private const int MaxFailedAttempts = 5;
	private static readonly TimeSpan LockoutWindow = TimeSpan.FromMinutes(15);

	private bool IsLockedOut(string key) => int.TryParse(cache.Get(key), out int attempts) && attempts >= MaxFailedAttempts;

	private void RegisterFailedAttempt(string key) {
		int attempts = int.TryParse(cache.Get(key), out int a) ? a : 0;
		cache.Set(key, (attempts + 1).ToString(), LockoutWindow);
	}

	private void ResetFailedAttempts(string key) => cache.Set(key, "0", TimeSpan.FromSeconds(1));
}