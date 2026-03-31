namespace SinaMN75U.Data.Responses;

public class IpgAdditionalData {
	public required string UserId { get; set; }
	public required string GatewayName { get; set; }
	public required string TrackingNumber { get; set; }
	public required decimal Amount { get; set; }
}