namespace SinaMN75U.Data.Responses;

public sealed class ParkingResponse : BaseResponse<TagParking, BaseJson> {
	public required string Title { get; set; }
}

public sealed class ParkingReportResponse : BaseResponse<TagParkingReport, BaseJson> {
	public required DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public decimal? Amount { get; set; }

	public required Guid VehicleId { get; set; }
	public VehicleResponse? Vehicle { get; set; }

	public required Guid ParkingId { get; set; }
	public ParkingResponse? Parking { get; set; }
}