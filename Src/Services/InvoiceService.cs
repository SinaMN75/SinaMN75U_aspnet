namespace SinaMN75U.Services;

public interface IInvoiceService {
	Task<UResponse<InvoiceEntity?>> Create(InvoiceCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<InvoiceEntity>>> Read(InvoiceReadParams p, CancellationToken ct);
	Task<UResponse<InvoiceEntity?>> Update(InvoiceUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);

	Task<UResponse> Pay(IdParams p, CancellationToken ct);
}

public class InvoiceService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	ILocalStorageService cache
) : IInvoiceService {
	public async Task<UResponse<InvoiceEntity?>> Create(InvoiceCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<InvoiceEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		EntityEntry<InvoiceEntity> e = await db.AddAsync(new InvoiceEntity {
			Tags = p.Tags,
			DebtAmount = p.DebtAmount,
			CreditorAmount = p.CreditorAmount,
			PaidAmount = p.PaidAmount,
			PenaltyAmount = p.PenaltyAmount,
			UserId = p.UserId,
			ContractId = p.ContractId,
			DueDate = p.DueDate,
			PaidDate = p.PaidDate,
			JsonData = new InvoiceJson {
				Description = p.Description,
			}
		}, ct);
		await db.SaveChangesAsync(ct);
		
		cache.DeleteAllByPartialKey(RouteTags.Invoice);
		return new UResponse<InvoiceEntity?>(e.Entity);
	}

	public async Task<UResponse<IEnumerable<InvoiceEntity>>> Read(InvoiceReadParams p, CancellationToken ct) {
		IQueryable<InvoiceEntity> q = db.Set<InvoiceEntity>().AsTracking();

		if (p.ShowContract) q = q.Include(x => x.Contract);
		if (p.ShowUser) q = q.Include(x => x.User);
		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags.Any(tag => p.Tags.Contains(tag)));
		if (p.UserId.HasValue()) q = q.Where(x => x.UserId == p.UserId);
		if (p.FromCreatedAt.HasValue) q = q.Where(x => x.CreatedAt >= p.FromCreatedAt);
		if (p.ToCreatedAt.HasValue) q = q.Where(x => x.CreatedAt <= p.ToCreatedAt);

		UResponse<IEnumerable<InvoiceEntity>> response = (await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct))!;

		foreach (InvoiceEntity i in response.Result!) {
			int daysLate = Math.Max(0, (DateTime.UtcNow - i.DueDate).Days);
			double expectedPenalty = i.DebtAmount * 0.01 * daysLate;
			bool needsPenaltyUpdate = i.PaidAmount < i.DebtAmount + i.PenaltyAmount &&
			                          i.DueDate <= DateTime.UtcNow &&
			                          i.PenaltyAmount < expectedPenalty;

			if (needsPenaltyUpdate) {
				i.PenaltyAmount = i.DebtAmount * (0.01 * daysLate);
				db.Update(i);
				await db.SaveChangesAsync(ct);
			}
		}

		return response;
	}

	public async Task<UResponse<InvoiceEntity?>> Update(InvoiceUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<InvoiceEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		InvoiceEntity e = (await db.Set<InvoiceEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (p.CreditorAmount.IsNotNull()) e.CreditorAmount = p.CreditorAmount.Value;
		if (p.DebtAmount.IsNotNull()) e.DebtAmount = p.DebtAmount.Value;
		if (p.PenaltyAmount.IsNotNull()) e.PenaltyAmount = p.PenaltyAmount.Value;
		if (p.PaidAmount.IsNotNull()) e.PaidAmount = p.PaidAmount.Value;
		if (p.PaidDate.HasValue) e.PaidDate = p.PaidDate.Value;
		if (p.DueDate.HasValue) e.DueDate = p.DueDate.Value;
		if (p.UserId.HasValue()) e.UserId = p.UserId.Value;
		if (p.ContractId.HasValue()) e.ContractId = p.ContractId.Value;
		if (p.Description.HasValue()) e.JsonData.Description = p.Description;

		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => p.RemoveTags.Contains(x));
		if (p.Tags.IsNotNullOrEmpty()) e.Tags = p.Tags;

		db.Update(e);
		await db.SaveChangesAsync(ct);
		
		cache.DeleteAllByPartialKey(RouteTags.Invoice);
		return new UResponse<InvoiceEntity?>(e);
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<InvoiceEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<InvoiceEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);
		
		cache.DeleteAllByPartialKey(RouteTags.Invoice);
		return new UResponse();
	}

	public async Task<UResponse> Pay(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		InvoiceEntity? e = await db.Set<InvoiceEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("InvoiceNotFound"));

		e.PaidAmount = e.DebtAmount - e.CreditorAmount;
		e.Tags = [TagInvoice.PaidOnline];
		e.PaidDate = DateTime.UtcNow;
		e.TrackingNumber = "123456789";
		db.Update(e);

		await db.SaveChangesAsync(ct);
		
		cache.DeleteAllByPartialKey(RouteTags.Invoice);
		return new UResponse();
	}
}