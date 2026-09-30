namespace SinaMN75U.Data.Params;

public sealed class ReserveChargeParams : BaseParams{
	public required decimal Amount { get; set; }
	public required string SimType { get; set; }  // Operator id (1 MCI, 2 Irancell, 3 Rightel, 5 Shatel)
}

public sealed class TopupChargeParams : BaseParams{
	public required decimal Amount { get; set; }
	public required string OperatorId { get; set; }
	public required string ChargeType { get; set; }  // Mobtakeran product type: "0" normal, "1" Irancell amazing / Rightel exciting, "2" MCI youth, "3" MCI women
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
	public string? Reserve { get; set; }  // The reserve (order number) sent with the original purchase
}

public sealed class MCITopOfferParams : BaseParams {
	public required string Subscriber { get; set; }  // Phone number
}

public sealed class InternetReserveParams : BaseParams {
	public required string Subscriber { get; set; }  // Phone number
	public required string OperatorId { get; set; }
	public required string PackageId { get; set; }   // Id from InternetList / MciTopOffer
	public required decimal Amount { get; set; }      // Amount from InternetList / MciTopOffer
	public string? Device { get; set; }              // Not used anymore, the server always sends "Mobile Application"
}
