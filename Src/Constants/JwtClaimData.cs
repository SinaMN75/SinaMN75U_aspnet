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
	public bool IsExpired => Expiration.HasValue && (Expiration.Value.Kind == DateTimeKind.Local ? Expiration.Value.ToUniversalTime() : Expiration.Value) < DateTime.UtcNow;
	public required IEnumerable<TagUser> Tags { get; set; }

	public bool IsSystemAdmin => Tags.Contains(TagUser.SystemAdmin);
	public bool IsSuperAdmin => IsSystemAdmin || !Core.App.MultiTenant && Tags.Contains(TagUser.SuperAdmin);
	public bool IsAdmin => IsSuperAdmin || Tags.Contains(TagUser.SystemUser);
	public bool IsSubAdmin => Tags.Contains(TagUser.SubAdmin);
	public int AdminRank => Rank(Tags);

	public bool CanAccess(Guid creatorId, ICollection<Guid> adminUserIds) => IsSuperAdmin || Id == creatorId || adminUserIds.Count == 0 || adminUserIds.Contains(Id);
	public bool CanManage(Guid creatorId, ICollection<Guid> adminUserIds) => IsAdmin || Id == creatorId || adminUserIds.Contains(Id);
	public bool HasPermission(TagUser permission) => IsAdmin || !Core.App.MultiTenant && IsSubAdmin && Tags.Contains(permission);

	public bool CanGrant(TagUser tag) => !IsRoleTag(tag) || AdminRank > RankOf(tag);

	public bool CanManageUser(Guid userId, IEnumerable<TagUser> userTags) => Id == userId || AdminRank > Rank(userTags);

	public static bool IsRoleTag(TagUser tag) => tag is TagUser.SystemAdmin or TagUser.SuperAdmin or TagUser.SystemUser or TagUser.SubAdmin || (int)tag is >= 600 and < 700;

	public static int Rank(IEnumerable<TagUser> tags) {
		IEnumerable<TagUser> tagUsers = tags.ToList();
		return tagUsers.Contains(TagUser.SystemAdmin) ? 3 : tagUsers.Contains(TagUser.SuperAdmin) || tagUsers.Contains(TagUser.SystemUser) ? 2 : tagUsers.Contains(TagUser.SubAdmin) ? 1 : 0;
	}

	private static int RankOf(TagUser tag) => tag switch {
		TagUser.SystemAdmin => int.MaxValue,
		TagUser.SuperAdmin or TagUser.SystemUser => 2,
		_ => 1
	};
}
