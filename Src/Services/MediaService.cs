namespace SinaMN75U.Services;

public interface IMediaService {
	Task<UResponse<Guid?>> Create(MediaCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<MediaResponse>?>> Read(BaseReadParams<TagMedia> p, CancellationToken ct);
	Task<UResponse> Update(MediaUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> DeleteRange(IdListParams p, CancellationToken ct);

	Task<List<MediaEntity>?> ReadEntity(BaseReadParams<TagMedia> p, CancellationToken ct);
}

public class MediaService(
	IWebHostEnvironment env,
	DbContext db,
	ITokenService ts,
	ILocalizationService ls
) : IMediaService {
	private async Task<bool> CanChange(JwtClaimData u, Guid creatorId, Guid? hotelId, Guid? hotelRoomId, Guid? dormId, Guid? dormRoomId, Guid? dormBedId, Guid? contentId, Guid? blogId, CancellationToken ct, Guid? venueId = null, Guid? postId = null) {
		if (u.IsSuperAdmin) return true;
		if (venueId != null) return u.IsAdmin || await db.Set<VenueEntity>().AnyAsync(x => x.Id == venueId && (x.CreatorId == u.Id || x.AdminUserIds.Contains(u.Id)), ct);
		if (postId != null) return await db.Set<PostEntity>().AnyAsync(x => x.Id == postId && x.CreatorId == u.Id, ct);
		(ICollection<Guid> AdminUserIds, TagUser Permission)? place = await db.PlaceOf(hotelId, hotelRoomId, dormId, dormRoomId, dormBedId, ct);
		if (place != null) return u.CanActOnPlace(place.Value.AdminUserIds, place.Value.Permission);
		if (contentId != null) return u.HasPermission(TagUser.PermissionManageContents);
		if (blogId != null) return u.HasPermission(TagUser.PermissionManageContents) || await db.Set<BlogEntity>().AnyAsync(x => x.Id == blogId && x.CreatorId == u.Id, ct);
		return u.Id == creatorId;
	}

	public async Task<UResponse<Guid?>> Create(MediaCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);

		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!await CanChange(userData, userData.Id, p.HotelId, p.HotelRoomId, p.DormId, p.DormRoomId, p.DormBedId, p.ContentId, p.BlogId, ct, p.VenueId, p.PostId)) return new UResponse<Guid?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		
		IEnumerable<string> allowedExtensions = [".png", ".gif", ".jpg", ".jpeg", ".svg", ".webp", ".mp4", ".mov", ".mp3", ".pdf", ".aac", ".apk", ".zip", ".rar", ".mkv"];
		if (!allowedExtensions.Contains(Path.GetExtension(p.File.FileName.ToLower()))) return new UResponse<Guid?>(null, Usc.MediaTypeNotSupported, ls.Get("thisFileTypeIsNotSupported"));

		string folderName;
		if (p.UserId != null) folderName = "users";
		else if (p.CategoryId != null) folderName = "categories";
		else if (p.CommentId != null) folderName = "comments";
		else if (p.ContentId != null) folderName = "contents";
		else if (p.ProductId != null) folderName = "products";
		else if (p.HotelId != null) folderName = "hotels";
		else if (p.HotelRoomId != null) folderName = "hotelRooms";
		else if (p.DormId != null) folderName = "dorms";
		else if (p.DormRoomId != null) folderName = "dormRooms";
		else if (p.DormBedId != null) folderName = "dormBeds";
		else if (p.BlogId != null) folderName = "blogs";
		else if (p.VenueId != null) folderName = "venues";
		else if (p.PostId != null) folderName = "posts";
		else folderName = "generic";

		string name = $"{folderName}/{Guid.CreateVersion7() + Path.GetExtension(p.File.FileName)}";

		ICollection<TagMedia> tags = [p.Tag1];
		if (p.Tag2 != null) tags.Add(p.Tag2.Value);
		if (p.Tag3 != null) tags.Add(p.Tag3.Value);
		MediaEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatorId = userData.IsSuperAdmin ? p.CreatorId ?? userData.Id : userData.Id,
			CreatedAt = DateTime.UtcNow,
			Path = name,
			UserId = p.UserId,
			CategoryId = p.CategoryId,
			ContentId = p.ContentId,
			CommentId = p.CommentId,
			ProductId = p.ProductId,
			HotelId = p.HotelId,
			HotelRoomId = p.HotelRoomId,
			DormId = p.DormId,
			DormRoomId = p.DormRoomId,
			DormBedId = p.DormBedId,
			BlogId = p.BlogId,
			VenueId = p.VenueId,
			PostId = p.PostId,
			Tags = tags,
			JsonData = new MediaJson {
				Detail1 = p.Title ?? "",
				Detail2 = p.Description ?? ""
			}
		};
		await SaveMedia(p.File, name);
		await db.Set<MediaEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id, message: $"{Core.App.BaseUrl}/Media/{name}");
	}

	public async Task<UResponse<IEnumerable<MediaResponse>?>> Read(BaseReadParams<TagMedia> p, CancellationToken ct) {
		IQueryable<MediaEntity> q = db.Set<MediaEntity>().ApplyReadParams(p);

		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags.Any(tag => p.Tags!.Contains(tag)));

		return await q.Select(Projections.MediaSelector()).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(MediaUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		MediaEntity? e = await db.Set<MediaEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("mediaNotFound"));
		if (!await CanChange(userData, e.CreatorId, e.HotelId, e.HotelRoomId, e.DormId, e.DormRoomId, e.DormBedId, e.ContentId, e.BlogId, ct, e.VenueId, e.PostId)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (!await CanChange(userData, e.CreatorId, p.HotelId ?? e.HotelId, p.HotelRoomId ?? e.HotelRoomId, p.DormId ?? e.DormId, p.DormRoomId ?? e.DormRoomId, p.DormBedId ?? e.DormBedId, p.ContentId ?? e.ContentId, p.BlogId ?? e.BlogId, ct, p.VenueId ?? e.VenueId, p.PostId ?? e.PostId)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));
		if (p.Title != null) e.JsonData.Detail1 = p.Title;
		if (p.Description != null) e.JsonData.Detail2 = p.Description;
		if (p.CategoryId != null) e.CategoryId = p.CategoryId;
		if (p.CommentId != null) e.CommentId = p.CommentId;
		if (p.ContentId != null) e.ContentId = p.ContentId;
		if (p.ProductId != null) e.ProductId = p.ProductId;
		if (p.BlogId != null) e.BlogId = p.BlogId;
		if (p.HotelId != null) e.HotelId = p.HotelId;
		if (p.VenueId != null) e.VenueId = p.VenueId;
		if (p.PostId != null) e.PostId = p.PostId;
		if (p.HotelRoomId != null) e.HotelRoomId = p.HotelRoomId;
		if (p.DormId != null) e.DormId = p.DormId;
		if (p.DormRoomId != null) e.DormRoomId = p.DormRoomId;
		if (p.DormBedId != null) e.DormBedId = p.DormBedId;
		if (p.UserId != null) e.UserId = p.UserId;
		if (p.AddTags != null) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags != null) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));

		await db.SaveChangesAsync(ct);
		return new UResponse<MediaResponse?>(new MediaResponse {
			Id = e.Id,
			JsonData = e.JsonData,
			Tags =  e.Tags,
			Path = e.Path
		});
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		MediaEntity? media = await db.Set<MediaEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (media == null) return new UResponse(Usc.NotFound, ls.Get("mediaNotFound"));
		if (!await CanChange(userData, media.CreatorId, media.HotelId, media.HotelRoomId, media.DormId, media.DormRoomId, media.DormBedId, media.ContentId, media.BlogId, ct, media.VenueId, media.PostId)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

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

	public async Task<UResponse> DeleteRange(IdListParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		foreach (MediaEntity m in await db.Set<MediaEntity>().Where(x => p.Ids.Contains(x.Id)).ToListAsync(ct))
			if (!await CanChange(userData, m.CreatorId, m.HotelId, m.HotelRoomId, m.DormId, m.DormRoomId, m.DormBedId, m.ContentId, m.BlogId, ct, m.VenueId, m.PostId)) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		await db.Set<MediaEntity>().Where(x => p.Ids.Contains(x.Id)).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<List<MediaEntity>?> ReadEntity(BaseReadParams<TagMedia> p, CancellationToken ct) {
		IQueryable<MediaEntity> q = db.Set<MediaEntity>().AsTracking().OrderByDescending(x => x.Id);

		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags.Any(tag => p.Tags!.Contains(tag)));
		if (p.Ids.IsNotNullOrEmpty()) q = q.Where(x => p.Ids.Contains(x.Id));

		return await q.ToListAsync(ct);
	}

	private async Task SaveMedia(IFormFile file, string name) {
		string fullPath = Path.Combine(env.WebRootPath, "Media", name);
		string? directory = Path.GetDirectoryName(fullPath);

		if (directory != null && !Directory.Exists(directory)) {
			Directory.CreateDirectory(directory);
		}

		await using FileStream stream = new(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
		await file.CopyToAsync(stream);
	}
}