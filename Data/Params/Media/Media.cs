namespace SinaMN75U.Data.Params.Media;

public class MediaCreateParams : BaseParam {
	public required IFormFile File { get; set; }
	public required List<TagMedia> Tags { get; set; }
	public Guid? UserId { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
}

public class MediaUpdateParams : BaseParam {
	public required Guid Id { get; set; }
	public List<TagMedia>? AddTags { get; set; }
	public List<TagMedia>? RemoveTags { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
}