namespace SinaMN75U.Routes;

public static class InvoiceRoutes {
	public static void MapInvoiceRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (InvoiceCreateParams d, IInvoiceService s, CancellationToken c) => (await s.Create(d, c)).ToResult()).Produces<UResponse<InvoiceEntity>>();
		r.MapPost("Read", async (InvoiceReadParams p, IInvoiceService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Cache(1).Produces<UResponse<IEnumerable<InvoiceEntity>>>();
		r.MapPost("Update", async (InvoiceUpdateParams d, IInvoiceService s, CancellationToken c) => (await s.Update(d, c)).ToResult()).Produces<UResponse<InvoiceEntity>>();
		r.MapPost("Pay", async (IdParams d, IInvoiceService s, CancellationToken c) => (await s.Pay(d, c)).ToResult()).Produces<UResponse>();
	}
}