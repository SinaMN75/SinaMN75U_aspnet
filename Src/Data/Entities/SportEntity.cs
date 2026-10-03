namespace SinaMN75U.Data.Entities;

// ---------------- Sport ----------------

[Table("Sports")]
public sealed class SportEntity : BaseEntity<TagSport, SportJson> {
	// The sport type is a 1xx tag (TagSport.Padel, ...), one sport per type.

	[Required, MaxLength(100)]
	public required string Title { get; set; } // a ULocalizedConstants key, e.g. ULocalizedConstants.Padel; clients translate it

	public int Order { get; set; }

	[Column(TypeName = "decimal(4,2)")]
	public decimal MinLevel { get; set; } = 1;

	[Column(TypeName = "decimal(4,2)")]
	public decimal MaxLevel { get; set; } = 7;

	public ICollection<PlayerSportProfileEntity> PlayerProfiles { get; set; } = [];
}

public sealed class SportJson : BaseJson {
	public string? Icon { get; set; }
}

// ---------------- PlayerSportProfile ----------------

[Table("PlayerSportProfiles")]
[Microsoft.EntityFrameworkCore.Index(nameof(UserId), nameof(SportId), IsUnique = true, Name = "IX_PlayerSportProfiles_UserId_SportId")]
public sealed class PlayerSportProfileEntity : BaseEntity<TagPlayerSportProfile, PlayerSportProfileJson> {
	[Column(TypeName = "decimal(4,2)")]
	public required decimal Level { get; set; }

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	public required Guid SportId { get; set; }
	public SportEntity Sport { get; set; } = null!;
}

public sealed class PlayerSportProfileJson : BaseJson;
