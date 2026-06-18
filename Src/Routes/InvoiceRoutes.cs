namespace SinaMN75U.Routes;

public static class InvoiceRoutes {
	public static void MapInvoiceRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (InvoiceCreateParams p, IInvoiceService s, CancellationToken c) => (await s.Create(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Read", async (InvoiceReadParams p, IInvoiceService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<InvoiceResponse>>>();
		r.MapPost("Update", async (InvoiceUpdateParams p, IInvoiceService s, CancellationToken c) => (await s.Update(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Delete", async (IdParams p, IInvoiceService s, CancellationToken c) => (await s.Delete(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Pay", async (IdParams p, IInvoiceService s, CancellationToken c) => (await s.Pay(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("ChartData", async (BaseParams p, IInvoiceService s, CancellationToken c) => (await s.ReadChartData(p, c)).ToResult()).Produces<UResponse>();
	}
}