namespace SinaMN75U.Routes;

public static class SocialRoutes {
	public static void MapSocialRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		r.MapPost("Post/Create", async (PostCreateParams p, ISocialService s, CancellationToken c) => (await s.CreatePost(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Post/Read", async (PostReadParams p, ISocialService s, CancellationToken c) => (await s.ReadPosts(p, c)).ToResult()).Produces<UResponse<IEnumerable<PostResponse>>>();
		r.MapPost("Post/ReadById", async (IdParams<PostSelectorArgs> p, ISocialService s, CancellationToken c) => (await s.ReadPostById(p, c)).ToResult()).Produces<UResponse<PostResponse>>();
		r.MapPost("Post/Update", async (PostUpdateParams p, ISocialService s, CancellationToken c) => (await s.UpdatePost(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Post/Delete", async (IdParams p, ISocialService s, CancellationToken c) => (await s.DeletePost(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Post/React", async (PostReactParams p, ISocialService s, CancellationToken c) => (await s.ReactPost(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Post/ViewStory", async (IdParams p, ISocialService s, CancellationToken c) => (await s.ViewStory(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Report/Create", async (ReportCreateParams p, ISocialService s, CancellationToken c) => (await s.CreateReport(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Report/Read", async (ReportReadParams p, ISocialService s, CancellationToken c) => (await s.ReadReports(p, c)).ToResult()).Produces<UResponse<IEnumerable<ReportResponse>>>();
		r.MapPost("Report/Update", async (ReportUpdateParams p, ISocialService s, CancellationToken c) => (await s.UpdateReport(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Block/Create", async (BlockCreateParams p, ISocialService s, CancellationToken c) => (await s.Block(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Block/Read", async (BlockReadParams p, ISocialService s, CancellationToken c) => (await s.ReadBlocks(p, c)).ToResult()).Produces<UResponse<IEnumerable<BlockResponse>>>();
		r.MapPost("Block/Delete", async (BlockCreateParams p, ISocialService s, CancellationToken c) => (await s.Unblock(p, c)).ToResult()).Produces<UResponse>();
	}
}
