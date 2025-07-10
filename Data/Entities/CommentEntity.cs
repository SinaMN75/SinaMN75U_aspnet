namespace SinaMN75U.Data.Entities;

[Table("Comment")]
public class CommentEntity : BaseEntity<TagComment, CommentJson> {
	[Required]
	public required double Score { get; set; }

	[Required]
	[MaxLength(2000)]
	public required string Description { get; set; }

	public Guid? ParentId { get; set; }
	public CommentEntity? Parent { get; set; }

	public UserEntity? User { get; set; }

	[Required]
	public required Guid UserId { get; set; }

	public UserEntity? TargetUser { get; set; }
	public Guid? TargetUserId { get; set; }

	public ProductEntity? Product { get; set; }
	public Guid? ProductId { get; set; }

	[InverseProperty("Parent")]
	public ICollection<CommentEntity> Children { get; set; } = [];

	public ICollection<MediaEntity> Media { get; set; } = [];
}

public class CommentJson {
	public ICollection<CommentReacts> Reacts { get; set; } = [];
}

public class CommentReacts {
	public required TagReaction Tag { get; set; }
	public required Guid UserId { get; set; }
}
