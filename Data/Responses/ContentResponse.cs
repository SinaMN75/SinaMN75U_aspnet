namespace SinaMN75U.Data.Responses;

public class ContentResponse : BaseResponse<int, ContentJson> {
	public IEnumerable<MediaResponse>? Media { get; set; }
}