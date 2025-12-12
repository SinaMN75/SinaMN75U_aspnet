namespace SinaMN75U.Data.Responses;

public sealed class CategoryResponse : BaseResponse<TagCategory, CategoryJson> {
	public required string Title { get; set; }
	public int? Order { get; set; }
	public string? Code { get; set; }
	
	public Guid? ParentId { get; set; }
	public CategoryResponse? Parent { get; set; }

	public IEnumerable<CategoryResponse>? Children { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
	public IEnumerable<UserResponse>? Users { get; set; }
	public IEnumerable<ProductResponse>? Products { get; set; }
}