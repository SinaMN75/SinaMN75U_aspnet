namespace SinaMN75U.Data.Responses;

public class ITHubResponse<T> {
	public T? Data { get; set; }
}

public class ITHubGetAccessTokenResponse {
	[JsonPropertyName("access_token")]
	public string AccessToken { get; set; } = null!;

	[JsonPropertyName("expires_in")]
	public int ExpiresIn { get; set; }

	[JsonPropertyName("token_type")]
	public string TokenType { get; set; } = null!;

	[JsonPropertyName("scope")]
	public string Scope { get; set; } = null!;
}

public class ITHubShahkarResponse {
	public required bool Data { get; set; }
	public string? Meta { get; set; }
	public string? Error { get; set; }
}

public class ITHubPostalCodeToAddressDetailResponse {
	public string? BuildingName { get; set; }
	public string? Description { get; set; }
	public string? Floor { get; set; }
	public string? HouseNumber { get; set; }
	public string? LocalityCode { get; set; }
	public string? LocalityName { get; set; }
	public string? LocalityType { get; set; }
	public string? ZipCode { get; set; }
	public string? Province { get; set; }
	public string? SideFloor { get; set; }
	public string? Street { get; set; }
	public string? Street1 { get; set; }
	public string? SubLocality { get; set; }
	public string? TownShip { get; set; }
	public string? TraceId { get; set; }
	public string? Village { get; set; }
	public string? City { get; set; }
}