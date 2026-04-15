namespace SinaMN75U.Data.Responses;

public class GetAccessTokenResponse {
	public string? AccessToken { get; set; }
	public int? ExpiresIn { get; set; }
}

public sealed class PostalCodeToAddressDetailResponse {
	public string? BuildingName { get; set; }
	public string? Description { get; set; }
	public string? Floor { get; set; }
	public string? HouseNumber { get; set; }
	public string? LocalityName { get; set; }
	public string? LocalityType { get; set; }
	public string? ZipCode { get; set; }
	public string? Province { get; set; }
	public string? SideFloor { get; set; }
	public string? Street { get; set; }
	public string? Street2 { get; set; }
	public string? SubLocality { get; set; }
	public string? TownShip { get; set; }
	public string? TraceId { get; set; }
	public string? Village { get; set; }
}

public sealed class DrivingLicenceStatusResponse {
	public string? NationalCode { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? RequestDate { get; set; }
	public string? Title { get; set; }
	public string? ConfirmDate { get; set; }
	public string? RahvarStatus { get; set; }
	public string? PacketNo { get; set; }
	public string? Barcode { get; set; }
	public string? PrintNnumber { get; set; }
	public string? PrintDate { get; set; }
	public string? ValidYears { get; set; }
}

public sealed class VehicleViolationDetailResponse {
	public string? PlateDictation { get; set; }
	public string? PlateChar { get; set; }
	public string? ComplaintStatus { get; set; }
	public string? Complaint { get; set; }
	public string? DateTime { get; set; }
	public string? PriceStatus { get; set; }
	public string? TraceId { get; set; }
	public string? PaperId { get; set; }
	public string? PaymentId { get; set; }
	public string? WarningPrice { get; set; }
	public string? InquirePrice { get; set; }
	public string? EjrInquireNo { get; set; }
	public string? WarningId { get; set; }
	public string? InquirePriceDictation { get; set; }
	public IEnumerable<VehicleViolationDetailItem> Items { get; set; } = [];
}

public sealed class VehicleViolationDetailItem {
	public string? SerialNo { get; set; }
	public string? Date { get; set; }
	public string? Type { get; set; }
	public string? Address { get; set; }
	public string? ViolationType { get; set; }
	public string? FinalPrice { get; set; }
	public string? PaperId { get; set; }
	public string? PaymentId { get; set; }
	public string? WarningId { get; set; }
	public string? InvestigationAbility { get; set; }
	public bool? HasImage { get; set; }
}

public sealed class LicencePlateInquiryResponse {
	public string? Status { get; set; }
	public string? TracePlate { get; set; }
	public IEnumerable<LicencePlateHistoryItem> Items { get; set; } = [];
}

public sealed class LicencePlateHistoryItem {
	public string? System { get; set; }
	public string? Type { get; set; }
	public string? InstallDate { get; set; }
	public string? Model { get; set; }
}

public sealed class DrivingLicenceNegativePointResponse {
	public string? Point { get; set; }
	public bool? Allowable { get; set; }
	public string? RuleId { get; set; }
}

public sealed class IBanToBankAccountDetailResponse {
	public string? DepositNumber { get; set; }
	public string? IBanType { get; set; }
	public string? BankCode { get; set; }
	public string? BankName { get; set; }
	public string? OwnerName { get; set; }
}