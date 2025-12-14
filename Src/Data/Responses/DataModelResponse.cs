namespace SinaMN75U.Data.Responses;

public sealed class DataModelResponse {
	public required CategoryResponse UCategoryResponse { get; set; }
	public required ChatBotResponse UChatBotResponse { get; set; }
	public required CommentResponse UCommentResponse { get; set; }
	public required ContentResponse UContentResponse { get; set; }
	public required ContractResponse UContractResponse { get; set; }
	public required FollowerFollowingCountResponse UFollowerFollowingCountResponse { get; set; }
	public required InvoiceResponse UInvoiceResponse { get; set; }
	public required MediaResponse UMediaResponse { get; set; }
	public required ProductResponse UProductResponse { get; set; }
	public required TicketResponse UTicketResponse { get; set; }
	public required TxnResponse UTxnResponse { get; set; }
	public required UserResponse UUserResponse { get; set; }
}

public sealed class ParamsResponse {
	public required RefreshTokenParams RefreshTokenParams { get; set; }
	public required GetMobileVerificationCodeForLoginParams GetMobileVerificationCodeForLoginParams { get; set; }
	public required LoginWithEmailPasswordParams LoginWithEmailPasswordParams { get; set; }
	public required LoginWithUserNamePasswordParams LoginWithUserNamePasswordParams { get; set; }
	public required RegisterParams RegisterParams { get; set; }
	public required VerifyMobileForLoginParams VerifyMobileForLoginParams { get; set; }
	public required CategoryCreateParams CategoryCreateParams { get; set; }
	public required CategoryUpdateParams CategoryUpdateParams { get; set; }
	public required CategoryReadParams CategoryReadParams { get; set; }
	public required CommentCreateParams CommentCreateParams { get; set; }
	public required CommentUpdateParams CommentUpdateParams { get; set; }
	public required CommentReadParams CommentReadParams { get; set; }
	public required ContentCreateParams ContentCreateParams { get; set; }
	public required ContentUpdateParams ContentUpdateParams { get; set; }
	public required ContentReadParams ContentReadParams { get; set; }
	public required ExamCreateParams ExamCreateParams { get; set; }
	public required ExamReadParams ExamReadParams { get; set; }
	public required SubmitAnswersParams SubmitAnswersParams { get; set; }
	public required MediaUpdateParams MediaUpdateParams { get; set; }
	public required ProductCreateParams ProductCreateParams { get; set; }
	public required ProductUpdateParams ProductUpdateParams { get; set; }
	public required ProductReadParams ProductReadParams { get; set; }
	public required UserCreateParams UserCreateParams { get; set; }
	public required UserReadParams UserReadParams { get; set; }
	public required UserUpdateParams UserUpdateParams { get; set; }
}