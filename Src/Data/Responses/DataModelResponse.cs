namespace SinaMN75U.Data.Responses;

public class DataModelResponse {
	public required CategoryEntity Category { get; set; }
	public required CommentEntity Comment { get; set; }
	public required ContentEntity Content { get; set; }
	public required ExamEntity Exam { get; set; }
	public required MediaEntity Media { get; set; }
	public required ProductEntity Product { get; set; }
	public required UserEntity User { get; set; }
}