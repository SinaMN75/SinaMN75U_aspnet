namespace SinaMN75U.Data.Params;

public class MerchantCreateParams : BaseCreateParams<TagMerchant> {
	[UValidationRequired("ZipCodeRequired"), UValidationStringLength(10, 10, "ZipCodeNotValid")]
	public string ZipCode { get; set; } = null!;

	[UValidationRequired("UserIdRequired")]
	public Guid UserId { get; set; }
}

public class MerchantUpdateParams : BaseUpdateParams<TagMerchant> {
	public string? ZipCode { get; set; }
}

public class MerchantReadParams : BaseReadParams<TagMerchant> {
	public string? ZipCode { get; set; }
	public Guid? UserId { get; set; }

	public MerchantSelectorArgs SelectorArgs { get; set; } = new();
}