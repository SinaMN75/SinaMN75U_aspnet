namespace SinaMN75U.Routes;

public static class CategoryRoutes {
	public static void MapCategoryRoutes(this IEndpointRouteBuilder app, string name) {
		app.MapPost("api/userCategory/Create", async (CategoryCreateParams d, ICategoryService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).WithTags(name).Produces<UResponse<CategoryResponse>>();
		app.MapPost("api/userCategory/Read", async (ICategoryService s, CancellationToken c) => (await s.Read(c)).ToResult()).WithTags(name).Produces<UResponse<IEnumerable<CategoryResponse>>>();
		app.MapPost("api/userCategory/Update", async (CategoryUpdateParams d, ICategoryService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).WithTags(name).Produces<UResponse<CategoryResponse>>();
		app.MapPost("api/userCategory/Delete", async (IdParams d, ICategoryService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).WithTags(name).Produces<UResponse<CategoryResponse>>();
	}
}