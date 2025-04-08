namespace SinaMN75U.Data.Responses;

public class UserResponse {
	public required Guid Id { get; set; }
	public required string UserName { get; set; }
	public required string PhoneNumber { get; set; }
	public required string? Email { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Bio { get; set; }
	public string? FcmToken { get; set; }
	public DateTime? Birthdate { get; set; }
	public required IEnumerable<TagUser> Tags { get; set; }
	public IEnumerable<CategoryResponse>? Categories { get; set; }
}

public class LoginResponse {
	public required string Token { get; set; }
	public required string RefreshToken { get; set; }
	public required string Expires { get; set; }
	public required UserResponse User { get; set; }
}