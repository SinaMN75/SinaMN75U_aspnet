namespace SinaMN75U.Data.Params;

public class IdParams : BaseParam {
	[Required]
	public required Guid Id { get; set; }
}

public class IdTitleParams {
	public int? Id { get; set; }
	public string? Title { get; set; }
}

public class BaseFilterParams : BaseParam {
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
	public DateTime? FromDate { get; set; }
}

public class BaseParam {
	[Required]
	public required string ApiKey { get; set; }

	public string? Token { get; set; }
}