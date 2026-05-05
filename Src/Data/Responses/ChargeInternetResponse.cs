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

public sealed class ApproveResponse {
    public long? Reserve { get; set; }
    public string? ServerDateTime { get; set; }
    public bool? Status { get; set; }
    public int? Code { get; set; }
    public string? Message { get; set; }
    public long? Reference { get; set; }
    public string? Serial { get; set; }      // PIN serial or operator reference
    public string? Pin { get; set; }         // PIN code for recharge
    public string? TraceId { get; set; }
    public string? Help { get; set; }
    public string? MessageSource { get; set; }
    public string? ExtCode { get; set; }
}

public sealed class GetStatusResponse {
    public long? Reserve { get; set; }
    public string? ServerDateTime { get; set; }
    public bool? Status { get; set; }
    public int? Code { get; set; }
    public string? Message { get; set; }
    public long? Reference { get; set; }
    public string? Subscriber { get; set; }
    public string? Serial { get; set; }
    public string? Pin { get; set; }
    public string? TxnTime { get; set; }
    public string? Help { get; set; }
    public string? MessageSource { get; set; }
    public string? ExtCode { get; set; }
}

public sealed class GetBalanceResponse {
    public long? Reserve { get; set; }
    public string? ServerDateTime { get; set; }
    public bool? Status { get; set; }
    public int? Code { get; set; }
    public string? Message { get; set; }
    public long? Balance { get; set; }   // Account balance
    public long? Wallet { get; set; }    // Consumable balance
    public long? Credit { get; set; }    // Credit
    public long? Limit { get; set; }     // Daily consumable limit
    public string? Help { get; set; }
    public string? MessageSource { get; set; }
    public string? ExtCode { get; set; }
}

public sealed class EchoResponse {
    public long? Reserve { get; set; }
    public string? ServerDateTime { get; set; }
    public bool? Status { get; set; }     // true = service is up
    public int? Code { get; set; }
    public string? Message { get; set; }
    public bool? MciTopup { get; set; }   // MCI Topup server status
    public bool? Mtn { get; set; }        // Irancel server status
    public bool? Rightel { get; set; }    // Rightel server status
    public bool? Shatel { get; set; }     // Shatel server status
    public bool? MciInternet { get; set; } // MCI Internet server status
}