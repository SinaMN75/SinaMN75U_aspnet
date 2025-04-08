namespace SinaMN75U.Routes;

public static class AuthRoutes {
	public static void MapAuthRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder route = app.MapGroup("api/auth/").WithTags(tag).AddEndpointFilter<UValidationFilter>();

		route.MapPost("Register", async (RegisterParams d, IAuthService s, CancellationToken c) => (await s.Register(d, c)).ToResult()).Produces<UResponse<LoginResponse>>();
		route.MapPost("LoginWithPassword", async (LoginWithEmailPasswordParams d, IAuthService s, CancellationToken c) => (await s.LoginWithPassword(d, c)).ToResult()).Produces<UResponse<LoginResponse>>();
		route.MapPost("RefreshToken", async (RefreshTokenParams d, IAuthService s, CancellationToken c) => (await s.RefreshToken(d, c)).ToResult()).Produces<UResponse<LoginResponse>>();
		route.MapPost("GetVerificationCodeForLogin", async (GetMobileVerificationCodeForLoginParams p, IAuthService s, CancellationToken c) => (await s.GetVerificationCodeForLogin(p, c)).ToResult()).Produces<UResponse<UserResponse>>();
		route.MapPost("VerifyCodeForLogin", async (VerifyMobileForLoginParams p, IAuthService s, CancellationToken c) => (await s.VerifyCodeForLogin(p, c)).ToResult()).Produces<UResponse<LoginResponse>>();
	}
}