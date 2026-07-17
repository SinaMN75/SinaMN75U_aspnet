namespace SinaMN75U.Data.Responses;

public class MoadiResponse : BaseResponse<TagMoadi, MoadiJson> {
	public required string Name { get; set; }
	public required string EconomicCode { get; set; }
	public required string LegalEntity { get; set; }
	public required string UniqueTaxCode { get; set; }
	public string? NationalCode { get; set; }
	public string? PostalCode { get; set; }
	public string? RegistrationDate { get; set; }
	public string? RegistrationNumber { get; set; }
	public string? Address { get; set; }
	public int? StartInvoiceNumber { get; set; }
	public string? IntroductionCode { get; set; }
	public required string OwnerName { get; set; }
	public required string OwnerMobile { get; set; }
	public required string OwnerNationalCode { get; set; }

	public required Guid UserId { get; set; }
	public UserResponse? User { get; set; }
}
