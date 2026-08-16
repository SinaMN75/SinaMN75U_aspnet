namespace SinaMN75U.Middlewares;

public sealed class TimezoneMiddleware(RequestDelegate next) {
	public async Task InvokeAsync(HttpContext context) {
		UTimeZone.Set(context.Request.Headers["X-Timezone"].FirstOrDefault());
		await next(context);
	}
}

public static class UTimeZone {
	public static readonly TimeZoneInfo Default = Resolve("Asia/Tehran") ?? TimeZoneInfo.Utc;
	private static readonly AsyncLocal<TimeZoneInfo?> Holder = new();
	private static readonly ConcurrentDictionary<string, TimeZoneInfo?> Cache = new();

	public static TimeZoneInfo Current => Holder.Value ?? Default;

	public static void Set(string? value) => Holder.Value = value.IsNotNullOrEmpty() ? Resolve(value) ?? Default : Default;

	private static TimeZoneInfo? Resolve(string value) => Cache.GetOrAdd(value.Trim(), key => {
		try {
			return TimeZoneInfo.FindSystemTimeZoneById(key);
		}
		catch {
			return TryParseOffset(key, out TimeSpan offset) ? TimeZoneInfo.CreateCustomTimeZone(key, offset, key, key) : null;
		}
	});

	private static bool TryParseOffset(string value, out TimeSpan offset) {
		offset = TimeSpan.Zero;
		if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int minutes)) {
			offset = TimeSpan.FromMinutes(minutes);
			return offset is { TotalMinutes: >= -840 and <= 840 };
		}

		string v = value.StartsWith('+') ? value[1..] : value;
		bool negative = v.StartsWith('-');
		if (negative) v = v[1..];
		if (TimeSpan.TryParseExact(v, [@"hh\:mm", @"h\:mm", "hh", "h"], CultureInfo.InvariantCulture, out TimeSpan ts)) {
			offset = negative ? -ts : ts;
			return offset is { TotalMinutes: >= -840 and <= 840 };
		}

		return false;
	}
}

public sealed class UDateTimeConverter : JsonConverter<DateTime> {
	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
		DateTimeOffset.TryParse(reader.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset dto)
			? dto.UtcDateTime
			: DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) {
		DateTimeOffset local = TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)), UTimeZone.Current);
		writer.WriteStringValue(local.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture));
	}
}
