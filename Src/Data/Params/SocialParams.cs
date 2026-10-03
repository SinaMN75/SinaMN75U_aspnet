namespace SinaMN75U.Data.Params;

public sealed class PostCreateParams : BaseCreateParams<TagPost> {
	[UValidationStringLength(0, 4000, "textIsTooLong")]
	public string? Text { get; set; }

	/// <summary>A reply to this post.</summary>
	public Guid? ParentId { get; set; }

	public string? LinkType { get; set; }
	public Guid? LinkId { get; set; }
}

public sealed class PostUpdateParams : BaseUpdateParams<TagPost> {
	public string? Text { get; set; }
}

public sealed class PostReadParams : BaseReadParams<TagPost> {
	/// <summary>Posts of the people the signed-in user follows, and their own.</summary>
	public bool Feed { get; set; }

	/// <summary>Stories that haven't expired.</summary>
	public bool Stories { get; set; }

	public Guid? UserId { get; set; }

	/// <summary>The replies of a post.</summary>
	public Guid? ParentId { get; set; }

	public PostSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class PostReactParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid Id { get; set; }

	/// <summary>Empty removes the reaction.</summary>
	public TagReaction? Tag { get; set; }
}

public sealed class ReportCreateParams : BaseCreateParams<TagReport> {
	[UValidationRequired("idIsRequired")]
	public Guid TargetId { get; set; }

	public string? Reason { get; set; }
}

public sealed class ReportUpdateParams : BaseUpdateParams<TagReport> {
	public string? Note { get; set; }
}

public sealed class ReportReadParams : BaseReadParams<TagReport> {
	public Guid? TargetId { get; set; }
	public ReportSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class BlockCreateParams : BaseParams {
	[UValidationRequired("userIsRequired")]
	public Guid UserId { get; set; }
}

public sealed class BlockReadParams : BaseReadParams<TagBlock>;
