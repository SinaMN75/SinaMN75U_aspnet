namespace SinaMN75U.Data.Params;

public class MerchantCreateParams : BaseCreateParams<TagMerchant> {
	[UValidationRequired("zipCodeIsRequired"), UValidationStringLength(10, 10, "zipCodeIsNotValid")]
	public string ZipCode { get; set; } = null!;

	public Guid? UserId { get; set; }

	[UValidationRequired("cityCodeIsRequired"), UValidationStringLength(1, 100, "cityCodeIsNotValid")]
	public string CityCode { get; set; } = null!;

	[UValidationRequired("addressIsRequired"), UValidationStringLength(1, 500, "addressIsNotValid")]
	public string Address { get; set; } = null!;

	[UValidationRequired("phoneNumberIsRequired"), UValidationStringLength(10, 15, "phoneNumberIsNotValid")]
	public string PhoneNumber { get; set; } = null!;

	[UValidationRequired("merchantNameIsRequired"), UValidationStringLength(5, 100, "merchantNameIsNotValid")]
	public string Title { get; set; } = null!;

	[UValidationRequired("landlineIsRequired"), UValidationStringLength(6, 12, "landlineIsNotValid")]
	public string Landline { get; set; } = null!;

	[UValidationRequired("nationalCodeIsRequired"), UValidationStringLength(10, 10, "NationalCodeNotValid")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("ownerPhoneNumberIsRequired"), UValidationStringLength(10, 15, "ownerPhoneNumberIsNotValid")]
	public string OwnerPhoneNumber { get; set; } = null!;

	[UValidationRequired("ownerNameIsRequired"), UValidationStringLength(5, 100, "ownerNameIsNotValid")]
	public string OwnerName { get; set; } = null!;

	[UValidationRequired("mccIsRequired"), UValidationStringLength(1, 100, "mccIsNotValid")]
	public string Mcc { get; set; } = null!;

	public string? BusinessTitle { get; set; }
	public string? BankAccountId { get; set; }
}

public class MerchantBindParams : BaseParams {
	public Guid? UserId { get; set; }
	public Guid? MerchantId { get; set; }
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