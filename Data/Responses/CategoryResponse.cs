namespace SinaMN75U.Data.Responses;

public class CategoryResponse {
	public required Guid Id { get; set; }
	public required string Title { get; set; }
	public string? Subtitle { get; set; }

	public required IEnumerable<TagCategory> Tags { get; set; }
	public IEnumerable<CategoryResponse>? Children { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
}