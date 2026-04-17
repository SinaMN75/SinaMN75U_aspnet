namespace SinaMN75U.Data.Entities;

[Table("Notifications")]
public sealed class NotificationEntity : BaseEntity<TagNotification, BaseJsonData> {
	public string? ZipCode { get; set; }
	
	public required Guid Userd { get; set; }
	public UserEntity User { get; set; } = null!;
}
