namespace SinaMN75U.Services;

public interface IFollowService {
	Task<UResponse> Follow(FollowParams p, CancellationToken ct);
	Task<UResponse> Unfollow(FollowParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<UserEntity>>> ReadFollowers(IdParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<UserEntity>>> ReadFollowedUsers(IdParams p, CancellationToken ct);
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
	ITokenService ts,
	ILocalStorageService cache
) : IFollowService {
	public async Task<UResponse> Follow(FollowParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		p.CreatorId ??= userData.Id;

		if (p.UserId != null) {
			if (p.CreatorId == p.UserId) return new UResponse(Usc.BadRequest, ls.Get("CannotFollowYourself"));
			bool alreadyFollowing = await db.Set<FollowEntity>()
				.AnyAsync(x => x.CreatorId == p.CreatorId && x.UserId == p.UserId, ct);

			if (alreadyFollowing)
				return new UResponse(Usc.Conflict, ls.Get("AlreadyFollowingUser"));
		}

		if (p.ProductId != null) {
			bool alreadyFollowing = await db.Set<FollowEntity>()
				.AnyAsync(x => x.CreatorId == p.CreatorId && x.ProductId == p.ProductId, ct);
			if (alreadyFollowing)
				return new UResponse(Usc.Conflict, ls.Get("AlreadyBookmarked"));
		}

		if (p.CategoryId != null) {
			bool alreadyFollowing = await db.Set<FollowEntity>()
				.AnyAsync(x => x.CreatorId == p.CreatorId && x.CategoryId == p.CategoryId, ct);
			if (alreadyFollowing)
				return new UResponse(Usc.Conflict, ls.Get("AlreadyBookmarked"));
		}

		FollowEntity userFollower = new() {
			CreatorId = p.CreatorId.Value,
			UserId = p.UserId,
			ProductId = p.ProductId,
			CategoryId = p.CategoryId,
			JsonData = new FollowJson(),
			Tags = [TagFollow.User]
		};

		await db.Set<FollowEntity>().AddAsync(userFollower, ct);
		await db.SaveChangesAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Follow);
		return new UResponse(Usc.Success, ls.Get("FollowSuccess"));
	}

	public async Task<UResponse> Unfollow(FollowParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

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
			return new UResponse(Usc.NotFound, ls.Get("FollowRelationshipNotFound"));

		db.Set<FollowEntity>().Remove(userFollower);
		await db.SaveChangesAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Follow);
		return new UResponse(Usc.Success, ls.Get("UnfollowSuccess"));
	}

	public async Task<UResponse<IEnumerable<UserEntity>>> ReadFollowers(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<UserEntity>>([], Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		p.Id ??= userData.Id;

		List<UserEntity> followers = (await db.Set<FollowEntity>()
			.Where(x => x.UserId == p.Id)
			.Select(x => x.Creator)
			.ToListAsync(ct))!;

		return new UResponse<IEnumerable<UserEntity>>(followers);
	}

	public async Task<UResponse<IEnumerable<UserEntity>>> ReadFollowedUsers(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<UserEntity>>([], Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		p.Id ??= userData.Id;
		List<UserEntity> following = (await db.Set<FollowEntity>()
			.Where(x => x.CreatorId == p.Id)
			.Select(x => x.User)
			.ToListAsync(ct))!;

		return new UResponse<IEnumerable<UserEntity>>(following);
	}

	public async Task<UResponse<IEnumerable<ProductEntity>>> ReadFollowedProducts(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<ProductEntity>>([], Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		p.Id ??= userData.Id;
		List<ProductEntity> following = (await db.Set<FollowEntity>()
			.Where(x => x.CreatorId == p.Id)
			.Select(x => x.Product)
			.ToListAsync(ct))!;

		return new UResponse<IEnumerable<ProductEntity>>(following);
	}

	public async Task<UResponse<IEnumerable<CategoryEntity>>> ReadFollowedCategories(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<CategoryEntity>>([], Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		p.Id ??= userData.Id;
		List<CategoryEntity> following = (await db.Set<FollowEntity>()
			.Where(x => x.CreatorId == p.Id)
			.Select(x => x.Category)
			.ToListAsync(ct))!;

		return new UResponse<IEnumerable<CategoryEntity>>(following);
	}

	public async Task<UResponse<FollowerFollowingCountResponse>> ReadFollowerFollowingCount(IdParams p, CancellationToken ct) {
		return new UResponse<FollowerFollowingCountResponse>(
			new FollowerFollowingCountResponse {
				Followers = await db.Set<FollowEntity>().CountAsync(x => x.UserId == p.Id, ct),
				FollowedUsers = await db.Set<FollowEntity>().CountAsync(x => x.CreatorId == p.Id, ct),
				FollowedProducts = await db.Set<FollowEntity>().CountAsync(x => x.CreatorId == p.Id && x.ProductId != null, ct),
				FollowedCategories = await db.Set<FollowEntity>().CountAsync(x => x.CreatorId == p.Id && x.CategoryId != null, ct)
			});
	}

	public async Task<UResponse<bool?>> IsFollowingUser(FollowParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<bool?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		p.CreatorId ??= userData.Id;
		bool isFollowing = await db.Set<FollowEntity>().AnyAsync(x => x.CreatorId == p.CreatorId && x.UserId == p.UserId, ct);
		return new UResponse<bool?>(isFollowing);
	}

	public async Task<UResponse<bool?>> IsFollowingProduct(FollowParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<bool?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		p.CreatorId ??= userData.Id;
		bool isFollowing = await db.Set<FollowEntity>().AnyAsync(x => x.CreatorId == p.CreatorId && x.ProductId == p.ProductId, ct);
		return new UResponse<bool?>(isFollowing);
	}

	public async Task<UResponse<bool?>> IsFollowingCategory(FollowParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<bool?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		p.CreatorId ??= userData.Id;
		bool isFollowing = await db.Set<FollowEntity>().AnyAsync(x => x.CreatorId == p.CreatorId && x.CategoryId == p.CategoryId, ct);
		return new UResponse<bool?>(isFollowing);
	}
}