namespace SinaMN75U.Data.Params;

public sealed class CommentCreateParams : BaseParams {
	[UValidationRequired("CommentRequired")]
	public required string Description { get; set; }

	public double Score { get; set; } = 0;
	public TagReaction? Reaction { get; set; }

	public Guid? ParentId { get; set; }
	public Guid? ProductId { get; set; }
	public Guid? TargetUserId { get; set; }
	public Guid? UserId { get; set; }

	[UValidationRequired("TagsRequired")]
	[UValidationMinCollectionLength(1, "TagsRequired")]
	public required List<TagComment> Tags { get; set; }
	
	public CommentEntity MapToEntity() => new() {
		Description = Description,
		Score = Score,
		ParentId = ParentId,
		ProductId = ProductId,
		TargetUserId = TargetUserId,
		UserId = UserId ?? Guid.Empty,
		JsonData = new CommentJson(),
		Tags = Tags
	};

}

public sealed class CommentUpdateParams : BaseUpdateParams<TagComment> {
	public string? Description { get; set; }
	public double? Score { get; set; }
	
	public CommentEntity MapToEntity(CommentEntity e) {
		if (Description != null) e.Description = Description;
		if (Score.HasValue) e.Score = Score.Value;
		if (Tags != null) e.Tags = Tags;
		if (AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(AddTags);
		if (RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => RemoveTags.Contains(x));
		return e;
	}
}

public sealed class CommentReadParams : BaseReadParams<TagComment> {
	public Guid? UserId { get; set; }
	public Guid? ProductId { get; set; }
	public Guid? TargetUserId { get; set; }
	public bool ShowMedia { get; set; }
	public bool ShowUser { get; set; }
	public bool ShowTargetUser { get; set; }
	public bool ShowProduct { get; set; }
	public bool ShowChildren { get; set; }
}