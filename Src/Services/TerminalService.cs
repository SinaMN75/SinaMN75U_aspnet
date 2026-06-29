using DocumentFormat.OpenXml.Spreadsheet;

namespace SinaMN75U.Services;

public interface ITerminalService {
	Task<UResponse<Guid?>> Create(TerminalCreateParams p, CancellationToken ct);
	Task<UResponse> BulkCreate(TerminalBulkCreateParams p, CancellationToken ct);
	Task<UResponse<TerminalImportResponse?>> Import(TerminalImportParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<TerminalResponse>?>> Read(TerminalReadParams p, CancellationToken ct);
	Task<UResponse> Update(TerminalUpdateParams p, CancellationToken ct);
	Task<UResponse<TerminalResponse?>> Assign(TerminalAssignParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse<TerminalSupportPasswordResponse?>> ReadSupportPassword(IdParams p, CancellationToken ct);
}

public class TerminalService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IHttpClientService http,
	ISmsNotificationService sms
) : ITerminalService {
	public async Task<UResponse<Guid?>> Create(TerminalCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		TerminalEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			Serial = p.Serial,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson(),
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
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!userData.IsAdmin) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		TerminalEntity? e = await db.Set<TerminalEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("TerminalNotFound"));

		if (p.Serial.IsNotNullOrEmpty()) e.Serial = p.Serial;
		if (p.Imei.IsNotNullOrEmpty()) e.Imei = p.Imei;
		if (p.InsId.IsNotNullOrEmpty()) e.InsId = p.InsId;
		if (p.SimCardNumber.IsNotNullOrEmpty()) e.SimCardNumber = p.SimCardNumber;
		if (p.SimCardSerial.IsNotNullOrEmpty()) e.SimCardSerial = p.SimCardSerial;
		if (p.TerminalId.IsNotNullOrEmpty()) e.TerminalId = p.TerminalId;
		if (p.MerchantId.IsNotNullOrEmpty()) e.MerchantId = p.MerchantId;

		e.ApplyUpdateParam<TerminalEntity, TagTerminal, BaseJson>(p);

		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<TerminalResponse?>> Assign(TerminalAssignParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		TerminalEntity? terminal = await db.Set<TerminalEntity>().AsTracking().FirstOrDefaultAsync(x => x.Serial == p.Serial && x.SimCardSerial == p.SimCardSerial, ct);
		if (terminal == null) return new UResponse<TerminalResponse?>(null, Usc.NotFound, ls.Get("TerminalNotFoundCheckDetails"));
		MerchantEntity? merchant = await db.Set<MerchantEntity>().AsTracking().Include(x => x.User).FirstOrDefaultAsync(x => x.Id == p.MerchantId, ct);
		if (merchant == null) return new UResponse<TerminalResponse?>(null, Usc.NotFound, ls.Get("MerchantNotFound"));
		
		string agreement = await GenerateAgreement(merchant.User, terminal);

		terminal.JsonData.Detail1 = p.Title ?? "";
		terminal.MerchantId = p.MerchantId;
		terminal.Agreement = agreement.FromBase64();

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
		
		if (response is null or { IsSuccessStatusCode: false }) return new UResponse<TerminalResponse?>(null);
		JsonElement merchantData = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(ct));
		if (merchantData.GetStringOrNull("insId") == null) return new UResponse<TerminalResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));
		
		merchant.InsId = merchantData.GetStringOrNull("insId")!;
		merchant.MerchantId = merchantData.GetStringOrNull("merchantId")!;

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

		if (terminalResponse is null or { IsSuccessStatusCode: false }) return new UResponse<TerminalResponse?>(null);
		JsonElement terminalData = JsonSerializer.Deserialize<JsonElement>(await terminalResponse.Content.ReadAsStringAsync(ct));
		if (terminalData.GetStringOrNull("insId") == null) return new UResponse<TerminalResponse?>(null, Usc.ThirdPartyError, ls.Get("ThirdPartyError"));

		terminal.TerminalId = terminalData.GetStringOrNull("terminalId");
		terminal.InsId = terminalData.GetStringOrNull("insId");
		
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
			Agreement = terminal.Agreement.ToBase64(),
			MerchantId = terminal.MerchantId
		});
	}

	public async Task<UResponse> BulkCreate(TerminalBulkCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		List<TerminalEntity> entities = [];

		entities.AddRange(p.List.Select(x => new TerminalEntity {
			Serial = x.Serial,
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJson(),
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
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<TerminalEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse<TerminalSupportPasswordResponse?>> ReadSupportPassword(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

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

		if (e == null) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.NotFound, ls.Get("TerminalNotFound"));
		if (e.Merchant == null) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.NotFound, ls.Get("MerchantNotFound"));
		if (!userData.IsAdmin && userData.Id != e.CreatorId && userData.Id != e.Merchant?.UserId) return new UResponse<TerminalSupportPasswordResponse?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

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

		return new UResponse<TerminalSupportPasswordResponse?>(
			new TerminalSupportPasswordResponse { Password = data.GetStringOrNull("supportPassword") }
		);
	}
	
		// Imports terminals from an uploaded .xlsx file; duplicates are skipped and the rest still import
	public async Task<UResponse<TerminalImportResponse?>> Import(TerminalImportParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<TerminalImportResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (p.File.IsNullOrEmpty()) return new UResponse<TerminalImportResponse?>(null, Usc.BadRequest, ls.Get("FileRequired"));

		// Decode the base64 file into a seekable stream and read every row
		List<Dictionary<string, string>> rows;
		try {
			byte[] bytes = Convert.FromBase64String(StripDataUri(p.File));
			using MemoryStream ms = new(bytes);
			rows = ParseSheet(ms);
		} catch {
			return new UResponse<TerminalImportResponse?>(null, Usc.BadRequest, ls.Get("InvalidFileFormat"));
		}

		// Pull existing unique values so we can detect duplicates without hitting the DB unique indexes
		HashSet<string> serials = (await db.Set<TerminalEntity>().Select(x => x.Serial).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
		HashSet<string> imeis = (await db.Set<TerminalEntity>().Where(x => x.Imei != null).Select(x => x.Imei!).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
		HashSet<string> simSerials = (await db.Set<TerminalEntity>().Where(x => x.SimCardSerial != null).Select(x => x.SimCardSerial!).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
		HashSet<string> terminalIds = (await db.Set<TerminalEntity>().Where(x => x.TerminalId != null).Select(x => x.TerminalId!).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);

		TerminalImportResponse result = new() { TotalRows = rows.Count };
		List<TerminalEntity> toAdd = [];

		foreach (Dictionary<string, string> row in rows) {
			string serial = Val(row, "Serial");
			if (serial.IsNullOrEmpty()) { result.SkippedSerials.Add("(empty serial)"); continue; } // no serial -> ignore

			string? imei = Val(row, "Imei").NullIfEmpty();
			string? simSerial = Val(row, "SimCardSerial").NullIfEmpty();
			string? terminalId = Val(row, "TerminalId").NullIfEmpty();

			// Skip any row that collides on a unique field (already in DB or earlier in this file)
			if (serials.Contains(serial) ||
			    (imei != null && imeis.Contains(imei)) ||
			    (simSerial != null && simSerials.Contains(simSerial)) ||
			    (terminalId != null && terminalIds.Contains(terminalId))) {
				result.SkippedSerials.Add($"{serial} (duplicate)");
				continue;
			}

			// First tag is required, second is optional; both go into the tags list
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
				JsonData = new BaseJson(),
				Tags = tags,
				CreatorId = userData.Id,
				SimCardNumber = Val(row, "SimCardNumber").NullIfEmpty(),
				SimCardSerial = simSerial,
				Imei = imei,
				TerminalId = terminalId,
				InsId = Val(row, "InsId").NullIfEmpty()
			});

			// Reserve these values so later rows in the same file can't duplicate them
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

	// Reads an .xlsx sheet into a list of header->value dictionaries (first row is the header)
	private static List<Dictionary<string, string>> ParseSheet(Stream stream) {
		List<Dictionary<string, string>> rows = [];
		using SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, false);
		WorkbookPart wb = doc.WorkbookPart!;
		WorksheetPart wsPart = wb.WorksheetParts.First();
		SharedStringTablePart? sst = wb.SharedStringTablePart;

		Row[] sheetRows = wsPart.Worksheet.GetFirstChild<SheetData>()!.Elements<Row>().ToArray();
		if (sheetRows.Length == 0) return rows;

		// Map column letter (A, B, ...) -> trimmed header text
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
			if (map.Values.Any(v => v.Length > 0)) rows.Add(map); // skip fully empty rows
		}
		return rows;
	}

	// Resolves a cell's display text, handling shared strings and inline strings
	private static string CellText(Cell cell, SharedStringTablePart? sst) {
		string raw = cell.CellValue?.InnerText ?? "";
		if (cell.DataType?.Value == CellValues.SharedString && sst != null && int.TryParse(raw, out int idx))
			return sst.SharedStringTable.Elements<SharedStringItem>().ElementAt(idx).InnerText;
		if (cell.DataType?.Value == CellValues.InlineString) return cell.InnerText;
		return raw;
	}

	// Extracts the column letters from a cell reference like "B12" -> "B"
	private static string ColumnLetter(string cellRef) {
		int i = 0;
		while (i < cellRef.Length && char.IsLetter(cellRef[i])) i++;
		return cellRef[..i];
	}

	// Strips an optional "data:...;base64," prefix so a raw or data-URI base64 string both work
	private static string StripDataUri(string s) {
		int i = s.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
		return i >= 0 ? s[(i + 7)..].Trim() : s.Trim();
	}

	private static string Val(Dictionary<string, string> row, string key) => row.TryGetValue(key, out string? v) ? v : "";

	// Accepts a tag by its numeric enum value only (e.g. 101 = Atm, 102 = WallCashless, 103 = DeskCashless)
	private static bool TryParseTag(string raw, out TagTerminal tag) {
		tag = default;
		if (!int.TryParse(raw.Trim(), out int n) || !Enum.IsDefined(typeof(TagTerminal), n)) return false;
		tag = (TagTerminal)n;
		return true;
	}
	

	private async Task<string> GenerateAgreement(UserEntity user, TerminalEntity terminal) {
		return await WordPdfGenerator.GenerateWithTextsAndImagesAsync(
			texts: new Dictionary<string, string> {
				{ "day", PersianDateTime.Now.Day.ToString() },
				{ "month", PersianDateTime.Now.Month.ToString() },
				{ "number", "NUMBER" },
				{ "fullName", $"{user.FirstName ?? ""} {user.LastName ?? ""}" },
				{ "nationalCode", user.NationalCode ?? "" },
				{ "birthdate", PersianDateTime.FromDateTime(user.Birthdate ?? DateTime.Now).ToString("yyyy-MM-dd") },
				{ "address", "ADDRESS" },
				{ "postalCode", terminal.Merchant?.ZipCode ?? "" },
				{ "phoneNumber", user.PhoneNumber ?? "" },
				{ "landLine", user.LandLine ?? "" },
				{ "fatherName", user.JsonData.FatherName ?? "" }
			},
			imagesBase64: new Dictionary<string, string> {
				{ "customerSignature", user.ESignature!.ToBase64()! }
			},
			templatePath: Path.Combine(Directory.GetCurrentDirectory(), "Templates", "atmAgreement.docx")
		);
	}
}