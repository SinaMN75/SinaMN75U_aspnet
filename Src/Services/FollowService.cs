namespace SinaMN75U.Services;

public interface IFollowService {
	Task<UResponse> Follow(FollowParams p, CancellationToken ct);
	Task<UResponse> Unfollow(FollowParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<UserResponse>>> ReadFollowers(IdParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<UserResponse>>> ReadFollowedUsers(IdParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ProductEntity>>> ReadFollowedProducts(IdParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<CategoryEntity>>> ReadFollowedCategories(IdParams p, CancellationToken ct);
	Task<UResponse<FollowerFollowingCountResponse>> ReadFollowerFollowingCount(IdParams p, CancellationToken ct);
	Task<UResponse<bool?>> IsFollowingUser(FollowParams p, CancellationToken ct);
	Task<UResponse<bool?>> IsFollowingProduct(FollowParams p, CancellationToken ct);
	Task<UResponse<bool?>> IsFollowingCategory(FollowParams p, CancellationToken ct);
}

public class FollowService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IFollowService {
	public async Task<UResponse> Follow(FollowParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		p.CreatorId ??= userData.Id;

		if (p.UserId != null) {
			if (p.CreatorId == p.UserId) return new UResponse(Usc.BadRequest, ls.Get("cannotFollowYourself"));
			bool alreadyFollowing = await db.Set<FollowEntity>()
				.AnyAsync(x => x.CreatorId == p.CreatorId && x.UserId == p.UserId, ct);

			if (alreadyFollowing)
				return new UResponse(Usc.Conflict, ls.Get("alreadyFollowingUser"));
		}

		if (p.ProductId != null) {
			bool alreadyFollowing = await db.Set<FollowEntity>()
				.AnyAsync(x => x.CreatorId == p.CreatorId && x.ProductId == p.ProductId, ct);
			if (alreadyFollowing)
				return new UResponse(Usc.Conflict, ls.Get("alreadyBookmarked"));
		}

		if (p.CategoryId != null) {
			bool alreadyFollowing = await db.Set<FollowEntity>()
				.AnyAsync(x => x.CreatorId == p.CreatorId && x.CategoryId == p.CategoryId, ct);
			if (alreadyFollowing)
				return new UResponse(Usc.Conflict, ls.Get("alreadyBookmarked"));
		}

		FollowEntity userFollower = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			CreatorId = p.CreatorId.Value,
			UserId = p.UserId,
			ProductId = p.ProductId,
			CategoryId = p.CategoryId,
			JsonData = new FollowJson(),
			Tags = [TagFollow.User]
		};

		await db.Set<FollowEntity>().AddAsync(userFollower, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse(Usc.Success, ls.Get("youAreNowFollowing"));
	}

	public async Task<UResponse> Unfollow(FollowParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		p.CreatorId ??= userData.Id;

		FollowEntity? userFollower = null;

		if (p.UserId != null)
			userFollower = await db.Set<FollowEntity>()
				.FirstOrDefaultAsync(x => x.CreatorId == p.CreatorId && x.UserId == p.UserId, ct);

		if (p.ProductId != null)
			userFollower = await db.Set<FollowEntity>()
				.FirstOrDefaultAsync(x => x.CreatorId == p.CreatorId && x.ProductId == p.ProductId, ct);

		if (p.CategoryId != null)
			userFollower = await db.Set<FollowEntity>()
				.FirstOrDefaultAsync(x => x.CreatorId == p.CreatorId && x.CategoryId == p.CategoryId, ct);

		if (userFollower == null)
			return new UResponse(Usc.NotFound, ls.Get("errorFindingTheRelation"));

		db.Set<FollowEntity>().Remove(userFollower);
		await db.SaveChangesAsync(ct);

		return new UResponse(Usc.Success, ls.Get("youAreNoLongerFollowing"));
	}

	// Projected to UserResponse: returning UserEntity serialized every follower's password hash and refresh token.
	public async Task<UResponse<IEnumerable<UserResponse>>> ReadFollowers(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<UserResponse>>([], Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<UserResponse>>([], Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		List<UserResponse> followers = await db.Set<FollowEntity>().AsNoTracking()
			.Where(x => x.UserId == p.Id)
			.Select(x => x.Creator)
			.Select(Projections.UserSelector(new UserSelectorArgs()))
			.ToListAsync(ct);

		return new UResponse<IEnumerable<UserResponse>>(followers);
	}

	public async Task<UResponse<IEnumerable<UserResponse>>> ReadFollowedUsers(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<UserResponse>>([], Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<UserResponse>>([], Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		List<UserResponse> following = await db.Set<FollowEntity>().AsNoTracking()
			.Where(x => x.CreatorId == p.Id && x.UserId != null)
			.Select(x => x.User!)
			.Select(Projections.UserSelector(new UserSelectorArgs()))
			.ToListAsync(ct);

		return new UResponse<IEnumerable<UserResponse>>(following);
	}

	public async Task<UResponse<IEnumerable<ProductEntity>>> ReadFollowedProducts(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<ProductEntity>>([], Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<ProductEntity>>([], Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		List<ProductEntity> following = (await db.Set<FollowEntity>()
			.Where(x => x.CreatorId == p.Id && x.ProductId != null)
			.Select(x => x.Product)
			.ToListAsync(ct))!;

		return new UResponse<IEnumerable<ProductEntity>>(following);
	}

	public async Task<UResponse<IEnumerable<CategoryEntity>>> ReadFollowedCategories(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<CategoryEntity>>([], Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<IEnumerable<CategoryEntity>>([], Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		List<CategoryEntity> following = (await db.Set<FollowEntity>()
			.Where(x => x.CreatorId == p.Id && x.CategoryId != null)
			.Select(x => x.Category)
			.ToListAsync(ct))!;

		return new UResponse<IEnumerable<CategoryEntity>>(following);
	}

	// One round trip instead of four; FollowedUsers now counts only user follows (it used to include product/category follows).
	public async Task<UResponse<FollowerFollowingCountResponse>> ReadFollowerFollowingCount(IdParams p, CancellationToken ct) {
		FollowerFollowingCountResponse? counts = await db.Set<FollowEntity>()
			.Where(x => x.UserId == p.Id || x.CreatorId == p.Id)
			.GroupBy(_ => 1)
			.Select(g => new FollowerFollowingCountResponse {
				Followers = g.Count(x => x.UserId == p.Id),
				FollowedUsers = g.Count(x => x.CreatorId == p.Id && x.UserId != null),
				FollowedProducts = g.Count(x => x.CreatorId == p.Id && x.ProductId != null),
				FollowedCategories = g.Count(x => x.CreatorId == p.Id && x.CategoryId != null)
			})
			.FirstOrDefaultAsync(ct);
		return new UResponse<FollowerFollowingCountResponse>(counts ?? new FollowerFollowingCountResponse());
	}

	public async Task<UResponse<bool?>> IsFollowingUser(FollowParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<bool?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<bool?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		p.CreatorId ??= userData.Id;
		bool isFollowing = await db.Set<FollowEntity>().AnyAsync(x => x.CreatorId == p.CreatorId && x.UserId == p.UserId, ct);
		return new UResponse<bool?>(isFollowing);
	}

	public async Task<UResponse<bool?>> IsFollowingProduct(FollowParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<bool?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<bool?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		p.CreatorId ??= userData.Id;
		bool isFollowing = await db.Set<FollowEntity>().AnyAsync(x => x.CreatorId == p.CreatorId && x.ProductId == p.ProductId, ct);
		return new UResponse<bool?>(isFollowing);
	}

	public async Task<UResponse<bool?>> IsFollowingCategory(FollowParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<bool?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<bool?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		bool isFollowing = await db.Set<FollowEntity>().AnyAsync(x => x.CreatorId == (p.CreatorId ?? userData.Id) && x.CategoryId == p.CategoryId, ct);
		return new UResponse<bool?>(isFollowing);
	}
}