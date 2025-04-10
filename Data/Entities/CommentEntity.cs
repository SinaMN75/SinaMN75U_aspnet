namespace SinaMN75U.Data.Entities;

[Table("Comment")]
public class CommentEntity : BaseEntity<TagComment> {
	[Required]
	public required double Score { get; set; }

	[Required]
	public required CommentJsonDetail JsonDetail { get; set; }

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
	public IEnumerable<CommentEntity>? Children { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }

	public CommentResponse MapToResponse(bool media = false) => new() {
		Id = Id,
		Description = Description,
		Tags = Tags,
		Children = Children?.Select(x => x.MapToResponse())
			.ToList(),
		Media = media
			? Media?.Select(x => x.MapToResponse())
			: null,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		Score = Score
	};

	public CommentEntity MapToEntity(bool media = false) => new() {
		Id = Id,
		Description = Description,
		Tags = Tags,
		Children = Children?.Select(x => x.MapToEntity()),
		Media = media ? Media?.Select(x => x.MapToEntity()) : null,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		Score = Score,
		ParentId = ProductId,
		JsonDetail = JsonDetail,
		UserId = UserId,
		ProductId = ProductId,
		TargetUserId = TargetUserId
	};
}

public class CommentJsonDetail {
	public List<CommentReacts> Reacts { get; set; } = [];
}

public class CommentReacts {
	public required TagReaction Tag { get; set; }
	public required Guid UserId { get; set; }
}