namespace SinaMN75U.Data.Responses;

public sealed class CommentResponse : BaseResponse<TagComment, CommentJson> {
	public required decimal Score { get; set; }
	public required string Description { get; set; }

	public Guid? ParentId { get; set; }
	public CommentResponse? Parent { get; set; }

	public UserResponse? Creator { get; set; }
	public required Guid CreatorId { get; set; }

	public UserResponse? User { get; set; }
	public Guid? UserId { get; set; }

	public ProductResponse? Product { get; set; }
	public Guid? ProductId { get; set; }
	
	public CategoryResponse? Category { get; set; }
	public Guid? CategoryId { get; set; }
	
	public IEnumerable<CommentResponse>? Children { get; set; }

	public IEnumerable<MediaResponse>? Media { get; set; }
}

