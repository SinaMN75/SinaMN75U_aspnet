namespace SinaMN75U.Data.Entities;

[Table("Media")]
[Index(nameof(UserId), Name = "IX_Media_UserId")]
[Index(nameof(ContentId), Name = "IX_Media_ContentId")]
[Index(nameof(CategoryId), Name = "IX_Media_CategoryId")]
[Index(nameof(CommentId), Name = "IX_Media_CommentId")]
[Index(nameof(ProductId), Name = "IX_Media_ProductId")]
[Index(nameof(TicketId), Name = "IX_Media_TicketId")]
public class MediaEntity : BaseEntity<TagMedia, MediaJson> {
	[Required]
	[MaxLength(200)]
	public required string Path { get; set; }

	[ForeignKey("FK_Media_UserId")]
	public Guid? UserId { get; set; }
	public UserEntity? User { get; set; }

	[ForeignKey("FK_Media_ContentId")]
	public Guid? ContentId { get; set; }
	public ContentEntity? Content { get; set; }

	[ForeignKey("FK_Media_CategoryId")]
	public Guid? CategoryId { get; set; }
	public CategoryEntity? Category { get; set; }

	[ForeignKey("FK_Media_CommentId")]
	public Guid? CommentId { get; set; }
	public CommentEntity? Comment { get; set; }

	[ForeignKey("FK_Media_ProductId")]
	public Guid? ProductId { get; set; }
	public ProductEntity? Product { get; set; }

	[ForeignKey("FK_Media_TicketId")]
	public Guid? TicketId { get; set; }
	public TicketEntity? Ticket { get; set; }

	public MediaResponse MapToResponse() => new() {
		Id = Id,
		Path = Path,
		JsonData = JsonData,
		Tags = Tags
	};
}

public class MediaJson {
	public string? Title { get; set; }
	public string? Description { get; set; }
}