namespace SinaMN75U.Data.Responses;

public sealed class ParkingResponse : BaseResponse<TagParking, BaseJson> {
	public required string Title { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public int Capacity { get; set; }
	public decimal EntrancePrice { get; set; }
	public decimal HourlyPrice { get; set; }
	public decimal DailyPrice { get; set; }
}

public sealed class ParkingReportResponse : BaseResponse<TagParkingReport, BaseJson> {
	public required DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public decimal? Amount { get; set; }

	public required Guid VehicleId { get; set; }
	public VehicleResponse? Vehicle { get; set; }

	public required Guid ParkingId { get; set; }
	public ParkingResponse? Parking { get; set; }

	public string? ReceiptNumber { get; set; }
	public string? SpotNumber { get; set; }
	public string? CustomerPhoneNumber { get; set; }
	public decimal Discount { get; set; }
	public decimal PaidAmount { get; set; }
	public TagParkingPayment? PaymentMethod { get; set; }
	public string? TrackingCode { get; set; }
	public Guid? SubscriptionId { get; set; }
	public Guid? ShiftId { get; set; }
}
public sealed class ParkingTariffResponse : BaseResponse<TagParkingTariff, BaseJson> {
	public required Guid ParkingId { get; set; }
	public required TagVehicle VehicleType { get; set; }

	public decimal EntrancePrice { get; set; }
	public decimal DayHourlyPrice { get; set; }
	public decimal NightHourlyPrice { get; set; }
	public decimal DailyCap { get; set; }

	public decimal WeeklyPrice { get; set; }
	public decimal MonthlyPrice { get; set; }
	public decimal QuarterlyPrice { get; set; }

	public int FreeMinutes { get; set; }
	public int NightStartHour { get; set; }
	public int NightEndHour { get; set; }
	public int HolidayExtraPercent { get; set; }
	public bool RoundToFullHour { get; set; }
	public bool PerMinuteAfterFirstHour { get; set; }

	public int SubscriptionDailyEntryLimit { get; set; }
	public bool SubscriptionOfficeHoursOnly { get; set; }
	public int SubscriptionExpiryReminderDays { get; set; }
}

public sealed class ParkingSubscriptionResponse : BaseResponse<TagParkingSubscription, BaseJson> {
	public required Guid ParkingId { get; set; }
	public required Guid VehicleId { get; set; }
	public VehicleResponse? Vehicle { get; set; }

	public string? CustomerName { get; set; }
	public string? CustomerPhoneNumber { get; set; }

	public decimal Price { get; set; }
	public required DateTime StartDate { get; set; }
	public required DateTime ExpiryDate { get; set; }
	public int DailyEntryLimit { get; set; }
	public bool OfficeHoursOnly { get; set; }

	/// Negative once the subscription has lapsed, so the client can render "expired" without re-deriving it.
	public int RemainingDays { get; set; }
}

public sealed class ParkingPlateFlagResponse : BaseResponse<TagParkingPlateFlag, BaseJson> {
	public required Guid ParkingId { get; set; }
	public required string LicencePlate { get; set; }
	public string? Reason { get; set; }
	public decimal? Amount { get; set; }
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
	public string? SpotNumber { get; set; }
}

public sealed class ParkingStaffResponse : BaseResponse<TagParkingStaff, BaseJson> {
	public required Guid ParkingId { get; set; }
	public required Guid UserId { get; set; }
	public UserResponse? User { get; set; }
	public string? ShiftTitle { get; set; }
	public int MaxDiscountPercent { get; set; }
}

public sealed class ParkingShiftResponse : BaseResponse<TagParkingShift, BaseJson> {
	public required Guid ParkingId { get; set; }
	public required DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }

	public decimal CashTotal { get; set; }
	public decimal CardTotal { get; set; }
	public decimal IpgTotal { get; set; }
	public decimal CountedCash { get; set; }
	public int EntryCount { get; set; }
	public int ExitCount { get; set; }

	public decimal Total => CashTotal + CardTotal + IpgTotal;
}

public sealed class ParkingPlateStatusResponse {
	public required string LicencePlate { get; set; }
	public VehicleResponse? Vehicle { get; set; }

	public ParkingSubscriptionResponse? Subscription { get; set; }
	public ParkingPlateFlagResponse? Reservation { get; set; }
	public ICollection<ParkingPlateFlagResponse> Flags { get; set; } = [];

	/// The still-open entry for this plate, when it is already inside the parking.
	public ParkingReportResponse? OpenReport { get; set; }

	public ParkingTariffResponse? Tariff { get; set; }

	public bool HasActiveSubscription { get; set; }
	public bool IsBanned { get; set; }
	public bool IsInside { get; set; }
}

public sealed class ParkingBillLineResponse {
	public required string Key { get; set; }
	public decimal Amount { get; set; }

	/// Filled for lines whose label needs a number, e.g. the minutes billed after the first hour.
	public int? Minutes { get; set; }
	public bool IsFree { get; set; }
}

public sealed class ParkingBillResponse {
	public Guid? ReportId { get; set; }
	public required string LicencePlate { get; set; }
	public TagVehicle VehicleType { get; set; }
	public string? SpotNumber { get; set; }
	public string? ReceiptNumber { get; set; }

	public required DateTime StartDate { get; set; }
	public required DateTime EndDate { get; set; }
	public int TotalMinutes { get; set; }

	public ICollection<ParkingBillLineResponse> Lines { get; set; } = [];

	public decimal Subtotal { get; set; }
	public decimal Discount { get; set; }
	public decimal Payable { get; set; }
	public decimal DailyCap { get; set; }
	public bool IsCapped { get; set; }
	public bool IsSubscription { get; set; }
	public bool IsNightRateApplied { get; set; }
}

public sealed class ParkingDashboardResponse {
	public required Guid ParkingId { get; set; }
	public required string Title { get; set; }
	public int Capacity { get; set; }
	public int InsideCount { get; set; }

	public decimal ShiftRevenue { get; set; }
	public ParkingShiftResponse? OpenShift { get; set; }

	public ICollection<ParkingReportResponse> RecentReports { get; set; } = [];
}

public sealed class ParkingInsideVehicleResponse {
	public required Guid ReportId { get; set; }
	public required string LicencePlate { get; set; }
	public TagVehicle VehicleType { get; set; }
	public required DateTime StartDate { get; set; }
	public string? SpotNumber { get; set; }
	public int StayedMinutes { get; set; }
	public decimal EstimatedAmount { get; set; }
	public bool HasSubscription { get; set; }
	public bool IsCapped { get; set; }
}

public sealed class ParkingSeedAccountResponse {
	public required string UserName { get; set; }
	public required string Password { get; set; }
	public required string FullName { get; set; }
	public required string Role { get; set; }
}

public sealed class ParkingSeedResponse {
	public ICollection<ParkingSeedAccountResponse> Accounts { get; set; } = [];
	public ICollection<ParkingResponse> Parkings { get; set; } = [];

	public int Tariffs { get; set; }
	public int Vehicles { get; set; }
	public int Subscriptions { get; set; }
	public int PlateFlags { get; set; }
	public int Staff { get; set; }
	public int Shifts { get; set; }
	public int OpenReports { get; set; }
	public int ClosedReports { get; set; }
}
