namespace SinaMN75U.Routes;

public static class MediaRoutes {
	public static void MapMediaRoutes(this IEndpointRouteBuilder app, string name) {
		app.MapPost("api/media/Create", async (MediaCreateParams d, IMediaService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).WithTags(name).Produces<UResponse<MediaEntity>>();
		app.MapPost("api/media/Update", async (MediaUpdateParams d, IMediaService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).WithTags(name).Produces<UResponse<MediaEntity>>();
		app.MapPost("api/media/Delete", async (IdParams d, IMediaService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).WithTags(name).Produces<UResponse>();
	}
}