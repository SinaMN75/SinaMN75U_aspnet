namespace SinaMN75U.Services;

public interface IUserService {
	public Task<UResponse<Guid?>> Create(UserCreateParams p, CancellationToken ct);
	public Task<UResponse> BulkCreate(UserBulkCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<UserResponse>?>> Read(UserReadParams p, CancellationToken ct);
	public Task<UResponse<UserResponse?>> ReadById(IdParams<UserSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> Update(UserUpdateParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdParams p, CancellationToken ct);
	public Task<UResponse<string?>> DownloadUserData(IdParams p, CancellationToken ct);
	public Task<UResponse<bool?>> IsUserAuthenticated(BaseParams p, CancellationToken ct);
}

public class UserService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IMemoryCache cache
) : IUserService {
	public async Task<UResponse<Guid?>> Create(UserCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!userData.IsAdmin) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		Guid userId = p.Id ?? Guid.CreateVersion7();
		DateTime now = DateTime.UtcNow;
		List<CategoryEntity>? categories = null;
		if (p.Categories.IsNotNullOrEmpty()) categories = await db.Set<CategoryEntity>().Where(x => p.Categories!.Contains(x.Id)).ToListAsync(ct);

		UserEntity e = new() {
			Id = userId,
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new UserJson { FcmToken = p.FcmToken, FatherName = p.FatherName, Weight = p.Weight, Height = p.Height },
			Tags = p.Tags,
			LandLine = p.LandLine,
			UserName = p.UserName,
			Password = UPasswordHasher.Hash(p.Password),
			RefreshToken = ts.GenerateRefreshToken(),
			PhoneNumber = p.PhoneNumber,
			NationalCode = p.NationalCode,
			Email = p.Email,
			FirstName = p.FirstName,
			LastName = p.LastName,
			Bio = p.Bio,
			Birthdate = p.Birthdate,
			NationalCardFront = p.NationalCardFront.FromBase64(),
			NationalCardBack = p.NationalCardBack.FromBase64(),
			BirthCertificateFirst = p.BirthCertificateFirst.FromBase64(),
			BirthCertificateSecond = p.BirthCertificateSecond.FromBase64(),
			BirthCertificateThird = p.BirthCertificateThird.FromBase64(),
			BirthCertificateForth = p.BirthCertificateForth.FromBase64(),
			BirthCertificateFifth = p.BirthCertificateFifth.FromBase64(),
			ESignature = p.ESignature.FromBase64(),
			VisualAuthentication = p.VisualAuthentication.FromBase64(),
			Categories = categories ?? [],
			Wallets = [new WalletEntity { Id = userId, CreatorId = userId, CreatedAt = now, JsonData = new WalletJson(), Tags = [TagWallet.Primary], Balance = 0 }]
		};

		await db.Set<UserEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse> BulkCreate(UserBulkCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		if (p.Users.Count == 0) return new UResponse(Usc.BadRequest, ls.Get("AtLeastOneUserRequired"));

		List<UserEntity> entities = [];
		List<Guid> categoryIds = p.Users.SelectMany(u => u.Categories ?? []).Distinct().ToList();
		List<CategoryEntity> categories = await db.Set<CategoryEntity>().Where(c => categoryIds.Contains(c.Id)).ToListAsync(ct);

		DateTime now = DateTime.UtcNow;
		entities.AddRange(p.Users.Select(userParam => {
			Guid userId = Guid.CreateVersion7();
			return new UserEntity {
				Id = userId,
				CreatorId = userData.Id,
				UserName = userParam.UserName,
				LandLine = userParam.LandLine,
				Password = UPasswordHasher.Hash(userParam.Password),
				RefreshToken = "",
				PhoneNumber = userParam.PhoneNumber,
				Email = userParam.Email,
				FirstName = userParam.FirstName,
				LastName = userParam.LastName,
				Bio = userParam.Bio,
				Birthdate = userParam.Birthdate,
				NationalCode = userParam.NationalCode,
				JsonData = new UserJson { FcmToken = userParam.FcmToken, Weight = userParam.Weight, Height = userParam.Height, FatherName = userParam.FatherName },
				Tags = userParam.Tags,
				CreatedAt = DateTime.UtcNow,
				Categories = categories.Where(c => userParam.Categories?.Contains(c.Id) ?? false).ToList(),
				Wallets = [new WalletEntity { Id = userId, CreatorId = userId, CreatedAt = now, JsonData = new WalletJson(), Tags = [TagWallet.Primary], Balance = 0 }]
			};
		}));

		await db.Set<UserEntity>().AddRangeAsync(entities, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse(Usc.Created);
	}

	public async Task<UResponse<IEnumerable<UserResponse>?>> Read(UserReadParams p, CancellationToken ct) {
		IQueryable<UserEntity> q = db.Set<UserEntity>().ApplyReadParams<UserEntity, TagUser, UserJson>(p);

		if (p.UserName.IsNotNullOrEmpty()) q = q.Where(u => u.UserName.Contains(p.UserName!));
		if (p.FirstName.IsNotNullOrEmpty()) q = q.Where(u => (u.FirstName ?? "").Contains(p.FirstName!));
		if (p.LastName.IsNotNullOrEmpty()) q = q.Where(u => (u.LastName ?? "").Contains(p.LastName!));
		if (p.PhoneNumber.IsNotNullOrEmpty()) q = q.Where(u => u.PhoneNumber == p.PhoneNumber);
		if (p.LandLine.IsNotNullOrEmpty()) q = q.Where(u => u.LandLine == p.LandLine);
		if (p.Email.IsNotNullOrEmpty()) q = q.Where(u => u.Email == p.Email);
		if (p.Bio.IsNotNullOrEmpty()) q = q.Where(u => (u.Bio ?? "").Contains(p.Bio));
		if (p.NationalCode.IsNotNullOrEmpty()) q = q.Where(u => u.NationalCode == p.NationalCode);
		if (p.StartBirthDate.HasValue) q = q.Where(u => u.Birthdate >= p.StartBirthDate);
		if (p.EndBirthDate.HasValue) q = q.Where(u => u.Birthdate <= p.EndBirthDate);
		if (p.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories.Any(y => p.Categories!.Contains(y.Id)));
		
		IQueryable<UserResponse> projected = q.Select(Projections.UserSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<UserResponse?>> ReadById(IdParams<UserSelectorArgs> p, CancellationToken ct) {
		UserResponse? e = await db.Set<UserEntity>().Select(Projections.UserSelector(p.SelectorArgs)).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<UserResponse?>(null, Usc.NotFound, ls.Get("UserNotFound")) : new UResponse<UserResponse?>(e);
	}

	public async Task<UResponse> Update(UserUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserEntity? e = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("UserNotFound"));

		if (!userData.IsAdmin && userData.Id != e.Id) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		if (p.Password.IsNotNullOrEmpty()) e.Password = UPasswordHasher.Hash(p.Password);
		if (p.FirstName.IsNotNullOrEmpty()) e.FirstName = p.FirstName;
		if (p.LastName.IsNotNullOrEmpty()) e.LastName = p.LastName;
		if (p.UserName.IsNotNullOrEmpty()) e.UserName = p.UserName;
		if (p.LandLine.IsNotNullOrEmpty()) e.LandLine = p.LandLine;
		if (p.PhoneNumber.IsNotNullOrEmpty()) e.PhoneNumber = p.PhoneNumber;
		if (p.Email.IsNotNullOrEmpty()) e.Email = p.Email;
		if (p.Bio.IsNotNullOrEmpty()) e.Bio = p.Bio;
		if (p.Birthdate.HasValue) e.Birthdate = p.Birthdate;
		if (p.NationalCode.IsNotNullOrEmpty()) e.NationalCode = p.NationalCode;
		if (p.FcmToken.IsNotNullOrEmpty()) e.JsonData.FcmToken = p.FcmToken;
		if (p.FatherName.IsNotNullOrEmpty()) e.JsonData.FatherName = p.FatherName;
		if (p.Weight.IsNotNullOrZero()) e.JsonData.Weight = p.Weight;
		if (p.Height.IsNotNullOrZero()) e.JsonData.Height = p.Height;
		if (p.NationalCardFront.IsNotNullOrEmpty()) e.NationalCardFront = ImageCompressor.CompressBase64(p.NationalCardFront);
		if (p.NationalCardBack.IsNotNullOrEmpty()) e.NationalCardBack = ImageCompressor.CompressBase64(p.NationalCardBack);
		if (p.BirthCertificateFirst.IsNotNullOrEmpty()) e.BirthCertificateFirst = ImageCompressor.CompressBase64(p.BirthCertificateFirst);
		if (p.BirthCertificateSecond.IsNotNullOrEmpty()) e.BirthCertificateSecond = ImageCompressor.CompressBase64(p.BirthCertificateSecond);
		if (p.BirthCertificateThird.IsNotNullOrEmpty()) e.BirthCertificateThird = ImageCompressor.CompressBase64(p.BirthCertificateThird);
		if (p.BirthCertificateForth.IsNotNullOrEmpty()) e.BirthCertificateForth = ImageCompressor.CompressBase64(p.BirthCertificateForth);
		if (p.BirthCertificateFifth.IsNotNullOrEmpty()) e.BirthCertificateFifth = ImageCompressor.CompressBase64(p.BirthCertificateFifth);
		if (p.VisualAuthentication.IsNotNullOrEmpty()) e.VisualAuthentication = p.VisualAuthentication.FromBase64();
		if (p.ESignature.IsNotNullOrEmpty()) e.ESignature = ImageCompressor.CompressBase64(p.ESignature, 10);

		if (p.NationalCardFrontRejectionReason != null) e.JsonData.NationalCardFrontRejectionReason = p.NationalCardFrontRejectionReason;
		if (p.NationalCardBackRejectionReason != null) e.JsonData.NationalCardBackRejectionReason = p.NationalCardBackRejectionReason;
		if (p.BirthCertificateFirstRejectionReason != null) e.JsonData.BirthCertificateFirstRejectionReason = p.BirthCertificateFirstRejectionReason;
		if (p.BirthCertificateSecondRejectionReason != null) e.JsonData.BirthCertificateSecondRejectionReason = p.BirthCertificateSecondRejectionReason;
		if (p.BirthCertificateThirdRejectionReason != null) e.JsonData.BirthCertificateThirdRejectionReason = p.BirthCertificateThirdRejectionReason;
		if (p.BirthCertificateForthRejectionReason != null) e.JsonData.BirthCertificateForthRejectionReason = p.BirthCertificateForthRejectionReason;
		if (p.BirthCertificateFifthRejectionReason  != null) e.JsonData.BirthCertificateFifthRejectionReason = p.BirthCertificateFifthRejectionReason;
		if (p.VisualAuthenticationRejectionReason != null) e.JsonData.VisualAuthenticationRejectionReason = p.VisualAuthenticationRejectionReason;
		if (p.ESignatureRejectionReason != null) e.JsonData.ESignatureRejectionReason = p.ESignatureRejectionReason;

		if (p.Categories.IsNotNullOrEmpty()) {
			List<CategoryEntity> list = await db.Set<CategoryEntity>().AsTracking().Where(x => p.Categories.Contains(x.Id)).OrderByDescending(x => x.Id).ToListAsync(ct);
			e.Categories.AddRangeIfNotExist(list);
		}

		e.ApplyUpdateParam<UserEntity, TagUser, UserJson>(p);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserEntity? e = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("UserNotFound"));

		if (!userData.IsAdmin && userData.Id != e.CreatorId) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		db.Set<UserEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<string?>> DownloadUserData(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<string?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		if (!userData.IsAdmin) return new UResponse<string?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		UserEntity? e = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<string?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		string firstName = e.FirstName ?? "---";
		string lastName = e.LastName ?? "---";
		string phoneNumber = e.PhoneNumber ?? "---";
		string email = e.Email ?? "---";
		string landLine = e.LandLine ?? "---";
		string nationalCode = e.NationalCode ?? "---";
		string birthdate = (e.Birthdate ?? DateTime.UtcNow).ToPersianString();
		string fatherName = e.JsonData.FatherName ?? "---";

		string nationalCardFront = e.NationalCardFront.ToBase64() ?? "";
		string nationalCardBack = e.NationalCardBack.ToBase64() ?? "";
		string birthCertificateFirst = e.BirthCertificateFirst.ToBase64() ?? "";
		string visualAuthentication = e.VisualAuthentication.ToBase64() ?? "";
		string eSignature = e.ESignature.ToBase64() ?? "";

		StringBuilder data = new();
		data.AppendLine($"First Name: {firstName}");
		data.AppendLine($"Last Name: {lastName}");
		data.AppendLine($"Phone Number: {phoneNumber}");
		data.AppendLine($"Email: {email}");
		data.AppendLine($"Land Line: {landLine}");
		data.AppendLine($"National Code: {nationalCode}");
		data.AppendLine($"Birthdate: {birthdate}");
		data.AppendLine($"Father Name: {fatherName}");

		byte[] zipBytes = await ZipUtils.CreateZipAsync(
			new Dictionary<string, string> { ["UserData.txt"] = data.ToString() },
			new Dictionary<string, string> {
				["NationalCardFront.jpg"] = nationalCardFront,
				["NationalCardBack.jpg"] = nationalCardBack,
				["BirthCertificateFirst.jpg"] = birthCertificateFirst,
				["VisualAuthentication.mp4"] = visualAuthentication,
				["ESignature.png"] = eSignature
			},
			ct
		);

		string downloadToken = $"{nationalCode} - {firstName} {lastName}";
		cache.Set(downloadToken, zipBytes, TimeSpan.FromMinutes(30));
		string downloadUrl = $"{Core.App.BaseUrl}/api/download/{downloadToken}";

		return new UResponse<string?>(downloadUrl, Usc.Success, "Download link generated");
	}

	public async Task<UResponse<bool?>> IsUserAuthenticated(BaseParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<bool?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserResponse? e = await db.Set<UserEntity>()
			.Select(Projections.UserSelector(new UserSelectorArgs {
				Wallet = new WalletSelectorArgs(),
				NationalCardFront = true,
				NationalCardBack = true,
				BirthCertificateFirst = true,
				BirthCertificateSecond = true,
				BirthCertificateThird = true,
				BirthCertificateForth = true,
				BirthCertificateFifth = true,
				VisualAuthentication = true,
				ESignature = true
			}))
			.FirstOrDefaultAsync(x => x.Id == userData.Id, ct);
		
		if (e == null) return new UResponse<bool?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		if (
			e.NationalCardFront.IsNullOrEmpty() ||
			e.Tags.Contains(TagUser.NationalCardFrontVerified) ||
			e.NationalCardBack.IsNullOrEmpty() ||
			e.Tags.Contains(TagUser.NationalCardBackVerified) ||
			e.BirthCertificateFirst.IsNullOrEmpty() ||
			e.Tags.Contains(TagUser.BirthCertificateFirstVerified) ||
			e.VisualAuthentication.IsNullOrEmpty() ||
			e.Tags.Contains(TagUser.VisualAuthenticationVerified) ||
			e.ESignature.IsNullOrEmpty() ||
			e.Tags.Contains(TagUser.ESignatureVerified)
		) return new UResponse<bool?>(true);
		
		return new UResponse<bool?>(false);
	}
}