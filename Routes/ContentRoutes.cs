namespace SinaMN75U.Routes;

public static class ContentRoutes {
	public static void MapContentRoutes(this IEndpointRouteBuilder app, string name) {
		RouteGroupBuilder route = app.MapGroup("api/content/");
		route.MapPost("Create", async (ContentCreateParams d, IContentService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).WithTags(name).Produces<UResponse<ContentResponse>>();
		route.MapPost("Filter", async (ContentFilterParams p, IContentService s, CancellationToken c) => (await s.Filter(p, c)).ToResult()).WithTags(name).Produces<UResponse<IEnumerable<ContentResponse>>>();
		route.MapPost("Update", async (ContentUpdateParams d, IContentService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).WithTags(name).Produces<UResponse<ContentResponse>>();
		route.MapPost("Delete", async (IdParams d, IContentService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).WithTags(name).Produces<UResponse>();
	}
}