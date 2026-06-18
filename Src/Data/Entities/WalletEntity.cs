namespace SinaMN75U.Data.Entities;

[Table("Wallets")]
public sealed class WalletEntity : BaseEntity<TagWallet, WalletJson> {
	public required decimal Balance { get; set; }
}

public sealed class WalletJson : BaseJson {
	public bool AllowMinusBalance { get; set; } = false;
}

[Table("WalletTxns")]
public sealed class WalletTxnEntity : BaseEntity<TagWalletTxn, BaseJson> {
	public UserEntity Sender { get; set; } = null!;
	public required Guid SenderId { get; set; }

	public UserEntity Receiver { get; set; } = null!;
	public required Guid ReceiverId { get; set; }

	public required decimal Amount { get; set; }
}
