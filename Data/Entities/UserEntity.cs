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

	[Required]
	public required UserJsonDetail JsonDetail { get; set; }
	
	[Required]
	public required List<TagUser> Tags { get; set; }
	
	public IEnumerable<CategoryEntity>? Categories { get; set; }
	
	public UserResponse MapToResponse(bool showCategories = false) {
		return new UserResponse {
			Id = Id,
			UserName = UserName,
			PhoneNumber = PhoneNumber,
			Email = Email,
			Bio = Bio,
			Birthdate = Birthdate,
			Tags = Tags,
			FcmToken = JsonDetail.FcmToken,
			Categories = showCategories ? Categories?.Select(u => u.MapToResponse()) : null
		};
	}
}

public class UserJsonDetail {
	public string? FcmToken { get; set; }
}
