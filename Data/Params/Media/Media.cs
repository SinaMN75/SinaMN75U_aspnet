namespace SinaMN75U.Data.Params.Media;

public class MediaCreateParams : BaseParams {
	public required IFormFile File { get; set; }
	public required List<TagMedia> Tags { get; set; }
	public Guid? UserId { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
}

public class MediaUpdateParams : BaseParams {
	public required Guid Id { get; set; }
	public IEnumerable<TagMedia>? AddTags { get; set; }
	public IEnumerable<TagMedia>? RemoveTags { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
}