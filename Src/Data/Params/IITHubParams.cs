namespace SinaMN75U.Data.Params;

public sealed class ITHubGetManagementAccessTokenParams {
	public required string UserName { get; set; }
	public required string Password { get; set; }
}

public sealed class ITHubRefreshManagementAccessTokenParams {
	public required string RefreshToken { get; set; }
}

public sealed class ITHubShahkarParams : BaseParams {
	public required string NationalCode { get; set; }
	public required string Mobile { get; set; }
}

public sealed class PostalCodeToAddressDetailParams {
	public required string PostCode { get; set; }
	public required string OrderId { get; set; }
}