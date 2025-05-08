namespace SinaMN75U.Routes;

public static class ExamRoutes {
	public static void MapExamRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder route = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		route.MapPost("Create", async (ExamCreateParams d, IExamService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Create(d, c)).ToResult();
		}).Produces<UResponse<ExamResponse>>();

		route.MapPost("Read", async (ExamReadParams p, IExamService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Cache(60).Produces<UResponse<IEnumerable<ExamResponse>>>();

		route.MapPost("ReadById", async (IdParams p, IExamService s, CancellationToken c) => (await s.ReadById(p, c)).ToResult()).Produces<UResponse<ExamResponse>>();

		route.MapPost("Delete", async (IdListParams p, IExamService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Delete(p, c)).ToResult();
		}).Produces<UResponse<ExamResponse>>();

		route.MapPost("SubmitAnswers", async (SubmitAnswersParams p, IExamService s, CancellationToken c) => (await s.SubmitAnswers(p, c)).ToResult()).Produces<UResponse>();
	}
}