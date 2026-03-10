namespace SinaMN75U.Data.Media;

public sealed class MediaResponse {
	public required Guid Id { get; set; }
	public required string Path { get; set; }
	public string Url => $"{Core.App.BaseUrl}/Media/{Path}";
	public required MediaJson JsonData { get; set; }
	public required IEnumerable<TagMedia> Tags { get; set; }
}