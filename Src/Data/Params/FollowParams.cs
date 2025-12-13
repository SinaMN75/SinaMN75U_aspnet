namespace SinaMN75U.Data.Params;

public sealed class FollowParams: BaseParams {
	public Guid? CreatorId { get; set; }
	public Guid? UserId { get; set; }
	public Guid? ProductId { get; set; }
	public Guid? CategoryId { get; set; }
}