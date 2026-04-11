namespace SinaMN75U.Data.Entities;

[Table("InquiryHistory")]
public sealed class InquiryHistoryEntity : BaseEntity<TagInquiryHistory, GeneralJsonData> {
	[MaxLength(20)]
	public string? NationalCode { get; set; }

	[MaxLength(20)]
	public string? PhoneNumber { get; set; }

	[MaxLength(20)]
	public string? ZipCode { get; set; }

	[MaxLength(20)]
	public string? LicencePlate { get; set; }

	[MaxLength(20)]
	public string? DrivingLicenceNumber { get; set; }

	[MaxLength(10000)]
	public required string Response { get; set; }
}