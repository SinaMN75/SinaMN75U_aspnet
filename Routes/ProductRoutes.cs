namespace SinaMN75U.Routes;

public static class ProductRoutes {
	public static void MapProductRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		r.MapPost("Create", async (ProductCreateParams d, IProductService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Create(d, c)).ToResult();
		}).Produces<UResponse<ProductResponse>>();

		r.MapPost("Read", async (ProductReadParams p, IProductService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Cache(60).Produces<UResponse<IEnumerable<ProductResponse>>>();

		r.MapPost("ReadById", async (IdParams p, IProductService s, CancellationToken c) => (await s.ReadById(p, c)).ToResult()).Produces<UResponse<ProductResponse>>();

		r.MapPost("Update", async (ProductUpdateParams d, IProductService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Update(d, c)).ToResult();
		}).Produces<UResponse<ProductResponse>>();

		r.MapPost("Delete", async (IdParams d, IProductService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Delete(d, c)).ToResult();
		}).Produces<UResponse<ProductResponse>>();
	}
}