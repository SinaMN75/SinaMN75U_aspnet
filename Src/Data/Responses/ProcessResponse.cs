namespace SinaMN75U.Data.Responses;

public class UProcessStepSend : BaseParams {
	public required string Id { get; set; }
	public required List<UProcessField> Fields { get; set; }
}

public class UProcess {
	public required ICollection<UProcessItem> MainProcesses { get; set; }
}

public class UProcessGetParams: BaseParams {
	public required string ProcessId { get; set; }
	public bool Intro { get; set; }
}


public class UProcessItem {
	public required string Id { get; set; }
	public required string Title { get; set; }
	public required string Description { get; set; }
	public required string Icon { get; set; }
	public required TagProcessStatus Status { get; set; }
}

public class UProcessStepGet {
	public required string Id { get; set; }
	public required string Title { get; set; }
	public required string Description { get; set; }
	public bool IsScrollable { get; set; }
	public List<UProcessField>? Fields { get; set; }
	public string? Message { get; set; }
}

public class UProcessStepGet<T> {
	public required string Id { get; set; }
	public required string Title { get; set; }
	public required string Description { get; set; }
	public bool IsScrollable { get; set; }
	public required T Response { get; set; }
}

public class UProcessField {
	public required string Label { get; set; }
	public string? Value { get; set; }
	public required TagFieldType Type { get; set; }
	public required bool Required { get; set; }
	public required string Key { get; set; }
	public UTextFieldConfig? TextFieldConfig { get; set; }
	public UFileConfig? FileConfig { get; set; }
	public UDropDownConfig? DropDownConfig { get; set; }
	public List<UOption>? Options { get; set; }
}

public class UTextFieldConfig {
	public int? MinLength { get; set; }
	public int? MaxLength { get; set; }
}

public class UFileConfig {
	public ICollection<string>? AllowedExtensions { get; set; }
	public bool IsImage { get; set; }
	public bool IsVideo { get; set; }
	public bool IsPdf { get; set; }
	public bool IsCamera { get; set; }
	public bool IsSelfieCamera { get; set; }
}

public class UDropDownConfig {
	public bool IsSearchable { get; set; }
}

public class UOption {
	public required string Key { get; set; }
	public required string Value { get; set; }
}