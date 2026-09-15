namespace SinaMN75U.Data.Entities;

[Table("Wallets")]
public sealed class WalletEntity : BaseEntity<TagWallet, WalletJson> {
	public required decimal Balance { get; set; }
}

public sealed class WalletJson : BaseJson {
	public bool AllowMinusBalance { get; set; } = false;
}

[Table("WalletTxns")]
public sealed class WalletTxnEntity : BaseEntity<TagWalletTxn, WalletTxnJson> {
	public UserEntity Sender { get; set; } = null!;
	public required Guid SenderId { get; set; }

	public UserEntity Receiver { get; set; } = null!;
	public required Guid ReceiverId { get; set; }

	public required decimal Amount { get; set; }
}

public class WalletTxnJson : BaseJson {
	public List<KeyValue> KeyValues { get; set; } = [];
}