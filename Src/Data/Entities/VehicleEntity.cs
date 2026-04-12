namespace SinaMN75U.Data.Entities;

[Table("Vehicles")]
[Index(nameof(LicencePlate), Name = "IX_Vehicles_NumberPlate", IsUnique = true)]
public sealed class VehicleEntity : BaseEntity<TagVehicle, GeneralJsonData> {
	[Required]
	[MinLength(6)]
	[MaxLength(10)]
	public required string LicencePlate { get; set; }

	[MaxLength(100)]
	public string? Brand { get; set; }

	[MaxLength(100)]
	public string? Color { get; set; }

	public required Guid CreatorId { get; set; }
	public UserEntity Creator { get; set; } = null!;

	public VehicleResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		LicencePlate = LicencePlate,
		Brand = Brand,
		Color = Color,
		CreatorId = CreatorId
	};
}
