namespace SinaMN75U.Data.Responses;

public sealed class WarehouseResponse : BaseResponse<TagWarehouse, WarehouseJson> {
	public required string Title { get; set; }
	public Guid? PlaceId { get; set; }
	public Guid OrganizationId { get; set; }
}

public sealed class InventoryItemResponse : BaseResponse<TagInventoryItem, InventoryItemJson> {
	public required string Title { get; set; }
	public string? Code { get; set; }
	public required string Unit { get; set; }
	public decimal MinStock { get; set; }
	public decimal Stock { get; set; }
	public Guid OrganizationId { get; set; }
}

public sealed class StockMovementResponse : BaseResponse<TagStockMovement, StockMovementJson> {
	public decimal Quantity { get; set; }
	public decimal UnitPrice { get; set; }
	public DateTime Date { get; set; }
	public Guid ItemId { get; set; }
	public string? ItemTitle { get; set; }
	public string? Unit { get; set; }
	public Guid WarehouseId { get; set; }
	public string? WarehouseTitle { get; set; }
}

public sealed class StockResponse {
	public Guid ItemId { get; set; }
	public required string ItemTitle { get; set; }
	public string? Code { get; set; }
	public required string Unit { get; set; }
	public Guid WarehouseId { get; set; }
	public required string WarehouseTitle { get; set; }
	public decimal Quantity { get; set; }
	public decimal MinStock { get; set; }
	public decimal AverageCost { get; set; }
	public decimal Value { get; set; }
	public bool Low { get; set; }
}

public sealed class SupplierResponse : BaseResponse<TagSupplier, SupplierJson> {
	public required string Title { get; set; }
	public string? PhoneNumber { get; set; }
	public Guid OrganizationId { get; set; }
}

public sealed class PurchaseResponse : BaseResponse<TagPurchase, PurchaseJson> {
	public int Number { get; set; }
	public DateTime Date { get; set; }
	public decimal Total { get; set; }
	public Guid? SupplierId { get; set; }
	public string? SupplierTitle { get; set; }
	public Guid WarehouseId { get; set; }
	public string? WarehouseTitle { get; set; }
	public Guid OrganizationId { get; set; }
}

public sealed class AssetResponse : BaseResponse<TagAsset, AssetJson> {
	public required string Title { get; set; }
	public required string Code { get; set; }
	public Guid? PlaceId { get; set; }
	public Guid OrganizationId { get; set; }
}
