namespace SinaMN75U.Services;

public interface IFollowService {
	Task<UResponse> Follow(FollowParams p, CancellationToken ct);
	Task<UResponse> Unfollow(FollowParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<UserEntity>>> ReadFollowers(UserIdParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<UserEntity>>> ReadFollowedUsers(UserIdParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ProductEntity>>> ReadFollowedProducts(UserIdParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<CategoryEntity>>> ReadFollowedCategories(UserIdParams p, CancellationToken ct);
	Task<UResponse<FollowerFollowingCountResponse>> ReadFollowerFollowingCount(UserIdParams p, CancellationToken ct);
}

public class FollowService(DbContext db, ILocalizationService ls, ITokenService ts) : IFollowService {
	public async Task<UResponse> Follow(FollowParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		p.UserId ??= userData.Id;

		if (p.TargetUserId != null) {
			if (p.UserId == p.TargetUserId) return new UResponse(Usc.BadRequest, ls.Get("CannotFollowYourself"));
			bool alreadyFollowing = await db.Set<FollowEntity>()
				.AnyAsync(x => x.UserId == p.UserId && x.TargetUserId == p.TargetUserId, ct);

			if (alreadyFollowing)
				return new UResponse(Usc.Conflict, ls.Get("AlreadyFollowingUser"));
		}

		if (p.TargetProductId != null) {
			bool alreadyFollowing = await db.Set<FollowEntity>()
				.AnyAsync(x => x.UserId == p.UserId && x.TargetProductId == p.TargetProductId, ct);
			if (alreadyFollowing)
				return new UResponse(Usc.Conflict, ls.Get("AlreadyFollowingUser"));
		}

		if (p.TargetCategoryId != null) {
			bool alreadyFollowing = await db.Set<FollowEntity>()
				.AnyAsync(x => x.UserId == p.UserId && x.TargetCategoryId == p.TargetCategoryId, ct);
			if (alreadyFollowing)
				return new UResponse(Usc.Conflict, ls.Get("AlreadyFollowingUser"));
		}

		FollowEntity userFollower = new() {
			UserId = p.UserId.Value,
			TargetUserId = p.TargetUserId,
			TargetProductId = p.TargetProductId,
			TargetCategoryId = p.TargetCategoryId,
			JsonData = new FollowJson(),
			Tags = [TagFollow.User]
		};

		await db.Set<FollowEntity>().AddAsync(userFollower, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse(Usc.Success, ls.Get("FollowSuccess"));
	}

	public async Task<UResponse> Unfollow(FollowParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		p.UserId ??= userData.Id;

		FollowEntity? userFollower = null;

		if (p.TargetUserId != null) 
			userFollower = await db.Set<FollowEntity>()
				.FirstOrDefaultAsync(x => x.UserId == p.UserId && x.TargetUserId == p.TargetUserId, ct);

		if (p.TargetProductId != null) 
			userFollower = await db.Set<FollowEntity>()
				.FirstOrDefaultAsync(x => x.UserId == p.UserId && x.TargetProductId == p.TargetProductId, ct);

		if (p.TargetCategoryId != null) 
			userFollower = await db.Set<FollowEntity>()
				.FirstOrDefaultAsync(x => x.UserId == p.UserId && x.TargetCategoryId == p.TargetCategoryId, ct);
		
		if (userFollower == null)
			return new UResponse(Usc.NotFound, ls.Get("FollowRelationshipNotFound"));

		db.Set<FollowEntity>().Remove(userFollower);
		await db.SaveChangesAsync(ct);

		return new UResponse(Usc.Success, ls.Get("UnfollowSuccess"));
	}

	public async Task<UResponse<IEnumerable<UserEntity>>> ReadFollowers(UserIdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<UserEntity>>([], Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		p.UserId ??= userData.Id;

		List<UserEntity> followers = (await db.Set<FollowEntity>()
			.Where(x => x.TargetUserId == p.UserId)
			.Select(x => x.User)
			.ToListAsync(ct))!;

		return new UResponse<IEnumerable<UserEntity>>(followers);
	}

	public async Task<UResponse<IEnumerable<UserEntity>>> ReadFollowedUsers(UserIdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<UserEntity>>([], Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		p.UserId ??= userData.Id;
		List<UserEntity> following = (await db.Set<FollowEntity>()
			.Where(x => x.UserId == p.UserId)
			.Select(x => x.TargetUser)
			.ToListAsync(ct))!;

		return new UResponse<IEnumerable<UserEntity>>(following);
	}
	
	public async Task<UResponse<IEnumerable<ProductEntity>>> ReadFollowedProducts(UserIdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<ProductEntity>>([], Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		p.UserId ??= userData.Id;
		List<ProductEntity> following = (await db.Set<FollowEntity>()
			.Where(x => x.UserId == p.UserId)
			.Select(x => x.TargetProduct)
			.ToListAsync(ct))!;
		
		return new UResponse<IEnumerable<ProductEntity>>(following);
	}	
	
	public async Task<UResponse<IEnumerable<CategoryEntity>>> ReadFollowedCategories(UserIdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<CategoryEntity>>([], Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		p.UserId ??= userData.Id;
		List<CategoryEntity> following = (await db.Set<FollowEntity>()
			.Where(x => x.UserId == p.UserId)
			.Select(x => x.TargetCategory)
			.ToListAsync(ct))!;
		
		return new UResponse<IEnumerable<CategoryEntity>>(following);
	}

	public async Task<UResponse<FollowerFollowingCountResponse>> ReadFollowerFollowingCount(UserIdParams p, CancellationToken ct) => new(
		new FollowerFollowingCountResponse {
			Followers = await db.Set<FollowEntity>().CountAsync(x => x.TargetUserId == p.UserId, ct),
			FollowedUsers = await db.Set<FollowEntity>().CountAsync(x => x.UserId == p.UserId, ct),
			FollowedProducts = await db.Set<FollowEntity>().CountAsync(x => x.UserId == p.UserId && x.TargetProductId != null, ct),
			FollowedCategories = await db.Set<FollowEntity>().CountAsync(x => x.UserId == p.UserId && x.TargetCategoryId != null, ct),
		});
}