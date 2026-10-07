namespace SinaMN75U.Data.Entities;

[Table("Accounts")]
public sealed class AccountEntity : BaseEntity<TagAccount, AccountJson> {
	[Required, MaxLength(20)]
	public required string Code { get; set; }

	[Required, MaxLength(100)]
	public required string Title { get; set; }

	public required Guid OrganizationId { get; set; }
	public OrganizationEntity Organization { get; set; } = null!;
}

public sealed class AccountJson : BaseJson {
	public Guid? RegisteredBy { get; set; }
}

[Table("Vouchers")]
public sealed class VoucherEntity : BaseEntity<TagVoucher, VoucherJson> {
	public required int Number { get; set; }
	public required DateTime Date { get; set; }
	public Guid? PlaceId { get; set; }
	public Guid? SourceId { get; set; }

	public required Guid OrganizationId { get; set; }
	public OrganizationEntity Organization { get; set; } = null!;

	public ICollection<VoucherLineEntity> Lines { get; set; } = [];
}

public sealed class VoucherJson : BaseJson {
	public Guid? RegisteredBy { get; set; }
}

[Table("VoucherLines")]
public sealed class VoucherLineEntity : BaseEntity<TagVoucher> {
	[Column(TypeName = "decimal(24,2)")]
	public required decimal Debit { get; set; }

	[Column(TypeName = "decimal(24,2)")]
	public required decimal Credit { get; set; }

	public Guid? PersonId { get; set; }

	[MaxLength(200)]
	public string? Description { get; set; }

	public required Guid VoucherId { get; set; }
	public VoucherEntity Voucher { get; set; } = null!;

	public required Guid AccountId { get; set; }
	public AccountEntity Account { get; set; } = null!;
}

[Table("Checks")]
public sealed class CheckEntity : BaseEntity<TagCheck, CheckJson> {
	[Column(TypeName = "decimal(24,2)")]
	public required decimal Amount { get; set; }

	public required DateTime DueDate { get; set; }

	[Required, MaxLength(30)]
	public required string Number { get; set; }

	[MaxLength(100)]
	public string? Bank { get; set; }

	public Guid? PersonId { get; set; }
	public Guid? ContractId { get; set; }
	public Guid? PlaceId { get; set; }

	public required Guid OrganizationId { get; set; }
	public OrganizationEntity Organization { get; set; } = null!;
}

public sealed class CheckJson : BaseJson {
	public string? SayadId { get; set; }
	public string? Drawer { get; set; }
	public Guid? AccountId { get; set; }
	public Guid? InvoiceId { get; set; }
	public Guid? RegisteredBy { get; set; }
	public bool DueReminded { get; set; }
}
