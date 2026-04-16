namespace SinaMN75U.Data.Responses;

public sealed class NotificationResponse : BaseResponse<TagNotification, GeneralJsonData> {
	public string? ZipCode { get; set; }
	
	public UserResponse? Creator { get; set; }
	public required Guid CreatorId { get; set; }	
	
	public UserResponse? User { get; set; }
	public required Guid UserId { get; set; }
}