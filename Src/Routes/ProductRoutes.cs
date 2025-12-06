namespace SinaMN75U.Routes;

public static class ProductRoutes {
	public static void MapProductRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		r.MapPost("Create", async (ProductCreateParams p, IProductService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Create(p, c)).ToResult();
		}).Produces<UResponse<ProductEntity>>();

		r.MapPost("BulkCreate", async (List<ProductCreateParams> p, IProductService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.BulkCreate(p, c)).ToResult();
		}).Produces<UResponse<ProductEntity>>();

		r.MapPost("Read", async (ProductReadParams p, IProductService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Cache(10).Produces<UResponse<IEnumerable<ProductEntity>>>();

		r.MapPost("ReadById", async (IdParams p, IProductService s, CancellationToken c) => (await s.ReadById(p, c)).ToResult()).Produces<UResponse<ProductEntity>>();

		r.MapPost("Update", async (ProductUpdateParams p, IProductService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Update(p, c)).ToResult();
		}).Produces<UResponse<ProductEntity>>();

		r.MapPost("Delete", async (IdParams p, IProductService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Delete(p, c)).ToResult();
		}).Produces<UResponse>();

		r.MapPost("DeleteRange", async (IdListParams p, IProductService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.DeleteRange(p, c)).ToResult();
		}).Produces<UResponse>();
	}
}