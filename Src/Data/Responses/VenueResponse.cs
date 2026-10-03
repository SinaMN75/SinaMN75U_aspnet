namespace SinaMN75U.Data.Responses;

public sealed class VenueResponse : BaseResponse<TagVenue, VenueJson> {
	public required string Title { get; set; }
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public string? Address { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Country { get; set; }
	public string? City { get; set; }
	public double? DistanceKm { get; set; } // when read near a point
	public decimal? Rating { get; set; } // the average review score
	public int ReviewCount { get; set; }
	public ICollection<Guid> SportIds { get; set; } = []; // of its active courts

	public ICollection<CourtResponse>? Courts { get; set; }
	public ICollection<MediaResponse>? Media { get; set; }
}

public sealed class CourtResponse : BaseResponse<TagCourt, CourtJson> {
	public required string Title { get; set; }
	public decimal PricePerHour { get; set; }
	public int SlotMinutes { get; set; }
	public required Guid VenueId { get; set; }
	public Guid? SportId { get; set; }

	public VenueResponse? Venue { get; set; }
	public SportResponse? Sport { get; set; }
}

public sealed class CourtSlotResponse {
	public required DateTime StartAt { get; set; }
	public required DateTime EndAt { get; set; }
	public required decimal Price { get; set; }
	public required bool Available { get; set; }
}

public sealed class CourtAvailabilityResponse {
	public required CourtResponse Court { get; set; }
	public List<CourtSlotResponse> Slots { get; set; } = [];
}

public sealed class BookingResponse : BaseResponse<TagBooking, BookingJson> {
	public required DateTime StartAt { get; set; }
	public required DateTime EndAt { get; set; }
	public decimal Price { get; set; }
	public required Guid UserId { get; set; }
	public ICollection<Guid> ParticipantIds { get; set; } = [];
	public required Guid CourtId { get; set; }
	public required Guid VenueId { get; set; }

	public UserResponse? User { get; set; }
	public CourtResponse? Court { get; set; }
	public VenueResponse? Venue { get; set; }
}

public sealed class VenueStatsResponse {
	public int Bookings { get; set; }
	public int Upcoming { get; set; }
	public int Completed { get; set; }
	public int Cancelled { get; set; }
	public decimal Revenue { get; set; } // completed and no-show bookings
	public int OccupancyPercent { get; set; } // booked hours of the open hours
}
