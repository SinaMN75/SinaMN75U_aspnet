namespace SinaMN75U.Data.Entities;

[Table("Merchants")]
public sealed class MerchantEntity : BaseEntity<TagMerchant, BaseJsonData> {

	[Required, StringLength(10)]
	public required string ZipCode { get; set; }

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;
	
	public ICollection<TerminalEntity> Terminals { get; set; } = [];
	public ICollection<AgreementEntity> Agreements { get; set; } = [];
}