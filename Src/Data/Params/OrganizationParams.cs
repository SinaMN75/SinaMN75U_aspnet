namespace SinaMN75U.Data.Params;

public sealed class OrganizationCreateParams : BaseCreateParams<TagOrganization> {
	[UValidationRequired("titleIsRequired"), UValidationStringLength(2, 100, "TitleMinLength")]
	public string Title { get; set; } = null!;

	[UValidationRequired("userIsRequired")]
	public Guid OwnerId { get; set; }

	public string? OwnerPassword { get; set; }
	public decimal CommissionPercent { get; set; }
}

public sealed class OrganizationUpdateParams : BaseUpdateParams<TagOrganization> {
	public string? Title { get; set; }
	public Guid? OwnerId { get; set; }
	public string? OwnerPassword { get; set; }
	public decimal? CommissionPercent { get; set; }
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
