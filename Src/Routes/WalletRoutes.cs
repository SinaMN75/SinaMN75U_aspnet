namespace SinaMN75U.Routes;

public static class WalletRoutes {
	public static void MapWalletRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Transfer", async (WalletTransferParams p, IWalletService s, CancellationToken c) => (await s.Transfer(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("ReadTxn", async (WalletTxnReadParams p, IWalletService s, CancellationToken c) => (await s.ReadTxn(p, c)).ToResult()).Produces<UResponse<IEnumerable<WalletTxnResponse>?>>();
	}
}