namespace SinaMN75U.Routes;

public static class DormRoutes {
	public static void MapDormRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>().AddEndpointFilter<ActivityLogFilter>();

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

		r.MapPost("DormBedContract/Checklist", async (DormBedContractChecklistParams p, IDormService s, CancellationToken c) => (await s.SetDormBedContractChecklist(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBedContract/Print", async (IdParams p, IDormService s, CancellationToken c) => (await s.PrintDormBedContract(p, c)).ToResult()).Produces<UResponse<string>>();
		r.MapPost("DormBedInvoice/Print", async (IdParams p, IDormService s, CancellationToken c) => (await s.PrintDormBedInvoice(p, c)).ToResult()).Produces<UResponse<string>>();

		r.MapPost("DormApplication/Create", async (DormApplicationCreateParams p, IDormService s, CancellationToken c) => (await s.CreateDormApplication(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("DormApplication/Read", async (DormApplicationReadParams p, IDormService s, CancellationToken c) => (await s.ReadDormApplications(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormApplicationResponse>>>();
		r.MapPost("DormApplication/Review", async (DormApplicationReviewParams p, IDormService s, CancellationToken c) => (await s.ReviewDormApplication(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormApplication/Delete", async (IdParams p, IDormService s, CancellationToken c) => (await s.DeleteDormApplication(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("DormRecord/Create", async (DormRecordCreateParams p, IDormService s, CancellationToken c) => (await s.CreateDormRecord(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("DormRecord/Read", async (DormRecordReadParams p, IDormService s, CancellationToken c) => (await s.ReadDormRecords(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormRecordResponse>>>();
		r.MapPost("DormRecord/Update", async (DormRecordUpdateParams p, IDormService s, CancellationToken c) => (await s.UpdateDormRecord(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormRecord/Delete", async (IdParams p, IDormService s, CancellationToken c) => (await s.DeleteDormRecord(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("DormMeal/Create", async (DormMealCreateParams p, IDormService s, CancellationToken c) => (await s.CreateDormMeal(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("DormMeal/Read", async (DormMealReadParams p, IDormService s, CancellationToken c) => (await s.ReadDormMeals(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormMealResponse>>>();
		r.MapPost("DormMeal/Update", async (DormMealUpdateParams p, IDormService s, CancellationToken c) => (await s.UpdateDormMeal(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormMeal/Delete", async (IdParams p, IDormService s, CancellationToken c) => (await s.DeleteDormMeal(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("DormBooking/Create", async (DormBookingCreateParams p, IDormService s, CancellationToken c) => (await s.CreateDormBooking(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("DormBooking/Read", async (DormBookingReadParams p, IDormService s, CancellationToken c) => (await s.ReadDormBookings(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormBookingResponse>>>();
		r.MapPost("DormBooking/Cancel", async (IdParams p, IDormService s, CancellationToken c) => (await s.CancelDormBooking(p, c)).ToResult()).Produces<UResponse>();
	}
}
