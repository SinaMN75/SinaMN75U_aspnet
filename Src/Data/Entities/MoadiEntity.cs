namespace SinaMN75U.Data.Entities;

[Table("Moadies")]
public sealed class MoadiEntity : BaseEntity<TagMoadi, MoadiJson> {
	[Required, StringLength(100)]
	public required string Name { get; set; }

	[Required, StringLength(30)]
	public required string EconomicCode { get; set; } // کد اقتصادی

	[Required, StringLength(20)]
	public required string LegalEntity { get; set; } // legal,natural,civic,foreigners,final_consumer

	[Required, StringLength(30)]
	public required string UniqueTaxCode { get; set; }

	[StringLength(10)]
	public string? NationalCode { get; set; }

	[StringLength(10)]
	public string? PostalCode { get; set; }

	public DateTime? RegisterDate { get; set; }

	[StringLength(10)]
	public string? RegistrationNumber { get; set; }

	[StringLength(1000)]
	public string? Address { get; set; }

	public int? StartInvoiceNumber { get; set; }

	[StringLength(50)]
	public string? IntroductionCode { get; set; }

	[Required, StringLength(100)]
	public required string OwnerName { get; set; }

	[Required, StringLength(15)]
	public required string OwnerMobile { get; set; }

	[Required, StringLength(15)]
	public required string OwnerNationalCode { get; set; }

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;
}

public sealed class MoadiJson : BaseJson {
	public string? Uuid { get; set; }
	public int? RegisterStep { get; set; }
	public string? CreatedType { get; set; }
	public long? OwnerId { get; set; }
	public bool ActiveContract { get; set; }
	public int InvoicesCount { get; set; }
	public int InvoicesSuccessCount { get; set; }
	public string? LastContractStatus { get; set; }
	public string? RejectReason { get; set; }
}
