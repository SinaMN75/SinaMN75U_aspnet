namespace SinaMN75U.Data.Params;

public sealed class UserCreateParams : BaseParams {
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
	public List<string>? Health2 { get; set; }
	public List<string>? FoodAllergies { get; set; }
	public List<string> DrugAllergies { get; set; } = [];
	public List<string>? Sickness { get; set; }

	[UValidationMinCollectionLength(1, "TagsRequired")]
	public required List<TagUser> Tags { get; set; }

	public IEnumerable<Guid>? Categories { get; set; }
	
	public UserEntity MapToEntity(string hashedPassword) => new() {
		UserName = UserName,
		Password = hashedPassword,
		RefreshToken = "",
		PhoneNumber = PhoneNumber,
		Email = Email,
		FirstName = FirstName,
		LastName = LastName,
		Bio = Bio,
		Country = Country,
		State = State,
		City = City,
		Birthdate = Birthdate,
		JsonData = new UserJson {
			Weight = Weight,
			Height = Height,
			Address = Address,
			FatherName = FatherName,
			FcmToken = FcmToken,
			Health1 = Health1 ?? [],
			Health2 = Health2 ?? [],
			FoodAllergies = FoodAllergies ?? [],
			DrugAllergies = DrugAllergies,
			Sickness = Sickness ?? []
		},
		Tags = Tags
	};

}

public sealed class UserUpdateParams : BaseUpdateParams<TagUser> {
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
	public List<string>? AddHealth2 { get; set; }
	public List<string>? RemoveHealth2 { get; set; }
	public List<string>? FoodAllergies { get; set; }
	public List<string>? DrugAllergies { get; set; }
	public List<string>? Sickness { get; set; }
	public List<string>? Health1 { get; set; }
	public List<string>? Health2 { get; set; }
	public ICollection<Guid>? Categories { get; set; }
	
	public void MapToEntity(UserEntity e, string? hashedPassword = null) {
		if (hashedPassword != null) e.Password = hashedPassword;
		if (UserName != null) e.UserName = UserName;
		if (PhoneNumber != null) e.PhoneNumber = PhoneNumber;
		if (Email != null) e.Email = Email;
		if (FirstName != null) e.FirstName = FirstName;
		if (LastName != null) e.LastName = LastName;
		if (Bio != null) e.Bio = Bio;
		if (Country != null) e.Country = Country;
		if (State != null) e.State = State;
		if (City != null) e.City = City;
		if (Birthdate.HasValue) e.Birthdate = Birthdate;

		if (FcmToken != null) e.JsonData.FcmToken = FcmToken;
		if (Address != null) e.JsonData.Address = Address;
		if (FatherName != null) e.JsonData.FatherName = FatherName;
		if (Weight.HasValue) e.JsonData.Weight = Weight;
		if (Height.HasValue) e.JsonData.Height = Height;

		if (Health1 != null) e.JsonData.Health1 = Health1;
		if (AddHealth1 != null) foreach (string item in AddHealth1) e.JsonData.Health1?.Add(item);
		if (RemoveHealth1 != null) foreach (string item in RemoveHealth1) e.JsonData.Health1?.Remove(item);

		if (Health2 != null) e.JsonData.Health2 = Health2;
		if (AddHealth2 != null) foreach (string item in AddHealth2) e.JsonData.Health2?.Add(item);
		if (RemoveHealth2 != null) foreach (string item in RemoveHealth2) e.JsonData.Health2?.Remove(item);

		if (FoodAllergies != null) e.JsonData.FoodAllergies = FoodAllergies;
		if (DrugAllergies != null) e.JsonData.DrugAllergies = DrugAllergies;
		if (Sickness != null) e.JsonData.Sickness = Sickness;

		if (Tags != null) e.Tags = Tags;
	}
}

public sealed class UserReadParams : BaseReadParams<TagUser> {
	public string? UserName { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }
	public string? Bio { get; set; }
	public DateTime? StartBirthDate { get; set; }
	public DateTime? EndBirthDate { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
	public bool ShowCategories { get; set; }
	public bool ShowMedia { get; set; }
	public bool OrderByLastName { get; set; }
	public bool OrderByLastNameDesc { get; set; }
}

public sealed class UserBulkCreateParams : BaseParams {
	[UValidationRequired("UsersRequired")]
	[UValidationMinCollectionLength(1, "AtLeastOneUserRequired")]
	public required List<UserCreateParams> Users { get; set; }
}