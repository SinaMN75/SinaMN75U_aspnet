using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SinaMN75U.Services;

public interface IContentService {
	Task<UResponse<ContentEntity>> Create(ContentCreateParams p, CancellationToken ct);
	UResponse<IQueryable<ContentEntity>> Read(BaseParams p, CancellationToken ct);
	Task<UResponse<ContentEntity>> Update(ContentUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class ContentService(DbContext context) : IContentService {
	public async Task<UResponse<ContentEntity>> Create(ContentCreateParams p, CancellationToken ct) {
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
		return new UResponse<ContentEntity>(e.Entity);
	}

	public UResponse<IQueryable<ContentEntity>> Read(BaseParams p, CancellationToken ct) => new(context.Set<ContentEntity>()
		.Select(x => new ContentEntity {
			Id = x.Id,
			Title = x.Title,
			SubTitle = x.SubTitle,
			Description = x.Description,
			Tags = x.Tags,
			JsonDetail = x.JsonDetail,
			Media = x.Media!.Select(y => new MediaEntity {
				Id = y.Id,
				JsonDetail = y.JsonDetail,
				Tags = y.Tags,
				Path = y.Path,
				CreatedAt = default,
				UpdatedAt = default
			}),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		}).AsNoTracking());

	public async Task<UResponse<ContentEntity>> Update(ContentUpdateParams p, CancellationToken ct) {
		ContentEntity e = (await context.Set<ContentEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (p.Title is not null) e.Title = p.Title;
		if (p.SubTitle is not null) e.SubTitle = p.SubTitle;
		if (p.Description is not null) e.Description = p.Description;
		if (p.Tags is not null) e.Tags = p.Tags;
		if (p.Instagram is not null) e.JsonDetail.Instagram = p.Instagram;

		context.Update(e);
		await context.SaveChangesAsync(ct);
		return new UResponse<ContentEntity>(e);
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		
		ContentEntity e = (await context.Set<ContentEntity>().Include(x => x.Media).FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		context.Set<ContentEntity>().Remove(e);
		await context.SaveChangesAsync(ct);
		return new UResponse();
	}
}