namespace SinaMN75U.Data.Params;

public class IpgBaseParams {
	public required decimal Amount { get; set; }
	public required string OrderId { get; set; }
	public required string Base64AdditionalData { get; set; }
	public required string User { get; set; }
}

public sealed class IpgSaleParams : IpgBaseParams;