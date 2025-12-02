namespace SinaMN75U.Services;

public interface IInvoiceService {
	Task<UResponse<InvoiceEntity?>> Create(InvoiceCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<InvoiceResponse>?>> Read(InvoiceReadParams p, CancellationToken ct);
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

	public async Task<UResponse<IEnumerable<InvoiceResponse>?>> Read(InvoiceReadParams p, CancellationToken ct) {
		IQueryable<InvoiceEntity> q = db.Set<InvoiceEntity>();

		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags.Any(tag => p.Tags.Contains(tag)));
		if (p.UserId.HasValue()) q = q.Where(x => x.UserId == p.UserId);
		if (p.FromCreatedAt.HasValue) q = q.Where(x => x.CreatedAt >= p.FromCreatedAt);
		if (p.ToCreatedAt.HasValue) q = q.Where(x => x.CreatedAt <= p.ToCreatedAt);
		
		IQueryable<InvoiceResponse> projected = q.Select(i => new InvoiceResponse {
			Id = i.Id,
			CreatedAt = i.CreatedAt,
			UpdatedAt = i.UpdatedAt,
			DeletedAt = i.DeletedAt,
			JsonData = i.JsonData,
			Tags = i.Tags,
			DebtAmount = i.DebtAmount,
			CreditorAmount = i.CreditorAmount,
			PaidAmount = i.PaidAmount,
			PenaltyAmount = i.PenaltyAmount,
			PaidDate = i.PaidDate,
			DueDate = i.DueDate,
			TrackingNumber = i.TrackingNumber,
			User = p.ShowUser
				? new UserResponse {
					Id = i.User.Id,
					UserName = i.User.UserName,
					PhoneNumber = i.User.PhoneNumber,
					FirstName = i.User.FirstName,
					LastName = i.User.LastName,
					JsonData = i.User.JsonData,
					Tags = i.User.Tags,
				}
				: null,
			Contract = p.ShowContract
				? new ContractResponse {
					Id = i.Contract.Id,
					StartDate = i.Contract.StartDate,
					EndDate = i.Contract.EndDate,
					Deposit = i.Contract.Deposit,
					Rent = i.Contract.Rent,
					UserId = i.Contract.UserId,
					CreatorId = i.Contract.CreatorId,
					ProductId = i.Contract.ProductId,
					JsonData = i.Contract.JsonData,
					Tags = i.Contract.Tags
				}
				: null
		});

		UResponse<IEnumerable<InvoiceResponse>?> response = await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);

		foreach (InvoiceResponse dto in response.Result!) {
			InvoiceEntity? entity = await db.Set<InvoiceEntity>().FindAsync([dto.Id], ct);
			if (entity == null) continue;

			int daysLate = Math.Max(0, (DateTime.UtcNow - entity.DueDate).Days);
			double expectedPenalty = entity.DebtAmount * 0.01 * daysLate;

			bool needsPenaltyUpdate =
				entity.PaidAmount < entity.DebtAmount + entity.PenaltyAmount &&
				entity.DueDate <= DateTime.UtcNow &&
				entity.PenaltyAmount < expectedPenalty;

			if (needsPenaltyUpdate) {
				entity.PenaltyAmount = expectedPenalty;
				db.Update(entity);
				await db.SaveChangesAsync(ct);
				dto.PenaltyAmount = expectedPenalty;
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