namespace SinaMN75U.Routes;

public static class CategoryRoutes {
	public static void MapCategoryRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (CategoryCreateParams p, ICategoryService s, CancellationToken c) => (await s.Create(p, c)).ToResult()).Produces<UResponse<CategoryEntity>>();
		r.MapPost("BulkCreate", async (List<CategoryCreateParams> p, ICategoryService s, CancellationToken c) => (await s.BulkCreate(p, c)).ToResult()).Produces<UResponse<CategoryEntity>>();
		r.MapPost("Read", async (CategoryReadParams p, ICategoryService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Cache(1).Produces<UResponse<IEnumerable<CategoryEntity>>>();
		r.MapPost("ReadDept", async (CategoryReadParams p, ICategoryService s, CancellationToken c) => (await s.ReadDept(p, c)).ToResult()).Cache(1).Produces<UResponse<IEnumerable<CategoryEntity>>>();
		r.MapPost("ReadById", async (IdParams p, ICategoryService s, CancellationToken c) => (await s.ReadById(p, c)).ToResult()).Cache(1).Produces<UResponse<CategoryEntity>>();
		r.MapPost("Update", async (CategoryUpdateParams p, ICategoryService s, CancellationToken c) => (await s.Update(p, c)).ToResult()).Produces<UResponse<CategoryEntity>>();
		r.MapPost("Delete", async (IdParams p, ICategoryService s, CancellationToken c) => (await s.Delete(p, c)).ToResult()).Produces<UResponse<CategoryEntity>>();
		r.MapPost("DeleteRange", async (IdListParams p, ICategoryService s, CancellationToken c) => (await s.DeleteRange(p, c)).ToResult()).Produces<UResponse<CategoryEntity>>();
	}
}