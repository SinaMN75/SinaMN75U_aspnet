namespace SinaMN75U.Data.Params;

public sealed class ConversationCreateParams : BaseCreateParams<TagConversation> {
	/// <summary>Direct: the other user (an existing conversation with them is returned). Group: the members.</summary>
	public List<Guid> UserIds { get; set; } = [];

	public string? Title { get; set; }
}

public sealed class ConversationUpdateParams : BaseUpdateParams<TagConversation> {
	public string? Title { get; set; }
	public List<Guid>? AddUserIds { get; set; }
	public List<Guid>? RemoveUserIds { get; set; }
}

public sealed class ConversationReadParams : BaseReadParams<TagConversation> {
	public ConversationSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class MessageCreateParams : BaseCreateParams<TagMessage> {
	[UValidationRequired("idIsRequired")]
	public Guid ConversationId { get; set; }

	[UValidationRequired("textIsRequired"), UValidationStringLength(1, 4000, "textIsTooLong")]
	public string Text { get; set; } = null!;

	public Guid? ReplyToId { get; set; }
	public string? LinkType { get; set; }
	public Guid? LinkId { get; set; }
}

public sealed class MessageUpdateParams : BaseUpdateParams<TagMessage> {
	public string? Text { get; set; }
}

public sealed class MessageReadParams : BaseReadParams<TagMessage> {
	[UValidationRequired("idIsRequired")]
	public Guid ConversationId { get; set; }

	/// <summary>Older messages than this (for scrolling up); newest first.</summary>
	public DateTime? Before { get; set; }

	public MessageSelectorArgs SelectorArgs { get; set; } = new();
}
