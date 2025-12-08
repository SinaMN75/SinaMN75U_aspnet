namespace SinaMN75U.Data.Responses;

public class InvoiceResponse : BaseResponse<TagInvoice, InvoiceJson> {
	public required double DebtAmount { get; set; }
	public required double CreditorAmount { get; set; }
	public required double PaidAmount { get; set; }
	public required double PenaltyAmount { get; set; }
	public required DateTime DueDate { get; set; }
	public DateTime? PaidDate { get; set; }
	public string? TrackingNumber { get; set; }

	public ContractResponse? Contract { get; set; }
}