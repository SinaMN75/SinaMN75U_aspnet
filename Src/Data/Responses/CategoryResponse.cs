namespace SinaMN75U.Data.Responses;

public sealed class CategoryResponse : BaseResponse<TagCategory, CategoryJson> {
	public required string Title { get; set; }
	public int? Order { get; set; }
	public string? Code { get; set; }
	
	public Guid? ParentId { get; set; }
	public CategoryResponse? Parent { get; set; }

	public ICollection<CategoryResponse> Children { get; set; } = [];
	public ICollection<MediaResponse> Media { get; set; } = [];
}