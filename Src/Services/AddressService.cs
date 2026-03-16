using SinaMN75U.Data.ServiceParams;
using SinaMN75U.Data.ServiceResponses;

namespace SinaMN75U.Services;

public interface IAddressService {
	Task<AddressServiceResponse> Create(AddressCreateServiceParams p, CancellationToken ct);
	Task BulkCreate(AddressBulkCreateServiceParams p, CancellationToken ct);
	Task<PaginatedServiceResponse<IEnumerable<AddressServiceResponse>>> Read(AddressReadServiceParams p, CancellationToken ct);
	Task<(ErrorServiceResponse?, AddressServiceResponse?)> Update(AddressUpdateServiceParams p, CancellationToken ct);
	Task<ErrorServiceResponse?> Delete(IdServiceParams p, CancellationToken ct);
	Task<ErrorServiceResponse?> SoftDelete(SoftDeleteServiceParams p, CancellationToken ct);
}

public class AddressService(
	DbContext db,
	IEfService efs
) : IAddressService {
	public async Task<AddressServiceResponse> Create(AddressCreateServiceParams p, CancellationToken ct) {
		AddressEntity entity = new() {
			Id = Guid.CreateVersion7(),
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow,
			Title = p.Title,
			ZipCode = p.ZipCode,
			CreatorId = p.CreatorId,
			Tags = p.Tags,
			JsonData = new AddressJson {
				Province = p.Province,
				Township = p.Township,
				Street = p.Street,
				Street2 = p.Street2,
				LocalityName = p.LocalityName,
				HouseNumber = p.HouseNumber,
				Floor = p.Floor,
				Description = p.Description
			}
		};
		EntityEntry<AddressEntity> e = await db.AddAsync(entity, ct);
		return e.Entity.MapToServiceResponse();
	}

	public async Task BulkCreate(AddressBulkCreateServiceParams p, CancellationToken ct) {
		IEnumerable<AddressEntity> addresses = p.List.Select(x => new AddressEntity {
			Title = x.Title,
			CreatorId = x.CreatorId,
			JsonData = new AddressJson {
				Province = x.Province,
				Township = x.Township,
				Street = x.Street,
				Street2 = x.Street2,
				LocalityName = x.LocalityName,
				HouseNumber = x.HouseNumber,
				Floor = x.Floor,
				Description = x.Description
			},
			Tags = x.Tags
		});
		await db.AddRangeAsync(addresses, ct);
	}

	public async Task<PaginatedServiceResponse<IEnumerable<AddressServiceResponse>>> Read(AddressReadServiceParams p, CancellationToken ct) {
		IQueryable<AddressEntity> q = db.Set<AddressEntity>();

		if (p.OrderByCreatedAt) q = q.OrderBy(x => x.CreatedAt);
		if (p.OrderByCreatedAtDesc) q = q.OrderByDescending(x => x.CreatedAt);
		if (p.OrderByUpdatedAt) q = q.OrderBy(x => x.UpdatedAt);
		if (p.OrderByUpdatedAtDesc) q = q.OrderByDescending(x => x.UpdatedAt);
		if (p.ToCreatedAt.HasValue) q = q.Where(x => x.CreatedAt <= p.ToCreatedAt.Value);
		if (p.FromCreatedAt.HasValue) q = q.Where(x => x.CreatedAt >= p.FromCreatedAt.Value);
		if (p.Tags.IsNotNullOrEmpty()) q = q.Where(x => x.Tags.Any(tag => p.Tags!.Contains(tag)));
		if (p.Ids.IsNotNullOrEmpty()) q = q.WhereIn(x => x.Id, p.Ids);

		if (p.CreatorId != null) q = q.Where(x => x.CreatorId == p.CreatorId);

		return await efs.QueryPaginatedAsync(q.Select(Projections.ServiceAddressSelector(p.SelectorArgs)), p.PageSize, p.PageNumber, ct);
	}

	public async Task<(ErrorServiceResponse?, AddressServiceResponse?)> Update(AddressUpdateServiceParams p, CancellationToken ct) {
		AddressEntity? e = await db.Set<AddressEntity>().FirstOrDefaultAsync(x => x.Id == p.Id, ct);
		if (e == null) return (new ErrorServiceResponse(Usc.NotFound, "AddressNotFound"), null);

		if (p.Title != null) e.Title = p.Title;
		if (p.Province != null) e.JsonData.Province = p.Province;
		if (p.Township != null) e.JsonData.Township = p.Township;
		if (p.Street != null) e.JsonData.Street = p.Street;
		if (p.Street2 != null) e.JsonData.Street2 = p.Street2;
		if (p.LocalityName != null) e.JsonData.LocalityName = p.LocalityName;
		if (p.HouseNumber != null) e.JsonData.HouseNumber = p.HouseNumber;
		if (p.Floor != null) e.JsonData.Floor = p.Floor;
		if (p.ZipCode != null) e.ZipCode = p.ZipCode;
		if (p.Description != null) e.JsonData.Description = p.Description;

		db.Update(e);
		await db.SaveChangesAsync(ct);
		return (null, e.MapToServiceResponse());
	}

	public async Task<ErrorServiceResponse?> Delete(IdServiceParams p, CancellationToken ct) {
		int count = await db.Set<AddressEntity>().Where(x => p.Id == x.Id).ExecuteDeleteAsync(ct);
		return count == 0 ? new ErrorServiceResponse(Usc.NotFound, "AddressNotFound") : null;
	}

	public async Task<ErrorServiceResponse?> SoftDelete(SoftDeleteServiceParams p, CancellationToken ct) {
		int count = await db.Set<AddressEntity>().Where(x => p.Id == x.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.DeletedAt, p.DateTime ?? DateTime.UtcNow), ct);
		return count == 0 ? new ErrorServiceResponse(Usc.NotFound, "AddressNotFound") : null;
	}
}