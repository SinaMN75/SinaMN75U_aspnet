namespace SinaMN75U.Services;

public interface IBankAccountService {
	Task<UResponse<Guid?>> Create(BankAccountCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<BankAccountResponse>?>> Read(BankAccountReadParams p, CancellationToken ct);
	Task<UResponse> Update(BankAccountUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct);
}

public class BankAccountService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IBankAccountService {
	public async Task<UResponse<Guid?>> Create(BankAccountCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		BankAccountEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			JsonData = new GeneralJsonData { Description = p.Description ?? "" },
			Tags = p.Tags,
			CardNumber = p.CardNumber,
			AccountNumber = p.AccountNumber,
			IBanNumber = p.IBanNumber,
			BankName = p.BankName,
			OwnerName = p.OwnerName,
			UserId = p.UserId ?? userData.Id
		};

		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<BankAccountResponse>?>> Read(BankAccountReadParams p, CancellationToken ct) {
		IQueryable<BankAccountResponse> q = db.Set<BankAccountEntity>().Select(Projections.BankAccountSelector(p.SelectorArgs));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(BankAccountUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		BankAccountEntity? e = await db.Set<BankAccountEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("BankAccountNotFound"));

		if (p.IBanNumber != null) e.IBanNumber = p.IBanNumber;
		if (p.AccountNumber != null) e.AccountNumber = p.AccountNumber;
		if (p.CardNumber != null) e.CardNumber = p.CardNumber;
		if (p.OwnerName != null) e.OwnerName = p.OwnerName;
		if (p.BankName != null) e.BankName = p.BankName;
		if (p.Description != null) e.JsonData.Description = p.Description;
		if (p.Tags != null) e.Tags = p.Tags;
		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));

		e.UpdatedAt = DateTime.UtcNow;
		db.Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<BankAccountEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		await db.Set<BankAccountEntity>().Where(x => p.Id == x.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.DeletedAt, p.DateTime ?? DateTime.UtcNow), ct);
		return new UResponse();
	}
}