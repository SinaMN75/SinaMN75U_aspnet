namespace SinaMN75U.Data.Entities;

[Table("AgreementTemplates")]
[Microsoft.EntityFrameworkCore.Index(nameof(Code), IsUnique = true, Name = "IX_AgreementTemplate_Code")]
public sealed class AgreementTemplateEntity : BaseEntity<TagAgreementTemplate, AgreementTemplateJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	[Required, MaxLength(50)]
	public required string Code { get; set; }
}

public sealed class AgreementTemplateJson : BaseJson {
	public string? HeaderTitle { get; set; }
	public List<AgreementTemplateBlock> Blocks { get; set; } = [];
}

public sealed class AgreementTemplateBlock {
	public AgreementBlockType Type { get; set; }
	public string Text { get; set; } = "";
	public int Order { get; set; }
}
