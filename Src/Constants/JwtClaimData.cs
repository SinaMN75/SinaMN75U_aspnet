namespace SinaMN75U.Constants;

public class JwtClaimData {
	public required Guid Id { get; set; }
	public string? Email { get; set; }
	public string? UserName { get; set; }
	public string? PhoneNumber { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? FullName { get; set; }
	public string? NationalCode { get; set; }
	public required DateTime? Expiration { get; set; }
	public bool IsExpired => Expiration.HasValue && Expiration.Value < DateTime.UtcNow;
	public bool IsAdmin => Tags.Contains(TagUser.SuperAdmin) || Tags.Contains(TagUser.SystemUser) || Tags.Contains(TagUser.SystemAdmin);
	public bool IsSuperAdmin => Tags.Contains(TagUser.SuperAdmin);

	/// <summary>
	/// SubAdmin is a restricted admin-panel role: it can only manage entities it's scoped to
	/// (via CanManage) AND only perform actions it's been explicitly granted via permission tags.
	/// </summary>
	public bool IsSubAdmin => Tags.Contains(TagUser.SubAdmin);

	public required IEnumerable<TagUser> Tags { get; set; }
	public bool CanAccess(Guid creatorId, ICollection<Guid> adminUserIds) => IsSuperAdmin || Id == creatorId || adminUserIds.Count == 0 || adminUserIds.Contains(Id);
	public bool CanManage(Guid creatorId, ICollection<Guid> adminUserIds) => IsAdmin || Id == creatorId || adminUserIds.Contains(Id);

	/// <summary>
	/// Gate for sensitive actions (delete, pay, role changes, etc). Full admins (SuperAdmin/SystemAdmin/SystemUser)
	/// always pass. Everyone else (SubAdmin or a plain user scoped in via AdminUserIds) needs the specific
	/// permission tag granted on their user record.
	/// </summary>
	public bool HasPermission(TagUser permission) => IsAdmin || Tags.Contains(permission);
}