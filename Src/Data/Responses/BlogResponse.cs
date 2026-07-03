namespace SinaMN75U.Data.Responses;

public sealed class BlogResponse : BaseResponse<TagBlog, BlogJson> {
	public required string Title { get; set; }
	public string? Subtitle { get; set; }
	public string? Slug { get; set; }
	public string? Content { get; set; }
	public int ViewCount { get; set; }
	public DateTime? PublishedAt { get; set; }

	public IEnumerable<MediaResponse>? Media { get; set; }
	public IEnumerable<CategoryResponse>? Categories { get; set; }
	public IEnumerable<CommentResponse>? Comments { get; set; }
	public int? CommentCount { get; set; }
}
