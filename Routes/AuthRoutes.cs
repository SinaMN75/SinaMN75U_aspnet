using Microsoft.AspNetCore.Routing;
using SinaMN75U.Data.Params.User;
using SinaMN75U.Services;

namespace SinaMN75U.Routes;

public static class AuthRoutes {
	public static void MapAuthRoutes(this IEndpointRouteBuilder app, string name) {
		app.MapPost("auth/Register", async (RegisterParams d, IAuthService s, CancellationToken c) => (await s.Register(d, c)).ToResult()).WithTags(name).AddEndpointFilter<ValidationFilter<RegisterParams>>().Produces<UResponse<LoginResponse>>();
		app.MapPost("auth/LoginWithPassword", async (LoginWithEmailPasswordParams d, IAuthService s, CancellationToken c) => (await s.LoginWithPassword(d, c)).ToResult()).WithTags(name).AddEndpointFilter<ValidationFilter<LoginWithEmailPasswordParams>>().Produces<UResponse<LoginResponse>>();
		app.MapPost("auth/RefreshToken", async (RefreshTokenParams d, IAuthService s, CancellationToken c) => (await s.RefreshToken(d, c)).ToResult()).WithTags(name).Produces<UResponse<LoginResponse>>();
		app.MapPost("auth/GetVerificationCodeForLogin", async (GetMobileVerificationCodeForLoginParams p, IAuthService s, CancellationToken c) => (await s.GetVerificationCodeForLogin(p, c)).ToResult()).WithTags(name).AddEndpointFilter<ValidationFilter<GetMobileVerificationCodeForLoginParams>>().Produces<UResponse<UserResponse>>();
		app.MapPost("auth/VerifyCodeForLogin", async (VerifyMobileForLoginParams p, IAuthService s, CancellationToken c) => (await s.VerifyCodeForLogin(p, c)).ToResult()).WithTags(name).Produces<UResponse<LoginResponse>>();
	}
}