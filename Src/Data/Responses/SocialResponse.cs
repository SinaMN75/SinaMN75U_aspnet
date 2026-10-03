namespace SinaMN75U.Data.Responses;

public sealed class PostResponse : BaseResponse<TagPost, PostJson> {
	public string? Text { get; set; }
	public DateTime? ExpiresAt { get; set; }
	public Guid? ParentId { get; set; }
	public int ReplyCount { get; set; }
	public int ReactionCount { get; set; }
	public TagReaction? MyReaction { get; set; }

	/// <summary>The author: public fields only.</summary>
	public UserResponse? User { get; set; }

	public ICollection<MediaResponse>? Media { get; set; }
	public ICollection<PostResponse>? Children { get; set; }
}

public sealed class ReportResponse : BaseResponse<TagReport, ReportJson> {
	public required Guid TargetId { get; set; }
	public string? Reason { get; set; }
}

public sealed class BlockResponse : BaseResponse<TagBlock, BlockJson> {
	public required Guid BlockedUserId { get; set; }
	public UserResponse? BlockedUser { get; set; }
}
