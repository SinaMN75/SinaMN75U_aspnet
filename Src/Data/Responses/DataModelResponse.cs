namespace SinaMN75U.Data.Responses;

public sealed class DataModelResponse {
	public required CategoryResponse Category { get; set; }
	public required ChatBotResponse ChatBot { get; set; }
	public required CommentResponse Comment { get; set; }
	public required ContentResponse Content { get; set; }
	public required ContractResponse Contract { get; set; }
	public required FollowerFollowingCountResponse FollowerFollowingCount { get; set; }
	public required InvoiceResponse Invoice { get; set; }
	public required MediaResponse Media { get; set; }
	public required ProductResponse Product { get; set; }
	public required TicketResponse Ticket { get; set; }
	public required TxnResponse Txn { get; set; }
	public required UserResponse User { get; set; }
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