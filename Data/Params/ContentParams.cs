namespace SinaMN75U.Data.Params;


public class ContentCreateParams: BaseParams {
	public required string Title { get; set; }
	public required string Description { get; set; }
	public required string SubTitle { get; set; }
	public string? Instagram { get; set; }
	public required List<TagContent> Tags { get; set; }
}

public class ContentUpdateParams: BaseParams {
	public required Guid Id { get; set; }
	public string? Title { get; set; }
	public string? SubTitle { get; set; }
	public string? Description { get; set; }
	public string? Instagram { get; set; }
	public List<TagContent>? Tags { get; set; }
}