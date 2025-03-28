namespace SinaMN75U.Routes;

public static class UserRoutes {
	public static void MapUserRoutes(this IEndpointRouteBuilder app, string name) {
		RouteGroupBuilder route = app.MapGroup("api/user/");
		route.MapPost("Create", async (UserCreateParams d, IUserService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).WithTags(name).Produces<UResponse<UserEntity>>();
		route.MapPost("ReadById", async (IdParams d, IUserService s, CancellationToken c) => (await s.ReadById(d, c)).ToResult()).WithTags(name).Produces<UResponse<UserEntity>>();
		route.MapPost("Filter", async (UserFilterParams d, IUserService s, CancellationToken c) => (await s.Filter(d, c)).ToResult()).WithTags(name).Produces<UResponse<IEnumerable<UserEntity>>>();
		route.MapPost("Update", async (UserUpdateParams d, IUserService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).WithTags(name).Produces<UResponse<UserEntity>>();
		route.MapPost("Delete", async (IdParams d, IUserService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).WithTags(name).Produces<UResponse>();
	}
}