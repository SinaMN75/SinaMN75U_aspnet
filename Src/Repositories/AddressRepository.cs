using SinaMN75U.Data.ServiceParams;
using SinaMN75U.Data.ServiceResponses;

namespace SinaMN75U.Repositories;

public interface IAddressRepository {
	Task<UResponse<AddressResponse?>> Create(AddressCreateParams p, CancellationToken ct);
	Task<UResponse<IEnumerable<AddressResponse>?>> Read(AddressReadParams p, CancellationToken ct);
	Task<UResponse<AddressResponse?>> Update(AddressUpdateParams p, CancellationToken ct);
	Task<UResponse> Delete(IdParams p, CancellationToken ct);
	Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct);
}

public class AddressRepository(
	ILocalizationService ls,
	ITokenService ts,
	IEfService efs,
	IAddressService addressService
) : IAddressRepository {
	public async Task<UResponse<AddressResponse?>> Create(AddressCreateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<AddressResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		AddressCreateServiceParams serviceParams = new() {
			Tags = p.Tags,
			Id = p.Id,
			Title = p.Title,
			Province = p.Province,
			Township = p.Township,
			Street = p.Street,
			Street2 = p.Street2,
			LocalityName = p.LocalityName,
			HouseNumber = p.HouseNumber,
			Floor = p.Floor,
			ZipCode = p.ZipCode,
			Description = p.Description,
			CreatorId = p.CreatorId ?? userData.Id
		};
		AddressServiceResponse address = await addressService.Create(serviceParams, ct);
		await efs.SaveChangesAsync(ct);
		return new UResponse<AddressResponse?>(address.MapToResponse());
	}

	public async Task<UResponse<IEnumerable<AddressResponse>?>> Read(AddressReadParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<IEnumerable<AddressResponse>?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		PaginatedServiceResponse<IEnumerable<AddressServiceResponse>> paginatedList = await addressService.Read(p.MapToServiceParams(), ct);

		return new UResponse<IEnumerable<AddressResponse>?>(paginatedList.Items.Select(x => x.MapToResponse())) {
			PageSize = paginatedList.PageSize,
			PageCount = paginatedList.PageCount,
			TotalCount = paginatedList.TotalCount
		};
	}

	public async Task<UResponse<AddressResponse?>> Update(AddressUpdateParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<AddressResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		(ErrorServiceResponse?, AddressServiceResponse?) serviceResponse = await addressService.Update(p.MapToParams(), ct);
		return serviceResponse.Item1 != null ? new UResponse<AddressResponse?>(null, serviceResponse.Item1.StatusCode, ls.Get(serviceResponse.Item1.ErrorCode)) : new UResponse<AddressResponse?>(serviceResponse.Item2!.MapToResponse(), Usc.Success, ls.Get("AddressUpdatedSuccessfully"));
	}

	public async Task<UResponse> Delete(IdParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<AddressEntity?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		ErrorServiceResponse? sr = await addressService.Delete(p.MapToServiceParams(), ct);
		return new UResponse(sr?.StatusCode ?? Usc.Deleted, ls.Get(sr?.ErrorCode ?? "AddressDeletedSuccessfully"));
	}

	public async Task<UResponse> SoftDelete(SoftDeleteParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse(Usc.UnAuthorized, ls.Get("AuthorizationRequired", p.Locale));

		ErrorServiceResponse? sr = await addressService.SoftDelete(p.MapToServiceParams(), ct);
		return new UResponse(sr?.StatusCode ?? Usc.Deleted, ls.Get(sr?.ErrorCode ?? "AddressDeletedSuccessfully"));
	}
}