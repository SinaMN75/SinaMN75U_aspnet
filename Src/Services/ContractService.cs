using SinaMN75U.Data;

namespace SinaMN75U.Services;

public interface IContractService {
	Task<UResponse<ContractResponse?>> Create(ContractCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ContractResponse>?>> Read(ContractReadParams p, CancellationToken ct);
	Task<UResponse<ContractResponse?>> Update(ContractUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class ContractService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	ILocalStorageService cache
) : IContractService {
	public async Task<UResponse<ContractResponse?>> Create(ContractCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ContractResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ProductEntity? product = await db.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == p.ProductId, ct);
		if (product?.Deposit == null || product.Rent == null) return new UResponse<ContractResponse?>(null, Usc.NotFound, ls.Get("ProductNotFound"));

		UserEntity? user = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == p.UserId, ct);
		if (user == null) return new UResponse<ContractResponse?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		Guid contractId = Guid.CreateVersion7();
		ContractEntity e = new() {
			Id = contractId,
			StartDate = p.StartDate,
			EndDate = p.EndDate,
			Deposit = product.Deposit ?? 0,
			Rent = product.Rent ?? 0,
			UserId = user.Id,
			CreatorId = userData.Id,
			ProductId = product.Id,
			JsonData = new ContractJson { Description = p.Description },
			Tags = p.Tags
		};
		await db.Set<ContractEntity>().AddAsync(e, ct);

		await db.Set<InvoiceEntity>().AddAsync(new InvoiceEntity {
			Tags = [TagInvoice.NotPaid, TagInvoice.Deposit],
			DebtAmount = product.Deposit ?? 0,
			CreditorAmount = 0,
			PaidAmount = 0,
			PenaltyAmount = 0,
			UserId = user.Id,
			ContractId = contractId,
			DueDate = p.StartDate,
			JsonData = new InvoiceJson { Description = "ودیعه" }
		}, ct);

		PersianDateTime startDate = e.StartDate.ToPersian();
		PersianDateTime endDate = e.EndDate.ToPersian();

		double rent = product.Rent ?? 0;

		int totalMonths = (endDate.Year - startDate.Year) * 12 + (endDate.Month - startDate.Month);
		if (endDate.Day < startDate.Day) {
			totalMonths--;
		}

		await db.Set<InvoiceEntity>().AddAsync(new InvoiceEntity {
			Tags = [TagInvoice.NotPaid, TagInvoice.Rent],
			DebtAmount = rent,
			CreditorAmount = 0,
			PaidAmount = 0,
			PenaltyAmount = 0,
			UserId = user.Id,
			ContractId = contractId,
			DueDate = startDate.ToDateTime(),
			JsonData = new InvoiceJson { Description = "قسط اول - قیمت کامل" },
		}, ct);

		if (totalMonths >= 1) {
			int remainingDaysInFirstMonth = PersianDateTime.DaysInMonth(startDate.Year, startDate.Month) - startDate.Day + 1;
			int totalDaysInFirstMonth = PersianDateTime.DaysInMonth(startDate.Year, startDate.Month);
			double proportionalPrice = (remainingDaysInFirstMonth / (double)totalDaysInFirstMonth) * rent;

			await db.Set<InvoiceEntity>().AddAsync(new InvoiceEntity {
				Tags = [TagInvoice.NotPaid, TagInvoice.Rent],
				DebtAmount = Math.Round(proportionalPrice, 2),
				CreditorAmount = 0,
				PaidAmount = 0,
				PenaltyAmount = 0,
				UserId = user.Id,
				ContractId = contractId,
				DueDate = startDate.AddMonths(1).ToDateTime(),
				JsonData = new InvoiceJson { Description = $"قسط دوم - قیمت متناسب ({remainingDaysInFirstMonth} روز از {totalDaysInFirstMonth} روز)" },
			}, ct);
		}

		for (int i = 2; i <= totalMonths; i++) {
			PersianDateTime firstOfMonth = startDate.AddMonths(i).StartOfMonth;

			await db.Set<InvoiceEntity>().AddAsync(new InvoiceEntity {
				Tags = [TagInvoice.NotPaid, TagInvoice.Rent],
				DebtAmount = rent,
				CreditorAmount = 0,
				PaidAmount = 0,
				PenaltyAmount = 0,
				UserId = user.Id,
				ContractId = contractId,
				DueDate = firstOfMonth.ToDateTime(),
				JsonData = new InvoiceJson { Description = $"قسط {i + 1} - قیمت کامل" },
			}, ct);
		}

		await db.SaveChangesAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Contract);
		cache.DeleteAllByPartialKey(RouteTags.Invoice);
		return new UResponse<ContractResponse?>(e.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<ContractResponse>?>> Read(ContractReadParams p, CancellationToken ct) {
		IQueryable<ContractEntity> q = db.Set<ContractEntity>();

		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(u => u.Tags.Any(tag => p.Tags.Contains(tag)));
		if (StringExtensions.HasValue(p.CreatorId)) q = q.Where(u => u.CreatorId == p.CreatorId);
		if (StringExtensions.HasValue(p.UserId)) q = q.Where(u => u.UserId == p.UserId);
		if (StringExtensions.HasValue(p.ProductId)) q = q.Where(u => u.ProductId == p.ProductId);
		if (p.StartDate.HasValue) q = q.Where(u => u.StartDate == p.StartDate);
		if (p.EndDate.HasValue) q = q.Where(u => u.EndDate == p.EndDate);
		if (p.FromCreatedAt.HasValue) q = q.Where(u => u.CreatedAt >= p.FromCreatedAt);
		if (p.ToCreatedAt.HasValue) q = q.Where(u => u.CreatedAt <= p.ToCreatedAt);
		if (p.UserName.HasValue()) q = q.Include(x => x.User).Where(x => x.User.UserName.Contains(p.UserName));
		
		IQueryable<ContractResponse> list = q.Select(Projections.ContractSelector(p.SelectorArgs));

		return await list.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ContractResponse?>> Update(ContractUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ContractResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ContractEntity e = (await db.Set<ContractEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (p.Deposit.HasValue) e.Deposit = p.Deposit.Value;
		if (p.Rent.HasValue) e.Rent = p.Rent.Value;
		if (p.StartDate.HasValue) e.StartDate = p.StartDate.Value;
		if (p.EndDate.HasValue) e.EndDate = p.EndDate.Value;

		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => p.RemoveTags.Contains(x));
		if (p.Tags.IsNotNullOrEmpty()) e.Tags = p.Tags;

		db.Update(e);
		await db.SaveChangesAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Contract);
		cache.DeleteAllByPartialKey(RouteTags.Invoice);
		return new UResponse<ContractResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ContractEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<ContractEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		cache.DeleteAllByPartialKey(RouteTags.Contract);
		cache.DeleteAllByPartialKey(RouteTags.Invoice);
		return new UResponse();
	}
}