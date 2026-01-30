namespace SinaMN75U.Data.Entities;

[Table("Vehicles")]
public class VehicleEntity : BaseEntity<TagVehicle, VehicleJson> {
	[Required, MinLength(6), MaxLength(10)]
	public required string NumberPlate { get; set; }

	[MaxLength(100)]
	public string? Title { get; set; }
	
	[MaxLength(100)]
	public string? Brand { get; set; }

	[MaxLength(100)]
	public string? Color { get; set; }
	
	public VehicleResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		NumberPlate = NumberPlate,
		Brand = Brand,
		Color = Color,
		Title = Title
	};
}

public class VehicleJson {
	public string? Title { get; set; }
}