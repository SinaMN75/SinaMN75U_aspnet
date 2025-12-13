namespace SinaMN75U.Data.Params;

public sealed class CommentCreateParams : BaseParams {
	[UValidationRequired("CommentRequired")]
	public required string Description { get; set; }

	public decimal Score { get; set; } = 0;
	public TagReaction? Reaction { get; set; }

	public Guid? ParentId { get; set; }
	public Guid? ProductId { get; set; }
	public Guid? UserId { get; set; }
	public Guid? CreatorId { get; set; }

	[UValidationRequired("TagsRequired")]
	[UValidationMinCollectionLength(1, "TagsRequired")]
	public required List<TagComment> Tags { get; set; }
	
	public CommentEntity MapToEntity() => new() {
		Description = Description,
		Score = Score,
		ParentId = ParentId,
		ProductId = ProductId,
		UserId = UserId,
		CreatorId = CreatorId ?? Guid.Empty,
		JsonData = new CommentJson(),
		Tags = Tags
	};

}

public sealed class CommentUpdateParams : BaseUpdateParams<TagComment> {
	public string? Description { get; set; }
	public decimal? Score { get; set; }
	
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
	public Guid? CreatorId { get; set; }
	public Guid? ProductId { get; set; }
	public Guid? UserId { get; set; }
	public CommentSelectorArgs SelectorArgs { get; set; } = new();
}