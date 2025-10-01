namespace SinaMN75U.Services;

public interface IUserService {
	public Task<UResponse<UserEntity?>> Create(UserCreateParams p, bool auth, CancellationToken ct);
	public Task<UResponse> BulkCreate(UserBulkCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<UserEntity>?>> Read(UserReadParams p, CancellationToken ct);
	public Task<UResponse<UserEntity?>> ReadById(IdParams p, CancellationToken ct);
	public Task<UResponse<UserEntity?>> Update(UserUpdateParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class UserService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	ICategoryService categoryService
) : IUserService {
	public async Task<UResponse<UserEntity?>> Create(UserCreateParams p, bool auth, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null && auth) return new UResponse<UserEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		UserEntity e = new() {
			UserName = p.UserName,
			Password = PasswordHasher.Hash(p.Password),
			RefreshToken = "",
			PhoneNumber = p.PhoneNumber,
			Email = p.Email,
			FirstName = p.FirstName,
			LastName = p.LastName,
			Bio = p.Bio,
			Country = p.Country,
			State = p.State,
			City = p.City,
			Birthdate = p.Birthdate,
			JsonData = new UserJson {
				FcmToken = p.FcmToken,
				Health1 = p.Health1 ?? [],
				Health2 = p.Health2 ?? [],
				Sickness = p.Sickness ?? [],
				Weight = p.Weight,
				Height = p.Height,
				Address = p.Address,
				FatherName = p.FatherName,
				FoodAllergies = p.FoodAllergies ?? [],
				DrugAllergies = p.DrugAllergies
			},
			Tags = p.Tags
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

		return new UResponse<UserEntity?>(e, Usc.Created);
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
			Password = PasswordHasher.Hash(userParam.Password),
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
			JsonData = new UserJson {
				FcmToken = userParam.FcmToken,
				Health1 = userParam.Health1 ?? [],
				Sickness = userParam.Sickness ?? [],
				Weight = userParam.Weight,
				Height = userParam.Height,
				Address = userParam.Address,
				FatherName = userParam.FatherName,
				FoodAllergies = userParam.FoodAllergies ?? [],
				DrugAllergies = userParam.DrugAllergies
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

	public async Task<UResponse<IEnumerable<UserEntity>?>> Read(UserReadParams p, CancellationToken ct) {
		IQueryable<UserEntity> q = db.Set<UserEntity>();

		if (p.UserName.IsNotNull()) q = q.Where(u => u.UserName.Contains(p.UserName!));
		if (p.PhoneNumber.IsNotNull()) q = q.Where(u => u.PhoneNumber == p.PhoneNumber);
		if (p.Email.IsNotNull()) q = q.Where(u => u.Email == p.Email);
		if (p.Bio.IsNotNull()) q = q.Where(u => u.Bio == p.Bio);
		if (p.StartBirthDate.HasValue) q = q.Where(u => u.Birthdate >= p.StartBirthDate);
		if (p.EndBirthDate.HasValue) q = q.Where(u => u.Birthdate <= p.EndBirthDate);
		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(u => u.Tags.Any(tag => p.Tags!.Contains(tag)));
		if (p.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories.Any(y => p.Categories!.Contains(y.Id)));

		if (p.ShowCategories) q = q.Include(x => x.Categories);
		if (p.ShowMedia) q = q.Include(x => x.Media);

		if (p.OrderByCreatedAt) q = q.OrderBy(x => x.CreatedAt);
		if (p.OrderByCreatedAtDesc) q = q.OrderByDescending(x => x.CreatedAt);
		if (p.OrderByLastName) q = q.OrderBy(x => x.LastName);
		if (p.OrderByLastNameDesc) q = q.OrderByDescending(x => x.LastName);

		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<UserEntity?>> Update(UserUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UserEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserEntity? e = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<UserEntity?>(null, Usc.NotFound);

		e.UpdatedAt = DateTime.UtcNow;

		if (p.AddHealth1.IsNotNullOrEmpty()) e.JsonData.Health1.AddRangeIfNotExist(p.AddHealth1);
		if (p.RemoveHealth1.IsNotNullOrEmpty()) e.JsonData.Health1.RemoveAll(x => p.RemoveHealth1.Contains(x));
		if (p.AddHealth2.IsNotNullOrEmpty()) e.JsonData.Health1.AddRangeIfNotExist(p.AddHealth2);
		if (p.RemoveHealth2.IsNotNullOrEmpty()) e.JsonData.Health1.RemoveAll(x => p.RemoveHealth2.Contains(x));

		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => p.RemoveTags.Contains(x));
		if (p.Tags.IsNotNullOrEmpty()) e.Tags = p.Tags;

		if (p.Categories.IsNotNullOrEmpty()) {
			List<CategoryEntity>? list = await categoryService.ReadEntity(new CategoryReadParams { Ids = p.Categories }, ct);
			e.Categories.AddRangeIfNotExist(list);
		}

		db.Set<UserEntity>().Update(e);
		await db.SaveChangesAsync(ct);

		return new UResponse<UserEntity?>(e);
	}

	public async Task<UResponse<UserEntity?>> ReadById(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);

		UserEntity? e = await db.Set<UserEntity>()
			.Include(x => x.Media)
			.Include(x => x.Categories).ThenInclude(x => x.Media)
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<UserEntity?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		try {
			VisitCount? visitCount = e.JsonData.VisitCounts.FirstOrDefault(v => v.UserId == (userData?.Id ?? Guid.Empty));
			if (visitCount != null) visitCount.Count++;
			else e.JsonData.VisitCounts.Add(new VisitCount { UserId = userData?.Id ?? Guid.Empty, Count = 1 });
		}
		catch (Exception) {
			// ignored
		}

		return new UResponse<UserEntity?>(e);
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		int count = await db.Set<UserEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return count == 0 ? new UResponse(Usc.NotFound, ls.Get("UserNotFound")) : new UResponse(Usc.Deleted, ls.Get("UserDeleted"));
	}
}