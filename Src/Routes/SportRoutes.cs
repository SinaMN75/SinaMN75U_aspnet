namespace SinaMN75U.Routes;

public static class SportRoutes {
	public static void MapSportRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		r.MapPost("Sport/Create", async (SportCreateParams p, ISportService s, CancellationToken c) => (await s.CreateSport(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Sport/Read", async (SportReadParams p, ISportService s, CancellationToken c) => (await s.ReadSports(p, c)).ToResult()).Produces<UResponse<IEnumerable<SportResponse>>>();
		r.MapPost("Sport/ReadById", async (IdParams<SportSelectorArgs> p, ISportService s, CancellationToken c) => (await s.ReadSportById(p, c)).ToResult()).Produces<UResponse<SportResponse>>();
		r.MapPost("Sport/Update", async (SportUpdateParams p, ISportService s, CancellationToken c) => (await s.UpdateSport(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Sport/Delete", async (IdParams p, ISportService s, CancellationToken c) => (await s.DeleteSport(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("PlayerSportProfile/Create", async (PlayerSportProfileCreateParams p, ISportService s, CancellationToken c) => (await s.CreatePlayerSportProfile(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("PlayerSportProfile/Read", async (PlayerSportProfileReadParams p, ISportService s, CancellationToken c) => (await s.ReadPlayerSportProfiles(p, c)).ToResult()).Produces<UResponse<IEnumerable<PlayerSportProfileResponse>>>();
		r.MapPost("PlayerSportProfile/Update", async (PlayerSportProfileUpdateParams p, ISportService s, CancellationToken c) => (await s.UpdatePlayerSportProfile(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("PlayerSportProfile/Delete", async (IdParams p, ISportService s, CancellationToken c) => (await s.DeletePlayerSportProfile(p, c)).ToResult()).Produces<UResponse>();
	}
}
