using Microsoft.AspNetCore.Routing;

namespace SinaMN75U.Routes;

public static class ContentRoutes {
	public static void MapContentRoutes(this IEndpointRouteBuilder app, string name) {
		app.MapPost("content/Create", async (ContentCreateParams d, IContentService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).WithTags(name).Produces<UResponse<ContentEntity>>();
		app.MapPost("content/Read", (BaseParams d, IContentService s, CancellationToken c) => s.Read(d, c).ToResult()).WithTags(name).Produces<UResponse<IEnumerable<ContentEntity>>>();
		app.MapPost("content/Update", async (ContentUpdateParams d, IContentService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).WithTags(name).Produces<UResponse<ContentEntity>>();
		app.MapPost("content/Delete", async (IdParams d, IContentService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).WithTags(name).Produces<UResponse>();
	}
}