namespace SinaMN75U.Data.Responses;

public sealed class IpgPayResponse {
	public required string Url { get; set; }
	public required string TrackingNumber { get; set; }
}

public sealed class IpgVerifyResponse {
	public required bool Paid { get; set; }
	public required bool Failed { get; set; }
	public required decimal Balance { get; set; }
}
