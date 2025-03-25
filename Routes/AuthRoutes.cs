namespace SinaMN75U.Routes;

public static class AuthRoutes {
	public static void MapAuthRoutes(this IEndpointRouteBuilder app, string name) {
		app.MapPost("api/auth/Register", async (RegisterParams d, IAuthService s, CancellationToken c) => (await s.Register(d, c)).ToResult()).WithTags(name).AddEndpointFilter<ValidationFilter<RegisterParams>>().Produces<UResponse<LoginResponse>>();
		app.MapPost("api/auth/LoginWithPassword", async (LoginWithEmailPasswordParams d, IAuthService s, CancellationToken c) => (await s.LoginWithPassword(d, c)).ToResult()).WithTags(name).AddEndpointFilter<ValidationFilter<LoginWithEmailPasswordParams>>().Produces<UResponse<LoginResponse>>();
		app.MapPost("api/auth/RefreshToken", async (RefreshTokenParams d, IAuthService s, CancellationToken c) => (await s.RefreshToken(d, c)).ToResult()).WithTags(name).Produces<UResponse<LoginResponse>>();
		app.MapPost("api/auth/GetVerificationCodeForLogin", async (GetMobileVerificationCodeForLoginParams p, IAuthService s, CancellationToken c) => (await s.GetVerificationCodeForLogin(p, c)).ToResult()).WithTags(name).AddEndpointFilter<ValidationFilter<GetMobileVerificationCodeForLoginParams>>().Produces<UResponse<UserResponse>>();
		app.MapPost("api/auth/VerifyCodeForLogin", async (VerifyMobileForLoginParams p, IAuthService s, CancellationToken c) => (await s.VerifyCodeForLogin(p, c)).ToResult()).WithTags(name).Produces<UResponse<LoginResponse>>();
	}
}