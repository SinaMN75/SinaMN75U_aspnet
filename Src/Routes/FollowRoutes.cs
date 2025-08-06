namespace SinaMN75U.Routes;

public static class FollowRoutes {
	public static void MapFollowRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		
		r.MapPost("Follow", async (FollowParams p, IFollowService s, CancellationToken c) => (await s.Follow(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Unfollow", async (FollowParams p, IFollowService s, CancellationToken c) => (await s.Unfollow(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("ReadFollowers", async (UserIdParams p, IFollowService s, CancellationToken c) => (await s.ReadFollowers(p, c)).ToResult()).Produces<UResponse<IEnumerable<UserEntity>>>();
		r.MapPost("ReadFollowing", async (UserIdParams p, IFollowService s, CancellationToken c) => (await s.ReadFollowing(p, c)).ToResult()).Produces<UResponse<IEnumerable<UserEntity>>>();
		r.MapPost("ReadFollowerFollowingCount", async (UserIdParams p, IFollowService s, CancellationToken c) => (await s.ReadFollowerFollowingCount(p, c)).ToResult()).Produces<UResponse<FollowerFollowingCountResponse>>();
	}
}