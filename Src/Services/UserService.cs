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
}

public class UserService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
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

		if (!userData.IsAdmin || userData.Id != e.CreatorId) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		if (p.Password.IsNotNullOrEmpty()) e.Password = UPasswordHasher.Hash(p.Password);
		if (p.FirstName.IsNotNullOrEmpty()) e.FirstName = p.FirstName;
		if (p.LastName.IsNotNullOrEmpty()) e.LastName = p.LastName;
		if (p.UserName.IsNotNullOrEmpty()) e.UserName = p.UserName;
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

		db.Set<UserEntity>().Update(e.ApplyUpdateParam<UserEntity,TagUser, UserJson>(p));
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserEntity? e = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("UserNotFound"));

		if (!userData.IsAdmin || userData.Id != e.CreatorId) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		db.Set<UserEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<UserExtraResponse?>> ReadExtraById(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UserExtraResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserExtraEntity? e = await db.Set<UserExtraEntity>().FirstOrDefaultAsync(x => x.CreatorId == p.Id, ct);
		if (e == null) return new UResponse<UserExtraResponse?>(null, Usc.NotFound);

		if (!userData.IsAdmin || userData.Id != e.CreatorId) return new UResponse<UserExtraResponse?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		return new UResponse<UserExtraResponse?>(new UserExtraResponse {
			NationalCardFront = e.NationalCardFront,
			NationalCardBack = e.NationalCardBack,
			BirthCertificateFirst = e.BirthCertificateFirst,
			BirthCertificateSecond = e.BirthCertificateSecond,
			BirthCertificateThird = e.BirthCertificateThird,
			BirthCertificateForth = e.BirthCertificateForth,
			BirthCertificateFifth = e.BirthCertificateFifth,
			VisualAuthentication = e.VisualAuthentication,
			ESignature = e.ESignature
		});
	}

	public async Task<UResponse> UpdateExtra(UserExtraUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserExtraEntity? e = await db.Set<UserExtraEntity>().FirstOrDefaultAsync(x => x.CreatorId == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound);

		if (!userData.IsAdmin || userData.Id != e.CreatorId) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		if (p.NationalCardFront.IsNotNullOrEmpty()) e.NationalCardFront = p.NationalCardFront;
		if (p.NationalCardBack.IsNotNullOrEmpty()) e.NationalCardBack = p.NationalCardBack;
		if (p.BirthCertificateFirst.IsNotNullOrEmpty()) e.BirthCertificateFirst = p.BirthCertificateFirst;
		if (p.BirthCertificateSecond.IsNotNullOrEmpty()) e.BirthCertificateSecond = p.BirthCertificateSecond;
		if (p.BirthCertificateThird.IsNotNullOrEmpty()) e.BirthCertificateThird = p.BirthCertificateThird;
		if (p.BirthCertificateForth.IsNotNullOrEmpty()) e.BirthCertificateForth = p.BirthCertificateForth;
		if (p.BirthCertificateFifth.IsNotNullOrEmpty()) e.BirthCertificateFifth = p.BirthCertificateFifth;
		if (p.VisualAuthentication.IsNotNullOrEmpty()) e.VisualAuthentication = p.VisualAuthentication;
		if (p.ESignature.IsNotNullOrEmpty()) e.ESignature = p.ESignature;

		db.Set<UserExtraEntity>().Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}
}