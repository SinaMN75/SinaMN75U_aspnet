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
	public required string OperatorId { get; set; }
}

public sealed class ApproveParams : BaseParams {
	public required string Reference { get; set; }
	public string? CardNumber { get; set; }
	public string? NationalCode { get; set; }
}

public sealed class GetStatusParams : BaseParams {
	public required string Reference { get; set; }
}

public sealed class MCITopOfferParams : BaseParams {
	public required string Subscriber { get; set; }  // Phone number
}

public sealed class InternetReserveParams : BaseParams {
	public required string Subscriber { get; set; }  // Phone number
	public required string OperatorId { get; set; }
	public required string PackageId { get; set; }   // Id from InternetList
	public required string Amount { get; set; }      // Amount from InternetList
	public required string Device { get; set; }      // e.g., "05"
	public string? Bank { get; set; }
}