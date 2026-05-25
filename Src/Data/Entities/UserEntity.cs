namespace SinaMN75U.Data.Entities;

[Table("Users")]
[Index(nameof(Email), Name = "IX_Users_Email", IsUnique = true)]
[Index(nameof(UserName), Name = "IX_Users_UserName", IsUnique = true)]
[Index(nameof(PhoneNumber), Name = "IX_Users_PhoneNumber", IsUnique = true)]
[Index(nameof(NationalCode), Name = "IX_Users_NationalCode", IsUnique = true)]
public sealed class UserEntity : BaseEntity<TagUser, UserJson> {
	[Required, MaxLength(100)]
	public required string UserName { get; set; }

	[Required, MaxLength(200)]
	public required string Password { get; set; }

	[Required, MaxLength(200)]
	public required string RefreshToken { get; set; }

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

	public byte[]? NationalCardFront { get; set; }
	public byte[]? NationalCardBack { get; set; }
	public byte[]? BirthCertificateFirst { get; set; }
	public byte[]? BirthCertificateSecond { get; set; }
	public byte[]? BirthCertificateThird { get; set; }
	public byte[]? BirthCertificateForth { get; set; }
	public byte[]? BirthCertificateFifth { get; set; }
	public byte[]? VisualAuthentication { get; set; }
	public byte[]? ESignature { get; set; }

	public ICollection<CategoryEntity> Categories { get; set; } = [];

	[InverseProperty(nameof(MediaEntity.User))]
	public ICollection<MediaEntity> Media { get; set; } = [];

	[InverseProperty(nameof(TxnEntity.User))]
	public ICollection<TxnEntity> Txns { get; set; } = [];

	[InverseProperty(nameof(AddressEntity.Creator))]
	public ICollection<AddressEntity> Addresses { get; set; } = [];

	[InverseProperty(nameof(WalletEntity.Creator))]
	public ICollection<WalletEntity> Wallets { get; set; } = [];

	[InverseProperty(nameof(MerchantEntity.User))]
	public ICollection<MerchantEntity> Merchants { get; set; } = [];

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

public sealed class UserJson : BaseJson {
	public string? FcmToken { get; set; }
	public string? FatherName { get; set; }
	public decimal? Weight { get; set; }
	public decimal? Height { get; set; }

	public string? NationalCardFrontRejectionReason { get; set; }
	public string? NationalCardBackRejectionReason { get; set; }
	public string? BirthCertificateFirstRejectionReason { get; set; }
	public string? BirthCertificateSecondRejectionReason { get; set; }
	public string? BirthCertificateThirdRejectionReason { get; set; }
	public string? BirthCertificateForthRejectionReason { get; set; }
	public string? BirthCertificateFifthRejectionReason { get; set; }
	public string? VisualAuthenticationRejectionReason { get; set; }
	public string? ESignatureRejectionReason { get; set; }
}