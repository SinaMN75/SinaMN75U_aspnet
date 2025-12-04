namespace SinaMN75U.Data.Responses;

public sealed class CommentResponse : BaseResponse<TagComment, CommentJson> {
	public required double Score { get; set; }
	public required string Description { get; set; }

	public Guid? ParentId { get; set; }
	public CommentResponse? Parent { get; set; }

	public UserResponse? User { get; set; }
	public required Guid UserId { get; set; }

	public UserResponse? TargetUser { get; set; }
	public Guid? TargetUserId { get; set; }

	public ProductResponse? Product { get; set; }
	public Guid? ProductId { get; set; }
	
	public IEnumerable<CommentResponse>? Children { get; set; }

	public IEnumerable<MediaResponse>? Media { get; set; }
}

