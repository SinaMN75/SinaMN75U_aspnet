namespace SinaMN75U.Services;

public interface IExamService {
	public Task<UResponse<ExamResponse>> Create(ExamCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<ExamResponse>?>> Read(ExamReadParams p, CancellationToken ct);
	public Task<UResponse<ExamResponse?>> ReadById(IdParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdListParams p, CancellationToken ct);
	public Task<UResponse> SubmitAnswers(SubmitAnswersParams p, CancellationToken ct);
}

public class ExamService(DbContext db, ILocalizationService ls, ITokenService ts, IUserService userService) : IExamService {
	public async Task<UResponse<ExamResponse>> Create(ExamCreateParams p, CancellationToken ct) {
		EntityEntry<ExamEntity> e = await db.AddAsync(new ExamEntity {
			Title = p.Title,
			Description = p.Description,
			CategoryId = p.CategoryId,
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			JsonData = new ExamJson { Questions = p.Questions },
			Tags = p.Tags
		});
		await db.SaveChangesAsync(ct);
		return new UResponse<ExamResponse>(new ExamResponse {
			Title = e.Entity.Title,
			Description = e.Entity.Description,
			Id = e.Entity.Id,
			Tags = e.Entity.Tags,
			JsonData = e.Entity.JsonData
		});
	}

	public async Task<UResponse<IEnumerable<ExamResponse>?>> Read(ExamReadParams p, CancellationToken ct) {
		IQueryable<ExamEntity> q = db.Set<ExamEntity>();

		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(u => u.Tags.Any(tag => p.Tags!.Contains(tag)));
		if (p.CategoryId.IsNotNullOrEmpty()) q = q.Where(x => x.CategoryId == p.CategoryId);

		return await q.Select(x => new ExamResponse {
			Title = x.Title,
			Description = x.Description,
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			Category = new CategoryResponse {
				Title = x.Category!.Title,
				Id = x.Category.Id,
				Tags = x.Category.Tags,
				JsonData = x.Category.JsonData
			}
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ExamResponse?>> ReadById(IdParams p, CancellationToken ct) {
		ExamResponse? e = await db.Set<ExamEntity>().Select(x => new ExamResponse {
			Title = x.Title,
			Description = x.Description,
			Id = x.Id,
			Tags = x.Tags,
			JsonData = x.JsonData,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			Category = new CategoryResponse {
				Title = x.Category!.Title,
				Id = x.Category.Id,
				Tags = x.Category.Tags,
				JsonData = x.Category.JsonData
			}
		}).FirstOrDefaultAsync(x => x.Id == p.Id);

		return e == null ? new UResponse<ExamResponse?>(null, USC.NotFound, ls.Get("ExamNotFound")) : new UResponse<ExamResponse?>(e);
	}

	public async Task<UResponse> Delete(IdListParams p, CancellationToken ct) {
		int? count = await db.Set<ExamEntity>().Where(x => p.Ids.Contains(x.Id)).ExecuteDeleteAsync();
		return count.IsNotNullOrZero() ? new UResponse<ExamResponse?>(null, USC.NotFound, ls.Get("ExamNotFound")) : new UResponse();
	}

	public async Task<UResponse> SubmitAnswers(SubmitAnswersParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(USC.UnAuthorized, ls.Get("AuthorizationRequired"));
		await userService.Update(new UserUpdateParams {
			Id = p.UserId ?? userData.Id,
			UserAnswers = p.UserAnswers
		}, ct);
		return new UResponse();
	}
}