namespace SinaMN75U.Routes;

public static class ProcessRoutes {
	public static void MapProcessRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Get", async (IdStringParams p, IProcessService s, CancellationToken c) => (await s.Get(p, c)).ToResult()).Produces<UResponse<UProcessStepGet?>>();
		r.MapPost("Send", async (UProcessStepSend p, IProcessService s, CancellationToken c) => (await s.Send(p, c)).ToResult()).Produces<UResponse<UProcessStepGet>>();
	}
}