namespace SinaMN75U.Services;

public interface IAddressService {
	Task<UResponse<AddressResponse?>> Create(AddressCreateParams p, CancellationToken ct);
	Task<UResponse<AddressResponse?>> CreateFromZipCode(AddressCreateFromZipCodeParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<AddressResponse>?>> Read(AddressReadParams p, CancellationToken ct);
	Task<UResponse<AddressResponse?>> Update(AddressUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct);
}

public class AddressService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts,
	IITHubService itHubService
) : IAddressService {
	public async Task<UResponse<AddressResponse?>> Create(AddressCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<AddressResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		EntityEntry<AddressEntity> e = await db.AddAsync(p.MapToEntity(), ct);

		await db.SaveChangesAsync(ct);
		return new UResponse<AddressResponse?>(e.Entity.MapToResponse());
	}

	public async Task<UResponse<AddressResponse?>> CreateFromZipCode(AddressCreateFromZipCodeParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<AddressResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		AddressEntity entity;

		AddressEntity? existingVerifiedAddress = await db.Set<AddressEntity>().Where(x => x.ZipCode == p.ZipCode && x.Tags.Contains(TagAddress.Verified)).FirstOrDefaultAsync(ct);
		if (existingVerifiedAddress == null) {
			ItHubBaseResponse<ItHubPostalCodeToAddressDetailResponse?> address = await itHubService.PostalCodeToAddressDetail(new PostalCodeToAddressDetailParams {
				PostCode = p.ZipCode,
				OrderId = "1"
			}, ct);

			entity = new AddressEntity {
				Title = p.Title,
				CreatorId = userData.Id,
				JsonData = new AddressJson {
					Province = address.Data!.Province,
					Township = address.Data!.TownShip,
					Street = address.Data!.Street,
					Street2 = address.Data!.Street2,
					LocalityName = address.Data!.LocalityName,
					HouseNumber = address.Data!.HouseNumber,
					Floor = address.Data!.Floor,
					Description = address.Data!.Description
				},
				Tags = [TagAddress.Verified]
			};
		}
		else {
			if (existingVerifiedAddress.CreatorId == userData.Id) return new UResponse<AddressResponse?>(null, Usc.Conflict, ls.Get("AddressWithThisZipCodeAlreadyExists"));
			entity = new AddressEntity {
				CreatorId = userData.Id,
				Title = p.Title,
				ZipCode = p.ZipCode,
				JsonData = new AddressJson {
					Province = existingVerifiedAddress.JsonData.Province,
					Township = existingVerifiedAddress.JsonData.Township,
					Street = existingVerifiedAddress.JsonData.Street,
					Street2 = existingVerifiedAddress.JsonData.Street2,
					LocalityName = existingVerifiedAddress.JsonData.LocalityName,
					HouseNumber = existingVerifiedAddress.JsonData.HouseNumber,
					Floor = existingVerifiedAddress.JsonData.Floor,
					Description = existingVerifiedAddress.JsonData.Description
				},
				Tags = existingVerifiedAddress.Tags
			};
		}

		await db.Set<AddressEntity>().AddAsync(entity, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<AddressResponse?>(entity.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<AddressResponse>?>> Read(AddressReadParams p, CancellationToken ct) {
		IQueryable<AddressResponse> q = db.Set<AddressEntity>().Select(Projections.AddressSelector(p.SelectorArgs));
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse<AddressResponse?>> Update(AddressUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<AddressResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		AddressEntity? e = await db.Set<AddressEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse<AddressResponse?>(null, Usc.NotFound, ls.Get("AddressNotFound"));
		p.MapToEntity(e);
		db.Update(p.MapToEntity(e));
		await db.SaveChangesAsync(ct);
		return new UResponse<AddressResponse?>(e.MapToResponse());
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<AddressEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

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