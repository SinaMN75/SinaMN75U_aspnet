namespace SinaMN75U.Data.Params;

public sealed class MediaCreateParams {
	public required IFormFile File { get; set; }
	public required TagMedia Tag1 { get; set; }
	public TagMedia? Tag2 { get; set; }
	public TagMedia? Tag3 { get; set; }
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