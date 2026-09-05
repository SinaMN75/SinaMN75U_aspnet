namespace SinaMN75U.Data.Params;

public sealed class ParkingCreateParams : BaseCreateParams<TagParking> {
	[UValidationRequired("titleIsRequired")]
	public string Title { get; set; } = null!;

	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public int Capacity { get; set; }

	[UValidationRequired("entrancePriceRequired")]
	public decimal EntrancePrice { get; set; }

	[UValidationRequired("hourlyPriceRequired")]
	public decimal HourlyPrice { get; set; }

	[UValidationRequired("dailyPriceRequired")]
	public decimal DailyPrice { get; set; }
}

public sealed class ParkingUpdateParams : BaseUpdateParams<TagParking> {
	public string? Title { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public int? Capacity { get; set; }
	public decimal? EntrancePrice { get; set; }
	public decimal? HourlyPrice { get; set; }
	public decimal? DailyPrice { get; set; }
}

public sealed class ParkingReadParams : BaseReadParams<TagParking> {
	public ParkingSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class ParkingUserCreateParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid ParkingId { get; set; }

	[UValidationRequired("userNameIsRequired")]
	public string UserName { get; set; } = null!;

	[UValidationRequired("pleaseEnterAPassword")]
	public string Password { get; set; } = null!;

	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? PhoneNumber { get; set; }
}

public sealed class ParkingUserReadParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid ParkingId { get; set; }

	public UserSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class ParkingUserDeleteParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid ParkingId { get; set; }

	[UValidationRequired("idIsRequired")]
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
public sealed class ParkingTariffCreateParams : BaseCreateParams<TagParkingTariff> {
	[UValidationRequired("idIsRequired")]
	public Guid ParkingId { get; set; }

	public TagVehicle VehicleType { get; set; }

	public decimal EntrancePrice { get; set; }
	public decimal DayHourlyPrice { get; set; }
	public decimal NightHourlyPrice { get; set; }
	public decimal DailyCap { get; set; }

	public decimal WeeklyPrice { get; set; }
	public decimal MonthlyPrice { get; set; }
	public decimal QuarterlyPrice { get; set; }

	public int FreeMinutes { get; set; }
	public int NightStartHour { get; set; } = 22;
	public int NightEndHour { get; set; } = 6;
	public int HolidayExtraPercent { get; set; }
	public bool RoundToFullHour { get; set; }
	public bool PerMinuteAfterFirstHour { get; set; } = true;

	public int SubscriptionDailyEntryLimit { get; set; }
	public bool SubscriptionOfficeHoursOnly { get; set; }
	public int SubscriptionExpiryReminderDays { get; set; } = 5;
}

public sealed class ParkingTariffUpdateParams : BaseUpdateParams<TagParkingTariff> {
	public TagVehicle? VehicleType { get; set; }

	public decimal? EntrancePrice { get; set; }
	public decimal? DayHourlyPrice { get; set; }
	public decimal? NightHourlyPrice { get; set; }
	public decimal? DailyCap { get; set; }

	public decimal? WeeklyPrice { get; set; }
	public decimal? MonthlyPrice { get; set; }
	public decimal? QuarterlyPrice { get; set; }

	public int? FreeMinutes { get; set; }
	public int? NightStartHour { get; set; }
	public int? NightEndHour { get; set; }
	public int? HolidayExtraPercent { get; set; }
	public bool? RoundToFullHour { get; set; }
	public bool? PerMinuteAfterFirstHour { get; set; }

	public int? SubscriptionDailyEntryLimit { get; set; }
	public bool? SubscriptionOfficeHoursOnly { get; set; }
	public int? SubscriptionExpiryReminderDays { get; set; }
}

public sealed class ParkingTariffReadParams : BaseReadParams<TagParkingTariff> {
	public Guid? ParkingId { get; set; }
	public TagVehicle? VehicleType { get; set; }
	public ParkingTariffSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class ParkingSubscriptionCreateParams : BaseCreateParams<TagParkingSubscription> {
	[UValidationRequired("idIsRequired")]
	public Guid ParkingId { get; set; }

	[UValidationRequired("licencePlateIsRequired")]
	public string LicencePlate { get; set; } = null!;

	public TagVehicle VehicleType { get; set; } = TagVehicle.Car;

	public string? CustomerName { get; set; }
	public string? CustomerPhoneNumber { get; set; }

	public decimal Price { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? ExpiryDate { get; set; }
	public int DailyEntryLimit { get; set; }
	public bool OfficeHoursOnly { get; set; }
}

public sealed class ParkingSubscriptionUpdateParams : BaseUpdateParams<TagParkingSubscription> {
	public string? CustomerName { get; set; }
	public string? CustomerPhoneNumber { get; set; }
	public decimal? Price { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? ExpiryDate { get; set; }
	public int? DailyEntryLimit { get; set; }
	public bool? OfficeHoursOnly { get; set; }
}

public sealed class ParkingSubscriptionReadParams : BaseReadParams<TagParkingSubscription> {
	public Guid? ParkingId { get; set; }
	public string? LicencePlate { get; set; }
	public string? Query { get; set; }

	/// Active = not expired and not cancelled. ExpiringSoon = expires within ExpiringInDays. Expired = past expiry.
	public bool? IsActive { get; set; }
	public bool? IsExpiringSoon { get; set; }
	public bool? IsExpired { get; set; }
	public int ExpiringInDays { get; set; } = 7;

	public ParkingSubscriptionSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class ParkingPlateFlagCreateParams : BaseCreateParams<TagParkingPlateFlag> {
	[UValidationRequired("idIsRequired")]
	public Guid ParkingId { get; set; }

	[UValidationRequired("licencePlateIsRequired")]
	public string LicencePlate { get; set; } = null!;

	public string? Reason { get; set; }
	public decimal? Amount { get; set; }
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
	public string? SpotNumber { get; set; }
}

public sealed class ParkingPlateFlagUpdateParams : BaseUpdateParams<TagParkingPlateFlag> {
	public string? Reason { get; set; }
	public decimal? Amount { get; set; }
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
	public string? SpotNumber { get; set; }
}

public sealed class ParkingPlateFlagReadParams : BaseReadParams<TagParkingPlateFlag> {
	public Guid? ParkingId { get; set; }
	public string? LicencePlate { get; set; }
	public ParkingPlateFlagSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class ParkingStaffCreateParams : BaseCreateParams<TagParkingStaff> {
	[UValidationRequired("idIsRequired")]
	public Guid ParkingId { get; set; }

	[UValidationRequired("userNameIsRequired")]
	public string UserName { get; set; } = null!;

	[UValidationRequired("pleaseEnterAPassword")]
	public string Password { get; set; } = null!;

	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? PhoneNumber { get; set; }
	public string? ShiftTitle { get; set; }
	public int MaxDiscountPercent { get; set; }
}

public sealed class ParkingStaffUpdateParams : BaseUpdateParams<TagParkingStaff> {
	public string? ShiftTitle { get; set; }
	public int? MaxDiscountPercent { get; set; }
	public string? Password { get; set; }
}

public sealed class ParkingStaffReadParams : BaseReadParams<TagParkingStaff> {
	public Guid? ParkingId { get; set; }
	public ParkingStaffSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class ParkingShiftOpenParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid ParkingId { get; set; }
}

public sealed class ParkingShiftCloseParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid Id { get; set; }

	public decimal CountedCash { get; set; }
}

public sealed class ParkingShiftReadParams : BaseReadParams<TagParkingShift> {
	public Guid? ParkingId { get; set; }
	public bool? IsOpen { get; set; }
	public ParkingShiftSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class ParkingPlateStatusParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid ParkingId { get; set; }

	[UValidationRequired("licencePlateIsRequired")]
	public string LicencePlate { get; set; } = null!;

	public TagVehicle VehicleType { get; set; } = TagVehicle.Car;
}

public sealed class ParkingEntryParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid ParkingId { get; set; }

	[UValidationRequired("licencePlateIsRequired")]
	public string LicencePlate { get; set; } = null!;

	public TagVehicle VehicleType { get; set; } = TagVehicle.Car;
	public DateTime? StartDate { get; set; }
	public string? SpotNumber { get; set; }
	public string? CustomerPhoneNumber { get; set; }
	public bool IsOffline { get; set; }
}

public sealed class ParkingExitCalculateParams : BaseParams {
	public Guid? ReportId { get; set; }
	public Guid? ParkingId { get; set; }
	public string? LicencePlate { get; set; }

	/// Defaults to now. Lets an operator correct the exit moment, or preview a bill for a future time.
	public DateTime? EndDate { get; set; }

	/// Overrides the stored entry time when the operator corrects it on the bill screen.
	public DateTime? CorrectedStartDate { get; set; }

	public decimal Discount { get; set; }
}

public sealed class ParkingExitParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid ReportId { get; set; }

	public DateTime? EndDate { get; set; }
	public DateTime? CorrectedStartDate { get; set; }
	public decimal Discount { get; set; }
	public TagParkingPayment PaymentMethod { get; set; } = TagParkingPayment.Cash;
	public string? TrackingCode { get; set; }
	public bool IsOffline { get; set; }
}

public sealed class ParkingDashboardParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid ParkingId { get; set; }

	public int RecentCount { get; set; } = 10;
}

public sealed class ParkingInsideVehiclesParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid ParkingId { get; set; }

	public string? Query { get; set; }
	public bool? LongerThanADay { get; set; }
	public bool? HasSubscription { get; set; }
	public int PageSize { get; set; } = 50;
	public int PageNumber { get; set; } = 1;
}

public sealed class ParkingSeedParams : BaseParams {
	/// Wipes everything a previous run of this seed created before writing it again, so the
	/// endpoint can be called repeatedly while testing without piling up duplicates.
	public bool Reset { get; set; } = true;

	/// Vehicles still inside the parking when the seed finishes.
	public int OpenEntries { get; set; } = 12;

	/// Completed, paid stays spread over the past week.
	public int ClosedEntries { get; set; } = 40;
}
