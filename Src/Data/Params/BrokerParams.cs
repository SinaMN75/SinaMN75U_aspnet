namespace SinaMN75U.Data.Params;

public class BrokerCreateParams : BaseCreateParams<TagBroker> {
	[UValidationRequired("titleIsRequired")]
	public string Title { get; set; } = null!;

	[UValidationRequired("codeIsRequired")]
	public string Code { get; set; } = null!;

	public string? LegalName { get; set; }
	public string? RegistrationNumber { get; set; }
	public string? NationalId { get; set; }
	public string? Address { get; set; }
	public string? PostalCode { get; set; }
	public string? PhoneNumber { get; set; }
	public string? SupportPhoneNumber { get; set; }
	public string? CallCenterPhoneNumber { get; set; }
	public string? RepresentativeName { get; set; }
	public string? RepresentativeRole { get; set; }
	public string? LogoBase64 { get; set; }
	public string? ThemeColor { get; set; }
	public string? ContractNumberSuffix { get; set; }

	public TagBrokerProvider? Provider { get; set; }
	public string? ProviderBaseUrl { get; set; }
	public string? ProviderAuthHeader { get; set; }
	public string? ProviderProject { get; set; }
	public int? ProviderDefinitionTemplate { get; set; }

	public Guid? AgreementTemplateId { get; set; }
	public List<BrokerSignatory> Signatories { get; set; } = [];
}

public class BrokerUpdateParams : BaseUpdateParams<TagBroker> {
	public string? Title { get; set; }
	public string? Code { get; set; }

	public string? LegalName { get; set; }
	public string? RegistrationNumber { get; set; }
	public string? NationalId { get; set; }
	public string? Address { get; set; }
	public string? PostalCode { get; set; }
	public string? PhoneNumber { get; set; }
	public string? SupportPhoneNumber { get; set; }
	public string? CallCenterPhoneNumber { get; set; }
	public string? RepresentativeName { get; set; }
	public string? RepresentativeRole { get; set; }
	public string? LogoBase64 { get; set; }
	public string? ThemeColor { get; set; }
	public string? ContractNumberSuffix { get; set; }

	public TagBrokerProvider? Provider { get; set; }
	public string? ProviderBaseUrl { get; set; }
	public string? ProviderAuthHeader { get; set; }
	public string? ProviderProject { get; set; }
	public int? ProviderDefinitionTemplate { get; set; }

	public Guid? AgreementTemplateId { get; set; }
	public List<BrokerSignatory>? Signatories { get; set; }
}

public class BrokerReadParams : BaseReadParams<TagBroker> {
	public string? Title { get; set; }
	public string? Code { get; set; }

	public BrokerSelectorArgs SelectorArgs { get; set; } = new();
}

public class TerminalBrandCreateParams : BaseCreateParams<TagTerminalBrand> {
	[UValidationRequired("titleIsRequired")]
	public string Title { get; set; } = null!;

	[UValidationRequired("codeIsRequired")]
	public string Code { get; set; } = null!;

	[UValidationRequired("brokerIsRequired")]
	public Guid BrokerId { get; set; }

	public bool? RequiresSimCardSerial { get; set; }
	public bool? RequiresImei { get; set; }
	public string? ImageBase64 { get; set; }
	public int? Order { get; set; }
	public Guid? AgreementTemplateId { get; set; }
	public TagTerminal? LegacyTag { get; set; }
}

public class TerminalBrandUpdateParams : BaseUpdateParams<TagTerminalBrand> {
	public string? Title { get; set; }
	public string? Code { get; set; }
	public Guid? BrokerId { get; set; }

	public bool? RequiresSimCardSerial { get; set; }
	public bool? RequiresImei { get; set; }
	public string? ImageBase64 { get; set; }
	public int? Order { get; set; }
	public Guid? AgreementTemplateId { get; set; }
	public TagTerminal? LegacyTag { get; set; }
}

public class TerminalBrandReadParams : BaseReadParams<TagTerminalBrand> {
	public string? Title { get; set; }
	public string? Code { get; set; }
	public Guid? BrokerId { get; set; }

	public TerminalBrandSelectorArgs SelectorArgs { get; set; } = new();
}

public class AgreementTemplateCreateParams : BaseCreateParams<TagAgreementTemplate> {
	[UValidationRequired("titleIsRequired")]
	public string Title { get; set; } = null!;

	[UValidationRequired("codeIsRequired")]
	public string Code { get; set; } = null!;

	public string? HeaderTitle { get; set; }
	public List<AgreementTemplateBlock> Blocks { get; set; } = [];
}

public class AgreementTemplateUpdateParams : BaseUpdateParams<TagAgreementTemplate> {
	public string? Title { get; set; }
	public string? Code { get; set; }
	public string? HeaderTitle { get; set; }
	public List<AgreementTemplateBlock>? Blocks { get; set; }
}

public class AgreementTemplateReadParams : BaseReadParams<TagAgreementTemplate> {
	public string? Title { get; set; }
	public string? Code { get; set; }

	public AgreementTemplateSelectorArgs SelectorArgs { get; set; } = new();
}
