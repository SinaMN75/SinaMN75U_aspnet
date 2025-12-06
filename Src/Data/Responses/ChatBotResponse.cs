namespace SinaMN75U.Data.Responses;

public class ChatBotResponse : BaseResponse<TagChatBot, ChatBotJsonData> {
	public required Guid UserId { get; set; }
	// public required UserResponse User { get; set; }
}
