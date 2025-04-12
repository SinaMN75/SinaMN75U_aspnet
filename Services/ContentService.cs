namespace SinaMN75U.Services;

public interface IContentService {
	Task<UResponse<ContentResponse>> Create(ContentCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ContentResponse>?>> Read(ContentReadParams p, CancellationToken ct);
	Task<UResponse<ContentResponse>> Update(ContentUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class ContentService(DbContext db) : IContentService {
	public async Task<UResponse<ContentResponse>> Create(ContentCreateParams p, CancellationToken ct) {
		EntityEntry<ContentEntity> e = await db.AddAsync(new ContentEntity {
			Description = p.Description,
			Title = p.Title,
			SubTitle = p.SubTitle,
			Tags = p.Tags,
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			JsonDetail = new ContentJsonDetail {
				Instagram = p.Instagram
			}
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<ContentResponse>(e.Entity.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<ContentResponse>?>> Read(ContentReadParams p, CancellationToken ct) {
		IQueryable<ContentEntity> q = db.Set<ContentEntity>();
		
		if (p.Tags != null) q = q.Where(u => u.Tags.Any(tag => p.Tags.Contains(tag)));
		
		return await q.Select(x => new ContentResponse {
			Id = x.Id,
			Tags = x.Tags,
			Description = x.Description,
			Title = x.Title,
			SubTitle = x.SubTitle,
			Instagram = x.JsonDetail.Instagram,
			Media = p.ShowMedia ? x.Media!.Select(m => new MediaResponse { Path = m.Path, Id = m.Id, Tags = m.Tags }) : null
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ContentResponse>> Update(ContentUpdateParams p, CancellationToken ct) {
		ContentEntity e = (await db.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title;
		if (p.SubTitle.IsNotNullOrEmpty()) e.SubTitle = p.SubTitle;
		if (p.Description.IsNotNullOrEmpty()) e.Description = p.Description;
		if (p.Instagram.IsNotNullOrEmpty()) e.JsonDetail.Instagram = p.Instagram;

		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));

		db.Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<ContentResponse>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		await db.Set<ContentEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync();
		return new UResponse();
	}
}