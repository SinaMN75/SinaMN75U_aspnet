namespace SinaMN75U.Data.Entities;

[Table("Users")]
[Index(nameof(Email), Name = "IX_Users_Email")]
[Index(nameof(UserName), Name = "IX_Users_UserName")]
[Index(nameof(PhoneNumber), Name = "IX_Users_PhoneNumber")]
public class UserEntity : BaseEntity<TagUser, UserJson> {
	[MaxLength(100)]
	[Required]
	public string UserName { get; set; } = null!;

	[MaxLength(200)]
	[JsonIgnore]
	[Required]
	public string Password { get; set; } = null!;

	[MaxLength(200)]
	[JsonIgnore]
	[Required]
	public string RefreshToken { get; set; } = null!;

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

	public ICollection<CategoryEntity> Categories { get; set; } = [];

	public ICollection<MediaEntity> Media { get; set; } = [];
	
	public new UserResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		UserName = UserName,
		PhoneNumber = PhoneNumber,
		Email = Email,
		FirstName = FirstName,
		LastName = LastName,
		Bio = Bio,
		Country = Country,
		State = State,
		City = City,
		Birthdate = Birthdate
	};

}

public class UserJson {
	public string? FcmToken { get; set; }
	public string? Address { get; set; }
	public string? FatherName { get; set; }
	public double? Weight { get; set; }
	public double? Height { get; set; }
	public ICollection<string> Health1 { get; set; } = [];
	public ICollection<string> Health2 { get; set; } = [];
	public ICollection<string> FoodAllergies { get; set; } = [];
	public ICollection<string> DrugAllergies { get; set; } = [];
	public ICollection<string> Sickness { get; set; } = [];
	public ICollection<UserAnswerJson> UserAnswerJson { get; set; } = [];
	public ICollection<VisitCount> VisitCounts { get; set; } = [];
}