namespace SinaMN75U.Data.Responses;

public class TxnResponse : BaseResponse<TagTxn, TxnJson> {
	public decimal Amount { get; set; }
	public string? TrackingNumber { get; set; }
	public DateTime? PaidAt { get; set; }
	
	public Guid? InvoiceId { get; set; }
	public InvoiceResponse? Invoice { get; set; }
	
	public Guid? UserId { get; set; }
	public UserResponse? User { get; set; }
}