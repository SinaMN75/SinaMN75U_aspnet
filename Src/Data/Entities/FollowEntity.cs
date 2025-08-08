namespace SinaMN75U.Data.Entities;

public class FollowEntity: BaseEntity<TagFollow, FollowJson> {
	
	public required Guid UserId { get; set; }
	public UserEntity? User { get; set; }
	
	public Guid? TargetUserId { get; set; }
	public UserEntity? TargetUser { get; set; }	
	
	public Guid? TargetProductId { get; set; }
	public ProductEntity? TargetProduct { get; set; }
	
	public Guid? TargetCategoryId { get; set; }
	public CategoryEntity? TargetCategory { get; set; }
}

public class FollowJson {
	public string? Subtitle { get; set; }
}