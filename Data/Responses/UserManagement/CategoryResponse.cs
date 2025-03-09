namespace SinaMN75U.Data.Responses.UserManagement;

public class CategoryResponse {
	public required string Title { get; set; }
	public string? TitleTr1 { get; set; }
	public string? TitleTr2 { get; set; }

	public required List<TagCategory> Tags { get; set; }
	public IEnumerable<CategoryResponse>? Children { get; set; }
	public IEnumerable<UserResponse>? Users { get; set; }

	public string? Subtitle { get; set; }
}