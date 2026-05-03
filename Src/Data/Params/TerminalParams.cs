namespace SinaMN75U.Data.Params;

public class TerminalCreateParams : BaseCreateParams<TagTerminal> {
	[UValidationRequired("SerialRequired")]
	public string Serial { get; set; } = null!;
	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }
	public string? MerchantId { get; set; }
	public string? TerminalId { get; set; }
	public Guid? AddressId { get; set; }
}

public class TerminalBulkCreateParams : BaseParams {
	public required List<TerminalCreateParams> List { get; set; }
}

public class TerminalUpdateParams : BaseUpdateParams<TagTerminal> {
	public string? Serial { get; set; }
	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }
	public string? MerchantId { get; set; }
	public string? TerminalId { get; set; }
	public Guid? AddressId { get; set; }
	public Guid? UserId { get; set; }
}

public class TerminalReadParams : BaseReadParams<TagTerminal> {
	public string? Serial { get; set; }
	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }
	public string? MerchantId { get; set; }
	public string? TerminalId { get; set; }
	public Guid? AddressId { get; set; }

	public TerminalSelectorArgs SelectorArgs { get; set; } = new();
}