namespace SinaMN75U.Data.Params;

public class MoadiCreateParams : BaseCreateParams<TagMoadi> {
	[UValidationRequired("nameIsRequired"), UValidationStringLength(1, 255, "nameIsNotValid")]
	public string Name { get; set; } = null!;

	[UValidationRequired("economicCodeIsRequired"), UValidationStringLength(1, 30, "economicCodeIsNotValid")]
	public string EconomicCode { get; set; } = null!;

	[UValidationRequired("legalEntityIsRequired"), UValidationStringLength(1, 20, "legalEntityIsNotValid")]
	public string LegalEntity { get; set; } = null!;

	[UValidationRequired("uniqueTaxCodeIsRequired"), UValidationStringLength(1, 30, "uniqueTaxCodeIsNotValid")]
	public string UniqueTaxCode { get; set; } = null!;

	[UValidationRequired("ownerNameIsRequired"), UValidationStringLength(1, 255, "ownerNameIsNotValid")]
	public string OwnerName { get; set; } = null!;

	[UValidationRequired("ownerMobileIsRequired"), UValidationStringLength(10, 15, "ownerMobileIsNotValid")]
	public string OwnerMobile { get; set; } = null!;

	[UValidationRequired("ownerNationalCodeIsRequired"), UValidationStringLength(10, 15, "ownerNationalCodeIsNotValid")]
	public string OwnerNationalCode { get; set; } = null!;

	public Guid? UserId { get; set; }
	public string? NationalCode { get; set; }
	public string? PostalCode { get; set; }
	public DateTime? RegisterDate { get; set; }
	public string? RegistrationNumber { get; set; }
	public string? Address { get; set; }
	public int? StartInvoiceNumber { get; set; }
	public string? IntroductionCode { get; set; }
}

public class MoadiUpdateParams : BaseUpdateParams<TagMoadi> {
	public string? Name { get; set; }
	public string? EconomicCode { get; set; }
	public string? LegalEntity { get; set; }
	public string? UniqueTaxCode { get; set; }
	public string? NationalCode { get; set; }
	public string? PostalCode { get; set; }
	public DateTime? RegisterDate { get; set; }
	public string? RegistrationNumber { get; set; }
	public string? Address { get; set; }
	public int? StartInvoiceNumber { get; set; }
	public string? IntroductionCode { get; set; }
	public string? OwnerName { get; set; }
	public string? OwnerMobile { get; set; }
	public string? OwnerNationalCode { get; set; }
}

public class MoadiRejectParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid Id { get; set; }

	public string? Reason { get; set; }
}

public class MoadiReadParams : BaseReadParams<TagMoadi> {
	public Guid? UserId { get; set; }
	public string? Name { get; set; }
	public string? EconomicCode { get; set; }
	public string? NationalCode { get; set; }
	public string? UniqueTaxCode { get; set; }
	public string? LegalEntity { get; set; }
	public string? Uuid { get; set; }

	public MoadiSelectorArgs SelectorArgs { get; set; } = new();
}
