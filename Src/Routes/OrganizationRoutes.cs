namespace SinaMN75U.Routes;

public static class OrganizationRoutes {
	public static void MapOrganizationRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		r.MapPost("Create", async (OrganizationCreateParams p, IOrganizationService s, CancellationToken c) => (await s.CreateOrganization(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Read", async (OrganizationReadParams p, IOrganizationService s, CancellationToken c) => (await s.ReadOrganizations(p, c)).ToResult()).Produces<UResponse<IEnumerable<OrganizationResponse>>>();
		r.MapPost("Update", async (OrganizationUpdateParams p, IOrganizationService s, CancellationToken c) => (await s.UpdateOrganization(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("SetMember", async (OrganizationMemberParams p, IOrganizationService s, CancellationToken c) => (await s.SetOrganizationMember(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("RemoveMember", async (OrganizationMemberParams p, IOrganizationService s, CancellationToken c) => (await s.RemoveOrganizationMember(p, c)).ToResult()).Produces<UResponse>();
	}
}
