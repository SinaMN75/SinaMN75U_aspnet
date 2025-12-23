namespace SinaMN75U.Data.Responses;

public class InvoiceResponse : BaseResponse<TagInvoice, InvoiceJson> {
	public required decimal DebtAmount { get; set; }
	public required decimal CreditorAmount { get; set; }
	public required decimal PaidAmount { get; set; }
	public required decimal PenaltyAmount { get; set; }
	public required DateTime DueDate { get; set; }

	public ContractResponse? Contract { get; set; }
}

public class InvoiceChartResponse {
	public string Month { get; set; } = "";
	public decimal TotalDebt { get; set; }
	public decimal TotalPaid { get; set; }
	public decimal TotalPenalty { get; set; }
	public decimal TotalRemaining { get; set; }
	public int InvoiceCount { get; set; }
}