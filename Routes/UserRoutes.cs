namespace SinaMN75U.Routes;

public static class UserRoutes {
	public static void MapUserRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder route = app.MapGroup("api/user/").WithTags(tag).AddEndpointFilter<UValidationFilter>();
		
		route.MapPost("Create", async (UserCreateParams d, IUserService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).Produces<UResponse<UserResponse>>();
		route.MapPost("BulkCreate", async (UserBulkCreateParams d, IUserService s, CancellationToken c) => (await s.BulkCreate(d, c)).ToResult()).Produces<UResponse>();
		route.MapPost("Read", async (UserReadParams d, IUserService s, CancellationToken c) => (await s.Read(d, c)).ToResult()).Produces<UResponse<IEnumerable<UserResponse>>>();
		route.MapPost("ReadById", async (IdParams d, IUserService s, CancellationToken c) => (await s.ReadById(d, c)).ToResult()).Produces<UResponse<UserResponse>>();
		route.MapPost("Update", async (UserUpdateParams d, IUserService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).Produces<UResponse<UserResponse>>();
		route.MapPost("Delete", async (IdParams d, IUserService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).Produces<UResponse>();
	}
}