namespace SinaMN75U.Services;

public interface IUserService {
	public Task<UResponse<Guid?>> Create(UserCreateParams p, CancellationToken ct);
	public Task<UResponse> BulkCreate(UserBulkCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<UserResponse>?>> Read(UserReadParams p, CancellationToken ct);
	public Task<UResponse<UserResponse?>> ReadById(IdParams p, CancellationToken ct);
	public Task<UResponse<UserResponse?>> ReadById(IdParams<UserSelectorArgs> p, CancellationToken ct);
	public Task<UResponse> Update(UserUpdateParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdParams p, CancellationToken ct);

	public Task<UResponse> CreateExtra(Guid userId, CancellationToken ct);
	public Task<UResponse<UserExtraResponse?>> ReadExtraById(IdParams p, CancellationToken ct);
	public Task<UResponse> UpdateExtra(UserExtraUpdateParams p, CancellationToken ct);
}

public class UserService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IWalletService walletService
) : IUserService {
	public async Task<UResponse<Guid?>> Create(UserCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		Guid userId = p.Id ?? Guid.CreateVersion7();
		UserEntity e = new() {
			Id = userId,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			JsonData = new UserJson {
				FcmToken = p.FcmToken,
				Address = p.Address,
				FatherName = p.FatherName,
				Weight = p.Weight,
				Height = p.Height,
				VisitCounts = []
			},
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
			Country = p.Country,
			State = p.State,
			City = p.City,
			Birthdate = p.Birthdate
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
		await CreateExtra(e.Id, ct);
		await walletService.Create(e.Id, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, Usc.Created);
	}

	public async Task<UResponse> BulkCreate(UserBulkCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		if (p.Users.Count == 0) return new UResponse(Usc.BadRequest, ls.Get("AtLeastOneUserRequired"));

		List<UserEntity> entities = [];
		List<Guid> categoryIds = p.Users.SelectMany(u => u.Categories ?? []).Distinct().ToList();
		List<CategoryEntity> categories = await db.Set<CategoryEntity>()
			.Where(c => categoryIds.Contains(c.Id))
			.ToListAsync(ct);

		entities.AddRange(p.Users.Select(userParam => new UserEntity {
			Id = Guid.CreateVersion7(),
			UserName = userParam.UserName,
			Password = UPasswordHasher.Hash(userParam.Password),
			RefreshToken = "",
			PhoneNumber = userParam.PhoneNumber,
			Email = userParam.Email,
			FirstName = userParam.FirstName,
			LastName = userParam.LastName,
			Bio = userParam.Bio,
			Country = userParam.Country,
			State = userParam.State,
			City = userParam.City,
			Birthdate = userParam.Birthdate,
			NationalCode = userParam.NationalCode,
			JsonData = new UserJson {
				FcmToken = userParam.FcmToken,
				Weight = userParam.Weight,
				Height = userParam.Height,
				Address = userParam.Address,
				FatherName = userParam.FatherName
			},
			Tags = userParam.Tags,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			Categories = categories.Where(c => userParam.Categories?.Contains(c.Id) ?? false).ToList()
		}));

		await db.Set<UserEntity>().AddRangeAsync(entities, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse(Usc.Created);
	}

	public async Task<UResponse<IEnumerable<UserResponse>?>> Read(UserReadParams p, CancellationToken ct) {
		IQueryable<UserEntity> q = db.Set<UserEntity>();

		if (p.UserName.IsNotNullOrEmpty()) q = q.Where(u => u.UserName.Contains(p.UserName!));
		if (p.FirstName.IsNotNullOrEmpty()) q = q.Where(u => (u.FirstName ?? "").Contains(p.FirstName!));
		if (p.LastName.IsNotNullOrEmpty()) q = q.Where(u => (u.LastName ?? "").Contains(p.UserName!));
		if (p.PhoneNumber.IsNotNullOrEmpty()) q = q.Where(u => u.PhoneNumber == p.PhoneNumber);
		if (p.Email.IsNotNullOrEmpty()) q = q.Where(u => u.Email == p.Email);
		if (p.Bio.IsNotNullOrEmpty()) q = q.Where(u => u.Bio == p.Bio);
		if (p.StartBirthDate.HasValue) q = q.Where(u => u.Birthdate >= p.StartBirthDate);
		if (p.EndBirthDate.HasValue) q = q.Where(u => u.Birthdate <= p.EndBirthDate);
		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(u => u.Tags.Any(tag => p.Tags!.Contains(tag)));
		if (p.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories.Any(y => p.Categories!.Contains(y.Id)));

		if (p.OrderByCreatedAt) q = q.OrderBy(x => x.CreatedAt);
		if (p.OrderByCreatedAtDesc) q = q.OrderByDescending(x => x.CreatedAt);
		if (p.OrderByUpdatedAt) q = q.OrderBy(x => x.UpdatedAt);
		if (p.OrderByUpdatedAtDesc) q = q.OrderByDescending(x => x.UpdatedAt);
		if (p.OrderByLastName) q = q.OrderBy(x => x.LastName);
		if (p.OrderByLastNameDesc) q = q.OrderByDescending(x => x.LastName);

		IQueryable<UserResponse> projected = q.Select(Projections.UserSelector(p.SelectorArgs));

		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<UserResponse?>> ReadById(IdParams<UserSelectorArgs> p, CancellationToken ct) {
		UserResponse? e = await db.Set<UserEntity>()
			.Select(Projections.UserSelector(p.SelectorArgs))
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);

		return e == null ? new UResponse<UserResponse?>(null, Usc.NotFound, ls.Get("UserNotFound")) : new UResponse<UserResponse?>(e);
	}

	public async Task<UResponse> Update(UserUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserEntity? e = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound);

		if (p.Categories.IsNotNullOrEmpty()) {
			List<CategoryEntity> list = await db.Set<CategoryEntity>().AsTracking().Where(x => p.Categories.Contains(x.Id)).OrderByDescending(x => x.Id).ToListAsync(ct);
			e.Categories.AddRangeIfNotExist(list);
		}

		e.UpdatedAt = DateTime.UtcNow;
		db.Set<UserEntity>().Update(e);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse<UserResponse?>> ReadById(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);

		UserResponse? e = await db.Set<UserEntity>().Select(Projections.UserSelector(new UserSelectorArgs {
				Media = new MediaSelectorArgs()
			}))
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<UserResponse?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		try {
			VisitCount? visitCount = e.JsonData.VisitCounts.FirstOrDefault(v => v.UserId == (userData?.Id ?? Guid.Empty));
			if (visitCount != null) visitCount.Count++;
			else e.JsonData.VisitCounts.Add(new VisitCount { UserId = userData?.Id ?? Guid.Empty, Count = 1 });
		}
		catch (Exception) {
			// ignored
		}

		return new UResponse<UserResponse?>(e);
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		int count = await db.Set<UserEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return count == 0 ? new UResponse(Usc.NotFound, ls.Get("UserNotFound")) : new UResponse(Usc.Deleted, ls.Get("UserDeleted"));
	}

	public async Task<UResponse> CreateExtra(Guid userId, CancellationToken ct) {
		await db.Set<UserExtraEntity>().AddAsync(new UserExtraEntity {
			Id = userId,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			UserId = userId,
			JsonData = new GeneralJsonData(),
			Tags = []
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<UserExtraResponse?>> ReadExtraById(IdParams p, CancellationToken ct) {
		UserExtraEntity? e = await db.Set<UserExtraEntity>().FirstOrDefaultAsync(x => x.UserId == p.Id, ct);
		if (e == null) return new UResponse<UserExtraResponse?>(null, Usc.NotFound);

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
		UserExtraEntity? e = await db.Set<UserExtraEntity>().FirstOrDefaultAsync(x => x.UserId == p.Id, ct);
		if (e == null) return new UResponse<UserExtraResponse?>(null, Usc.NotFound);

		if (p.BirthCertificateFifth.IsNotNullOrEmpty()) e.NationalCardFront = p.NationalCardFront;
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