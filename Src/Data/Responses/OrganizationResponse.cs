namespace SinaMN75U.Data.Responses;

public sealed class OrganizationResponse : BaseResponse<TagOrganization, OrganizationJson> {
	public required string Title { get; set; }
	public Guid OwnerId { get; set; }
	public decimal Balance { get; set; }
}

public sealed class StaffShiftResponse : BaseResponse<TagStaffShift, StaffShiftJson> {
	public DateTime StartAt { get; set; }
	public DateTime EndAt { get; set; }
	public Guid? PlaceId { get; set; }
	public Guid UserId { get; set; }
	public string? UserName { get; set; }
	public Guid OrganizationId { get; set; }
}

public sealed class StaffTaskResponse : BaseResponse<TagStaffTask, StaffTaskJson> {
	public required string Title { get; set; }
	public Guid? PlaceId { get; set; }
	public Guid? AssigneeId { get; set; }
	public string? AssigneeName { get; set; }
	public string? RequesterName { get; set; }
	public DateTime? DueDate { get; set; }
	public Guid OrganizationId { get; set; }
}

public sealed class OrganizationCustomerResponse : BaseResponse<TagOrganizationCustomer, OrganizationCustomerJson> {
	public Guid UserId { get; set; }
	public string? UserName { get; set; }
	public string? PhoneNumber { get; set; }
	public string? NationalCode { get; set; }
	public Guid OrganizationId { get; set; }
}

public sealed class ActivityLogResponse : BaseResponse<TagActivityLog, ActivityLogJson> {
	public required string Path { get; set; }
	public Guid? OrganizationId { get; set; }
	public Guid? EntityId { get; set; }
}

