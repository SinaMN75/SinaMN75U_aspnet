namespace SinaMN75U.Data.Responses;

[Table("Contents")]
public class ContentResponse : BaseResponse<TagContent, ContentJson> {
	public ICollection<MediaResponse> Media { get; set; } = [];
}
