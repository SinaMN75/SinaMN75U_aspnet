namespace SinaMN75U.Constants;

public static class UJsonOptions {
	public static readonly JsonSerializerOptions Default = new() {
		WriteIndented = false,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		ReferenceHandler = ReferenceHandler.IgnoreCycles,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		MaxDepth = 128
	};
}