namespace SinaMN75U.Data.Params;

public class UProcessStepSend : BaseParams {
	public string Id { get; set; } = "";
	public List<UProcessField> Fields { get; set; } = [];
}