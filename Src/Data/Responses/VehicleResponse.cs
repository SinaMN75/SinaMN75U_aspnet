namespace SinaMN75U.Data.Responses;

public sealed class VehicleResponse : BaseResponse<TagVehicle, BaseJson> {
	public required string LicencePlate { get; set; }
	public string? Title { get; set; }
	public string? Brand { get; set; }
	public string? Color { get; set; }
}