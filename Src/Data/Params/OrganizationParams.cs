namespace SinaMN75U.Data.Params;

public sealed class OrganizationCreateParams : BaseCreateParams<TagOrganization> {
	[UValidationRequired("titleIsRequired"), UValidationStringLength(2, 100, "TitleMinLength")]
	public string Title { get; set; } = null!;

	[UValidationRequired("userIsRequired")]
	public Guid OwnerId { get; set; }

	public string? OwnerPassword { get; set; }
	public decimal CommissionPercent { get; set; }
	public string? LogoUrl { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public string? NationalId { get; set; }
	public string? EconomicCode { get; set; }
	public decimal? VatPercent { get; set; }
	public string? TaxServiceId { get; set; }
	public OrganizationPlan? Plan { get; set; }
}

public sealed class OrganizationUpdateParams : BaseUpdateParams<TagOrganization> {
	public string? Title { get; set; }
	public Guid? OwnerId { get; set; }
	public string? OwnerPassword { get; set; }
	public decimal? CommissionPercent { get; set; }
	public string? LogoUrl { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public string? NationalId { get; set; }
	public string? EconomicCode { get; set; }
	public decimal? VatPercent { get; set; }
	public string? TaxServiceId { get; set; }
	public OrganizationPlan? Plan { get; set; }
}

public sealed class OrganizationReadParams : BaseReadParams<TagOrganization> {
	public string? Title { get; set; }
}

public sealed class OrganizationMemberParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	[UValidationRequired("userIsRequired")]
	public Guid UserId { get; set; }

	public List<TagUser> Permissions { get; set; } = [];
	public string? Password { get; set; }
}

public sealed class StaffShiftCreateParams : BaseCreateParams<TagStaffShift> {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	[UValidationRequired("userIsRequired")]
	public Guid UserId { get; set; }

	[UValidationRequired("dateIsRequired")]
	public DateTime StartAt { get; set; }

	[UValidationRequired("dateIsRequired")]
	public DateTime EndAt { get; set; }

	public Guid? PlaceId { get; set; }
}

public sealed class StaffShiftUpdateParams : BaseUpdateParams<TagStaffShift> {
	public Guid? UserId { get; set; }
	public DateTime? StartAt { get; set; }
	public DateTime? EndAt { get; set; }
	public Guid? PlaceId { get; set; }
}

public sealed class StaffShiftReadParams : BaseReadParams<TagStaffShift> {
	public Guid? OrganizationId { get; set; }
	public Guid? UserId { get; set; }
	public Guid? PlaceId { get; set; }
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
}

public sealed class StaffTaskCreateParams : BaseCreateParams<TagStaffTask> {
	public Guid? OrganizationId { get; set; }

	[UValidationRequired("titleIsRequired"), UValidationStringLength(2, 200, "TitleMinLength")]
	public string Title { get; set; } = null!;

	public Guid? PlaceId { get; set; }
	public Guid? AssigneeId { get; set; }
	public DateTime? DueDate { get; set; }
	public string? Description { get; set; }
	public string? Location { get; set; }
	public Guid? RoomId { get; set; }
	public Guid? BedId { get; set; }
}

public sealed class StaffTaskUpdateParams : BaseUpdateParams<TagStaffTask> {
	public string? Title { get; set; }
	public Guid? PlaceId { get; set; }
	public Guid? AssigneeId { get; set; }
	public DateTime? DueDate { get; set; }
	public string? Description { get; set; }
	public string? Location { get; set; }
	public string? DoneNote { get; set; }
	public decimal? Cost { get; set; }
}

public sealed class StaffTaskReadParams : BaseReadParams<TagStaffTask> {
	public Guid? OrganizationId { get; set; }
	public Guid? PlaceId { get; set; }
	public Guid? AssigneeId { get; set; }
	public bool Mine { get; set; }
}

public sealed class OrganizationCustomerSetParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	[UValidationRequired("userIsRequired")]
	public Guid UserId { get; set; }

	public List<TagOrganizationCustomer> Tags { get; set; } = [];
	public string? Note { get; set; }
}

public sealed class OrganizationCustomerReadParams : BaseReadParams<TagOrganizationCustomer> {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	public Guid? UserId { get; set; }
}

public sealed class ActivityLogReadParams : BaseReadParams<TagActivityLog> {
	public Guid? OrganizationId { get; set; }
	public Guid? UserId { get; set; }
	public string? Path { get; set; }
}
