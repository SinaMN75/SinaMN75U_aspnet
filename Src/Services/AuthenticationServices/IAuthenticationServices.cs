namespace SinaMN75U.Services.AuthenticationServices;

public interface IAuthenticationService {
	Task<UResponse<ITHubGetAccessTokenResponse>> GetAccessToken(ITHubGetAccessTokenParams p, CancellationToken ct);
	Task<UResponse<ITHubShahkarResponse>> Shahkar(ITHubShahkarParams p, CancellationToken ct);
	Task<UResponse<ITHubPostalCodeToAddressDetailResponse>> PostalCodeToAddressDetail(ITHubPostalCodeToAddressDetailParams p, CancellationToken ct);
	Task<UResponse> GetManagementAccessToken(ITHubGetManagementAccessTokenParams p, CancellationToken ct);
	Task<UResponse> RefreshManagementAccessToken(ITHubRefreshManagementAccessTokenParams p, CancellationToken ct);
}