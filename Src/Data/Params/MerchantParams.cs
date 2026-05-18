namespace SinaMN75U.Data.Params;

public class MerchantCreateParams : BaseCreateParams<TagMerchant> {
	[UValidationRequired("ZipCodeRequired"), UValidationStringLength(10, 10, "ZipCodeNotValid")]
	public string ZipCode { get; set; } = null!;

	public Guid? UserId { get; set; }

	[UValidationRequired("CityCodeRequired"), UValidationStringLength(1, 100, "CityCodeNotValid")]
	public string CityCode { get; set; } = null!;

	[UValidationRequired("AddressRequired"), UValidationStringLength(1, 500, "AddressNotValid")]
	public string Address { get; set; } = null!;

	[UValidationRequired("PhoneNumberRequired"), UValidationStringLength(10, 15, "PhoneNumberNotValid")]
	public string PhoneNumber { get; set; } = null!;

	[UValidationRequired("MerchantTitleRequired"), UValidationStringLength(5, 100, "MerchantTitleNotValid")]
	public string Title { get; set; } = null!;

	[UValidationRequired("LandlineRequired"), UValidationStringLength(6, 12, "LandlineNotValid")]
	public string Landline { get; set; } = null!;

	[UValidationRequired("NationalCodeRequired"), UValidationStringLength(10, 10, "NationalCodeNotValid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("OwnerPhoneNumberRequired"), UValidationStringLength(10, 15, "OwnerPhoneNumberNotValid")]
	public string OwnerPhoneNumber { get; set; } = null!;

	[UValidationRequired("OwnerNameRequired"), UValidationStringLength(5, 100, "OwnerNameNotValid")]
	public string OwnerName { get; set; } = null!;

	[UValidationRequired("MccRequired"), UValidationStringLength(1, 100, "MccNotValid")]
	public string Mcc { get; set; } = null!;

	public string? BusinessTitle { get; set; }
	public string? BankAccountId { get; set; }
}

public class MerchantUpdateParams : BaseUpdateParams<TagMerchant> {
	public string? ZipCode { get; set; }
	public string? InsId { get; set; }
}

public class MerchantReadParams : BaseReadParams<TagMerchant> {
	public string? ZipCode { get; set; }
	public Guid? UserId { get; set; }
	public string? CityCode { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Title { get; set; }
	public string? Landline { get; set; }
	public string? NationalCode { get; set; }
	public string? BankAccountId { get; set; }
	public string? Mcc { get; set; }
	public string? MerchantId { get; set; }
	public string? InsId { get; set; }

	public MerchantSelectorArgs SelectorArgs { get; set; } = new();
}