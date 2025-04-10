namespace SinaMN75U.Data.Params;

public class IdParams : BaseParams {
	[Required]
	public required Guid Id { get; set; }
}

public class IdTitleParams : BaseParams {
	public int? Id { get; set; }
	public string? Title { get; set; }
}

public class BaseReadParams<T> : BaseParams {
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
	public DateTime? FromDate { get; set; }
	public List<T>? Tags { get; set; }
}

public class BaseUpdateParams<T> : BaseParams {
	[URequired("IdRequired")]
	public required Guid Id { get; set; }

	public IEnumerable<T>? AddTags { get; set; }
	public IEnumerable<T>? RemoveTags { get; set; }
}

public class BaseParams {
	public string ApiKey { get; set; } = null!;
	public string Token { get; set; } = null!;
}