namespace SinaMN75U.Data.Responses;

public sealed class CommentResponse : BaseResponse<TagComment, CommentJson> {
	public required decimal Score { get; set; }
	public required string Description { get; set; }
	
	public UserResponse? User { get; set; }
	public Guid? UserId { get; set; }

	public Guid? ParentId { get; set; }
	public Guid? ProductId { get; set; }

	public IEnumerable<CommentResponse>? Children { get; set; }

	public IEnumerable<MediaResponse>? Media { get; set; }
}