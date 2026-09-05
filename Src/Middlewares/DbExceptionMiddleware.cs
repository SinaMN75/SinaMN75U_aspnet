namespace SinaMN75U.Middlewares;

public class DbExceptionMiddleware(RequestDelegate next, ILocalizationService ls) {
	public async Task InvokeAsync(HttpContext context) {
		try {
			await next(context);
		}
		catch (DbUpdateConcurrencyException ex) {
			context.Items[UConstants.ApiLogExceptionKey] = ex;
			await WriteAsync(context, Usc.Conflict, ls.Get("thisRecordWasModifiedByAnotherOperationPleaseRetry"));
		}
		catch (DbUpdateException ex) {
			context.Items[UConstants.ApiLogExceptionKey] = ex;
			if (!await TryHandlePostgresAsync(context, ex.InnerException as PostgresException)) throw;
		}
		catch (PostgresException ex) {
			context.Items[UConstants.ApiLogExceptionKey] = ex;
			if (!await TryHandlePostgresAsync(context, ex)) throw;
		}
	}

	private async Task<bool> TryHandlePostgresAsync(HttpContext context, PostgresException? ex) {
		if (ex is null) return false;
		switch (ex.SqlState) {
			case "23505":
				await WriteAsync(context, Usc.Conflict, ResolveUniqueMessage(ex.ConstraintName));
				return true;
			case "23503":
				await WriteAsync(context, Usc.BadRequest, ResolveForeignKeyMessage(ex.ConstraintName));
				return true;
			case "23502":
				await WriteAsync(context, Usc.BadRequest, ls.Get("aRequiredFieldIsMissing"));
				return true;
			case "23514":
				await WriteAsync(context, Usc.BadRequest, ls.Get("oneOfTheSubmittedValuesIsInvalid"));
				return true;
			case "22001":
				await WriteAsync(context, Usc.BadRequest, ls.Get("oneOfTheSubmittedValuesIsTooLong"));
				return true;
			default:
				return false;
		}
	}

	private string ResolveUniqueMessage(string? constraint) => constraint switch {
		"IX_Users_Email" => ls.Get("thisEmailAlreadyExists"),
		"IX_Users_UserName" => ls.Get("thisUserNameAlreadyExists"),
		"IX_Users_PhoneNumber" => ls.Get("thisPhoneNumberAlreadyExists"),
		"IX_Users_NationalCode" => ls.Get("thisNationalCodeAlreadyExists"),
		"IX_Products_Slug" => ls.Get("thisSlugAlreadyExists"),
		"IX_Products_Code" => ls.Get("thisCodeAlreadyExists"),
		"IX_Terminal_TerminalId" => ls.Get("thisTerminalIdAlreadyExists"),
		"IX_Terminal_SimCardSerial" => ls.Get("thisSimCardSerialAlreadyExists"),
		"IX_Terminal_Imei" => ls.Get("thisIMEIAlreadyExists"),
		"IX_Txn_TrackingNumber" => ls.Get("thisTrackingNumberAlreadyExists"),
		"IX_Vehicles_NumberPlate" => ls.Get("thisNumberPlateAlreadyExists"),
		_ when constraint?.StartsWith("PK_", StringComparison.Ordinal) == true => ls.Get("thisIdAlreadyExists"),
		_ => ls.Get("thisRecordAlreadyExists")
	};
	
	private string ResolveForeignKeyMessage(string? constraint) {
		if (string.IsNullOrEmpty(constraint)) return ls.Get("aRelatedRecordWasNotFound");
		if (constraint.Contains("ProductId") || constraint.Contains("_Products_")) return ls.Get("productNotFoundPleaseCheckDetails");
		if (constraint.Contains("CategoryId") || constraint.Contains("_Categories_")) return ls.Get("categoryNotFoundPleaseTryAnother");
		if (constraint.Contains("CommentId")) return ls.Get("commentNotFound");
		if (constraint.Contains("ContentId")) return ls.Get("contentNotFound");
		if (constraint.Contains("MerchantId")) return ls.Get("merchantNotFound");
		if (constraint.Contains("TerminalId")) return ls.Get("terminalNotFound");
		if (constraint.Contains("BankAccountId")) return ls.Get("bankAccountNotFound");
		if (constraint.Contains("AddressId")) return ls.Get("addressNotFound");
		if (constraint.Contains("WalletId") || constraint.Contains("WalletTxnId")) return ls.Get("walletNotFound");
		if (constraint.Contains("UserId") || constraint.Contains("CreatorId") || constraint.Contains("SenderId") || constraint.Contains("ReceiverId") || constraint.Contains("_Users_")) return ls.Get("accountNotFound");
		return ls.Get("aRelatedRecordWasNotFound");
	}

	private static Task WriteAsync(HttpContext context, Usc status, string message) => context.Response.HasStarted ? Task.CompletedTask : new UResponse(status, message).ToResult().ExecuteAsync(context);
}
