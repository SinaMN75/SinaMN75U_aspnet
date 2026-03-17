namespace SinaMN75U.Data.Entities;

[Table("Terminals")]
public class TerminalEntity : BaseEntity<TagTerminal, TerminalJson> {
	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }
}

public class TerminalJson { }