namespace SinaMN75U.Routes;

public static class FollowRoutes {
	public static void MapFollowRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		
		r.MapPost("Follow", async (FollowParams p, IFollowService s, CancellationToken c) => (await s.Follow(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Unfollow", async (FollowParams p, IFollowService s, CancellationToken c) => (await s.Unfollow(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("ReadFollowers", async (IdParams p, IFollowService s, CancellationToken c) => (await s.ReadFollowers(p, c)).ToResult()).Produces<UResponse<IEnumerable<UserEntity>>>();
		r.MapPost("ReadFollowedUsers", async (IdParams p, IFollowService s, CancellationToken c) => (await s.ReadFollowedUsers(p, c)).ToResult()).Produces<UResponse<IEnumerable<UserEntity>>>();
		r.MapPost("ReadFollowedProducts", async (IdParams p, IFollowService s, CancellationToken c) => (await s.ReadFollowedProducts(p, c)).ToResult()).Produces<UResponse<IEnumerable<ProductEntity>>>();
		r.MapPost("ReadFollowedCategories", async (IdParams p, IFollowService s, CancellationToken c) => (await s.ReadFollowedCategories(p, c)).ToResult()).Produces<UResponse<IEnumerable<CategoryEntity>>>();
		r.MapPost("ReadFollowerFollowingCount", async (IdParams p, IFollowService s, CancellationToken c) => (await s.ReadFollowerFollowingCount(p, c)).ToResult()).Produces<UResponse<FollowerFollowingCountResponse>>();
		r.MapPost("IsFollowingUser", async (FollowParams p, IFollowService s, CancellationToken c) => (await s.IsFollowingUser(p, c)).ToResult()).Produces<UResponse<bool?>>();
		r.MapPost("IsFollowingProduct", async (FollowParams p, IFollowService s, CancellationToken c) => (await s.IsFollowingProduct(p, c)).ToResult()).Produces<UResponse<bool?>>();
		r.MapPost("IsFollowingCategory", async (FollowParams p, IFollowService s, CancellationToken c) => (await s.IsFollowingCategory(p, c)).ToResult()).Produces<UResponse<bool?>>();
	}
}