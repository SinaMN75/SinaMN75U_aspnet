namespace SinaMN75U.Routes;

public static class MediaRoutes {
	public static void MapMediaRoutes(this IEndpointRouteBuilder app, string name) {
		RouteGroupBuilder route = app.MapGroup("api/media/");
		route.MapPost("Create", async (MediaCreateParams d, IMediaService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).WithTags(name).Produces<UResponse<MediaEntity>>();
		route.MapPost("Update", async (MediaUpdateParams d, IMediaService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).WithTags(name).Produces<UResponse<MediaEntity>>();
		route.MapPost("Delete", async (IdParams d, IMediaService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).WithTags(name).Produces<UResponse>();
	}
}