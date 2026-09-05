namespace SinaMN75U.Data.Params;

public sealed class BillInfoParams : BaseParams {
	[UValidationRequired("billIdRequired")]
	public string BillId { get; set; } = null!;

	[UValidationRequired("paymentIdRequired")]
	public string PaymentId { get; set; } = null!;
}

public sealed class VerifyNationalCodeAndPhoneNumber : BaseParams {
	[UValidationRequired("nationalCodeIsRequired"), UValidationStringLength(10, 10, "nationalCodeIsInvalid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("phoneNumberIsRequired"), UValidationStringLength(9, 15, "pleaseEnterAValidPhoneNumber")]
	public string PhoneNumber { get; set; } = null!;

	public bool Refresh { get; set; }
}

public sealed class ZipCodeToAddressDetailParams : BaseParams {
	[UValidationRequired("zipCodeIsRequired"), UValidationStringLength(10, 10, "zipCodeIsInvalid")]
	public string ZipCode { get; set; } = null!;

	public bool Refresh { get; set; }
}

public sealed class VehicleViolationDetailParams : BaseParams {
	[UValidationRequired("nationalCodeIsRequired"), UValidationStringLength(10, 10, "nationalCodeIsInvalid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("phoneNumberIsRequired"), UValidationStringLength(9, 15, "pleaseEnterAValidPhoneNumber")]
	public string PhoneNumber { get; set; } = null!;

	[UValidationRequired("licencePlateIsRequired"), UValidationStringLength(4, 10, "licencePlateIsInvalid")]
	public string LicencePlate { get; set; } = null!;

	public bool Refresh { get; set; }
}

public sealed class DrivingLicenceDetailParams : BaseParams {
	[UValidationRequired("nationalCodeIsRequired"), UValidationStringLength(10, 10, "nationalCodeIsInvalid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("phoneNumberIsRequired"), UValidationStringLength(9, 15, "pleaseEnterAValidPhoneNumber")]
	public string PhoneNumber { get; set; } = null!;

	public bool Refresh { get; set; }
}

public sealed class LicencePlateDetailParams : BaseParams {
	[UValidationRequired("nationalCodeIsRequired"), UValidationStringLength(10, 10, "nationalCodeIsInvalid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("licencePlateIsRequired"), UValidationStringLength(5, 15, "licencePlateIsInvalid")]
	public string LicencePlate { get; set; } = null!;

	public bool Refresh { get; set; }
}

public sealed class FreewayTollsParams : BaseParams {
	[UValidationRequired("licencePlateIsRequired"), UValidationStringLength(5, 15, "licencePlateIsInvalid")]
	public string LicencePlate { get; set; } = null!;

	public bool Refresh { get; set; }
}

public sealed class DrivingLicenceNegativePointParams : BaseParams {
	[UValidationRequired("nationalCodeIsRequired"), UValidationStringLength(10, 10, "nationalCodeIsInvalid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("phoneNumberIsRequired"), UValidationStringLength(9, 15, "pleaseEnterAValidPhoneNumber")]
	public string PhoneNumber { get; set; } = null!;

	[UValidationRequired("DrivingLicenceNumberRequired"), UValidationStringLength(5, 15, "DrivingLicenceNumberInvalid")]
	public string DrivingLicenceNumber { get; set; } = null!;

	public bool Refresh { get; set; }
}

public sealed class IBanToBankAccountDetailParams : BaseParams {
	[UValidationRequired("iBanIsRequired"), UValidationStringLength(20, 28, "iBanIsInvalid")]
	public string IBan { get; set; } = null!;

	public bool Refresh { get; set; }
}
// Read-only cache lookup for a vehicle: reports which vehicle inquiries are already cached and when they expire. Never charges.
public sealed class InquiryCacheStatusParams : BaseParams {
	[UValidationRequired("nationalCodeIsRequired"), UValidationStringLength(10, 10, "nationalCodeIsInvalid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("phoneNumberIsRequired"), UValidationStringLength(9, 15, "pleaseEnterAValidPhoneNumber")]
	public string PhoneNumber { get; set; } = null!;

	[UValidationRequired("licencePlateIsRequired"), UValidationStringLength(4, 10, "licencePlateIsInvalid")]
	public string LicencePlate { get; set; } = null!;
}
