namespace SinaMN75U.Data.Entities;

[Table("Media")]
public class MediaEntity : BaseEntity<TagMedia> {
	[Required]
	[MaxLength(200)]
	public required string Path { get; set; }

	[Required]
	public required MediaJsonDetail JsonDetail { get; set; }

	public Guid? UserId { get; set; }
	public UserEntity? User { get; set; }
	
	public MediaResponse MapToResponse() {
		return new MediaResponse {
			Path = $"{Server.ServerAddress}/Media/{Path}",
			Tags = Tags,
			Title = JsonDetail.Title,
			Description = JsonDetail.Description
		};
	}
}

public class MediaJsonDetail {
	public string? Title { get; set; }
	public string? Description { get; set; }
}