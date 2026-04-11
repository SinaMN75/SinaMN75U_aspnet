namespace SinaMN75U.Data.Entities;

[Table("Vehicles")]
[Index(nameof(NumberPlate), Name = "IX_Vehicles_NumberPlate", IsUnique = true)]
public sealed class VehicleEntity : BaseEntity<TagVehicle, GeneralJsonData> {
	[Required]
	[MinLength(6)]
	[MaxLength(10)]
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
