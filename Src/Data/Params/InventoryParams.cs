namespace SinaMN75U.Data.Params;

public class InventoryReadParams<T> : BaseReadParams<T> {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	public string? Title { get; set; }
	public Guid? PlaceId { get; set; }
}

public sealed class WarehouseCreateParams : BaseCreateParams<TagWarehouse> {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	[UValidationRequired("titleIsRequired"), UValidationStringLength(2, 100, "TitleMinLength")]
	public string Title { get; set; } = null!;

	public Guid? PlaceId { get; set; }
	public string? Address { get; set; }
	public Guid? KeeperId { get; set; }
}

public sealed class WarehouseUpdateParams : BaseUpdateParams<TagWarehouse> {
	public string? Title { get; set; }
	public Guid? PlaceId { get; set; }
	public string? Address { get; set; }
	public Guid? KeeperId { get; set; }
}

public sealed class InventoryItemCreateParams : BaseCreateParams<TagInventoryItem> {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	[UValidationRequired("titleIsRequired"), UValidationStringLength(2, 100, "TitleMinLength")]
	public string Title { get; set; } = null!;

	[UValidationRequired("unitIsRequired")]
	public string Unit { get; set; } = null!;

	public string? Code { get; set; }
	public decimal MinStock { get; set; }
	public string? Description { get; set; }
}

public sealed class InventoryItemUpdateParams : BaseUpdateParams<TagInventoryItem> {
	public string? Title { get; set; }
	public string? Unit { get; set; }
	public string? Code { get; set; }
	public decimal? MinStock { get; set; }
	public string? Description { get; set; }
}

public sealed class StockMovementCreateParams : BaseCreateParams<TagStockMovement> {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	[UValidationRequired("idIsRequired")]
	public Guid ItemId { get; set; }

	[UValidationRequired("idIsRequired")]
	public Guid WarehouseId { get; set; }

	public Guid? TargetWarehouseId { get; set; }
	public decimal Quantity { get; set; }
	public decimal UnitPrice { get; set; }
	public DateTime? Date { get; set; }
	public Guid? PlaceId { get; set; }
	public Guid? AccountId { get; set; }
}

public sealed class StockMovementReadParams : BaseReadParams<TagStockMovement> {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	public Guid? ItemId { get; set; }
	public Guid? WarehouseId { get; set; }
	public DateTime? FromDate { get; set; }
	public DateTime? ToDate { get; set; }
}

public sealed class StockReadParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	public Guid? WarehouseId { get; set; }
	public Guid? ItemId { get; set; }
	public bool LowOnly { get; set; }
}

public sealed class SupplierCreateParams : BaseCreateParams<TagSupplier> {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	[UValidationRequired("titleIsRequired"), UValidationStringLength(2, 100, "TitleMinLength")]
	public string Title { get; set; } = null!;

	public string? PhoneNumber { get; set; }
	public string? ContactName { get; set; }
	public string? Address { get; set; }
	public string? NationalId { get; set; }
	public string? Iban { get; set; }
}

public sealed class SupplierUpdateParams : BaseUpdateParams<TagSupplier> {
	public string? Title { get; set; }
	public string? PhoneNumber { get; set; }
	public string? ContactName { get; set; }
	public string? Address { get; set; }
	public string? NationalId { get; set; }
	public string? Iban { get; set; }
}

public sealed class PurchaseLineParams {
	public Guid ItemId { get; set; }
	public decimal Quantity { get; set; }
	public decimal UnitPrice { get; set; }
}

public sealed class PurchaseCreateParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	[UValidationRequired("idIsRequired")]
	public Guid WarehouseId { get; set; }

	public Guid? SupplierId { get; set; }
	public DateTime? Date { get; set; }
	public string Detail1 { get; set; } = "";
	public decimal Vat { get; set; }

	[UValidationMinCollectionLength(1, "itemsAreRequired")]
	public List<PurchaseLineParams> Lines { get; set; } = [];
}

public sealed class PurchaseReadParams : BaseReadParams<TagPurchase> {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	public Guid? SupplierId { get; set; }
	public Guid? WarehouseId { get; set; }
}

public sealed class PurchaseReviewParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid Id { get; set; }

	public bool Approve { get; set; }
	public string? Note { get; set; }
}

public sealed class PurchaseReceiveParams : BaseParams {
	[UValidationRequired("idIsRequired")]
	public Guid Id { get; set; }

	public Guid? AccountId { get; set; }
	public DateTime? Date { get; set; }
}

public sealed class AssetCreateParams : BaseCreateParams<TagAsset> {
	[UValidationRequired("idIsRequired")]
	public Guid OrganizationId { get; set; }

	[UValidationRequired("titleIsRequired"), UValidationStringLength(2, 100, "TitleMinLength")]
	public string Title { get; set; } = null!;

	[UValidationRequired("codeIsRequired")]
	public string Code { get; set; } = null!;

	public Guid? PlaceId { get; set; }
	public string? Location { get; set; }
	public Guid? RoomId { get; set; }
	public Guid? BedId { get; set; }
	public Guid? ItemId { get; set; }
	public string? SerialNumber { get; set; }
	public DateTime? PurchaseDate { get; set; }
	public decimal? Price { get; set; }
	public Guid? AssignedUserId { get; set; }
}

public sealed class AssetUpdateParams : BaseUpdateParams<TagAsset> {
	public string? Title { get; set; }
	public string? Code { get; set; }
	public Guid? PlaceId { get; set; }
	public string? Location { get; set; }
	public Guid? RoomId { get; set; }
	public Guid? BedId { get; set; }
	public string? SerialNumber { get; set; }
	public DateTime? PurchaseDate { get; set; }
	public decimal? Price { get; set; }
	public Guid? AssignedUserId { get; set; }
}
