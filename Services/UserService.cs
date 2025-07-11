namespace SinaMN75U.Services;

public interface IUserService {
	public Task<UResponse<UserEntity?>> Create(UserCreateParams p, CancellationToken ct);
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
	public async Task<UResponse<UserEntity?>> Create(UserCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UserEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		UserEntity e = new() {
			Id = Guid.CreateVersion7(),
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
				Sickness = p.Sickness ?? [],
				Weight = p.Weight,
				Height = p.Height,
				Address = p.Address,
				FatherName = p.FatherName,
				FoodAllergies = p.FoodAllergies ?? [],
				DrugAllergies = p.DrugAllergies
			},
			Tags = p.Tags,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		if (p.Categories.IsNotNullOrEmpty()) {
			List<CategoryEntity> list = [];
			foreach (Guid item in p.Categories!) {
				CategoryEntity? c = await db.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == item);
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

		q = q.WhereIf(p.UserName.IsNotNull(), u => u.UserName.Contains(p.UserName!));
		q = q.WhereIf(p.PhoneNumber.IsNotNull(), u => u.PhoneNumber == p.PhoneNumber);
		q = q.WhereIf(p.Email.IsNotNull(), u => u.Email == p.Email);
		q = q.WhereIf(p.Bio.IsNotNull(), u => u.Bio == p.Bio);
		q = q.WhereIf(p.StartBirthDate.HasValue, u => u.Birthdate >= p.StartBirthDate);
		q = q.WhereIf(p.EndBirthDate.HasValue, u => u.Birthdate <= p.EndBirthDate);
		q = q.WhereIf(p.Tags.IsNotNullOrEmpty(), u => u.Tags.Any(tag => p.Tags!.Contains(tag)));
		q = q.WhereIf(p.Categories.IsNotNullOrEmpty(), x => x.Categories.Any(y => p.Categories!.Contains(y.Id)));

		q = q.IncludeIf(p.ShowCategories, x => x.Categories);
		q = q.IncludeIf(p.ShowMedia, x => x.Media);

		q = q.OrderByIf(p.OrderByCreatedAt, x => x.CreatedAt);
		q = q.OrderByDescendingIf(p.OrderByCreatedAtDesc, x => x.CreatedAt);
		q = q.OrderByIf(p.OrderByLastName, x => x.LastName);
		q = q.OrderByDescendingIf(p.OrderByLastNameDesc, x => x.LastName);

		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<UserEntity?>> Update(UserUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UserEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserEntity? e = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<UserEntity?>(null, Usc.NotFound);

		e.UpdatedAt = DateTime.UtcNow;

		if (p.Categories.IsNotNullOrEmpty()) {
			List<CategoryEntity>? list = await categoryService.ReadEntity(new CategoryReadParams { Ids = p.Categories }, ct);
			e.Categories.AddRangeIfNotExist(list);
		}

		db.Set<UserEntity>().Update(e);
		await db.SaveChangesAsync(ct);

		return new UResponse<UserEntity?>(e);
	}

	public async Task<UResponse<UserEntity?>> ReadById(IdParams p, CancellationToken ct) {
		UserEntity? user = await db.Set<UserEntity>().FindAsync(p.Id, ct);

		return user == null ? new UResponse<UserEntity?>(null, Usc.NotFound, ls.Get("UserNotFound")) : new UResponse<UserEntity?>(user);
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		int count = await db.Set<UserEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return count == 0 ? new UResponse(Usc.NotFound, ls.Get("UserNotFound")) : new UResponse(Usc.Deleted, ls.Get("UserDeleted"));
	}
}