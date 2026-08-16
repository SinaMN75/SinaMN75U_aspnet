namespace SinaMN75U.Middlewares;

public sealed class TimezoneMiddleware(RequestDelegate next) {
	public async Task InvokeAsync(HttpContext context) {
		UTimeZone.Offset = ParseOffset(context.Request.Headers["Timezone"]);
		await next(context);
	}

	private static TimeSpan ParseOffset(string? value) {
		if (string.IsNullOrWhiteSpace(value)) return UTimeZone.Default;
		if (int.TryParse(value, out int minutes)) return TimeSpan.FromMinutes(minutes);
		try {
			return TimeZoneInfo.FindSystemTimeZoneById(value).GetUtcOffset(DateTime.UtcNow);
		}
		catch {
			return UTimeZone.Default;
		}
	}
}

public static class UTimeZone {
	public static readonly TimeSpan Default = TimeSpan.FromMinutes(210);
	private static readonly AsyncLocal<TimeSpan?> Holder = new();

	public static TimeSpan Offset {
		get => Holder.Value ?? Default;
		set => Holder.Value = value;
	}
}

public sealed class UDateTimeConverter : JsonConverter<DateTime> {
	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
		DateTimeOffset.TryParse(reader.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset dto)
			? dto.UtcDateTime
			: DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
		writer.WriteStringValue(new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)).ToOffset(UTimeZone.Offset).ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
}
