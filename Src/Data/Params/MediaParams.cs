namespace SinaMN75U.Data.Params;

public sealed class MediaCreateParams: BaseParams {
	public Guid? CreatorId { get; set; }
	public IFormFile File { get; set; } = null!;
	public TagMedia Tag1 { get; set; }
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

public sealed class MediaUpdateParams : BaseUpdateParams<TagMedia> {
	public string? Title { get; set; }
	public string? Description { get; set; }
	public Guid? UserId { get; set; }
	public Guid? ContentId { get; set; }
	public Guid? CommentId { get; set; }
	public Guid? CategoryId { get; set; }
	public Guid? ProductId { get; set; }
}