namespace SinaMN75U.Data.Entities;

[Table("Comment")]
public class CommentEntity : BaseEntity<TagComment, CommentJson> {
	[Required, Column(TypeName = "decimal(4,2)")]
	public required decimal Score { get; set; }

	[MaxLength(2000)]
	public required string Description { get; set; }

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	public Guid? TargetUserId { get; set; }
	public UserEntity? TargetUser { get; set; }

	public Guid? ProductId { get; set; }
	public ProductEntity? Product { get; set; }
	
	public Guid? ParentId { get; set; }
	public CommentEntity? Parent { get; set; }

	[InverseProperty("Parent")]
	public ICollection<CommentEntity> Children { get; set; } = [];

	public ICollection<MediaEntity> Media { get; set; } = [];
	
	public new CommentResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		Score = Score,
		Description = Description,
		ParentId = ParentId,
		UserId = UserId,
		TargetUserId = TargetUserId,
		ProductId = ProductId
	};
}

public class CommentJson {
	public ICollection<CommentReacts> Reacts { get; set; } = [];
}

public class CommentReacts {
	public required TagReaction Tag { get; set; }
	public required Guid UserId { get; set; }
}
