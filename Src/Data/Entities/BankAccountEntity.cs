namespace SinaMN75U.Data.Entities;

[Table("BankAccounts")]
public class BankAccountEntity : BaseEntity<TagBankAccount, BankAccountJson> {
	[MaxLength(20), MinLength(15)]
	public string? CardNumber { get; set; }

	[MaxLength(100)]
	public string? AccountNumber { get; set; }

	[MaxLength(100)]
	public string? IBanNumber { get; set; }

	[MaxLength(100)]
	public string? BankName { get; set; }

	[MaxLength(100)]
	public string? OwnerName { get; set; }

	[ForeignKey("FK_BankAccounts_UserId")]
	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;
}

public class BankAccountJson {
	public string? Description { get; set; }
}