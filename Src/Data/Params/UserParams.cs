namespace SinaMN75U.Data.Params;

public class UserCreateParams : BaseParams {
	[UValidationRequired("UserNameRequired")]
	[UValidationStringLength(2, 100, "UserNameMinLenght")]
	public required string UserName { get; set; }

	[UValidationRequired("PasswordRequired")]
	[UValidationStringLength(4, 100, "PasswordMinLength")]
	public required string Password { get; set; }

	public required string PhoneNumber { get; set; }
	public required string Email { get; set; }

	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Bio { get; set; }
	public string? Country { get; set; }
	public string? State { get; set; }
	public string? City { get; set; }
	public DateTime? Birthdate { get; set; }

	public double? Weight { get; set; }
	public double? Height { get; set; }
	public string? Address { get; set; }
	public string? FatherName { get; set; }
	public string? FcmToken { get; set; }
	public List<string>? Health1 { get; set; }
	public List<string>? FoodAllergies { get; set; }
	public List<string> DrugAllergies { get; set; } = [];
	public List<string>? Sickness { get; set; }

	[UValidationMinCollectionLength(1, "TagsRequired")]
	public required List<TagUser> Tags { get; set; }

	public IEnumerable<Guid>? Categories { get; set; }
}

public class UserBulkCreateParams : BaseParams {
	[UValidationRequired("UsersRequired")]
	[UValidationMinCollectionLength(1, "AtLeastOneUserRequired")]
	public required List<UserCreateParams> Users { get; set; }
}

public class UserReadParams : BaseReadParams<TagUser> {
	public string? UserName { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }
	public string? Bio { get; set; }
	public DateTime? StartBirthDate { get; set; }
	public DateTime? EndBirthDate { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
	public bool ShowCategories { get; set; } = false;
	public bool ShowMedia { get; set; } = false;
	public bool OrderByLastName { get; set; } = false;
	public bool OrderByLastNameDesc { get; set; } = false;
}

public class UserUpdateParams : BaseUpdateParams<TagUser> {
	public string? Password { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Country { get; set; }
	public string? State { get; set; }
	public string? City { get; set; }
	public string? UserName { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }
	public string? Bio { get; set; }
	public DateTime? Birthdate { get; set; }
	public string? FcmToken { get; set; }
	public string? Address { get; set; }
	public string? FatherName { get; set; }
	public double? Weight { get; set; }
	public double? Height { get; set; }
	public List<string>? AddHealth1 { get; set; }
	public List<string>? RemoveHealth1 { get; set; }
	public List<string>? FoodAllergies { get; set; }
	public List<string>? DrugAllergies { get; set; }
	public List<string>? Sickness { get; set; }
	public List<string>? Health1 { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
}