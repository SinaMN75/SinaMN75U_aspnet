namespace SinaMN75U.Data.Entities;

public class BaseEntity<T> where T : Enum {
	[Key]
	public required Guid Id { get; set; }

	[Required]
	public required DateTime CreatedAt { get; set; }

	[Required]
	public required ICollection<T> Tags { get; set; }

	public required Guid CreatorId { get; set; }
	public UserEntity Creator { get; set; } = null!;

	/// <summary>
	/// Scopes visibility/management of this entity to specific users.
	/// Empty list = visible/manageable by everyone (subject to normal rules).
	/// Non-empty list = only users whose Id is in this list (plus the creator and super admins) can see/manage it.
	/// </summary>
	public ICollection<Guid> AdminUserIds { get; set; } = [];
}

public class BaseEntity<T, TJ> : BaseEntity<T> where T : Enum where TJ : BaseJson {
	[Required]
	public required TJ JsonData { get; set; }
}


public class BaseJson {
	public string Detail1 { get; set; } = "";
	public string Detail2 { get; set; } = "";
}