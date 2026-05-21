namespace SinaMN75U.Data.Entities;

[Table("Terminals")]
[Index(nameof(TerminalId), IsUnique = true, Name = "IX_Terminal_TerminalId")]
[Index(nameof(SimCardSerial), IsUnique = true, Name = "IX_Terminal_SimCardSerial")]
[Index(nameof(Imei), IsUnique = true, Name = "IX_Terminal_Imei")]
public sealed class TerminalEntity : BaseEntity<TagTerminal, BaseJson> {
	[Required, MaxLength(40)]
	public required string Serial { get; set; }

	[MaxLength(40)]
	public string? SimCardNumber { get; set; }

	[MaxLength(40)]
	public string? SimCardSerial { get; set; }

	[MaxLength(40)]
	public string? Imei { get; set; }
	
	[MaxLength(40)]
	public string? TerminalId { get; set; }	
	
	[MaxLength(40)]
	public string? InsId { get; set; }

	public Guid? MerchantId { get; set; }
	public MerchantEntity? Merchant { get; set; }

	public string? Agreement { get; set; }
}