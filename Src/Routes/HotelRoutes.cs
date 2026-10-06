namespace SinaMN75U.Routes;

public static class HotelRoutes {
	public static void MapHotelRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		r.MapPost("Organization/Create", async (OrganizationCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateOrganization(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Organization/Read", async (OrganizationReadParams p, IHotelService s, CancellationToken c) => (await s.ReadOrganizations(p, c)).ToResult()).Produces<UResponse<IEnumerable<OrganizationResponse>>>();
		r.MapPost("Organization/Update", async (OrganizationUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateOrganization(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Organization/SetMember", async (OrganizationMemberParams p, IHotelService s, CancellationToken c) => (await s.SetOrganizationMember(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Organization/RemoveMember", async (OrganizationMemberParams p, IHotelService s, CancellationToken c) => (await s.RemoveOrganizationMember(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Organization/RequestSettlement", async (OrganizationSettlementRequestParams p, IHotelService s, CancellationToken c) => (await s.RequestOrganizationSettlement(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Organization/ProcessSettlement", async (OrganizationSettlementProcessParams p, IHotelService s, CancellationToken c) => (await s.ProcessOrganizationSettlement(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Hotel/Create", async (HotelCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateHotel(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Hotel/Read", async (HotelReadParams p, IHotelService s, CancellationToken c) => (await s.ReadHotels(p, c)).ToResult()).Produces<UResponse<IEnumerable<HotelResponse>>>();
		r.MapPost("Hotel/ReadById", async (IdParams<HotelSelectorArgs> p, IHotelService s, CancellationToken c) => (await s.ReadHotelById(p, c)).ToResult()).Produces<UResponse<HotelResponse>>();
		r.MapPost("Hotel/Update", async (HotelUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateHotel(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Hotel/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteHotel(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("HotelRoom/Create", async (HotelRoomCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateHotelRoom(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("HotelRoom/Read", async (HotelRoomReadParams p, IHotelService s, CancellationToken c) => (await s.ReadHotelRooms(p, c)).ToResult()).Produces<UResponse<IEnumerable<HotelRoomResponse>>>();
		r.MapPost("HotelRoom/ReadById", async (IdParams<HotelRoomSelectorArgs> p, IHotelService s, CancellationToken c) => (await s.ReadHotelRoomById(p, c)).ToResult()).Produces<UResponse<HotelRoomResponse>>();
		r.MapPost("HotelRoom/Update", async (HotelRoomUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateHotelRoom(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelRoom/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteHotelRoom(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelRoom/Availability", async (HotelRoomAvailabilityParams p, IHotelService s, CancellationToken c) => (await s.ReadHotelRoomAvailability(p, c)).ToResult()).Produces<UResponse<IEnumerable<HotelRoomAvailabilityResponse>>>();

		r.MapPost("HotelReservation/Create", async (HotelReservationCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateHotelReservation(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("HotelReservation/Read", async (HotelReservationReadParams p, IHotelService s, CancellationToken c) => (await s.ReadHotelReservations(p, c)).ToResult()).Produces<UResponse<IEnumerable<HotelReservationResponse>>>();
		r.MapPost("HotelReservation/ReadById", async (IdParams<HotelReservationSelectorArgs> p, IHotelService s, CancellationToken c) => (await s.ReadHotelReservationById(p, c)).ToResult()).Produces<UResponse<HotelReservationResponse>>();
		r.MapPost("HotelReservation/Update", async (HotelReservationUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateHotelReservation(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelReservation/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteHotelReservation(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelReservation/Confirm", async (IdParams p, IHotelService s, CancellationToken c) => (await s.ConfirmHotelReservation(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelReservation/CheckIn", async (IdParams p, IHotelService s, CancellationToken c) => (await s.CheckInHotelReservation(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelReservation/CheckOut", async (IdParams p, IHotelService s, CancellationToken c) => (await s.CheckOutHotelReservation(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelReservation/Cancel", async (IdParams p, IHotelService s, CancellationToken c) => (await s.CancelHotelReservation(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelReservation/Book", async (HotelReservationBookParams p, IHotelService s, CancellationToken c) => (await s.BookHotelReservation(p, c)).ToResult()).Produces<UResponse<HotelReservationResponse>>();
		r.MapPost("HotelReservation/CancelByUser", async (HotelReservationCancelParams p, IHotelService s, CancellationToken c) => (await s.CancelHotelReservationByUser(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("HotelInvoice/Create", async (HotelInvoiceCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateHotelInvoice(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("HotelInvoice/Read", async (HotelInvoiceReadParams p, IHotelService s, CancellationToken c) => (await s.ReadHotelInvoices(p, c)).ToResult()).Produces<UResponse<IEnumerable<HotelInvoiceResponse>>>();
		r.MapPost("HotelInvoice/Update", async (HotelInvoiceUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateHotelInvoice(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelInvoice/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteHotelInvoice(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelInvoice/Pay", async (IdParams p, IHotelService s, CancellationToken c) => (await s.PayHotelInvoice(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Dorm/Create", async (DormCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateDorm(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Dorm/Read", async (DormReadParams p, IHotelService s, CancellationToken c) => (await s.ReadDorms(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormResponse>>>();
		r.MapPost("Dorm/ReadById", async (IdParams<DormSelectorArgs> p, IHotelService s, CancellationToken c) => (await s.ReadDormById(p, c)).ToResult()).Produces<UResponse<DormResponse>>();
		r.MapPost("Dorm/Update", async (DormUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateDorm(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Dorm/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteDorm(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("DormRoom/Create", async (DormRoomCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateDormRoom(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("DormRoom/Read", async (DormRoomReadParams p, IHotelService s, CancellationToken c) => (await s.ReadDormRooms(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormRoomResponse>>>();
		r.MapPost("DormRoom/ReadById", async (IdParams<DormRoomSelectorArgs> p, IHotelService s, CancellationToken c) => (await s.ReadDormRoomById(p, c)).ToResult()).Produces<UResponse<DormRoomResponse>>();
		r.MapPost("DormRoom/Update", async (DormRoomUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateDormRoom(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormRoom/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteDormRoom(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("DormBed/Create", async (DormBedCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateDormBed(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("DormBed/Read", async (DormBedReadParams p, IHotelService s, CancellationToken c) => (await s.ReadDormBeds(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormBedResponse>>>();
		r.MapPost("DormBed/ReadById", async (IdParams<DormBedSelectorArgs> p, IHotelService s, CancellationToken c) => (await s.ReadDormBedById(p, c)).ToResult()).Produces<UResponse<DormBedResponse>>();
		r.MapPost("DormBed/Update", async (DormBedUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateDormBed(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBed/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteDormBed(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("DormBedContract/Create", async (DormBedContractCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateDormBedContract(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("DormBedContract/Read", async (DormBedContractReadParams p, IHotelService s, CancellationToken c) => (await s.ReadDormBedContracts(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormBedContractResponse>>>();
		r.MapPost("DormBedContract/Update", async (DormBedContractUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateDormBedContract(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBedContract/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteDormBedContract(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("DormBedInvoice/Create", async (DormBedInvoiceCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateDormBedInvoice(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("DormBedInvoice/Read", async (DormBedInvoiceReadParams p, IHotelService s, CancellationToken c) => (await s.ReadDormBedInvoices(p, c)).ToResult()).Produces<UResponse<IEnumerable<DormBedInvoiceResponse>>>();
		r.MapPost("DormBedInvoice/Update", async (DormBedInvoiceUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateDormBedInvoice(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBedInvoice/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteDormBedInvoice(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBedInvoice/Pay", async (IdParams p, IHotelService s, CancellationToken c) => (await s.PayDormBedInvoiceByUser(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBedInvoice/ChartData", async (BaseParams p, IHotelService s, CancellationToken c) => (await s.ReadDormBedInvoiceChartData(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBedInvoice/Split", async (DormBedInvoiceSplitParams p, IHotelService s, CancellationToken c) => (await s.SplitDormBedInvoice(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBedContract/Settle", async (DormBedContractSettleParams p, IHotelService s, CancellationToken c) => (await s.SettleDormBedContract(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBedContract/Renew", async (DormBedContractRenewParams p, IHotelService s, CancellationToken c) => (await s.RenewDormBedContract(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DormBedContract/Transfer", async (DormBedContractTransferParams p, IHotelService s, CancellationToken c) => (await s.TransferDormBedContract(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("DormBedInvoice/Receive", async (InvoiceReceiveParams p, IHotelService s, CancellationToken c) => (await s.ReceiveDormBedInvoice(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("HotelInvoice/Receive", async (InvoiceReceiveParams p, IHotelService s, CancellationToken c) => (await s.ReceiveHotelInvoice(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Account/Create", async (AccountCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateAccount(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Account/Read", async (AccountReadParams p, IHotelService s, CancellationToken c) => (await s.ReadAccounts(p, c)).ToResult()).Produces<UResponse<IEnumerable<AccountResponse>>>();
		r.MapPost("Account/Update", async (AccountUpdateParams p, IHotelService s, CancellationToken c) => (await s.UpdateAccount(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Account/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteAccount(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Voucher/Create", async (VoucherCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateVoucher(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Voucher/Read", async (VoucherReadParams p, IHotelService s, CancellationToken c) => (await s.ReadVouchers(p, c)).ToResult()).Produces<UResponse<IEnumerable<VoucherResponse>>>();
		r.MapPost("Voucher/Delete", async (IdParams p, IHotelService s, CancellationToken c) => (await s.DeleteVoucher(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Ledger/Read", async (LedgerReadParams p, IHotelService s, CancellationToken c) => (await s.ReadLedger(p, c)).ToResult()).Produces<UResponse<LedgerResponse>>();
		r.MapPost("Ledger/Report", async (LedgerReportParams p, IHotelService s, CancellationToken c) => (await s.ReadLedgerReport(p, c)).ToResult()).Produces<UResponse<LedgerReportResponse>>();
		r.MapPost("Check/Create", async (CheckCreateParams p, IHotelService s, CancellationToken c) => (await s.CreateCheck(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Check/Read", async (CheckReadParams p, IHotelService s, CancellationToken c) => (await s.ReadChecks(p, c)).ToResult()).Produces<UResponse<IEnumerable<CheckResponse>>>();
		r.MapPost("Check/SetStatus", async (CheckStatusParams p, IHotelService s, CancellationToken c) => (await s.SetCheckStatus(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Dashboard/Read", async (DashboardRangeParams p, IHotelService s, CancellationToken ct) => (await s.ReadPropertyDashboard(p, ct)).ToResult()).Produces<UResponse<PropertyDashboardResponse>>();
		r.MapPost("Seed", async (IHotelService s, CancellationToken c) => (await s.SeedHotelsAndDorms(c)).ToResult()).Produces<UResponse<List<KeyValue>>>();
	}
}