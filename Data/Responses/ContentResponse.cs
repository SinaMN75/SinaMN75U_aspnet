namespace SinaMN75U.Data.Responses;

public class ContentResponse : BaseResponse<int> {
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
}