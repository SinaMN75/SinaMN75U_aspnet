namespace SinaMN75U.Services;

public interface IInvoiceService {
	Task<UResponse<InvoiceEntity?>> Create(InvoiceCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<InvoiceEntity>?>> Read(InvoiceReadParams p, CancellationToken ct);
	Task<UResponse<InvoiceEntity?>> Update(InvoiceUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);

	Task<UResponse> Pay(IdParams p, CancellationToken ct);

	Task<UResponse> CheckDueDate(CancellationToken ct);
	Task<UResponse> CheckPenalty(CancellationToken ct);
}

public class InvoiceService(DbContext db, ILocalizationService ls, ITokenService ts) : IInvoiceService {
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
		if (p.PaidAmount.HasValue) e.PaidAmount = p.PaidAmount.Value;

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
		return new UResponse();
	}

	public async Task<UResponse> CheckDueDate(CancellationToken ct) {
		IQueryable<InvoiceEntity> invoices = db.Set<InvoiceEntity>().AsTracking()
			.Where(x => x.NextInvoiceIssueDate != null && DateTime.UtcNow >= x.NextInvoiceIssueDate)
			.Include(x => x.Contract);

		foreach (InvoiceEntity invoice in invoices) {
			PersianDateTime date = invoice.NextInvoiceIssueDate!.Value.ToPersian();
			int daysInMonth = PersianDateTime.UtcNow.DaysInMonth();
			int daysUntilEndOfMonth = daysInMonth - date.Day;
			double debtAmount = invoice.Contract.Price2;

			DateTime dueDate = invoice.DueDate;
			if (daysUntilEndOfMonth <= 2) {
				debtAmount = (invoice.Contract.Price2 / daysInMonth) * daysUntilEndOfMonth;
				PersianDateTime now = PersianDateTime.Now;
				dueDate = new PersianDateTime(now.Year, now.Month, daysInMonth).ToDateTime();
			}

			await db.Set<InvoiceEntity>().AddAsync(
				new InvoiceEntity {
					Tags = [TagInvoice.NotPaid, TagInvoice.Rent],
					DebtAmount = debtAmount,
					CreditorAmount = 0,
					PaidAmount = 0,
					PenaltyAmount = 0,
					UserId = invoice.UserId,
					ContractId = invoice.ContractId,
					DueDate = dueDate,
					JsonData = new InvoiceJson { Description = "" }
				},
				ct
			);
		}

		return new UResponse();
	}

	public async Task<UResponse> CheckPenalty(CancellationToken ct) {
		IQueryable<InvoiceEntity> invoices = db.Set<InvoiceEntity>().AsTracking()
			.Where(x => DateTime.UtcNow >= x.DueDate.AddDays(2));

		foreach (InvoiceEntity invoice in invoices) {
			invoice.PenaltyAmount += invoice.DebtAmount * 1 / 100;
			invoice.DebtAmount += invoice.PenaltyAmount;
			db.Set<InvoiceEntity>().Update(invoice);
			await db.SaveChangesAsync(ct);
		}

		return new UResponse();
	}
}