namespace SinaMN75U.Routes;

public static class UserRoutes {
	public static void MapUserRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		r.MapPost("Create", async (UserCreateParams d, IUserService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Create(d, c)).ToResult();
		}).Produces<UResponse<UserResponse>>();
		r.MapPost("BulkCreate", async (UserBulkCreateParams d, IUserService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.BulkCreate(d, c)).ToResult();
		}).Produces<UResponse>();
		r.MapPost("Read", async (UserReadParams d, IUserService s, CancellationToken c) => (await s.Read(d, c)).ToResult()).Cache(60).Produces<UResponse<IEnumerable<UserResponse>>>();
		r.MapPost("ReadById", async (IdParams d, IUserService s, CancellationToken c) => (await s.ReadById(d, c)).ToResult()).Produces<UResponse<UserResponse>>();
		r.MapPost("Update", async (UserUpdateParams d, IUserService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Update(d, c)).ToResult();
		}).Produces<UResponse<UserResponse>>();
		r.MapPost("Delete", async (IdParams d, IUserService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Delete(d, c)).ToResult();
		}).Produces<UResponse>();
	}
}