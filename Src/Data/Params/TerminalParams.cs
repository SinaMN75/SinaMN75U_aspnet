namespace SinaMN75U.Data.Params;

public class TerminalCreateParams : BaseCreateParams<TagTerminal> {
	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }
}

public class TerminalUpdateParams : BaseUpdateParams<TagTerminal> {
	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }
}

public class TerminalReadParams : BaseReadParams<TagTerminal> {
	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }
}

