namespace SinaMN75U.Routes;

public static class BankAccountRoutes {
	public static void MapBankAccountRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (BankAccountCreateParams p, IBankAccountService s, CancellationToken c) => (await s.Create(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Read", async (BankAccountReadParams p, IBankAccountService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<BankAccountResponse>>>();
		r.MapPost("Update", async (BankAccountUpdateParams p, IBankAccountService s, CancellationToken c) => (await s.Update(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Delete", async (IdParams p, IBankAccountService s, CancellationToken c) => (await s.Delete(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("SoftDelete", async (SoftDeleteParams p, IBankAccountService s, CancellationToken c) => (await s.SoftDelete(p, c)).ToResult()).Produces<UResponse>();
	}
}