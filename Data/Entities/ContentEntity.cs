using System.ComponentModel.DataAnnotations.Schema;

namespace SinaMN75U.Data.Entities;

[Table("Contents")]
public class ContentEntity : BaseEntity {
	[Required]
	[MaxLength(100)]
	public required string Title { get; set; }

	[Required]
	[MaxLength(100)]
	public required string SubTitle { get; set; }

	[Required]
	[MaxLength(5000)]
	public required string Description { get; set; }

	[MaxLength(100)]
	public required List<int> Tags { get; set; }

	[Required]
	public required ContentJsonDetail JsonDetail { get; set; }

	public IEnumerable<MediaEntity>? Media { get; set; }
}

public class ContentJsonDetail {
	public string? Instagram { get; set; }
}
