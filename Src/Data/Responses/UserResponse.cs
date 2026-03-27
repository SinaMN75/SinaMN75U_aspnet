namespace SinaMN75U.Data.Responses;

public sealed class UserResponse : BaseResponse<TagUser, UserJson> {
	public string? UserName { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Bio { get; set; }
	public string? Country { get; set; }
	public string? State { get; set; }
	public string? City { get; set; }
	public string? NationalCode { get; set; }
	public DateTime? Birthdate { get; set; }

	public ICollection<CategoryResponse>? Categories { get; set; }
	public ICollection<MediaResponse>? Media { get; set; }
	public ICollection<ContractResponse>? Contracts { get; set; }
	public ICollection<AddressResponse>? Addresses { get; set; }
	public ICollection<TerminalResponse>? Terminals { get; set; }
}

public sealed class LoginResponse {
	public required string Token { get; set; }
	public required string RefreshToken { get; set; }
	public required string Expires { get; set; }
	public required UserResponse User { get; set; }
}

public sealed class UserExtraResponse {
	public string? NationalCardFront { get; set; }
	public string? NationalCardBack { get; set; }
	public string? BirthCertificateFirst { get; set; }
	public string? BirthCertificateSecond { get; set; }
	public string? BirthCertificateThird { get; set; }
	public string? BirthCertificateForth { get; set; }
	public string? BirthCertificateFifth { get; set; }
	public string? VisualAuthentication { get; set; }
	public string? ESignature { get; set; }

	public ICollection<string> NotVerifiedNationalCodes { get; set; } = [];
}