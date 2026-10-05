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

	[UValidationRequired("landlineIsRequired"), UValidationStringLength(6, 15, "landlineIsNotValid")]
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

public class TerminalCreateParams : BaseCreateParams<TagTerminal> {
	[UValidationRequired("serialIsRequired")]
	public string Serial { get; set; } = null!;

	[UValidationRequired("brandIsRequired")]
	public Guid TerminalBrandId { get; set; }

	[UValidationRequired("brokerIsRequired")]
	public Guid TerminalBrokerId { get; set; }

	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }
	public string? TerminalId { get; set; }
	public string? InsId { get; set; }
	public Guid? MerchantId { get; set; }
}

public class TerminalUpdateParams : BaseUpdateParams<TagTerminal> {
	public Guid? TerminalBrandId { get; set; }
	public Guid? TerminalBrokerId { get; set; }

	public string? Serial { get; set; }
	public string? SimCardNumber { get; set; }
	public string? SimCardSerial { get; set; }
	public string? Imei { get; set; }
	public string? TerminalId { get; set; }
	public string? InsId { get; set; }
	public Guid? MerchantId { get; set; }
}

public class TerminalAssignParams : BaseParams {
	[UValidationRequired("titleIsRequired")]
	public string Title { get; set; } = null!;

	[UValidationRequired("serialIsRequired")]
	public string Serial { get; set; } = null!;

	[UValidationRequired("merchantIsRequired")]
	public Guid MerchantId { get; set; }

	[UValidationRequired("brandIsRequired")]
	public Guid TerminalBrandId { get; set; }

	[UValidationRequired("brokerIsRequired")]
	public Guid TerminalBrokerId { get; set; }
	
	public string? SimCardSerial { get; set; }
	public bool AcceptedAgreement { get; set; }
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
	public Guid? TerminalBrandId { get; set; }
	public Guid? TerminalBrokerId { get; set; }

	public TerminalSelectorArgs SelectorArgs { get; set; } = new();
}

public class TerminalBrandCreateParams : BaseCreateParams<TagTerminalBrand> {
	[UValidationRequired("codeIsRequired")]
	public string Code { get; set; } = null!;

	[UValidationRequired("TitleRequired")]
	public string Title { get; set; } = null!;

	[UValidationRequired("ModelIsRequired")]
	public string Model { get; set; } = null!;

	public string? Agreement { get; set; }
}

public class TerminalBrandReadParams : BaseReadParams<TagTerminalBrand> {
	public string? Code { get; set; }
	public string? Title { get; set; }
	public string? Model { get; set; }
	public TerminalBrandSelectorArgs SelectorArgs { get; set; } = new();
}

public class TerminalBrandUpdateParams : BaseUpdateParams<TagTerminalBrand> {
	public string? Code { get; set; }
	public string? Title { get; set; }
	public string? Model { get; set; }
	public string? Agreement { get; set; }
}

public class TerminalBrokerCreateParams : BaseCreateParams<TagTerminalBroker> {
	[UValidationRequired("codeIsRequired")]
	public string Code { get; set; } = null!;

	[UValidationRequired("TitleRequired")]
	public string Title { get; set; } = null!;

	[UValidationRequired("registrationNumberIsRequired")]
	public string RegistrationNumber { get; set; } = null!;

	[UValidationRequired("nationalCodeIsRequired")]
	public string NationalCode { get; set; } = null!;

	[UValidationRequired("representativeIsRequired")]
	public string Representative { get; set; } = null!;

	[UValidationRequired("addressIsRequired")]
	public string Address { get; set; } = null!;

	[UValidationRequired("postalCodeIsRequired")]
	public string PostalCode { get; set; } = null!;

	[UValidationRequired("phoneNumberIsRequired")]
	public string PhoneNumber { get; set; } = null!;

	[UValidationRequired("signatureIsRequired")]
	public string Sign1Base64 { get; set; } = null!;

	[UValidationRequired("signatureIsRequired")]
	public string Sign1Owner { get; set; } = null!;

	public string? Sign2Base64 { get; set; }
	public string? Sign2Owner { get; set; }

	[UValidationRequired("logoIsRequired")]
	public string LogoBase64 { get; set; } = null!;
}

public class TerminalBrokerReadParams : BaseReadParams<TagTerminalBroker> {
	public string? Code { get; set; }
	public string? Title { get; set; }

	public TerminalBrokerSelectorArgs SelectorArgs { get; set; } = new();
}

public class TerminalBrokerUpdateParams : BaseUpdateParams<TagTerminalBroker> {
	public string? Code { get; set; }
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