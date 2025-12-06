namespace SinaMN75U.Routes;

public static class ChatBotRoutes {
	public static void MapChatBotRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (ChatBotCreateParams d, IChatBotService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).Produces<UResponse<ChatBotResponse>>();
		r.MapPost("Read", async (ChatBotReadParams p, IChatBotService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Cache(1).Produces<UResponse<IEnumerable<ChatBotResponse>>>();
	}
}