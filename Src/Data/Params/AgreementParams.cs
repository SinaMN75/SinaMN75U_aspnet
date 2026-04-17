namespace SinaMN75U.Data.Params;

public sealed class GenerateAgreementParams : BaseCreateParams<TagAgreement> {
	[UValidationRequired("IdRequired")]
	public Guid TerminalId { get; set; }
}

public sealed class AgreementUpdateParams : BaseUpdateParams<TagAgreement> {
	public string? Agreement { get; set; }
}

public sealed class AgreementReadParams : BaseReadParams<TagAgreement> {
	public AgreementSelectorArgs SelectorArgs { get; set; } = new();
}