namespace SinaMN75U.Routes;

public static class FileManagerRoutes {
	public static void MapFileManagerRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Browse", async (FileManagerBrowseParams p, IFileManagerService s, CancellationToken c) => (await s.Browse(p, c)).ToResult()).Produces<UResponse<FileManagerListResponse>>();
		r.MapPost("CreateFolder", async (FileManagerCreateFolderParams p, IFileManagerService s, CancellationToken c) => (await s.CreateFolder(p, c)).ToResult()).Produces<UResponse<FileManagerEntryResponse>>();
		r.MapPost("Rename", async (FileManagerRenameParams p, IFileManagerService s, CancellationToken c) => (await s.Rename(p, c)).ToResult()).Produces<UResponse<FileManagerEntryResponse>>();
		r.MapPost("Move", async (FileManagerMoveParams p, IFileManagerService s, CancellationToken c) => (await s.Move(p, c)).ToResult()).Produces<UResponse<FileManagerEntryResponse>>();
		r.MapPost("Delete", async (FileManagerDeleteParams p, IFileManagerService s, CancellationToken c) => (await s.Delete(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Upload", async ([FromForm] FileManagerUploadParams p, IFileManagerService s, CancellationToken c) => (await s.Upload(p, c)).ToResult()).Produces<UResponse<FileManagerEntryResponse>>().DisableAntiforgery();
		r.MapGet("Download", (string path, string token, IFileManagerService s) => {
			UResponse<(string fullPath, string contentType)?> res = s.ResolveDownload(new FileManagerDeleteParams { Path = path, Token = token });
			if (res.Result == null) return Results.NotFound(res.Message);
			(string fullPath, string contentType) = res.Result.Value;
			return Results.File(new FileStream(fullPath, FileMode.Open, FileAccess.Read), contentType, Path.GetFileName(fullPath));
		});
	}
}
