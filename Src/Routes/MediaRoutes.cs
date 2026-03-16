namespace SinaMN75U.Routes;

public static class MediaRoutes {
	public static void MapMediaRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async ([FromForm] MediaCreateParams d, IMediaRepository s, CancellationToken c) => (await s.Create(d, c)).ToResult()).Produces<UResponse<MediaResponse>>().DisableAntiforgery();
		r.MapPost("Read", async (BaseReadParams<TagMedia> p, IMediaRepository s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<MediaResponse>>>();
		r.MapPost("Update", async (MediaUpdateParams d, IMediaRepository s, CancellationToken c) => (await s.Update(d, c)).ToResult()).Produces<UResponse<MediaResponse>>();
		r.MapPost("Delete", async (IdParams d, IMediaRepository s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).Produces<UResponse>();
		r.MapGet("Download", (string filePath, IWebHostEnvironment env) => {
			if (!File.Exists(filePath)) return Task.FromResult(Results.NotFound("File not found"));
			FileExtensionContentTypeProvider provider = new();
			if (!provider.TryGetContentType(filePath, out string? contentType)) contentType = "application/octet-stream";
			return Task.FromResult(Results.File(new FileStream(filePath, FileMode.Open, FileAccess.Read), contentType, Path.Combine(env.WebRootPath, "Media", filePath)));
		});
	}
}