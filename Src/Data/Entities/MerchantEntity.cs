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

	[Required, StringLength(15)]
	public required string Landline { get; set; } // شماره ثابت

	[Required, StringLength(10)]
	public required string NationalCode { get; set; } // کد ملی 
	
	[Required, StringLength(100)]
	public required string BankAccountId { get; set; } // شماره حساب / شبا
	
	[Required, StringLength(20)]
	public required string Mcc { get; set; } // کد نصنف

	[Required, StringLength(20)]
	public required string MerchantId { get; set; }
	
	[Required, StringLength(20)]
	public required string InsId { get; set; }

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	public ICollection<TerminalEntity> Terminals { get; set; } = [];
	public ICollection<AgreementEntity> Agreements { get; set; } = [];
}

public sealed class MerchantJson : BaseJsonData {
	public required string BusinessTitle { get; set; } // عنوان کسب و کار
	public required string Address { get; set; } // آدرس محل پذیرنده (آدرس فروشگاه، قرارگیری ترمینال)
	public required string OwnerPhoneNumber { get; set; } // شماره موبایل مالک
	public int DefinitionTemplate { get; set; } = 1;
	public int SettlementCurrency { get; set; } = 364; // واحد پول تسویه
	public string? OwnerName { get; set; } // نام مالک فروشگاه
}