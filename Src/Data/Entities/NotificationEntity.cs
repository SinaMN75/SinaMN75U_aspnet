namespace SinaMN75U.Data.Entities;

[Table("Notifications")]
public sealed class NotificationEntity : BaseEntity<TagNotification, GeneralJsonData> {
	public string? ZipCode { get; set; }

	public required Guid CreatorId { get; set; }
	public UserEntity Creator { get; set; } = null!;
	
	public required Guid Userd { get; set; }
	public UserEntity User { get; set; } = null!;

	public NotificationResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		JsonData = JsonData,
		Tags = Tags,
		ZipCode = ZipCode,
		CreatorId = CreatorId,
		UserId = Userd
	};
}
