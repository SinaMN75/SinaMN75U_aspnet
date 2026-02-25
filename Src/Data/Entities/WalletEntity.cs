namespace SinaMN75U.Data.Entities;

public sealed class WalletEntity: BaseEntity<TagWallet, WalletJson> {
	public UserEntity User { get; set; } = null!;
	public required Guid UserId { get; set; }

	public required decimal Balance { get; set; }
}

public sealed class WalletJson {
	public string? Description { get; set; }
}

public sealed class WalletTxnEntity: BaseEntity<TagWalletTxn, WalletTxnJson> {
	public UserEntity Sender { get; set; } = null!;
	public required Guid SenderId { get; set; }
	
	public UserEntity Receiver { get; set; } = null!;
	public required Guid ReceiverId { get; set; }

	public required decimal Amount { get; set; }
}

public sealed class WalletTxnJson {
	public string? Description { get; set; }
}
