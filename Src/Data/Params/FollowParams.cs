namespace SinaMN75U.Data.Params;

public sealed class FollowParams: BaseParams {
	public Guid? UserId { get; set; }
	public Guid? TargetUserId { get; set; }
	public Guid? TargetProductId { get; set; }
	public Guid? TargetCategoryId { get; set; }
}