namespace SinaMN75U.Data.Params;

public sealed class ReserveChargeParams : BaseParams{
	public required string Amount { get; set; }
	public required string SimType { get; set; }
}

public sealed class TopupChargeParams : BaseParams{
	public required string Amount { get; set; }
	public required string OperatorId { get; set; }
	public required string ChargeType { get; set; }
	public required string PhoneNumber { get; set; }
}

public sealed class InternetListParams : BaseParams{
	public required string SimType { get; set; }
}
