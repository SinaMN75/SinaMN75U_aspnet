using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SinaMN75U.Services;

public interface IContentService {
	Task<UResponse<ContentResponse>> Create(ContentCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ContentResponse>>> Filter(ContentFilterParams p, CancellationToken ct);
	Task<UResponse<ContentResponse>> Update(ContentUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class ContentService(DbContext context) : IContentService {
	public async Task<UResponse<ContentResponse>> Create(ContentCreateParams p, CancellationToken ct) {
		EntityEntry<ContentEntity> e = await context.AddAsync(new ContentEntity {
			Description = p.Description,
			Title = p.Title,
			SubTitle = p.SubTitle,
			Tags = p.Tags,
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			JsonDetail = new ContentJsonDetail {
				Instagram = p.Instagram,
			}
		}, ct);
		await context.SaveChangesAsync(ct);
		return new UResponse<ContentResponse>(e.Entity.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<ContentResponse>>> Filter(ContentFilterParams p, CancellationToken ct) {
		IQueryable<ContentEntity> q = context.Set<ContentEntity>();
		if (p.Tags != null) q = q.Where(u => u.Tags.Any(tag => p.Tags.Contains(tag)));
		return new UResponse<IEnumerable<ContentResponse>>(await q.Select(x => x.MapToResponse(p.ShowMedia)).ToListAsync());
	}

	public async Task<UResponse<ContentResponse>> Update(ContentUpdateParams p, CancellationToken ct) {
		ContentEntity e = (await context.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (p.Title is not null) e.Title = p.Title;
		if (p.SubTitle is not null) e.SubTitle = p.SubTitle;
		if (p.Description is not null) e.Description = p.Description;
		if (p.Instagram is not null) e.JsonDetail.Instagram = p.Instagram;

		if (p.AddTags != null) e.Tags.AddRange(p.AddTags);
		if (p.RemoveTags != null) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));


		context.Update(e);
		await context.SaveChangesAsync(ct);
		return new UResponse<ContentResponse>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		ContentEntity e = (await context.Set<ContentEntity>()
			.Include(x => x.Media)
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		context.Set<ContentEntity>().Remove(e);
		await context.SaveChangesAsync(ct);
		return new UResponse();
	}
}