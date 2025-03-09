using Microsoft.AspNetCore.Routing;
using SinaMN75U.Data.Params.Media;
using SinaMN75U.Services;

namespace SinaMN75U.Routes;

public static class MediaRoutes {
	public static void MapMediaRoutes(this IEndpointRouteBuilder app, string name) {
		app.MapPost("media/Create", async (MediaCreateParams d, IMediaService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).WithTags(name);
		app.MapPost("media/Update", async (MediaUpdateParams d, IMediaService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).WithTags(name);
		app.MapPost("media/Delete", async (IdParams d, IMediaService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).WithTags(name);
	}
}