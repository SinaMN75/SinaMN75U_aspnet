namespace SinaMN75U.Data.Params;

public sealed class MediaCreateParams : BaseCreateParams<TagMedia> {
	public required IFormFile File { get; set; }
	public Guid? UserId { get; set; }
	public Guid? ContentId { get; set; }
	public Guid? CommentId { get; set; }
	public Guid? CategoryId { get; set; }
	public Guid? ProductId { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	
	public MediaEntity MapToEntity(string filePath) => new() {
		Path = filePath,
		UserId = UserId,
		ContentId = ContentId,
		CommentId = CommentId,
		CategoryId = CategoryId,
		ProductId = ProductId,
		JsonData = new MediaJson {
			Title = Title,
			Description = Description
		},
		Tags = Tags
	};

}

public sealed class MediaUpdateParams : BaseParams {
	public required Guid Id { get; set; }
	public IEnumerable<TagMedia>? AddTags { get; set; }
	public IEnumerable<TagMedia>? RemoveTags { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public Guid? UserId { get; set; }
	public Guid? ContentId { get; set; }
	public Guid? CommentId { get; set; }
	public Guid? CategoryId { get; set; }
	public Guid? ProductId { get; set; }
	
	public void MapToEntity(MediaEntity e) {
		if (Title != null) e.JsonData.Title = Title;
		if (Description != null) e.JsonData.Description = Description;

		if (UserId.HasValue) e.UserId = UserId;
		if (ContentId.HasValue) e.ContentId = ContentId;
		if (CommentId.HasValue) e.CommentId = CommentId;
		if (CategoryId.HasValue) e.CategoryId = CategoryId;
		if (ProductId.HasValue) e.ProductId = ProductId;

		if (AddTags != null) foreach (TagMedia t in AddTags) e.Tags.Add(t);
		if (RemoveTags != null) foreach (TagMedia t in RemoveTags) e.Tags.Remove(t);
	}

}