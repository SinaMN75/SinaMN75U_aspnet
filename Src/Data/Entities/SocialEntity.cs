namespace SinaMN75U.Data.Entities;

// ---------------- Post ----------------

/// <summary>A post, a story (expires) or a reply to a post (ParentId).</summary>
[Table("Posts")]
[Microsoft.EntityFrameworkCore.Index(nameof(ParentId), Name = "IX_Posts_ParentId")]
public sealed class PostEntity : BaseEntity<TagPost, PostJson> {
	[MaxLength(4000)]
	public string? Text { get; set; }

	public DateTime? ExpiresAt { get; set; } // stories

	public Guid? ParentId { get; set; }
	public PostEntity? Parent { get; set; }

	[InverseProperty(nameof(Parent))]
	public ICollection<PostEntity> Children { get; set; } = [];

	public ICollection<MediaEntity> Media { get; set; } = [];
}

public sealed class PostJson : BaseJson {
	public List<PostReaction> Reactions { get; set; } = [];
	public List<Guid> ViewerIds { get; set; } = []; // stories
	public string? LinkType { get; set; } // "tournament", "openMatch", "achievement", "venue"...
	public Guid? LinkId { get; set; }
}

public sealed class PostReaction {
	public required Guid UserId { get; set; }
	public required TagReaction Tag { get; set; }
}

// ---------------- Report ----------------

[Table("Reports")]
public sealed class ReportEntity : BaseEntity<TagReport, ReportJson> {
	public required Guid TargetId { get; set; }

	[MaxLength(1000)]
	public string? Reason { get; set; }
}

public sealed class ReportJson : BaseJson {
	public string? Note { get; set; } // the moderator's
}

// ---------------- Block ----------------

[Table("Blocks")]
[Microsoft.EntityFrameworkCore.Index(nameof(CreatorId), nameof(BlockedUserId), IsUnique = true, Name = "IX_Blocks_CreatorId_BlockedUserId")]
public sealed class BlockEntity : BaseEntity<TagBlock, BlockJson> {
	public required Guid BlockedUserId { get; set; }
	public UserEntity BlockedUser { get; set; } = null!;
}

public sealed class BlockJson : BaseJson;
