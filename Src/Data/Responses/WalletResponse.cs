namespace SinaMN75U.Data.Responses;

public sealed class WalletResponse : BaseResponse<TagWallet, WalletJson> {
	public UserResponse? User { get; set; }
	public required Guid UserId { get; set; }

	public required decimal Balance { get; set; }
}

public sealed class WalletTxnResponse : BaseResponse<TagWalletTxn, WalletTxnJson> {
	public required UserResponse Sender { get; set; }
	public required Guid SenderId { get; set; }

	public required UserResponse Receiver { get; set; }
	public required Guid ReceiverId { get; set; }

	public required decimal Amount { get; set; }
}