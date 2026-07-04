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

	public HotelSelectorArgs SelectorArgs { get; set; } = new();
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

	public HotelRoomSelectorArgs SelectorArgs { get; set; } = new();
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

	public DormSelectorArgs SelectorArgs { get; set; } = new();
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

	public DormRoomSelectorArgs SelectorArgs { get; set; } = new();
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
	public DormBedSelectorArgs SelectorArgs { get; set; } = new();
}

// ---------------- DormBedContract ----------------

public sealed class DormBedContractCreateParams : BaseCreateParams<TagDormBedContract> {
	[UValidationRequired("StartDateRequired")]
	public DateTime StartDate { get; set; }

	[UValidationRequired("EndDateRequired")]
	public DateTime EndDate { get; set; }

	public decimal? Deposit { get; set; }
	public decimal? Rent { get; set; }

	[UValidationRequired("UserIdRequired")]
	public Guid UserId { get; set; }

	[UValidationRequired("BedIdRequired")]
	public Guid BedId { get; set; }

	public int PenaltyPrecentEveryDate { get; set; }
}

public sealed class DormBedContractUpdateParams : BaseUpdateParams<TagDormBedContract> {
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public decimal? Deposit { get; set; }
	public decimal? Rent { get; set; }
}

public sealed class DormBedContractReadParams : BaseReadParams<TagDormBedContract> {
	public Guid? UserId { get; set; }
	public string? UserName { get; set; }
	public Guid? BedId { get; set; }
	public Guid? DormId { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }

	/// <summary>Contract is currently within its start/end date range.</summary>
	public bool? ActiveOnly { get; set; }

	/// <summary>Contract's start date hasn't arrived yet.</summary>
	public bool? UpcomingOnly { get; set; }

	/// <summary>Contract's end date has already passed.</summary>
	public bool? ExpiredOnly { get; set; }

	/// <summary>Contract ends within this many days from now (and hasn't already expired).</summary>
	public int? ExpiringWithinDays { get; set; }

	public DormBedContractSelectorArgs SelectorArgs { get; set; } = new();
}

// ---------------- DormBedInvoice ----------------

public sealed class DormBedInvoiceCreateParams : BaseCreateParams<TagDormBedInvoice> {
	[UValidationRequired("PriceRequired")]
	public decimal DebtAmount { get; set; }

	[UValidationRequired("PriceRequired")]
	public decimal CreditorAmount { get; set; }

	[UValidationRequired("PriceRequired")]
	public decimal PaidAmount { get; set; }

	[UValidationRequired("PriceRequired")]
	public decimal PenaltyAmount { get; set; }

	public int PenaltyPrecentEveryDate { get; set; }

	[UValidationRequired("ContractIdRequired")]
	public Guid ContractId { get; set; }

	[UValidationRequired("DateRequired")]
	public DateTime DueDate { get; set; }
}

public sealed class DormBedInvoiceUpdateParams : BaseUpdateParams<TagDormBedInvoice> {
	public decimal? DebtAmount { get; set; }
	public decimal? CreditorAmount { get; set; }
	public decimal? PaidAmount { get; set; }
	public decimal? PenaltyAmount { get; set; }
	public int? PenaltyPrecentEveryDate { get; set; }
	public DateTime? DueDate { get; set; }
	public Guid? UserId { get; set; }
	public Guid? ContractId { get; set; }
}

public sealed class DormBedInvoiceReadParams : BaseReadParams<TagDormBedInvoice> {
	public DormBedInvoiceSelectorArgs SelectorArgs { get; set; } = new();

	public Guid? ContractId { get; set; }
	public Guid? UserId { get; set; }
	public Guid? DormId { get; set; }

	/// <summary>true = only paid invoices, false = only unpaid invoices.</summary>
	public bool? IsPaid { get; set; }

	/// <summary>true = only unpaid invoices whose due date has passed.</summary>
	public bool? IsOverdue { get; set; }

	public DateTime? MinDueDate { get; set; }
	public DateTime? MaxDueDate { get; set; }
	public decimal? MinDebtAmount { get; set; }
	public decimal? MaxDebtAmount { get; set; }
}
