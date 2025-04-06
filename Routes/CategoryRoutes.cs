namespace SinaMN75U.Routes;

public static class CategoryRoutes {
	public static void MapCategoryRoutes(this IEndpointRouteBuilder app, string name) {
		RouteGroupBuilder route = app.MapGroup("api/Category/").WithTags(name);
		
		route.MapPost("Create", async (CategoryCreateParams d, ICategoryService s, IOutputCacheStore store, CancellationToken c) => {
			await store.EvictByTagAsync(name, c);
			return (await s.Create(d, c)).ToResult();
		}).Produces<UResponse<CategoryResponse>>();
		
		route.MapPost("Read", async (ICategoryService s, CancellationToken c) => (await s.Read(c)).ToResult()).CacheOutput(o => o.Tag(name)).Produces<UResponse<IEnumerable<CategoryResponse>>>();
		
		route.MapPost("Update", async (CategoryUpdateParams d, ICategoryService s, IOutputCacheStore store, CancellationToken c) => {
			await store.EvictByTagAsync(name, c);
			return (await s.Update(d, c)).ToResult();
		}).Produces<UResponse<CategoryResponse>>();
		
		route.MapPost("Delete", async (IdParams d, ICategoryService s, IOutputCacheStore store, CancellationToken c) => {
			await store.EvictByTagAsync(name, c);
			return (await s.Delete(d, c)).ToResult();
		}).Produces<UResponse<CategoryResponse>>();
	}
}