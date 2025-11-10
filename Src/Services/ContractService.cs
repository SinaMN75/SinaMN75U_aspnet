namespace SinaMN75U.Services;

public interface IContractService {
	Task<UResponse<ContractEntity?>> Create(ContractCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<ContractEntity>?>> Read(ContractReadParams p, CancellationToken ct);
	Task<UResponse<ContractEntity?>> Update(ContractUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class ContractService(DbContext db, ILocalizationService ls, ITokenService ts) : IContractService {
	public async Task<UResponse<ContractEntity?>> Create(ContractCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ContractEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ProductEntity? product = await db.Set<ProductEntity>().FirstOrDefaultAsync(x => x.Id == p.ProductId, ct);
		if (product == null) return new UResponse<ContractEntity?>(null, Usc.NotFound, ls.Get("ProductNotFound"));

		UserEntity? user = await db.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == p.UserId, ct);
		if (user == null) return new UResponse<ContractEntity?>(null, Usc.NotFound, ls.Get("UserNotFound"));

		Guid contractId = Guid.CreateVersion7();
		EntityEntry<ContractEntity> e = await db.AddAsync(new ContractEntity {
			Id = contractId,
			StartDate = p.StartDate,
			EndDate = p.EndDate,
			Price1 = p.Price1,
			Price2 = p.Price2,
			UserId = user.Id,
			CreatorId = userData.Id,
			ProductId = product.Id,
			JsonData = new ContractJson {
				Description = p.Description
			},
			Tags = p.Tags
		}, ct);

		if (product.Price1 != null)
			await db.Set<InvoiceEntity>().AddAsync(new InvoiceEntity {
				Tags = [TagInvoice.NotPaid, TagInvoice.Deposit],
				DebtAmount = product.Price1!.Value,
				CreditorAmount = 0,
				PaidAmount = 0,
				PenaltyAmount = 0,
				UserId = user.Id,
				ContractId = contractId,
				DueDate = p.StartDate,
				JsonData = new InvoiceJson { Description = "" }
			}, ct);

		if (product.Price2 != null) {
			bool hasNextInvoice = (p.EndDate - p.StartDate).TotalDays >= 32;
			DateTime nextInvoiceIssueDate = p.StartDate.AddMonths(1).AddDays(-1);
			nextInvoiceIssueDate = new DateTime(
				nextInvoiceIssueDate.Year,
				nextInvoiceIssueDate.Month,
				nextInvoiceIssueDate.Day,
				10,
				30,
				0,
				nextInvoiceIssueDate.Kind
			);
			await db.Set<InvoiceEntity>().AddAsync(new InvoiceEntity {
				Tags = [TagInvoice.NotPaid, TagInvoice.Rent],
				DebtAmount = product.Price2!.Value,
				CreditorAmount = 0,
				PaidAmount = 0,
				PenaltyAmount = 0,
				UserId = user.Id,
				ContractId = contractId,
				DueDate = p.StartDate,
				NextInvoiceIssueDate = hasNextInvoice ? nextInvoiceIssueDate : null,
				JsonData = new InvoiceJson { Description = "" },
			}, ct);
		}

		await db.SaveChangesAsync(ct);
		return new UResponse<ContractEntity?>(e.Entity);
	}

	public async Task<UResponse<IEnumerable<ContractEntity>?>> Read(ContractReadParams p, CancellationToken ct) {
		IQueryable<ContractEntity> q = db.Set<ContractEntity>();

		if (p.Tags != null) q = q.Where(u => u.Tags.Any(tag => p.Tags.Contains(tag)));
		if (p.CreatorId.IsNotNullOrEmpty()) q = q.Where(u => u.CreatorId == p.CreatorId);
		if (p.UserId.IsNotNullOrEmpty()) q = q.Where(u => u.UserId == p.UserId);
		if (p.ProductId.IsNotNullOrEmpty()) q = q.Where(u => u.ProductId == p.ProductId);
		if (p.StartDate.HasValue) q = q.Where(u => u.StartDate == p.StartDate);
		if (p.EndDate.HasValue) q = q.Where(u => u.EndDate == p.EndDate);
		if (p.FromCreatedAt.HasValue) q = q.Where(u => u.CreatedAt >= p.FromCreatedAt);
		if (p.ToCreatedAt.HasValue) q = q.Where(u => u.CreatedAt <= p.ToCreatedAt);

		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<ContractEntity?>> Update(ContractUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ContractEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		ContractEntity e = (await db.Set<ContractEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct))!;
		e.UpdatedAt = DateTime.UtcNow;
		if (p.Price1.HasValue) e.Price1 = p.Price1.Value;
		if (p.Price2.HasValue) e.Price2 = p.Price2.Value;
		if (p.StartDate.HasValue) e.StartDate = p.StartDate.Value;
		if (p.EndDate.HasValue) e.EndDate = p.EndDate.Value;

		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(x => p.RemoveTags.Contains(x));
		if (p.Tags.IsNotNullOrEmpty()) e.Tags = p.Tags;

		db.Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse<ContractEntity?>(e);
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<ContractEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<ContractEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}
}