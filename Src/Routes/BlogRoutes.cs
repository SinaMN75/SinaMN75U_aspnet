namespace SinaMN75U.Routes;

public static class BlogRoutes {
	public static void MapBlogRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		r.MapPost("Create", async (BlogCreateParams p, IBlogService s, CancellationToken c) => (await s.Create(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Read", async (BlogReadParams p, IBlogService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<BlogResponse>>>();
		r.MapPost("ReadById", async (IdParams<BlogSelectorArgs> p, IBlogService s, CancellationToken c) => (await s.ReadById(p, c)).ToResult()).Produces<UResponse<BlogResponse>>();
		r.MapPost("Update", async (BlogUpdateParams p, IBlogService s, CancellationToken c) => (await s.Update(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Delete", async (IdParams p, IBlogService s, CancellationToken c) => (await s.Delete(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DeleteRange", async (IdListParams p, IBlogService s, CancellationToken c) => (await s.DeleteRange(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Publish", async (IdParams p, IBlogService s, CancellationToken c) => (await s.Publish(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Unpublish", async (IdParams p, IBlogService s, CancellationToken c) => (await s.Unpublish(p, c)).ToResult()).Produces<UResponse>();
	}
}
