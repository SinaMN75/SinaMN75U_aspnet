namespace SinaMN75U.Data.Responses;

public class CommentResponse : BaseResponse<TagComment> {
	public required double Score { get; set; }
	public required string Description { get; set; }
	public required CommentJson Json { get; set; }
	
	public IEnumerable<CommentResponse>? Children { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
}