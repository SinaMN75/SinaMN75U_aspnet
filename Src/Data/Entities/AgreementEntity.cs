namespace SinaMN75U.Data.Entities;

[Table("Agreements")]
public sealed class AgreementEntity : BaseEntity<TagAgreement, GeneralJsonData> {
	public required string Agreement { get; set; }
	
	public required Guid TerminalId { get; set; }
	public TerminalEntity Terminal { get; set; } = null!;
	
	public AgreementResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		TerminalId = TerminalId,
		Agreement =  Agreement
	};
}
