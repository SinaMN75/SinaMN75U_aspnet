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

public sealed class InternetPackageResponse {
	public long? Reserve { get; set; }
	public string? ServerDateTime { get; set; }
	public bool? Status { get; set; }
	public int? Code { get; set; }
	public string? Message { get; set; }
	public string? Reference { get; set; }
	public string? TraceId { get; set; }
	public string? Help { get; set; }

	public IEnumerable<InternetPackageItem> List { get; set; } = [];
}

public class InternetPackageItem {
	public long? Amount { get; set; }
	public string? Id { get; set; }
	public string? Title { get; set; }
	public string? ShortTitle { get; set; }
	public int? SimType { get; set; }
	public string? Duration { get; set; }
	public string? OfferCode { get; set; }
	public decimal? Price { get; set; } 
	public int? PackageDType { get; set; }
	public string? Capacity { get; set; }
}