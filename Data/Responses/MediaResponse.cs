namespace SinaMN75U.Data.Responses;

public class MediaResponse: BaseResponse<TagMedia> {
	public required string Path { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
}