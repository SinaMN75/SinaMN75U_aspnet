namespace SinaMN75U.Data.Responses;

public class CategoryResponse {
	public required string Title { get; set; }
	public string? TitleTr1 { get; set; }
	public string? TitleTr2 { get; set; }
	public string? Subtitle { get; set; }

	public required IEnumerable<TagCategory> Tags { get; set; }
	public IEnumerable<CategoryResponse>? Children { get; set; }
}