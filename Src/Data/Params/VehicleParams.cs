namespace SinaMN75U.Data.Params;

public class VehicleCreateParams : BaseCreateParams<TagVehicle> {
	public required string NumberPlate { get; set; }
	public string? Title { get; set; }
	public string? Brand { get; set; }
	public string? Color { get; set; }

	public VehicleEntity MapToEntity() => new() {
		Id = Id ?? Guid.CreateVersion7(),
		JsonData = new VehicleJson(),
		Tags = Tags,
		NumberPlate = NumberPlate,
		Brand =  Brand,
		Color = Color,
		Title =  Title
	};
}

public class VehicleUpdateParams : BaseUpdateParams<TagVehicle> {
	public string? NumberPlate { get; set; }
	public string? Title { get; set; }
	public string? Brand { get; set; }
	public string? Color { get; set; }

	public VehicleEntity MapToEntity(VehicleEntity e) {
		e.UpdatedAt = DateTime.UtcNow;
		if (NumberPlate.IsNotNull()) e.NumberPlate = NumberPlate;
		if (Title.IsNotNull()) e.Title = Title;
		if (Brand.IsNotNull()) e.Brand = Brand;
		if (Color.IsNotNull()) e.Color = Color;
		if (Tags.IsNotNull()) e.Tags = Tags;
		if (AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(AddTags);
		if (RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(tag => RemoveTags.Contains(tag));
		return e;
	}
}

public class VehicleReadParams : BaseReadParams<TagVehicle> {
	public string? NumberPlate { get; set; }
	public string? Title { get; set; }
	public string? Brand { get; set; }
	public string? Color { get; set; }

	public required VehicleSelectorArgs SelectorArgs { get; set; }
}