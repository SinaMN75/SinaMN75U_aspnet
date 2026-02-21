namespace SinaMN75U.Data.Params;

public class ITHubGetManagementAccessTokenParams {
	public required string UserName { get; set; }
	public required string Password { get; set; }
}

public class ITHubRefreshManagementAccessTokenParams {
	public required string RefreshToken { get; set; }
}

public class ITHubShahkarParams {
	public required string NationalCode { get; set; }
	public required string Mobile { get; set; }
}

public class PostalCodeToAddressDetailParams {
	public required string PostCode { get; set; }
	public required string OrderId { get; set; }
}