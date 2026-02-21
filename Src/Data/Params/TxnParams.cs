namespace SinaMN75U.Data.Params;

public sealed class TxnCreateParams : BaseCreateParams<TagTxn> {
	public required Guid ReservationId { get; set; }
	public required decimal Amount { get; set; }
	public required string TrackingNumber { get; set; }
	public required Guid InvoiceId { get; set; }
	public string? GatewayName { get; set; }

	public TxnEntity MapToEntity(Guid userId) => new() {
		Id = Id ?? Guid.CreateVersion7(),
		Tags = Tags,
		TrackingNumber = TrackingNumber,
		Amount = Amount,
		JsonData = new TxnJson {
			GatewayName = GatewayName
		},
		UserId = userId,
		InvoiceId = InvoiceId
	};
}

public sealed class TxnUpdateParams : BaseUpdateParams<TagTxn> {
	public decimal? Amount { get; set; }
	public string? TrackingNumber { get; set; }
	public DateTime? PaidAt { get; set; }
	public string? GatewayName { get; set; }
}

public sealed class TxnReadParams : BaseReadParams<TagTxn> {
	public TxnSelectorArgs SelectorArgs { get; set; } = new();
}