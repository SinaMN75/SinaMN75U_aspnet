namespace SinaMN75U.Data.Responses;

// public class UserResponse : BaseResponse<TagUser, UserJson> {
// 	public required string UserName { get; set; }
// 	public string? PhoneNumber { get; set; }
// 	public string? Email { get; set; }
// 	public string? FirstName { get; set; }
// 	public string? LastName { get; set; }
// 	public string? Country { get; set; }
// 	public string? State { get; set; }
// 	public string? City { get; set; }
// 	public string? Bio { get; set; }
// 	public DateTime? Birthdate { get; set; }
// 	public IEnumerable<UserResponse>? Children { get; set; }
// 	public IEnumerable<CategoryEntity>? Categories { get; set; }
// 	public IEnumerable<MediaEntity>? Media { get; set; }
// }

public class LoginResponse {
	public required string Token { get; set; }
	public required string RefreshToken { get; set; }
	public required string Expires { get; set; }
	public required UserEntity User { get; set; }
}