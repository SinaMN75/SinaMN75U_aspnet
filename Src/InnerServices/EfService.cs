using SinaMN75U.Data.ServiceResponses;

namespace SinaMN75U.InnerServices;

public interface IEfService {
	Task SaveChangesAsync(CancellationToken ct);
	Task<PaginatedServiceResponse<IEnumerable<T>>> QueryPaginatedAsync<T>(IQueryable<T> list, int pageSize, int pageNumber, CancellationToken ct);
}

public class EfService(DbContext db) : IEfService {
	public async Task SaveChangesAsync(CancellationToken ct) {
		await db.SaveChangesAsync(ct);
	}

	public async Task<PaginatedServiceResponse<IEnumerable<T>>> QueryPaginatedAsync<T>(IQueryable<T> list, int pageSize, int pageNumber, CancellationToken ct) {
		int totalCount = await list.CountAsync(ct);
		List<T> items = await list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
		return new PaginatedServiceResponse<IEnumerable<T>> {
			TotalCount = totalCount,
			PageNumber = pageNumber,
			PageSize = pageSize,
			PageCount = (int)Math.Ceiling(totalCount / (decimal)pageSize),
			Items = items,
		};
	}
}