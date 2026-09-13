namespace SinaMN75U.Data.Params;

public class TerminalCreateParams : BaseCreateParams<TagTerminal> {
	[UValidationRequired("serialRequired")]
	public string Serial { get; set; } = null!;

	public required Guid TerminalBrandId { get; set; }
	public required Guid TerminalBrokerId { get; set; }

	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }
	public string? TerminalId { get; set; }
	public string? InsId { get; set; }
	public Guid? MerchantId { get; set; }
}

public class TerminalUpdateParams : BaseUpdateParams<TagTerminal> {
	public string? Serial { get; set; }
	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }
	public string? TerminalId { get; set; }
	public string? InsId { get; set; }
	public Guid? MerchantId { get; set; }
}

public class TerminalAssignParams : BaseParams {
	public string? Title { get; set; }
	public string Serial { get; set; } = null!;
	public string? SimCardSerial { get; set; }
	public Guid MerchantId { get; set; }
	public bool AcceptedAgreement { get; set; }
	public Guid? TerminalBrandId { get; set; }
	public Guid? TerminalBrokerId { get; set; }
}

public class TerminalRejectParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid Id { get; set; }

	public string? Reason { get; set; }
}

public class TerminalBulkCreateParams : BaseParams {
	public required List<TerminalCreateParams> List { get; set; }
}

public sealed class TerminalImportParams : BaseParams {
	[UValidationRequired("FileRequired")]
	public string File { get; set; } = null!;
}

public class TerminalReadParams : BaseReadParams<TagTerminal> {
	public string? Serial { get; set; }
	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }
	public string? TerminalId { get; set; }
	public string? InsId { get; set; }
	public Guid? MerchantId { get; set; }

	public TerminalSelectorArgs SelectorArgs { get; set; } = new();
}

public class TerminalBrandCreateParams : BaseCreateParams<TagTerminalBrand> {
	[UValidationRequired("TitleRequired")]
	public string Title { get; set; } = null!;

	[UValidationRequired("ModelIsRequired")]
	public string Model { get; set; } = null!;
}

public class TerminalBrandReadParams : BaseReadParams<TagTerminalBrand> {
	public string? Title { get; set; }
	public string? Model { get; set; }
	public TerminalBrandSelectorArgs SelectorArgs { get; set; } = new();
}

public class TerminalBrandUpdateParams : BaseUpdateParams<TagTerminalBrand> {
	public string? Title { get; set; }
	public string? Model { get; set; }
}

public class TerminalBrokerCreateParams : BaseCreateParams<TagTerminalBroker> {
	[UValidationRequired("TitleRequired")]
	public string Title { get; set; } = null!;

	[UValidationRequired("TitleRequired")]
	public string RegistrationNumber { get; set; } = null!;

	[UValidationRequired("TitleRequired")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("TitleRequired")]
	public string Representative { get; set; } = null!;

	[UValidationRequired("TitleRequired")]
	public string Address { get; set; } = null!;

	[UValidationRequired("TitleRequired")]
	public string PostalCode { get; set; } = null!;

	[UValidationRequired("TitleRequired")]
	public string PhoneNumber { get; set; } = null!;

	[UValidationRequired("TitleRequired")]
	public string Sign1Base64 { get; set; } = null!;

	[UValidationRequired("TitleRequired")]
	public string Sign1Owner { get; set; } = null!;

	[UValidationRequired("TitleRequired")]
	public string Sign2Base64 { get; set; } = null!;

	[UValidationRequired("TitleRequired")]
	public string Sign2Owner { get; set; } = null!;

	[UValidationRequired("TitleRequired")]
	public string LogoBase64 { get; set; } = null!;
}

public class TerminalBrokerReadParams : BaseReadParams<TagTerminalBroker> {
	public string? Title { get; set; }

	public TerminalBrokerSelectorArgs SelectorArgs { get; set; } = new();
}

public class TerminalBrokerUpdateParams : BaseUpdateParams<TagTerminalBroker> {
	public string? Title { get; set; }

	public string? RegistrationNumber { get; set; }
	public string? NationalCode { get; set; }
	public string? Representative { get; set; }
	public string? Address { get; set; }
	public string? PostalCode { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Sign1Base64 { get; set; }
	public string? Sign1Owner { get; set; }
	public string? Sign2Base64 { get; set; }
	public string? Sign2Owner { get; set; }
	public string? LogoBase64 { get; set; }
}