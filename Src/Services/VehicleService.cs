namespace SinaMN75U.Services;

public interface IVehicleService {
	Task<UResponse<Guid?>> Create(VehicleCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<VehicleResponse>?>> Read(VehicleReadParams p, CancellationToken ct);
	Task<UResponse> Update(VehicleUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct);
}

public class VehicleService(
	DbContext db,
	ILocalizationService ls,
	ITokenService ts
) : IVehicleService {
	public async Task<UResponse<Guid?>> Create(VehicleCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<Guid?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		VehicleEntity e = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			JsonData = new VehicleJson {
				Title = p.Title
			},
			Tags = p.Tags,
			NumberPlate = p.NumberPlate,
			Title = p.Title,
			Brand = p.Brand,
			Color = p.Color
		};
		
		await db.Set<VehicleEntity>().AddAsync(e, ct);
		await db.SaveChangesAsync(ct);
		return new UResponse<Guid?>(e.Id);
	}

	public async Task<UResponse<IEnumerable<VehicleResponse>?>> Read(VehicleReadParams p, CancellationToken ct) {
		IQueryable<VehicleResponse> q = db.Set<VehicleEntity>().Select(Projections.VehicleSelector(p.SelectorArgs));
		if (p.Brand.IsNotNullOrEmpty()) q = q.Where(x => x.Brand == p.Brand);
		if (p.NumberPlate.IsNotNullOrEmpty()) q = q.Where(x => x.NumberPlate == p.NumberPlate);
		if (p.Title.IsNotNullOrEmpty()) q = q.Where(x => x.Title == p.Title);
		if (p.Color.IsNotNullOrEmpty()) q = q.Where(x => x.Color == p.Color);
		return await q.ToPaginatedResponse(p.PageNumber, p.PageSize, ct);
	}

	public async Task<UResponse> Update(VehicleUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		VehicleEntity? e = await db.Set<VehicleEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return new UResponse(Usc.NotFound, ls.Get("VehicleNotFound"));
		
		e.UpdatedAt = DateTime.UtcNow;
		if (p.NumberPlate.IsNotNull()) e.NumberPlate = p.NumberPlate;
		if (p.Title.IsNotNull()) e.Title = p.Title;
		if (p.Brand.IsNotNull()) e.Brand = p.Brand;
		if (p.Color.IsNotNull()) e.Color = p.Color;
		if (p.Tags.IsNotNull()) e.Tags = p.Tags;
		if (p.AddTags.IsNotNullOrEmpty()) e.Tags.AddRangeIfNotExist(p.AddTags);
		if (p.RemoveTags.IsNotNullOrEmpty()) e.Tags.RemoveAll(tag => p.RemoveTags.Contains(tag));
		db.Update(e);
		await db.SaveChangesAsync(ct);
		return new UResponse();
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<VehicleEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);

		return new UResponse();
	}

	public async Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		await db.Set<VehicleEntity>().Where(x => p.Id == x.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.DeletedAt, p.DateTime ?? DateTime.UtcNow), ct);
		return new UResponse();
	}
}