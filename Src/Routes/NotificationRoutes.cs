namespace SinaMN75U.Routes;

public static class NotificationRoutes {
	public static void MapNotificationRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (NotificationCreateParams p, INotificationService s, CancellationToken c) => (await s.Create(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Read", async (NotificationReadParams p, INotificationService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<NotificationResponse>>>();
		r.MapPost("Update", async (NotificationUpdateParams p, INotificationService s, CancellationToken c) => (await s.Update(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Delete", async (IdParams p, INotificationService s, CancellationToken c) => (await s.Delete(p, c)).ToResult()).Produces<UResponse>();
	}
}