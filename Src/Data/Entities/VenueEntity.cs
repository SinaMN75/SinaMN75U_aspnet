namespace SinaMN75U.Data.Entities;

// ---------------- Venue ----------------

/// <summary>A club with courts or a shop, shown on the map. The creator owns it; AdminUserIds are its staff.</summary>
[Table("Venues")]
[Microsoft.EntityFrameworkCore.Index(nameof(Latitude), nameof(Longitude), Name = "IX_Venues_Latitude_Longitude")]
public sealed class VenueEntity : BaseEntity<TagVenue, VenueJson> {
	[Required, MaxLength(200)]
	public required string Title { get; set; }

	public required double Latitude { get; set; }
	public required double Longitude { get; set; }

	[MaxLength(500)]
	public string? Address { get; set; }

	[MaxLength(20)]
	public string? PhoneNumber { get; set; }

	[MaxLength(2)]
	public string? Country { get; set; } // ISO 3166-1 alpha-2

	[MaxLength(100)]
	public string? City { get; set; }

	public ICollection<CourtEntity> Courts { get; set; } = [];
	public ICollection<BookingEntity> Bookings { get; set; } = [];
	public ICollection<MediaEntity> Media { get; set; } = [];
	public ICollection<CommentEntity> Comments { get; set; } = [];
}

public sealed class VenueJson : BaseJson {
	public string? Description { get; set; }
	public string? Website { get; set; }
	public string? Instagram { get; set; }
	public string? Whatsapp { get; set; }
	public string TimeZone { get; set; } = "UTC"; // IANA id; opening hours are in this zone
	public string? Currency { get; set; }
	public List<string> Amenities { get; set; } = [];
	public List<VenueOpeningHour> OpeningHours { get; set; } = []; // empty = open all day
	public List<VenueClosure> Closures { get; set; } = [];
	public int CancellationFreeHours { get; set; } = 24;
	public int CancellationPenaltyPercent { get; set; } = 100; // of the price, when cancelled later than that
	public string? RejectionReason { get; set; }
}

public sealed class VenueOpeningHour {
	public required int Day { get; set; } // 0 = Sunday ... 6 = Saturday
	public required string Open { get; set; } // "08:00"
	public required string Close { get; set; } // "23:00"; "24:00" = midnight
}

public sealed class VenueClosure {
	public required DateTime From { get; set; }
	public required DateTime To { get; set; }
	public string? Reason { get; set; }
}

// ---------------- Court ----------------

[Table("Courts")]
[Microsoft.EntityFrameworkCore.Index(nameof(VenueId), Name = "IX_Courts_VenueId")]
public sealed class CourtEntity : BaseEntity<TagCourt, CourtJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	[Column(TypeName = "decimal(18,2)")]
	public required decimal PricePerHour { get; set; }

	public int SlotMinutes { get; set; } = 60;

	public required Guid VenueId { get; set; }
	public VenueEntity Venue { get; set; } = null!;

	public Guid? SportId { get; set; }
	public SportEntity? Sport { get; set; }

	public ICollection<BookingEntity> Bookings { get; set; } = [];
}

public sealed class CourtJson : BaseJson {
	public string? Description { get; set; }
	public string? Surface { get; set; }
	public int Players { get; set; } = 4;
	public List<CourtPriceRule> PriceRules { get; set; } = [];
}

/// <summary>A different hourly price on some days and hours (venue time), e.g. evenings and weekends.</summary>
public sealed class CourtPriceRule {
	public List<int> Days { get; set; } = []; // empty = every day
	public required string From { get; set; }
	public required string To { get; set; }
	public required decimal PricePerHour { get; set; }
}

// ---------------- Booking ----------------

[Table("Bookings")]
[Microsoft.EntityFrameworkCore.Index(nameof(CourtId), nameof(StartAt), Name = "IX_Bookings_CourtId_StartAt")]
[Microsoft.EntityFrameworkCore.Index(nameof(UserId), Name = "IX_Bookings_UserId")]
public sealed class BookingEntity : BaseEntity<TagBooking, BookingJson> {
	public required DateTime StartAt { get; set; }
	public required DateTime EndAt { get; set; }

	[Column(TypeName = "decimal(18,2)")]
	public required decimal Price { get; set; }

	public required Guid UserId { get; set; }
	public UserEntity User { get; set; } = null!;

	public ICollection<Guid> ParticipantIds { get; set; } = []; // the booker and everyone sharing the price

	public required Guid CourtId { get; set; }
	public CourtEntity Court { get; set; } = null!;

	public required Guid VenueId { get; set; }
	public VenueEntity Venue { get; set; } = null!;
}

public sealed class BookingJson : BaseJson {
	public string? Code { get; set; }
	public string? Notes { get; set; }
	public List<BookingParticipant> Participants { get; set; } = []; // who shares the price
	public decimal PaidAmount { get; set; } // held by the platform until the booking is completed
	public decimal Penalty { get; set; }
	public decimal RefundAmount { get; set; }
	public string? CancelReason { get; set; }
	public bool Settled { get; set; } // paid out to the venue owner
	public bool Reminded { get; set; }
}

public sealed class BookingParticipant {
	public required Guid UserId { get; set; }
	public required decimal Share { get; set; }
	public bool Paid { get; set; }
}
