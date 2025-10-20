namespace SinaMN75U.Routes;

public static class ExamRoutes {
	public static void MapExamRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		r.MapPost("Create", async (ExamCreateParams d, IExamService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Create(d, c)).ToResult();
		}).Produces<UResponse<ExamEntity>>();

		r.MapPost("Read", async (ExamReadParams p, IExamService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Cache(1).Produces<UResponse<IEnumerable<ExamEntity>>>();

		r.MapPost("ReadById", async (IdParams p, IExamService s, CancellationToken c) => (await s.ReadById(p, c)).ToResult()).Produces<UResponse<ExamEntity>>();

		r.MapPost("Delete", async (IdListParams p, IExamService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Delete(p, c)).ToResult();
		}).Produces<UResponse<ExamEntity>>();

		r.MapPost("SubmitAnswers", async (SubmitAnswersParams p, IExamService s, CancellationToken c) => (await s.SubmitAnswers(p, c)).ToResult()).Produces<UResponse>();
	}
}