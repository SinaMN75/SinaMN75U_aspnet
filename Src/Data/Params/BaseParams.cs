namespace SinaMN75U.Data.Params;

public class BaseParams {
	public string ApiKey { get; set; } = null!;
	public string Token { get; set; } = null!;
}

public sealed class IdParams : BaseParams {
	[UValidationRequired("IdRequired")]
	public Guid Id { get; set; }
}

public sealed class IdParams<T> : BaseParams where T : new() {
	[UValidationRequired("IdRequired")]
	public Guid Id { get; set; }

	public T SelectorArgs { get; set; } = new();
}

public sealed class IdListParams : BaseParams {
	[UValidationMinCollectionLength(1, "IdRequired")]
	public IEnumerable<Guid> Ids { get; set; } = null!;
}

public sealed class IdTitleParams : BaseParams {
	public int? Id { get; set; }
	public string? Title { get; set; }
}

public class BaseReadParams<T> : BaseParams {
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
	public DateTime? FromCreatedAt { get; set; }
	public DateTime? ToCreatedAt { get; set; }
	public bool OrderByCreatedAt { get; set; } = true;
	public bool OrderByCreatedAtDesc { get; set; }
	public IEnumerable<T>? Tags { get; set; }
	public IEnumerable<Guid> Ids { get; set; } = [];
	public Guid? CreatorId { get; set; }
}

public class BaseUpdateParams<T> : BaseParams {
	[UValidationRequired("IdRequired")]
	public Guid Id { get; set; }

	public string? Detail1 { get; set; }
	public string? Detail2 { get; set; }

	public IEnumerable<T>? AddTags { get; set; }
	public IEnumerable<T>? RemoveTags { get; set; }
	public ICollection<T>? Tags { get; set; }
}

public class BaseCreateParams<T> : BaseParams {
	public string Detail1 { get; set; } = "";
	public string Detail2 { get; set; } = "";
	
	[UValidationRequired("TagsRequired"), UValidationMinCollectionLength(1, "TagsRequired")]
	public ICollection<T> Tags { get; set; } = [];

	public Guid? Id { get; set; }
	public Guid? CreatorId { get; set; }
}