namespace SinaMN75U.Data.Params;

public sealed class VerifyNationalCodeAndPhoneNumber : BaseParams {
	[UValidationRequired("NationalCodeRequired"), UValidationStringLength(10, 10, "NationalCodeInvalid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("PhoneNumberRequired"), UValidationStringLength(9, 15, "PhoneNumberInvalid")]
	public string PhoneNumber { get; set; } = null!;
}

public sealed class ZipCodeToAddressDetailParams : BaseParams {
	[UValidationRequired("ZipCodeRequired"), UValidationStringLength(10, 10, "ZipCodeInvalid")]
	public string ZipCode { get; set; } = null!;
}

public sealed class VehicleViolationDetailParams : BaseParams {
	[UValidationRequired("NationalCodeRequired"), UValidationStringLength(10, 10, "NationalCodeInvalid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("PhoneNumberRequired"), UValidationStringLength(9, 15, "PhoneNumberInvalid")]
	public string PhoneNumber { get; set; } = null!;

	[UValidationRequired("LicencePlateRequired"), UValidationStringLength(4, 10, "LicencePlateInvalid")]
	public string LicencePlate { get; set; } = null!;
}

public sealed class DrivingLicenceStatusParams : BaseParams {
	[UValidationRequired("NationalCodeRequired"), UValidationStringLength(10, 10, "NationalCodeInvalid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("PhoneNumberRequired"), UValidationStringLength(9, 15, "PhoneNumberInvalid")]
	public string PhoneNumber { get; set; } = null!;
}

public sealed class LicencePlateInquiryParams : BaseParams {
	[UValidationRequired("NationalCodeRequired"), UValidationStringLength(10, 10, "NationalCodeInvalid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("LicencePlateRequired"), UValidationStringLength(5, 15, "LicencePlateInvalid")]
	public string LicencePlate { get; set; } = null!;
}

public sealed class FreewayTollsParams : BaseParams {
	[UValidationRequired("LicencePlateRequired"), UValidationStringLength(5, 15, "LicencePlateInvalid")]
	public string LicencePlate { get; set; } = null!;
}

public sealed class DrivingLicenceNegativePointParams : BaseParams {
	[UValidationRequired("NationalCodeRequired"), UValidationStringLength(10, 10, "NationalCodeInvalid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("PhoneNumberRequired"), UValidationStringLength(9, 15, "PhoneNumberInvalid")]
	public string PhoneNumber { get; set; } = null!;

	[UValidationRequired("DrivingLicenceNumberRequired"), UValidationStringLength(5, 15, "DrivingLicenceNumberInvalid")]
	public string DrivingLicenceNumber { get; set; } = null!;
}

public sealed class IBanToBankAccountDetailParams : BaseParams {
	[UValidationRequired("IBanRequired"), UValidationStringLength(20, 28, "IBanInvalid")]
	public string IBan { get; set; } = null!;
}