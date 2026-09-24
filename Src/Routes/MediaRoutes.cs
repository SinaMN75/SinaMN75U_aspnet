namespace SinaMN75U.Routes;

public static class MediaRoutes {
	public static void MapMediaRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async ([FromForm] MediaCreateParams d, IMediaService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).Produces<UResponse>().DisableAntiforgery();
		r.MapPost("Read", async (BaseReadParams<TagMedia> p, IMediaService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<MediaResponse>>>();
		r.MapPost("Update", async (MediaUpdateParams d, IMediaService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Delete", async (IdParams d, IMediaService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DeleteRange", async (IdListParams p, IMediaService s, CancellationToken c) => (await s.DeleteRange(p, c)).ToResult()).Produces<UResponse>();
		r.MapGet("Download", (string filePath, IWebHostEnvironment env) => {
			string? fullPath = ResolveMediaPath(env, filePath);
			if (fullPath == null) return Results.NotFound("File not found");
			if (!ContentTypeProvider.TryGetContentType(fullPath, out string? contentType)) contentType = "application/octet-stream";
			return Results.File(path: fullPath, contentType: contentType, fileDownloadName: Path.GetFileName(fullPath), enableRangeProcessing: true);
		});
	}

	private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

	private static string? ResolveMediaPath(IWebHostEnvironment env, string? filePath) {
		if (string.IsNullOrWhiteSpace(filePath)) return null;
		string mediaRoot = Path.GetFullPath(Path.Combine(env.WebRootPath, "Media")) + Path.DirectorySeparatorChar;
		return new[] { Path.Combine(mediaRoot, filePath.TrimStart('/', '\\')), filePath }.Select(Path.GetFullPath).FirstOrDefault(full => full.StartsWith(mediaRoot, StringComparison.Ordinal) && File.Exists(full));
	}
}