namespace SinaMN75U.Routes;

public static class CategoryRoutes {
	public static void MapCategoryRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		r.MapPost("Create", async (CategoryCreateParams d, ICategoryService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Create(d, c)).ToResult();
		}).Produces<UResponse<CategoryResponse>>();

		r.MapPost("Read", async (CategoryReadParams p, ICategoryService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Cache(60).Produces<UResponse<IEnumerable<CategoryResponse>>>();

		r.MapPost("Update", async (CategoryUpdateParams d, ICategoryService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Update(d, c)).ToResult();
		}).Produces<UResponse<CategoryResponse>>();

		r.MapPost("Delete", async (IdParams d, ICategoryService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Delete(d, c)).ToResult();
		}).Produces<UResponse<CategoryResponse>>();
		
		r.MapPost("DeleteRange", async (IdListParams d, ICategoryService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.DeleteRange(d.Ids, c)).ToResult();
		}).Produces<UResponse<CategoryResponse>>();
	}
}