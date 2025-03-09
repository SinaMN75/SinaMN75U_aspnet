using SinaMN75U.Data.Responses.Media;
using System.ComponentModel.DataAnnotations.Schema;

namespace SinaMN75U.Data.Entities;

[Table("Media")]
public class MediaEntity : BaseEntity {
	[Required]
	[MaxLength(200)]
	public required string Path { get; set; }

	[Required]
	public required MediaJsonDetail JsonDetail { get; set; }

	[Required]
	public required List<TagMedia> Tags { get; set; }

	public Guid? UserId { get; set; }
	public UserEntity? User { get; set; }
}

public class MediaJsonDetail {
	public string? Title { get; set; }
	public string? Description { get; set; }
}

public static class MediaModelExtensions {
	public static MediaResponse MapToResponse(this MediaEntity e) {
		return new MediaResponse {
			Path = $"{Server.ServerAddress}/Medias/{e.Path}",
			Tags = e.Tags,
			UserId = e.UserId,
			Title = e.JsonDetail.Title,
			Description = e.JsonDetail.Description
		};
	}
}