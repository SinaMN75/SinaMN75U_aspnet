namespace SinaMN75U.Data;

public class AllData {
	public required CategoryCreateParams CategoryCreateParams { get; set; }
	public required CategoryUpdateParams CategoryUpdateParams { get; set; }
	public required CategoryReadParams CategoryReadParams { get; set; }
	public required ContentCreateParams CommentCreateParams { get; set; }
	public required ContentUpdateParams ContentUpdateParams { get; set; }
	public required ContentReadParams ContentReadParams { get; set; }
	public required MediaCreateParams MediaCreateParams { get; set; }
	public required ProductCreateParams ProductCreateParams { get; set; }
	public required UserCreateParams UserCreateParams { get; set; }
	public required UserReadParams UserReadParams { get; set; }
	public required UserUpdateParams UserUpdateParams { get; set; }
	public required CategoryResponse CategoryResponse { get; set; }
	public required CommentResponse CommentResponse { get; set; }
	public required ContentResponse ContentResponse { get; set; }
	public required ExamResponse ExamResponse { get; set; }
	public required MediaResponse MediaResponse { get; set; }
	public required ProductResponse ProductResponse { get; set; }
	public required UserResponse UserResponse { get; set; }
	public required LoginResponse Type { get; set; }
}