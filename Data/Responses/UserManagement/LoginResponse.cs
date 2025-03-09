namespace SinaMN75U.Data.Responses.UserManagement;

public class LoginResponse {
	public required string Token { get; set; }
	public required string RefreshToken { get; set; }
	public required UserResponse User { get; set; }
}