namespace SinaMN75U.Data.Params;

public class BaseParams {
	public string ApiKey { get; set; } = null!;
	public string? Token { get; set; }
}

public sealed class IdParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid Id { get; set; }
}

public sealed class IdStringParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public required string Id { get; set; }
}

public sealed class IdParams<T> : BaseParams where T : new() {
	[UValidationRequired("idIsRequired")]
	public Guid Id { get; set; }

	public T SelectorArgs { get; set; } = new();
}

public sealed class IdListParams : BaseParams {
	[UValidationMinCollectionLength(1, "idIsRequired")]
	public ICollection<Guid> Ids { get; set; } = null!;
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
	public ICollection<T>? Tags { get; set; }
	public ICollection<Guid> Ids { get; set; } = [];
	public Guid? CreatorId { get; set; }

	public TagOrderBy OrderBy { get; set; } = TagOrderBy.CreatedAt;
}

public class BaseUpdateParams<T> : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid Id { get; set; }

	public string? Detail1 { get; set; }
	public string? Detail2 { get; set; }

	public ICollection<T>? AddTags { get; set; }
	public ICollection<T>? RemoveTags { get; set; }
	public ICollection<T>? Tags { get; set; }

	public ICollection<Guid>? AdminUserIds { get; set; }
	public ICollection<Guid>? AddAdminUserIds { get; set; }
	public ICollection<Guid>? RemoveAdminUserIds { get; set; }
}

public class BaseCreateParams<T> : BaseParams {
	public string Detail1 { get; set; } = "";
	public string Detail2 { get; set; } = "";

	[UValidationRequired("tagsIsRequired"), UValidationMinCollectionLength(1, "tagsIsRequired")]
	public ICollection<T> Tags { get; set; } = [];

	public Guid? Id { get; set; }
	public Guid? CreatorId { get; set; }

	public ICollection<Guid>? AdminUserIds { get; set; }
}