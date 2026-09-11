namespace SinaMN75U.Data.Entities;

[Table("Txn")]
[Microsoft.EntityFrameworkCore.Index(nameof(TrackingNumber), IsUnique = true, Name = "IX_Txn_TrackingNumber")]
[Microsoft.EntityFrameworkCore.Index(nameof(UserId), Name = "IX_Txn_UserId")]
public sealed class TxnEntity : BaseEntity<TagTxn, BaseJson> {
	
	[Required, Column(TypeName = "decimal(18,2)")]
	public required decimal Amount { get; set; }
	
	[Required, MaxLength(100)]
	public required string TrackingNumber { get; set; }

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;
}
