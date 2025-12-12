namespace SinaMN75U.Routes;

public static class TxnRoutes {
	public static void MapTxnRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (TxnCreateParams d, ITxnService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).Produces<UResponse<TxnResponse>>();
		r.MapPost("Read", async (TxnReadParams p, ITxnService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Cache(1).Produces<UResponse<IEnumerable<TxnResponse>>>();
		r.MapPost("Update", async (TxnUpdateParams d, ITxnService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).Produces<UResponse<TxnResponse>>();
		r.MapPost("Delete", async (IdParams d, ITxnService s, CancellationToken c) => (await s.Delete(d, c)).ToResult()).Produces<UResponse>();
	}
}