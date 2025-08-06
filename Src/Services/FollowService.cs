namespace SinaMN75U.Services;

public interface IFollowService {
	Task<UResponse> Follow(FollowParams p, CancellationToken c);
	Task<UResponse> Unfollow(FollowParams p, CancellationToken c);
	Task<UResponse<IEnumerable<UserEntity>>> ReadFollowers(UserIdParams p, CancellationToken c);
	Task<UResponse<IEnumerable<UserEntity>>> ReadFollowings(UserIdParams p, CancellationToken c);
	Task<UResponse<FollowerFollowingCountResponse>> ReadFollowerFollowingCount(UserIdParams p, CancellationToken c);
}

public class FollowService(DbContext db, ILocalizationService ls, ITokenService ts) : IFollowService {
	public async Task<UResponse> Follow(FollowParams p, CancellationToken c) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		p.UserId ??= userData.Id;

		if (p.UserId == p.TargetUserId)
			return new UResponse(Usc.BadRequest, ls.Get("CannotFollowYourself"));

		bool alreadyFollowing = await db.Set<FollowEntity>()
			.AnyAsync(x => x.FollowerUserId == p.UserId && x.FollowedUserId == p.TargetUserId);

		if (alreadyFollowing)
			return new UResponse(Usc.Conflict, ls.Get("AlreadyFollowingUser"));

		FollowEntity userFollower = new() {
			FollowerUserId = p.UserId.Value,
			FollowedUserId = p.TargetUserId
		};

		await db.Set<FollowEntity>().AddAsync(userFollower);
		await db.SaveChangesAsync();

		return new UResponse(Usc.Success, ls.Get("FollowSuccess"));
	}

	public async Task<UResponse> Unfollow(FollowParams p, CancellationToken c) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		p.UserId ??= userData.Id;

		FollowEntity? userFollower = await db.Set<FollowEntity>()
			.FirstOrDefaultAsync(x => x.FollowerUserId == p.UserId && x.FollowedUserId == p.TargetUserId);

		if (userFollower == null)
			return new UResponse(Usc.NotFound, ls.Get("FollowRelationshipNotFound"));

		db.Set<FollowEntity>().Remove(userFollower);
		await db.SaveChangesAsync();

		return new UResponse(Usc.Success, ls.Get("UnfollowSuccess"));
	}

	public async Task<UResponse<IEnumerable<UserEntity>>> ReadFollowers(UserIdParams p, CancellationToken c) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<UserEntity>>([], Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		p.UserId ??= userData.Id;

		List<UserEntity> followers = (await db.Set<FollowEntity>()
			.Where(uf => uf.FollowedUserId == p.UserId)
			.Select(uf => uf.FollowerUser)
			.ToListAsync())!;

		return new UResponse<IEnumerable<UserEntity>>(followers);
	}

	public async Task<UResponse<IEnumerable<UserEntity>>> ReadFollowings(UserIdParams p, CancellationToken c) {
		List<UserEntity> following = (await db.Set<FollowEntity>()
			.Where(uf => uf.FollowerUserId == p.UserId)
			.Select(uf => uf.FollowedUser)
			.ToListAsync())!;

		return new UResponse<IEnumerable<UserEntity>>(following);
	}
	
	public async Task<UResponse<FollowerFollowingCountResponse>> ReadFollowerFollowingCount(UserIdParams p, CancellationToken c) => new(
		new FollowerFollowingCountResponse {
			Followers = await db.Set<FollowEntity>().CountAsync(uf => uf.FollowedUserId == p.UserId),
			Followings = await db.Set<FollowEntity>().CountAsync(uf => uf.FollowerUserId == p.UserId)
		});
}