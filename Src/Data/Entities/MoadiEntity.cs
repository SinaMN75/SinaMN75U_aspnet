namespace SinaMN75U.Data.Entities;

[Table("Moadies")]
public sealed class MoadiEntity : BaseEntity<TagMoadi, MoadiJson> {
	[Required, StringLength(255)]
	public required string Name { get; set; } // نام مودی

	[Required, StringLength(30)]
	public required string EconomicCode { get; set; } // کد اقتصادی

	[Required, StringLength(20)]
	public required string LegalEntity { get; set; } // legal,natural,civic,foreigners,final_consumer

	[Required, StringLength(30)]
	public required string UniqueTaxCode { get; set; } // شناسه یکتای مالیاتی (ebill)

	[StringLength(15)]
	public string? NationalCode { get; set; } // کد ملی مودی

	[StringLength(10)]
	public string? PostalCode { get; set; } // کد پستی

	[StringLength(10)]
	public string? RegistrationDate { get; set; } // تاریخ ثبت (Y-m-d)

	[StringLength(10)]
	public string? RegistrationNumber { get; set; } // شماره ثبت

	[StringLength(1000)]
	public string? Address { get; set; } // آدرس

	public int? StartInvoiceNumber { get; set; } // شماره شروع فاکتور

	[StringLength(50)]
	public string? IntroductionCode { get; set; } // کد معرف

	[Required, StringLength(255)]
	public required string OwnerName { get; set; } // نام مالک

	[Required, StringLength(15)]
	public required string OwnerMobile { get; set; } // موبایل مالک

	[Required, StringLength(15)]
	public required string OwnerNationalCode { get; set; } // کد ملی مالک

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;
}

public sealed class MoadiJson : BaseJson {
	public string? Uuid { get; set; } // شناسه یکتای مودی در سامانه نما
	public int? RegisterStep { get; set; } // مرحله ثبت نام (۳ یعنی کامل)
	public string? CreatedType { get; set; }
	public long? OwnerId { get; set; }
	public bool ActiveContract { get; set; } // وجود قرارداد فعال
	public int InvoicesCount { get; set; }
	public int InvoicesSuccessCount { get; set; }
	public string? LastContractStatus { get; set; }
	public string? RejectReason { get; set; } // دلیل رد درخواست توسط ادمین
}
