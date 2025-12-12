namespace SinaMN75U.Data.Entities;

[Table("ChatBots")]
public class ChatBotEntity: BaseEntity<TagChatBot, ChatBotJsonData> {
	public UserEntity User { get; set; } = null!;
	public required Guid UserId { get; set; }
	
	public new ChatBotResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		UserId = UserId
	};
}

public class ChatBotJsonData {
	public List<ChatBotHistoryItem> History { get; set; } = [];
}

public class ChatBotHistoryItem {
	public required string User { get; set; }
	public required string Bot { get; set; }
}