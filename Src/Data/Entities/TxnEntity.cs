namespace SinaMN75U.Data.Entities;

[Table("Txn")]
[Index(nameof(TrackingNumber), IsUnique = true, Name = "IX_Txn_TrackingNumber")]
[Index(nameof(UserId), Name = "IX_Txn_UserId")]
public sealed class TxnEntity : BaseEntity<TagTxn, GeneralJsonData> {
	[Required]
	[Column(TypeName = "decimal(18,2)")]
	public required decimal Amount { get; set; }

	[Required]
	[MaxLength(100)]
	public required string TrackingNumber { get; set; }

	public Guid? InvoiceId { get; set; }
	public InvoiceEntity? Invoice { get; set; }

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	public TxnResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		JsonData = JsonData,
		Tags = Tags,
		Amount = Amount,
		TrackingNumber = TrackingNumber,
		UserId = UserId
	};
}
