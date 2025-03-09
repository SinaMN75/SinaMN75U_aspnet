namespace SinaMN75U.Data.Responses.UserManagement;

public class UserResponse {
	public required Guid Id { get; set; }
	public required string UserName { get; set; }
	public required string PhoneNumber { get; set; }
	public required string? Email { get; set; }
	public string? FullName { get; set; }
	public string? Bio { get; set; }
	public string? FcmToken { get; set; }
	public DateTime? Birthdate { get; set; }
	public required List<TagUser> Tags { get; set; }
	public IEnumerable<CategoryResponse>? Categories { get; set; }
}