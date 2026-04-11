namespace SinaMN75U.Data.Responses;

public sealed class ParkingResponse : BaseResponse<TagParking, GeneralJsonData> {
	public required string Title { get; set; }
	public required Guid CreatorId { get; set; }
	public UserResponse Creator { get; set; } = null!;
	public IEnumerable<Guid> Users { get; set; } = [];
}

public sealed class ParkingReportResponse : BaseResponse<TagParkingReport, GeneralJsonData> {
	public required DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public decimal? Amount { get; set; }

	public Guid? CreatorId { get; set; }
	public UserEntity? Creator { get; set; }

	public required Guid VehicleId { get; set; }
	public VehicleEntity? Vehicle { get; set; }

	public required Guid ParkingId { get; set; }
	public ParkingEntity? Parking { get; set; }
}