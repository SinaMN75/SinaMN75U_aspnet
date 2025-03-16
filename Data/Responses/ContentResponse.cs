namespace SinaMN75U.Data.Responses;

public class ContentResponse {
	public required string Title { get; set; }
	public required string SubTitle { get; set; }
	public required string Description { get; set; }
	public string? Instagram { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
	public required IEnumerable<int> Tags { get; set; }
}