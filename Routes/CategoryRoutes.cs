using Microsoft.AspNetCore.Routing;
using SinaMN75U.Data.Params.Category;
using SinaMN75U.Services;

namespace SinaMN75U.Routes;

public static class CategoryRoutes {
	public static void MapCategoryRoutes(this IEndpointRouteBuilder app, string name) {
		app.MapPost("userCategory/Create", async (CategoryCreateParams d, ICategoryService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).WithTags(name).Produces<UResponse<CategoryEntity>>();
		app.MapPost("userCategory/Read", async (ICategoryService s, CancellationToken c) => (await s.Read(c)).ToResult()).WithTags(name).Produces<UResponse<IEnumerable<CategoryEntity>>>();
		app.MapPost("userCategory/Update", async (CategoryUpdateParams d, ICategoryService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).WithTags(name).Produces<UResponse<CategoryEntity>>();
		app.MapPost("userCategory/Delete", async (IdParams d, ICategoryService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).WithTags(name).Produces<UResponse<CategoryEntity>>();
	}
}