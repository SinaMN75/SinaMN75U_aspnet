namespace SinaMN75U.Data.Entities;

[Table("Brokers")]
[Microsoft.EntityFrameworkCore.Index(nameof(Code), IsUnique = true, Name = "IX_Broker_Code")]
public sealed class BrokerEntity : BaseEntity<TagBroker, BrokerJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	[Required, MaxLength(50)]
	public required string Code { get; set; }

	public Guid? AgreementTemplateId { get; set; }
	public AgreementTemplateEntity? AgreementTemplate { get; set; }

	[InverseProperty("Broker")]
	public ICollection<TerminalBrandEntity> Brands { get; set; } = [];
}

public sealed class BrokerJson : BaseJson {
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

	public TagBrokerProvider Provider { get; set; } = TagBrokerProvider.Avreen;
	public string? ProviderBaseUrl { get; set; }
	public string? ProviderAuthHeader { get; set; }
	public string? ProviderProject { get; set; }
	public int ProviderDefinitionTemplate { get; set; } = 1;

	public List<BrokerSignatory> Signatories { get; set; } = [];
}

public sealed class BrokerSignatory {
	public string? Name { get; set; }
	public string? Role { get; set; }
	public string? SignatureBase64 { get; set; }
	public int? Order { get; set; }
}
