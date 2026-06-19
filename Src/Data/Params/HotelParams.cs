namespace SinaMN75U.Data.Params;

// ---------------- Hotel ----------------

public sealed class HotelCreateParams : BaseCreateParams<TagHotel> {
	[UValidationRequired("TitleRequired"), UValidationStringLength(2, 100, "TitleMinLength")]
	public string Title { get; set; } = null!;

	[UValidationRequired("CityRequired"), UValidationStringLength(2, 100, "CityMinLength")]
	public string City { get; set; } = null!;

	[UValidationRequired("CountryRequired"), UValidationStringLength(2, 100, "CountryMinLength")]
	public string Country { get; set; } = null!;
}

public sealed class HotelUpdateParams : BaseUpdateParams<TagHotel> {
	public string? Title { get; set; }
	public string? City { get; set; }
	public string? Country { get; set; }
}

public sealed class HotelReadParams : BaseReadParams<TagHotel> {
	public string? Title { get; set; }
	public string? City { get; set; }
	public string? Country { get; set; }
	public bool OrderByTitle { get; set; }
	public bool OrderByTitleDesc { get; set; }
	public bool ShowRooms { get; set; }
}

// ---------------- HotelRoom ----------------

public sealed class HotelRoomCreateParams : BaseCreateParams<TagRoom> {
	[UValidationRequired("TitleRequired"), UValidationStringLength(2, 100, "TitleMinLength")]
	public string Title { get; set; } = null!;

	[UValidationRequired("CapacityRequired")]
	public int Capacity { get; set; }

	[UValidationRequired("PricePerNightRequired")]
	public decimal PricePerNight { get; set; }

	public bool IsAvailable { get; set; }

	[UValidationRequired("HotelIdRequired")]
	public Guid HotelId { get; set; }
}

public sealed class HotelRoomUpdateParams : BaseUpdateParams<TagRoom> {
	public string? Title { get; set; }
	public int? Capacity { get; set; }
	public decimal? PricePerNight { get; set; }
	public bool? IsAvailable { get; set; }
	public Guid? HotelId { get; set; }
}

public sealed class HotelRoomReadParams : BaseReadParams<TagRoom> {
	public string? Title { get; set; }
	public Guid? HotelId { get; set; }
	public bool? IsAvailable { get; set; }
	public int? MinCapacity { get; set; }
	public int? MaxCapacity { get; set; }
	public decimal? MinPrice { get; set; }
	public decimal? MaxPrice { get; set; }
	public bool OrderByPrice { get; set; }
	public bool OrderByPriceDesc { get; set; }
}

// ---------------- Dorm ----------------

public sealed class DormCreateParams : BaseCreateParams<TagDorm> {
	[UValidationRequired("TitleRequired"), UValidationStringLength(2, 100, "TitleMinLength")]
	public string Title { get; set; } = null!;

	[UValidationRequired("CityRequired"), UValidationStringLength(2, 100, "CityMinLength")]
	public string City { get; set; } = null!;

	[UValidationRequired("CountryRequired"), UValidationStringLength(2, 100, "CountryMinLength")]
	public string Country { get; set; } = null!;
}

public sealed class DormUpdateParams : BaseUpdateParams<TagDorm> {
	public string? Title { get; set; }
	public string? City { get; set; }
	public string? Country { get; set; }
}

public sealed class DormReadParams : BaseReadParams<TagDorm> {
	public string? Title { get; set; }
	public string? City { get; set; }
	public string? Country { get; set; }
	public bool OrderByTitle { get; set; }
	public bool OrderByTitleDesc { get; set; }
	public bool ShowRooms { get; set; }
}

// ---------------- DormRoom ----------------

public sealed class DormRoomCreateParams : BaseCreateParams<TagDormRoom> {
	[UValidationRequired("TitleRequired"), UValidationStringLength(2, 100, "TitleMinLength")]
	public string Title { get; set; } = null!;

	[UValidationRequired("DormIdRequired")]
	public Guid DormId { get; set; }
}

public sealed class DormRoomUpdateParams : BaseUpdateParams<TagDormRoom> {
	public string? Title { get; set; }
	public Guid? DormId { get; set; }
}

public sealed class DormRoomReadParams : BaseReadParams<TagDormRoom> {
	public string? Title { get; set; }
	public Guid? DormId { get; set; }
	public bool ShowBeds { get; set; }
	public bool OrderByTitle { get; set; }
	public bool OrderByTitleDesc { get; set; }
}

// ---------------- DormBed ----------------

public sealed class DormBedCreateParams : BaseCreateParams<TagDormBed> {
	[UValidationRequired("TitleRequired"), UValidationStringLength(1, 4, "TitleMaxLength")]
	public string Title { get; set; } = null!;

	public bool IsAvailable { get; set; }

	[UValidationRequired("DepositRequired")]
	public decimal Deposit { get; set; }

	[UValidationRequired("MonthlyRentRequired")]
	public decimal MonthlyRent { get; set; }

	[UValidationRequired("RoomIdRequired")]
	public Guid RoomId { get; set; }
}

public sealed class DormBedUpdateParams : BaseUpdateParams<TagDormBed> {
	public string? Title { get; set; }
	public bool? IsAvailable { get; set; }
	public decimal? Deposit { get; set; }
	public decimal? MonthlyRent { get; set; }
	public Guid? RoomId { get; set; }
}

public sealed class DormBedReadParams : BaseReadParams<TagDormBed> {
	public string? Title { get; set; }
	public Guid? RoomId { get; set; }
	public bool? IsAvailable { get; set; }
	public decimal? MinDeposit { get; set; }
	public decimal? MaxDeposit { get; set; }
	public decimal? MinMonthlyRent { get; set; }
	public decimal? MaxMonthlyRent { get; set; }
	public bool OrderByMonthlyRent { get; set; }
	public bool OrderByMonthlyRentDesc { get; set; }
}
