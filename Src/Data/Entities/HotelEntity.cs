namespace SinaMN75U.Data.Entities;

[Table("Hotels")]
public class HotelEntity : BaseEntity<TagHotel, BaseJson> {
    [Required, MaxLength(100)]
    public required string Title { get; set; }
    
    [Required, MaxLength(100)]
    public required string City { get; set; }
    
    [Required, MaxLength(100)]
    public required string Country { get; set; }
    
    public ICollection<HotelRoomEntity> Rooms { get; set; } = [];
    public ICollection<MediaEntity> Media { get; set; } = [];
}

[Table("HotelRooms")]
public class HotelRoomEntity : BaseEntity<TagRoom, BaseJson> {
    [Required, MaxLength(100)]
    public required string Title { get; set; }
    
    public required int Capacity { get; set; }
    
    [Required, Column(TypeName = "decimal(24,2)")]
    public decimal PricePerNight { get; set; }
    
    public required bool IsAvailable { get; set; }
    
    public required Guid HotelId { get; set; }
    public HotelEntity HotelEntity { get; set; } = null!;
    
    public ICollection<MediaEntity> Media { get; set; } = [];
}

[Table("Dorms")]
public class DormEntity : BaseEntity<TagDorm, BaseJson> {
    [Required, MaxLength(100)]
    public required string Title { get; set; }
    
    [Required, MaxLength(100)]
    public required string City { get; set; }
    
    [Required, MaxLength(100)]
    public required string Country { get; set; }
    
    public ICollection<DormRoomEntity> Rooms { get; set; } = [];
    public ICollection<MediaEntity> Media { get; set; } = [];
}

[Table("DormRooms")]
public class DormRoomEntity : BaseEntity<TagDormRoom, BaseJson> {
    [Required, MaxLength(100)]
    public required string Title { get; set; }
    
    public required Guid DormId { get; set; }
    public DormEntity DormEntity { get; set; } = null!;
    
    public ICollection<DormBedEntity> Beds { get; set; } = [];
    public ICollection<MediaEntity> Media { get; set; } = [];
}

[Table("DormBeds")]
public class DormBedEntity : BaseEntity<TagDormBed, BaseJson> {
    [Required, MaxLength(4)]
    public required string Title { get; set; }
    
    public required bool IsAvailable { get; set; }
    
    [Required, Column(TypeName = "decimal(24,2)")]
    public required decimal Deposit { get; set; }
    
    [Required, Column(TypeName = "decimal(24,2)")]
    public required decimal MonthlyRent { get; set; }
    
    public required Guid RoomId { get; set; }
    public DormRoomEntity RoomEntity { get; set; } = null!;
    
    public ICollection<MediaEntity> Media { get; set; } = [];
    public ICollection<ContractEntity> Contracts { get; set; } = [];
}