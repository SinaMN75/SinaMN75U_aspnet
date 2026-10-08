namespace SinaMN75U.Services;

public interface IInventoryService {
	Task<UResponse<Guid?>> CreateWarehouse(WarehouseCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<WarehouseResponse>?>> ReadWarehouses(InventoryReadParams<TagWarehouse> p, CancellationToken ct);
	Task<UResponse> UpdateWarehouse(WarehouseUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteWarehouse(IdParams p, CancellationToken ct);
	Task<UResponse<Guid?>> CreateItem(InventoryItemCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<InventoryItemResponse>?>> ReadItems(InventoryReadParams<TagInventoryItem> p, CancellationToken ct);
	Task<UResponse> UpdateItem(InventoryItemUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteItem(IdParams p, CancellationToken ct);
	Task<UResponse<Guid?>> CreateMovement(StockMovementCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<StockMovementResponse>?>> ReadMovements(StockMovementReadParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<StockResponse>?>> ReadStock(StockReadParams p, CancellationToken ct);
	Task<UResponse<Guid?>> CreateSupplier(SupplierCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<SupplierResponse>?>> ReadSuppliers(InventoryReadParams<TagSupplier> p, CancellationToken ct);
	Task<UResponse> UpdateSupplier(SupplierUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteSupplier(IdParams p, CancellationToken ct);
	Task<UResponse<Guid?>> CreatePurchase(PurchaseCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<PurchaseResponse>?>> ReadPurchases(PurchaseReadParams p, CancellationToken ct);
	Task<UResponse> ReviewPurchase(PurchaseReviewParams p, CancellationToken ct);
	Task<UResponse> ReceivePurchase(PurchaseReceiveParams p, CancellationToken ct);
	Task<UResponse> DeletePurchase(IdParams p, CancellationToken ct);
	Task<UResponse<Guid?>> CreateAsset(AssetCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<AssetResponse>?>> ReadAssets(InventoryReadParams<TagAsset> p, CancellationToken ct);
	Task<UResponse> UpdateAsset(AssetUpdateParams p, CancellationToken ct);
	Task<UResponse> DeleteAsset(IdParams p, CancellationToken ct);
}

public class InventoryService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IOrganizationService os,
	IAccountingService acc
) : IInventoryService {
	private async Task<(JwtClaimData? User, UResponse? Error)> Keeper(string? token, Guid organizationId, CancellationToken ct, TagUser permission = TagUser.PermissionManageInventory) {
		JwtClaimData? u = ts.ExtractClaims(token);
		if (u == null) return (null, new UResponse(Usc.UnAuthorized, ls.Get("pleaseSignInToContinue")));
		if (u.IsExpired) return (null, new UResponse(Usc.ExpiredToken, ls.Get("authTokenIsExpired")));
		if (!await db.Set<OrganizationEntity>().AnyAsync(x => x.Id == organizationId, ct)) return (null, new UResponse(Usc.NotFound, ls.Get("organizationNotFound")));
		return await os.CanManage(u, organizationId, permission, ct) ? (u, null) : (null, new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction")));
	}

	private async Task<UResponse?> CheckPlace(Guid organizationId, Guid? placeId, CancellationToken ct) =>
		placeId != null && !await os.IsPlaceOf(organizationId, placeId.Value, ct) ? new UResponse(Usc.Forbidden, ls.Get("youDoNotHaveClearanceToDoThisAction")) : null;

	private Task<decimal> StockOf(Guid itemId, Guid? warehouseId, CancellationToken ct) =>
		db.Set<StockMovementEntity>().Where(x => x.ItemId == itemId && (warehouseId == null || x.WarehouseId == warehouseId)).SumAsync(x => x.Quantity, ct);

	private async Task<decimal> AverageCost(Guid itemId, CancellationToken ct) {
		var totals = await db.Set<StockMovementEntity>()
			.Where(x => x.ItemId == itemId)
			.GroupBy(_ => 1)
			.Select(g => new { Quantity = g.Sum(x => x.Quantity), Value = g.Sum(x => x.Quantity * x.UnitPrice) })
			.FirstOrDefaultAsync(ct);
		return totals == null || totals.Quantity == 0 ? 0 : Math.Round(totals.Value / totals.Quantity, 2);
	}

	private async Task NotifyLowStock(InventoryItemEntity item, CancellationToken ct) {
		decimal stock = await StockOf(item.Id, null, ct);
		bool low = item.MinStock > 0 && stock < item.MinStock;
		if (low && !item.JsonData.LowStockNotified) {
			OrganizationEntity? o = await os.ReadOrganization(item.OrganizationId, ct);
			if (o != null)
				await db.Set<NotificationEntity>().AddAsync(new NotificationEntity {
					Id = Guid.CreateVersion7(),
					CreatedAt = DateTime.UtcNow,
					CreatorId = o.OwnerId,
					UserId = o.OwnerId,
					Tags = [TagNotification.Reminder, TagNotification.Unread],
					JsonData = new NotificationJson { Detail1 = ls.Get("lowStockAlert", "fa"), Detail2 = $"{item.Title}: {stock:0.###} {item.Unit}" }
				}, ct);
		}

		item.JsonData.LowStockNotified = low;
	}

	public async Task<UResponse<Guid?>> CreateWarehouse(WarehouseCreateParams p, CancellationToken ct) {
		(_, UResponse? error) = await Keeper(p.Token, p.OrganizationId, ct);
		error ??= await CheckPlace(p.OrganizationId, p.PlaceId, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);

		Guid id = Guid.CreateVersion7();
		await db.Set<WarehouseEntity>().AddAsync(new WarehouseEntity {
			Id = id,
			CreatorId = p.OrganizationId,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			Title = p.Title,
			PlaceId = p.PlaceId,
			OrganizationId = p.OrganizationId,
			JsonData = new WarehouseJson { Detail1 = p.Detail1, Detail2 = p.Detail2, Address = p.Address, KeeperId = p.KeeperId }
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<WarehouseResponse>?>> ReadWarehouses(InventoryReadParams<TagWarehouse> p, CancellationToken ct) {
		(_, UResponse? error) = await Keeper(p.Token, p.OrganizationId, ct);
		if (error != null) return new UResponse<IEnumerable<WarehouseResponse>?>(null, error.Status, error.Message);

		IQueryable<WarehouseEntity> q = db.Set<WarehouseEntity>().Where(x => x.OrganizationId == p.OrganizationId).ApplyReadParams(p);
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));
		if (p.PlaceId != null) q = q.Where(x => x.PlaceId == p.PlaceId);
		return await q.Select(x => new WarehouseResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			PlaceId = x.PlaceId,
			OrganizationId = x.OrganizationId
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateWarehouse(WarehouseUpdateParams p, CancellationToken ct) {
		WarehouseEntity? e = await db.Set<WarehouseEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, UResponse? error) = await Keeper(p.Token, e.OrganizationId, ct);
		error ??= await CheckPlace(e.OrganizationId, p.PlaceId, ct);
		if (error != null) return error;

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title!;
		if (p.PlaceId != null) e.PlaceId = p.PlaceId;
		if (p.Address != null) e.JsonData.Address = p.Address;
		if (p.KeeperId != null) e.JsonData.KeeperId = p.KeeperId;
		e.ApplyUpdateParam<WarehouseEntity, TagWarehouse, WarehouseJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteWarehouse(IdParams p, CancellationToken ct) {
		WarehouseEntity? e = await db.Set<WarehouseEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, UResponse? error) = await Keeper(p.Token, e.OrganizationId, ct);
		if (error != null) return error;
		if (await db.Set<StockMovementEntity>().AnyAsync(x => x.WarehouseId == e.Id, ct) || await db.Set<PurchaseEntity>().AnyAsync(x => x.WarehouseId == e.Id, ct))
			return new UResponse(Usc.Conflict, ls.Get("itemIsInUse"));
		db.Set<WarehouseEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateItem(InventoryItemCreateParams p, CancellationToken ct) {
		(_, UResponse? error) = await Keeper(p.Token, p.OrganizationId, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);
		if (p.Code.IsNotNullOrEmpty() && await db.Set<InventoryItemEntity>().AnyAsync(x => x.OrganizationId == p.OrganizationId && x.Code == p.Code, ct))
			return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("codeAlreadyExists"));

		Guid id = Guid.CreateVersion7();
		await db.Set<InventoryItemEntity>().AddAsync(new InventoryItemEntity {
			Id = id,
			CreatorId = p.OrganizationId,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			Title = p.Title,
			Code = p.Code.NullIfEmpty(),
			Unit = p.Unit,
			MinStock = Math.Max(0, p.MinStock),
			OrganizationId = p.OrganizationId,
			JsonData = new InventoryItemJson { Detail1 = p.Detail1, Detail2 = p.Detail2, Description = p.Description }
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<InventoryItemResponse>?>> ReadItems(InventoryReadParams<TagInventoryItem> p, CancellationToken ct) {
		(_, UResponse? error) = await Keeper(p.Token, p.OrganizationId, ct);
		if (error != null) return new UResponse<IEnumerable<InventoryItemResponse>?>(null, error.Status, error.Message);

		IQueryable<InventoryItemEntity> q = db.Set<InventoryItemEntity>().Where(x => x.OrganizationId == p.OrganizationId).ApplyReadParams(p);
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!) || x.Code == p.Title);
		return await q.Select(x => new InventoryItemResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			Code = x.Code,
			Unit = x.Unit,
			MinStock = x.MinStock,
			Stock = db.Set<StockMovementEntity>().Where(m => m.ItemId == x.Id).Sum(m => (decimal?)m.Quantity) ?? 0,
			OrganizationId = x.OrganizationId
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateItem(InventoryItemUpdateParams p, CancellationToken ct) {
		InventoryItemEntity? e = await db.Set<InventoryItemEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, UResponse? error) = await Keeper(p.Token, e.OrganizationId, ct);
		if (error != null) return error;
		if (p.Code.IsNotNullOrEmpty() && p.Code != e.Code && await db.Set<InventoryItemEntity>().AnyAsync(x => x.OrganizationId == e.OrganizationId && x.Code == p.Code, ct))
			return new UResponse(Usc.Conflict, ls.Get("codeAlreadyExists"));

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title!;
		if (p.Unit.IsNotNullOrEmpty()) e.Unit = p.Unit!;
		if (p.Code != null) e.Code = p.Code.NullIfEmpty();
		if (p.MinStock != null) e.MinStock = Math.Max(0, p.MinStock.Value);
		if (p.Description != null) e.JsonData.Description = p.Description;
		e.ApplyUpdateParam<InventoryItemEntity, TagInventoryItem, InventoryItemJson>(p);
		await NotifyLowStock(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteItem(IdParams p, CancellationToken ct) {
		InventoryItemEntity? e = await db.Set<InventoryItemEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, UResponse? error) = await Keeper(p.Token, e.OrganizationId, ct);
		if (error != null) return error;
		if (await db.Set<StockMovementEntity>().AnyAsync(x => x.ItemId == e.Id, ct)) return new UResponse(Usc.Conflict, ls.Get("itemIsInUse"));
		db.Set<InventoryItemEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	private async Task<StockMovementEntity> AddMovement(InventoryItemEntity item, Guid warehouseId, TagStockMovement tag, decimal quantity, decimal unitPrice, DateTime date, StockMovementJson json, CancellationToken ct) {
		StockMovementEntity m = new() {
			Id = Guid.CreateVersion7(),
			CreatorId = item.OrganizationId,
			CreatedAt = DateTime.UtcNow,
			Tags = [tag],
			Quantity = quantity,
			UnitPrice = unitPrice,
			Date = date,
			ItemId = item.Id,
			WarehouseId = warehouseId,
			OrganizationId = item.OrganizationId,
			JsonData = json
		};
		await db.Set<StockMovementEntity>().AddAsync(m, ct);
		return m;
	}

	public async Task<UResponse<Guid?>> CreateMovement(StockMovementCreateParams p, CancellationToken ct) {
		(JwtClaimData? u, UResponse? error) = await Keeper(p.Token, p.OrganizationId, ct);
		error ??= await CheckPlace(p.OrganizationId, p.PlaceId, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);

		TagStockMovement tag = p.Tags.FirstOrDefault(x => x is TagStockMovement.In or TagStockMovement.Out or TagStockMovement.TransferOut or TagStockMovement.Adjustment, TagStockMovement.In);
		InventoryItemEntity? item = await db.Set<InventoryItemEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.ItemId && x.OrganizationId == p.OrganizationId, ct);
		WarehouseEntity? warehouse = await db.Set<WarehouseEntity>().FirstOrDefaultAsync(x => x.Id == p.WarehouseId && x.OrganizationId == p.OrganizationId, ct);
		if (item == null || warehouse == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("itemNotFound"));
		if (p.Quantity == 0 || tag != TagStockMovement.Adjustment && p.Quantity < 0) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("quantityIsNotValid"));
		if (p.AccountId != null && !await acc.IsAccountOf(p.OrganizationId, p.AccountId.Value, ct)) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("ledgerAccountNotFound"));

		decimal outgoing = tag is TagStockMovement.Out or TagStockMovement.TransferOut ? p.Quantity : tag == TagStockMovement.Adjustment && p.Quantity < 0 ? -p.Quantity : 0;
		if (outgoing > 0 && await StockOf(item.Id, warehouse.Id, ct) < outgoing) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("notEnoughStock"));

		DateTime date = p.Date ?? DateTime.UtcNow;
		Guid? placeId = p.PlaceId ?? warehouse.PlaceId;
		decimal average = await AverageCost(item.Id, ct);
		StockMovementJson Json() => new() { Detail1 = p.Detail1, Detail2 = p.Detail2, PlaceId = placeId, AccountId = p.AccountId, RegisteredBy = u!.Id };
		string description = $"{ls.Get(tag == TagStockMovement.In ? "stockIn" : tag == TagStockMovement.Adjustment ? "stockAdjustment" : "stockOut", "fa")} - {item.Title}";

		StockMovementEntity m;
		if (tag == TagStockMovement.TransferOut) {
			WarehouseEntity? target = await db.Set<WarehouseEntity>().FirstOrDefaultAsync(x => x.Id == p.TargetWarehouseId && x.OrganizationId == p.OrganizationId, ct);
			if (target == null || target.Id == warehouse.Id) return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("selectTheTargetWarehouse"));
			Guid transferId = Guid.CreateVersion7();
			StockMovementJson json = Json();
			json.TransferId = transferId;
			m = await AddMovement(item, warehouse.Id, TagStockMovement.TransferOut, -p.Quantity, average, date, json, ct);
			StockMovementJson incoming = Json();
			incoming.TransferId = transferId;
			incoming.PlaceId = target.PlaceId;
			await AddMovement(item, target.Id, TagStockMovement.TransferIn, p.Quantity, average, date, incoming, ct);
		}
		else if (tag == TagStockMovement.In) {
			decimal price = Math.Max(0, p.UnitPrice);
			m = await AddMovement(item, warehouse.Id, tag, p.Quantity, price, date, Json(), ct);
			decimal value = Math.Round(p.Quantity * price, 2);
			if (value > 0)
				await acc.Post(p.OrganizationId, TagVoucher.Purchase, m.Id, placeId, null, description, date, ct,
					new AccountingLeg(TagAccount.Inventory, value), new AccountingLeg(TagAccount.Payables, -value, p.AccountId));
		}
		else {
			decimal quantity = tag == TagStockMovement.Out ? -p.Quantity : p.Quantity;
			m = await AddMovement(item, warehouse.Id, tag, quantity, average, date, Json(), ct);
			decimal value = Math.Round(-quantity * average, 2);
			if (value != 0)
				await acc.Post(p.OrganizationId, TagVoucher.Consumption, m.Id, placeId, null, description, date, ct,
					new AccountingLeg(TagAccount.ConsumptionExpense, value, p.AccountId), new AccountingLeg(TagAccount.Inventory, -value));
		}

		await db.SaveChangesAsync(ct);
		await NotifyLowStock(item, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(m.Id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<StockMovementResponse>?>> ReadMovements(StockMovementReadParams p, CancellationToken ct) {
		(_, UResponse? error) = await Keeper(p.Token, p.OrganizationId, ct);
		if (error != null) return new UResponse<IEnumerable<StockMovementResponse>?>(null, error.Status, error.Message);

		IQueryable<StockMovementEntity> q = db.Set<StockMovementEntity>().Where(x => x.OrganizationId == p.OrganizationId).ApplyReadParams(p);
		if (p.ItemId != null) q = q.Where(x => x.ItemId == p.ItemId);
		if (p.WarehouseId != null) q = q.Where(x => x.WarehouseId == p.WarehouseId);
		if (p.FromDate != null) q = q.Where(x => x.Date >= p.FromDate);
		if (p.ToDate != null) q = q.Where(x => x.Date <= p.ToDate);
		return await q.OrderByDescending(x => x.Date).Select(x => new StockMovementResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Quantity = x.Quantity,
			UnitPrice = x.UnitPrice,
			Date = x.Date,
			ItemId = x.ItemId,
			ItemTitle = x.Item.Title,
			Unit = x.Item.Unit,
			WarehouseId = x.WarehouseId,
			WarehouseTitle = x.Warehouse.Title
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<IEnumerable<StockResponse>?>> ReadStock(StockReadParams p, CancellationToken ct) {
		(_, UResponse? error) = await Keeper(p.Token, p.OrganizationId, ct);
		if (error != null) return new UResponse<IEnumerable<StockResponse>?>(null, error.Status, error.Message);

		IQueryable<StockMovementEntity> q = db.Set<StockMovementEntity>().Where(x => x.OrganizationId == p.OrganizationId);
		if (p.WarehouseId != null) q = q.Where(x => x.WarehouseId == p.WarehouseId);
		if (p.ItemId != null) q = q.Where(x => x.ItemId == p.ItemId);

		var rows = await q.GroupBy(x => new { x.ItemId, x.WarehouseId })
			.Select(g => new { g.Key.ItemId, g.Key.WarehouseId, Quantity = g.Sum(x => x.Quantity), Value = g.Sum(x => x.Quantity * x.UnitPrice) })
			.ToListAsync(ct);
		Dictionary<Guid, InventoryItemEntity> items = await db.Set<InventoryItemEntity>().Where(x => x.OrganizationId == p.OrganizationId).ToDictionaryAsync(x => x.Id, ct);
		Dictionary<Guid, string> warehouses = await db.Set<WarehouseEntity>().Where(x => x.OrganizationId == p.OrganizationId).ToDictionaryAsync(x => x.Id, x => x.Title, ct);
		Dictionary<Guid, decimal> totals = rows.GroupBy(x => x.ItemId).ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

		List<StockResponse> result = rows.Where(x => items.ContainsKey(x.ItemId)).Select(x => {
			InventoryItemEntity item = items[x.ItemId];
			decimal average = x.Quantity == 0 ? 0 : Math.Round(x.Value / x.Quantity, 2);
			return new StockResponse {
				ItemId = x.ItemId,
				ItemTitle = item.Title,
				Code = item.Code,
				Unit = item.Unit,
				WarehouseId = x.WarehouseId,
				WarehouseTitle = warehouses.GetValueOrDefault(x.WarehouseId, ""),
				Quantity = x.Quantity,
				MinStock = item.MinStock,
				AverageCost = average,
				Value = Math.Round(x.Value, 2),
				Low = item.MinStock > 0 && totals[x.ItemId] < item.MinStock
			};
		}).Where(x => !p.LowOnly || x.Low).OrderBy(x => x.ItemTitle).ThenBy(x => x.WarehouseTitle).ToList();
		return new UResponse<IEnumerable<StockResponse>?>(result);
	}

	public async Task<UResponse<Guid?>> CreateSupplier(SupplierCreateParams p, CancellationToken ct) {
		(_, UResponse? error) = await Keeper(p.Token, p.OrganizationId, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);

		Guid id = Guid.CreateVersion7();
		await db.Set<SupplierEntity>().AddAsync(new SupplierEntity {
			Id = id,
			CreatorId = p.OrganizationId,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			Title = p.Title,
			PhoneNumber = p.PhoneNumber,
			OrganizationId = p.OrganizationId,
			JsonData = new SupplierJson { Detail1 = p.Detail1, Detail2 = p.Detail2, ContactName = p.ContactName, Address = p.Address, NationalId = p.NationalId, Iban = p.Iban }
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<SupplierResponse>?>> ReadSuppliers(InventoryReadParams<TagSupplier> p, CancellationToken ct) {
		(_, UResponse? error) = await Keeper(p.Token, p.OrganizationId, ct);
		if (error != null) return new UResponse<IEnumerable<SupplierResponse>?>(null, error.Status, error.Message);

		IQueryable<SupplierEntity> q = db.Set<SupplierEntity>().Where(x => x.OrganizationId == p.OrganizationId).ApplyReadParams(p);
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));
		return await q.Select(x => new SupplierResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			PhoneNumber = x.PhoneNumber,
			OrganizationId = x.OrganizationId
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateSupplier(SupplierUpdateParams p, CancellationToken ct) {
		SupplierEntity? e = await db.Set<SupplierEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, UResponse? error) = await Keeper(p.Token, e.OrganizationId, ct);
		if (error != null) return error;

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title!;
		if (p.PhoneNumber != null) e.PhoneNumber = p.PhoneNumber.NullIfEmpty();
		if (p.ContactName != null) e.JsonData.ContactName = p.ContactName;
		if (p.Address != null) e.JsonData.Address = p.Address;
		if (p.NationalId != null) e.JsonData.NationalId = p.NationalId;
		if (p.Iban != null) e.JsonData.Iban = p.Iban;
		e.ApplyUpdateParam<SupplierEntity, TagSupplier, SupplierJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteSupplier(IdParams p, CancellationToken ct) {
		SupplierEntity? e = await db.Set<SupplierEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, UResponse? error) = await Keeper(p.Token, e.OrganizationId, ct);
		if (error != null) return error;
		if (await db.Set<PurchaseEntity>().AnyAsync(x => x.SupplierId == e.Id, ct)) return new UResponse(Usc.Conflict, ls.Get("itemIsInUse"));
		db.Set<SupplierEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreatePurchase(PurchaseCreateParams p, CancellationToken ct) {
		(JwtClaimData? u, UResponse? error) = await Keeper(p.Token, p.OrganizationId, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);
		if (!await db.Set<WarehouseEntity>().AnyAsync(x => x.Id == p.WarehouseId && x.OrganizationId == p.OrganizationId, ct)) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("itemNotFound"));
		if (p.SupplierId != null && !await db.Set<SupplierEntity>().AnyAsync(x => x.Id == p.SupplierId && x.OrganizationId == p.OrganizationId, ct)) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("itemNotFound"));
		List<Guid> itemIds = p.Lines.Select(x => x.ItemId).Distinct().ToList();
		if (p.Lines.Any(x => x.Quantity <= 0 || x.UnitPrice < 0) || await db.Set<InventoryItemEntity>().CountAsync(x => itemIds.Contains(x.Id) && x.OrganizationId == p.OrganizationId, ct) != itemIds.Count)
			return new UResponse<Guid?>(null, Usc.BadRequest, ls.Get("quantityIsNotValid"));

		int last = await db.Set<PurchaseEntity>().Where(x => x.OrganizationId == p.OrganizationId).MaxAsync(x => (int?)x.Number, ct) ?? 0;
		Guid id = Guid.CreateVersion7();
		await db.Set<PurchaseEntity>().AddAsync(new PurchaseEntity {
			Id = id,
			CreatorId = p.OrganizationId,
			CreatedAt = DateTime.UtcNow,
			Tags = [TagPurchase.Requested],
			Number = last + 1,
			Date = p.Date ?? DateTime.UtcNow,
			Total = Math.Round(p.Lines.Sum(x => x.Quantity * x.UnitPrice), 2),
			SupplierId = p.SupplierId,
			WarehouseId = p.WarehouseId,
			OrganizationId = p.OrganizationId,
			JsonData = new PurchaseJson {
				Detail1 = p.Detail1,
				Vat = Math.Max(0, Math.Round(p.Vat, 2)),
				RequestedBy = u!.Id,
				Lines = p.Lines.Select(x => new PurchaseLine { ItemId = x.ItemId, Quantity = x.Quantity, UnitPrice = x.UnitPrice }).ToList()
			}
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<PurchaseResponse>?>> ReadPurchases(PurchaseReadParams p, CancellationToken ct) {
		(_, UResponse? error) = await Keeper(p.Token, p.OrganizationId, ct);
		if (error != null) return new UResponse<IEnumerable<PurchaseResponse>?>(null, error.Status, error.Message);

		IQueryable<PurchaseEntity> q = db.Set<PurchaseEntity>().Where(x => x.OrganizationId == p.OrganizationId).ApplyReadParams(p);
		if (p.SupplierId != null) q = q.Where(x => x.SupplierId == p.SupplierId);
		if (p.WarehouseId != null) q = q.Where(x => x.WarehouseId == p.WarehouseId);
		return await q.OrderByDescending(x => x.Number).Select(x => new PurchaseResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Number = x.Number,
			Date = x.Date,
			Total = x.Total,
			SupplierId = x.SupplierId,
			SupplierTitle = db.Set<SupplierEntity>().Where(s => s.Id == x.SupplierId).Select(s => s.Title).FirstOrDefault(),
			WarehouseId = x.WarehouseId,
			WarehouseTitle = x.Warehouse.Title,
			OrganizationId = x.OrganizationId
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> ReviewPurchase(PurchaseReviewParams p, CancellationToken ct) {
		PurchaseEntity? e = await db.Set<PurchaseEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(JwtClaimData? u, UResponse? error) = await Keeper(p.Token, e.OrganizationId, ct, TagUser.PermissionManageAccounting);
		if (error != null) return error;
		if (!e.Tags.Contains(TagPurchase.Requested)) return new UResponse(Usc.Conflict, ls.Get("thisRequestIsAlreadyReviewed"));

		e.Tags = [p.Approve ? TagPurchase.Approved : TagPurchase.Rejected];
		e.JsonData.ReviewedBy = u!.Id;
		e.JsonData.ReviewedAt = DateTime.UtcNow;
		e.JsonData.ReviewNote = p.Note;
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> ReceivePurchase(PurchaseReceiveParams p, CancellationToken ct) {
		PurchaseEntity? e = await db.Set<PurchaseEntity>().AsTracking().Include(x => x.Warehouse).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(JwtClaimData? u, UResponse? error) = await Keeper(p.Token, e.OrganizationId, ct);
		if (error != null) return error;
		if (!e.Tags.Contains(TagPurchase.Approved)) return new UResponse(Usc.Conflict, ls.Get("purchaseIsNotApproved"));
		if (p.AccountId != null && !await acc.IsAccountOf(e.OrganizationId, p.AccountId.Value, ct)) return new UResponse(Usc.BadRequest, ls.Get("ledgerAccountNotFound"));

		DateTime date = p.Date ?? DateTime.UtcNow;
		List<Guid> itemIds = e.JsonData.Lines.Select(x => x.ItemId).ToList();
		Dictionary<Guid, InventoryItemEntity> items = await db.Set<InventoryItemEntity>().AsTracking().Where(x => itemIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
		foreach (PurchaseLine line in e.JsonData.Lines.Where(x => items.ContainsKey(x.ItemId)))
			await AddMovement(items[line.ItemId], e.WarehouseId, TagStockMovement.In, line.Quantity, line.UnitPrice, date,
				new StockMovementJson { PurchaseId = e.Id, PlaceId = e.Warehouse.PlaceId, RegisteredBy = u!.Id, Detail1 = $"{ls.Get("purchase", "fa")} {e.Number}" }, ct);

		if (e.Total > 0)
			await acc.Post(e.OrganizationId, TagVoucher.Purchase, e.Id, e.Warehouse.PlaceId, null, $"{ls.Get("purchase", "fa")} {e.Number}", date, ct,
				new AccountingLeg(TagAccount.Inventory, e.Total), new AccountingLeg(TagAccount.VatReceivable, e.JsonData.Vat), new AccountingLeg(TagAccount.Payables, -e.Total - e.JsonData.Vat, p.AccountId));

		e.Tags = [TagPurchase.Received];
		e.JsonData.ReceivedAt = date;
		e.JsonData.PaidFromAccountId = p.AccountId;
		await db.SaveChangesAsync(ct);
		foreach (InventoryItemEntity item in items.Values) await NotifyLowStock(item, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeletePurchase(IdParams p, CancellationToken ct) {
		PurchaseEntity? e = await db.Set<PurchaseEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, UResponse? error) = await Keeper(p.Token, e.OrganizationId, ct);
		if (error != null) return error;
		if (e.Tags.Contains(TagPurchase.Received)) return new UResponse(Usc.Conflict, ls.Get("itemIsInUse"));
		db.Set<PurchaseEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse<Guid?>> CreateAsset(AssetCreateParams p, CancellationToken ct) {
		(_, UResponse? error) = await Keeper(p.Token, p.OrganizationId, ct);
		error ??= await CheckPlace(p.OrganizationId, p.PlaceId, ct);
		if (error != null) return new UResponse<Guid?>(null, error.Status, error.Message);
		if (await db.Set<AssetEntity>().AnyAsync(x => x.OrganizationId == p.OrganizationId && x.Code == p.Code, ct)) return new UResponse<Guid?>(null, Usc.Conflict, ls.Get("codeAlreadyExists"));

		Guid id = Guid.CreateVersion7();
		await db.Set<AssetEntity>().AddAsync(new AssetEntity {
			Id = id,
			CreatorId = p.OrganizationId,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			Title = p.Title,
			Code = p.Code,
			PlaceId = p.PlaceId,
			OrganizationId = p.OrganizationId,
			JsonData = new AssetJson {
				Detail1 = p.Detail1,
				Detail2 = p.Detail2,
				Location = p.Location,
				RoomId = p.RoomId,
				BedId = p.BedId,
				ItemId = p.ItemId,
				SerialNumber = p.SerialNumber,
				PurchaseDate = p.PurchaseDate,
				Price = p.Price,
				AssignedUserId = p.AssignedUserId
			}
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(id, Usc.Created);
	}

	public async Task<UResponse<IEnumerable<AssetResponse>?>> ReadAssets(InventoryReadParams<TagAsset> p, CancellationToken ct) {
		(_, UResponse? error) = await Keeper(p.Token, p.OrganizationId, ct);
		if (error != null) return new UResponse<IEnumerable<AssetResponse>?>(null, error.Status, error.Message);

		IQueryable<AssetEntity> q = db.Set<AssetEntity>().Where(x => x.OrganizationId == p.OrganizationId).ApplyReadParams(p);
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!) || x.Code == p.Title);
		if (p.PlaceId != null) q = q.Where(x => x.PlaceId == p.PlaceId);
		return await q.Select(x => new AssetResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			CreatorId = x.CreatorId,
			Tags = x.Tags,
			JsonData = x.JsonData,
			Title = x.Title,
			Code = x.Code,
			PlaceId = x.PlaceId,
			OrganizationId = x.OrganizationId
		}).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> UpdateAsset(AssetUpdateParams p, CancellationToken ct) {
		AssetEntity? e = await db.Set<AssetEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, UResponse? error) = await Keeper(p.Token, e.OrganizationId, ct);
		error ??= await CheckPlace(e.OrganizationId, p.PlaceId, ct);
		if (error != null) return error;
		if (p.Code.IsNotNullOrEmpty() && p.Code != e.Code && await db.Set<AssetEntity>().AnyAsync(x => x.OrganizationId == e.OrganizationId && x.Code == p.Code, ct))
			return new UResponse(Usc.Conflict, ls.Get("codeAlreadyExists"));

		if (p.Title.IsNotNullOrEmpty()) e.Title = p.Title!;
		if (p.Code.IsNotNullOrEmpty()) e.Code = p.Code!;
		if (p.PlaceId != null) e.PlaceId = p.PlaceId;
		if (p.Location != null) e.JsonData.Location = p.Location;
		if (p.RoomId != null) e.JsonData.RoomId = p.RoomId;
		if (p.BedId != null) e.JsonData.BedId = p.BedId;
		if (p.SerialNumber != null) e.JsonData.SerialNumber = p.SerialNumber;
		if (p.PurchaseDate != null) e.JsonData.PurchaseDate = p.PurchaseDate;
		if (p.Price != null) e.JsonData.Price = p.Price;
		if (p.AssignedUserId != null) e.JsonData.AssignedUserId = p.AssignedUserId;
		e.ApplyUpdateParam<AssetEntity, TagAsset, AssetJson>(p);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> DeleteAsset(IdParams p, CancellationToken ct) {
		AssetEntity? e = await db.Set<AssetEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("itemNotFound"));
		(_, UResponse? error) = await Keeper(p.Token, e.OrganizationId, ct);
		if (error != null) return error;
		db.Set<AssetEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}
}
