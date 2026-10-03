namespace SinaMN75U.Data.Responses;

public sealed class ConversationResponse : BaseResponse<TagConversation, ConversationJson> {
	public string? Title { get; set; }
	public DateTime LastMessageAt { get; set; }
	public int UnreadCount { get; set; } // for the signed-in user

	/// <summary>Public fields only.</summary>
	public ICollection<UserResponse>? Users { get; set; }
}

public sealed class MessageResponse : BaseResponse<TagMessage, MessageJson> {
	public required string Text { get; set; }
	public required Guid ConversationId { get; set; }

	/// <summary>The sender: public fields only.</summary>
	public UserResponse? User { get; set; }
}
