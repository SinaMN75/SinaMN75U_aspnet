namespace SinaMN75U.Data.Responses;

public sealed class IpgAdditionalData {
	public required string UserId { get; set; }
	public required string GatewayName { get; set; }
	public required string TrackingNumber { get; set; }
	public required decimal Amount { get; set; }
}

public sealed class MpgSaleResponse {
	public string? Rrn { get; set; }
	public string? Trace { get; set; }
	public string? OrderId { get; set; }
	public string? Token { get; set; }
	public string? Pan { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }
	public string? Status { get; set; }
	public string? Message { get; set; }
}