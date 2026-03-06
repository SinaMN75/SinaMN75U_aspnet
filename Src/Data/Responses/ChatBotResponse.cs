namespace SinaMN75U.Data.Responses;

public sealed class ChatBotResponse : BaseResponse<TagChatBot, ChatBotJsonData> {
	public required Guid CreatorId { get; set; }
}