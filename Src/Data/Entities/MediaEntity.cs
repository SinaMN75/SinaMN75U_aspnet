namespace SinaMN75U.Data.Entities;

[Table("Media")]
[Index(nameof(UserId), Name = "IX_Media_UserId")]
[Index(nameof(ContentId), Name = "IX_Media_ContentId")]
[Index(nameof(CategoryId), Name = "IX_Media_CategoryId")]
[Index(nameof(CommentId), Name = "IX_Media_CommentId")]
[Index(nameof(ProductId), Name = "IX_Media_ProductId")]
[Index(nameof(TicketId), Name = "IX_Media_TicketId")]
public sealed class MediaEntity : BaseEntity<TagMedia, BaseJsonData> {
	[Required]
	[MaxLength(200)]
	public required string Path { get; set; }

	public Guid? UserId { get; set; }
	public UserEntity? User { get; set; }

	public Guid? ContentId { get; set; }
	public ContentEntity? Content { get; set; }

	public Guid? CategoryId { get; set; }
	public CategoryEntity? Category { get; set; }

	public Guid? CommentId { get; set; }
	public CommentEntity? Comment { get; set; }

	public Guid? ProductId { get; set; }
	public ProductEntity? Product { get; set; }

	public Guid? TicketId { get; set; }
	public TicketEntity? Ticket { get; set; }

	public MediaResponse MapToResponse() => new() {
		Id = Id,
		Path = Path,
		JsonData = JsonData,
		Tags = Tags
	};
}
