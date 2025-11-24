namespace SinaMN75U.Data.Responses;

public sealed class LoginResponse {
	public required string Token { get; set; }
	public required string RefreshToken { get; set; }
	public required string Expires { get; set; }
	public required UserEntity User { get; set; }
}