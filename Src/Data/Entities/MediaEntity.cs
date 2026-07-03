namespace SinaMN75U.Data.Entities;

[Table("Media")]
[Microsoft.EntityFrameworkCore.Index(nameof(UserId), Name = "IX_Media_UserId")]
[Microsoft.EntityFrameworkCore.Index(nameof(ContentId), Name = "IX_Media_ContentId")]
[Microsoft.EntityFrameworkCore.Index(nameof(CategoryId), Name = "IX_Media_CategoryId")]
[Microsoft.EntityFrameworkCore.Index(nameof(CommentId), Name = "IX_Media_CommentId")]
[Microsoft.EntityFrameworkCore.Index(nameof(ProductId), Name = "IX_Media_ProductId")]
[Microsoft.EntityFrameworkCore.Index(nameof(TicketId), Name = "IX_Media_TicketId")]
[Microsoft.EntityFrameworkCore.Index(nameof(BlogId), Name = "IX_Media_BlogId")]
public sealed class MediaEntity : BaseEntity<TagMedia, BaseJson> {
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

	public Guid? HotelId { get; set; }
	public HotelEntity? Hotel { get; set; }
	
	public Guid? HotelRoomId { get; set; }
	public HotelRoomEntity? HotelRoom { get; set; }
	
	public Guid? DormId { get; set; }
	public DormEntity? Dorm { get; set; }
	
	public Guid? DormRoomId { get; set; }
	public DormRoomEntity? DormRoom { get; set; }
	
	public Guid? DormBedId { get; set; }
	public DormBedEntity? DormBed { get; set; }
	
	public Guid? TicketId { get; set; }
	public TicketEntity? Ticket { get; set; }

	public Guid? BlogId { get; set; }
	public BlogEntity? Blog { get; set; }
}
