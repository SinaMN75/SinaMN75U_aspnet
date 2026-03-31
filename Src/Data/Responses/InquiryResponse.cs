namespace SinaMN75U.Data.Responses;

public class GetAccessTokenResponse {
	public string? AccessToken { get; set; }
	public int? ExpiresIn { get; set; }
}

public sealed class PostalCodeToAddressDetailResponse {
	public string? BuildingName { get; set; }
	public string? Description { get; set; }
	public string? Floor { get; set; }
	public string? HouseNumber { get; set; }
	public string? LocalityName { get; set; }
	public string? LocalityType { get; set; }
	public string? ZipCode { get; set; }
	public string? Province { get; set; }
	public string? SideFloor { get; set; }
	public string? Street { get; set; }
	public string? Street2 { get; set; }
	public string? SubLocality { get; set; }
	public string? TownShip { get; set; }
	public string? TraceId { get; set; }
	public string? Village { get; set; }
	public int? LocalityCode { get; set; }
}