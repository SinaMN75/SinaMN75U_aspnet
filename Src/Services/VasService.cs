namespace SinaMN75U.Services;

public interface IVasService {
	Task<UResponse<Guid?>> Create(VasCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<VasResponse>?>> Read(VasReadParams p, CancellationToken ct);
}

public class VasService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IVasService {
	public async Task<UResponse<Guid?>> Create(VasCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		VasEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			Amount = p.Amount,
			AuthorizeCode = p.AuthorizeCode,
			OrganizationType = p.OrganizationType,
			OrganizationName = p.OrganizationName,
			BillId = p.BillId,
			PaymentId = p.PaymentId,
			TxnId = p.TxnId,
			WalletTxnId = p.WalletTxnId,
			CreatedAt = DateTime.UtcNow,
			JsonData = new BaseJsonData { Detail1 = p.Detail1, Detail2 = p.Detail2 },
			Tags = p.Tags
		};

		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<VasResponse>?>> Read(VasReadParams p, CancellationToken ct) {
		IQueryable<VasEntity> q = db.Set<VasEntity>().ApplyReadParams<VasEntity, TagVas, BaseJsonData>(p);

		if (p.AuthorizeCode.IsNotNullOrEmpty()) q = q.Where(x => x.AuthorizeCode == p.AuthorizeCode);
		if (p.BillId.IsNotNullOrEmpty()) q = q.Where(x => x.BillId == p.BillId);
		if (p.PaymentId.IsNotNullOrEmpty()) q = q.Where(x => x.PaymentId == p.PaymentId);
		if (p.OrganizationType.IsNotNullOrEmpty()) q = q.Where(x => x.OrganizationType == p.OrganizationType);
		if (p.WalletTxnId.HasValue) q = q.Where(x => x.WalletTxnId == p.WalletTxnId);
		if (p.TxnId.HasValue) q = q.Where(x => x.TxnId == p.TxnId);

		IQueryable<VasResponse> projected = q.Select(Projections.VasSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}
}