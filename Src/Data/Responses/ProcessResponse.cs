namespace SinaMN75U.Data.Responses;

public class UProcessStepStatusResponse {
	public string Id { get; set; } = "";
	public string Title { get; set; } = "";
	public TagProcessStepStatus Status { get; set; }
}

public class UProcessStepGetResponse {
	public string Id { get; set; } = "";
	public string Title { get; set; } = "";
	public string Description { get; set; } = "";
	public string? Message { get; set; }
	public List<UProcessField> Fields { get; set; } = [];
	public List<UProcessStepStatusResponse> Steps { get; set; } = [];
}

public class UProcessField {
	public string Key { get; set; } = "";
	public string Label { get; set; } = "";
	public string? Value { get; set; }
	public bool Required { get; set; }
	public TagFieldType Type { get; set; }
	public UTextFieldConfig? TextFieldConfig { get; set; }
	public UFileConfig? FileConfig { get; set; }
}

public class UTextFieldConfig {
	public TagTextFieldType Type { get; set; }
	public int? MaxLength { get; set; }
	public int? MinLength { get; set; }
}

public class UFileConfig {
	public TagFileFieldType Type { get; set; }
	public bool IsCamera { get; set; }
	public bool IsSelfieCamera { get; set; }
}