namespace SinaMN75U.Data.Responses;

public class ProductResponse : BaseResponse<TagProduct, ProductJson> {
	public required string Title { get; set; }
	public required string Code { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public string? Slug { get; set; }
	public string? Type { get; set; }
	public string? Content { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int? Stock { get; set; }
	public double? Price { get; set; }

	public Guid? ParentId { get; set; }

	public UserResponse? User { get; set; }

	public IEnumerable<ProductResponse>? Children { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
	public IEnumerable<CategoryResponse>? Categories { get; set; }
}