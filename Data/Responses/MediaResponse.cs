namespace SinaMN75U.Data.Responses;

public class MediaResponse: BaseResponse<TagMedia, MediaJson> {
	public required string Path { get; set; }
}