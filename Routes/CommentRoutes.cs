namespace SinaMN75U.Routes;

public static class CommentRoutes {
	public static void MapCommentRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (CommentCreateParams d, ICommentService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Create(d, c)).ToResult();
		}).Produces<UResponse<CommentResponse>>();

		r.MapPost("Read", async (CommentReadParams p, ICommentService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Cache(o => o.Minutes = 60).Produces<UResponse<IEnumerable<CommentResponse>>>();

		r.MapPost("Update", async (CommentUpdateParams d, ICommentService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Update(d, c)).ToResult();
		}).Produces<UResponse<CommentResponse>>();

		r.MapPost("Delete", async (IdParams d, ICommentService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Delete(d, c)).ToResult();
		}).Produces<UResponse>();
	}
}