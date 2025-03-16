namespace SinaMN75U.Data.Entities;

[Table("Users")]
public class UserEntity : BaseEntity {
	[MaxLength(100)]
	public required string UserName { get; set; }

	[MaxLength(200)]
	public required string Password { get; set; }

	[MaxLength(200)]
	public required string RefreshToken { get; set; }

	[MaxLength(15)]
	public required string PhoneNumber { get; set; }

	[MaxLength(100)]
	public required string Email { get; set; }

	[MaxLength(100)]
	public string? FirstName { get; set; }

	[MaxLength(100)]
	public string? LastName { get; set; }

	[MaxLength(1000)]
	public string? Bio { get; set; }

	[MaxLength(100)]
	public string? Country { get; set; }

	[MaxLength(100)]
	public string? State { get; set; }

	[MaxLength(100)]
	public string? City { get; set; }

	public DateTime? Birthdate { get; set; }

	public required UserJsonDetail JsonDetail { get; set; }
	public required List<TagUser> Tags { get; set; }
	public IEnumerable<CategoryEntity>? Categories { get; set; }
}

public class UserJsonDetail {
	public string? FcmToken { get; set; }
}

public static class UserModelExtensions {
	public static UserResponse MapToResponse(this UserEntity e) {
		return new UserResponse {
			Id = e.Id,
			UserName = e.UserName,
			PhoneNumber = e.PhoneNumber,
			Email = e.Email,
			Bio = e.Bio,
			Birthdate = e.Birthdate,
			Tags = e.Tags,
			FcmToken = e.JsonDetail.FcmToken,
			Categories = e.Categories?.Select(u => u.MapToResponse()).ToList()
		};
	}
}