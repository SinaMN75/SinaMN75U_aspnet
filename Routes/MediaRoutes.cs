namespace SinaMN75U.Routes;

using Microsoft.AspNetCore.Mvc;

public static class MediaRoutes {
	public static void MapMediaRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder route = app.MapGroup("api/media/").WithTags(tag).AddEndpointFilter<UValidationFilter>();;
		route.MapPost("Create", async ([FromForm] MediaCreateParams d, IMediaService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).Produces<UResponse<MediaResponse>>().DisableAntiforgery();
		route.MapPost("Update", async (MediaUpdateParams d, IMediaService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).Produces<UResponse<MediaResponse>>();
		route.MapPost("Delete", async (IdParams d, IMediaService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).Produces<UResponse>();
	}
}