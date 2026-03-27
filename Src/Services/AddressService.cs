namespace SinaMN75U.Services;

public interface IAddressService {
	Task<UResponse<Guid?>> Create(AddressCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<AddressResponse>?>> Read(AddressReadParams p, CancellationToken ct);
	Task<UResponse> Update(AddressUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct);
}

public class AddressService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IAddressService {
	public async Task<UResponse<Guid?>> Create(AddressCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		AddressEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			JsonData = new AddressJson {
				Province = p.Province,
				Township = p.Township,
				Street = p.Street,
				Street2 = p.Street2,
				LocalityName = p.LocalityName,
				HouseNumber = p.HouseNumber,
				Floor = p.Floor,
				Description = p.Description
			},
			Tags = p.Tags,
			Title = p.Title,
			ZipCode = p.ZipCode,
			CreatorId = p.CreatorId ?? userData.Id
		};

		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<AddressResponse>?>> Read(AddressReadParams p, CancellationToken ct) {
		IQueryable<AddressResponse> q = db.Set<AddressEntity>().Select(Projections.AddressSelector(p.SelectorArgs));

		if (p.OrderByCreatedAt) q = q.OrderBy(x => x.CreatedAt);
		if (p.OrderByCreatedAtDesc) q = q.OrderByDescending(x => x.CreatedAt);
		if (p.OrderByUpdatedAt) q = q.OrderBy(x => x.UpdatedAt);
		if (p.OrderByUpdatedAtDesc) q = q.OrderByDescending(x => x.UpdatedAt);
		if (p.CreatorId != null) q = q.Where(x => x.CreatorId == p.CreatorId);

		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => p.Tags.All(tag => x.Tags.Contains(tag)));
		if (p.Ids.IsNotNullOrEmpty()) q = q.Where(x => p.Ids.Contains(x.Id));

		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(AddressUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		AddressEntity? e = await db.Set<AddressEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("AddressNotFound"));

		if (p.Title != null) e.Title = p.Title;
		if (p.ZipCode != null) e.ZipCode = p.ZipCode;
		if (p.Province != null) e.JsonData.Province = p.Province;
		if (p.Township != null) e.JsonData.Township = p.Township;
		if (p.Street != null) e.JsonData.Street = p.Street;
		if (p.Street2 != null) e.JsonData.Street2 = p.Street2;
		if (p.LocalityName != null) e.JsonData.LocalityName = p.LocalityName;
		if (p.HouseNumber != null) e.JsonData.HouseNumber = p.HouseNumber;
		if (p.Floor != null) e.JsonData.Floor = p.Floor;
		if (p.Description != null) e.JsonData.Description = p.Description;
		if (p.Tags != null) e.Tags = p.Tags;

		db.Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<AddressEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<AddressEntity>().Where(x => p.Id == x.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.DeletedAt, p.DateTime ?? DateTime.UtcNow), ct);
		return new UResponse();
	}
}