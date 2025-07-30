namespace SinaMN75U.Routes;

public static class DashboardRoutes {
	public static void MapDashboardRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("ReadSystemMetrics", async (IDashboardService s) => await s.ReadSystemMetrics()).Produces<SystemMetricsResponse>();
		r.MapPost("Read", async (IDashboardService s, CancellationToken ct) => await s.ReadDashboardData(ct)).Produces<DashboardResponse>();
		r.MapGet("Enums", () => {
			Dictionary<string, IEnumerable<IdTitleParams>> result = new() {
				[nameof(Usc)] = EnumExtensions.GetValues<Usc>(),
				[nameof(TagUser)] = EnumExtensions.GetValues<TagUser>(),
				[nameof(TagCategory)] = EnumExtensions.GetValues<TagCategory>(),
				[nameof(TagMedia)] = EnumExtensions.GetValues<TagMedia>(),
				[nameof(TagProduct)] = EnumExtensions.GetValues<TagProduct>(),
				[nameof(TagComment)] = EnumExtensions.GetValues<TagComment>(),
				[nameof(TagReaction)] = EnumExtensions.GetValues<TagReaction>()
			};

			return Results.Ok(result);
		});
		
		r.MapPost("/api/logs/structure", () => {
			string logPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Logs");

			if (!Directory.Exists(logPath))
				return Results.NotFound("Logs directory not found");

			List<YearLog> structure = GetStructuredLogDirectory(logPath);
			return Results.Ok(new { logs = structure });
		});

		r.MapPost("/api/logs/content", (LogFileRequest request) => {
			if (string.IsNullOrEmpty(request.Id))
				return Results.BadRequest("Id parameter is required");

			if (request.Id.Length < 8 || !request.Id.EndsWith("success") && !request.Id.EndsWith("failed"))
				return Results.BadRequest("Invalid ID format");

			string status = request.Id.EndsWith("success") ? "success" : "failed";
			string datePart = request.Id.Substring(0, request.Id.Length - status.Length);

			if (datePart.Length < 7)
				return Results.BadRequest("Invalid date format in ID");

			string year = datePart.Substring(0, 4);
			string month = datePart.Substring(4, 2);
			string day = datePart.Substring(6);

			string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Logs", year, month, $"{day}_{status}.json");

			if (!File.Exists(filePath))
				return Results.NotFound("Log file not found");

			try {
				string fileContent = File.ReadAllText(filePath);
				object? jsonContent = JsonSerializer.Deserialize<object>(fileContent);
				return Results.Ok(jsonContent);
			}
			catch (Exception ex) {
				return Results.Problem($"Error reading log file: {ex.Message}");
			}
		});
	}
	
	private static List<YearLog> GetStructuredLogDirectory(string path) {
		List<YearLog> result = [];

		foreach (string yearDir in Directory.GetDirectories(path)) {
			string yearName = Path.GetFileName(yearDir);
			if (!int.TryParse(yearName, out int year)) continue;

			YearLog yearLog = new() {
				Year = year,
				Months = []
			};

			foreach (string monthDir in Directory.GetDirectories(yearDir)) {
				string monthName = Path.GetFileName(monthDir);
				if (!int.TryParse(monthName, out int month)) continue;

				MonthLog monthLog = new() {
					Month = month,
					Days = []
				};

				foreach (string file in Directory.GetFiles(monthDir)) {
					string fileName = Path.GetFileNameWithoutExtension(file);
					string[] parts = fileName.Split('_');
					if (parts.Length != 2 || !int.TryParse(parts[0], out int day)) continue;

					DayLog? existingDay = monthLog.Days.FirstOrDefault(d => d.Day == day);
					if (existingDay == null) {
						existingDay = new DayLog { Day = day };
						monthLog.Days.Add(existingDay);
					}

					if (parts[1] == "success")
						existingDay.Success = $"{year}{month:00}{day:00}success";
					else if (parts[1] == "failed")
						existingDay.Failed = $"{year}{month:00}{day:00}failed";
				}

				if (monthLog.Days.Count == 0) continue;
				{
					monthLog.Days = monthLog.Days.OrderBy(d => d.Day).ToList();
					yearLog.Months.Add(monthLog);
				}
			}

			if (yearLog.Months.Count == 0) continue;
			yearLog.Months = yearLog.Months.OrderBy(m => m.Month).ToList();
			result.Add(yearLog);
		}

		return result.OrderBy(y => y.Year).ToList();
	}
	
	private class LogFileRequest {
		public string Id { get; set; } = string.Empty;
	}

	private class YearLog {
		public int Year { get; set; }
		public List<MonthLog> Months { get; set; } = new();
	}

	private class MonthLog {
		public int Month { get; set; }
		public List<DayLog> Days { get; set; } = new();
	}

	private class DayLog {
		public int Day { get; set; }
		public string? Success { get; set; }
		public string? Failed { get; set; }
	}
}