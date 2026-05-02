namespace SinaMN75U.Data.Entities;

[Table("Users")]
[Index(nameof(Email), Name = "IX_Users_Email", IsUnique = true)]
[Index(nameof(UserName), Name = "IX_Users_UserName", IsUnique = true)]
[Index(nameof(PhoneNumber), Name = "IX_Users_PhoneNumber", IsUnique = true)]
[Index(nameof(NationalCode), Name = "IX_Users_NationalCode", IsUnique = true)]
public sealed class UserEntity : BaseEntity<TagUser, UserJson> {
	[Required, MaxLength(100)]
	public string UserName { get; set; } = null!;

	[Required, MaxLength(200)]
	public string Password { get; set; } = null!;

	[Required, MaxLength(200)]
	public string RefreshToken { get; set; } = null!;

	[MaxLength(15)]
	public string? PhoneNumber { get; set; }
	
	[MaxLength(15)]
	public string? LandLine { get; set; }
	
	[MinLength(10), MaxLength(10)]
	public string? NationalCode { get; set; }

	[MaxLength(100)]
	public string? Email { get; set; }

	[MaxLength(100)]
	public string? FirstName { get; set; }

	[MaxLength(100)]
	public string? LastName { get; set; }

	[MaxLength(1000)]
	public string? Bio { get; set; }

	public DateTime? Birthdate { get; set; }

	[Required]
	public UserExtraEntity Extra { get; set; } = null!;

	public ICollection<CategoryEntity> Categories { get; set; } = [];

	[InverseProperty(nameof(MediaEntity.User))]
	public ICollection<MediaEntity> Media { get; set; } = [];

	[InverseProperty(nameof(ContractEntity.User))]
	public ICollection<ContractEntity> Contracts { get; set; } = [];

	public ICollection<InvoiceEntity> Invoices { get; set; } = [];

	[InverseProperty(nameof(TxnEntity.User))]
	public ICollection<TxnEntity> Txns { get; set; } = [];

	[InverseProperty(nameof(AddressEntity.Creator))]
	public ICollection<AddressEntity> Addresses { get; set; } = [];

	[InverseProperty(nameof(WalletEntity.Creator))]
	public ICollection<WalletEntity> Wallets { get; set; } = [];
	
	[InverseProperty(nameof(TerminalEntity.User))]
	public ICollection<TerminalEntity> Terminals { get; set; } = [];
	
	[InverseProperty(nameof(BankAccountEntity.Creator))]
	public ICollection<BankAccountEntity> BankAccounts { get; set; } = [];
	
	[InverseProperty(nameof(SimCardEntity.User))]
	public ICollection<SimCardEntity> SimCards { get; set; } = [];

	public UserResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		JsonData = JsonData,
		Tags = Tags,
		UserName = UserName,
		PhoneNumber = PhoneNumber,
		NationalCode = NationalCode,
		Email = Email,
		FirstName = FirstName,
		LastName = LastName,
		Bio = Bio,
		Birthdate = Birthdate
	};
}

public sealed class UserJson : BaseJsonData {
	public string? FcmToken { get; set; }
	public string? FatherName { get; set; }
	public decimal? Weight { get; set; }
	public decimal? Height { get; set; }
}

[Table("UserExtras")]
public sealed class UserExtraEntity : BaseEntity<TagUserExtra, BaseJsonData> {
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
