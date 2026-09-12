using DocumentFormat.OpenXml.Spreadsheet;

namespace SinaMN75U.Services;

public interface ITerminalService {
	Task<UResponse<Guid?>> Create(TerminalCreateParams p, CancellationToken ct);
	Task<UResponse> BulkCreate(TerminalBulkCreateParams p, CancellationToken ct);
	Task<UResponse<TerminalImportResponse?>> Import(TerminalImportParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<TerminalResponse>?>> Read(TerminalReadParams p, CancellationToken ct);
	Task<UResponse> Update(TerminalUpdateParams p, CancellationToken ct);
	Task<UResponse<TerminalAvailabilityResponse?>> CheckAvailability(TerminalCheckAvailabilityParams p, CancellationToken ct);
	Task<UResponse<TerminalResponse?>> Assign(TerminalAssignParams p, CancellationToken ct);
	Task<UResponse<TerminalResponse?>> Approve(IdParams p, CancellationToken ct);
	Task<UResponse> Reject(TerminalRejectParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse<TerminalSupportPasswordResponse?>> ReadSupportPassword(IdParams p, CancellationToken ct);
}

public class TerminalService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IHttpClientService http,
	ISmsNotificationService sms,
	IWebHostEnvironment env
) : ITerminalService {
	public async Task<UResponse<Guid?>> Create(TerminalCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		TerminalEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			Serial = p.Serial,
			CreatedAt = DateTime.UtcNow,
			JsonData = new TerminalJson(),
			Tags = p.Tags,
			CreatorId = p.CreatorId ?? userData.Id,
			SimCardNumber = p.SimCardNumber,
			SimCardSerial = p.SimCardSerial,
			Imei = p.Imei,
			TerminalId = p.TerminalId,
			MerchantId = p.MerchantId,
			InsId = p.InsId
		};

		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse> Update(TerminalUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<Guid?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		TerminalEntity? e = await db.Set<TerminalEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("terminalNotFound"));

		if (p.Serial.IsNotNullOrEmpty()) e.Serial = p.Serial;
		if (p.Imei.IsNotNullOrEmpty()) e.Imei = p.Imei;
		if (p.InsId.IsNotNullOrEmpty()) e.InsId = p.InsId;
		if (p.SimCardNumber.IsNotNullOrEmpty()) e.SimCardNumber = p.SimCardNumber;
		if (p.SimCardSerial.IsNotNullOrEmpty()) e.SimCardSerial = p.SimCardSerial;
		if (p.TerminalId.IsNotNullOrEmpty()) e.TerminalId = p.TerminalId;
		if (p.MerchantId.IsNotNullOrEmpty()) e.MerchantId = p.MerchantId;

		e.ApplyUpdateParam<TerminalEntity, TagTerminal, TerminalJson>(p);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<TerminalAvailabilityResponse?>> CheckAvailability(TerminalCheckAvailabilityParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalAvailabilityResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<TerminalAvailabilityResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		(TerminalEntity? terminal, MerchantEntity? merchant, Usc status, string message) = await ResolveAssignable(p.Serial, p.SimCardSerial, p.Tag, p.MerchantId, userData, ct);
		if (terminal == null || merchant == null) return new UResponse<TerminalAvailabilityResponse?>(null, status, message);

		(byte[]? _, string? path) = GenerateAgreement(merchant.User, merchant, terminal);
		if (path == null) return new UResponse<TerminalAvailabilityResponse?>(null, Usc.InternalServerError, ls.Get("generatingTheAgreementFailed"));

		return new UResponse<TerminalAvailabilityResponse?>(new TerminalAvailabilityResponse {
			Id = terminal.Id,
			Serial = terminal.Serial,
			Agreement = AgreementUrl(path)
		});
	}

	public async Task<UResponse<TerminalResponse?>> Assign(TerminalAssignParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<TerminalResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!p.AcceptedAgreement) return new UResponse<TerminalResponse?>(null, Usc.BadRequest, ls.Get("youHaveToAcceptTheAgreementToContinue"));

		(TerminalEntity? terminal, MerchantEntity? merchant, Usc status, string message) = await ResolveAssignable(p.Serial, p.SimCardSerial, p.Tag, p.MerchantId, userData, ct);
		if (terminal == null || merchant == null) return new UResponse<TerminalResponse?>(null, status, message);

		(byte[]? agreement, string? path) = GenerateAgreement(merchant.User, merchant, terminal);
		if (agreement == null || path == null) return new UResponse<TerminalResponse?>(null, Usc.InternalServerError, ls.Get("generatingTheAgreementFailed"));

		terminal.JsonData.Detail1 = p.Title ?? "";
		terminal.JsonData.Detail2 = "";
		terminal.JsonData.AgreementPath = path;
		terminal.MerchantId = merchant.Id;
		terminal.Agreement = agreement;
		SetStatus(terminal, TagTerminal.PendingApproval);

		await db.SaveChangesAsync(ct);

		return new UResponse<TerminalResponse?>(new TerminalResponse {
			Id = terminal.Id,
			CreatedAt = terminal.CreatedAt,
			JsonData = terminal.JsonData,
			Tags = terminal.Tags,
			CreatorId = terminal.CreatorId,
			Serial = terminal.Serial,
			SimCardNumber = terminal.SimCardNumber,
			SimCardSerial = terminal.SimCardSerial,
			Imei = terminal.Imei,
			TerminalId = terminal.TerminalId,
			Agreement = AgreementUrl(terminal.JsonData.AgreementPath),
			MerchantId = terminal.MerchantId
		}, Usc.Success, ls.Get("yourRequestHasBeenSubmittedAndIsAwaitingApproval"));
	}

	public async Task<UResponse<TerminalResponse?>> Approve(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<TerminalResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse<TerminalResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		TerminalEntity? terminal = await db.Set<TerminalEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (terminal == null) return new UResponse<TerminalResponse?>(null, Usc.NotFound, ls.Get("terminalNotFound"));
		if (terminal.Tags.Contains(TagTerminal.Approved)) return new UResponse<TerminalResponse?>(null, Usc.Conflict, ls.Get("thisTerminalRequestIsAlreadyApproved"));
		if (!terminal.Tags.Contains(TagTerminal.PendingApproval)) return new UResponse<TerminalResponse?>(null, Usc.BadRequest, ls.Get("thisTerminalRequestIsNotWaitingForApproval"));

		MerchantEntity? merchant = await db.Set<MerchantEntity>().AsTracking().Include(x => x.User).FirstOrDefaultAsync(x => x.Id == terminal.MerchantId, ct);
		if (merchant == null) return new UResponse<TerminalResponse?>(null, Usc.NotFound, ls.Get("merchantNotFound"));

		if (merchant.MerchantId.IsNullOrEmpty()) {
			HttpResponseMessage? response = await http.Post(
				$"{Core.App.Avreen.BaseUrl}api/mms/ing/v2/addMerchant",
				new {
					accountId = merchant.BankAccountId,
					businessTitle = merchant.JsonData.BusinessTitle,
					cityCode = merchant.CityCode,
					mcc = merchant.Mcc,
					merchantAddress = merchant.JsonData.Address,
					merchantMobileNo = merchant.PhoneNumber,
					merchantName = merchant.Title,
					merchantOwnerName = merchant.JsonData.OwnerName,
					merchantPhone = merchant.Landline,
					nationalId = merchant.NationalCode,
					ownerMobileNo = merchant.JsonData.OwnerPhoneNumber,
					postalCode = merchant.ZipCode,
					definitionTemplate = 1,
					settlementCurrency = 364
				},
				new Dictionary<string, string> { { "Authorization", $"{Core.App.Avreen.AuthHeader}" }, { "Accept", "application/json" } }
			);

			if (response is null or { IsSuccessStatusCode: false }) return new UResponse<TerminalResponse?>(null, Usc.ThirdPartyError, ls.Get("failedToRegisterMerchantInAvreen"));
			JsonElement merchantData = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
			if (merchantData.GetStringOrNull("insId") == null)
				return new UResponse<TerminalResponse?>(null, Usc.ThirdPartyError, ls.Get("merchantRegistrationSucceededButMerchantIdentifierWasNotReturnedByAvreen"));

			merchant.InsId = merchantData.GetStringOrNull("insId")!;
			merchant.MerchantId = merchantData.GetStringOrNull("merchantId")!;
		}

		HttpResponseMessage? terminalResponse = await http.Post(
			$"{Core.App.Avreen.BaseUrl}api/mms/ing/v2/defineAndBindTerminal",
			new {
				definitionTemplate = 1,
				merchantId = merchant.MerchantId,
				project = "AvaPlus",
				terminalSerial = terminal.Serial,
				terminalSerial2 = terminal.SimCardSerial
			},
			new Dictionary<string, string> { { "Authorization", $"{Core.App.Avreen.AuthHeader}" }, { "Accept", "application/json" } }
		);

		if (terminalResponse is null or { IsSuccessStatusCode: false }) return new UResponse<TerminalResponse?>(null, Usc.ThirdPartyError, ls.Get("failedToBindTerminalToMerchantInAvreen"));
		JsonElement terminalData = JsonSerializer.Deserialize<JsonElement>(await terminalResponse.Content.ReadAsStringAsync(ct));
		if (terminalData.GetStringOrNull("insId") == null) return new UResponse<TerminalResponse?>(null, Usc.ThirdPartyError, ls.Get("failedToBindTerminalToMerchantInAvreen"));

		terminal.TerminalId = terminalData.GetStringOrNull("terminalId");
		terminal.InsId = terminalData.GetStringOrNull("insId");
		SetStatus(terminal, TagTerminal.Approved);

		await db.SaveChangesAsync(ct);

		return new UResponse<TerminalResponse?>(new TerminalResponse {
			Id = terminal.Id,
			CreatedAt = terminal.CreatedAt,
			JsonData = terminal.JsonData,
			Tags = terminal.Tags,
			CreatorId = terminal.CreatorId,
			Serial = terminal.Serial,
			SimCardNumber = terminal.SimCardNumber,
			SimCardSerial = terminal.SimCardSerial,
			Imei = terminal.Imei,
			TerminalId = terminal.TerminalId,
			Agreement = AgreementUrl(terminal.JsonData.AgreementPath),
			MerchantId = terminal.MerchantId
		});
	}

	public async Task<UResponse> Reject(TerminalRejectParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (!userData.IsAdmin) return new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		TerminalEntity? terminal = await db.Set<TerminalEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (terminal == null) return new UResponse(Usc.NotFound, ls.Get("terminalNotFound"));
		if (terminal.Tags.Contains(TagTerminal.Approved)) return new UResponse(Usc.Conflict, ls.Get("thisTerminalRequestIsAlreadyApproved"));

		terminal.JsonData.Detail2 = p.Reason ?? "";
		SetStatus(terminal, TagTerminal.Rejected);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	private async Task<(TerminalEntity? Terminal, MerchantEntity? Merchant, Usc Status, string Message)> ResolveAssignable(
		string serial,
		string? simCardSerial,
		TagTerminal tag,
		Guid? merchantId,
		JwtClaimData userData,
		CancellationToken ct
	) {
		TerminalEntity? terminal = await db.Set<TerminalEntity>().AsTracking().FirstOrDefaultAsync(x => x.Serial == serial, ct);
		if (terminal == null || tag != TagTerminal.Ava104 && terminal.SimCardSerial != simCardSerial)
			return (null, null, Usc.NotFound, ls.Get("terminalNotFoundCheckYourDetails"));

		MerchantEntity? merchant = await db.Set<MerchantEntity>().AsTracking().Include(x => x.User).FirstOrDefaultAsync(x => x.Id == merchantId, ct);
		if (merchant == null) return (null, null, Usc.NotFound, ls.Get("merchantNotFound"));
		if (!userData.IsAdmin && merchant.UserId != userData.Id) return (null, null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		if (terminal.Tags.Contains(TagTerminal.Approved)) return (null, null, Usc.Conflict, ls.Get("terminalIsAlreadyAssignedToAMerchant"));
		if (terminal.Tags.Contains(TagTerminal.PendingApproval)) return (null, null, Usc.Conflict, ls.Get("thisTerminalRequestIsWaitingForApproval"));
		if (terminal.MerchantId.IsNotNullOrEmpty() && terminal.MerchantId != merchant.Id) return (null, null, Usc.Conflict, ls.Get("terminalIsAlreadyAssignedToAMerchant"));
		if (merchant.User.ESignature.IsNullOrEmpty()) return (null, null, Usc.BadRequest, ls.Get("userElectronicSignatureIsMissing"));

		return (terminal, merchant, Usc.Success, "");
	}

	private static void SetStatus(TerminalEntity terminal, TagTerminal status) =>
		terminal.Tags = terminal.Tags.Where(x => x is not (TagTerminal.PendingApproval or TagTerminal.Approved or TagTerminal.Rejected)).Append(status).ToList();

	public async Task<UResponse> BulkCreate(TerminalBulkCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		List<TerminalEntity> entities = [];

		entities.AddRange(p.List.Select(x => new TerminalEntity {
			Serial = x.Serial,
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new TerminalJson(),
			Tags = x.Tags,
			CreatorId = userData.Id
		}));

		await db.Set<TerminalEntity>().AddRangeAsync(entities, ct);
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse<IEnumerable<TerminalResponse>?>> Read(TerminalReadParams p, CancellationToken ct) {
		IQueryable<TerminalEntity> q = db.Set<TerminalEntity>().ApplyReadParams(p);

		if (p.Serial.IsNotNullOrEmpty()) q = q.Where(x => x.Serial == p.Serial);
		if (p.TerminalId.IsNotNullOrEmpty()) q = q.Where(x => x.TerminalId == p.TerminalId);
		if (p.MerchantId.IsNotNullOrEmpty()) q = q.Where(x => x.MerchantId == p.MerchantId);
		if (p.Imei.IsNotNullOrEmpty()) q = q.Where(x => x.Imei == p.Imei);
		if (p.SimCardNumber.IsNotNullOrEmpty()) q = q.Where(x => x.SimCardNumber == p.SimCardNumber);
		if (p.SimCardSerial.IsNotNullOrEmpty()) q = q.Where(x => x.SimCardSerial == p.SimCardSerial);

		IQueryable<TerminalResponse> projected = q.Select(Projections.TerminalSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));

		await db.Set<TerminalEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse<TerminalSupportPasswordResponse?>> ReadSupportPassword(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));

		TerminalEntity? e = await db.Set<TerminalEntity>().Select(x => new TerminalEntity {
			Serial = x.Serial,
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			JsonData = x.JsonData,
			Tags = x.Tags,
			CreatorId = x.CreatorId,
			InsId = x.InsId,
			TerminalId = x.TerminalId,
			Merchant = new MerchantEntity {
				ZipCode = "",
				CityCode = "",
				PhoneNumber = "",
				Title = "",
				Landline = "",
				NationalCode = "",
				Mcc = "",
				UserId = x.Merchant!.UserId,
				Id = x.Merchant.Id,
				CreatedAt = x.Merchant.CreatedAt,
				JsonData = x.Merchant.JsonData,
				Tags = x.Merchant.Tags,
				CreatorId = x.Merchant.CreatorId,
				InsId = x.InsId,
				MerchantId = x.Merchant.MerchantId
			}
		}).FirstOrDefaultAsync(x => x.Id == p.Id, ct);

		if (e == null) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.NotFound, ls.Get("terminalNotFound"));
		if (e.Merchant == null) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.NotFound, ls.Get("merchantNotFound"));
		if (!userData.IsAdmin && userData.Id != e.CreatorId && userData.Id != e.Merchant?.UserId) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction"));

		HttpResponseMessage? response = await http.Post(
			$"{Core.App.Avreen.BaseUrl}api/mms/ing/v2/generateSupportPassword",
			new {
				insId = e.InsId,
				merchantId = e.Merchant!.MerchantId,
				terminalId = e.TerminalId,
				terminalSerial = e.Serial,
				terminalSerial2 = e.SimCardSerial
			},
			new Dictionary<string, string> { { "Authorization", $"{Core.App.Avreen.AuthHeader}" }, { "Accept", "application/json" } }
		);

		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<TerminalSupportPasswordResponse?>(null);

		JsonElement data = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));

		await sms.SendSms(new SmsNotificationParams {
			Mobile = userData.PhoneNumber!,
			Template = Core.App.SmsPanel.SupportPasswordOtp,
			Text = "12345"
		});

		return new UResponse<TerminalSupportPasswordResponse?>(new TerminalSupportPasswordResponse { Password = data.GetStringOrNull("supportPassword") });
	}

	public async Task<UResponse<TerminalImportResponse?>> Import(TerminalImportParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalImportResponse?>(null, Usc.UnAuthorized, ls.Get("pleaseSignInToContinue"));
		if (userData.IsExpired) return new UResponse<TerminalImportResponse?>(null, Usc.ExpiredToken, ls.Get("authTokenIsExpired"));
		if (p.File.IsNullOrEmpty()) return new UResponse<TerminalImportResponse?>(null, Usc.BadRequest, ls.Get("FileRequired"));

		List<Dictionary<string, string>> rows;
		try {
			byte[] bytes = Convert.FromBase64String(StripDataUri(p.File));
			using MemoryStream ms = new(bytes);
			rows = ParseSheet(ms);
		}
		catch {
			return new UResponse<TerminalImportResponse?>(null, Usc.BadRequest, ls.Get("InvalidFileFormat"));
		}

		HashSet<string> serials = (await db.Set<TerminalEntity>().Select(x => x.Serial).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
		HashSet<string> imeis = (await db.Set<TerminalEntity>().Where(x => x.Imei != null).Select(x => x.Imei!).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
		HashSet<string> simSerials = (await db.Set<TerminalEntity>().Where(x => x.SimCardSerial != null).Select(x => x.SimCardSerial!).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
		HashSet<string> terminalIds = (await db.Set<TerminalEntity>().Where(x => x.TerminalId != null).Select(x => x.TerminalId!).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);

		TerminalImportResponse result = new() { TotalRows = rows.Count };
		List<TerminalEntity> toAdd = [];

		foreach (Dictionary<string, string> row in rows) {
			string serial = Val(row, "Serial");
			if (serial.IsNullOrEmpty()) {
				result.SkippedSerials.Add("(empty serial)");
				continue;
			}

			string? imei = Val(row, "Imei").NullIfEmpty();
			string? simSerial = Val(row, "SimCardSerial").NullIfEmpty();
			string? terminalId = Val(row, "TerminalId").NullIfEmpty();

			if (serials.Contains(serial) ||
			    (imei != null && imeis.Contains(imei)) ||
			    (simSerial != null && simSerials.Contains(simSerial)) ||
			    (terminalId != null && terminalIds.Contains(terminalId))) {
				result.SkippedSerials.Add($"{serial} (duplicate)");
				continue;
			}

			if (!TryParseTag(Val(row, "Tag1"), out TagTerminal tag1)) {
				result.SkippedSerials.Add($"{serial} (missing/invalid Tag1)");
				continue;
			}

			List<TagTerminal> tags = [tag1];
			if (TryParseTag(Val(row, "Tag2"), out TagTerminal tag2) && tag2 != tag1) tags.Add(tag2);

			toAdd.Add(new TerminalEntity {
				Id = Guid.CreateVersion7(),
				Serial = serial,
				CreatedAt = DateTime.UtcNow,
				JsonData = new TerminalJson(),
				Tags = tags,
				CreatorId = userData.Id,
				SimCardNumber = Val(row, "SimCardNumber").NullIfEmpty(),
				SimCardSerial = simSerial,
				Imei = imei,
				TerminalId = terminalId,
				InsId = Val(row, "InsId").NullIfEmpty()
			});

			serials.Add(serial);
			if (imei != null) imeis.Add(imei);
			if (simSerial != null) simSerials.Add(simSerial);
			if (terminalId != null) terminalIds.Add(terminalId);
		}

		if (toAdd.Count > 0) {
			await db.Set<TerminalEntity>().AddRangeAsync(toAdd, ct);
			await db.SaveChangesAsync(ct);
		}

		result.Imported = toAdd.Count;
		result.Skipped = result.TotalRows - result.Imported;
		return new UResponse<TerminalImportResponse?>(result, Usc.Success, ls.Get("ImportCompleted"));
	}

	private static List<Dictionary<string, string>> ParseSheet(Stream stream) {
		List<Dictionary<string, string>> rows = [];
		using SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, false);
		WorkbookPart wb = doc.WorkbookPart!;
		WorksheetPart wsPart = wb.WorksheetParts.First();
		SharedStringTablePart? sst = wb.SharedStringTablePart;

		Row[] sheetRows = wsPart.Worksheet!.GetFirstChild<SheetData>()!.Elements<Row>().ToArray();
		if (sheetRows.Length == 0) return rows;

		Dictionary<string, string> headers = new(StringComparer.OrdinalIgnoreCase);
		foreach (Cell c in sheetRows[0].Elements<Cell>()) {
			string text = CellText(c, sst).Trim();
			if (text.Length > 0) headers[ColumnLetter(c.CellReference!.Value!)] = text;
		}

		foreach (Row r in sheetRows.Skip(1)) {
			Dictionary<string, string> map = new(StringComparer.OrdinalIgnoreCase);
			foreach (Cell c in r.Elements<Cell>()) {
				string col = ColumnLetter(c.CellReference!.Value!);
				if (headers.TryGetValue(col, out string? header)) map[header] = CellText(c, sst).Trim();
			}

			if (map.Values.Any(v => v.Length > 0)) rows.Add(map);
		}

		return rows;
	}

	private static string CellText(Cell cell, SharedStringTablePart? sst) {
		string raw = cell.CellValue?.InnerText ?? "";
		if (cell.DataType?.Value == CellValues.SharedString && sst != null && int.TryParse(raw, out int idx))
			return sst.SharedStringTable!.Elements<SharedStringItem>().ElementAt(idx).InnerText;
		return cell.DataType?.Value == CellValues.InlineString ? cell.InnerText : raw;
	}

	private static string ColumnLetter(string cellRef) {
		int i = 0;
		while (i < cellRef.Length && char.IsLetter(cellRef[i])) i++;
		return cellRef[..i];
	}

	private static string StripDataUri(string s) {
		int i = s.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
		return i >= 0 ? s[(i + 7)..].Trim() : s.Trim();
	}

	private static string Val(Dictionary<string, string> row, string key) => row.GetValueOrDefault(key, "");

	private static bool TryParseTag(string raw, out TagTerminal tag) {
		tag = default;
		if (!int.TryParse(raw.Trim(), out int n) || !Enum.IsDefined(typeof(TagTerminal), n)) return false;
		tag = (TagTerminal)n;
		return true;
	}

	private static string? AgreementUrl(string? path) => path == null ? null : $"{Core.App.BaseUrl}/Media/{path}";

	private (byte[]? Pdf, string? Path) GenerateAgreement(UserEntity user, MerchantEntity merchant, TerminalEntity terminal) {
		try {
			byte[] bytes = AgreementPdf.Build(new Dictionary<string, string> {
				{ "day", PersianDateTime.Now.Day.ToString() },
				{ "month", PersianDateTime.Now.Month.ToString() },
				{ "number", terminal.Serial },
				{ "fullName", $"{user.FirstName ?? "---"} {user.LastName ?? "---"}" },
				{ "nationalCode", user.NationalCode ?? "---" },
				{ "birthdate", PersianDateTime.FromDateTime(user.Birthdate ?? DateTime.Now).ToString("yyyy-MM-dd") },
				{ "address", merchant.JsonData.Address ?? "---" },
				{ "postalCode", merchant.ZipCode },
				{ "phoneNumber", user.PhoneNumber ?? "---" },
				{ "landLine", user.LandLine ?? "---" },
				{ "fatherName", user.JsonData.FatherName ?? "---" }
			}, ReadSignature(user));

			return (bytes, UserFileStore.SaveBytes(env.WebRootPath, user.Id, $"terminalAgreement_{terminal.Id}.pdf", bytes));
		}
		catch (Exception ex) {
			ULog.Error(ex, $"Generating the agreement failed for terminal {terminal.Id}");
			return (null, null);
		}
	}

	private byte[]? ReadSignature(UserEntity user) {
		if (user.ESignature.IsNullOrEmpty()) return null;
		string path = Path.Combine(env.WebRootPath, "Media", user.ESignature.TrimStart('/', '\\'));
		return File.Exists(path) ? File.ReadAllBytes(path) : null;
	}
}
