namespace SinaMN75U.Data.Responses;

public class TerminalResponse : BaseResponse<TagTerminal, TerminalJson> {
	public required string Serial { get; set; }
	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }
	public string? TerminalId { get; set; }
	public string? Agreement { get; set; }

	public Guid? MerchantId { get; set; }
	public MerchantResponse? Merchant { get; set; }
}

public class TerminalAvailabilityResponse {
	public required Guid Id { get; set; }
	public required string Serial { get; set; }
	public string? Agreement { get; set; }
}

public class TerminalSupportPasswordResponse {
	public string? Password { get; set; }
}

public class TerminalImportResponse {
	public int TotalRows { get; set; }
	public int Imported { get; set; }
	public int Skipped { get; set; }
	public List<string> SkippedSerials { get; set; } = [];
}

public sealed class TerminalBrandResponse : BaseResponse<TagTerminalBrand, TerminalBrandJson> {
	public required string Title { get; set; }
	public required string Model { get; set; }
}

public sealed class TerminalBrokerResponse : BaseResponse<TagTerminalBroker, TerminalBrokerJson> {
	public required string Title { get; set; }
	
}

