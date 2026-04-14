namespace SinaMN75U.Data.Responses;

public sealed class TxnResponse : BaseResponse<TagTxn, GeneralJsonData> {
	public decimal Amount { get; set; }
	public string? TrackingNumber { get; set; }

	public Guid? InvoiceId { get; set; }
	public InvoiceResponse? Invoice { get; set; }

	public Guid? UserId { get; set; }
	public UserResponse? User { get; set; }
}