namespace SinaMN75U.Routes;

public static class ChatRoutes {
	public static void MapChatRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		r.MapPost("Conversation/Create", async (ConversationCreateParams p, IChatService s, CancellationToken c) => (await s.CreateConversation(p, c)).ToResult()).Produces<UResponse<ConversationResponse>>();
		r.MapPost("Conversation/Read", async (ConversationReadParams p, IChatService s, CancellationToken c) => (await s.ReadConversations(p, c)).ToResult()).Produces<UResponse<IEnumerable<ConversationResponse>>>();
		r.MapPost("Conversation/ReadById", async (IdParams<ConversationSelectorArgs> p, IChatService s, CancellationToken c) => (await s.ReadConversationById(p, c)).ToResult()).Produces<UResponse<ConversationResponse>>();
		r.MapPost("Conversation/Update", async (ConversationUpdateParams p, IChatService s, CancellationToken c) => (await s.UpdateConversation(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Conversation/Leave", async (IdParams p, IChatService s, CancellationToken c) => (await s.LeaveConversation(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Conversation/MarkRead", async (IdParams p, IChatService s, CancellationToken c) => (await s.MarkConversationRead(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Message/Create", async (MessageCreateParams p, IChatService s, CancellationToken c) => (await s.CreateMessage(p, c)).ToResult()).Produces<UResponse<MessageResponse>>();
		r.MapPost("Message/Read", async (MessageReadParams p, IChatService s, CancellationToken c) => (await s.ReadMessages(p, c)).ToResult()).Produces<UResponse<IEnumerable<MessageResponse>>>();
		r.MapPost("Message/Update", async (MessageUpdateParams p, IChatService s, CancellationToken c) => (await s.UpdateMessage(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Message/Delete", async (IdParams p, IChatService s, CancellationToken c) => (await s.DeleteMessage(p, c)).ToResult()).Produces<UResponse>();
	}
}
