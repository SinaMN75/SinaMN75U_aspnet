namespace SinaMN75U.Services;

public interface IInvoiceService {
	Task<UResponse<InvoiceEntity?>> Create(InvoiceCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<InvoiceEntity>?>> Read(InvoiceReadParams p, CancellationToken ct);
	Task<UResponse<InvoiceEntity?>> Update(InvoiceUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class InvoiceService(DbContext db, ILocalizationService ls, ITokenService ts) : IInvoiceService {
	public async Task<UResponse<InvoiceEntity?>> Create(InvoiceCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<InvoiceEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		EntityEntry<InvoiceEntity> e = await db.AddAsync(new InvoiceEntity {
			Tags = p.Tags,
			DebtAmount = p.DebtAmount,
			CreditorAmount = p.CreditorAmount,
			SettlementAmount = p.SettlementAmount,
			PenaltyAmount = p.PenaltyAmount,
			UserId = p.UserId,
			JsonData = new InvoiceJson {
				Description = p.Description,
			}
		}, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<InvoiceEntity?>(e.Entity);
	}

	public async Task<UResponse<IEnumerable<InvoiceEntity>?>> Read(InvoiceReadParams p, CancellationToken ct) {
		IQueryable<InvoiceEntity> q = db.Set<InvoiceEntity>();

		if (p.Tags != null) q = q.Where(u => u.Tags.Any(tag => p.Tags.Contains(tag)));
		if (p.UserId.IsNotNullOrEmpty()) q = q.Where(u => u.UserId == p.UserId);
		if (p.FromCreatedAt.HasValue) q = q.Where(u => u.CreatedAt >= p.FromCreatedAt);
		if (p.ToCreatedAt.HasValue) q = q.Where(u => u.CreatedAt <= p.ToCreatedAt);

		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<InvoiceEntity?>> Update(InvoiceUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<InvoiceEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		InvoiceEntity e = (await db.Set<InvoiceEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (p.CreditorAmount.HasValue) e.CreditorAmount = p.CreditorAmount.Value;
		if (p.DebtAmount.HasValue) e.DebtAmount = p.DebtAmount.Value;
		if (p.PenaltyAmount.HasValue) e.PenaltyAmount = p.PenaltyAmount.Value;
		if (p.SettlementAmount.HasValue) e.SettlementAmount = p.SettlementAmount.Value;

		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => p.RemoveTags.Contains(x));
		if (p.Tags.IsNotNullOrEmpty()) e.Tags = p.Tags;

		db.Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<InvoiceEntity?>(e);
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<InvoiceEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<InvoiceEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}
}