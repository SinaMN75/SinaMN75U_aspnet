namespace SinaMN75U.Data.Responses;

public sealed class VehicleResponse : BaseResponse<TagVehicle, GeneralJsonData> {
	public required string NumberPlate { get; set; }
	public string? Title { get; set; }
	public string? Brand { get; set; }
	public string? Color { get; set; }
}