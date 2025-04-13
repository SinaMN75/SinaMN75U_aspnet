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
	public IEnumerable<CommentEntity>? Children { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }

	public CommentResponse MapToResponse(bool media = false) => new() {
		Id = Id,
		Description = Description,
		Json = Json,
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
		Json = Json,
		UserId = UserId,
		ProductId = ProductId,
		TargetUserId = TargetUserId
	};
}

public class CommentJson {
	public List<CommentReacts> Reacts { get; set; } = [];
}

public class CommentReacts {
	public required TagReaction Tag { get; set; }
	public required Guid UserId { get; set; }
}

public static class CommentReactsExtensions {
	public static IQueryable<CommentResponse> ToResponse(this IQueryable<CommentEntity> query, bool media, bool children) => query.Select(x => new CommentResponse {
			Id = x.Id,
			Json = x.Json,
			Tags = x.Tags,
			Score = x.Score,
			Description = x.Description,
			Children = children
				? x.Children!.Select(c => new CommentResponse {
					Id = x.Id,
					Tags = x.Tags,
					Score = x.Score,
					Description = x.Description,
					CreatedAt = x.CreatedAt,
					UpdatedAt = x.UpdatedAt,
					Json = x.Json,
					Media = media
						? x.Media!.Select(m => new MediaResponse {
							Path = m.Path,
							Id = m.Id,
							Tags = m.Tags,
							Json = m.Json
						})
						: null
				})
				: null,
			Media = media
				? x.Media!.Select(m => new MediaResponse {
					Path = m.Path,
					Id = m.Id,
					Tags = m.Tags,
					Json = m.Json
				})
				: null
		}
	);
}