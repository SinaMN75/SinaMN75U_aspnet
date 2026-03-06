namespace SinaMN75U.Services;

public interface IAuthService {
	Task<UResponse<LoginResponse?>> Register(RegisterParams p, CancellationToken ct);
	Task<UResponse<UserResponse?>> CompleteProfile(AuthCompleteProfileParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> LoginWithEmailPassword(LoginWithEmailPasswordParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> LoginWithUserNamePassword(LoginWithUserNamePasswordParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> RefreshToken(RefreshTokenParams p, CancellationToken ct);
	Task<UResponse> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> VerifyCodeForLogin(VerifyMobileForLoginParams p, CancellationToken ct);
	Task<UResponse<UserEntity?>> ReadUserByToken(BaseParams p, CancellationToken ct);
}

public class AuthService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	ISmsNotificationService smsNotificationService,
	ILocalStorageService cache,
	IITHubService iTHubService,
	IWalletService walletService
) : IAuthService {
	public async Task<UResponse<LoginResponse?>> Register(RegisterParams p, CancellationToken ct) {
		bool isUserExists = await db.Set<UserEntity>().AnyAsync(x => x.UserName == p.UserName, ct);
		if (isUserExists)
			return new UResponse<LoginResponse?>(null, Usc.Conflict, ls.Get("UserAlreadyExist"));

		UserEntity user = new() {
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

		await db.Set<UserEntity>().AddAsync(user, ct);
		await db.SaveChangesAsync(ct);
		await walletService.Create(user.Id, ct);

		return new UResponse<LoginResponse?>(new LoginResponse {
			Token = CreateToken(user),
			RefreshToken = user.RefreshToken,
			Expires = Core.App.Jwt.Expires,
			User = user.MapToResponse()
		});
	}

	public async Task<UResponse<UserResponse?>> CompleteProfile(AuthCompleteProfileParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UserResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserEntity? e = await db.Set<UserEntity>().Include(x => x.Extra).AsTracking().FirstOrDefaultAsync(x => x.Id == userData.Id, ct);
		if (e == null) return new UResponse<UserResponse?>(null, Usc.NotFound);

		if (e.Extra.NotVerifiedNationalCodes.ContainsSafe(p.NationalCode))
			return new UResponse<UserResponse?>(null, Usc.ShahkarError, ls.Get("NationalCodeNotMatchWithPhoneNumberOwner"));

		ItHubBaseResponse<bool?> shahkarResponse = await iTHubService.Shahkar(new ITHubShahkarParams {
			NationalCode = p.NationalCode,
			Mobile = e.PhoneNumber!,
		}, ct);

		if (shahkarResponse.Error?.ErrorCode != null && shahkarResponse.Error?.ErrorCode != 400) return new UResponse<UserResponse?>(null, Usc.ShahkarError, ls.Get("ShahkarIsNotAvailableAtThisTime"));
		if (shahkarResponse.Error?.ErrorCode == 400 || shahkarResponse.Data == false) {
			e.Extra.NotVerifiedNationalCodes.Add(p.NationalCode);
			db.Set<UserEntity>().Update(e);
			await db.SaveChangesAsync(ct);
			return new UResponse<UserResponse?>(null, Usc.ShahkarError, ls.Get("NationalCodeNotMatchWithPhoneNumberOwner"));
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
		if (user == null || !UPasswordHasher.Verify(p.Password, user.Password))
			return new UResponse<LoginResponse?>(null, Usc.NotFound, ls.Get("InvalidCredentials"));

		user.RefreshToken = ts.GenerateRefreshToken();
		await db.SaveChangesAsync(ct);

		return new UResponse<LoginResponse?>(new LoginResponse {
			Token = CreateToken(user),
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
			Token = CreateToken(user),
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
			Token = CreateToken(user),
			RefreshToken = user.RefreshToken,
			Expires = Core.App.Jwt.Expires,
			User = user.MapToResponse()
		});
	}

	public async Task<UResponse> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginParams p, CancellationToken ct) {
		UserEntity? existingUser = await db.Set<UserEntity>().Select(x => new UserEntity {
			Id = x.Id,
			UserName = x.UserName,
			PhoneNumber = x.PhoneNumber,
			Email = x.Email,
			JsonData = x.JsonData,
			Tags = x.Tags,
			Password = "",
			RefreshToken = "",
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt
		}).AsNoTracking().FirstOrDefaultAsync(x => x.PhoneNumber == p.PhoneNumber, ct);

		if (existingUser != null) {
			if (!await smsNotificationService.SendOtpSms(existingUser)) return new UResponse(Usc.MaximumLimitReached, ls.Get("MaxOtpReached"));
			return new UResponse();
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
				UserId = userId,
				JsonData = new UserExtraJson(),
				Tags = []
			}
		};

		await db.Set<UserEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		await walletService.Create(e.Id, ct);
		if (!await smsNotificationService.SendOtpSms(e)) return new UResponse(Usc.MaximumLimitReached, ls.Get("MaxOtpReached"));

		return new UResponse();
	}

	public async Task<UResponse<LoginResponse?>> VerifyCodeForLogin(VerifyMobileForLoginParams p, CancellationToken ct) {
		string mobile = p.PhoneNumber.Replace("+", "");
		UserEntity? user = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.PhoneNumber == mobile, ct);
		if (user == null) return new UResponse<LoginResponse?>(null, Usc.UserNotFound);

		user.FirstName = p.FirstName ?? user.FirstName;
		user.LastName = p.LastName ?? user.LastName;

		db.Update(user);
		await db.SaveChangesAsync(ct);

		return p.Otp == Core.App.BasicSettings.DefaultVerificationKey || p.Otp == cache.Get(user.Id.ToString())
			? new UResponse<LoginResponse?>(new LoginResponse {
					Token = CreateToken(user),
					RefreshToken = user.RefreshToken,
					User = user.MapToResponse(),
					Expires = Core.App.Jwt.Expires
				}
			)
			: new UResponse<LoginResponse?>(null, Usc.WrongVerificationCode);
	}

	public async Task<UResponse<UserEntity?>> ReadUserByToken(BaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UserEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserEntity? user = await db.Set<UserEntity>()
			.Include(x => x.Media)
			.Include(x => x.Categories)
			.Include(x => x.Addresses)
			.Include(x => x.Wallets)
			.FirstOrDefaultAsync(x => x.Id == userData.Id, ct);
		return user == null ? new UResponse<UserEntity?>(null, Usc.NotFound, ls.Get("UserNotFound")) : new UResponse<UserEntity?>(user);
	}

	private string CreateToken(UserEntity user) => ts.GenerateJwt([
			new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
			new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
			new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
			new Claim(JwtRegisteredClaimNames.PhoneNumber, user.PhoneNumber ?? ""),
			new Claim(JwtRegisteredClaimNames.Name, user.FirstName ?? ""),
			new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? ""),
			new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
			new Claim(ClaimTypes.Expiration, DateTime.UtcNow.Add(TimeSpan.FromSeconds(60)).ToString(CultureInfo.InvariantCulture)),
			new Claim(ClaimTypes.Role, string.Join(",", user.Tags))
		],
		DateTime.UtcNow.AddMinutes(60)
	);
}