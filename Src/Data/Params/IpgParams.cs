namespace SinaMN75U.Data.Params;

public class IpgBaseParams {
	public required decimal Amount { get; set; }
	public required string OrderId { get; set; }
	public required string Base64AdditionalData { get; set; }
	public required string User { get; set; }
}

public class MpgBaseParams {
	public required decimal Amount { get; set; }
	public required string OrderId { get; set; }
	public required string Base64AdditionalData { get; set; }
	public required string User { get; set; }
	public required string Pan { get; set; }
	public required string Pin2 { get; set; }
	public required string Cvv2 { get; set; }
	public required string ExpireYear { get; set; }
	public required string ExpireMonth { get; set; }
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
}

public sealed class IpgSaleParams : IpgBaseParams;

public sealed class MpgSaleParams : MpgBaseParams;