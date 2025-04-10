namespace SinaMN75U.Data.Params;

public class CommentCreateParams: BaseParams {
	[URequired("CommentRequired")]
	public required string Description { get; set; }

	public double Score { get; set; } = 0;
	public TagReaction? Reaction { get; set; }
	
	public Guid? ParentId { get; set; }
	public Guid? ProductId { get; set; }
	public Guid? TargetUserId { get; set; }
	public Guid? UserId { get; set; }

	[URequired("TagsRequired")]
	[UMinCollectionLength(1, "TagsRequired")]
	public required List<TagComment> Tags { get; set; }
}

public class CommentUpdateParams {
	[URequired("IdRequired")]
	public required Guid Id { get; set; }

	public string? Comment { get; set; }
	public double? Score { get; set; }
	public List<TagComment>? Tags { get; set; }
}

public class CommentReadParams : BaseReadParams<TagComment> {
	public Guid? UserId { get; set; }
	public Guid? ProductId { get; set; }
	public Guid? TargetUserId { get; set; }
	public bool ShowMedia { get; set; } = false;
}