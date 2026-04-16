namespace SinaMN75U.Data.Params;

public sealed class ParkingCreateParams : BaseCreateParams<TagParking> {
	public required decimal EntrancePrice { get; set; }
	public required decimal HourlyPrice { get; set; }
	public required decimal DailyPrice { get; set; }
	public IEnumerable<Guid> Users { get; set; } = [];
}

public sealed class ParkingUpdateParams : BaseUpdateParams<TagParking> {
	public decimal? EntrancePrice { get; set; }
	public decimal? HourlyPrice { get; set; }
	public decimal? DailyPrice { get; set; }

	public IEnumerable<Guid> AddUsers { get; set; } = [];
	public IEnumerable<Guid> RemoveUsers { get; set; } = [];
	public IEnumerable<Guid> Users { get; set; } = [];
}

public sealed class ParkingReadParams : BaseReadParams<TagParking> {
	public string? Title { get; set; }
	public IEnumerable<Guid> Users { get; set; } = [];

	public required ParkingSelectorArgs SelectorArgs { get; set; }
}

public sealed class ParkingReportCreateParams : BaseCreateParams<TagParkingReport> {
	public required Guid ParkingId { get; set; }
	public required DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public decimal? Amount { get; set; }
	public required string NumberPlate { get; set; }
}

public sealed class ParkingReportUpdateParams : BaseUpdateParams<TagParkingReport> {
	public Guid? CreatorId { get; set; }
	public Guid? VehicleId { get; set; }
	public Guid? ParkingId { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public decimal? Amount { get; set; }
}

public sealed class ParkingReportReadParams : BaseReadParams<TagParking> {
	public Guid? VehicleId { get; set; }
	public Guid? ParkingId { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }

	public required ParkingReportSelectorArgs SelectorArgs { get; set; }
}