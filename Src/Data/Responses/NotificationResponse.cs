namespace SinaMN75U.Data.Responses;

public sealed class NotificationResponse : BaseResponse<TagNotification, BaseJsonData> {
	public string? ZipCode { get; set; }
	
	public UserResponse? User { get; set; }
	public required Guid UserId { get; set; }
}