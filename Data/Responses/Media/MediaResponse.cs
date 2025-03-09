namespace SinaMN75U.Data.Responses.Media;

public class MediaResponse {
	public required string Path { get; set; }
	public required List<TagMedia> Tags { get; set; }
	
	public Guid? UserId { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
}