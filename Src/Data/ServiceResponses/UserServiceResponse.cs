namespace SinaMN75U.Data.ServiceResponses;

public class UserServiceResponse: BaseServiceResponse<TagUser, UserJson> {
	public string? UserName { get; set; }
	public string? Password { get; set; }
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
}