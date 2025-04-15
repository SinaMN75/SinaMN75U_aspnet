namespace SinaMN75U.Data.Entities;

[Table("Exams")]
public class ExamEntity : BaseEntity<TagExam, ExamJson> {
	public required string Title { get; set; }
	public required string Description { get; set; }

	public required Guid CategoryId { get; set; }
	public CategoryEntity? Category { get; set; }
}

public class UserAnswerResultJson {
	public required string Question { get; set; }
	public required Option Answer { get; set; }
}

public class ExamJson {
	public required List<Question> Questions { get; set; }
}

public class Question {
	public int Order { get; set; } = 0;
	public required string Title { get; set; }
	public required string Description { get; set; }
	public required List<Option> Options { get; set; }
}

public class Option {
	public required string Title { get; set; }
	public required string Score { get; set; }
}

public class UserAnswerJson {
	public required DateTime Date { get; set; }
	public required double TotalScore { get; set; }
	public required List<UserAnswerResultJson> Results { get; set; }
}