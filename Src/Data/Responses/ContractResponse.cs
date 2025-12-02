namespace SinaMN75U.Data.Responses;

public class ContractResponse : BaseResponse<TagContract, ContractJson> {
	public required DateTime StartDate { get; set; }
	public required DateTime EndDate { get; set; }
	public required double Deposit { get; set; }
	public required double Rent { get; set; }

	public UserResponse User { get; set; } = null!;
	public required Guid UserId { get; set; }

	public UserEntity Creator { get; set; } = null!;
	public required Guid CreatorId { get; set; }

	public ProductEntity Product { get; set; } = null!;
	public required Guid ProductId { get; set; }

	public ICollection<InvoiceResponse> Invoices { get; set; } = [];
}