namespace SinaMN75U.Data.Media;

public interface IMediaService {
	Task<MediaEntity?> Create(MediaEntity entity, CancellationToken ct);
	IQueryable<MediaEntity> Read(BaseReadParams<TagMedia> p);
	Task<MediaEntity?> ReadById(Guid id, CancellationToken ct);
	Task<MediaEntity?> Update(MediaEntity entity, CancellationToken ct);
	Task<bool> Delete(Guid id, CancellationToken ct);
}

public class MediaService(DbContext db) : IMediaService {
	public async Task<MediaEntity?> Create(MediaEntity entity, CancellationToken ct) {
		EntityEntry<MediaEntity> entry = await db.Set<MediaEntity>().AddAsync(entity, ct);
		await db.SaveChangesAsync(ct);
		return entry.Entity;
	}

	public async Task<MediaEntity?> ReadById(Guid id, CancellationToken ct) {
		return await db.Set<MediaEntity>().FindAsync([id], ct);
	}

	public IQueryable<MediaEntity> Read(BaseReadParams<TagMedia> p) {
		IQueryable<MediaEntity> q = db.Set<MediaEntity>().OrderByDescending(x => x.Id);

		if (p.OrderByCreatedAt) q = q.OrderBy(x => x.CreatedAt);
		if (p.OrderByCreatedAtDesc) q = q.OrderByDescending(x => x.CreatedAt);
		if (p.OrderByUpdatedAt) q = q.OrderBy(x => x.UpdatedAt);
		if (p.OrderByUpdatedAtDesc) q = q.OrderByDescending(x => x.UpdatedAt);

		if (p.ToCreatedAt.HasValue) q = q.Where(x => x.CreatedAt <= p.ToCreatedAt.Value);
		if (p.FromCreatedAt.HasValue) q = q.Where(x => x.CreatedAt >= p.FromCreatedAt.Value);
		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags.Any(tag => p.Tags!.Contains(tag)));
		if (p.Ids.IsNotNullOrEmpty()) q = q.WhereIn(x => x.Id, p.Ids);

		return q;
	}

	public async Task<MediaEntity?> Update(MediaEntity entity, CancellationToken ct) {
		db.Set<MediaEntity>().Update(entity);
		await db.SaveChangesAsync(ct);
		return entity;
	}

	public async Task<bool> Delete(Guid id, CancellationToken ct) {
		int affected = await db.Set<MediaEntity>().Where(x => x.Id == id).ExecuteDeleteAsync(ct);
		return affected > 0;
	}
}