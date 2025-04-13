namespace SinaMN75U.Data.Responses;

public class CategoryResponse : BaseResponse<TagCategory, CategoryJson> {
	public required string Title { get; set; }

	public IEnumerable<CategoryResponse>? Children { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
}