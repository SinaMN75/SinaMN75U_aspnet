namespace SinaMN75U.Routes;

public static class UserRoutes {
	public static void MapUserRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (UserCreateParams p, IUserService s, CancellationToken c) => (await s.Create(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("BulkCreate", async (UserBulkCreateParams d, IUserService s, CancellationToken c) => (await s.BulkCreate(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Read", async (UserReadParams d, IUserService s, CancellationToken c) => (await s.Read(d, c)).ToResult()).Produces<UResponse<IEnumerable<UserResponse>>>();
		r.MapPost("ReadById", async (IdParams<UserSelectorArgs> d, IUserService s, CancellationToken c) => (await s.ReadById(d, c)).ToResult()).Produces<UResponse<UserResponse>>();
		r.MapPost("Update", async (UserUpdateParams d, IUserService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Delete", async (IdParams d, IUserService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("ReadExtraById", async (IdParams d, IUserService s, CancellationToken c) => (await s.ReadExtraById(d, c)).ToResult()).Produces<UResponse<UserExtraResponse>>();
		r.MapPost("UpdateExtra", async (UserExtraUpdateParams d, IUserService s, CancellationToken c) => (await s.UpdateExtra(d, c)).ToResult()).Produces<UResponse>();
	}
}