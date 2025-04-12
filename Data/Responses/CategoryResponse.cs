namespace SinaMN75U.Data.Responses;

public class CategoryResponse : BaseResponse<TagCategory> {
	public required string Title { get; set; }
	public required CategoryJson Json { get; set; }

	public IEnumerable<CategoryResponse>? Children { get; set; }
	public IEnumerable<MediaResponse>? Media { get; set; }
}