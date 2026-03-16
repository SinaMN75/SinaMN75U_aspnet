namespace SinaMN75U.Data.Media;

public interface IMediaRepository {
	Task<UResponse<MediaResponse?>> Create(MediaCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<MediaResponse>?>> Read(BaseReadParams<TagMedia> p, CancellationToken ct);
	Task<UResponse<MediaResponse?>> Update(MediaUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class MediaRepository(
	IMediaService mediaService,
	IWebHostEnvironment env
) : IMediaRepository {
	private static readonly string[] AllowedExtensions = [
		".png", ".gif", ".jpg", ".jpeg", ".svg", ".webp", ".mp4", ".mov", ".mp3", ".pdf", ".aac", ".apk", ".zip", ".rar", ".mkv"
	];

	public async Task<UResponse<MediaResponse?>> Create(MediaCreateParams p, CancellationToken ct) {
		string ext = Path.GetExtension(p.File.FileName).ToLowerInvariant();
		if (!AllowedExtensions.Contains(ext))
			return new UResponse<MediaResponse?>(null, Usc.MediaTypeNotSupported);

		string folder = GetFolderName(p);
		string name = $"{folder}/{Guid.CreateVersion7()}{ext}";

		List<TagMedia> tags = [p.Tag1];
		if (p.Tag2 != null) tags.Add(p.Tag2.Value);
		if (p.Tag3 != null) tags.Add(p.Tag3.Value);

		MediaEntity entity = new() {
			Path = name,
			UserId = p.UserId,
			CategoryId = p.CategoryId,
			ContentId = p.ContentId,
			CommentId = p.CommentId,
			ProductId = p.ProductId,
			Tags = tags,
			JsonData = new MediaJson {
				Title = p.Title,
				Description = p.Description
			}
		};

		await SaveMediaAsync(p.File, name);
		await mediaService.Create(entity, ct);

		return new UResponse<MediaResponse?>(entity.MapToResponse(), Usc.Created);
	}

	public async Task<UResponse<IEnumerable<MediaResponse>?>> Read(BaseReadParams<TagMedia> p, CancellationToken ct) {
		UResponse<IEnumerable<MediaResponse>?> result = await mediaService.Read(p)
			.Select(Projections.MediaSelector(new MediaSelectorArgs()))
			.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);

		return result;
	}

	public async Task<UResponse<MediaResponse?>> Update(MediaUpdateParams p, CancellationToken ct) {
		MediaEntity? e = await mediaService.ReadById(p.Id, ct);
		if (e is null) return new UResponse<MediaResponse?>(null, Usc.NotFound);

		if (p.Title != null) e.JsonData.Title = p.Title;
		if (p.Description != null) e.JsonData.Description = p.Description;
		if (p.CategoryId != null) e.CategoryId = p.CategoryId;
		if (p.CommentId != null) e.CommentId = p.CommentId;
		if (p.ContentId != null) e.ContentId = p.ContentId;
		if (p.ProductId != null) e.ProductId = p.ProductId;
		if (p.UserId != null) e.UserId = p.UserId;
		if (p.AddTags != null) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags != null) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));

		await mediaService.Update(e, ct);
		return new UResponse<MediaResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		MediaEntity? e = await mediaService.ReadById(p.Id!.Value, ct);
		if (e is null)
			return new UResponse(Usc.NotFound);

		try {
			string path = Path.Combine(env.WebRootPath, "Media", e.Path);
			if (File.Exists(path))
				File.Delete(path);
		}
		catch {
			// ignored
		}

		await mediaService.Delete(p.Id!.Value, ct);
		return new UResponse();
	}

	private static string GetFolderName(MediaCreateParams p) {
		return p switch {
			{ UserId: not null } => "users",
			{ CategoryId: not null } => "categories",
			{ CommentId: not null } => "comments",
			{ ContentId: not null } => "contents",
			{ ProductId: not null } => "products",
			_ => "generic"
		};
	}

	private async Task SaveMediaAsync(IFormFile file, string relativePath) {
		string fullPath = Path.Combine(env.WebRootPath, "Media", relativePath);
		string? dir = Path.GetDirectoryName(fullPath);

		if (dir != null && !Directory.Exists(dir))
			Directory.CreateDirectory(dir);

		await using FileStream stream = new(fullPath, FileMode.Create);
		await file.CopyToAsync(stream);
	}
}