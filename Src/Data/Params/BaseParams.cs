namespace SinaMN75U.Data.Params;

public class BaseParams {
	public string ApiKey { get; set; } = null!;
	public string Token { get; set; } = null!;
	public string Locale { get; set; } = "en";
}

public sealed class IdParams : BaseParams {
	public required Guid? Id { get; set; }
}

public sealed class IdListParams : BaseParams {
	[Required]
	public required IEnumerable<Guid> Ids { get; set; }
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
	public bool OrderByCreatedAt { get; set; }
	public bool OrderByCreatedAtDesc { get; set; }
	public bool OrderByUpdatedAt { get; set; }
	public bool OrderByUpdatedAtDesc { get; set; }
	public ICollection<T>? Tags { get; set; }
	public ICollection<Guid> Ids { get; set; } = [];
}

public class BaseUpdateParams<T> : BaseParams {
	[UValidationRequired("IdRequired")]
	public required Guid Id { get; set; }

	public ICollection<T>? AddTags { get; set; }
	public ICollection<T>? RemoveTags { get; set; }
	public ICollection<T>? Tags { get; set; }
}

public class BaseCreateParams<T> : BaseParams {
	[UValidationRequired("TagsRequired")]
	[UValidationMinCollectionLength(1, "TagsRequired")]
	public required List<T> Tags { get; set; }

	public Guid? Id { get; set; }
}
