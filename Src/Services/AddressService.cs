namespace SinaMN75U.Services;

public interface IAddressService {
	Task<UResponse<Guid?>> Create(AddressCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<AddressResponse>?>> Read(AddressReadParams p, CancellationToken ct);
	Task<UResponse> Update(AddressUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
}

public class AddressService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IAddressService {
	public async Task<UResponse<Guid?>> Create(AddressCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		AddressEntity e = new() {
			Id = p.Id ?? Guid.CreateVersion7(),
			CreatorId = p.CreatorId ?? userData.Id,
			CreatedAt = DateTime.UtcNow,
			Tags = p.Tags,
			ZipCode = p.ZipCode,
			JsonData = new AddressJson {
				Title = p.Title,
				Province = p.Province,
				Township = p.Township,
				Street = p.Street,
				Street2 = p.Street2,
				LocalityName = p.LocalityName,
				HouseNumber = p.HouseNumber,
				Floor = p.Floor,
				Description = p.Description,
				BuildingName = p.BuildingName,
				LocalityType = p.LocalityType,
				SideFloor = p.SideFloor,
				SubLocality = p.SubLocality,
				Village = p.Village
			}
		};

		await db.AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<AddressResponse>?>> Read(AddressReadParams p, CancellationToken ct) {
		IQueryable<AddressEntity> q = db.Set<AddressEntity>().ApplyReadParams<AddressEntity, TagAddress, AddressJson>(p);
		
		if (p.ZipCode.IsNotNullOrEmpty()) q = q.Where(x => x.ZipCode == p.ZipCode);

		IQueryable<AddressResponse> projected = q.Select(Projections.AddressSelector(p.SelectorArgs));
		return await projected.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(AddressUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		AddressEntity? e = await db.Set<AddressEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("AddressNotFound"));
		
		if (!userData.IsAdmin && userData.Id != e.CreatorId) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		if (p.ZipCode != null) e.ZipCode = p.ZipCode;
		if (p.Title != null) e.JsonData.Title = p.Title;
		if (p.Province != null) e.JsonData.Province = p.Province;
		if (p.Township != null) e.JsonData.Township = p.Township;
		if (p.Street != null) e.JsonData.Street = p.Street;
		if (p.Street2 != null) e.JsonData.Street2 = p.Street2;
		if (p.LocalityName != null) e.JsonData.LocalityName = p.LocalityName;
		if (p.HouseNumber != null) e.JsonData.HouseNumber = p.HouseNumber;
		if (p.Floor != null) e.JsonData.Floor = p.Floor;
		if (p.Description != null) e.JsonData.Description = p.Description;
		if (p.BuildingName != null) e.JsonData.BuildingName = p.BuildingName;
		if (p.LocalityType != null) e.JsonData.LocalityType = p.LocalityType;
		if (p.SideFloor != null) e.JsonData.SideFloor = p.SideFloor;
		if (p.SubLocality != null) e.JsonData.SubLocality = p.SubLocality;
		if (p.Village != null) e.JsonData.Village = p.Village;

		db.Update(e.ApplyUpdateParam<AddressEntity, TagAddress, AddressJson>(p));
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired"));
		
		AddressEntity? e = await db.Set<AddressEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("AddressNotFound"));

		if (!userData.IsAdmin && userData.Id != e.CreatorId) return new UResponse(Usc.Forbidden, ls.Get("YouDoNotHaveClearanceToDoThisAction"));

		db.Set<AddressEntity>().Remove(e);
		await db.SaveChangesAsync(ct);
		
		return new UResponse(Usc.Deleted, ls.Get("AddressDeletedSuccessfully"));
	}
}