namespace SinaMN75U.Data.Params;

public class IdParams : BaseParams {
	[Required]
	public required Guid Id { get; set; }
}

public class IdTitleParams : BaseParams {
	public int? Id { get; set; }
	public string? Title { get; set; }
}

public class BaseReadParams : BaseParams {
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
	public DateTime? FromDate { get; set; }
}

public class BaseParams {
	public string ApiKey { get; set; } = null!;
	public string Token { get; set; } = null!;
}