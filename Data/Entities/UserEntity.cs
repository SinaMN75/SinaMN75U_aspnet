namespace SinaMN75U.Data.Entities;

[Table("Users")]
[Index(nameof(Email), IsUnique = true, Name = "IX_Users_Email")]
[Index(nameof(UserName), IsUnique = true, Name = "IX_Users_UserName")]
[Index(nameof(PhoneNumber), IsUnique = true, Name = "IX_Users_PhoneNumber")]
public class UserEntity : BaseEntity<TagUser, UserJson> {
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
		Json = Json,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		City = City,
		Country = Country,
		State = State,
		FirstName = FirstName,
		LastName = LastName,
		Categories = showCategories ? Categories?.Select(u => u.MapToResponse()) : null
	};
}

public class UserJson {
	public string? FcmToken { get; set; }
	public List<string>? Health1 { get; set; }
}

public static class UserEntityExtensions {
	public static IQueryable<UserResponse> ToResponse(this IQueryable<UserEntity> query, bool media, bool categories) => query.Select(x => new UserResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			Json = x.Json,
			Tags = x.Tags,
			UserName = x.UserName,
			PhoneNumber = x.PhoneNumber,
			Email = x.Email,
			FirstName = x.FirstName,
			LastName = x.LastName,
			Country = x.Country,
			State = x.State,
			City = x.City,
			Bio = x.Bio,
			Birthdate = x.Birthdate,
			Categories = categories
				? x.Categories!.Select(c => new CategoryResponse {
					Title = c.Title,
					Id = c.Id,
					Tags = c.Tags,
					Json = c.Json,
					Media = c.Media!.Select(m => new MediaResponse {
						Path = m.Path,
						Id = m.Id,
						Tags = m.Tags,
						Json = m.Json
					})
				}).ToList()
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