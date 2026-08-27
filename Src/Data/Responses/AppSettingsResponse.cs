namespace SinaMN75U.Data.Responses;

public class AppSettingsResponse {
	public required ApiCallCosts ApiCallCosts { get; set; }
	public required IEnumerable<ChargeInternet> ChargeInternet { get; set; }
	public required decimal ChargeInternetTaxPercent { get; set; }
}