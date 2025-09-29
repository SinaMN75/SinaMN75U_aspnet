namespace SinaMN75U.Services;

public interface IExamService {
	public Task<UResponse<ExamEntity>> Create(ExamCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<ExamEntity>?>> Read(ExamReadParams p, CancellationToken ct);
	public Task<UResponse<ExamEntity?>> ReadById(IdParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdListParams p, CancellationToken ct);
	public Task<UResponse> SubmitAnswers(SubmitAnswersParams p, CancellationToken ct);
}

public class ExamService(DbContext db, ILocalizationService ls, ITokenService ts) : IExamService {
	public async Task<UResponse<ExamEntity>> Create(ExamCreateParams p, CancellationToken ct) {
		EntityEntry<ExamEntity> e = await db.AddAsync(new ExamEntity {
			Title = p.Title,
			Description = p.Description,
			CategoryId = p.CategoryId,
			JsonData = new ExamJson {
				Questions = p.Questions,
				ScoreDetails = p.ScoreDetails
			},
			Tags = p.Tags
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<ExamEntity>(new ExamEntity {
			Title = e.Entity.Title,
			Description = e.Entity.Description,
			Id = e.Entity.Id,
			Tags = e.Entity.Tags,
			JsonData = e.Entity.JsonData,
			CategoryId = e.Entity.CategoryId,
			CreatedAt = e.Entity.CreatedAt,
			UpdatedAt = e.Entity.UpdatedAt
		});
	}

	public async Task<UResponse<IEnumerable<ExamEntity>?>> Read(ExamReadParams p, CancellationToken ct) {
		IQueryable<ExamEntity> q = db.Set<ExamEntity>();

		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(u => u.Tags.Any(tag => p.Tags!.Contains(tag)));
		if (p.CategoryId.IsNotNullOrEmpty()) q = q.Where(x => x.CategoryId == p.CategoryId);

		return await q.Select(x => new ExamEntity {
			Title = x.Title,
			Description = x.Description,
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			CategoryId = x.CategoryId,
			Category = new CategoryEntity {
				Title = x.Category!.Title,
				Id = x.Category.Id,
				Tags = x.Category.Tags,
				JsonData = x.Category.JsonData,
				CreatedAt = x.CreatedAt,
				UpdatedAt = x.UpdatedAt
			}
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ExamEntity?>> ReadById(IdParams p, CancellationToken ct) {
		ExamEntity? e = await db.Set<ExamEntity>().Select(x => new ExamEntity {
			Title = x.Title,
			Description = x.Description,
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			CategoryId = x.CategoryId,
			Category = new CategoryEntity {
				Title = x.Category!.Title,
				Id = x.Category.Id,
				Tags = x.Category.Tags,
				JsonData = x.Category.JsonData,
				CreatedAt = x.CreatedAt,
				UpdatedAt = x.UpdatedAt
			}
		}).FirstOrDefaultAsync(x => x.Id == p.Id, ct);

		return e == null ? new UResponse<ExamEntity?>(null, Usc.NotFound, ls.Get("ExamNotFound")) : new UResponse<ExamEntity?>(e);
	}

	public async Task<UResponse> Delete(IdListParams p, CancellationToken ct) {
		int? count = await db.Set<ExamEntity>().Where(x => p.Ids.Contains(x.Id)).ExecuteDeleteAsync(ct);
		return count.IsNotNullOrZero() ? new UResponse<ExamEntity?>(null, Usc.NotFound, ls.Get("ExamNotFound")) : new UResponse();
	}

	public async Task<UResponse> SubmitAnswers(SubmitAnswersParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		UserEntity? user = await db.Set<UserEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.UserId, ct);
		if (user == null) return new UResponse(Usc.NotFound, ls.Get("UserNotFound"));

		ExamEntity? exam = await db.Set<ExamEntity>().FirstOrDefaultAsync(x => x.Id == p.ExamId, ct);
		if (exam == null) return new UResponse(Usc.NotFound, ls.Get("ExamNotFound"));

		double score = p.Answers.Sum(x => x.Answer.Score);

		ExamScoreDetail scoreDetail = exam.JsonData.ScoreDetails.FirstOrDefault(c => score >= c.MinScore && score <= c.MaxScore)
		                              ?? new ExamScoreDetail {
			                              Label = "Uncategorized",
			                              Description = "Your score doesn't fall into defined categories",
			                              MinScore = 1,
			                              MaxScore = 1000
		                              };

		foreach (ExamScoreDetail condition in exam.JsonData.ScoreDetails.Where(condition => score >= condition.MinScore && score <= condition.MaxScore))
			scoreDetail = condition;

		UserAnswerJson json = new() {
			Date = DateTime.UtcNow,
			TotalScore = score,
			Results = p.Answers,
			Label = scoreDetail.Label,
			Description = scoreDetail.Description
		};

		user.JsonData.UserAnswerJson.Add(json);
		db.Update(user);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}
}