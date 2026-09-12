namespace SinaMN75U.Routes;

public static class TerminalRoutes {
	public static void MapTerminalRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Create", async (TerminalCreateParams p, ITerminalService s, CancellationToken c) => (await s.Create(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("BulkCreate", async (TerminalBulkCreateParams p, ITerminalService s, CancellationToken c) => (await s.BulkCreate(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Import", async (TerminalImportParams p, ITerminalService s, CancellationToken c) => (await s.Import(p, c)).ToResult()).Produces<UResponse<TerminalImportResponse>>();
		r.MapPost("Read", async (TerminalReadParams p, ITerminalService s, CancellationToken c) => (await s.Read(p, c)).ToResult()).Produces<UResponse<IEnumerable<TerminalResponse>>>();
		r.MapPost("Delete", async (IdParams p, ITerminalService s, CancellationToken c) => (await s.Delete(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("ReadSupportPassword", async (IdParams p, ITerminalService s, CancellationToken c) => (await s.ReadSupportPassword(p, c)).ToResult()).Produces<UResponse<TerminalSupportPasswordResponse>>();
		r.MapPost("CheckAvailability", async (TerminalCheckAvailabilityParams p, ITerminalService s, CancellationToken c) => (await s.CheckAvailability(p, c)).ToResult()).Produces<UResponse<TerminalAvailabilityResponse>>();
		r.MapPost("Assign", async (TerminalAssignParams p, ITerminalService s, CancellationToken c) => (await s.Assign(p, c)).ToResult()).Produces<UResponse<TerminalResponse>>();
		r.MapPost("Approve", async (IdParams p, ITerminalService s, CancellationToken c) => (await s.Approve(p, c)).ToResult()).Produces<UResponse<TerminalResponse>>();
		r.MapPost("Reject", async (TerminalRejectParams p, ITerminalService s, CancellationToken c) => (await s.Reject(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Update", async (TerminalUpdateParams p, ITerminalService s, CancellationToken c) => (await s.Update(p, c)).ToResult()).Produces<UResponse>();
		
		r.MapPost("CreateBrand", async (TerminalBrandCreateParams p, ITerminalService s, CancellationToken c) => (await s.CreateBrand(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("ReadBrand", async (TerminalBrandReadParams p, ITerminalService s, CancellationToken c) => (await s.ReadBrand(p, c)).ToResult()).Produces<UResponse<IEnumerable<TerminalBrandResponse>>>();
		r.MapPost("UpdateBrand", async (TerminalBrandUpdateParams p, ITerminalService s, CancellationToken c) => (await s.UpdateBrand(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DeleteBrand", async (IdParams p, ITerminalService s, CancellationToken c) => (await s.DeleteBrand(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("CreateBroker", async (TerminalBrokerCreateParams p, ITerminalService s, CancellationToken c) => (await s.CreateBroker(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("ReadBroker", async (TerminalBrokerReadParams p, ITerminalService s, CancellationToken c) => (await s.ReadBroker(p, c)).ToResult()).Produces<UResponse<IEnumerable<TerminalBrokerResponse>>>();
		r.MapPost("UpdateBroker", async (TerminalBrokerUpdateParams p, ITerminalService s, CancellationToken c) => (await s.UpdateBroker(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("DeleteBroker", async (IdParams p, ITerminalService s, CancellationToken c) => (await s.DeleteBroker(p, c)).ToResult()).Produces<UResponse>();
	}
}