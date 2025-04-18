namespace SinaMN75U.Data.Responses;

public class BaseResponse<T, TJ> {
	public required Guid Id { get; set; }
	public DateTime? CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public required List<T> Tags { get; set; }
	public required TJ JsonData { get; set; }
}

public class UResponse<T> : UResponse {
	public UResponse(T result, USC status = USC.Success, string message = "") {
		Result = result;
		Status = status;
		Message = message;
	}

	public T? Result { get; }
}

public class UResponse(USC status = USC.Success, string message = "") {
	public USC Status { get; protected set; } = status;
	public int? PageSize { get; set; }
	public int? PageCount { get; set; }
	public int? TotalCount { get; set; }
	public string Message { get; set; } = message;

	public IResult ToResult() => TypedResults.Json(this, statusCode: Status.Value());
}

public static class UResponseExtensions {
	public static async Task<UResponse<IEnumerable<T>?>> ToPaginatedResponse<T>(this IQueryable<T> query, int pageNumber, int pageSize, CancellationToken ct) {
		if (pageNumber < 1) pageNumber = 1;
		if (pageSize < 1) pageSize = 10;

		int totalCount = await query.CountAsync(ct);
		List<T> items = await query
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(ct);

		return new UResponse<IEnumerable<T>?>(items) {
			TotalCount = totalCount,
			PageCount = (int)Math.Ceiling(totalCount / (double)pageSize),
			PageSize = pageSize
		};
	}
}