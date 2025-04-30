namespace SinaMN75U.Services;

public interface IUserService {
	public Task<UResponse<UserResponse?>> Create(UserCreateParams p, CancellationToken ct);
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
		if (userData == null) return new UResponse<UserResponse?>(null, USC.UnAuthorized, ls.Get("AuthorizationRequired"));
		UserEntity e = new() {
			Id = Guid.CreateVersion7(),
			UserName = p.UserName,
			Password = p.Password,
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

		return new UResponse<UserResponse?>(e.MapToResponse(), USC.Created);
	}

	public async Task<UResponse<IEnumerable<UserResponse>?>> Read(UserReadParams p, CancellationToken ct) {
		IQueryable<UserEntity> q = db.Set<UserEntity>();

		if (p.UserName.IsNotNull()) q = q.Where(u => u.UserName.Contains(p.UserName!));
		if (p.PhoneNumber.IsNotNull()) q = q.Where(u => u.PhoneNumber == p.PhoneNumber);
		if (p.Email.IsNotNull()) q = q.Where(u => u.Email == p.Email);
		if (p.Bio.IsNotNull()) q = q.Where(u => u.Bio == p.Bio);
		if (p.ParentId.IsNotNullOrEmpty()) q = q.Where(u => u.ParentId == p.ParentId);
		if (p.StartBirthDate.HasValue) q = q.Where(u => u.Birthdate >= p.StartBirthDate.Value);
		if (p.EndBirthDate.HasValue) q = q.Where(u => u.Birthdate <= p.EndBirthDate.Value);
		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(u => u.Tags.Any(tag => p.Tags!.Contains(tag)));
		if (p.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories!.Any(y => p.Categories.Contains(y.Id)));

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

		if (e == null) return new UResponse<UserResponse?>(null, USC.NotFound);

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

		if (p.AddSickness.IsNotNullOrEmpty()) e.JsonData.Sickness.AddRangeIfNotExist(p.AddSickness);
		if (p.RemoveSickness.IsNotNullOrEmpty()) e.JsonData.Sickness.RemoveAll(x => p.RemoveSickness.Contains(x));

		if (p.AddDrugAllergies.IsNotNullOrEmpty()) e.JsonData.DrugAllergies.AddRangeIfNotExist(p.AddDrugAllergies);
		if (p.RemoveDrugAllergies.IsNotNullOrEmpty()) e.JsonData.DrugAllergies.RemoveAll(x => p.RemoveDrugAllergies.Contains(x));

		if (p.AddFoodAllergies.IsNotNullOrEmpty()) e.JsonData.FoodAllergies.AddRangeIfNotExist(p.AddFoodAllergies);
		if (p.RemoveFoodAllergies.IsNotNullOrEmpty()) e.JsonData.FoodAllergies.RemoveAll(x => p.RemoveFoodAllergies.Contains(x));

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

		return user == null ? new UResponse<UserResponse?>(null, USC.NotFound, ls.Get("UserNotFound")) : new UResponse<UserResponse?>(user.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		int count = await db.Set<UserEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);
		return count == 0 ? new UResponse(USC.NotFound, ls.Get("UserNotFound")) : new UResponse(USC.Deleted, ls.Get("UserDeleted"));
	}
}