namespace SinaMN75U.Data.Entities;

[Table("Comments")]
public sealed class CommentEntity : BaseEntity<TagComment, CommentJson> {
	[Required]
	[Column(TypeName = "decimal(4,2)")]
	public required decimal Score { get; set; }

	[MaxLength(2000)]
	public required string Description { get; set; }

	public Guid? UserId { get; set; }
	public UserEntity? User { get; set; }

	public Guid? ProductId { get; set; }
	public ProductEntity? Product { get; set; }

	public Guid? ParentId { get; set; }
	public CommentEntity? Parent { get; set; }

	[InverseProperty("Parent")]
	public ICollection<CommentEntity> Children { get; set; } = [];

	public ICollection<MediaEntity> Media { get; set; } = [];
}

public sealed class CommentJson : BaseJsonData {
	public ICollection<CommentReacts> Reacts { get; set; } = [];
}

public sealed class CommentReacts {
	public required TagReaction Tag { get; set; }
	public required Guid UserId { get; set; }
}