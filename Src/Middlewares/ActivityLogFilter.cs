namespace SinaMN75U.Middlewares;

public sealed class ActivityLogFilter : IEndpointFilter {
	private static readonly string[] ReadOnly = ["Read", "Report", "Print", "Calendar", "Export", "Availability"];

	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
		object? result = await next(context);
		try {
			string path = context.HttpContext.Request.Path.Value ?? "";
			if (result is not IStatusCodeHttpResult { StatusCode: >= 200 and < 300 } || ReadOnly.Any(x => path.Contains(x, StringComparison.OrdinalIgnoreCase))) return result;
			BaseParams? p = context.Arguments.OfType<BaseParams>().FirstOrDefault();
			if (p?.Token == null) return result;
			IServiceProvider sp = context.HttpContext.RequestServices;
			JwtClaimData? u = sp.GetRequiredService<ITokenService>().ExtractClaims(p.Token);
			if (u == null || u.AdminRank == 0) return result;
			await sp.GetRequiredService<IOrganizationService>().LogActivity(u, path, p, context.HttpContext.RequestAborted);
		}
		catch (Exception e) {
			ULog.Error(e, "Activity log was not written");
		}

		return result;
	}
}
