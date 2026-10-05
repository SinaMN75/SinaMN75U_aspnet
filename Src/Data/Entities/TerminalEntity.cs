namespace SinaMN75U.Data.Entities;

[Table("Merchants")]
public sealed class MerchantEntity : BaseEntity<TagMerchant, MerchantJson> {
	[Required, StringLength(10)]
	public required string ZipCode { get; set; }

	[Required, StringLength(20)]
	public required string CityCode { get; set; } // کد شهر (استاندارد شاپرک

	[Required, StringLength(15)]
	public required string PhoneNumber { get; set; } // شماره موبایل پذیرنده

	[Required, StringLength(100)]
	public required string Title { get; set; } // عنوان فروشگاه

	[Required, MinLength(6), MaxLength(15)]
	public required string Landline { get; set; } // شماره ثابت

	[Required, StringLength(10)]
	public required string NationalCode { get; set; } // کد ملی 

	[Required, StringLength(20)]
	public required string Mcc { get; set; } // کد نصنف

	[MaxLength(50)]
	public string? BankAccountId { get; set; } // شماره حساب / شبا

	[MaxLength(50)]
	public string? MerchantId { get; set; }

	[StringLength(20)]
	public string? InsId { get; set; }

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	public ICollection<TerminalEntity> Terminals { get; set; } = [];
}

public sealed class MerchantJson : BaseJson {
	public string? BusinessTitle { get; set; } // عنوان کسب و کار
	public string? Address { get; set; } // آدرس محل پذیرنده (آدرس فروشگاه، قرارگیری ترمینال)
	public string? OwnerPhoneNumber { get; set; } // شماره موبایل مالک
	public int DefinitionTemplate { get; set; } = 1;
	public int SettlementCurrency { get; set; } = 364; // واحد پول تسویه
	public string? OwnerName { get; set; } // نام مالک فروشگاه
}

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

	public required Guid TerminalBrandId { get; set; }
	public TerminalBrandEntity TerminalBrand { get; set; } = null!;

	public required Guid TerminalBrokerId { get; set; }
	public TerminalBrokerEntity TerminalBroker { get; set; } = null!;

	public Guid? MerchantId { get; set; }
	public MerchantEntity? Merchant { get; set; }
	
	public string? Agreement { get; set; }
}

public sealed class TerminalJson : BaseJson;

[Table("TerminalBrands")]
[Microsoft.EntityFrameworkCore.Index(nameof(Code), IsUnique = true, Name = "IX_TerminalBrands_Code")]
public sealed class TerminalBrandEntity : BaseEntity<TagTerminalBrand, TerminalBrandJson> {
	[Required, MaxLength(40)]
	public required string Code { get; set; }

	[Required, MaxLength(40)]
	public required string Title { get; set; }
	
	[Required, MaxLength(40)]
	public required string Model { get; set; }

	public ICollection<TerminalEntity> Terminals { get; set; } = [];
}

public sealed class TerminalBrandJson : BaseJson {
	public string? Agreement { get; set; }
}


[Table("TerminalBroker")]
[Microsoft.EntityFrameworkCore.Index(nameof(Code), IsUnique = true, Name = "IX_TerminalBroker_Code")]
public sealed class TerminalBrokerEntity : BaseEntity<TagTerminalBroker, TerminalBrokerJson> {
	[Required, MaxLength(40)]
	public required string Code { get; set; }

	[Required, MaxLength(40)]
	public required string Title { get; set; }
	
	public ICollection<TerminalEntity> Terminals { get; set; } = [];
}

public sealed class TerminalBrokerJson : BaseJson {
	public string? RegistrationNumber { get; set; }
	public string? NationalCode { get; set; }
	public string? Representative { get; set; }
	public string? Address { get; set; }
	public string? PostalCode { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Sign1Base64 { get; set; }
	public string? Sign1Owner { get; set; }
	public string? Sign2Base64 { get; set; }
	public string? Sign2Owner { get; set; }
	public string? LogoBase64 { get; set; }
}
