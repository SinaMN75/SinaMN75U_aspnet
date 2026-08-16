namespace SinaMN75U.Middlewares;

public sealed class TimezoneMiddleware(RequestDelegate next) {
	public async Task InvokeAsync(HttpContext context) {
		UTimeZone.Offset = int.TryParse(context.Request.Headers["Timezone"], out int minutes) ? TimeSpan.FromMinutes(minutes) : UTimeZone.Default;
		await next(context);
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
		writer.WriteStringValue(new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)).ToOffset(UTimeZone.Offset).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture));
}
