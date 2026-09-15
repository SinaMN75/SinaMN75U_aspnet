namespace SinaMN75U.Data.Entities;

[Table("Follows")]
public sealed class FollowEntity : BaseEntity<TagFollow, FollowJson> {
	public Guid? UserId { get; set; }
	public UserEntity? User { get; set; }

	public Guid? ProductId { get; set; }
	public ProductEntity? Product { get; set; }

	public Guid? CategoryId { get; set; }
	public CategoryEntity? Category { get; set; }
}

public class FollowJson: BaseJson;