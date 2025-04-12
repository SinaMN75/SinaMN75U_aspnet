namespace SinaMN75U.Data.Responses;

public class MediaResponse: BaseResponse<TagMedia> {
	public required string Path { get; set; }
	public required MediaJson Json { get; set; }

}