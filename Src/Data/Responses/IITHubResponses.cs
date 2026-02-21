namespace SinaMN75U.Data.Responses;

public class ItHubBaseResponse<T> {
	[JsonPropertyName("data")]
	public T? Data { get; set; }

	[JsonPropertyName("meta")]
	public object? Meta { get; set; }

	[JsonPropertyName("error")]
	public object? Error { get; set; }
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

public class ItHubPostalCodeToAddressDetailResponse {
	[JsonPropertyName("buildingName")]
	public string BuildingName { get; set; }

	[JsonPropertyName("description")]
	public string Description { get; set; }

	[JsonPropertyName("floor")]
	public string Floor { get; set; }

	[JsonPropertyName("houseNumber")]
	public long HouseNumber { get; set; }

	[JsonPropertyName("localityCode")]
	public long LocalityCode { get; set; }

	[JsonPropertyName("localityName")]
	public string LocalityName { get; set; }

	[JsonPropertyName("localityType")]
	public string LocalityType { get; set; }

	[JsonPropertyName("zipCode")]
	public string ZipCode { get; set; }

	[JsonPropertyName("province")]
	public string Province { get; set; }

	[JsonPropertyName("sideFloor")]
	public string SideFloor { get; set; }

	[JsonPropertyName("street")]
	public string Street { get; set; }

	[JsonPropertyName("street2")]
	public string Street2 { get; set; }

	[JsonPropertyName("subLocality")]
	public string SubLocality { get; set; }

	[JsonPropertyName("townShip")]
	public string TownShip { get; set; }

	[JsonPropertyName("traceId")]
	public string TraceId { get; set; }

	[JsonPropertyName("village")]
	public string Village { get; set; }
}