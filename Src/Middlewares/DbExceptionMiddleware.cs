namespace SinaMN75U.Middlewares;

public class DbExceptionMiddleware(RequestDelegate next, ILocalizationService ls) {
	public async Task InvokeAsync(HttpContext context) {
		try {
			await next(context);
		}
		catch (DbUpdateConcurrencyException) {
			await WriteAsync(context, Usc.Conflict, ls.Get("ConcurrencyConflict"));
		}
		catch (DbUpdateException ex) {
			// EF wraps the provider error; the meaningful detail is the inner PostgresException.
			if (!await TryHandlePostgresAsync(context, ex.InnerException as PostgresException)) throw;
		}
		catch (PostgresException ex) {
			// Some queries (raw SQL, explicit commands) surface PostgresException directly.
			if (!await TryHandlePostgresAsync(context, ex)) throw;
		}
	}

	private async Task<bool> TryHandlePostgresAsync(HttpContext context, PostgresException? ex) {
		if (ex is null) return false;

		switch (ex.SqlState) {
			case "23505": // unique_violation
				await WriteAsync(context, Usc.Conflict, ResolveUniqueMessage(ex.ConstraintName));
				return true;
			case "23503": // foreign_key_violation
				await WriteAsync(context, Usc.BadRequest, ResolveForeignKeyMessage(ex.ConstraintName));
				return true;
			case "23502": // not_null_violation
				await WriteAsync(context, Usc.BadRequest, ls.Get("RequiredFieldMissing"));
				return true;
			case "23514": // check_violation
				await WriteAsync(context, Usc.BadRequest, ls.Get("InvalidValue"));
				return true;
			case "22001": // string_data_right_truncation (value too long for column)
				await WriteAsync(context, Usc.BadRequest, ls.Get("ValueTooLong"));
				return true;
			default:
				// Unknown DB error: let the outer middleware log it and return 500.
				return false;
		}
	}

	// Maps unique indexes declared on the entities to localized messages.
	private string ResolveUniqueMessage(string? constraint) => constraint switch {
		"IX_Users_Email" => ls.Get("EmailIsDuplicated"),
		"IX_Users_UserName" => ls.Get("UserNameIsDuplicated"),
		"IX_Users_PhoneNumber" => ls.Get("PhoneNumberIsDuplicated"),
		"IX_Users_NationalCode" => ls.Get("NationalCodeIsDuplicated"),
		"IX_Products_Slug" => ls.Get("SlugIsDuplicated"),
		"IX_Products_Code" => ls.Get("CodeIsDuplicated"),
		"IX_Terminal_TerminalId" => ls.Get("TerminalIdIsDuplicated"),
		"IX_Terminal_SimCardSerial" => ls.Get("SimCardSerialIsDuplicated"),
		"IX_Terminal_Imei" => ls.Get("ImeiIsDuplicated"),
		"IX_Txn_TrackingNumber" => ls.Get("TrackingNumberIsDuplicated"),
		"IX_Vehicles_NumberPlate" => ls.Get("NumberPlateIsDuplicated"),
		_ when constraint?.StartsWith("PK_", StringComparison.Ordinal) == true => ls.Get("IdIsDuplicated"),
		_ => ls.Get("DuplicateEntry")
	};

	// FK constraint names follow EF's "FK_{Dependent}_{Principal}_{Column}" pattern.
	// Matching on the column/principal keeps this resilient to table-name pluralization.
	private string ResolveForeignKeyMessage(string? constraint) {
		if (string.IsNullOrEmpty(constraint)) return ls.Get("RelatedRecordNotFound");

		if (constraint.Contains("ProductId") || constraint.Contains("_Products_")) return ls.Get("ProductNotFound");
		if (constraint.Contains("CategoryId") || constraint.Contains("_Categories_")) return ls.Get("CategoryNotFound");
		if (constraint.Contains("CommentId")) return ls.Get("CommentNotFound");
		if (constraint.Contains("ContentId")) return ls.Get("ContentNotFound");
		if (constraint.Contains("MerchantId")) return ls.Get("MerchantNotFound");
		if (constraint.Contains("TerminalId")) return ls.Get("TerminalNotFound");
		if (constraint.Contains("BankAccountId")) return ls.Get("BankAccountNotFound");
		if (constraint.Contains("AddressId")) return ls.Get("AddressNotFound");
		if (constraint.Contains("WalletId") || constraint.Contains("WalletTxnId")) return ls.Get("WalletNotFound");
		// SenderId / ReceiverId / CreatorId / UserId all reference UserEntity.
		if (constraint.Contains("UserId") || constraint.Contains("CreatorId") ||
		    constraint.Contains("SenderId") || constraint.Contains("ReceiverId") ||
		    constraint.Contains("_Users_")) return ls.Get("UserNotFound");

		return ls.Get("RelatedRecordNotFound");
	}

	private static Task WriteAsync(HttpContext context, Usc status, string message) =>
		context.Response.HasStarted
			? Task.CompletedTask
			: new UResponse(status, message).ToResult().ExecuteAsync(context);
}
