namespace SinaMN75U.Data.Responses;

public sealed class MediaResponse {
	public required string Path { get; set; }
	public string Url => $"{Server.BaseUrl}/Media/{Path}";
	public required MediaJson JsonData { get; set; }
	public required ICollection<TagMedia> Tags { get; set; }
}