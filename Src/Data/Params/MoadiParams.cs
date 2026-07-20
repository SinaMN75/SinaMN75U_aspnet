namespace SinaMN75U.Data.Params;

public class MoadiCreateParams : BaseCreateParams<TagMoadi> {
	[UValidationRequired("NameRequired"), UValidationStringLength(1, 255, "NameNotValid")]
	public string Name { get; set; } = null!;

	[UValidationRequired("EconomicCodeRequired"), UValidationStringLength(1, 30, "EconomicCodeNotValid")]
	public string EconomicCode { get; set; } = null!;

	[UValidationRequired("LegalEntityRequired"), UValidationStringLength(1, 20, "LegalEntityNotValid")]
	public string LegalEntity { get; set; } = null!;

	[UValidationRequired("UniqueTaxCodeRequired"), UValidationStringLength(1, 30, "UniqueTaxCodeNotValid")]
	public string UniqueTaxCode { get; set; } = null!;

	[UValidationRequired("OwnerNameRequired"), UValidationStringLength(1, 255, "OwnerNameNotValid")]
	public string OwnerName { get; set; } = null!;

	[UValidationRequired("OwnerMobileRequired"), UValidationStringLength(10, 15, "OwnerMobileNotValid")]
	public string OwnerMobile { get; set; } = null!;

	[UValidationRequired("OwnerNationalCodeRequired"), UValidationStringLength(10, 15, "OwnerNationalCodeNotValid")]
	public string OwnerNationalCode { get; set; } = null!;

	public Guid? UserId { get; set; }
	public string? NationalCode { get; set; }
	public string? PostalCode { get; set; }
	public DateTime? RegistrationDate { get; set; }
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
	public DateTime? RegistrationDate { get; set; }
	public string? RegistrationNumber { get; set; }
	public string? Address { get; set; }
	public int? StartInvoiceNumber { get; set; }
	public string? IntroductionCode { get; set; }
	public string? OwnerName { get; set; }
	public string? OwnerMobile { get; set; }
	public string? OwnerNationalCode { get; set; }
}

public class MoadiRejectParams : BaseParams {
	[UValidationRequired("IdRequired")]
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
