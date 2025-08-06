namespace SinaMN75U.Data.Entities;

public class FollowEntity {
	[Key]
	public Guid Id { get; set; } = Guid.CreateVersion7();
	
	public required Guid FollowedUserId { get; set; }
	public UserEntity? FollowedUser { get; set; }
	
	public required Guid FollowerUserId { get; set; }
	public UserEntity? FollowerUser { get; set; }

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}