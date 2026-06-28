namespace SinaMN75U.Routes;

public static class IpgRoutes {
	public static void MapIpgRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();

		// 1) App calls this to open the gateway; returns the url to show in the WebView (PNA page, or our Gateway page in test).
		r.MapPost("Pay", async (IpgSaleParams p, IIpgService s, CancellationToken c) => (await s.GetSaleIpgLink(p, c)).ToResult()).Produces<UResponse<IpgPayResponse?>>();

		// 2) Test-only fake gateway PAGE (what Pay opens in test mode): two buttons that redirect to the callback (Verify).
		r.MapGet("Gateway", ([FromQuery] string additionalData, [FromQuery] long amount, HttpContext ctx) => {
			// Build the callback (Verify) url from this same host (.../api/ipg/Gateway -> .../api/ipg/Verify).
			HttpRequest req = ctx.Request;
			string basePath = req.Path.Value![..(req.Path.Value!.LastIndexOf('/') + 1)];
			string verify = $"{req.Scheme}://{req.Host}{basePath}Verify";
			string successUrl = $"{verify}?additionalData={additionalData}&token=FAKE&status=0&rrn=123456789&cardNumberMasked=627412******2424";
			string errorUrl = $"{verify}?additionalData={additionalData}&token=FAKE&status=1";
			return Results.Content(GatewayPage(amount, successUrl, errorUrl), "text/html");
		});

		// 3) The callback the gateway redirects to (success/cancel/error). Confirms + credits the wallet, fully server-side.
		r.MapGet("Verify", async (
			[FromQuery] string additionalData,
			[FromQuery] string? token,
			[FromQuery] short status,
			[FromQuery] string? cardNumberMasked,
			[FromQuery] long? rrn,
			IIpgService s,
			CancellationToken c) => {
			await s.Verify(token ?? "", status, cardNumberMasked, rrn, additionalData, c);
			return Results.Content(ResultPage(status), "text/html");
		});
	}

	// Fake gateway: shows the amount and two buttons that redirect to the callback with a success/error status.
	private static string GatewayPage(long amount, string successUrl, string errorUrl) => $@"
<!DOCTYPE html>
<html lang='fa' dir='rtl'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>درگاه پرداخت آزمایشی</title>
    <style>
        body {{ font-family: Tahoma, Arial, sans-serif; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); min-height: 100vh; display: flex; justify-content: center; align-items: center; margin: 0; padding: 20px; }}
        .container {{ background: white; border-radius: 20px; padding: 40px; max-width: 450px; width: 100%; text-align: center; box-shadow: 0 20px 60px rgba(0,0,0,0.3); }}
        h2 {{ color: #333; margin-bottom: 8px; }}
        .badge {{ color: #764ba2; font-size: 13px; margin-bottom: 24px; }}
        .amount {{ font-size: 28px; font-weight: bold; color: #333; margin: 16px 0 28px; }}
        a.button {{ display: block; text-decoration: none; color: white; padding: 14px 30px; border-radius: 25px; font-size: 16px; margin-top: 12px; }}
        .pay {{ background: #4CAF50; }}
        .err {{ background: #f44336; }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>درگاه پرداخت آزمایشی</h2>
        <div class='badge'>این یک درگاه تستی است و پولی جابجا نمی‌شود</div>
        <div class='amount'>{amount:N0} ریال</div>
        <a class='button pay' href='{successUrl}'>پرداخت موفق</a>
        <a class='button err' href='{errorUrl}'>پرداخت ناموفق / انصراف</a>
    </div>
</body>
</html>";

	// Shown after the callback runs; the WebView detects this url (the Verify callback) and closes itself.
	private static string ResultPage(short status) {
		bool success = status == 0;
		return $@"
<!DOCTYPE html>
<html lang='fa' dir='rtl'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>نتیجه پرداخت</title>
    <style>
        body {{ font-family: Tahoma, Arial, sans-serif; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); min-height: 100vh; display: flex; justify-content: center; align-items: center; margin: 0; padding: 20px; }}
        .container {{ background: white; border-radius: 20px; padding: 40px; max-width: 450px; width: 100%; text-align: center; box-shadow: 0 20px 60px rgba(0,0,0,0.3); }}
        .icon {{ font-size: 72px; margin-bottom: 16px; }}
        .success {{ color: #4CAF50; }}
        .error {{ color: #f44336; }}
        h2 {{ color: #333; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='icon {(success ? "success" : "error")}'>{(success ? "✅" : "❌")}</div>
        <h2>{(success ? "پرداخت موفق" : "پرداخت ناموفق")}</h2>
    </div>
</body>
</html>";
	}
}
