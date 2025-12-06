namespace SinaMN75U.Data.Params;

public sealed class ChatBotCreateParams : BaseParams {
	public Guid? ChatId { get; set; }
	public Guid? UserId { get; set; }
	public required string Message { get; set; }
	public required List<TagChatBot> Tags { get; set; }
}

public sealed class ChatBotReadParams : BaseReadParams<int> {
	public Guid? UserId { get; set; }
}