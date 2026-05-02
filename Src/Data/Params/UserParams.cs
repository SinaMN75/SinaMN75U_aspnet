namespace SinaMN75U.Data.Params;

public sealed class UserCreateParams : BaseCreateParams<TagUser> {
	[UValidationRequired("UserNameRequired"), UValidationStringLength(2, 100, "UserNameMinLenght")]
	public string UserName { get; set; } = null!;

	[UValidationRequired("PasswordRequired"), UValidationStringLength(4, 100, "PasswordMinLength")]
	public string Password { get; set; } = null!;

	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }
	public string? LandLine { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? NationalCode { get; set; }
	public string? Bio { get; set; }
	public string? FatherName { get; set; }
	public string? FcmToken { get; set; }
	public decimal? Weight { get; set; }
	public decimal? Height { get; set; }
	public DateTime? Birthdate { get; set; }

	public IEnumerable<Guid>? Categories { get; set; }
}

public sealed class UserUpdateParams : BaseUpdateParams<TagUser> {
	public string? LandLine { get; set; }
	public string? Password { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? UserName { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }
	public string? Bio { get; set; }
	public string? FcmToken { get; set; }
	public string? FatherName { get; set; }
	public string? NationalCode { get; set; }
	public decimal? Weight { get; set; }
	public decimal? Height { get; set; }
	public DateTime? Birthdate { get; set; }
	public ICollection<Guid>? Categories { get; set; }
}

public sealed class UserReadParams : BaseReadParams<TagUser> {
	public string? LandLine { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? UserName { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }
	public string? Bio { get; set; }
	public string? NationalCode { get; set; }
	public DateTime? StartBirthDate { get; set; }
	public DateTime? EndBirthDate { get; set; }
	public IEnumerable<Guid>? Categories { get; set; }
	public bool OrderByFirstName { get; set; }
	public bool OrderByFirstNameDesc { get; set; }
	public bool OrderByLastName { get; set; }
	public bool OrderByLastNameDesc { get; set; }
	public UserSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class UserBulkCreateParams : BaseParams {
	[UValidationRequired("UsersRequired"), UValidationMinCollectionLength(1, "AtLeastOneUserRequired")]
	public List<UserCreateParams> Users { get; set; } = null!;
}

public sealed class UserExtraUpdateParams : BaseUpdateParams<TagUserExtra> {
	public string? NationalCardFront { get; set; }
	public string? NationalCardBack { get; set; }
	public string? BirthCertificateFirst { get; set; }
	public string? BirthCertificateSecond { get; set; }
	public string? BirthCertificateThird { get; set; }
	public string? BirthCertificateForth { get; set; }
	public string? BirthCertificateFifth { get; set; }
	public string? VisualAuthentication { get; set; }
	public string? ESignature { get; set; }
}