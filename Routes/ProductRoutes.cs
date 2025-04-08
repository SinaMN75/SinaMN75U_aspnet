namespace SinaMN75U.Routes;

public static class ProductRoutes {
	public static void MapProductRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder route = app.MapGroup("api/Product/").WithTags(tag).AddEndpointFilter<UValidationFilter>();;

		route.MapPost("Create", async (ProductCreateParams d, IProductService s, IOutputCacheStore store, CancellationToken c) => {
			await store.EvictByTagAsync(tag, c);
			return (await s.Create(d, c)).ToResult();
		}).Produces<UResponse<ProductResponse>>();

		route.MapPost("Read", async (ProductReadParams p, IProductService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).CacheOutput(o => o.Tag(tag)).Produces<UResponse<IEnumerable<ProductResponse>>>();
		
		route.MapPost("ReadById", async (IdParams p, IProductService s, CancellationToken c) => (await s.ReadById(p, c)).ToResult()).CacheOutput(o => o.Tag(tag)).Produces<UResponse<ProductResponse>>();

		route.MapPost("Update", async (ProductUpdateParams d, IProductService s, IOutputCacheStore store, CancellationToken c) => {
			await store.EvictByTagAsync(tag, c);
			return (await s.Update(d, c)).ToResult();
		}).Produces<UResponse<ProductResponse>>();

		route.MapPost("Delete", async (IdParams d, IProductService s, IOutputCacheStore store, CancellationToken c) => {
			await store.EvictByTagAsync(tag, c);
			return (await s.Delete(d, c)).ToResult();
		}).Produces<UResponse<ProductResponse>>();
	}
}