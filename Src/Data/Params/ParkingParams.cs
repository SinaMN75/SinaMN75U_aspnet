namespace SinaMN75U.Data.Params;

public sealed class ParkingCreateParams : BaseCreateParams<TagParking> {
	[UValidationRequired("TitleRequired")]
	public string Title { get; set; } = null!;

	[UValidationRequired("EntrancePriceRequired")]
	public decimal EntrancePrice { get; set; }

	[UValidationRequired("HourlyPriceRequired")]
	public decimal HourlyPrice { get; set; }

	[UValidationRequired("DailyPriceRequired")]
	public decimal DailyPrice { get; set; }
}

public sealed class ParkingUpdateParams : BaseUpdateParams<TagParking> {
	public decimal? EntrancePrice { get; set; }
	public decimal? HourlyPrice { get; set; }
	public decimal? DailyPrice { get; set; }
}

public sealed class ParkingReadParams : BaseReadParams<TagParking> {
	public ParkingSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class ParkingUserCreateParams : BaseParams {
	[UValidationRequired("IdRequired")]
	public Guid ParkingId { get; set; }

	[UValidationRequired("UserNameRequired")]
	public string UserName { get; set; } = null!;

	[UValidationRequired("PasswordRequired")]
	public string Password { get; set; } = null!;

	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? PhoneNumber { get; set; }
}

public sealed class ParkingUserReadParams : BaseParams {
	[UValidationRequired("IdRequired")]
	public Guid ParkingId { get; set; }

	public UserSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class ParkingUserDeleteParams : BaseParams {
	[UValidationRequired("IdRequired")]
	public Guid ParkingId { get; set; }

	[UValidationRequired("IdRequired")]
	public Guid UserId { get; set; }
}

public sealed class ParkingReportCreateParams : BaseCreateParams<TagParkingReport> {
	public Guid ParkingId { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public decimal? Amount { get; set; }
	public string NumberPlate { get; set; } = null!;
}

public sealed class ParkingReportUpdateParams : BaseUpdateParams<TagParkingReport> {
	public Guid? CreatorId { get; set; }
	public Guid? VehicleId { get; set; }
	public Guid? ParkingId { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public decimal? Amount { get; set; }
}

public sealed class ParkingReportReadParams : BaseReadParams<TagParkingReport> {
	public Guid? VehicleId { get; set; }
	public Guid? ParkingId { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }

	public ParkingReportSelectorArgs SelectorArgs { get; set; } = new();
}