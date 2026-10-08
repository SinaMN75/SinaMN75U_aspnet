namespace SinaMN75U.Routes;

public static class AccountingRoutes {
	public static void MapAccountingRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>().AddEndpointFilter<ActivityLogFilter>();
		r.MapPost("Report", async (AccountingReportParams p, IAccountingService s, CancellationToken c) => (await s.Report(p, c)).ToResult()).Produces<UResponse<AccountingReportResponse?>>();

		r.MapPost("Settlement/Request", async (OrganizationSettlementRequestParams p, IAccountingService s, CancellationToken c) => (await s.RequestOrganizationSettlement(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Settlement/Process", async (OrganizationSettlementProcessParams p, IAccountingService s, CancellationToken c) => (await s.ProcessOrganizationSettlement(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Account/Create", async (AccountCreateParams p, IAccountingService s, CancellationToken c) => (await s.CreateAccount(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Account/Read", async (AccountReadParams p, IAccountingService s, CancellationToken c) => (await s.ReadAccounts(p, c)).ToResult()).Produces<UResponse<IEnumerable<AccountResponse>>>();
		r.MapPost("Account/Update", async (AccountUpdateParams p, IAccountingService s, CancellationToken c) => (await s.UpdateAccount(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Account/Delete", async (IdParams p, IAccountingService s, CancellationToken c) => (await s.DeleteAccount(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Voucher/Create", async (VoucherCreateParams p, IAccountingService s, CancellationToken c) => (await s.CreateVoucher(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Voucher/Read", async (VoucherReadParams p, IAccountingService s, CancellationToken c) => (await s.ReadVouchers(p, c)).ToResult()).Produces<UResponse<IEnumerable<VoucherResponse>>>();
		r.MapPost("Voucher/Delete", async (IdParams p, IAccountingService s, CancellationToken c) => (await s.DeleteVoucher(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Ledger/Read", async (LedgerReadParams p, IAccountingService s, CancellationToken c) => (await s.ReadLedger(p, c)).ToResult()).Produces<UResponse<LedgerResponse>>();
		r.MapPost("Ledger/TaxInvoices", async (LedgerReportParams p, IAccountingService s, CancellationToken c) => (await s.ReadTaxInvoices(p, c)).ToResult()).Produces<UResponse<IEnumerable<TaxInvoiceItem>>>();
		r.MapPost("Ledger/Report", async (LedgerReportParams p, IAccountingService s, CancellationToken c) => (await s.ReadLedgerReport(p, c)).ToResult()).Produces<UResponse<LedgerReportResponse>>();

		r.MapPost("Check/Create", async (CheckCreateParams p, IAccountingService s, CancellationToken c) => (await s.CreateCheck(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Check/Read", async (CheckReadParams p, IAccountingService s, CancellationToken c) => (await s.ReadChecks(p, c)).ToResult()).Produces<UResponse<IEnumerable<CheckResponse>>>();
		r.MapPost("Check/SetStatus", async (CheckStatusParams p, IAccountingService s, CancellationToken c) => (await s.SetCheckStatus(p, c)).ToResult()).Produces<UResponse>();
	}
}
