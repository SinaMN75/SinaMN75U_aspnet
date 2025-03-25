namespace SinaMN75U.Constants;

public class UJsonOptions {
	public static readonly JsonSerializerOptions JsonOptions = new() {
		WriteIndented = false,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		ReferenceHandler = ReferenceHandler.IgnoreCycles,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};
}