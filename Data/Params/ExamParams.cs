namespace SinaMN75U.Data.Params;

public class ExamCreateParams : BaseCreateParams<TagExam> {
	public required string Title { get; set; }
	public required string Description { get; set; }
	public required List<QuestionJson> Questions { get; set; }
	public required List<ExamScoreDetail> ScoreDetails { get; set; }
	
	public required Guid CategoryId { get; set; }
}

public class ExamReadParams : BaseReadParams<TagExam> {
	public Guid? CategoryId { get; set; }
}

public class SubmitAnswersParams: BaseParams {
	public required List<UserAnswerResultJson> Answers { get; set; }
	public required Guid UserId { get; set; }
	public required Guid ExamId { get; set; }
}