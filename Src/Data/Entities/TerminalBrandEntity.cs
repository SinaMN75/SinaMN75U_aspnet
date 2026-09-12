namespace SinaMN75U.Data.Entities;

[Table("TerminalBrands")]
[Microsoft.EntityFrameworkCore.Index(nameof(Code), IsUnique = true, Name = "IX_TerminalBrand_Code")]
public sealed class TerminalBrandEntity : BaseEntity<TagTerminalBrand, TerminalBrandJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	[Required, MaxLength(50)]
	public required string Code { get; set; }

	public required Guid BrokerId { get; set; }
	public BrokerEntity Broker { get; set; } = null!;
}

public sealed class TerminalBrandJson : BaseJson {
	public bool RequiresSimCardSerial { get; set; } = true;
	public bool RequiresImei { get; set; }
	public string? ImageBase64 { get; set; }
	public int? Order { get; set; }

	public Guid? AgreementTemplateId { get; set; }

	/// <summary>
	/// The TagTerminal value this brand replaced. Kept so terminals imported before the
	/// brand tables existed still resolve, and so old app builds that send a tag keep working.
	/// </summary>
	public TagTerminal? LegacyTag { get; set; }
}
