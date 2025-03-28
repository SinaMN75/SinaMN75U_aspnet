namespace SinaMN75U.Services;

public interface IMediaService {
	Task<UResponse<MediaResponse?>> Create(MediaCreateParams p, CancellationToken ct);
	Task<UResponse<MediaResponse?>> Update(MediaUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> DeleteRange(IEnumerable<Guid> ids, CancellationToken ct);
}

public class MediaService(IWebHostEnvironment env, DbContext db) : IMediaService {
	public async Task<UResponse<MediaResponse?>> Create(MediaCreateParams p, CancellationToken ct) {
		IEnumerable<string> allowedExtensions = [".png", ".gif", ".jpg", ".jpeg", ".svg", ".mp4", ".mov", ".mp3", ".pdf", ".aac", ".apk", ".zip", ".rar", ".mkv"];
		if (!allowedExtensions.Contains(Path.GetExtension(p.File.FileName.ToLower())))
			return new UResponse<MediaResponse?>(null, USC.BadRequest);

		string folderName = "";
		if (p.UserId != null) folderName = "users/";
		string name = $"{folderName}{Guid.NewGuid() + Path.GetExtension(p.File.FileName)}";
		MediaEntity e = new() {
			Id = Guid.CreateVersion7(),
			Path = name,
			UserId = p.UserId,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			JsonDetail = new MediaJsonDetail {
				Title = p.Title,
				Description = p.Description
			}
		};
		await db.Set<MediaEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		await SaveMedia(p.File, name);
		return new UResponse<MediaResponse?>(e.MapToResponse(), message: Path.Combine(env.WebRootPath, "Media"));
	}

	public async Task<UResponse<MediaResponse?>> Update(MediaUpdateParams p, CancellationToken ct) {
		MediaEntity? e = await db.Set<MediaEntity>().FindAsync(p.Id, ct);
		if (e == null) return new UResponse<MediaResponse?>(null, USC.BadRequest);
		if (p.Title != null) e.JsonDetail.Title = p.Title;
		if (p.Description != null) e.JsonDetail.Description = p.Description;
		if (p.AddTags != null) e.Tags.AddRange(p.AddTags);
		if (p.RemoveTags != null) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));

		db.Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<MediaResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		MediaEntity media = (await db.Set<MediaEntity>().FindAsync(p.Id))!;

		try {
			File.Delete(Path.Combine(env.WebRootPath, "Media", media.Path));
		}
		catch (Exception) {
			// ignored
		}

		db.Set<MediaEntity>().Remove(media);
		await db.SaveChangesAsync();
		return new UResponse();
	}

	public async Task<UResponse> DeleteRange(IEnumerable<Guid> ids, CancellationToken ct) {
		await db.Set<MediaEntity>().Where(x => ids.Contains(x.Id)).ExecuteDeleteAsync();
		return new UResponse();
	}

	private async Task SaveMedia(IFormFile file, string name) {
		string webRoot = env.WebRootPath;
		string path = Path.Combine(webRoot, "Media", name);
		string uploadDir = Path.Combine(webRoot, "Media");
		if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
		try {
			File.Delete(path);
		}
		catch (Exception) {
			// ignored
		}

		await using FileStream stream = new(path, FileMode.Create);
		await file.CopyToAsync(stream);
	}
}