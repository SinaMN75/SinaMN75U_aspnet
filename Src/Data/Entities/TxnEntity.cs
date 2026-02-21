namespace SinaMN75U.Data.Entities;

[Table("Txns")]
[Index(nameof(TrackingNumber), IsUnique = true)]
[Index(nameof(UserId))]
public class TxnEntity : BaseEntity<TagTxn, TxnJson> {
	[Required, Column(TypeName = "decimal(18,2)")]
	public required decimal Amount { get; set; }

	[Required, MaxLength(100)]
	public required string TrackingNumber { get; set; }
	
	public required Guid InvoiceId { get; set; }
	public InvoiceEntity Invoice { get; set; } = null!;

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	public TxnResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		Amount = Amount,
		TrackingNumber = TrackingNumber,
		UserId = UserId
	};
}

public class TxnJson {
	public string? GatewayName { get; set; }
}