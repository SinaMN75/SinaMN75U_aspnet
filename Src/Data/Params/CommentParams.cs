namespace SinaMN75U.Data.Params;

public sealed class CommentCreateParams : BaseCreateParams<TagComment> {
	[UValidationRequired("CommentRequired")]
	public required string Description { get; set; }

	public decimal Score { get; set; } = 0;
	public TagReaction? Reaction { get; set; }

	public Guid? ParentId { get; set; }
	public Guid? ProductId { get; set; }
	public Guid? UserId { get; set; }
}

public sealed class CommentUpdateParams : BaseUpdateParams<TagComment> {
	public string? Description { get; set; }
	public decimal? Score { get; set; }
}

public sealed class CommentReadParams : BaseReadParams<TagComment> {
	public Guid? ProductId { get; set; }
	public Guid? UserId { get; set; }
	public CommentSelectorArgs SelectorArgs { get; set; } = new();
}