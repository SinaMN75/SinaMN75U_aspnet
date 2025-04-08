namespace SinaMN75U.Data.Responses;

public class ProductResponse: BaseResponse<TagProduct> {
	public required string Title { get; set; }
	public required string Code { get; set; }
	public string? Subtitle { get; set; }
	public string? Description { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public int? Stock { get; set; }
	public double? Price { get; set; }
	
	public Guid? ParentId { get; set; }

	public string? Details { get; set; }
	
	public required List<VisitCount> VisitCounts { get; set; }
	public List<Guid>? RelatedProducts { get; set; }

	public UserResponse? User { get; set; }

	public IEnumerable<ProductResponse>? Children { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
	public IEnumerable<CategoryResponse>? Categories { get; set; }
}