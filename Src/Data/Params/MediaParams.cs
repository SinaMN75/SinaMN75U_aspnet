namespace SinaMN75U.Data.Params;

public sealed class MediaCreateParams : BaseCreateParams<TagMedia> {
	public required IFormFile File { get; set; }
	public Guid? UserId { get; set; }
	public Guid? ContentId { get; set; }
	public Guid? CommentId { get; set; }
	public Guid? CategoryId { get; set; }
	public Guid? ProductId { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
}

public sealed class MediaUpdateParams : BaseParams {
	public required Guid Id { get; set; }
	public IEnumerable<TagMedia>? AddTags { get; set; }
	public IEnumerable<TagMedia>? RemoveTags { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public Guid? UserId { get; set; }
	public Guid? ContentId { get; set; }
	public Guid? CommentId { get; set; }
	public Guid? CategoryId { get; set; }
	public Guid? ProductId { get; set; }
}