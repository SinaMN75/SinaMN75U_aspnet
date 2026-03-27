using Npgsql;

namespace SinaMN75U.Middlewares;

public class DbExceptionMiddleware(RequestDelegate next, ILocalizationService ls) {
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
			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			await context.Response.WriteAsJsonAsync(new UResponse(
					status: Usc.Conflict,
					message: pgEx.ConstraintName switch {
						"IX_Id" => ls.Get("IdIsDuplicated"),
						"IX_Products_Slug" => ls.Get("SlugIsDuplicated"),
						"IX_Products_Code" => ls.Get("CodeIsDuplicated"),
						"IX_Terminal_SimCardNumber" => ls.Get("SimCardNumberIsDuplicated"),
						"IX_Terminal_SimCardSerial" => ls.Get("SimCardSerialIsDuplicated"),
						"IX_Terminal_Imei" => ls.Get("ImeiIsDuplicated"),
						"IX_Txn_TrackingNumber" => ls.Get("TrackingNumberIsDuplicated"),
						"IX_Users_Email" => ls.Get("EmailIsDuplicated"),
						"IX_Users_UserName" => ls.Get("UserNameIsDuplicated"),
						"IX_Users_PhoneNumber" => ls.Get("PhoneNumberIsDuplicated"),
						"IX_Users_NationalCode" => ls.Get("NationalCodeIsDuplicated"),
						"IX_Vehicles_NumberPlate" => ls.Get("NumberPlateIsDuplicated"),
						_ => "A unique constraint violation occurred."
					}
				)
			);
			return;
		}

		context.Response.StatusCode = StatusCodes.Status500InternalServerError;
		await context.Response.WriteAsJsonAsync(new UResponse { Message = ls.Get("SystemErrorAccorded") });
	}
}