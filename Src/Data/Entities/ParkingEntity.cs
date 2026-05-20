namespace SinaMN75U.Data.Entities;

[Table("Parking")]
public sealed class ParkingEntity : BaseEntity<TagParking, BaseJson> {
	[Required]
	[MaxLength(100)]
	public required string Title { get; set; }

	public ICollection<Guid> Users { get; set; } = [];

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
}
