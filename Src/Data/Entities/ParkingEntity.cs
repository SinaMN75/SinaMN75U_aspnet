namespace SinaMN75U.Data.Entities;

[Table("Parkings")]
public class ParkingEntity : BaseEntity<TagParking, ParkingJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	public required Guid CreatorId { get; set; }

	public UserEntity Creator { get; set; } = null!;

	public IEnumerable<Guid> Users { get; set; } = [];

	public decimal EntrancePrice { get; set; }
	public decimal HourlyPrice { get; set; }
	public decimal DailyPrice { get; set; }
	
	public ParkingResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		Title = Title,
		CreatorId = CreatorId
	};
}

public class ParkingJson {
	public string? Title { get; set; }
}

[Table("ParkingReport")]
public class ParkingReportEntity : BaseEntity<TagParkingReport, ParkingReportJson> {
	public required DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }

	public decimal? Amount { get; set; }

	public required Guid CreatorId { get; set; }
	public UserEntity Creator { get; set; } = null!;

	public required Guid VehicleId { get; set; }
	public VehicleEntity Vehicle { get; set; } = null!;

	public required Guid ParkingId { get; set; }
	public ParkingEntity Parking { get; set; } = null!;

	public ParkingReportResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		CreatorId = CreatorId,
		StartDate = StartDate,
		VehicleId = VehicleId,
		ParkingId = ParkingId,
		Amount = Amount,
		EndDate = EndDate
	};
}

public class ParkingReportJson {
	public string? Title { get; set; }
}