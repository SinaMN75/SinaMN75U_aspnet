namespace SinaMN75U.Data.Entities;

[Table("Warehouses")]
public sealed class WarehouseEntity : BaseEntity<TagWarehouse, WarehouseJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	public Guid? PlaceId { get; set; }

	public required Guid OrganizationId { get; set; }
	public OrganizationEntity Organization { get; set; } = null!;
}

public sealed class WarehouseJson : BaseJson {
	public string? Address { get; set; }
	public Guid? KeeperId { get; set; }
}

[Table("InventoryItems")]
public sealed class InventoryItemEntity : BaseEntity<TagInventoryItem, InventoryItemJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	[MaxLength(30)]
	public string? Code { get; set; }

	[Required, MaxLength(20)]
	public required string Unit { get; set; }

	[Column(TypeName = "decimal(24,3)")]
	public decimal MinStock { get; set; }

	public required Guid OrganizationId { get; set; }
	public OrganizationEntity Organization { get; set; } = null!;
}

public sealed class InventoryItemJson : BaseJson {
	public string? Description { get; set; }
	public bool LowStockNotified { get; set; }
}

[Table("StockMovements")]
public sealed class StockMovementEntity : BaseEntity<TagStockMovement, StockMovementJson> {
	[Column(TypeName = "decimal(24,3)")]
	public required decimal Quantity { get; set; }

	[Column(TypeName = "decimal(24,2)")]
	public required decimal UnitPrice { get; set; }

	public required DateTime Date { get; set; }

	public required Guid ItemId { get; set; }
	public InventoryItemEntity Item { get; set; } = null!;

	public required Guid WarehouseId { get; set; }
	public WarehouseEntity Warehouse { get; set; } = null!;

	public required Guid OrganizationId { get; set; }
	public OrganizationEntity Organization { get; set; } = null!;
}

public sealed class StockMovementJson : BaseJson {
	public Guid? PurchaseId { get; set; }
	public Guid? TransferId { get; set; }
	public Guid? PlaceId { get; set; }
	public Guid? AccountId { get; set; }
	public Guid? RegisteredBy { get; set; }
}

[Table("Suppliers")]
public sealed class SupplierEntity : BaseEntity<TagSupplier, SupplierJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	[MaxLength(20)]
	public string? PhoneNumber { get; set; }

	public required Guid OrganizationId { get; set; }
	public OrganizationEntity Organization { get; set; } = null!;
}

public sealed class SupplierJson : BaseJson {
	public string? ContactName { get; set; }
	public string? Address { get; set; }
	public string? NationalId { get; set; }
	public string? Iban { get; set; }
}

[Table("Purchases")]
public sealed class PurchaseEntity : BaseEntity<TagPurchase, PurchaseJson> {
	public required int Number { get; set; }
	public required DateTime Date { get; set; }

	[Column(TypeName = "decimal(24,2)")]
	public required decimal Total { get; set; }

	public Guid? SupplierId { get; set; }

	public required Guid WarehouseId { get; set; }
	public WarehouseEntity Warehouse { get; set; } = null!;

	public required Guid OrganizationId { get; set; }
	public OrganizationEntity Organization { get; set; } = null!;
}

public sealed class PurchaseJson : BaseJson {
	public List<PurchaseLine> Lines { get; set; } = [];
	public Guid? RequestedBy { get; set; }
	public Guid? ReviewedBy { get; set; }
	public DateTime? ReviewedAt { get; set; }
	public string? ReviewNote { get; set; }
	public DateTime? ReceivedAt { get; set; }
	public Guid? PaidFromAccountId { get; set; }
	public decimal Vat { get; set; }
}

public sealed class PurchaseLine {
	public Guid ItemId { get; set; }
	public decimal Quantity { get; set; }
	public decimal UnitPrice { get; set; }
}

[Table("Assets")]
public sealed class AssetEntity : BaseEntity<TagAsset, AssetJson> {
	[Required, MaxLength(100)]
	public required string Title { get; set; }

	[Required, MaxLength(50)]
	public required string Code { get; set; }

	public Guid? PlaceId { get; set; }

	public required Guid OrganizationId { get; set; }
	public OrganizationEntity Organization { get; set; } = null!;
}

public sealed class AssetJson : BaseJson {
	public string? Location { get; set; }
	public Guid? RoomId { get; set; }
	public Guid? BedId { get; set; }
	public Guid? ItemId { get; set; }
	public string? SerialNumber { get; set; }
	public DateTime? PurchaseDate { get; set; }
	public decimal? Price { get; set; }
	public Guid? AssignedUserId { get; set; }
}
