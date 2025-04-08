namespace SinaMN75U.Services;

public interface IUserService {
	public Task<UResponse<UserResponse?>> Create(UserCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<UserResponse>>> Read(UserReadParams p, CancellationToken ct);
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
			JsonDetail = new UserJsonDetail {
				FcmToken = p.FcmToken
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

	public async Task<UResponse<IEnumerable<UserResponse>>> Read(UserReadParams p, CancellationToken ct) {
		IQueryable<UserEntity> q = db.Set<UserEntity>();

		if (p.UserName.IsNotNull()) q = q.Where(u => u.UserName.Contains(p.UserName!));
		if (p.PhoneNumber.IsNotNull()) q = q.Where(u => u.PhoneNumber == p.PhoneNumber);
		if (p.Email.IsNotNull()) q = q.Where(u => u.Email == p.Email);
		if (p.Bio.IsNotNull()) q = q.Where(u => u.Bio == p.Bio);
		if (p.StartBirthDate.HasValue) q = q.Where(u => u.Birthdate >= p.StartBirthDate.Value);
		if (p.EndBirthDate.HasValue) q = q.Where(u => u.Birthdate <= p.EndBirthDate.Value);
		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(u => u.Tags.Any(tag => p.Tags!.Contains(tag)));

		if (p.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories!.Any(y => p.Categories.Contains(y.Id)));
		
		int totalCount = await q.CountAsync(ct);
		q = q.Skip((p.PageNumber - 1) * p.PageSize).Take(p.PageSize);

		List<UserEntity> list = await q.ToListAsync(ct);

		return new UResponse<IEnumerable<UserResponse>>(list.Select(x => x.MapToResponse())) {
			TotalCount = totalCount,
			PageCount = totalCount % p.PageSize == 0 ? totalCount / p.PageSize : totalCount / p.PageSize + 1,
			PageSize = p.PageSize
		};
	}

	public async Task<UResponse<UserResponse?>> Update(UserUpdateParams p, CancellationToken ct) {
		UserEntity? e = await db.Set<UserEntity>().FindAsync(p.Id, ct);

		if (e == null) return new UResponse<UserResponse?>(null, USC.NotFound);

		e.UpdatedAt = DateTime.UtcNow;

		if (p.UserName != null) e.UserName = p.UserName;
		if (p.PhoneNumber != null) e.PhoneNumber = p.PhoneNumber;
		if (p.Email != null) e.Email = p.Email;
		if (p.Bio != null) e.Bio = p.Bio;
		if (p.Birthdate != null) e.Birthdate = p.Birthdate;
		if (p.Password != null) e.Password = p.Password;
		if (p.FirstName != null) e.FirstName = p.FirstName;
		if (p.LastName != null) e.LastName = p.LastName;
		if (p.City != null) e.City = p.City;
		if (p.Country != null) e.Country = p.Country;
		if (p.State != null) e.State = p.State;
		if (p.FcmToken != null) e.JsonDetail.FcmToken = p.FcmToken;

		if (p.AddTags != null) e.Tags.AddRange(p.AddTags);
		if (p.RemoveTags != null) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));

		if (p.Categories.IsNotNullOrEmpty()) {
			List<CategoryEntity> list = [];
			foreach (Guid item in p.Categories!) {
				CategoryEntity? c = await db.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == item);
				if (c != null) list.Add(c);
			}

			e.Categories = list;
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
		UserEntity? e = await db.Set<UserEntity>()
			.Include(x => x.Categories)
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);

		if (e == null)
			return new UResponse(USC.NotFound, ls.Get("UserNotFound"));

		if (e.Categories.IsNotNullOrEmpty())
			await categoryService.DeleteRange(e.Categories!.Select(x => x.Id), ct);

		db.Set<UserEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse(USC.Deleted, ls.Get("UserDeleted"));
	}
}