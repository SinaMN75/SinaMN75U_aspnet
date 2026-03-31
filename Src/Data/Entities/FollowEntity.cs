namespace SinaMN75U.Data.Entities;

[Table("Follows")]
public class FollowEntity : BaseEntity<TagFollow, FollowJson> {
	public required Guid CreatorId { get; set; }
	public UserEntity? Creator { get; set; }

	[ForeignKey("FK_Follows_UserId")]
	public Guid? UserId { get; set; }
	public UserEntity? User { get; set; }

	[ForeignKey("FK_Follows_ProductId")]
	public Guid? ProductId { get; set; }
	public ProductEntity? Product { get; set; }

	[ForeignKey("FK_Follows_CategoryId")]
	public Guid? CategoryId { get; set; }
	public CategoryEntity? Category { get; set; }
}

public class FollowJson {
	public string? Subtitle { get; set; }
}