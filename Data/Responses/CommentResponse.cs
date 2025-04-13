namespace SinaMN75U.Data.Responses;

public class CommentResponse : BaseResponse<TagComment, CommentJson> {
	public required double Score { get; set; }
	public required string Description { get; set; }
	
	public IEnumerable<CommentResponse>? Children { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
}