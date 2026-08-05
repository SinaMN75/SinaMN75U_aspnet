namespace SinaMN75U.Routes;

public static class DbAdminRoutes {
	public static void MapDbAdminRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Tables", async (DbAdminTablesParams p, IDbAdminService s, CancellationToken c) => (await s.Tables(p, c)).ToResult()).Produces<UResponse<List<DbAdminTableResponse>>>();
		r.MapPost("Schema", async (DbAdminTableSchemaParams p, IDbAdminService s, CancellationToken c) => (await s.Schema(p, c)).ToResult()).Produces<UResponse<DbAdminTableSchemaResponse>>();
		r.MapPost("Rows", async (DbAdminRowsParams p, IDbAdminService s, CancellationToken c) => (await s.Rows(p, c)).ToResult()).Produces<UResponse<DbAdminQueryResultResponse>>();
		r.MapPost("Query", async (DbAdminQueryParams p, IDbAdminService s, CancellationToken c) => (await s.Query(p, c)).ToResult()).Produces<UResponse<DbAdminQueryResultResponse>>();
		r.MapPost("UpdateRow", async (DbAdminUpdateRowParams p, IDbAdminService s, CancellationToken c) => (await s.UpdateRow(p, c)).ToResult()).Produces<UResponse<DbAdminQueryResultResponse>>();
		r.MapPost("InsertRow", async (DbAdminInsertRowParams p, IDbAdminService s, CancellationToken c) => (await s.InsertRow(p, c)).ToResult()).Produces<UResponse<DbAdminQueryResultResponse>>();
		r.MapPost("DeleteRow", async (DbAdminDeleteRowParams p, IDbAdminService s, CancellationToken c) => (await s.DeleteRow(p, c)).ToResult()).Produces<UResponse>();
	}
}
