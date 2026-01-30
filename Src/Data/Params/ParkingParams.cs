namespace SinaMN75U.Data.Params;

public class ParkingCreateParams : BaseCreateParams<TagParking> {
	public required string Title { get; set; }
	public required Guid CreatorId { get; set; }
	public required decimal EntrancePrice { get; set; }
	public required decimal HourlyPrice { get; set; }
	public required decimal DailyPrice { get; set; }
	public IEnumerable<Guid> Users { get; set; } = [];

	public ParkingEntity MapToEntity() => new() {
		Tags = Tags,
		CreatorId = CreatorId,
		Title = Title,
		EntrancePrice = EntrancePrice,
		HourlyPrice = HourlyPrice,
		DailyPrice = DailyPrice,
		JsonData = new ParkingJson {
			Title = Title,
		}
	};
}

public class ParkingUpdateParams : BaseUpdateParams<TagParking> {
	public string? Title { get; set; }
	public decimal? EntrancePrice { get; set; }
	public decimal? HourlyPrice { get; set; }
	public decimal? DailyPrice { get; set; }

	public IEnumerable<Guid> AddUsers { get; set; } = [];
	public IEnumerable<Guid> RemoveUsers { get; set; } = [];
	public IEnumerable<Guid> Users { get; set; } = [];

	public ParkingEntity MapToEntity(ParkingEntity e) {
		e.UpdatedAt = DateTime.UtcNow;
		if (Title.IsNotNull()) e.JsonData.Title = Title;
		if (EntrancePrice.IsNotNull()) e.EntrancePrice = EntrancePrice.Value;
		if (HourlyPrice.IsNotNull()) e.HourlyPrice = HourlyPrice.Value;
		if (DailyPrice.IsNotNull()) e.DailyPrice = DailyPrice.Value;
		if (Tags.IsNotNull()) e.Tags = Tags;
		if (AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(AddTags);
		if (RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(tag => RemoveTags.Contains(tag));
		return e;
	}
}

public class ParkingReadParams : BaseReadParams<TagParking> {
	public string? Title { get; set; }
	public Guid? CreatorId { get; set; }
	public IEnumerable<Guid> Users { get; set; } = [];

	public required ParkingSelectorArgs SelectorArgs { get; set; }
}

public class ParkingReportCreateParams : BaseCreateParams<TagParkingReport> {
	public Guid? CreatorId { get; set; }
	public required Guid ParkingId { get; set; }
	public required DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public decimal? Amount { get; set; }
	public required string NumberPlate { get; set; }
	public string? Title { get; set; }
}

public class ParkingReportUpdateParams : BaseUpdateParams<TagParkingReport> {
	public Guid? CreatorId { get; set; }
	public Guid? VehicleId { get; set; }
	public Guid? ParkingId { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public decimal? Amount { get; set; }
	public string? Title { get; set; }

	public ParkingReportEntity MapToEntity(ParkingReportEntity e) {
		e.UpdatedAt = DateTime.UtcNow;
		if (CreatorId.IsNotNull()) e.CreatorId = CreatorId.Value;
		if (VehicleId.IsNotNull()) e.VehicleId = VehicleId.Value;
		if (ParkingId.IsNotNull()) e.ParkingId = ParkingId.Value;
		if (StartDate != null) e.StartDate = StartDate.Value;
		if (EndDate != null) e.EndDate = EndDate;
		if (Amount.IsNotNull()) e.Amount = Amount.Value;
		if (Title.IsNotNull()) e.JsonData.Title = Title;
		if (Tags.IsNotNull()) e.Tags = Tags;
		if (AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(AddTags);
		if (RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(tag => RemoveTags.Contains(tag));
		return e;
	}
}

public class ParkingReportReadParams : BaseReadParams<TagParking> {
	public Guid? CreatorId { get; set; }
	public Guid? VehicleId { get; set; }
	public Guid? ParkingId { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }

	public required ParkingReportSelectorArgs SelectorArgs { get; set; }
}