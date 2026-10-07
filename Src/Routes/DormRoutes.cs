namespace SinaMN75U.Routes;

public static class DormRoutes {
	public static void MapDormRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		r.MapPost("Dorm/Create", async (DormCreateParams p, IDormService s, CancellationToken c) => (await s.CreateDorm(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Dorm/Read", async (DormReadParams p, IDormService s, CancellationToken c) => (await s.ReadDorms(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormResponse>>>();
		r.MapPost("Dorm/ReadById", async (IdParams<DormSelectorArgs> p, IDormService s, CancellationToken c) => (await s.ReadDormById(p, c)).ToResult()).Produces<UResponse<DormResponse>>();
		r.MapPost("Dorm/Update", async (DormUpdateParams p, IDormService s, CancellationToken c) => (await s.UpdateDorm(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Dorm/Delete", async (IdParams p, IDormService s, CancellationToken c) => (await s.DeleteDorm(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("DormRoom/Create", async (DormRoomCreateParams p, IDormService s, CancellationToken c) => (await s.CreateDormRoom(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("DormRoom/Read", async (DormRoomReadParams p, IDormService s, CancellationToken c) => (await s.ReadDormRooms(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormRoomResponse>>>();
		r.MapPost("DormRoom/ReadById", async (IdParams<DormRoomSelectorArgs> p, IDormService s, CancellationToken c) => (await s.ReadDormRoomById(p, c)).ToResult()).Produces<UResponse<DormRoomResponse>>();
		r.MapPost("DormRoom/Update", async (DormRoomUpdateParams p, IDormService s, CancellationToken c) => (await s.UpdateDormRoom(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormRoom/Delete", async (IdParams p, IDormService s, CancellationToken c) => (await s.DeleteDormRoom(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("DormBed/Create", async (DormBedCreateParams p, IDormService s, CancellationToken c) => (await s.CreateDormBed(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("DormBed/Read", async (DormBedReadParams p, IDormService s, CancellationToken c) => (await s.ReadDormBeds(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormBedResponse>>>();
		r.MapPost("DormBed/ReadById", async (IdParams<DormBedSelectorArgs> p, IDormService s, CancellationToken c) => (await s.ReadDormBedById(p, c)).ToResult()).Produces<UResponse<DormBedResponse>>();
		r.MapPost("DormBed/Update", async (DormBedUpdateParams p, IDormService s, CancellationToken c) => (await s.UpdateDormBed(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBed/Delete", async (IdParams p, IDormService s, CancellationToken c) => (await s.DeleteDormBed(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("DormBedContract/Create", async (DormBedContractCreateParams p, IDormService s, CancellationToken c) => (await s.CreateDormBedContract(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("DormBedContract/Read", async (DormBedContractReadParams p, IDormService s, CancellationToken c) => (await s.ReadDormBedContracts(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormBedContractResponse>>>();
		r.MapPost("DormBedContract/Update", async (DormBedContractUpdateParams p, IDormService s, CancellationToken c) => (await s.UpdateDormBedContract(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBedContract/Delete", async (IdParams p, IDormService s, CancellationToken c) => (await s.DeleteDormBedContract(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("DormBedInvoice/Create", async (DormBedInvoiceCreateParams p, IDormService s, CancellationToken c) => (await s.CreateDormBedInvoice(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("DormBedInvoice/Read", async (DormBedInvoiceReadParams p, IDormService s, CancellationToken c) => (await s.ReadDormBedInvoices(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormBedInvoiceResponse>>>();
		r.MapPost("DormBedInvoice/Update", async (DormBedInvoiceUpdateParams p, IDormService s, CancellationToken c) => (await s.UpdateDormBedInvoice(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBedInvoice/Delete", async (IdParams p, IDormService s, CancellationToken c) => (await s.DeleteDormBedInvoice(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBedInvoice/Pay", async (IdParams p, IDormService s, CancellationToken c) => (await s.PayDormBedInvoiceByUser(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBedInvoice/ChartData", async (BaseParams p, IDormService s, CancellationToken c) => (await s.ReadDormBedInvoiceChartData(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBedInvoice/Split", async (DormBedInvoiceSplitParams p, IDormService s, CancellationToken c) => (await s.SplitDormBedInvoice(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("DormBedContract/Settle", async (DormBedContractSettleParams p, IDormService s, CancellationToken c) => (await s.SettleDormBedContract(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBedContract/Renew", async (DormBedContractRenewParams p, IDormService s, CancellationToken c) => (await s.RenewDormBedContract(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBedContract/Transfer", async (DormBedContractTransferParams p, IDormService s, CancellationToken c) => (await s.TransferDormBedContract(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("DormBedInvoice/Receive", async (InvoiceReceiveParams p, IDormService s, CancellationToken c) => (await s.ReceiveDormBedInvoice(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Dashboard/Read", async (DashboardRangeParams p, IDormService s, CancellationToken ct) => (await s.ReadDormDashboard(p, ct)).ToResult()).Produces<UResponse<DormDashboardResponse>>();
		r.MapPost("Seed", async (IDormService s, CancellationToken c) => (await s.SeedDorms(c)).ToResult()).Produces<UResponse<List<KeyValue>>>();
	}
}
