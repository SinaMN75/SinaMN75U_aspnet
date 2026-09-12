namespace SinaMN75U.Data.Entities;

[Table("Terminals")]
[Microsoft.EntityFrameworkCore.Index(nameof(TerminalId), IsUnique = true, Name = "IX_Terminal_TerminalId")]
[Microsoft.EntityFrameworkCore.Index(nameof(SimCardSerial), IsUnique = true, Name = "IX_Terminal_SimCardSerial")]
[Microsoft.EntityFrameworkCore.Index(nameof(Imei), IsUnique = true, Name = "IX_Terminal_Imei")]
public sealed class TerminalEntity : BaseEntity<TagTerminal, TerminalJson> {
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

	public Guid? TerminalBrandId { get; set; }
	public TerminalBrandEntity? TerminalBrand { get; set; }

	public Guid? TerminalBrokerId { get; set; }
	public TerminalBrokerEntity? TerminalBroker { get; set; }

	public Guid? MerchantId { get; set; }
	public MerchantEntity? Merchant { get; set; }

	public byte[]? Agreement { get; set; }
}

public sealed class TerminalJson : BaseJson {
	public string? AgreementPath { get; set; }
}

[Table("TerminalBrands")]
public sealed class TerminalBrandEntity : BaseEntity<TagTerminalBrand, TerminalBrandJson> {

	[Required, MaxLength(40)]
	public required string Title { get; set; }
	
	[Required, MaxLength(40)]
	public required string Model { get; set; }

	public ICollection<TerminalEntity> Terminals { get; set; } = [];
}

public sealed class TerminalBrandJson : BaseJson;


[Table("TerminalBroker")]
public sealed class TerminalBrokerEntity : BaseEntity<TagTerminalBroker, TerminalBrokerJson> {

	[Required, MaxLength(40)]
	public required string Title { get; set; }
	
	public ICollection<TerminalEntity> Terminals { get; set; } = [];
}

public sealed class TerminalBrokerJson : BaseJson {
	public string? Sign1Base64 { get; set; }
	public string? Sign1Owner { get; set; }
	public string? Sign2Base64 { get; set; }
	public string? Sign2Owner { get; set; }
}
