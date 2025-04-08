namespace SinaMN75U.Data.Entities;

[Table("Users")]
[Index(nameof(Email), IsUnique = true, Name = "IX_Users_Email")]
[Index(nameof(UserName), IsUnique = true, Name = "IX_Users_UserName")]
[Index(nameof(PhoneNumber), IsUnique = true, Name = "IX_Users_PhoneNumber")]
public class UserEntity : BaseEntity<TagUser> {
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

	public IEnumerable<CategoryEntity>? Categories { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }

	public UserResponse MapToResponse(bool showCategories = false) => new() {
		Id = Id,
		UserName = UserName,
		PhoneNumber = PhoneNumber,
		Email = Email,
		Bio = Bio,
		Birthdate = Birthdate,
		Tags = Tags,
		FcmToken = JsonDetail.FcmToken,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		City = City,
		Country = Country,
		State = State,
		FirstName = FirstName,
		LastName = LastName,
		Categories = showCategories ? Categories?.Select(u => u.MapToResponse()) : null
	};

	public UserEntity MapToEntity(bool showCategories = false) => new() {
		Id = Id,
		UserName = UserName,
		PhoneNumber = PhoneNumber,
		Email = Email,
		Bio = Bio,
		Birthdate = Birthdate,
		Tags = Tags,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		City = City,
		Country = Country,
		State = State,
		FirstName = FirstName,
		LastName = LastName,
		JsonDetail = JsonDetail,
		Categories = showCategories
			? Categories?.Select(u => u.MapToEntity())
			: null,
		Password = "",
		RefreshToken = ""
	};
}

public class UserJsonDetail {
	public string? FcmToken { get; set; }
}