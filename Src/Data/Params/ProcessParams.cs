namespace SinaMN75U.Data.Params;

public class UProcessStepSend : BaseParams {
	public required string ProcessId { get; set; }
	public required string StepId { get; set; } = "";
	public List<UProcessField> Fields { get; set; } = [];
}