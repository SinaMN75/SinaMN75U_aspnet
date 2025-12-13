namespace SinaMN75U.Data.Entities;

public class FollowEntity: BaseEntity<TagFollow, FollowJson> {
	
	public required Guid CreatorId { get; set; }
	public UserEntity? Creator { get; set; }
	
	public Guid? UserId { get; set; }
	public UserEntity? User { get; set; }	
	
	public Guid? ProductId { get; set; }
	public ProductEntity? Product { get; set; }
	
	public Guid? CategoryId { get; set; }
	public CategoryEntity? Category { get; set; }
}

public class FollowJson {
	public string? Subtitle { get; set; }
}