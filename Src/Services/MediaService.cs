namespace SinaMN75U.Services;

public interface IMediaService {
	Task<UResponse<MediaEntity?>> Create(MediaCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<MediaEntity>?>> Read(BaseReadParams<TagMedia> p, CancellationToken ct);
	Task<UResponse<MediaEntity?>> Update(MediaUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> DeleteRange(IEnumerable<Guid> ids, CancellationToken ct);

	Task<List<MediaEntity>?> ReadEntity(BaseReadParams<TagMedia> p, CancellationToken ct);
}

public class MediaService(IWebHostEnvironment env, DbContext db) : IMediaService {
	public async Task<UResponse<MediaEntity?>> Create(MediaCreateParams p, CancellationToken ct) {
		IEnumerable<string> allowedExtensions = [".png", ".gif", ".jpg", ".jpeg", ".svg", ".webp", ".mp4", ".mov", ".mp3", ".pdf", ".aac", ".apk", ".zip", ".rar", ".mkv"];
		if (!allowedExtensions.Contains(Path.GetExtension(p.File.FileName.ToLower())))
			return new UResponse<MediaEntity?>(null, Usc.BadRequest);

		string folderName;
		if (p.UserId != null) folderName = "users";
		else if (p.CategoryId != null) folderName = "categories";
		else if (p.CommentId != null) folderName = "comments";
		else if (p.ContentId != null) folderName = "contents";
		else if (p.ProductId != null) folderName = "products";
		else folderName = "generic";

		string name = $"{folderName}/{Guid.CreateVersion7() + Path.GetExtension(p.File.FileName)}";
		MediaEntity e = new() {
			Path = name,
			UserId = p.UserId,
			CategoryId = p.CategoryId,
			ContentId = p.ContentId,
			CommentId = p.CommentId,
			ProductId = p.ProductId,
			Tags = p.Tags,
			JsonData = new MediaJson {
				Title = p.Title,
				Description = p.Description
			}
		};
		await SaveMedia(p.File, name);
		await db.Set<MediaEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<MediaEntity?>(e, message: Path.Combine(env.WebRootPath, "Media"));
	}

	public async Task<UResponse<IEnumerable<MediaEntity>?>> Read(BaseReadParams<TagMedia> p, CancellationToken ct) {
		IQueryable<MediaEntity> q = db.Set<MediaEntity>().OrderByDescending(x => x.Id);

		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags.Any(tag => p.Tags!.Contains(tag)));

		return await q.Select(x => new MediaEntity {
			Id = x.Id,
			JsonData = x.JsonData,
			Tags = x.Tags,
			Path = x.Path,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<MediaEntity?>> Update(MediaUpdateParams p, CancellationToken ct) {
		MediaEntity? e = await db.Set<MediaEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<MediaEntity?>(null, Usc.BadRequest);
		if (p.Title != null) e.JsonData.Title = p.Title;
		if (p.Description != null) e.JsonData.Description = p.Description;
		if (p.CategoryId != null) e.CategoryId = p.CategoryId;
		if (p.CommentId != null) e.CommentId = p.CommentId;
		if (p.ContentId != null) e.ContentId = p.ContentId;
		if (p.ProductId != null) e.ProductId = p.ProductId;
		if (p.UserId != null) e.UserId = p.UserId;
		if (p.AddTags != null) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags != null) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));

		db.Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<MediaEntity?>(e);
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		MediaEntity media = (await db.Set<MediaEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;

		try {
			File.Delete(Path.Combine(env.WebRootPath, "Media", media.Path));
		}
		catch (Exception) {
			// ignored
		}

		db.Set<MediaEntity>().Remove(media);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteRange(IEnumerable<Guid> ids, CancellationToken ct) {
		await db.Set<MediaEntity>().Where(x => ids.Contains(x.Id)).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<List<MediaEntity>?> ReadEntity(BaseReadParams<TagMedia> p, CancellationToken ct) {
		IQueryable<MediaEntity> q = db.Set<MediaEntity>().AsTracking().OrderByDescending(x => x.Id);

		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags.Any(tag => p.Tags!.Contains(tag)));
		if (p.Ids.IsNotNullOrEmpty()) q = q.Where(x => p.Ids.Contains(x.Id));

		return await q.ToListAsync(ct);
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