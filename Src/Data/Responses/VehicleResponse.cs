namespace SinaMN75U.Data.Responses;

public class VehicleResponse : BaseResponse<TagVehicle, VehicleJson> {
	public required string NumberPlate { get; set; }
	public string? Title { get; set; }
	public string? Brand { get; set; }
	public string? Color { get; set; }
}