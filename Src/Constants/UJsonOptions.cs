namespace SinaMN75U.Constants;

public class UJsonOptions {
	public static readonly JsonSerializerOptions Default = new() {
		WriteIndented = false,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		ReferenceHandler = ReferenceHandler.IgnoreCycles,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};
}