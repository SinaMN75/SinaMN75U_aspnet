namespace SinaMN75U.Data.Entities;

[Table("BankAccounts")]
public sealed class BankAccountEntity : BaseEntity<TagBankAccount, BaseJsonData> {
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
}
