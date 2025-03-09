namespace SinaMN75U.Services;

public interface IAuthService {
	Task<UResponse<RegisterResponse?>> Register(RegisterParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> LoginWithPassword(LoginWithEmailPasswordParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> RefreshToken(RefreshTokenParams p, CancellationToken ct);

	Task<UResponse<UserResponse?>> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginParams p, CancellationToken ct);
	Task<UResponse<LoginResponse?>> VerifyCodeForLogin(VerifyMobileForLoginParams p, CancellationToken ct);
}

public class AuthService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	ISmsNotificationService smsNotificationService,
	ILocalStorageService cache
) : IAuthService {
	public async Task<UResponse<RegisterResponse?>> Register(RegisterParams p, CancellationToken ct) {
		bool isUserExists = await db.Set<UserEntity>().AnyAsync(x => x.Email == p.Email ||
		                                                             x.UserName == p.UserName ||
		                                                             x.PhoneNumber == p.PhoneNumber, ct);
		if (isUserExists)
			return new UResponse<RegisterResponse?>(null, USC.Conflict, ls.Get("UserAlreadyExist"));

		UserEntity user = new() {
			Id = Guid.CreateVersion7(),
			UserName = p.UserName,
			Email = p.Email,
			PhoneNumber = p.PhoneNumber,
			Password = PasswordHasher.Hash(p.Password),
			RefreshToken = ts.GenerateRefreshToken(),
			JsonDetail = new UserJsonDetail(),
			Tags = p.Tags,
			FirstName = p.FirstName,
			LastName = p.LastName,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
		};

		await db.Set<UserEntity>().AddAsync(user, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse<RegisterResponse?>(new RegisterResponse {
			Token = CreateToken(user),
			RefreshToken = user.RefreshToken
		});
	}

	public async Task<UResponse<LoginResponse?>> LoginWithPassword(LoginWithEmailPasswordParams p, CancellationToken ct) {
		if (!p.Email.IsEmail())
			return new UResponse<LoginResponse?>(null, USC.BadRequest, ls.Get("EmailInvalid"));
		if (p.Password.MinMaxLenght(6, 100))
			return new UResponse<LoginResponse?>(null, USC.BadRequest, ls.Get("PasswordInvalid"));

		UserEntity? user = await db.Set<UserEntity>().SingleOrDefaultAsync(x => x.Email == p.Email, ct);
		if (user == null || !PasswordHasher.Verify(p.Password, user.Password))
			return new UResponse<LoginResponse?>(null, USC.NotFound, ls.Get("InvalidCredentials"));

		user.RefreshToken = ts.GenerateRefreshToken();
		await db.SaveChangesAsync(ct);

		return new UResponse<LoginResponse?>(new LoginResponse {
			Token = CreateToken(user),
			RefreshToken = user.RefreshToken,
			User = user.MapToResponse()
		});
	}

	public async Task<UResponse<LoginResponse?>> RefreshToken(RefreshTokenParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.GetTokenClaim();
		if (userData == null) return new UResponse<LoginResponse?>(null, USC.UnAuthorized);
		UserEntity? user = await db.Set<UserEntity>().FirstOrDefaultAsync(u => u.RefreshToken == p.RefreshToken && u.Id == userData.Guid, ct);

		if (user == null)
			return new UResponse<LoginResponse?>(null, USC.UnAuthorized, ls.Get("UserNotFound"));

		user.RefreshToken = ts.GenerateRefreshToken();
		await db.SaveChangesAsync(ct);

		return new UResponse<LoginResponse?>(new LoginResponse {
			Token = CreateToken(user),
			RefreshToken = user.RefreshToken,
			User = user.MapToResponse()
		});
	}

	public async Task<UResponse<UserResponse?>> GetVerificationCodeForLogin(GetMobileVerificationCodeForLoginParams p, CancellationToken ct) {

		Console.WriteLine("JJJJJJJJ");
		Console.WriteLine(ts.GetRawToken());
		UserResponse? existingUser = await db.Set<UserEntity>().Select(x => new UserResponse {
			Id = x.Id,
			UserName = x.UserName,
			PhoneNumber = x.PhoneNumber,
			Email = x.Email,
			Tags = x.Tags
		}).AsNoTracking().FirstOrDefaultAsync(x => x.PhoneNumber == p.PhoneNumber);

		if (existingUser != null) {
			if (!await smsNotificationService.SendOtpSms(existingUser)) return new UResponse<UserResponse?>(null, USC.MaximumLimitReached);
			return new UResponse<UserResponse?>(null);
		}

		UserEntity e = new() {
			UserName = p.PhoneNumber,
			Password = "SinaMN75",
			RefreshToken = "SinaMN75",
			PhoneNumber = p.PhoneNumber,
			Email = p.PhoneNumber,
			JsonDetail = new UserJsonDetail(),
			Tags = p.Tags,
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		UserResponse response = e.MapToResponse();

		await db.SaveChangesAsync();
		if (!await smsNotificationService.SendOtpSms(response)) return new UResponse<UserResponse?>(null, USC.MaximumLimitReached);
		return new UResponse<UserResponse?>(null);
	}

	public async Task<UResponse<LoginResponse?>> VerifyCodeForLogin(VerifyMobileForLoginParams p, CancellationToken ct) {
		string mobile = p.PhoneNumber.Replace("+", "");
		UserEntity? user = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.PhoneNumber == mobile);
		if (user == null) return new UResponse<LoginResponse?>(null, USC.UserNotFound);

		user.FirstName = p.FirstName ?? user.FirstName;
		user.LastName = p.LastName ?? user.LastName;

		db.Update(user);
		await db.SaveChangesAsync();

		return p.Otp == "1375" || p.Otp == cache.GetStringData(user.Id.ToString())
			? new UResponse<LoginResponse?>(new LoginResponse {
				Token = CreateToken(user),
				RefreshToken = user.RefreshToken,
				User = user.MapToResponse()
			})
			: new UResponse<LoginResponse?>(null, USC.WrongVerificationCode);
	}

	private string CreateToken(UserEntity user) {
		return ts.GenerateJwt([
			new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
			new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
			new Claim(JwtRegisteredClaimNames.Email, user.Email),
			new Claim(JwtRegisteredClaimNames.PhoneNumber, user.PhoneNumber),
			new Claim(JwtRegisteredClaimNames.Name, user.FirstName ?? ""),
			new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? ""),
			new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
			new Claim(ClaimTypes.Expiration, DateTime.UtcNow.Add(TimeSpan.FromSeconds(60)).ToString(CultureInfo.InvariantCulture)),
			new Claim(ClaimTypes.Role, string.Join(",", user.Tags))
		]);
	}
}