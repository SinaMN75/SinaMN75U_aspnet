namespace SinaMN75U.Data.Responses;

public sealed class TxnResponse : BaseResponse<TagTxn, BaseJson> {
	public decimal Amount { get; set; }
	public string? TrackingNumber { get; set; }

	public Guid? UserId { get; set; }
	public UserResponse? User { get; set; }
}