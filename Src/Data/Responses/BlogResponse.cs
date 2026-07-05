namespace SinaMN75U.Data.Responses;

public sealed class BlogResponse : BaseResponse<TagBlog, BlogJson> {
	public required string Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Code { get; set; }
	public string? Description { get; set; }
	public string? Slug { get; set; }
	public string? Type { get; set; }
	public string? Content { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }

	public Guid? ParentId { get; set; }
	public IEnumerable<BlogResponse>? Children { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
	public IEnumerable<CategoryResponse>? Categories { get; set; }
	public IEnumerable<CommentResponse>? Comments { get; set; }
	public int? CommentCount { get; set; }
	public int? ChildrenCount { get; set; }
}
