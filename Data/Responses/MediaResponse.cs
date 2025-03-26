namespace SinaMN75U.Data.Responses;

public class MediaResponse {
	public required string Path { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }

	public required IEnumerable<TagMedia> Tags { get; set; }
}