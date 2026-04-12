namespace SinaMN75U.Services;

public interface IAuthService {
	Task<UResponse<LoginResponse?>> Register(RegisterParams p, CancellationToken ct);
	Task<UResponse> CompleteProfile(AuthCompleteProfileParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> LoginWithEmailPassword(LoginWithEmailPasswordParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> LoginWithUserNamePassword(LoginWithUserNamePasswordParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> RefreshToken(RefreshTokenParams p, CancellationToken ct);
	Task<UResponse> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> VerifyCodeForLogin(VerifyMobileForLoginParams p, CancellationToken ct);
	Task<UResponse<UserResponse?>> ReadUserByToken(BaseParams p, CancellationToken ct);
}

public class AuthService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	ISmsNotificationService smsNotificationService,
	ILocalStorageService cache,
	IInquiryService inquiryService,
	IWalletService walletService,
	IUserService userService
) : IAuthService {
	public async Task<UResponse<LoginResponse?>> Register(RegisterParams p, CancellationToken ct) {
		bool isUserExists = await db.Set<UserEntity>().AnyAsync(x => x.UserName == p.UserName, ct);
		if (isUserExists) return new UResponse<LoginResponse?>(null, Usc.Conflict, ls.Get("UserAlreadyExist"));

		UserEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			UserName = p.UserName,
			Email = p.Email,
			PhoneNumber = p.PhoneNumber,
			Password = UPasswordHasher.Hash(p.Password),
			RefreshToken = ts.GenerateRefreshToken(),
			JsonData = new UserJson(),
			Tags = p.Tags,
			FirstName = p.FirstName,
			LastName = p.LastName
		};

		await db.Set<UserEntity>().AddAsync(e, ct);
		await userService.CreateExtra(e.Id, ct);
		await walletService.Create(e.Id, ct);
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

		if (e.Extra.NotVerifiedNationalCodes.ContainsSafe(p.NationalCode))
			return new UResponse(Usc.ShahkarError, ls.Get("NationalCodeNotMatchWithPhoneNumberOwner"));

		UResponse<bool?> shahkarResponse = await inquiryService.Shahkar(new VerifyNationalCodeAndPhoneNumber {
			NationalCode = p.NationalCode,
			Mobile = e.PhoneNumber!
		}, ct);

		if (shahkarResponse.Result == null) return new UResponse(Usc.ShahkarException, ls.Get("ShahkarIsNotAvailableAtThisTime"));
		if (shahkarResponse.Result == false) {
			e.Extra.NotVerifiedNationalCodes.Add(p.NationalCode);
			db.Set<UserEntity>().Update(e);
			await db.SaveChangesAsync(ct);
			return new UResponse(Usc.ShahkarError, ls.Get("NationalCodeNotMatchWithPhoneNumberOwner"));
		}

		e.UpdatedAt = DateTime.UtcNow;
		e.NationalCode = p.NationalCode;
		e.FirstName = p.FirstName;
		e.LastName = p.LastName;
		db.Set<UserEntity>().Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<UserResponse?>(e.MapToResponse(), message: ls.Get("YourDetailSubmittedSuccessfully"));
	}

	public async Task<UResponse<LoginResponse?>> LoginWithEmailPassword(LoginWithEmailPasswordParams p, CancellationToken ct) {
		UserEntity? user = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.Email == p.Email, ct);
		if (user == null || !UPasswordHasher.Verify(p.Password, user.Password)) return new UResponse<LoginResponse?>(null, Usc.NotFound, ls.Get("InvalidCredentials"));

		user.RefreshToken = ts.GenerateRefreshToken();
		await db.SaveChangesAsync(ct);

		return new UResponse<LoginResponse?>(new LoginResponse {
			Token = ts.GenerateJwt(user),
			RefreshToken = user.RefreshToken,
			Expires = Core.App.Jwt.Expires,
			User = user.MapToResponse()
		});
	}

	public async Task<UResponse<LoginResponse?>> LoginWithUserNamePassword(LoginWithUserNamePasswordParams p, CancellationToken ct) {
		UserEntity? user = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.UserName == p.UserName, ct);
		if (user == null || !UPasswordHasher.Verify(p.Password, user.Password))
			return new UResponse<LoginResponse?>(null, Usc.NotFound, ls.Get("InvalidCredentials"));

		user.RefreshToken = ts.GenerateRefreshToken();
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

		if (user == null)
			return new UResponse<LoginResponse?>(null, Usc.UnAuthorized, ls.Get("UserNotFound"));

		user.RefreshToken = ts.GenerateRefreshToken();
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

		UserEntity e = new() {
			Id = userId,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			UserName = p.PhoneNumber,
			Password = "SinaMN75",
			RefreshToken = "SinaMN75",
			PhoneNumber = p.PhoneNumber,
			Email = p.PhoneNumber,
			JsonData = new UserJson(),
			Tags = [],
			Extra = new UserExtraEntity {
				Id = Guid.CreateVersion7(),
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
				UserId = userId,
				JsonData = new GeneralJsonData(),
				Tags = []
			}
		};

		await db.Set<UserEntity>().AddAsync(e, ct);
		await userService.CreateExtra(e.Id, ct);
		await walletService.Create(e.Id, ct);
		await db.SaveChangesAsync(ct);
		if (!await smsNotificationService.SendOtpSms(e.MapToResponse())) return new UResponse(Usc.MaximumLimitReached, ls.Get("MaxOtpReached"));

		return new UResponse();
	}

	public async Task<UResponse<LoginResponse?>> VerifyCodeForLogin(VerifyMobileForLoginParams p, CancellationToken ct) {
		string mobile = p.PhoneNumber.Replace("+", "");
		UserEntity? user = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.PhoneNumber == mobile, ct);
		if (user == null) return new UResponse<LoginResponse?>(null, Usc.UserNotFound);

		db.Update(user);
		await db.SaveChangesAsync(ct);

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

	public async Task<UResponse<UserResponse?>> ReadUserByToken(BaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UserResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserEntity? user = await db.Set<UserEntity>()
			.Include(x => x.Media)
			.Include(x => x.Categories)
			.Include(x => x.Addresses)
			.Include(x => x.Wallets)
			.FirstOrDefaultAsync(x => x.Id == userData.Id, ct);
		return user == null ? new UResponse<UserResponse?>(null, Usc.NotFound, ls.Get("UserNotFound")) : new UResponse<UserResponse?>(user.MapToResponse());
	}
}