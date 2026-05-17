using Core = SinaMN75U.Constants.Core;
using Lock = System.Threading.Lock;

namespace SinaMN75U.InnerServices;

public interface IRequestLogger {
	void TryLog(RequestLogDto log);
}

public sealed class RequestLogger : IRequestLogger {
	private static readonly Lock LogLock = new();

	public void TryLog(RequestLogDto log) {
		if (!Core.App.Middleware.Log) return;
		if (log.StatusCode is >= 200 and <= 299 && !Core.App.Middleware.LogSuccess) return;

		string rawReq = log.RawRequest;
		string decodedReq = log.DecodedRequest;
		string res = log.Response;

		const int maxLen = 10_000;
		if (rawReq.Length > maxLen) rawReq = rawReq[..maxLen] + "...<truncated>";
		if (decodedReq.Length > maxLen) decodedReq = decodedReq[..maxLen] + "...<truncated>";
		if (res.Length > maxLen) res = res[..maxLen] + "...<truncated>";

		LogToFile(
			log.Timestamp,
			log.Method,
			log.Path,
			log.StatusCode,
			log.DurationMs,
			rawReq,
			decodedReq,
			res,
			log.Exception
		);
	}

	private static void LogToFile(DateTime ts, string method, string path, int status, long ms, string rawReq, string decodedReq, string res, Exception? ex) {
		try {
			DateTime now = DateTime.Now;
			string dir = Path.Combine("wwwroot", "Logs", now.Year.ToString(), $"{now.Month:00}");
			Directory.CreateDirectory(dir);
			string file = Path.Combine(dir, $"{now:dd}_{(status < 300 ? "success" : "failed")}.json");

			var entry = new {
				summary = $"{ts:yyyy-MM-dd HH:mm:ss} | {method} {path} | {status} | {ms}ms",
				requestBodyRaw = TryParseJson(rawReq) ?? rawReq,
				requestBody = TryParseJson(decodedReq) ?? decodedReq,
				responseBody = TryParseJson(res) ?? res,
				exception = ex is null ? null : new { type = ex.GetType().Name, message = ex.Message, stackTrace = ex.StackTrace }
			};

			lock (LogLock) {
				List<object> list = File.Exists(file)
					? JsonSerializer.Deserialize<List<object>>(File.ReadAllText(file), Core.Default) ?? []
					: [];

				list.Add(entry);

				File.WriteAllText(file, JsonSerializer.Serialize(list, Core.Default));
			}
		}
		catch {
			/* ignore */
		}
	}

	private static object? TryParseJson(string s) {
		try {
			return JsonElementToDynamic(JsonDocument.Parse(s).RootElement);
		}
		catch {
			return null;
		}
	}

	private static object? JsonElementToDynamic(JsonElement e) => e.ValueKind switch {
		JsonValueKind.Object => e.EnumerateObject().ToDictionary(p => p.Name, p => JsonElementToDynamic(p.Value)),
		JsonValueKind.Array => e.EnumerateArray().Select(JsonElementToDynamic).ToList(),
		JsonValueKind.String => e.GetString(),
		JsonValueKind.Number => e.TryGetInt64(out long l) ? l : e.GetDouble(),
		JsonValueKind.True => true,
		JsonValueKind.False => false,
		_ => null
	};
}

public sealed class RequestLogDto {
	public DateTime Timestamp { get; init; }
	public string Method { get; init; } = "";
	public string Path { get; init; } = "";
	public int StatusCode { get; init; }
	public long DurationMs { get; init; }
	public string RawRequest { get; init; } = "";
	public string DecodedRequest { get; init; } = "";
	public string Response { get; init; } = "";
	public Exception? Exception { get; init; }
}