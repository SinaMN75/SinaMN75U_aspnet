namespace SinaMN75U.Data.Responses;

public class UserResponse : BaseResponse<TagUser, UserJson> {
	public string? UserName { get; set; } = null!;
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Bio { get; set; }
	public string? Country { get; set; }
	public string? State { get; set; }
	public string? City { get; set; }
	public DateTime? Birthdate { get; set; }
	
	public IEnumerable<CategoryResponse>? Categories { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
	public IEnumerable<ContractResponse>? Contracts { get; set; }
}

public sealed class LoginResponse {
	public required string Token { get; set; }
	public required string RefreshToken { get; set; }
	public required string Expires { get; set; }
	public required UserResponse User { get; set; }
}