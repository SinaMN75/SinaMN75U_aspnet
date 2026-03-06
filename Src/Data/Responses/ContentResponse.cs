namespace SinaMN75U.Data.Responses;

public sealed class ContentResponse : BaseResponse<TagContent, ContentJson> {
	public IEnumerable<MediaResponse>? Media { get; set; }
}