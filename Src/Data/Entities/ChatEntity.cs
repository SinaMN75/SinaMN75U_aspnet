namespace SinaMN75U.Data.Entities;

// ---------------- Conversation ----------------

[Table("Conversations")]
[Microsoft.EntityFrameworkCore.Index(nameof(DirectKey), IsUnique = true, Name = "IX_Conversations_DirectKey")]
public sealed class ConversationEntity : BaseEntity<TagConversation, ConversationJson> {
	[MaxLength(200)]
	public string? Title { get; set; }

	[MaxLength(80)]
	public string? DirectKey { get; set; } // the two members' ids; one direct conversation per pair

	public DateTime LastMessageAt { get; set; }

	public ICollection<UserEntity> Users { get; set; } = [];
	public ICollection<MessageEntity> Messages { get; set; } = [];
}

public sealed class ConversationJson : BaseJson {
	public string? LastMessageText { get; set; }
	public Guid? LastMessageUserId { get; set; }
	public List<ConversationRead> Reads { get; set; } = [];
}

/// <summary>Until when a member has read the conversation.</summary>
public sealed class ConversationRead {
	public required Guid UserId { get; set; }
	public required DateTime At { get; set; }
}

// ---------------- Message ----------------

[Table("Messages")]
[Microsoft.EntityFrameworkCore.Index(nameof(ConversationId), nameof(CreatedAt), Name = "IX_Messages_ConversationId_CreatedAt")]
public sealed class MessageEntity : BaseEntity<TagMessage, MessageJson> {
	[MaxLength(4000)]
	public required string Text { get; set; }

	public required Guid ConversationId { get; set; }
	public ConversationEntity Conversation { get; set; } = null!;
}

public sealed class MessageJson : BaseJson {
	public Guid? ReplyToId { get; set; }
	public string? LinkType { get; set; }
	public Guid? LinkId { get; set; }
}
