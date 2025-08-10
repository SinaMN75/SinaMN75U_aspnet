namespace SinaMN75U.Routes;

public static class CommentRoutes {
	public static void MapCommentRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (CommentCreateParams d, ICommentService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Create(d, c)).ToResult();
		}).Produces<UResponse<CommentEntity>>();

		r.MapPost("Read", async (CommentReadParams p, ICommentService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Cache(60).Produces<UResponse<IEnumerable<CommentEntity>>>();
		r.MapPost("ReadById", async (IdParams p, ICommentService s, CancellationToken c) => (await s.ReadById(p, c)).ToResult()).Cache(60).Produces<UResponse<CommentEntity>>();
		
		r.MapPost("Update", async (CommentUpdateParams d, ICommentService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Update(d, c)).ToResult();
		}).Produces<UResponse<CommentEntity>>();

		r.MapPost("Delete", async (IdParams d, ICommentService s, ILocalStorageService ls, CancellationToken c) => {
			ls.DeleteAllByPartialKey(tag);
			return (await s.Delete(d, c)).ToResult();
		}).Produces<UResponse>();
		
		r.MapPost("ReadProductCommentCount", async (IdParams p, ICommentService s, CancellationToken c) => (await s.ReadProductCommentCount(p, c)).ToResult()).Cache(60).Produces<UResponse<int>>();
		r.MapPost("ReadUserCommentCount", async (IdParams p, ICommentService s, CancellationToken c) => (await s.ReadUserCommentCount(p, c)).ToResult()).Cache(60).Produces<UResponse<int>>();
	}
}