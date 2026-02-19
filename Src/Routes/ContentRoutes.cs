namespace SinaMN75U.Routes;

public static class ContentRoutes {
	public static void MapContentRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (ContentCreateParams d, IContentService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).Produces<UResponse<ContentResponse>>();
		r.MapPost("Read", async (ContentReadParams p, IContentService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<ContentResponse>>>();
		r.MapPost("Update", async (ContentUpdateParams d, IContentService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).Produces<UResponse<ContentResponse>>();
		r.MapPost("Delete", async (IdParams d, IContentService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("SoftDelete", async (SoftDeleteParams d, IContentService s, CancellationToken c) => (await s.SoftDelete(d, c)).ToResult()).Produces<UResponse>();
	}
}