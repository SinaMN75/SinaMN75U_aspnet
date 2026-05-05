namespace SinaMN75U.Services;

public interface IBankAccountService {
	Task<UResponse<Guid?>> Create(BankAccountCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<BankAccountResponse>?>> Read(BankAccountReadParams p, CancellationToken ct);
	Task<UResponse> Update(BankAccountUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
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
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJsonData { Detail1 = p.Detail1, Detail2 = p.Detail2 },
			Tags = p.Tags,
			CardNumber = p.CardNumber,
			AccountNumber = p.AccountNumber,
			IBanNumber = p.IBanNumber,
			BankName = p.BankName,
			OwnerName = p.OwnerName
		};

		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<BankAccountResponse>?>> Read(BankAccountReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<BankAccountResponse>?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		if (!userData.IsAdmin) return new UResponse<IEnumerable<BankAccountResponse>?>(null, Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		IQueryable<BankAccountEntity> q = db.Set<BankAccountEntity>().ApplyReadParams<BankAccountEntity, TagBankAccount, BaseJsonData>(p);
		
		if (p.CardNumber.IsNotNullOrEmpty()) q = q.Where(x => x.CardNumber == p.CardNumber);
		if (p.OwnerName.IsNotNullOrEmpty()) q = q.Where(x => x.OwnerName == p.OwnerName);
		if (p.IBanNumber.IsNotNullOrEmpty()) q = q.Where(x => x.BankName == p.BankName);
		if (p.AccountNumber.IsNotNullOrEmpty()) q = q.Where(x => x.AccountNumber == p.AccountNumber);
		if (p.BankName.IsNotNullOrEmpty()) q = q.Where(x => x.BankName == p.BankName);
		
		IQueryable<BankAccountResponse> projected = q.Select(Projections.BankAccountSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(BankAccountUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		BankAccountEntity? e = await db.Set<BankAccountEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("BankAccountNotFound"));
		if (!userData.IsAdmin && userData.Id != e.CreatorId) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));
		
		if (p.IBanNumber.IsNotNullOrEmpty()) e.IBanNumber = p.IBanNumber;
		if (p.AccountNumber.IsNotNullOrEmpty()) e.AccountNumber = p.AccountNumber;
		if (p.CardNumber.IsNotNullOrEmpty()) e.CardNumber = p.CardNumber;
		if (p.OwnerName.IsNotNullOrEmpty()) e.OwnerName = p.OwnerName;
		if (p.BankName.IsNotNullOrEmpty()) e.BankName = p.BankName;

		db.Set<BankAccountEntity>().Update(e.ApplyUpdateParam<BankAccountEntity,TagBankAccount, BaseJsonData>(p));
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		
		BankAccountEntity? e = await db.Set<BankAccountEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("AddressNotFound"));

		if (!userData.IsAdmin && userData.Id != e.CreatorId) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		db.Set<BankAccountEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		
		return new UResponse(Usc.Deleted, ls.Get("AddressDeletedSuccessfully"));
	}
}