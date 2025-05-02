namespace SinaMN75U.Data.Entities;

[Table("Exams")]
public class ExamEntity : BaseEntity<TagExam, ExamJson> {
	public required string Title { get; set; }
	public required string Description { get; set; }

	public required Guid CategoryId { get; set; }
	public CategoryEntity? Category { get; set; }
}

public class ExamJson {
	public required List<QuestionJson> Questions { get; set; }
	public required List<ExamScoreDetail> ScoreDetails { get; set; }
}

public class ExamScoreDetail {
	public required double MinScore { get; set; }
	public required double MaxScore { get; set; }
	public required string Label { get; set; }
	public required string Description { get; set; }
}

public class QuestionJson {
	public int Order { get; set; } = 0;
	public required string Title { get; set; }
	public required string Description { get; set; }
	public required List<QuestionOptionJson> Options { get; set; }
}

public class QuestionOptionJson {
	public required string Title { get; set; }
	public required string Hint { get; set; }
	public required double Score { get; set; }
}

public class UserAnswerJson {
	public required DateTime Date { get; set; }
	public required double TotalScore { get; set; }
	public required List<UserAnswerResultJson> Results { get; set; }
	public required string Label { get; set; }
	public required string Description { get; set; }
}

public class UserAnswerResultJson {
	public required string Question { get; set; }
	public required QuestionOptionJson Answer { get; set; }
}