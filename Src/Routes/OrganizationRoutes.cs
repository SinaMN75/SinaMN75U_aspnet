namespace SinaMN75U.Routes;

public static class OrganizationRoutes {
	public static void MapOrganizationRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>().AddEndpointFilter<ActivityLogFilter>();

		r.MapPost("Create", async (OrganizationCreateParams p, IOrganizationService s, CancellationToken c) => (await s.CreateOrganization(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Read", async (OrganizationReadParams p, IOrganizationService s, CancellationToken c) => (await s.ReadOrganizations(p, c)).ToResult()).Produces<UResponse<IEnumerable<OrganizationResponse>>>();
		r.MapPost("Update", async (OrganizationUpdateParams p, IOrganizationService s, CancellationToken c) => (await s.UpdateOrganization(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("SetMember", async (OrganizationMemberParams p, IOrganizationService s, CancellationToken c) => (await s.SetOrganizationMember(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("RemoveMember", async (OrganizationMemberParams p, IOrganizationService s, CancellationToken c) => (await s.RemoveOrganizationMember(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Shift/Create", async (StaffShiftCreateParams p, IOrganizationService s, CancellationToken c) => (await s.CreateShift(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Shift/Read", async (StaffShiftReadParams p, IOrganizationService s, CancellationToken c) => (await s.ReadShifts(p, c)).ToResult()).Produces<UResponse<IEnumerable<StaffShiftResponse>>>();
		r.MapPost("Shift/Update", async (StaffShiftUpdateParams p, IOrganizationService s, CancellationToken c) => (await s.UpdateShift(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Shift/Delete", async (IdParams p, IOrganizationService s, CancellationToken c) => (await s.DeleteShift(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Shift/Clock", async (IdParams p, IOrganizationService s, CancellationToken c) => (await s.ClockShift(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Task/Create", async (StaffTaskCreateParams p, IOrganizationService s, CancellationToken c) => (await s.CreateTask(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Task/Read", async (StaffTaskReadParams p, IOrganizationService s, CancellationToken c) => (await s.ReadTasks(p, c)).ToResult()).Produces<UResponse<IEnumerable<StaffTaskResponse>>>();
		r.MapPost("Task/Update", async (StaffTaskUpdateParams p, IOrganizationService s, CancellationToken c) => (await s.UpdateTask(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Task/Delete", async (IdParams p, IOrganizationService s, CancellationToken c) => (await s.DeleteTask(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Customer/Set", async (OrganizationCustomerSetParams p, IOrganizationService s, CancellationToken c) => (await s.SetCustomer(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Customer/Read", async (OrganizationCustomerReadParams p, IOrganizationService s, CancellationToken c) => (await s.ReadCustomers(p, c)).ToResult()).Produces<UResponse<IEnumerable<OrganizationCustomerResponse>>>();
		r.MapPost("Customer/Delete", async (IdParams p, IOrganizationService s, CancellationToken c) => (await s.DeleteCustomer(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("ActivityLog/Read", async (ActivityLogReadParams p, IOrganizationService s, CancellationToken c) => (await s.ReadActivityLogs(p, c)).ToResult()).Produces<UResponse<IEnumerable<ActivityLogResponse>>>();
	}
}
