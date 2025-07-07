namespace SinaMN75U.Data.Params;

public class UserCreateParams : BaseParams {
	[URequired("UserNameRequired")]
	[UStringLength(2, 100, "UserNameMinLenght")]
	public required string UserName { get; set; }

	[URequired("PasswordRequired")]
	[UStringLength(4, 100, "PasswordMinLength")]
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

	[UMinCollectionLength(1, "TagsRequired")]
	public required List<TagUser> Tags { get; set; }

	public IEnumerable<Guid>? Categories { get; set; }
}

public class UserBulkCreateParams : BaseParams {
	[URequired("UsersRequired")]
	[UMinCollectionLength(1, "AtLeastOneUserRequired")]
	public required List<UserCreateParams> Users { get; set; }
}

public class UserReadParams : BaseReadParams<TagUser> {
	[UFilterContains(nameof(UserEntity.UserName))]
	public string? UserName { get; set; }

	[UFilterContains(nameof(UserEntity.PhoneNumber))]
	public string? PhoneNumber { get; set; }

	[UFilterContains(nameof(UserEntity.Email))]
	public string? Email { get; set; }

	[UFilterContains(nameof(UserEntity.Bio))]
	public string? Bio { get; set; }

	[UFilterDateAfter(nameof(UserEntity.Birthdate))]
	public DateTime? StartBirthDate { get; set; }

	[UFilterDateAfter(nameof(UserEntity.Birthdate))]
	public DateTime? EndBirthDate { get; set; }

	public IEnumerable<Guid>? Categories { get; set; }

	public bool ShowCategories { get; set; } = false;
	public bool ShowMedia { get; set; } = false;

	[USort(nameof(UserEntity.LastName))]
	public bool OrderByLastName { get; set; } = false;

	[USort(nameof(UserEntity.LastName), true)]
	public bool OrderByLastNameDesc { get; set; } = false;
}

public class UserUpdateParams : BaseUpdateParams<TagUser> {
	[UUpdateAssignHashed(nameof(UserEntity.Password))]
	public string? Password { get; set; }

	[UUpdateAssignIfNotNullOrEmpty(nameof(UserEntity.FirstName))]
	public string? FirstName { get; set; }

	[UUpdateAssignIfNotNullOrEmpty(nameof(UserEntity.LastName))]
	public string? LastName { get; set; }

	[UUpdateAssignIfNotNullOrEmpty(nameof(UserEntity.Country))]
	public string? Country { get; set; }

	[UUpdateAssignIfNotNullOrEmpty(nameof(UserEntity.State))]
	public string? State { get; set; }

	[UUpdateAssignIfNotNullOrEmpty(nameof(UserEntity.City))]
	public string? City { get; set; }

	[UUpdateAssignIfNotNullOrEmpty(nameof(UserEntity.UserName))]
	public string? UserName { get; set; }

	[UUpdateAssignIfNotNullOrEmpty(nameof(UserEntity.PhoneNumber))]
	public string? PhoneNumber { get; set; }

	[UUpdateAssignIfNotNullOrEmpty(nameof(UserEntity.Email))]
	public string? Email { get; set; }

	[UUpdateAssignIfNotNullOrEmpty(nameof(UserEntity.Bio))]
	public string? Bio { get; set; }

	[UUpdateAssignIfNotNullOrEmpty(nameof(UserEntity.Birthdate))]
	public DateTime? Birthdate { get; set; }

	[UUpdateAssignNestedIfNotNullOrEmpty(nameof(UserEntity.JsonData), nameof(UserJson.FcmToken))]
	public string? FcmToken { get; set; }

	[UUpdateAssignNestedIfNotNullOrEmpty(nameof(UserEntity.JsonData), nameof(UserJson.Address))]
	public string? Address { get; set; }

	[UUpdateAssignNestedIfNotNullOrEmpty(nameof(UserEntity.JsonData), nameof(UserJson.FatherName))]
	public string? FatherName { get; set; }

	[UUpdateAssignNestedIfNotNullOrEmpty(nameof(UserEntity.JsonData), nameof(UserJson.Weight))]
	public double? Weight { get; set; }

	[UUpdateAssignNestedIfNotNullOrEmpty(nameof(UserEntity.JsonData), nameof(UserJson.Height))]
	public double? Height { get; set; }

	[UUpdateAddRangeNestedIfNotExistIfNotNull(nameof(UserEntity.JsonData), nameof(UserJson.Health1))]
	public List<string>? AddHealth1 { get; set; }

	[UUpdateAddRangeNestedIfNotExistIfNotNull(nameof(UserEntity.JsonData), nameof(UserJson.Health1))]
	public List<string>? RemoveHealth1 { get; set; }

	[UUpdateReplaceNestedListIfNotNull(nameof(UserEntity.JsonData), nameof(UserJson.FoodAllergies))]
	public List<string>? FoodAllergies { get; set; }

	[UUpdateReplaceNestedListIfNotNull(nameof(UserEntity.JsonData), nameof(UserJson.DrugAllergies))]
	public List<string>? DrugAllergies { get; set; }

	[UUpdateReplaceNestedListIfNotNull(nameof(UserEntity.JsonData), nameof(UserJson.Sickness))]
	public List<string>? Sickness { get; set; }

	[UUpdateReplaceNestedListIfNotNull(nameof(UserEntity.JsonData), nameof(UserJson.Health1))]
	public List<string>? Health1 { get; set; }

	public IEnumerable<Guid>? Categories { get; set; }
}