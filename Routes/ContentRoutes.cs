namespace SinaMN75U.Routes;

public static class ContentRoutes {
	public static void MapContentRoutes(this IEndpointRouteBuilder app, string name) {
		app.MapPost("api/content/Create", async (ContentCreateParams d, IContentService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).WithTags(name).Produces<UResponse<ContentResponse>>();
		app.MapPost("api/content/Filter", async (ContentFilterParams p, IContentService s, CancellationToken c) => (await s.Filter(p, c)).ToResult()).WithTags(name).Produces<UResponse<IEnumerable<ContentResponse>>>();
		app.MapPost("api/content/Update", async (ContentUpdateParams d, IContentService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).WithTags(name).Produces<UResponse<ContentResponse>>();
		app.MapPost("api/content/Delete", async (IdParams d, IContentService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).WithTags(name).Produces<UResponse>();
	}
}