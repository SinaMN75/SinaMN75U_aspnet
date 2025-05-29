namespace SinaMN75U.Routes;

using Microsoft.AspNetCore.Mvc;

public static class MediaRoutes {
	public static void MapMediaRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async ([FromForm] MediaCreateParams d, IMediaService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).Produces<UResponse<MediaResponse>>().DisableAntiforgery();
		r.MapPost("Read", async ([FromForm] BaseReadParams<TagMedia> p, IMediaService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<MediaResponse>>>();
		r.MapPost("Update", async (MediaUpdateParams d, IMediaService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).Produces<UResponse<MediaResponse>>();
		r.MapPost("Delete", async (IdParams d, IMediaService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).Produces<UResponse>();
	}
}