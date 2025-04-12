namespace SinaMN75U.Data.Responses;

public class ContentResponse : BaseResponse<int> {
	public required ContentJson Json { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
}