using Microsoft.EntityFrameworkCore.Query;

namespace SinaMN75U.Services;

public interface ICategoryService {
	Task<UResponse> BulkCreate(IEnumerable<CategoryCreateParams> p, CancellationToken ct);
	Task<UResponse<CategoryEntity?>> Create(CategoryCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<CategoryResponse>?>> Read(CategoryReadParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<CategoryResponse>?>> ReadDept(CategoryReadParams p, CancellationToken ct);
	Task<UResponse<CategoryEntity?>> ReadById(IdParams p, CancellationToken ct);
	Task<UResponse<CategoryEntity?>> Update(CategoryUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> DeleteRange(IdListParams p, CancellationToken ct);

	Task<List<CategoryEntity>?> ReadEntity(CategoryReadParams p, CancellationToken ct);
}

public class CategoryService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	ILocalStorageService cache,
	IMediaService mediaService
) : ICategoryService {
	public async Task<UResponse> BulkCreate(IEnumerable<CategoryCreateParams> p, CancellationToken ct) {
		foreach (CategoryCreateParams param in p) await Create(param, ct);
		return new UResponse();
	}

	public async Task<UResponse<CategoryEntity?>> Create(CategoryCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<CategoryEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		CategoryEntity e = FillData(p);
		await db.Set<CategoryEntity>().AddAsync(e, ct);

		if (p.Children.IsNotNullOrEmpty()) await AddChildrenRecursively(p.Children, e.Id, ct);

		await db.SaveChangesAsync(ct);
		await AddMedia(e.Id, p.Media, ct);

		cache.DeleteAllByPartialKey(RouteTags.Category);
		return new UResponse<CategoryEntity?>(e);
	}

	// public async Task<UResponse<IEnumerable<CategoryEntity>?>> Read(CategoryReadParams p, CancellationToken ct) {
	// 	IQueryable<CategoryEntity> q = db.Set<CategoryEntity>()
	// 		.Where(x => x.ParentId == null)
	// 		.OrderBy(x => x.Id);
	//
	// 	if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => p.Tags.All(tag => x.Tags.Contains(tag)));
	// 	if (p.Ids.IsNotNullOrEmpty()) q = q.Where(x => p.Ids.Contains(x.Id));
	//
	// 	if (p.ShowMedia) q = q.Include(x => x.Media);
	//
	// 	if (p.ShowChildren) {
	// 		if (p.ShowChildrenMedia) q = q.Include(x => x.Children).ThenInclude(x => x.Media);
	// 		else q = q.Include(x => x.Children);
	// 	}
	//
	// 	if (p.OrderByOrder) q = q.OrderBy(x => x.Order);
	// 	if (p.OrderByOrderDesc) q = q.OrderByDescending(x => x.Order);
	//
	// 	return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	// }

	public async Task<UResponse<IEnumerable<CategoryResponse>?>> Read(CategoryReadParams p, CancellationToken ct) {
		IQueryable<CategoryEntity> q = db.Set<CategoryEntity>()
			.Where(x => x.ParentId == null)
			.OrderBy(x => x.Id);

		if (p.Tags.IsNotNullOrEmpty())
			q = q.Where(x => p.Tags.All(tag => x.Tags.Contains(tag)));

		if (p.Ids.IsNotNullOrEmpty())
			q = q.Where(x => p.Ids.Contains(x.Id));

		if (p.ShowMedia)
			q = q.Include(x => x.Media);

		if (p.ShowChildren) {
			if (p.ShowChildrenMedia)
				q = q.Include(x => x.Children).ThenInclude(x => x.Media);
			else
				q = q.Include(x => x.Children);
		}

		if (p.OrderByOrder) q = q.OrderBy(x => x.Order);
		if (p.OrderByOrderDesc) q = q.OrderByDescending(x => x.Order);

		IQueryable<CategoryResponse> projected = q.Select(x => new CategoryResponse {
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			DeletedAt = x.DeletedAt,
			Tags = x.Tags,
			JsonData = x.JsonData,

			Title = x.Title,
			Order = x.Order,
			Code = x.Code,
			ParentId = x.ParentId,

			Media = p.ShowMedia
				? x.Media.Select(m => new MediaResponse {
					Tags = m.Tags,
					JsonData = m.JsonData,
					Path = m.Path
				}).ToList()
				: new List<MediaResponse>(),

			Children = p.ShowChildren
				? x.Children.Select(c => new CategoryResponse {
					Id = c.Id,
					CreatedAt = c.CreatedAt,
					UpdatedAt = c.UpdatedAt,
					DeletedAt = c.DeletedAt,
					Tags = c.Tags,
					JsonData = c.JsonData,
					Title = c.Title,
					Order = c.Order,
					Code = c.Code,
					ParentId = c.ParentId,
					Media = p.ShowChildrenMedia
						? c.Media.Select(m => new MediaResponse {
							Tags = m.Tags,
							JsonData = m.JsonData,
							Path = m.Path
						}).ToList()
						: new List<MediaResponse>(),

					Children = new List<CategoryResponse>()
				}).ToList()
				: new List<CategoryResponse>()
		});

		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	// public async Task<UResponse<IEnumerable<CategoryEntity>?>> ReadDept(CategoryReadParams p, CancellationToken ct) {
	// 	IQueryable<CategoryEntity> q = db.Set<CategoryEntity>()
	// 		.Where(x => x.ParentId == null)
	// 		.OrderBy(x => x.Id);
	//
	// 	if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags.Any(tag => p.Tags!.Contains(tag)));
	// 	if (p.Ids.IsNotNullOrEmpty()) q = q.Where(x => p.Ids.Contains(x.Id));
	//
	// 	if (p.ShowMedia) q = q.Include(x => x.Media);
	//
	// 	if (p.ShowChildren)
	// 		q = q.Include(x => x.Children)
	// 			.ThenInclude(x => x.Children)
	// 			.ThenInclude(x => x.Children)
	// 			.ThenInclude(x => x.Children)
	// 			.ThenInclude(x => x.Children)
	// 			.ThenInclude(x => x.Children)
	// 			.ThenInclude(x => x.Children)
	// 			.ThenInclude(x => x.Children)
	// 			.ThenInclude(x => x.Children);
	//
	// 	if (p.OrderByOrder) q = q.OrderBy(x => x.Order);
	// 	if (p.OrderByOrderDesc) q = q.OrderByDescending(x => x.Order);
	//
	// 	return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	// }

	public async Task<UResponse<IEnumerable<CategoryResponse>?>> ReadDept(CategoryReadParams p, CancellationToken ct) {
		IQueryable<CategoryEntity> q = db.Set<CategoryEntity>().Where(x => x.ParentId == null).OrderBy(x => x.Id);

		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags.Any(tag => p.Tags!.Contains(tag)));
		if (p.Ids.IsNotNullOrEmpty()) q = q.Where(x => p.Ids.Contains(x.Id));
		
		q = q
			.Include(x => x.Children).ThenInclude(c => c.Media)
			.Include(x => x.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Media)
			.Include(x => x.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Media)
			.Include(x => x.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Media)
			.Include(x => x.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Media)
			.Include(x => x.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Media)
			.Include(x => x.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Media)
			.Include(x => x.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Media);

		if (p.OrderByOrder) q = q.OrderBy(x => x.Order);
		if (p.OrderByOrderDesc) q = q.OrderByDescending(x => x.Order);

		IQueryable<CategoryResponse> projected = q.Select(x => ToDtoDeep(x, p.ShowChildren, p.ShowMedia, p.ShowChildrenMedia));

		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<CategoryEntity?>> ReadById(IdParams p, CancellationToken ct) {
		CategoryEntity? e = await db.Set<CategoryEntity>().Select(x => new CategoryEntity {
			Title = x.Title,
			Order = x.Order,
			ParentId = x.ParentId,
			Media = x.Media.Select(y => new MediaEntity {
				Path = y.Path,
				Id = y.Id,
				Tags = y.Tags,
				JsonData = y.JsonData
			}).ToList(),
			Id = x.Id,
			CreatedAt = x.CreatedAt,
			UpdatedAt = x.UpdatedAt,
			Tags = x.Tags,
			JsonData = x.JsonData
		}).FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		return e == null ? new UResponse<CategoryEntity?>(null, Usc.NotFound, ls.Get("CategoryNotFound")) : new UResponse<CategoryEntity?>(e);
	}

	public async Task<UResponse<CategoryEntity?>> Update(CategoryUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<CategoryEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		CategoryEntity? e = await db.Set<CategoryEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<CategoryEntity?>(null, Usc.NotFound, ls.Get("CategoryNotFound"));

		e.UpdatedAt = DateTime.UtcNow;
		if (p.Title.IsNotNull()) e.Title = p.Title;
		if (p.ParentId.HasValue()) e.ParentId = p.ParentId;
		if (p.Order.IsNotNull()) e.Order = p.Order;
		if (p.Code.IsNotNull()) e.Code = p.Code;

		if (p.Subtitle.IsNotNull()) e.JsonData.Subtitle = p.Subtitle;
		if (p.Link.IsNotNull()) e.JsonData.Link = p.Link;
		if (p.Location.IsNotNull()) e.JsonData.Location = p.Location;
		if (p.Type.IsNotNull()) e.JsonData.Type = p.Type;
		if (p.Address.IsNotNull()) e.JsonData.Address = p.Address;
		if (p.PhoneNumber.IsNotNull()) e.JsonData.PhoneNumber = p.PhoneNumber;

		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => p.RemoveTags.Contains(x));
		if (p.Tags.IsNotNullOrEmpty()) e.Tags = p.Tags;

		if (p.RelatedProducts.IsNotNull()) e.JsonData.RelatedProducts = p.RelatedProducts;
		if (p.AddRelatedProducts.IsNotNullOrEmpty()) e.JsonData.RelatedProducts.AddRangeIfNotExist(p.AddRelatedProducts);
		if (p.RemoveRelatedProducts.IsNotNullOrEmpty()) e.JsonData.RelatedProducts.RemoveRangeIfExist(p.RemoveRelatedProducts);

		if (p.ProductDeposit.IsNotNull()) {
			await db.Set<ProductEntity>()
				.Where(x => x.Categories.Any(c => c.Id == e.Id))
				.ExecuteUpdateAsync(set => set.SetProperty(x => x.Deposit, p.ProductDeposit.Value), cancellationToken: ct);
		}

		if (p.ProductRent.IsNotNull()) {
			await db.Set<ProductEntity>()
				.Where(x => x.Categories.Any(c => c.Id == e.Id))
				.ExecuteUpdateAsync(set => set.SetProperty(x => x.Rent, p.ProductRent.Value), cancellationToken: ct);

			if (p.UpdateInvoicesRent) {
				PersianDateTime today = PersianDateTime.Today;
				int totalDays = PersianDateTime.DaysInMonth(today.Year, today.Month);
				int pastDays = today.Day;
				int remainingDays = totalDays - today.Day;
				double newPrice = p.ProductRent.Value;

				List<InvoiceEntity> invoices = await db.Set<InvoiceEntity>()
					.Where(inv => inv.Tags.Contains(TagInvoice.NotPaid) && inv.Contract.Product.Categories.Any(c => c.Id == e.Id))
					.AsNoTracking()
					.ToListAsync(ct);

				foreach (InvoiceEntity inv in invoices) {
					double oldPrice = inv.DebtAmount;
					double newDebt = oldPrice / totalDays * pastDays + newPrice / totalDays * remainingDays;
					inv.DebtAmount = Math.Round(newDebt, 2);
					db.Update(inv);
				}

				await db.SaveChangesAsync(ct);
			}

			cache.DeleteAllByPartialKey(RouteTags.Invoice);
		}


		db.Update(e);
		await db.SaveChangesAsync(ct);
		await AddMedia(e.Id, p.Media, ct);

		cache.DeleteAllByPartialKey(RouteTags.Category);
		cache.DeleteAllByPartialKey(RouteTags.Product);
		cache.DeleteAllByPartialKey(RouteTags.User);

		return new UResponse<CategoryEntity?>(e);
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null)
			return new UResponse<CategoryEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		CategoryEntity? category = await db.Set<CategoryEntity>()
			.Include(x => x.Media)
			.FirstOrDefaultAsync(x => x.Id == p.Id, ct);

		if (category == null)
			return new UResponse(Usc.NotFound, ls.Get("CategoryNotFound"));

		if (category.Media.IsNotNullOrEmpty())
			await mediaService.DeleteRange(category.Media.Select(x => x.Id), ct);

		db.Set<CategoryEntity>().Remove(category);
		await db.SaveChangesAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Category);
		cache.DeleteAllByPartialKey(RouteTags.Product);
		cache.DeleteAllByPartialKey(RouteTags.User);
		return new UResponse();
	}

	public async Task<UResponse> DeleteRange(IdListParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null)
			return new UResponse<CategoryEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<CategoryEntity>().WhereIn(u => u.Id, p.Ids).ExecuteDeleteAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Category);
		cache.DeleteAllByPartialKey(RouteTags.Product);
		cache.DeleteAllByPartialKey(RouteTags.User);
		return new UResponse();
	}

	public async Task<List<CategoryEntity>?> ReadEntity(CategoryReadParams p, CancellationToken ct) {
		IQueryable<CategoryEntity> q = db.Set<CategoryEntity>().AsTracking().OrderByDescending(x => x.Id);
		if (p.Ids.IsNotNullOrEmpty()) q = q.Where(x => p.Ids.Contains(x.Id));
		return await q.ToListAsync(ct);
	}

	private async Task AddChildrenRecursively(IEnumerable<CategoryCreateParams> children, Guid parentId, CancellationToken ct) {
		foreach (CategoryCreateParams childParams in children) {
			CategoryEntity childEntity = FillData(childParams, parentId);
			await db.Set<CategoryEntity>().AddAsync(childEntity, ct);
			if (childParams.Children.IsNotNullOrEmpty()) await AddChildrenRecursively(childParams.Children, childEntity.Id, ct);
		}
	}

	private static CategoryEntity FillData(CategoryCreateParams p, Guid? parentId = null) {
		CategoryEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			Title = p.Title,
			Code = p.Code,
			JsonData = new CategoryJson {
				Subtitle = p.Subtitle,
				Link = p.Link,
				Location = p.Location,
				Type = p.Type,
				Address = p.Address,
				PhoneNumber = p.PhoneNumber,
				RelatedProducts = p.RelatedProducts ?? []
			},
			Tags = p.Tags,
			Order = p.Order,
			ParentId = parentId ?? p.ParentId
		};
		return e;
	}

	private async Task AddMedia(Guid id, ICollection<Guid> ids, CancellationToken ct) {
		if (ids.IsNullOrEmpty()) return;
		List<MediaEntity> media = await mediaService.ReadEntity(new BaseReadParams<TagMedia> { Ids = ids }, ct) ?? [];
		if (media.Count == 0) return;
		foreach (MediaEntity i in media)
			await db.Set<MediaEntity>().Where(x => x.Id == i.Id)
				.ExecuteUpdateAsync(u => u.SetProperty(y => y.CategoryId, id), ct);
	}

	private static CategoryResponse ToDtoDeep(
		CategoryEntity e,
		bool loadChildren,
		bool loadMedia,
		bool loadChildrenMedia) {
		return new CategoryResponse {
			Id = e.Id,
			CreatedAt = e.CreatedAt,
			UpdatedAt = e.UpdatedAt,
			DeletedAt = e.DeletedAt,
			Tags = e.Tags,
			JsonData = e.JsonData,
			Title = e.Title,
			Order = e.Order,
			Code = e.Code,
			ParentId = e.ParentId,
			Media = loadMedia
				? e.Media.Select(m => new MediaResponse {
					Tags = m.Tags,
					JsonData = m.JsonData,
					Path = m.Path
				}).ToList()
				: [],

			Children = loadChildren
				? e.Children.Select(c => ToDtoDeep(
					c,
					loadChildren,
					loadChildrenMedia,
					loadChildrenMedia
				)).ToList()
				: []
		};
	}
}