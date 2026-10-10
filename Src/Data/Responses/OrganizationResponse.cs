namespace SinaMN75U.Data.Responses;

public sealed class OrganizationResponse : BaseResponse<TagOrganization, OrganizationJson> {
	public required string Title { get; set; }
	public Guid OwnerId { get; set; }
	public decimal Balance { get; set; }
	public List<TagModule>? Modules { get; set; }
	public DateTime? SubscriptionEndsAt { get; set; }
	public BankAccountResponse? BankAccount { get; set; }
	public OrganizationPaymentsResponse? Payments { get; set; }
}

public sealed class OrganizationPaymentsResponse {
	public decimal Received { get; set; }
	public int ReceivedCount { get; set; }
	public decimal Commission { get; set; }
	public decimal Refunded { get; set; }
	public decimal Settled { get; set; }
}

public sealed class SubscriptionPlanResponse : BaseResponse<TagSubscriptionPlan, SubscriptionPlanJson> {
	public required string Title { get; set; }
	public int Order { get; set; }
}

public sealed class SubscriptionQuoteResponse {
	public Guid PlanId { get; set; }
	public string Title { get; set; } = "";
	public int Months { get; set; }
	public bool Trial { get; set; }
	public bool Renewal { get; set; }
	public decimal Price { get; set; }
	public decimal Credit { get; set; }
	public decimal Payable { get; set; }
	public DateTime StartsAt { get; set; }
	public DateTime ExpiresAt { get; set; }
	public List<string> Replaces { get; set; } = [];
}

public sealed class SubscriptionBuyResponse {
	public Guid OrganizationId { get; set; }
	public Guid SubscriptionId { get; set; }
	public decimal Payable { get; set; }
	public bool Active { get; set; }
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

