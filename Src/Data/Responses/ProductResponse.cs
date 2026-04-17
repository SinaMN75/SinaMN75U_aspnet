namespace SinaMN75U.Data.Responses;

public sealed class ProductResponse : BaseResponse<TagProduct, ProductJson> {
	public required string Title { get; set; }
	public string? Code { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Slug { get; set; }
	public string? Type { get; set; }
	public string? Content { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public decimal? Deposit { get; set; }
	public decimal? Rent { get; set; }
	public int Stock { get; set; }
	public int Point { get; set; }
	public int Order { get; set; }
	
	public Guid? ParentId { get; set; }
	public IEnumerable<CategoryResponse>? Categories { get; set; }
	public IEnumerable<ProductResponse>? Children { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }

	public int? CommentCount { get; set; }
	public bool? IsFollowing { get; set; }
	public int? ChildrenCount { get; set; }
}