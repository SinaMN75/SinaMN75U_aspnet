namespace SinaMN75U.Data.Responses;

public sealed class BedResponse : BaseResponse<TagBed, BaseJson> {
	public decimal Deposit { get; set; }
	public decimal Rent { get; set; }
	
	public Guid? ParentId { get; set; }
	public BedResponse? Parent { get; set; }

	public ICollection<BedResponse> Children { get; set; } = [];
	public ICollection<ContractResponse> Contracts { get; set; } = [];
	public ICollection<MediaResponse> Media { get; set; } = [];
}