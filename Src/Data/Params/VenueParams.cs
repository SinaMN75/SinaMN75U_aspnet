namespace SinaMN75U.Data.Params;

public sealed class VenueCreateParams : BaseCreateParams<TagVenue> {
	[UValidationRequired("titleIsRequired"), UValidationStringLength(2, 200, "titleIsRequired")]
	public string Title { get; set; } = null!;

	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Country { get; set; }
	public string? City { get; set; }
	public string? Description { get; set; }
	public string? Website { get; set; }
	public string? Instagram { get; set; }
	public string? Whatsapp { get; set; }
	public string? TimeZone { get; set; }
	public string? Currency { get; set; }
	public List<string>? Amenities { get; set; }
	public List<VenueOpeningHour>? OpeningHours { get; set; }
	public int? CancellationFreeHours { get; set; }
	public int? CancellationPenaltyPercent { get; set; }
}

public sealed class VenueUpdateParams : BaseUpdateParams<TagVenue> {
	public string? Title { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Country { get; set; }
	public string? City { get; set; }
	public string? Description { get; set; }
	public string? Website { get; set; }
	public string? Instagram { get; set; }
	public string? Whatsapp { get; set; }
	public string? TimeZone { get; set; }
	public string? Currency { get; set; }
	public List<string>? Amenities { get; set; }
	public List<VenueOpeningHour>? OpeningHours { get; set; }
	public List<VenueClosure>? Closures { get; set; }
	public int? CancellationFreeHours { get; set; }
	public int? CancellationPenaltyPercent { get; set; }

	/// <summary>Admins: why the venue was rejected.</summary>
	public string? RejectionReason { get; set; }
}

public sealed class VenueReadParams : BaseReadParams<TagVenue> {
	public string? Title { get; set; }

	/// <summary>Venues with a court for this sport.</summary>
	public Guid? SportId { get; set; }

	/// <summary>Venues the signed-in user owns or works at (any status).</summary>
	public bool Mine { get; set; }

	public string? Country { get; set; }
	public string? City { get; set; }

	/// <summary>Nearest first; with RadiusKm, only venues inside it.</summary>
	public double? Latitude { get; set; }

	public double? Longitude { get; set; }
	public double? RadiusKm { get; set; }
	public VenueSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class VenueStatsParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid Id { get; set; }

	public DateTime? From { get; set; }
	public DateTime? To { get; set; }
}

public sealed class CourtCreateParams : BaseCreateParams<TagCourt> {
	[UValidationRequired("titleIsRequired"), UValidationStringLength(1, 100, "titleIsRequired")]
	public string Title { get; set; } = null!;

	[UValidationRequired("idIsRequired")]
	public Guid VenueId { get; set; }

	public Guid? SportId { get; set; }
	public decimal PricePerHour { get; set; }
	public int SlotMinutes { get; set; } = 60;
	public string? Description { get; set; }
	public string? Surface { get; set; }
	public int? Players { get; set; }
	public List<CourtPriceRule>? PriceRules { get; set; }
}

public sealed class CourtUpdateParams : BaseUpdateParams<TagCourt> {
	public string? Title { get; set; }
	public Guid? SportId { get; set; }
	public decimal? PricePerHour { get; set; }
	public int? SlotMinutes { get; set; }
	public string? Description { get; set; }
	public string? Surface { get; set; }
	public int? Players { get; set; }
	public List<CourtPriceRule>? PriceRules { get; set; }
}

public sealed class CourtReadParams : BaseReadParams<TagCourt> {
	public Guid? VenueId { get; set; }
	public Guid? SportId { get; set; }
	public CourtSelectorArgs SelectorArgs { get; set; } = new();
}

public sealed class CourtAvailabilityParams : BaseParams {
	/// <summary>One court, or every active court of the venue.</summary>
	public Guid? CourtId { get; set; }

	public Guid? VenueId { get; set; }
	public Guid? SportId { get; set; }

	/// <summary>The day, in the venue's time zone (only the date part is used).</summary>
	[UValidationRequired("dateIsRequired")]
	public DateTime Date { get; set; }
}

public sealed class BookingCreateParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid CourtId { get; set; }

	[UValidationRequired("startDateIsRequired")]
	public DateTime StartAt { get; set; }

	public int DurationMinutes { get; set; } = 60;

	/// <summary>Pays the booker's share from the wallet; otherwise it is paid at the venue (if the venue allows it).</summary>
	public bool PayFromWallet { get; set; } = true;

	/// <summary>Players who split the price with the booker; each pays their share.</summary>
	public List<Guid> SplitWithUserIds { get; set; } = [];

	public string? Notes { get; set; }
}

public sealed class BookingReadParams : BaseReadParams<TagBooking> {
	/// <summary>Bookings the signed-in user made or shares.</summary>
	public bool Mine { get; set; }

	public Guid? VenueId { get; set; }
	public Guid? CourtId { get; set; }
	public DateTime? From { get; set; }
	public DateTime? To { get; set; }
	public BookingSelectorArgs SelectorArgs { get; set; } = new();
}

/// <summary>Staff mark a booking Completed or NoShow (pays the venue out), or add notes.</summary>
public sealed class BookingUpdateParams : BaseUpdateParams<TagBooking> {
	public string? Notes { get; set; }
}

public sealed class BookingCancelParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid Id { get; set; }

	public string? Reason { get; set; }
}
