namespace SinaMN75U.Data.Responses;

public sealed class FollowerFollowingCountResponse {
	public int Followers { get; set; }
	public int FollowedUsers { get; set; }
	public int FollowedProducts { get; set; }
	public int FollowedCategories { get; set; }
}