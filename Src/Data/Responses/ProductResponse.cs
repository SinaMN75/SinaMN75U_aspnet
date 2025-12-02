namespace SinaMN75U.Data.Responses;

public class ProductResponse : BaseResponse<TagProduct, ProductJson> {
	public required string Title { get; set; }
	public string? Code { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Slug { get; set; }
	public string? Type { get; set; }
	public string? Content { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public double? Deposit { get; set; }
	public double? Rent { get; set; }
	public int Stock { get; set; }
	public int Point { get; set; }
	public int Order { get; set; }
	
	public ProductResponse? Parent { get; set; }
	public Guid? ParentId { get; set; }

	public UserResponse User { get; set; } = null!;
	public Guid UserId { get; set; }

	public int? CommentCount { get; set; }
	public bool? IsFollowing { get; set; }
	public int? VisitCount { get; set; }
	public int? ChildrenCount { get; set; }
}