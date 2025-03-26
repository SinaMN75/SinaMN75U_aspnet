namespace SinaMN75U.Routes;

public static class CategoryRoutes {
	public static void MapCategoryRoutes(this IEndpointRouteBuilder app, string name) {
		RouteGroupBuilder route = app.MapGroup("api/Category/");
		route.MapPost("Create", async (CategoryCreateParams d, ICategoryService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).WithTags(name).Produces<UResponse<CategoryResponse>>();
		route.MapPost("Read", async (ICategoryService s, CancellationToken c) => (await s.Read(c)).ToResult()).WithTags(name).Produces<UResponse<IEnumerable<CategoryResponse>>>();
		route.MapPost("Update", async (CategoryUpdateParams d, ICategoryService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).WithTags(name).Produces<UResponse<CategoryResponse>>();
		route.MapPost("Delete", async (IdParams d, ICategoryService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).WithTags(name).Produces<UResponse<CategoryResponse>>();
	}
}