namespace SinaMN75U.Data.Responses;

public sealed class VehicleResponse : BaseResponse<TagVehicle, GeneralJsonData> {
	public required string LicencePlate { get; set; }
	public string? Title { get; set; }
	public string? Brand { get; set; }
	public string? Color { get; set; }
	public required Guid CreatorId { get; set; }
	public UserResponse? Creator { get; set; }
}