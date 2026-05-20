namespace SinaMN75U.Data.Entities;

[Table("Notifications")]
public sealed class NotificationEntity : BaseEntity<TagNotification, BaseJson> {
	public string? ZipCode { get; set; }
	
	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;
}
