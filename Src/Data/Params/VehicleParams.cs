namespace SinaMN75U.Data.Params;

public sealed class VehicleCreateParams : BaseCreateParams<TagVehicle> {
	public required string NumberPlate { get; set; }
	public string? Title { get; set; }
	public string? Brand { get; set; }
	public string? Color { get; set; }
}

public sealed class VehicleUpdateParams : BaseUpdateParams<TagVehicle> {
	public string? NumberPlate { get; set; }
	public string? Title { get; set; }
	public string? Brand { get; set; }
	public string? Color { get; set; }
}

public sealed class VehicleReadParams : BaseReadParams<TagVehicle> {
	public string? NumberPlate { get; set; }
	public string? Title { get; set; }
	public string? Brand { get; set; }
	public string? Color { get; set; }

	public required VehicleSelectorArgs SelectorArgs { get; set; }
}