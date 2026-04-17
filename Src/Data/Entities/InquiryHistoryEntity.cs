namespace SinaMN75U.Data.Entities;

[Table("InquiryHistory")]
public sealed class InquiryHistoryEntity : BaseEntity<TagInquiryHistory, BaseJsonData> {
	[MaxLength(40)]
	public string? NationalCode { get; set; }

	[MaxLength(40)]
	public string? PhoneNumber { get; set; }

	[MaxLength(40)]
	public string? ZipCode { get; set; }

	[MaxLength(40)]
	public string? LicencePlate { get; set; }

	[MaxLength(40)]
	public string? DrivingLicenceNumber { get; set; }
	
	[MaxLength(40)]
	public string? IBan { get; set; }

	[MaxLength(10000)]
	public required string Response { get; set; }
}