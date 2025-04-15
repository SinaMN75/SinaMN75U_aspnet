namespace SinaMN75U.Routes;

public static class ExamRoutes {
	public static void MapExamRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder route = app.MapGroup("api/Exam/").WithTags(tag).AddEndpointFilter<UValidationFilter>();

		route.MapPost("Create", async (ExamCreateParams d, IExamService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).Produces<UResponse<ExamResponse>>();

		route.MapPost("Read", async (ExamReadParams p, IExamService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).CacheOutput(o => o.Tag(tag)).Produces<UResponse<IEnumerable<ExamResponse>>>();

		route.MapPost("ReadById", async (IdParams p, IExamService s, CancellationToken c) => (await s.ReadById(p, c)).ToResult()).CacheOutput(o => o.Tag(tag)).Produces<UResponse<ExamResponse>>();

		route.MapPost("Delete", async (IdListParams p, IExamService s, CancellationToken c) => (await s.Delete(p, c)).ToResult()).Produces<UResponse<ExamResponse>>();

		route.MapPost("SubmitAnswers", async (SubmitAnswersParams p, IExamService s, CancellationToken c) => (await s.SubmitAnswers(p, c)).ToResult()).Produces<UResponse>();
	}
}