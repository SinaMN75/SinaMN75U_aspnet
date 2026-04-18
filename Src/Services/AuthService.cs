namespace SinaMN75U.Services;

public interface IAuthService {
	Task<UResponse<LoginResponse?>> Register(RegisterParams p, CancellationToken ct);
	Task<UResponse> CompleteProfile(AuthCompleteProfileParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> Login(LoginParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> RefreshToken(RefreshTokenParams p, CancellationToken ct);
	Task<UResponse> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> VerifyCodeForLogin(VerifyMobileForLoginParams p, CancellationToken ct);
}

public class AuthService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	ISmsNotificationService smsNotificationService,
	ILocalStorageService cache,
	IInquiryService inquiryService
) : IAuthService {
	public async Task<UResponse<LoginResponse?>> Register(RegisterParams p, CancellationToken ct) {
		bool isUserExists = await db.Set<UserEntity>().AnyAsync(x => x.UserName == p.UserName, ct);
		if (isUserExists) return new UResponse<LoginResponse?>(null, Usc.Conflict, ls.Get("UserAlreadyExist"));

		Guid userId = Guid.CreateVersion7();
		DateTime now = DateTime.UtcNow;
		UserEntity e = new() {
			Id = userId,
			CreatorId = userId,
			CreatedAt = now,
			UserName = p.UserName,
			Email = p.Email,
			PhoneNumber = p.PhoneNumber,
			Password = UPasswordHasher.Hash(p.Password),
			RefreshToken = ts.GenerateRefreshToken(),
			Tags = p.Tags,
			FirstName = p.FirstName,
			LastName = p.LastName,
			JsonData = new UserJson(),
			Extra = new UserExtraEntity { Id = userId, CreatorId = userId, CreatedAt = now, JsonData = new BaseJsonData(), Tags = [] },
			Wallets = [new WalletEntity { Id = userId, CreatorId = userId, CreatedAt = now, JsonData = new BaseJsonData(), Tags = [TagWallet.Primary], Balance = 0 }]
		};

		await db.Set<UserEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse<LoginResponse?>(new LoginResponse {
			Token = ts.GenerateJwt(e),
			RefreshToken = e.RefreshToken,
			Expires = Core.App.Jwt.Expires,
			User = e.MapToResponse()
		});
	}

	public async Task<UResponse> CompleteProfile(AuthCompleteProfileParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserEntity? e = await db.Set<UserEntity>().Include(x => x.Extra).AsTracking().FirstOrDefaultAsync(x => x.Id == userData.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound);
		
		if (!userData.IsAdmin || userData.Id != e.Id) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));
		
		UResponse<bool?> shahkarResponse = await inquiryService.MobileAndNationalCodeVerification(new VerifyNationalCodeAndPhoneNumber {
			NationalCode = p.NationalCode,
			PhoneNumber = e.PhoneNumber!
		}, ct);

		if (shahkarResponse.Result == null) return new UResponse(Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));
		if (shahkarResponse.Result == false) return new UResponse(Usc.ShahkarError, ls.Get("NationalCodeNotMatchWithPhoneNumberOwner"));

		e.NationalCode = p.NationalCode;
		if (p.FirstName.IsNotNullOrEmpty()) e.FirstName = p.FirstName;
		if (p.LastName.IsNotNullOrEmpty()) e.LastName = p.LastName;
		
		db.Set<UserEntity>().Update(e);
		await db.SaveChangesAsync(ct);
		
		return new UResponse<UserResponse?>(e.MapToResponse(), message: ls.Get("YourDetailSubmittedSuccessfully"));
	}

	public async Task<UResponse<LoginResponse?>> Login(LoginParams p, CancellationToken ct) {
		if (p.Email.IsNullOrEmpty() && p.UserName.IsNullOrEmpty()) return new UResponse<LoginResponse?>(null, Usc.NotFound, ls.Get("InvalidCredentials"));
		
		UserEntity? user = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.UserName == p.UserName || x.Email == p.Email, ct);
		if (user == null || !UPasswordHasher.Verify(p.Password, user.Password)) return new UResponse<LoginResponse?>(null, Usc.NotFound, ls.Get("InvalidCredentials"));

		user.RefreshToken = ts.GenerateRefreshToken();
		db.Set<UserEntity>().Update(user);
		await db.SaveChangesAsync(ct);

		return new UResponse<LoginResponse?>(new LoginResponse {
			Token = ts.GenerateJwt(user),
			RefreshToken = user.RefreshToken,
			Expires = Core.App.Jwt.Expires,
			User = user.MapToResponse()
		});
	}

	public async Task<UResponse<LoginResponse?>> RefreshToken(RefreshTokenParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<LoginResponse?>(null, Usc.UnAuthorized);
		UserEntity? user = await db.Set<UserEntity>().FirstOrDefaultAsync(u => u.RefreshToken == p.RefreshToken && u.Id == userData.Id, ct);

		if (user == null) return new UResponse<LoginResponse?>(null, Usc.UnAuthorized, ls.Get("UserNotFound"));

		user.RefreshToken = ts.GenerateRefreshToken();
		db.Set<UserEntity>().Update(user);
		await db.SaveChangesAsync(ct);

		return new UResponse<LoginResponse?>(new LoginResponse {
			Token = ts.GenerateJwt(user),
			RefreshToken = user.RefreshToken,
			Expires = Core.App.Jwt.Expires,
			User = user.MapToResponse()
		});
	}

	public async Task<UResponse> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginParams p, CancellationToken ct) {
		UserResponse? existingUser = await db.Set<UserEntity>().Select(x => new UserResponse {
			Id = x.Id,
			UserName = x.UserName,
			PhoneNumber = x.PhoneNumber,
			Email = x.Email,
			JsonData = x.JsonData,
			Tags = x.Tags
		}).FirstOrDefaultAsync(x => x.PhoneNumber == p.PhoneNumber, ct);

		if (existingUser != null) {
			if (!await smsNotificationService.SendOtpSms(existingUser)) return new UResponse(Usc.MaximumLimitReached, ls.Get("MaxOtpReached"));
			return new UResponse(message: ls.Get("OtpSent"));
		}

		Guid userId = Guid.CreateVersion7();
		DateTime now = DateTime.UtcNow;

		UserEntity e = new() {
			Id = userId,
			CreatedAt = now,
			UserName = p.PhoneNumber,
			Password = "SinaMN75",
			RefreshToken = "SinaMN75",
			PhoneNumber = p.PhoneNumber,
			Email = p.PhoneNumber,
			JsonData = new UserJson(),
			Tags = [],
			CreatorId = Core.App.Users.SystemAdmin.Id,
			Extra = new UserExtraEntity { Id = userId, CreatorId = userId, CreatedAt = now, JsonData = new BaseJsonData(), Tags = [] },
			Wallets = [new WalletEntity { Id = userId, CreatorId = userId, CreatedAt = now, JsonData = new BaseJsonData(), Tags = [TagWallet.Primary], Balance = 0 }]
		};

		await db.Set<UserEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		if (!await smsNotificationService.SendOtpSms(e.MapToResponse())) return new UResponse(Usc.MaximumLimitReached, ls.Get("MaxOtpReached"));

		return new UResponse();
	}

	public async Task<UResponse<LoginResponse?>> VerifyCodeForLogin(VerifyMobileForLoginParams p, CancellationToken ct) {
		UserEntity? user = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.PhoneNumber == p.PhoneNumber, ct);
		if (user == null) return new UResponse<LoginResponse?>(null, Usc.UserNotFound);

		return p.Otp == Core.App.BasicSettings.DefaultVerificationKey || p.Otp == cache.Get(user.Id.ToString())
			? new UResponse<LoginResponse?>(new LoginResponse {
					Token = ts.GenerateJwt(user),
					RefreshToken = user.RefreshToken,
					User = user.MapToResponse(),
					Expires = Core.App.Jwt.Expires
				}
			)
			: new UResponse<LoginResponse?>(null, Usc.WrongVerificationCode);
	}
}