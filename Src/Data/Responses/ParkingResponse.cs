namespace SinaMN75U.Data.Responses;

public class ParkingResponse : BaseResponse<TagParking, ParkingJson> {
	public required string Title { get; set; }
	public required Guid CreatorId { get; set; }
	public UserResponse Creator { get; set; } = null!;
	public IEnumerable<Guid> Users { get; set; } = [];
}

public class ParkingReportResponse : BaseResponse<TagParkingReport, ParkingReportJson> {
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
