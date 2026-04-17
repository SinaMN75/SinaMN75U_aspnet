namespace SinaMN75U.Constants;

public class JwtClaimData {
	public required Guid Id { get; set; }
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? FullName { get; set; }
	public required DateTime? Expiration { get; set; }
	public bool IsExpired => Expiration.HasValue && Expiration.Value < DateTime.UtcNow;
	public bool IsAdmin => Tags.Contains(TagUser.SuperAdmin) || Tags.Contains(TagUser.SystemUser) || Tags.Contains(TagUser.SystemAdmin);
	public required IEnumerable<TagUser> Tags { get; set; }
}