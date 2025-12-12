namespace SinaMN75U.Data.Responses;

public class InvoiceResponse : BaseResponse<TagInvoice, InvoiceJson> {
	public required decimal DebtAmount { get; set; }
	public required decimal CreditorAmount { get; set; }
	public required decimal PaidAmount { get; set; }
	public required decimal PenaltyAmount { get; set; }
	public required DateTime DueDate { get; set; }

	public ContractResponse? Contract { get; set; }
}