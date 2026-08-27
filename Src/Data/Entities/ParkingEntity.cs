namespace SinaMN75U.Data.Entities;

[Table("Parking")]
public sealed class ParkingEntity : BaseEntity<TagParking, BaseJson> {
	[Required]
	[MaxLength(100)]
	public required string Title { get; set; }

	public ICollection<Guid> Users { get; set; } = [];

	[MaxLength(500)]
	public string? Address { get; set; }

	[MaxLength(20)]
	public string? PhoneNumber { get; set; }

	public int Capacity { get; set; }

	public decimal EntrancePrice { get; set; }
	public decimal HourlyPrice { get; set; }
	public decimal DailyPrice { get; set; }
}

[Table("ParkingReport")]
public sealed class ParkingReportEntity : BaseEntity<TagParkingReport, BaseJson> {
	public required DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }

	public decimal? Amount { get; set; }

	public required Guid VehicleId { get; set; }
	public VehicleEntity Vehicle { get; set; } = null!;

	public required Guid ParkingId { get; set; }
	public ParkingEntity Parking { get; set; } = null!;

	[MaxLength(30)]
	public string? ReceiptNumber { get; set; }

	[MaxLength(20)]
	public string? SpotNumber { get; set; }

	[MaxLength(20)]
	public string? CustomerPhoneNumber { get; set; }

	public decimal Discount { get; set; }
	public decimal PaidAmount { get; set; }
	public TagParkingPayment? PaymentMethod { get; set; }

	[MaxLength(50)]
	public string? TrackingCode { get; set; }

	public Guid? SubscriptionId { get; set; }
	public Guid? ShiftId { get; set; }
}

/// One row per parking + vehicle type. Carries both the hourly tariff and the subscription prices,
/// mirroring the two tabs of the tariff screen.
[Table("ParkingTariff")]
public sealed class ParkingTariffEntity : BaseEntity<TagParkingTariff, BaseJson> {
	public required Guid ParkingId { get; set; }
	public ParkingEntity Parking { get; set; } = null!;

	public required TagVehicle VehicleType { get; set; }

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

[Table("ParkingSubscription")]
public sealed class ParkingSubscriptionEntity : BaseEntity<TagParkingSubscription, BaseJson> {
	public required Guid ParkingId { get; set; }
	public ParkingEntity Parking { get; set; } = null!;

	public required Guid VehicleId { get; set; }
	public VehicleEntity Vehicle { get; set; } = null!;

	[MaxLength(100)]
	public string? CustomerName { get; set; }

	[MaxLength(20)]
	public string? CustomerPhoneNumber { get; set; }

	public decimal Price { get; set; }
	public required DateTime StartDate { get; set; }
	public required DateTime ExpiryDate { get; set; }
	public int DailyEntryLimit { get; set; }
	public bool OfficeHoursOnly { get; set; }
}

[Table("ParkingPlateFlag")]
public sealed class ParkingPlateFlagEntity : BaseEntity<TagParkingPlateFlag, BaseJson> {
	public required Guid ParkingId { get; set; }
	public ParkingEntity Parking { get; set; } = null!;

	[Required]
	[MaxLength(10)]
	public required string LicencePlate { get; set; }

	[MaxLength(500)]
	public string? Reason { get; set; }

	public decimal? Amount { get; set; }
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }

	[MaxLength(20)]
	public string? SpotNumber { get; set; }
}

[Table("ParkingStaff")]
public sealed class ParkingStaffEntity : BaseEntity<TagParkingStaff, BaseJson> {
	public required Guid ParkingId { get; set; }
	public ParkingEntity Parking { get; set; } = null!;

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	[MaxLength(100)]
	public string? ShiftTitle { get; set; }

	public int MaxDiscountPercent { get; set; }
}

[Table("ParkingShift")]
public sealed class ParkingShiftEntity : BaseEntity<TagParkingShift, BaseJson> {
	public required Guid ParkingId { get; set; }
	public ParkingEntity Parking { get; set; } = null!;

	public required DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }

	public decimal CashTotal { get; set; }
	public decimal CardTotal { get; set; }
	public decimal IpgTotal { get; set; }
	public decimal CountedCash { get; set; }
	public int EntryCount { get; set; }
	public int ExitCount { get; set; }
}
