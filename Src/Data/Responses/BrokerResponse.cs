namespace SinaMN75U.Data.Responses;

public class BrokerResponse : BaseResponse<TagBroker, BrokerJson> {
	public required string Title { get; set; }
	public required string Code { get; set; }
	public Guid? AgreementTemplateId { get; set; }

	public IEnumerable<TerminalBrandResponse>? Brands { get; set; }
}

/// <summary>
/// Broker fields that are safe to hand to a non admin client. BrokerJson carries the provider
/// credentials, so it must never travel on a route a merchant user can call.
/// </summary>
public class BrokerBriefResponse {
	public required Guid Id { get; set; }
	public required string Title { get; set; }
	public string? LegalName { get; set; }
	public string? LogoBase64 { get; set; }
	public string? PhoneNumber { get; set; }
	public string? SupportPhoneNumber { get; set; }
}

public class TerminalBrandResponse : BaseResponse<TagTerminalBrand, TerminalBrandJson> {
	public required string Title { get; set; }
	public required string Code { get; set; }
	public required Guid BrokerId { get; set; }

	public BrokerBriefResponse? Broker { get; set; }
}

public class AgreementTemplateResponse : BaseResponse<TagAgreementTemplate, AgreementTemplateJson> {
	public required string Title { get; set; }
	public required string Code { get; set; }
}
