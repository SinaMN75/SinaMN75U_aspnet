namespace SinaMN75U.Data.Params;

public class IdParams {
	[Required]
	public required Guid Id { get; set; }
}

public class IdTitleParams {
	public int? Id { get; set; }
	public string? Title { get; set; }
}

public class BaseFilterParams {
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
	public DateTime? FromDate { get; set; }
}