namespace SinaMN75U.Data.Params;

public sealed class GetAccessTokenParams {
	public required string Reserve { get; set; }
	public required string UserName { get; set; }
	public required string Password { get; set; }
}

public sealed class ReserveChargeParams : BaseParams{
	public required string Amount { get; set; }
	public required TagSimChargeType SimType { get; set; }
}

public sealed class TopupChargeParams : BaseParams{
	public required string Amount { get; set; }
	public required TagSimChargeType SimType { get; set; }
	public required int ChargeType { get; set; }
	public required string PhoneNumber { get; set; }
}
