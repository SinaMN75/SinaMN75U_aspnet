namespace SinaMN75U.Data.Responses;

public sealed class ITHubShahkarBaseResponse {
	[JsonPropertyName("data")]
	public bool? Data { get; set; }
}

public sealed class ITHubGetAccessTokenBaseResponse {
	[JsonPropertyName("data")]
	public ITHubGetAccessTokenResponse? Data { get; set; }
}

public sealed class ITHubGetAccessTokenResponse {
	[JsonPropertyName("access_token")]
	public string? AccessToken { get; set; }

	[JsonPropertyName("expires_in")]
	public int ExpiresIn { get; set; }

	[JsonPropertyName("token_type")]
	public string? TokenType { get; set; }

	[JsonPropertyName("scope")]
	public string? Scope { get; set; }
}

public sealed class ItHubPostalCodeToAddressDetailBaseResponse {
	[JsonPropertyName("data")]
	public ItHubPostalCodeToAddressDetailResponse? Data { get; set; }
}

public sealed class ItHubPostalCodeToAddressDetailResponse {
	[JsonPropertyName("buildingName")]
	public string? BuildingName { get; set; }

	[JsonPropertyName("description")]
	public string? Description { get; set; }

	[JsonPropertyName("floor")]
	public string? Floor { get; set; }

	[JsonPropertyName("houseNumber")]
	public string? HouseNumber { get; set; }

	[JsonPropertyName("localityCode")]
	public int LocalityCode { get; set; }

	[JsonPropertyName("localityName")]
	public string? LocalityName { get; set; }

	[JsonPropertyName("localityType")]
	public string? LocalityType { get; set; }

	[JsonPropertyName("zipCode")]
	public string? ZipCode { get; set; }

	[JsonPropertyName("province")]
	public string? Province { get; set; }

	[JsonPropertyName("sideFloor")]
	public string? SideFloor { get; set; }

	[JsonPropertyName("street")]
	public string? Street { get; set; }

	[JsonPropertyName("street2")]
	public string? Street2 { get; set; }

	[JsonPropertyName("subLocality")]
	public string? SubLocality { get; set; }

	[JsonPropertyName("townShip")]
	public string? TownShip { get; set; }

	[JsonPropertyName("traceId")]
	public string? TraceId { get; set; }

	[JsonPropertyName("village")]
	public string? Village { get; set; }
}