namespace SinaMN75U.Routes;

public static class ProductRoutes {
	public static void MapProductRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder route = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		route.MapPost("Create", async (ProductCreateParams d, IProductService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Create(d, c)).ToResult();
		}).Produces<UResponse<ProductResponse>>();

		route.MapPost("Read", async (ProductReadParams p, IProductService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Cache(o => o.Minutes = 60).Produces<UResponse<IEnumerable<ProductResponse>>>();

		route.MapPost("ReadById", async (IdParams p, IProductService s, ILocalStorageService ls, CancellationToken c) => (await s.ReadById(p, c)).ToResult()).Produces<UResponse<ProductResponse>>();

		route.MapPost("Update", async (ProductUpdateParams d, IProductService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Update(d, c)).ToResult();
		}).Produces<UResponse<ProductResponse>>();

		route.MapPost("Delete", async (IdParams d, IProductService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Delete(d, c)).ToResult();
		}).Produces<UResponse<ProductResponse>>();
	}
}