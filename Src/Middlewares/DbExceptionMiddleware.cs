using Npgsql;

namespace SinaMN75U.Middlewares;

public class DbExceptionMiddleware(
	RequestDelegate next,
	ILogger<DbExceptionMiddleware> logger,
	ILocalizationService ls
) {
	public async Task InvokeAsync(HttpContext context) {
		try {
			await next(context);
		}
		catch (DbUpdateException ex) {
			await HandleDbUpdateExceptionAsync(context, ex);
		}
	}

	private async Task HandleDbUpdateExceptionAsync(HttpContext context, DbUpdateException ex) {
		if (ex.InnerException is PostgresException { SqlState: "23505" } pgEx) {
			UResponse response = new(
				Usc.Conflict,
				message: pgEx.ConstraintName switch {
					"IX_Products_Slug" => ls.Get("SlugIsDuplicated"),
					"IX_Products_Code" => ls.Get("CodeIsDuplicated"),
					_ => "A unique constraint violation occurred."
				}
			);

			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			await context.Response.WriteAsJsonAsync(response);
			return;
		}

		context.Response.StatusCode = StatusCodes.Status500InternalServerError;
		await context.Response.WriteAsJsonAsync(new UResponse { Message = ls.Get("SystemErrorAccorded") });
	}
}