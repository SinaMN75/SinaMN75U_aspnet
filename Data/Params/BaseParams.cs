namespace SinaMN75U.Data.Params;

public class IdParams : BaseParams {
	[Required]
	public required Guid Id { get; set; }
}

public class IdListParams : BaseParams {
	[Required]
	public required IEnumerable<Guid> Ids { get; set; }
}

public class IdTitleParams : BaseParams {
	public int? Id { get; set; }
	public string? Title { get; set; }
}

public class BaseReadParams<T> : BaseParams {
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
	public DateTime? FromCreatedAt { get; set; }
	public DateTime? ToCreatedAt { get; set; }
	public bool OrderByCreatedAt { get; set; } = false;
	public bool OrderByCreatedAtDesc { get; set; } = false;
	public bool OrderByUpdatedAt { get; set; } = false;
	public bool OrderByUpdatedAtDesc { get; set; } = false;
	public List<T>? Tags { get; set; }
}

public class BaseUpdateParams<T> : BaseParams {
	[URequired("IdRequired")]
	public required Guid Id { get; set; }

	[UAddRangeIfNotExist("Tags")]
	public IEnumerable<T>? AddTags { get; set; }

	[URemoveMatching("Tags")]
	public IEnumerable<T>? RemoveTags { get; set; }
	
	[UReplaceListAttribute("Tags")]
	public IEnumerable<T>? Tags { get; set; }

}

public class BaseCreateParams<T> : BaseParams {
	[URequired("TagsRequired")]
	[UMinCollectionLength(1, "TagsRequired")]
	public required List<T> Tags { get; set; }
}

public class BaseParams {
	public string ApiKey { get; set; } = null!;
	public string Token { get; set; } = null!;
}