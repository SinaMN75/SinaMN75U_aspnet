namespace SinaMN75U.Data.Responses;

public sealed class MediaResponse {
	public required string Path { get; set; }
	public string Url => $"{AppSettings.Instance.BaseUrl}/Media/{Path}";
	public required MediaJson JsonData { get; set; }
	public required IEnumerable<TagMedia> Tags { get; set; }
}