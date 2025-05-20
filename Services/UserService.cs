namespace SinaMN75U.Services;

public interface IUserService {
	public Task<UResponse<UserResponse?>> Create(UserCreateParams p, CancellationToken ct);
	public Task<UResponse> BulkCreate(UserBulkCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<UserResponse>?>> Read(UserReadParams p, CancellationToken ct);
	public Task<UResponse<UserResponse?>> ReadById(IdParams p, CancellationToken ct);
	public Task<UResponse<UserResponse?>> Update(UserUpdateParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class UserService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	ICategoryService categoryService
) : IUserService {
	public async Task<UResponse<UserResponse?>> Create(UserCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UserResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
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
			ParentId = p.ParentId,
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

		return new UResponse<UserResponse?>(e.MapToResponse(), Usc.Created);
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
			ParentId = userParam.ParentId,
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

	public async Task<UResponse<IEnumerable<UserResponse>?>> Read(UserReadParams p, CancellationToken ct) {
		IQueryable<UserEntity> q = db.Set<UserEntity>();

		if (p.FromCreatedAt.HasValue) q = q.Where(u => u.CreatedAt >= p.FromCreatedAt.Value);
		if (p.ToCreatedAt.HasValue) q = q.Where(u => u.CreatedAt <= p.ToCreatedAt.Value);

		if (p.UserName.IsNotNull()) q = q.Where(u => u.UserName.Contains(p.UserName!));
		if (p.PhoneNumber.IsNotNull()) q = q.Where(u => u.PhoneNumber == p.PhoneNumber);
		if (p.Email.IsNotNull()) q = q.Where(u => u.Email == p.Email);
		if (p.Bio.IsNotNull()) q = q.Where(u => u.Bio == p.Bio);
		if (p.ParentId.IsNotNullOrEmpty()) q = q.Where(u => u.ParentId == p.ParentId);
		if (p.StartBirthDate.HasValue) q = q.Where(u => u.Birthdate >= p.StartBirthDate.Value);
		if (p.EndBirthDate.HasValue) q = q.Where(u => u.Birthdate <= p.EndBirthDate.Value);
		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(u => u.Tags.Any(tag => p.Tags!.Contains(tag)));
		if (p.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories!.Any(y => p.Categories.Contains(y.Id)));
		if (p.OrderByLastNameDesc) q = q.OrderByDescending(x => x.LastName);

		if (p.OrderByCreatedAt) q = q.OrderBy(x => x.CreatedAt);
		if (p.OrderByCreatedAtDesc) q = q.OrderByDescending(x => x.CreatedAt);
		if (p.OrderByUpdatedAt) q = q.OrderBy(x => x.UpdatedAt);
		if (p.OrderByUpdatedAtDesc) q = q.OrderByDescending(x => x.UpdatedAt);
		if (p.OrderByLastName) q = q.OrderBy(x => x.LastName);
		if (p.OrderByLastNameDesc) q = q.OrderByDescending(x => x.LastName);

		if (p.ShowCategories) q = q.Include(x => x.Categories);
		if (p.ShowMedia) q = q.Include(x => x.Media);
		if (p.ShowChildren)
			q = q.Include(x => x.Children)!
				.ThenInclude(x => x.Children)!
				.ThenInclude(x => x.Children)!
				.ThenInclude(x => x.Children)!
				.ThenInclude(x => x.Children)!
				.ThenInclude(x => x.Children)!
				.ThenInclude(x => x.Children)!
				.ThenInclude(x => x.Children)!
				.ThenInclude(x => x.Children)!
				.ThenInclude(x => x.Children);
		return await q.ToResponse(p.ShowMedia, p.ShowCategories).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<UserResponse?>> Update(UserUpdateParams p, CancellationToken ct) {
		UserEntity? e = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);

		if (e == null) return new UResponse<UserResponse?>(null, Usc.NotFound);

		e.UpdatedAt = DateTime.UtcNow;

		if (p.UserName.IsNotNullOrEmpty()) e.UserName = p.UserName;
		if (p.PhoneNumber.IsNotNullOrEmpty()) e.PhoneNumber = p.PhoneNumber;
		if (p.Email.IsNotNullOrEmpty()) e.Email = p.Email;
		if (p.Bio.IsNotNullOrEmpty()) e.Bio = p.Bio;
		if (p.Birthdate != null) e.Birthdate = p.Birthdate;
		if (p.Password.IsNotNullOrEmpty()) e.Password = PasswordHasher.Hash(p.Password);
		if (p.FirstName.IsNotNullOrEmpty()) e.FirstName = p.FirstName;
		if (p.LastName.IsNotNullOrEmpty()) e.LastName = p.LastName;
		if (p.City.IsNotNullOrEmpty()) e.City = p.City;
		if (p.Country.IsNotNullOrEmpty()) e.Country = p.Country;
		if (p.State.IsNotNullOrEmpty()) e.State = p.State;
		if (p.ParentId.IsNotNullOrEmpty()) e.ParentId = p.ParentId;

		// Json
		if (p.FcmToken.IsNotNullOrEmpty()) e.JsonData.FcmToken = p.FcmToken;
		if (p.FatherName.IsNotNullOrEmpty()) e.JsonData.FatherName = p.FatherName;
		if (p.Weight.IsNotNullOrEmpty()) e.JsonData.Weight = p.Weight;
		if (p.Height.IsNotNullOrEmpty()) e.JsonData.Height = p.Height;
		if (p.Address.IsNotNullOrEmpty()) e.JsonData.Address = p.Address;

		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => p.RemoveTags.Contains(x));

		if (p.AddHealth1.IsNotNullOrEmpty()) e.JsonData.Health1.AddRangeIfNotExist(p.AddHealth1);
		if (p.RemoveHealth1.IsNotNullOrEmpty()) e.JsonData.Health1.RemoveAll(x => p.RemoveHealth1.Contains(x));

		if (p.Sickness.IsNotNullOrEmpty()) e.JsonData.Sickness = p.Sickness;
		if (p.DrugAllergies.IsNotNullOrEmpty()) e.JsonData.DrugAllergies = p.DrugAllergies;
		if (p.FoodAllergies.IsNotNullOrEmpty()) e.JsonData.FoodAllergies = p.FoodAllergies;

		if (p.Categories.IsNotNullOrEmpty()) {
			List<CategoryEntity>? list = await categoryService.ReadEntity(new CategoryReadParams { Ids = p.Categories }, ct);
			e.Categories.AddRangeIfNotExist(list);
		}

		db.Set<UserEntity>().Update(e);
		await db.SaveChangesAsync(ct);

		return new UResponse<UserResponse?>(e.MapToResponse());
	}

	public async Task<UResponse<UserResponse?>> ReadById(IdParams p, CancellationToken ct) {
		UserEntity? user = await db.Set<UserEntity>().FindAsync(p.Id, ct);

		return user == null ? new UResponse<UserResponse?>(null, Usc.NotFound, ls.Get("UserNotFound")) : new UResponse<UserResponse?>(user.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		int count = await db.Set<UserEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return count == 0 ? new UResponse(Usc.NotFound, ls.Get("UserNotFound")) : new UResponse(Usc.Deleted, ls.Get("UserDeleted"));
	}
}