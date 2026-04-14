namespace SinaMN75U.Data.Entities;

[Table("Users")]
[Index(nameof(Email), Name = "IX_Users_Email", IsUnique = true)]
[Index(nameof(UserName), Name = "IX_Users_UserName", IsUnique = true)]
[Index(nameof(PhoneNumber), Name = "IX_Users_PhoneNumber", IsUnique = true)]
[Index(nameof(NationalCode), Name = "IX_Users_NationalCode", IsUnique = true)]
public sealed class UserEntity : BaseEntity<TagUser, UserJson> {
	[Required]
	[MaxLength(100)]
	public string UserName { get; set; } = null!;

	[Required]
	[MaxLength(200)]
	public string Password { get; set; } = null!;

	[Required]
	[MaxLength(200)]
	public string RefreshToken { get; set; } = null!;

	[MaxLength(15)]
	public string? PhoneNumber { get; set; }

	[MinLength(10)]
	[MaxLength(10)]
	public string? NationalCode { get; set; }

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

	[Required]
	public UserExtraEntity Extra { get; set; } = null!;

	public ICollection<CategoryEntity> Categories { get; set; } = [];

	public ICollection<MediaEntity> Media { get; set; } = [];

	[InverseProperty(nameof(ContractEntity.User))]
	public ICollection<ContractEntity> Contracts { get; set; } = [];

	[InverseProperty(nameof(ContractEntity.Creator))]
	public ICollection<ContractEntity> CreatedContracts { get; set; } = [];

	public ICollection<InvoiceEntity> Invoices { get; set; } = [];

	public ICollection<TxnEntity> Txns { get; set; } = [];

	public ICollection<AddressEntity> Addresses { get; set; } = [];

	public ICollection<WalletEntity> Wallets { get; set; } = [];
	
	public ICollection<TerminalEntity> Terminals { get; set; } = [];
	
	public ICollection<BankAccountEntity> BankAccounts { get; set; } = [];
	
	public ICollection<SimCardEntity> SimCards { get; set; } = [];

	public UserResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		UserName = UserName,
		PhoneNumber = PhoneNumber,
		NationalCode = NationalCode,
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

public sealed class UserJson {
	public string? FcmToken { get; set; }
	public string? Address { get; set; }
	public string? FatherName { get; set; }
	public decimal? Weight { get; set; }
	public decimal? Height { get; set; }
	public ICollection<VisitCount> VisitCounts { get; set; } = [];
}

[Table("UserExtras")]
public sealed class UserExtraEntity : BaseEntity<TagUserExtra, GeneralJsonData> {
	public string? NationalCardFront { get; set; }
	public string? NationalCardBack { get; set; }
	public string? BirthCertificateFirst { get; set; }
	public string? BirthCertificateSecond { get; set; }
	public string? BirthCertificateThird { get; set; }
	public string? BirthCertificateForth { get; set; }
	public string? BirthCertificateFifth { get; set; }
	public string? VisualAuthentication { get; set; }
	public string? ESignature { get; set; }
	
	public UserEntity User { get; set; } = null!;
	public required Guid UserId { get; set; }
}
