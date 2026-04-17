namespace SinaMN75U.Data.Params;

public sealed class VerifyNationalCodeAndPhoneNumber : BaseParams {
	[UValidationRequired("NationalCodeRequired"), UValidationStringLength(10, 10, "NationalCodeInvalid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("PhoneNumberRequired"), UValidationStringLength(9, 15, "PhoneNumberInvalid")]
	public string PhoneNumber { get; set; } = null!;
}

public sealed class PostalCodeToAddressDetailParams {
	[UValidationRequired("ZipCodeRequired"), UValidationStringLength(10, 10, "ZipCodeInvalid")]
	public string ZipCode { get; set; } = null!;
}

public sealed class VehicleViolationDetailParams {
	[UValidationRequired("NationalCodeRequired"), UValidationStringLength(10, 10, "NationalCodeInvalid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("PhoneNumberRequired"), UValidationStringLength(9, 15, "PhoneNumberInvalid")]
	public string PhoneNumber { get; set; } = null!;

	[UValidationRequired("LicencePlateRequired"), UValidationStringLength(4, 10, "LicencePlateInvalid")]
	public string LicencePlate { get; set; } = null!;
}

public sealed class DrivingLicenceStatusParams {
	[UValidationRequired("NationalCodeRequired"), UValidationStringLength(10, 10, "NationalCodeInvalid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("PhoneNumberRequired"), UValidationStringLength(9, 15, "PhoneNumberInvalid")]
	public string PhoneNumber { get; set; } = null!;
}

public sealed class LicencePlateInquiryParams {
	[UValidationRequired("NationalCodeRequired"), UValidationStringLength(10, 10, "NationalCodeInvalid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("LicencePlateRequired"), UValidationStringLength(5, 15, "LicencePlateInvalid")]
	public string LicencePlate { get; set; } = null!;
}

public sealed class DrivingLicenceNegativePointParams {
	[UValidationRequired("NationalCodeRequired"), UValidationStringLength(10, 10, "NationalCodeInvalid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("PhoneNumberRequired"), UValidationStringLength(9, 15, "PhoneNumberInvalid")]
	public string PhoneNumber { get; set; } = null!;

	[UValidationRequired("DrivingLicenceNumberRequired"), UValidationStringLength(5, 15, "DrivingLicenceNumberInvalid")]
	public string DrivingLicenceNumber { get; set; } = null!;
}

public sealed class IBanToBankAccountDetailParams {
	[UValidationRequired("IBanRequired"), UValidationStringLength(20, 28, "IBanInvalid")]
	public string IBan { get; set; } = null!;
}