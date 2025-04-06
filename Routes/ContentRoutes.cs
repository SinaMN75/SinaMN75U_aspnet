namespace SinaMN75U.Routes;

public static class ContentRoutes {
	public static void MapContentRoutes(this IEndpointRouteBuilder app, string name) {
		RouteGroupBuilder r = app.MapGroup("api/content/").WithTags(name);
		r.MapPost("Create", async (ContentCreateParams d, IContentService s, IOutputCacheStore store, CancellationToken c) => {
			await store.EvictByTagAsync(name, c);
			return (await s.Create(d, c)).ToResult();
		}).Produces<UResponse<ContentResponse>>();
		
		r.MapPost("Read", async (ContentReadParams p, IContentService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).WithTags(name).CacheOutput(o => {
			o.Tag(name);
			o.Expire(TimeSpan.FromDays(1));
		}).Produces<UResponse<IEnumerable<ContentResponse>>>();

		r.MapPost("Update", async (ContentUpdateParams d, IContentService s, IOutputCacheStore store, CancellationToken c) => {
			await store.EvictByTagAsync(name, c);
			return (await s.Update(d, c)).ToResult();
		}).Produces<UResponse<ContentResponse>>();

		r.MapPost("Delete", async (IdParams d, IContentService s, IOutputCacheStore store, CancellationToken c) => {
			await store.EvictByTagAsync(name, c);
			return (await s.Delete(d, c)).ToResult();
		}).Produces<UResponse>();
	}
}