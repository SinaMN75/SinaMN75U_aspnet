namespace SinaMN75U.Data.Entities;

[Table("Users")]
[Index(nameof(Email), Name = "IX_Users_Email")]
[Index(nameof(UserName), Name = "IX_Users_UserName")]
[Index(nameof(PhoneNumber), Name = "IX_Users_PhoneNumber")]
public class UserEntity : BaseEntity<TagUser, UserJson> {
	[MaxLength(100)]
	public required string UserName { get; set; }

	[MaxLength(200)]
	public required string Password { get; set; }

	[MaxLength(200)]
	public required string RefreshToken { get; set; }

	[MaxLength(15)]
	public string? PhoneNumber { get; set; }

	[MaxLength(100)]
	public string? Email { get; set; }

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

	public Guid? ParentId { get; set; }
	public UserEntity? Parent { get; set; }

	[InverseProperty("Parent")]
	public IEnumerable<UserEntity>? Children { get; set; }

	public List<CategoryEntity>? Categories { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }
}

public class UserJson {
	public string? FcmToken { get; set; }
	public List<string> Health1 { get; set; } = [];
	public List<string> FoodAllergies { get; set; } = [];
	public List<string> DrugAllergies { get; set; } = [];
	public List<string> Sickness { get; set; } = [];
	public double? Weight { get; set; }
	public double? Height { get; set; }
	public string? Address { get; set; }
	public string? FatherName { get; set; }
	public List<UserAnswerJson> UserAnswerJson { get; set; } = [];
}

public static class UserEntityExtensions {
	public static IQueryable<UserEntity> ToResponse(this IQueryable<UserEntity> query, bool media, bool categories) => query.Select(x => new UserEntity {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			JsonData = x.JsonData,
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
				? x.Categories!.Select(c => new CategoryEntity {
						Title = c.Title,
						Id = c.Id,
						Tags = c.Tags,
						JsonData = c.JsonData,
						CreatedAt = c.CreatedAt,
						UpdatedAt = c.UpdatedAt,
						Media = c.Media!.Select(m => new MediaEntity {
							Path = m.Path,
							Id = m.Id,
							Tags = m.Tags,
							JsonData = m.JsonData,
							CreatedAt = c.CreatedAt,
							UpdatedAt = c.UpdatedAt
						})
					})
					.ToList()
				: null,
			Media = media
				? x.Media!.Select(m => new MediaEntity {
					Path = m.Path,
					Id = m.Id,
					Tags = m.Tags,
					JsonData = m.JsonData,
					CreatedAt = m.CreatedAt,
					UpdatedAt = m.UpdatedAt
				})
				: null,
			Password = "",
			RefreshToken = ""
		}
	);
}