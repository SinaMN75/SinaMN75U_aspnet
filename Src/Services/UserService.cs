using System.IO.Compression;

namespace SinaMN75U.Services;

public interface IUserService {
	public Task<UResponse<Guid?>> Create(UserCreateParams p, CancellationToken ct);
	public Task<UResponse> BulkCreate(UserBulkCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<UserResponse>?>> Read(UserReadParams p, CancellationToken ct);
	public Task<UResponse<UserResponse?>> ReadById(IdParams<UserSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> Update(UserUpdateParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdParams p, CancellationToken ct);
	public Task<UResponse<UserExtraResponse?>> ReadExtraById(IdParams p, CancellationToken ct);
	public Task<UResponse> UpdateExtra(UserExtraUpdateParams p, CancellationToken ct);
	public Task<UResponse<UserExtraStatusResponse?>> ReadExtraStatusById(IdParams p, CancellationToken ct);
	public Task<UResponse<string?>> DownloadUserData(IdParams p, CancellationToken ct);
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
		UserEntity e = new() {
			Id = userId,
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new UserJson { FcmToken = p.FcmToken, FatherName = p.FatherName, Weight = p.Weight, Height = p.Height },
			Tags = p.Tags,
			LandLine = p.LandLine,
			UserName = p.UserName,
			Password = p.Password,
			RefreshToken = UPasswordHasher.Hash(p.Password),
			PhoneNumber = p.PhoneNumber,
			NationalCode = p.NationalCode,
			Email = p.Email,
			FirstName = p.FirstName,
			LastName = p.LastName,
			Bio = p.Bio,
			Birthdate = p.Birthdate,
			Extra = new UserExtraEntity { Id = userId, CreatorId = userId, CreatedAt = now, JsonData = new BaseJsonData(), Tags = [] },
			Wallets = [new WalletEntity { Id = userId, CreatorId = userId, CreatedAt = now, JsonData = new BaseJsonData(), Tags = [TagWallet.Primary], Balance = 0 }]
		};

		if (p.Categories.IsNotNullOrEmpty()) {
			List<CategoryEntity> list = [];
			foreach (Guid item in p.Categories!) {
				CategoryEntity? c = await db.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == item, ct);
				if (c != null) list.Add(c);
			}

			e.Categories = list;
		}

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
				Extra = new UserExtraEntity { Id = userId, CreatorId = userId, CreatedAt = now, JsonData = new BaseJsonData(), Tags = [] },
				Wallets = [new WalletEntity { Id = userId, CreatorId = userId, CreatedAt = now, JsonData = new BaseJsonData(), Tags = [TagWallet.Primary], Balance = 0 }]
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
		if (p.LastName.IsNotNullOrEmpty()) q = q.Where(u => (u.LastName ?? "").Contains(p.UserName!));
		if (p.PhoneNumber.IsNotNullOrEmpty()) q = q.Where(u => u.PhoneNumber == p.PhoneNumber);
		if (p.LandLine.IsNotNullOrEmpty()) q = q.Where(u => u.LandLine == p.LandLine);
		if (p.Email.IsNotNullOrEmpty()) q = q.Where(u => u.Email == p.Email);
		if (p.Bio.IsNotNullOrEmpty()) q = q.Where(u => (u.Bio ?? "").Contains(p.Bio));
		if (p.NationalCode.IsNotNullOrEmpty()) q = q.Where(u => u.NationalCode == p.NationalCode);
		if (p.StartBirthDate.HasValue) q = q.Where(u => u.Birthdate >= p.StartBirthDate);
		if (p.EndBirthDate.HasValue) q = q.Where(u => u.Birthdate <= p.EndBirthDate);
		if (p.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories.Any(y => p.Categories!.Contains(y.Id)));

		if (p.OrderByFirstName) q = q.OrderBy(x => x.FirstName);
		if (p.OrderByFirstNameDesc) q = q.OrderByDescending(x => x.FirstName);
		if (p.OrderByLastName) q = q.OrderBy(x => x.LastName);
		if (p.OrderByLastNameDesc) q = q.OrderByDescending(x => x.LastName);

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
		if (e == null) return new UResponse(Usc.NotFound);

		if (!userData.IsAdmin && userData.Id != e.CreatorId) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

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

		if (p.Categories.IsNotNullOrEmpty()) {
			List<CategoryEntity> list = await db.Set<CategoryEntity>().AsTracking().Where(x => p.Categories.Contains(x.Id)).OrderByDescending(x => x.Id).ToListAsync(ct);
			e.Categories.AddRangeIfNotExist(list);
		}

		db.Set<UserEntity>().Update(e.ApplyUpdateParam<UserEntity, TagUser, UserJson>(p));
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

	public async Task<UResponse<UserExtraResponse?>> ReadExtraById(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UserExtraResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserExtraEntity? e = await db.Set<UserExtraEntity>().FirstOrDefaultAsync(x => x.CreatorId == p.Id, ct);
		if (e == null) return new UResponse<UserExtraResponse?>(null, Usc.NotFound);

		if (!userData.IsAdmin && userData.Id != e.CreatorId) return new UResponse<UserExtraResponse?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		return new UResponse<UserExtraResponse?>(new UserExtraResponse {
			NationalCardFront = e.NationalCardFront.ToBase64(),
			NationalCardBack = e.NationalCardBack.ToBase64(),
			BirthCertificateFirst = e.BirthCertificateFirst.ToBase64(),
			BirthCertificateSecond = e.BirthCertificateSecond.ToBase64(),
			BirthCertificateThird = e.BirthCertificateThird.ToBase64(),
			BirthCertificateForth = e.BirthCertificateForth.ToBase64(),
			BirthCertificateFifth = e.BirthCertificateFifth.ToBase64(),
			VisualAuthentication = e.VisualAuthentication.ToBase64(),
			ESignature = e.ESignature.ToBase64()
		});
	}

	public async Task<UResponse> UpdateExtra(UserExtraUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserExtraEntity? e = await db.Set<UserExtraEntity>().FirstOrDefaultAsync(x => x.CreatorId == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound);

		if (!userData.IsAdmin && userData.Id != e.CreatorId) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		if (p.NationalCardFront.IsNotNullOrEmpty()) e.NationalCardFront = p.NationalCardFront.FromBase64();
		if (p.NationalCardBack.IsNotNullOrEmpty()) e.NationalCardBack = p.NationalCardBack.FromBase64();
		if (p.BirthCertificateFirst.IsNotNullOrEmpty()) e.BirthCertificateFirst = p.BirthCertificateFirst.FromBase64();
		if (p.BirthCertificateSecond.IsNotNullOrEmpty()) e.BirthCertificateSecond = p.BirthCertificateSecond.FromBase64();
		if (p.BirthCertificateThird.IsNotNullOrEmpty()) e.BirthCertificateThird = p.BirthCertificateThird.FromBase64();
		if (p.BirthCertificateForth.IsNotNullOrEmpty()) e.BirthCertificateForth = p.BirthCertificateForth.FromBase64();
		if (p.BirthCertificateFifth.IsNotNullOrEmpty()) e.BirthCertificateFifth = p.BirthCertificateFifth.FromBase64();
		if (p.VisualAuthentication.IsNotNullOrEmpty()) e.VisualAuthentication = p.VisualAuthentication.FromBase64();
		if (p.ESignature.IsNotNullOrEmpty()) e.ESignature = p.ESignature.FromBase64();

		db.Set<UserExtraEntity>().Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<UserExtraStatusResponse?>> ReadExtraStatusById(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UserExtraStatusResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserExtraEntity? e = await db.Set<UserExtraEntity>().FirstOrDefaultAsync(x => x.CreatorId == p.Id, ct);
		if (e == null) return new UResponse<UserExtraStatusResponse?>(null, Usc.NotFound);

		return new UResponse<UserExtraStatusResponse?>(new UserExtraStatusResponse {
				NationalCardFront = e.NationalCardFront != null,
				NationalCardBack = e.NationalCardBack != null,
				BirthCertificateFirst = e.BirthCertificateFirst != null,
				BirthCertificateSecond = e.BirthCertificateSecond != null,
				BirthCertificateThird = e.BirthCertificateThird != null,
				BirthCertificateForth = e.BirthCertificateForth != null,
				BirthCertificateFifth = e.BirthCertificateFifth != null,
				VisualAuthentication = e.VisualAuthentication != null,
				ESignature = e.ESignature != null
			}
		);
	}

	public async Task<UResponse<string?>> DownloadUserData(IdParams p, CancellationToken ct) {
		// JwtClaimData? userData = ts.ExtractClaims(p.Token);
		// if (userData == null) return new UResponse<string?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		//
		// if (!userData.IsAdmin) return new UResponse<string?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		UserEntity? e = await db.Set<UserEntity>().Include(x => x.Extra).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<string?>(null, Usc.NotFound);

		string firstName = e.FirstName ?? "---";
		string lastName = e.LastName ?? "---";
		string phoneNumber = e.PhoneNumber ?? "---";
		string email = e.Email ?? "---";
		string landLine = e.LandLine ?? "---";
		string nationalCode = e.NationalCode ?? "---";
		string birthdate = (e.Birthdate ?? DateTime.UtcNow).ToPersianString();
		string fatherName = e.JsonData.FatherName ?? "---";

		string nationalCardFront = e.Extra.NationalCardFront.ToBase64() ?? "";
		string nationalCardBack = e.Extra.NationalCardBack.ToBase64() ?? "";
		string birthCertificateFirst = e.Extra.BirthCertificateFirst.ToBase64() ?? "";
		string visualAuthentication = e.Extra.VisualAuthentication.ToBase64() ?? "";
		string eSignature = e.Extra.ESignature.ToBase64() ?? "";

		using MemoryStream memoryStream = new MemoryStream();
		await using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true)) {
			ZipArchiveEntry textEntry = archive.CreateEntry("UserData.txt");
			await using (Stream textStream = await textEntry.OpenAsync(ct))
			await using (StreamWriter textWriter = new StreamWriter(textStream)) {
				await textWriter.WriteAsync($"First Name: {firstName}\n");
				await textWriter.WriteAsync($"Last Name: {lastName}\n");
				await textWriter.WriteAsync($"Phone Number: {phoneNumber}\n");
				await textWriter.WriteAsync($"Email: {email}\n");
				await textWriter.WriteAsync($"Land Line: {landLine}\n");
				await textWriter.WriteAsync($"National Code: {nationalCode}\n");
				await textWriter.WriteAsync($"Birthdate: {birthdate}\n");
				await textWriter.WriteAsync($"Father Name: {fatherName}\n");
			}

			AddFileToZip(archive, "NationalCardFront.jpg", nationalCardFront);
			AddFileToZip(archive, "NationalCardBack.jpg", nationalCardBack);
			AddFileToZip(archive, "BirthCertificateFirst.jpg", birthCertificateFirst);
			AddFileToZip(archive, "VisualAuthentication.jpg", visualAuthentication);
			AddFileToZip(archive, "ESignature.png", eSignature);
		}

		string downloadToken = Guid.NewGuid().ToString();
		byte[] zipBytes = memoryStream.ToArray();
		cache.Set(downloadToken, zipBytes, TimeSpan.FromMinutes(30));
		string downloadUrl = $"{Core.App.BaseUrl}/api/download/{downloadToken}";
		return new UResponse<string?>(downloadUrl, Usc.Success, "Download link generated");
	}

	private void AddFileToZip(ZipArchive archive, string fileName, string base64Content) {
		if (!string.IsNullOrEmpty(base64Content)) {
			ZipArchiveEntry entry = archive.CreateEntry(fileName);
			using Stream entryStream = entry.Open();
			byte[] bytes = Convert.FromBase64String(base64Content);
			entryStream.Write(bytes, 0, bytes.Length);
		}
	}
}