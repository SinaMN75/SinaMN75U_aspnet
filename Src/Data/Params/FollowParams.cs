namespace SinaMN75U.Data.Params;

public class FollowParams: BaseParams {
	public Guid? UserId { get; set; }
	public required Guid TargetUserId { get; set; }
}