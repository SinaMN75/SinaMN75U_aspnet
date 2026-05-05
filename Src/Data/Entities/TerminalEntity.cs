namespace SinaMN75U.Data.Entities;

[Table("Terminals")]
[Index(nameof(SimCardNumber), IsUnique = true, Name = "IX_Terminal_SimCardNumber")]
[Index(nameof(SimCardSerial), IsUnique = true, Name = "IX_Terminal_SimCardSerial")]
[Index(nameof(Imei), IsUnique = true, Name = "IX_Terminal_Imei")]
public sealed class TerminalEntity : BaseEntity<TagTerminal, BaseJsonData> {
	[MaxLength(40)]
	public required string Serial { get; set; }

	[MaxLength(40)]
	public string? SimCardNumber { get; set; }

	[MaxLength(40)]
	public string? SimCardSerial { get; set; }

	[MaxLength(40)]
	public string? Imei { get; set; }
	
	[MaxLength(40)]
	public string? TerminalId { get; set; }

	public Guid? MerchantId { get; set; }
	public MerchantEntity? Merchant { get; set; }
}