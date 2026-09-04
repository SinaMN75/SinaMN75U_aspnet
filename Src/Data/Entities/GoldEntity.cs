namespace SinaMN75U.Data.Entities;

// The provider account is a single business account, so each app user's gold is tracked in this ledger and the
// provider is only used to move the aggregate position.
[Table("GoldWallets")]
public sealed class GoldWalletEntity : BaseEntity<TagGoldAsset, GoldWalletJson> {
	public required decimal Balance { get; set; }
}

public sealed class GoldWalletJson : BaseJson {
	public bool Locked { get; set; } = false;
}

[Table("GoldTxns")]
public sealed class GoldTxnEntity : BaseEntity<TagGoldTxn, GoldTxnJson> {
	public UserEntity User { get; set; } = null!;
	public required Guid UserId { get; set; }

	public required decimal GoldAmount { get; set; }

	public required decimal Amount { get; set; }

	public decimal UnitPrice { get; set; }

	[StringLength(100)]
	public string? OrderId { get; set; }

	[Required, StringLength(100)]
	public required string IdempotencyKey { get; set; }
}

public sealed class GoldTxnJson : BaseJson {
	public decimal? FeeAmount { get; set; }
	public string? FeeAsset { get; set; }
	public decimal? RequestedGoldAmount { get; set; }
	public decimal? RequestedAmount { get; set; }
	public decimal? ReservedAmount { get; set; }
	public decimal? ReservedGoldAmount { get; set; }
	public string? ProviderStatus { get; set; }
	public string? Error { get; set; }
}
