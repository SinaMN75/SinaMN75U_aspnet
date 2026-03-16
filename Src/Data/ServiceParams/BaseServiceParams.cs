namespace SinaMN75U.Data.ServiceParams;

public class IdServiceParams {
	public required Guid? Id { get; set; }
}

public sealed class SoftDeleteServiceParams {
	public required Guid? Id { get; set; }
	public required DateTime? DateTime { get; set; }
}

public sealed class IdListServiceParams {
	public required IEnumerable<Guid> Ids { get; set; }
}

public sealed class IdTitleServiceParams {
	public int? Id { get; set; }
	public string? Title { get; set; }
}

public class BaseReadServiceParams<T> {
	public int PageSize { get; set; } = 100;
	public int PageNumber { get; set; } = 1;
	public DateTime? FromCreatedAt { get; set; }
	public DateTime? ToCreatedAt { get; set; }
	public bool OrderByCreatedAt { get; set; } = true;
	public bool OrderByCreatedAtDesc { get; set; }
	public bool OrderByUpdatedAt { get; set; }
	public bool OrderByUpdatedAtDesc { get; set; }
	public IEnumerable<T>? Tags { get; set; }
	public IEnumerable<Guid> Ids { get; set; } = [];
}

public class BaseUpdateServiceParams<T> {
	public required Guid Id { get; set; }

	public IEnumerable<T>? AddTags { get; set; }
	public IEnumerable<T>? RemoveTags { get; set; }
	public ICollection<T>? Tags { get; set; }
}

public class BaseCreateServiceParams<T> {
	public required ICollection<T> Tags { get; set; }
	public Guid? Id { get; set; }
}