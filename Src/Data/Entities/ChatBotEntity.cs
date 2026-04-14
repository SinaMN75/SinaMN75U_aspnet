namespace SinaMN75U.Data.Entities;

[Table("ChatBots")]
public sealed class ChatBotEntity : BaseEntity<TagChatBot, ChatBotJsonData> {
	public required Guid CreatorId { get; set; }
	public UserEntity Creator { get; set; } = null!;

	public ChatBotResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		JsonData = JsonData,
		Tags = Tags,
		CreatorId = CreatorId
	};
}

public sealed class ChatBotJsonData {
	public ICollection<ChatBotHistoryItem> History { get; set; } = [];
}

public sealed class ChatBotHistoryItem {
	public required string User { get; set; }
	public required string Bot { get; set; }
}