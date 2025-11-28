namespace SinaMN75U.Routes;

public static class AuthRoutes {
	public static void MapAuthRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Register", async (RegisterParams d, IAuthService s, CancellationToken c) => (await s.Register(d, c)).ToResult()).Produces<UResponse<LoginResponse>>();
		r.MapPost("LoginWithEmailPassword", async (LoginWithEmailPasswordParams d, IAuthService s, CancellationToken c) => (await s.LoginWithEmailPassword(d, c)).ToResult()).Produces<UResponse<LoginResponse>>();
		r.MapPost("LoginWithUserNamePassword", async (LoginWithUserNamePasswordParams d, IAuthService s, CancellationToken c) => (await s.LoginWithUserNamePassword(d, c)).ToResult()).Produces<UResponse<LoginResponse>>();
		r.MapPost("Test", async (LoginWithEmailPasswordParams d, IAuthService s, CancellationToken c) => (await s.TestToken(d, c)).ToResult()).Produces<UResponse<LoginResponse>>();
		r.MapPost("RefreshToken", async (RefreshTokenParams d, IAuthService s, CancellationToken c) => (await s.RefreshToken(d, c)).ToResult()).Produces<UResponse<LoginResponse>>();
		r.MapPost("GetVerificationCodeForLogin", async (GetMobileVerificationCodeForLoginParams p, IAuthService s, CancellationToken c) => (await s.GetVerificationCodeForLogin(p, c)).ToResult()).Produces<UResponse<UserEntity>>();
		r.MapPost("VerifyCodeForLogin", async (VerifyMobileForLoginParams p, IAuthService s, CancellationToken c) => (await s.VerifyCodeForLogin(p, c)).ToResult()).Produces<UResponse<LoginResponse>>();
		r.MapPost("ReadUserByToken", async (BaseParams p, IAuthService s, CancellationToken c) => (await s.ReadUserByToken(p, c)).ToResult()).Produces<UResponse<UserEntity>>();
	}
}