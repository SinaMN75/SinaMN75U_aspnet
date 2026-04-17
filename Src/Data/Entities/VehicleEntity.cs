namespace SinaMN75U.Data.Entities;

[Table("Vehicles")]
[Index(nameof(LicencePlate), Name = "IX_Vehicles_NumberPlate", IsUnique = true)]
public sealed class VehicleEntity : BaseEntity<TagVehicle, BaseJsonData> {
	[Required, MinLength(6), MaxLength(10)]
	public required string LicencePlate { get; set; }

	[MaxLength(100)]
	public string? Title { get; set; }
	
	[MaxLength(100)]
	public string? Brand { get; set; }

	[MaxLength(100)]
	public string? Color { get; set; }
}
