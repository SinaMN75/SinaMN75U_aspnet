namespace SinaMN75U.Data.Responses;

// public class UFlowResponse {
// 	public required int CurrentStep { get; set; }
// 	public required int TotalSteps { get; set; }
// 	public required List<UFlowStep> Steps { get; set; }
// }

public class UFlowStep {
	public required string Title { get; set; }
	public required string Description { get; set; }
	public required string Endpoint { get; set; }
	public required List<UFlowField> Fields { get; set; }
}

public class UFlowField {
	public required string Label { get; set; }
	public string? Value { get; set; }
	public required TagFieldType Type { get; set; }
	public required bool Required { get; set; }
	public required string Key { get; set; }
	public UValidationRule? Validation { get; set; }
	public List<UOption>? Options { get; set; }
}

public class UValidationRule {
	public int? MinLength { get; set; }
	public int? MaxLength { get; set; }
	public string? Message { get; set; }

	public List<string> AllowedType { get; set; } = [];
	public int? MaxSizeMb { get; set; }
}

public class UOption {
	public required string Key { get; set; }
	public required string Value { get; set; }
}