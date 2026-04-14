namespace SinaMN75U.Data.Entities;

[Table("Wallets")]
public sealed class WalletEntity : BaseEntity<TagWallet, GeneralJsonData> {
	
	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	public required decimal Balance { get; set; }

	public WalletResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		User = User.MapToResponse(),
		UserId = UserId,
		Balance = Balance
	};
}

public sealed class WalletTxnEntity : BaseEntity<TagWalletTxn, GeneralJsonData> {
	public UserEntity Sender { get; set; } = null!;
	public required Guid SenderId { get; set; }

	public UserEntity Receiver { get; set; } = null!;
	public required Guid ReceiverId { get; set; }

	public required decimal Amount { get; set; }
}
