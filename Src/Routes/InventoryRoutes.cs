namespace SinaMN75U.Routes;

public static class InventoryRoutes {
	public static void MapInventoryRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>().AddEndpointFilter<ActivityLogFilter>();

		r.MapPost("Warehouse/Create", async (WarehouseCreateParams p, IInventoryService s, CancellationToken c) => (await s.CreateWarehouse(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Warehouse/Read", async (InventoryReadParams<TagWarehouse> p, IInventoryService s, CancellationToken c) => (await s.ReadWarehouses(p, c)).ToResult()).Produces<UResponse<IEnumerable<WarehouseResponse>>>();
		r.MapPost("Warehouse/Update", async (WarehouseUpdateParams p, IInventoryService s, CancellationToken c) => (await s.UpdateWarehouse(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Warehouse/Delete", async (IdParams p, IInventoryService s, CancellationToken c) => (await s.DeleteWarehouse(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Item/Create", async (InventoryItemCreateParams p, IInventoryService s, CancellationToken c) => (await s.CreateItem(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Item/Read", async (InventoryReadParams<TagInventoryItem> p, IInventoryService s, CancellationToken c) => (await s.ReadItems(p, c)).ToResult()).Produces<UResponse<IEnumerable<InventoryItemResponse>>>();
		r.MapPost("Item/Update", async (InventoryItemUpdateParams p, IInventoryService s, CancellationToken c) => (await s.UpdateItem(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Item/Delete", async (IdParams p, IInventoryService s, CancellationToken c) => (await s.DeleteItem(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Movement/Create", async (StockMovementCreateParams p, IInventoryService s, CancellationToken c) => (await s.CreateMovement(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Movement/Read", async (StockMovementReadParams p, IInventoryService s, CancellationToken c) => (await s.ReadMovements(p, c)).ToResult()).Produces<UResponse<IEnumerable<StockMovementResponse>>>();
		r.MapPost("Stock/Read", async (StockReadParams p, IInventoryService s, CancellationToken c) => (await s.ReadStock(p, c)).ToResult()).Produces<UResponse<IEnumerable<StockResponse>>>();

		r.MapPost("Supplier/Create", async (SupplierCreateParams p, IInventoryService s, CancellationToken c) => (await s.CreateSupplier(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Supplier/Read", async (InventoryReadParams<TagSupplier> p, IInventoryService s, CancellationToken c) => (await s.ReadSuppliers(p, c)).ToResult()).Produces<UResponse<IEnumerable<SupplierResponse>>>();
		r.MapPost("Supplier/Update", async (SupplierUpdateParams p, IInventoryService s, CancellationToken c) => (await s.UpdateSupplier(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Supplier/Delete", async (IdParams p, IInventoryService s, CancellationToken c) => (await s.DeleteSupplier(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Purchase/Create", async (PurchaseCreateParams p, IInventoryService s, CancellationToken c) => (await s.CreatePurchase(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Purchase/Read", async (PurchaseReadParams p, IInventoryService s, CancellationToken c) => (await s.ReadPurchases(p, c)).ToResult()).Produces<UResponse<IEnumerable<PurchaseResponse>>>();
		r.MapPost("Purchase/Review", async (PurchaseReviewParams p, IInventoryService s, CancellationToken c) => (await s.ReviewPurchase(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Purchase/Receive", async (PurchaseReceiveParams p, IInventoryService s, CancellationToken c) => (await s.ReceivePurchase(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Purchase/Delete", async (IdParams p, IInventoryService s, CancellationToken c) => (await s.DeletePurchase(p, c)).ToResult()).Produces<UResponse>();

		r.MapPost("Asset/Create", async (AssetCreateParams p, IInventoryService s, CancellationToken c) => (await s.CreateAsset(p, c)).ToResult()).Produces<UResponse<Guid?>>();
		r.MapPost("Asset/Read", async (InventoryReadParams<TagAsset> p, IInventoryService s, CancellationToken c) => (await s.ReadAssets(p, c)).ToResult()).Produces<UResponse<IEnumerable<AssetResponse>>>();
		r.MapPost("Asset/Update", async (AssetUpdateParams p, IInventoryService s, CancellationToken c) => (await s.UpdateAsset(p, c)).ToResult()).Produces<UResponse>();
		r.MapPost("Asset/Delete", async (IdParams p, IInventoryService s, CancellationToken c) => (await s.DeleteAsset(p, c)).ToResult()).Produces<UResponse>();
	}
}
