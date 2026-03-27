namespace SinaMN75U.Data.Params;

public sealed class BankAccountCreateParams : BaseCreateParams<TagBankAccount> {
	public string? CardNumber { get; set; }
	public string? AccountNumber { get; set; }
	public string? IBanNumber { get; set; }
	public string? BankName { get; set; }
	public string? OwnerName { get; set; }
	public string? Description { get; set; }
	public Guid? UserId { get; set; }
}

public sealed class BankAccountUpdateParams : BaseUpdateParams<TagBankAccount> {
	public string? CardNumber { get; set; }
	public string? AccountNumber { get; set; }
	public string? IBanNumber { get; set; }
	public string? BankName { get; set; }
	public string? OwnerName { get; set; }
	public string? Description { get; set; }
	public Guid? UserId { get; set; }
}

public sealed class BankAccountReadParams : BaseReadParams<TagBankAccount> {
	public BankAccountSelectorArgs SelectorArgs { get; set; } = new();
}