namespace SinaMN75U.Data.Params;

public sealed class ITHubShahkarParams : BaseParams {
	public required string NationalCode { get; set; }
	public required string Mobile { get; set; }
}

public sealed class PostalCodeToAddressDetailParams {
	public required string ZipCode { get; set; }
}

public sealed class VehicleViolationDetailParams {
	public required string NationalCode { get; set; }
	public required string PhoneNumber { get; set; }
	public required string LicencePlate { get; set; }
}

public sealed class DrivingLicenceStatusParams {
	public required string NationalCode { get; set; }
	public required string PhoneNumber { get; set; }
}