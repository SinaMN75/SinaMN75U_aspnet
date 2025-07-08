namespace SinaMN75U.Data.Params;

public class ExamCreateParams : BaseCreateParams<TagExam> {
	[UValidationRequired("TitleRequired")]
	public required string Title { get; set; }
	
	[UValidationRequired("DescriptionRequired")]
	public required string Description { get; set; }
	
	public required List<QuestionJson> Questions { get; set; }
	public required List<ExamScoreDetail> ScoreDetails { get; set; }

	public required Guid CategoryId { get; set; }
}

public class ExamReadParams : BaseReadParams<TagExam> {
	public Guid? CategoryId { get; set; }
}

public class SubmitAnswersParams : BaseParams {
	public required List<UserAnswerResultJson> Answers { get; set; }
	public required Guid UserId { get; set; }
	public required Guid ExamId { get; set; }
}