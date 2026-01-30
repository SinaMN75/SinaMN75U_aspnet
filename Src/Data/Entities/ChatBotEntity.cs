namespace SinaMN75U.Data.Entities;

[Table("ChatBots")]
public class ChatBotEntity: BaseEntity<TagChatBot, ChatBotJsonData> {
	public UserEntity Creator { get; set; } = null!;
	public required Guid CreatorId { get; set; }
	
	public ChatBotResponse MapToResponse() => new() {
		Id = Id,
		CreatedAt = CreatedAt,
		UpdatedAt = UpdatedAt,
		DeletedAt = DeletedAt,
		JsonData = JsonData,
		Tags = Tags,
		CreatorId = CreatorId
	};
}

public class ChatBotJsonData {
	public List<ChatBotHistoryItem> History { get; set; } = [];
}

public class ChatBotHistoryItem {
	public required string User { get; set; }
	public required string Bot { get; set; }
}