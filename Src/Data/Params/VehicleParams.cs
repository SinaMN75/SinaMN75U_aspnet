namespace SinaMN75U.Data.Params;

public sealed class VehicleCreateParams : BaseCreateParams<TagVehicle> {
	[UValidationStringLength(5, 10, "LicencePlateMinMaxLenght")]
	public string LicencePlate { get; set; } = null!;

	public string? Title { get; set; }
	public string? Brand { get; set; }
	public string? Color { get; set; }
}

public sealed class VehicleUpdateParams : BaseUpdateParams<TagVehicle> {
	[UValidationStringLength(5, 10, "LicencePlateMinMaxLenght")]
	public string? LicencePlate { get; set; }

	public string? Brand { get; set; }
	public string? Color { get; set; }
}

public sealed class VehicleReadParams : BaseReadParams<TagVehicle> {
	public string? LicencePlate { get; set; }
	public string? Brand { get; set; }
	public string? Color { get; set; }

	public VehicleSelectorArgs SelectorArgs { get; set; } = new();
}