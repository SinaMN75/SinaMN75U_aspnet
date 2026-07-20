namespace SinaMN75U.Data.Params;

public sealed class IpgSaleParams : BaseParams {
	[UValidationRequired("AmountRequired")]
	public decimal Amount { get; set; }
}

public sealed class IpgVerifyParams : BaseParams {
	[UValidationRequired("TrackingNumberRequired")]
	public required string TrackingNumber { get; set; }
}
