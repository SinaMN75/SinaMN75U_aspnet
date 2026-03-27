namespace SinaMN75U.Data.Entities;

[Table("Wallets")]
public class WalletEntity : BaseEntity<TagWallet, WalletJson> {
	public UserEntity User { get; set; } = null!;
	public required Guid UserId { get; set; }

	public required decimal Balance { get; set; }

	public WalletResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		User = User.MapToResponse(),
		UserId = UserId,
		Balance = Balance
	};
}

public class WalletJson {
	public string? Description { get; set; }
}

public class WalletTxnEntity : BaseEntity<TagWalletTxn, WalletTxnJson> {
	public UserEntity Sender { get; set; } = null!;
	public required Guid SenderId { get; set; }

	public UserEntity Receiver { get; set; } = null!;
	public required Guid ReceiverId { get; set; }

	public required decimal Amount { get; set; }
}

public class WalletTxnJson {
	public string? Description { get; set; }
}