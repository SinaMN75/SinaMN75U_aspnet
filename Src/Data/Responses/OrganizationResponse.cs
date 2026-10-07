namespace SinaMN75U.Data.Responses;

public sealed class OrganizationResponse : BaseResponse<TagOrganization, OrganizationJson> {
	public required string Title { get; set; }
	public Guid OwnerId { get; set; }
	public decimal Balance { get; set; }
}
