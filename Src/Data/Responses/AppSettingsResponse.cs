namespace SinaMN75U.Data.Responses;

public class AppSettingsResponse {
	public required ApiCallCosts ApiCallCosts { get; set; }
	public required IEnumerable<ChargeInternet> ChargeInternet { get; set; }
	public required decimal ChargeInternetTaxPercent { get; set; }
	public required IEnumerable<AppVersionResponse> AppVersions { get; set; }
}

public sealed class AppVersionResponse : BaseResponse<TagAppVersion, AppVersionJson> {
	public int LatestBuildNumber { get; set; }
	public int MinBuildNumber { get; set; }
}