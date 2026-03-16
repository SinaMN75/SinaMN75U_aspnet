namespace SinaMN75U.Data.ServiceResponses;

public record ErrorServiceResponse(Usc StatusCode, string ErrorCode);

public class BaseServiceResponse<T, TJ> where T : Enum where TJ : class {
	public Guid Id { get; set; } = Guid.CreateVersion7();
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? DeletedAt { get; set; }
	public required TJ JsonData { get; set; }
	public required ICollection<T> Tags { get; set; }
}

public class PaginatedServiceResponse<T> {
	public required int PageNumber { get; set; }
	public required int PageSize { get; set; }
	public required int TotalCount { get; set; }
	public required int PageCount { get; set; }
	public required T Items { get; set; }

	public UResponse<T> ToUResponse(string message = "") => new(Items) {
		TotalCount = TotalCount,
		PageCount = PageCount,
		PageSize = PageSize,
		Message = message
	};
}