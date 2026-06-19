namespace SinaMN75U.Data.Responses;

public sealed class ContractResponse : BaseResponse<TagContract, BaseJson> {
	public required DateTime StartDate { get; set; }
	public required DateTime EndDate { get; set; }
	public required decimal Deposit { get; set; }
	public required decimal Rent { get; set; }

	public UserResponse? User { get; set; }
	public required Guid UserId { get; set; }

	public DormBedResponse? Bed { get; set; }
	public required Guid BedId { get; set; }

	public IEnumerable<InvoiceResponse>? Invoices { get; set; }
}