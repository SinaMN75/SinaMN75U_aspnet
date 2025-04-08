namespace SinaMN75U.Routes;

public static class CategoryRoutes {
	public static void MapCategoryRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder route = app.MapGroup("api/Category/").WithTags(tag).AddEndpointFilter<ValidationFilter>();

		route.MapPost("Create", async (CategoryCreateParams d, ICategoryService s, IOutputCacheStore store, CancellationToken c) => {
			await store.EvictByTagAsync(tag, c);
			return (await s.Create(d, c)).ToResult();
		}).Produces<UResponse<CategoryResponse>>();

		route.MapPost("Read", async (CategoryReadParams p, ICategoryService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).CacheOutput(o => o.Tag(tag)).Produces<UResponse<IEnumerable<CategoryResponse>>>();

		route.MapPost("Update", async (CategoryUpdateParams d, ICategoryService s, IOutputCacheStore store, CancellationToken c) => {
			await store.EvictByTagAsync(tag, c);
			return (await s.Update(d, c)).ToResult();
		}).Produces<UResponse<CategoryResponse>>();

		route.MapPost("Delete", async (IdParams d, ICategoryService s, IOutputCacheStore store, CancellationToken c) => {
			await store.EvictByTagAsync(tag, c);
			return (await s.Delete(d, c)).ToResult();
		}).Produces<UResponse<CategoryResponse>>();
	}
}