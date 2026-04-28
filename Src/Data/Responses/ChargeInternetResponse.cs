namespace SinaMN75U.Data.Responses;

public sealed class ChargeInternetReserveResponse {
	public long? Reserve { get; set; }
	public string? ServerDateTime { get; set; }
	public bool? Status { get; set; }
	public int? Code { get; set; }
	public string? Message { get; set; }
	public string? Reference { get; set; }
	public string? TraceId { get; set; }
	public long? AffectiveAmount { get; set; }
	public string? Help { get; set; }
	public string? MessageSource { get; set; }
}

public sealed class ChargeInternetGetPinResponse {
	public required string ReferenceCode { get; set; }
	public required long EffectiveAmount { get; set; }
}