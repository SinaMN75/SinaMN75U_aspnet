using Microsoft.AspNetCore.OutputCaching;

namespace SinaMN75U.Routes;

public static class ContentRoutes {
	public static void MapContentRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (ContentCreateParams d, IContentService s, IOutputCacheStore ls, CancellationToken c) => {
			await ls.EvictByTagAsync(tag, c);
			return (await s.Create(d, c)).ToResult();
		}).Produces<UResponse<ContentEntity>>();

		r.MapPost("Read", async (ContentReadParams p, IContentService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).CacheOutput("VaryByBodyOnly").Produces<UResponse<IEnumerable<ContentEntity>>>();

		r.MapPost("Update", async (ContentUpdateParams d, IContentService s, IOutputCacheStore ls, CancellationToken c) => {
			await ls.EvictByTagAsync(tag, c);
			return (await s.Update(d, c)).ToResult();
		}).Produces<UResponse<ContentEntity>>();

		r.MapPost("Delete", async (IdParams d, IContentService s, IOutputCacheStore ls, CancellationToken c) => {
			await ls.EvictByTagAsync(tag, c);
			return (await s.Delete(d, c)).ToResult();
		}).Produces<UResponse>();
	}
}