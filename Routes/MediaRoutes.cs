namespace SinaMN75U.Routes;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

public static class MediaRoutes {
	public static void MapMediaRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async ([FromForm] MediaCreateParams d, IMediaService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).Produces<UResponse<MediaEntity>>().DisableAntiforgery();
		r.MapPost("Read", async (BaseReadParams<TagMedia> p, IMediaService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<MediaEntity>>>();
		r.MapPost("Update", async (MediaUpdateParams d, IMediaService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).Produces<UResponse<MediaEntity>>();
		r.MapPost("Delete", async (IdParams d, IMediaService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).Produces<UResponse>();
		r.MapGet("Download", (string filePath, IWebHostEnvironment env) => {
			if (!File.Exists(filePath))
				return Task.FromResult(Results.NotFound("File not found"));

			FileExtensionContentTypeProvider provider = new();
			if (!provider.TryGetContentType(filePath, out string? contentType)) {
				contentType = "application/octet-stream";
			}

			FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read);
			return Task.FromResult(Results.File(fileStream, contentType, fileDownloadName: Path.Combine(env.WebRootPath, "Media", filePath)));
		});
	}
}