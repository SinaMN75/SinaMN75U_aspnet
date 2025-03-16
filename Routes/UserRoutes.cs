using Microsoft.AspNetCore.Routing;

namespace SinaMN75U.Routes;

public static class UserRoutes {
	public static void MapUserRoutes(this IEndpointRouteBuilder app, string name) {
		app.MapPost("user/Create", async (UserCreateParams d, IUserService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).WithTags(name).Produces<UResponse<UserEntity>>();
		app.MapPost("user/ReadById", async (IdParams d, IUserService s, CancellationToken c) => (await s.ReadById(d, c)).ToResult()).WithTags(name).Produces<UResponse<UserEntity>>();
		app.MapPost("user/Filter", async (UserFilterParams d, IUserService s, CancellationToken c) => (await s.Filter(d, c)).ToResult()).WithTags(name).Produces<UResponse<IEnumerable<UserEntity>>>();
		app.MapPost("user/Update", async (UserUpdateParams d, IUserService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).WithTags(name).Produces<UResponse<UserEntity>>();
		app.MapPost("user/Delete", async (IdParams d, IUserService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).WithTags(name).Produces<UResponse>();
	}
}