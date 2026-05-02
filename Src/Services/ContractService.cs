namespace SinaMN75U.Services;

public interface IContractService {
	Task<UResponse<Guid?>> Create(ContractCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ContractResponse>?>> Read(ContractReadParams p, CancellationToken ct);
	Task<UResponse> Update(ContractUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class ContractService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IContractService {
	public async Task<UResponse<Guid?>> Create(ContractCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ProductEntity? product = await db.Set<ProductEntity>().Include(x => x.Contracts).FirstOrDefaultAsync(x => x.Id == p.ProductId, ct);
		if (product?.Deposit == null || product.Rent == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("ProductNotFound"));
		if (product.Contracts.Any(y => y.EndDate >= DateTime.UtcNow)) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("ProductHasActiveContract"));

		UserEntity? user = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == p.UserId, ct);
		if (user == null) return new UResponse<Guid?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		Guid contractId = Guid.CreateVersion7();
		ContractEntity e = new() {
			Id = contractId,
			CreatedAt = DateTime.UtcNow,
			StartDate = p.StartDate,
			EndDate = p.EndDate,
			Deposit = p.Deposit ?? product.Deposit ?? 0,
			Rent = p.Rent ?? product.Rent ?? 0,
			UserId = user.Id,
			CreatorId = p.CreatorId ?? userData.Id,
			ProductId = product.Id,
			JsonData = new BaseJsonData { Detail2 = p.Detail2},
			Tags = p.Tags
		};
		await db.Set<ContractEntity>().AddAsync(e, ct);

		if (p.Tags.Contains(TagContract.SingleInvoice)) {
			await db.Set<InvoiceEntity>().AddAsync(new InvoiceEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = p.CreatorId ?? userData.Id,
				CreatedAt = DateTime.UtcNow,
				Tags = [TagInvoice.NotPaid],
				DebtAmount = e.Deposit + e.Rent,
				CreditorAmount = 0,
				PaidAmount = 0,
				PenaltyAmount = 0,
				ContractId = contractId,
				DueDate = p.StartDate,
				JsonData = new InvoiceJson { Detail1 = "", PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate }
			}, ct);

			await db.SaveChangesAsync(ct);
			return new UResponse<Guid?>(e.Id);
		}

		if (e.Deposit >= 1)
			await db.Set<InvoiceEntity>().AddAsync(new InvoiceEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = p.CreatorId ?? userData.Id,
				CreatedAt = DateTime.UtcNow,
				Tags = [TagInvoice.NotPaid, TagInvoice.Deposit],
				DebtAmount = product.Deposit ?? 0,
				CreditorAmount = 0,
				PaidAmount = 0,
				PenaltyAmount = 0,
				ContractId = contractId,
				DueDate = p.StartDate,
				JsonData = new InvoiceJson {
					Detail1 = "ودیعه",
					PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate
				}
			}, ct);

		PersianDateTime startDate = e.StartDate.ToPersian();
		PersianDateTime endDate = e.EndDate.ToPersian();

		decimal rent = product.Rent ?? 0;

		int totalMonths = (endDate.Year - startDate.Year) * 12 + (endDate.Month - startDate.Month);
		if (endDate.Day < startDate.Day) totalMonths--;

		await db.Set<InvoiceEntity>().AddAsync(new InvoiceEntity {
			Id = Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = [TagInvoice.NotPaid, TagInvoice.Rent],
			DebtAmount = rent,
			CreditorAmount = 0,
			PaidAmount = 0,
			PenaltyAmount = 0,
			ContractId = contractId,
			DueDate = startDate.ToDateTime(),
			JsonData = new InvoiceJson {
				Detail2 = "قسط اول - قیمت کامل",
				PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate
			}
		}, ct);

		if (totalMonths >= 1) {
			int remainingDaysInFirstMonth = PersianDateTime.DaysInMonth(startDate.Year, startDate.Month) - startDate.Day + 1;
			int totalDaysInFirstMonth = PersianDateTime.DaysInMonth(startDate.Year, startDate.Month);
			decimal proportionalPrice = remainingDaysInFirstMonth / (decimal)totalDaysInFirstMonth * rent;

			await db.Set<InvoiceEntity>().AddAsync(new InvoiceEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = p.CreatorId ?? userData.Id,
				CreatedAt = DateTime.UtcNow,
				Tags = [TagInvoice.NotPaid, TagInvoice.Rent],
				DebtAmount = Math.Round(proportionalPrice, 2),
				CreditorAmount = 0,
				PaidAmount = 0,
				PenaltyAmount = 0,
				ContractId = contractId,
				DueDate = startDate.AddMonths(1).ToDateTime(),
				JsonData = new InvoiceJson {
					Detail2 = $"قسط دوم - قیمت متناسب ({remainingDaysInFirstMonth} روز از {totalDaysInFirstMonth} روز)",
					PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate
				}
			}, ct);
		}

		for (int i = 2; i <= totalMonths; i++) {
			PersianDateTime firstOfMonth = startDate.AddMonths(i).StartOfMonth;

			await db.Set<InvoiceEntity>().AddAsync(new InvoiceEntity {
				Id = Guid.CreateVersion7(),
				CreatorId = p.CreatorId ?? userData.Id,
				CreatedAt = DateTime.UtcNow,
				Tags = [TagInvoice.NotPaid, TagInvoice.Rent],
				DebtAmount = rent,
				CreditorAmount = 0,
				PaidAmount = 0,
				PenaltyAmount = 0,
				ContractId = contractId,
				DueDate = firstOfMonth.ToDateTime(),
				JsonData = new InvoiceJson {
					Detail2 = $"قسط {i + 1} - قیمت کامل",
					PenaltyPrecentEveryDate = p.PenaltyPrecentEveryDate
				}
			}, ct);
		}

		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<ContractResponse>?>> Read(ContractReadParams p, CancellationToken ct) {
		IQueryable<ContractEntity> q = db.Set<ContractEntity>().ApplyReadParams<ContractEntity, TagContract, BaseJsonData>(p);

		if (p.UserId.IsNotNull()) q = q.Where(u => u.UserId == p.UserId);
		if (p.ProductId.IsNotNull()) q = q.Where(u => u.ProductId == p.ProductId);
		if (p.UserName.IsNotNullOrEmpty()) q = q.Include(x => x.User).Where(x => x.User.UserName.Contains(p.UserName));
		if (p.StartDate.HasValue) q = q.Where(u => u.StartDate <= p.StartDate);
		if (p.EndDate.HasValue) q = q.Where(u => u.EndDate >= p.EndDate);

		IQueryable<ContractResponse> list = q.Select(Projections.ContractSelector(p.SelectorArgs));

		return await list.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(ContractUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ContractEntity e = (await db.Set<ContractEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		
		if (p.Deposit.HasValue) e.Deposit = p.Deposit.Value;
		if (p.Rent.HasValue) e.Rent = p.Rent.Value;
		if (p.StartDate.HasValue) e.StartDate = p.StartDate.Value;
		if (p.EndDate.HasValue) e.EndDate = p.EndDate.Value;
		db.Set<ContractEntity>().Update(e.ApplyUpdateParam<ContractEntity,TagContract, BaseJsonData>(p));
		await db.SaveChangesAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<ContractEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}
}