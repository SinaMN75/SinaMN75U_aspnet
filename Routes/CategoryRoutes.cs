namespace SinaMN75U.Routes;

public static class CategoryRoutes {
	public static void MapCategoryRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder route = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		route.MapPost("Create", async (CategoryCreateParams d, ICategoryService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Create(d, c)).ToResult();
		}).Produces<UResponse<CategoryResponse>>();

		route.MapPost("Read", async (CategoryReadParams p, ICategoryService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Cache(o => o.Minutes = 60).Produces<UResponse<IEnumerable<CategoryResponse>>>();

		route.MapPost("Update", async (CategoryUpdateParams d, ICategoryService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Update(d, c)).ToResult();
		}).Produces<UResponse<CategoryResponse>>();

		route.MapPost("Delete", async (IdParams d, ICategoryService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Delete(d, c)).ToResult();
		}).Produces<UResponse<CategoryResponse>>();
	}
}