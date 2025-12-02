namespace SinaMN75U.Data.Responses;

public class ContractResponse : BaseResponse<TagContract, ContractJson> {
	public required DateTime StartDate { get; set; }
	public required DateTime EndDate { get; set; }
	public required double Deposit { get; set; }
	public required double Rent { get; set; }

	public UserResponse? User { get; set; }
	public required Guid UserId { get; set; }

	public UserResponse? Creator { get; set; }
	public required Guid CreatorId { get; set; }

	public ProductResponse? Product { get; set; }
	public required Guid ProductId { get; set; }

	public ICollection<InvoiceResponse>? Invoices { get; set; } = [];
}