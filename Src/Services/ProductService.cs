using SinaMN75U.Data;

namespace SinaMN75U.Services;

public interface IProductService {
	public Task<UResponse> BulkCreate(List<ProductCreateParams> p, CancellationToken ct);
	public Task<UResponse<ProductResponse?>> Create(ProductCreateParams p, CancellationToken ct);
	public Task<UResponse<IEnumerable<ProductResponse>?>> Read(ProductReadParams p, CancellationToken ct);
	public Task<UResponse<ProductResponse?>> ReadById(IdParams p, CancellationToken ct);
	public Task<UResponse<ProductResponse?>> Update(ProductUpdateParams p, CancellationToken ct);
	public Task<UResponse> Delete(IdParams p, CancellationToken ct);
	public Task<UResponse> DeleteRange(IdListParams p, CancellationToken ct);
}

public class ProductService(
	DbContext db,
	ITokenService ts,
	ILocalizationService ls,
	ILocalStorageService cache,
	ICategoryService categoryService,
	IMediaService mediaService
) : IProductService {
	public async Task<UResponse> BulkCreate(List<ProductCreateParams> p, CancellationToken ct) {
		foreach (ProductCreateParams param in p) await Create(param, ct);
		return new UResponse();
	}

	public async Task<UResponse<ProductResponse?>> Create(ProductCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null)
			return new UResponse<ProductResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		List<CategoryEntity> categories = p.Categories.IsNotNullOrEmpty()
			? await categoryService.ReadEntity(new CategoryReadParams { Ids = p.Categories }, ct) ?? []
			: [];

		ProductEntity e = FillData(p, userData.Id, p.ParentId, categories);
		EntityEntry<ProductEntity> created = await db.Set<ProductEntity>().AddAsync(e, ct);

		await db.SaveChangesAsync(ct);

		if (p.Children.IsNotNullOrEmpty()) await AddChildrenRecursively(p.Children, userData.Id, e.Id, categories, ct);

		await AddMedia(e.Id, p.Media ?? [], ct);

		cache.DeleteAllByPartialKey(RouteTags.Product);
		return new UResponse<ProductResponse?>(created.Entity.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<ProductResponse>?>> Read(ProductReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		p.SelectorArgs.UserId = userData?.Id;
		IQueryable<ProductEntity> q = db.Set<ProductEntity>().Where(x => x.ParentId == null);

		if (p.Query.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Query!) || (x.Description ?? "").Contains(p.Query!) || (x.Subtitle ?? "").Contains(p.Query!));
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title.Contains(p.Title!));
		if (p.Code.IsNotNullOrEmpty()) q = q.Where(x => (x.Code ?? "").Contains(p.Code!));
		if (p.Slug.IsNotNullOrEmpty()) q = q.Where(x => (x.Slug ?? "") == p.Code!);
		if (p.ParentId.HasValue) q = q.Where(x => x.ParentId == p.ParentId);
		if (p.UserId.HasValue) q = q.Where(x => x.UserId == p.UserId);
		if (p.Ids.IsNotNullOrEmpty()) q = q.Where(x => p.Ids.Contains(x.Id));
		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => p.Tags.All(tag => x.Tags.Contains(tag)));
		if (p.MinStock.IsNotNull()) q = q.Where(x => x.Stock >= p.MinStock);
		if (p.MaxStock.IsNotNull()) q = q.Where(x => x.Stock <= p.MaxStock);
		if (p.MaxRent.IsNotNull()) q = q.Where(x => x.Deposit >= p.MaxRent);
		if (p.MinDeposit.IsNotNull()) q = q.Where(x => x.Deposit <= p.MinDeposit);
		if (p.Categories.IsNotNullOrEmpty()) q = q.Where(x => x.Categories.Any(y => p.Categories.Contains(y.Id)));
		
		if (p.HasActiveContract == true) q = q.Where(x => x.Contracts.Any(y => y.EndDate >= DateTime.UtcNow));
		if (p.HasActiveContract == false) q = q.Where(x => x.Contracts.Any(y => y.EndDate <= DateTime.UtcNow));
		
		if (p.OrderByCreatedAt) q = q.OrderBy(x => x.CreatedAt);
		if (p.OrderByCreatedAtDesc) q = q.OrderByDescending(x => x.CreatedAt);
		if (p.OrderByOrder) q = q.OrderBy(x => x.Order);
		if (p.OrderByOrderDesc) q = q.OrderByDescending(x => x.Order);

		UResponse<IEnumerable<ProductResponse>?> list = await q.Select(Projections.ProductSelector(p.SelectorArgs)).ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
		return list;
	}

	public async Task<UResponse<ProductResponse?>> ReadById(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);

		ProductEntity? e = await db.Set<ProductEntity>().AsTracking()
			.Include(x => x.Media)
			.Include(x => x.Categories)
			.Include(x => x.User)
			.Include(x => x.Children)
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<ProductResponse?>(null, Usc.NotFound, ls.Get("ProductNotFound"));

		VisitCount? visitCount = e.JsonData.VisitCounts.FirstOrDefault(v => v.UserId == (userData?.Id ?? Guid.Empty));

		if (visitCount != null) visitCount.Count++;
		else e.JsonData.VisitCounts.Add(new VisitCount { UserId = userData?.Id ?? Guid.Empty, Count = 1 });

		db.Set<ProductEntity>().Update(e);
		await db.SaveChangesAsync(ct);

		return new UResponse<ProductResponse?>(e.MapToResponse());
	}

	public async Task<UResponse<ProductResponse?>> Update(ProductUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ProductResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ProductEntity? e = await db.Set<ProductEntity>().Include(x => x.Categories).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<ProductResponse?>(null, Usc.NotFound, ls.Get("ProductNotFound"));

		e.UpdatedAt = DateTime.UtcNow;
		if (p.Title.IsNotNull()) e.Title = p.Title;
		if (p.Code.IsNotNull()) e.Code = p.Code;
		if (p.Subtitle.IsNotNull()) e.Subtitle = p.Subtitle;
		if (p.Description.IsNotNull()) e.Description = p.Description;
		if (p.Slug.IsNotNull()) e.Slug = p.Slug;
		if (p.Type.IsNotNull()) e.Type = p.Type;
		if (p.Content.IsNotNull()) e.Content = p.Content;
		if (p.Latitude.IsNotNull()) e.Latitude = p.Latitude;
		if (p.Longitude.IsNotNull()) e.Longitude = p.Longitude;
		if (p.Stock.IsNotNull()) e.Stock = p.Stock.Value;
		if (p.Deposit.IsNotNull()) e.Deposit = p.Deposit.Value;
		if (p.Point.IsNotNull()) e.Point = p.Point.Value;
		if (p.Order.IsNotNull()) e.Order = p.Order.Value;
		if (p.ParentId.IsNotNull()) e.ParentId = p.ParentId;
		if (p.UserId.IsNotNull()) e.UserId = p.UserId.Value;
		if (p.ActionType.IsNotNull()) e.JsonData.ActionType = p.ActionType;
		if (p.ActionTitle.IsNotNull()) e.JsonData.ActionTitle = p.ActionTitle;
		if (p.ActionUri.IsNotNull()) e.JsonData.ActionUri = p.ActionUri;
		if (p.Details.IsNotNull()) e.JsonData.Details = p.Details;
		if (p.PhoneNumber.IsNotNull()) e.JsonData.PhoneNumber = p.PhoneNumber;
		if (p.Address.IsNotNull()) e.JsonData.Address = p.Address;
		if (p.RelatedProducts.IsNotNull()) e.JsonData.RelatedProducts = p.RelatedProducts;
		if (p.AddRelatedProducts.IsNotNullOrEmpty()) e.JsonData.RelatedProducts.AddRangeIfNotExist(p.AddRelatedProducts);
		if (p.RemoveRelatedProducts.IsNotNullOrEmpty()) e.JsonData.RelatedProducts.RemoveRangeIfExist(p.RemoveRelatedProducts);

		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => p.RemoveTags.Contains(x));
		if (p.Tags.IsNotNullOrEmpty()) e.Tags = p.Tags;

		if (p.AddCategories.IsNotNullOrEmpty())
			e.Categories.AddRangeIfNotExist(await categoryService.ReadEntity(new CategoryReadParams { Ids = p.AddCategories }, ct) ?? []);

		if (p.RemoveCategories.IsNotNullOrEmpty())
			e.Categories.RemoveRangeIfExist(await categoryService.ReadEntity(new CategoryReadParams { Ids = p.RemoveCategories }, ct) ?? []);

		if (p.Categories.IsNotNull()) {
			if (p.Categories.Count == 0) e.Categories = [];
			else e.Categories = await categoryService.ReadEntity(new CategoryReadParams { Ids = p.Categories }, ct) ?? [];
		}

		if (p is { UpdateInvoicesPrices: true, Rent: not null }) {
			PersianDateTime today = PersianDateTime.Today;

			int currentDay = today.Day;
			int totalDays = PersianDateTime.DaysInMonth(today.Year, today.Month);
			int remainingDays = totalDays - currentDay;

			IQueryable<ContractEntity> contracts = db.Set<ContractEntity>()
				.Where(x => x.ProductId == p.Id)
				.Include(x => x.Invoices);

			foreach (ContractEntity contract in contracts) {
				foreach (InvoiceEntity invoice in contract.Invoices) {
					if (invoice.Tags.Contains(TagInvoice.NotPaid)) {
						decimal oldPrice = invoice.DebtAmount;
						decimal newPrice = p.Rent.Value;
						decimal newDebt = oldPrice / totalDays * currentDay + newPrice / totalDays * remainingDays;
						invoice.DebtAmount = Math.Round(newDebt, 2);
						db.Update(invoice);
					}
				}
			}
		}

		db.Set<ProductEntity>().Update(e);
		await db.SaveChangesAsync(ct);
		await AddMedia(p.Id, p.Media ?? [], ct);

		cache.DeleteAllByPartialKey(RouteTags.Product);
		cache.DeleteAllByPartialKey(RouteTags.Contract);
		cache.DeleteAllByPartialKey(RouteTags.Invoice);
		return new UResponse<ProductResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ProductEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		int count = await db.Set<ProductEntity>().Where(x => x.Id == p.Id).ExecuteDeleteAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Product);
		return count > 0 ? new UResponse(Usc.Deleted, ls.Get("ProductDeleted")) : new UResponse(Usc.NotFound, ls.Get("ProductNotFound"));
	}

	public async Task<UResponse> DeleteRange(IdListParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ProductEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		int count = await db.Set<ProductEntity>().WhereIn(u => u.Id, p.Ids).ExecuteDeleteAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Product);
		return count > 0 ? new UResponse(Usc.Deleted, ls.Get("ProductDeleted")) : new UResponse(Usc.NotFound, ls.Get("ProductNotFound"));
	}

	private async Task AddChildrenRecursively(
		ICollection<ProductCreateParams> children,
		Guid userId,
		Guid parentId,
		List<CategoryEntity> categories,
		CancellationToken ct) {
		List<ProductEntity> childEntities = [];
		foreach (ProductCreateParams childParams in children) {
			ProductEntity childEntity = FillData(childParams, userId, parentId, categories);
			childEntities.Add(childEntity);
			await db.Set<ProductEntity>().AddAsync(childEntity, ct);
		}

		await db.SaveChangesAsync(ct);

		foreach ((ProductEntity childEntity, ProductCreateParams childParams) in childEntities.Zip(children, (e, p) => (e, p))) {
			await AddMedia(childEntity.Id, childParams.Media ?? [], ct);

			if (childParams.Children.IsNotNullOrEmpty()) {
				await AddChildrenRecursively(childParams.Children ?? [], userId, childEntity.Id, categories, ct);
			}
		}
	}

	private static ProductEntity FillData(
		ProductCreateParams p,
		Guid userId,
		Guid? parentId = null,
		List<CategoryEntity>? categories = null
	) {
		ProductEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			Title = p.Title,
			Code = p.Code,
			Subtitle = p.Subtitle,
			Description = p.Description,
			Latitude = p.Latitude,
			Longitude = p.Longitude,
			Deposit = p.Deposit,
			Rent = p.Rent,
			ParentId = parentId ?? p.ParentId,
			UserId = p.UserId ?? userId,
			Tags = p.Tags,
			Categories = categories ?? [],
			Type = p.Type,
			Content = p.Content,
			Slug = p.Slug,
			Stock = p.Stock ?? 0,
			Point = p.Point ?? 0,
			Order = p.Order ?? 0,
			JsonData = new ProductJson {
				Details = p.Details,
				ActionTitle = p.ActionTitle,
				ActionUri = p.ActionUri,
				ActionType = p.ActionType,
				PhoneNumber = p.PhoneNumber,
				Address = p.Address,
				VisitCounts = [],
				RelatedProducts = p.RelatedProducts?.ToList() ?? []
			}
		};
		return e;
	}

	private async Task AddMedia(Guid productId, ICollection<Guid> ids, CancellationToken ct) {
		if (ids.IsNullOrEmpty()) return;
		List<MediaEntity> media = await mediaService.ReadEntity(new BaseReadParams<TagMedia> { Ids = ids }, ct) ?? [];
		if (media.Count == 0) return;
		foreach (MediaEntity i in media) {
			await db.Set<MediaEntity>().Where(x => x.Id == i.Id).ExecuteUpdateAsync(
				u => u.SetProperty(y => y.ProductId, productId),
				ct
			);
		}
	}
}