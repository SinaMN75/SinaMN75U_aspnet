using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace SinaMN75U.Routes;

public static class IpgRoutes {
	public static void MapIpgRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Pay", async (IpgSaleParams p, IIpgService s, CancellationToken c) => (await s.GetSaleIpgLink(p, c)).ToResult()).Produces<UResponse<IpgPayResponse?>>();
		r.MapPost("PayBill", async (IpgBillParams p, IIpgService s, CancellationToken c) => (await s.GetBillIpgLink(p, c)).ToResult()).Produces<UResponse<IpgPayResponse?>>();
		r.MapPost("Status", async (IpgStatusParams p, IIpgService s, CancellationToken c) => (await s.Status(p, c)).ToResult()).Produces<UResponse<IpgVerifyResponse?>>();

		r.MapPost("Verify", async (
			[FromQuery] string additionalData,
			HttpContext ctx,
			IIpgService s,
			CancellationToken c) => {
			Dictionary<string, StringValues> form = ctx.Request.HasFormContentType ? QueryHelpers.ParseQuery(await ctx.ReadBodyOnceAsync()) : new Dictionary<string, StringValues>();
			string Field(string key) => form.TryGetValue(key, out StringValues v) && v.ToString() is { Length: > 0 } f ? f : ctx.Request.Query[key].ToString();
			string token = Field("Token") is { Length: > 0 } t ? t : Field("token");
			short status = short.TryParse(Field("status"), out short st) ? st : (short)1;
			long? rrn = long.TryParse(Field("RRN") is { Length: > 0 } rr ? rr : Field("rrn"), out long r) ? r : null;
			string? cardNumberMasked = (Field("HashCardNumber") is { Length: > 0 } h ? h : Field("cardNumberMasked")) is { Length: > 0 } cm ? cm : null;
			string? trackingNumber = await s.Verify(token, status, cardNumberMasked, rrn, additionalData, c);
			HttpRequest req = ctx.Request;
			string basePath = req.Path.Value![..(req.Path.Value!.LastIndexOf('/') + 1)];
			return Results.Redirect($"{req.Scheme}://{req.Host}{basePath}Verify?status={status}&trackingNumber={trackingNumber}");
		}).DisableAntiforgery();

		r.MapGet("Gateway", ([FromQuery] string additionalData, [FromQuery] long amount, HttpContext ctx) => {
			HttpRequest req = ctx.Request;
			string basePath = req.Path.Value![..(req.Path.Value!.LastIndexOf('/') + 1)];
			string callBackUrl = $"{req.Scheme}://{req.Host}{basePath}Verify?additionalData={additionalData}";
			return Results.Content(
				$$"""
				  <!DOCTYPE html>
				  <html lang='fa' dir='rtl'>
				  <head>
				      <meta charset='UTF-8'>
				      <meta name='viewport' content='width=device-width, initial-scale=1.0'>
				      <title>درگاه پرداخت آزمایشی</title>
				      <style>
				          body { font-family: Tahoma, Arial, sans-serif; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); min-height: 100vh; display: flex; justify-content: center; align-items: center; margin: 0; padding: 20px; }
				          .container { background: white; border-radius: 20px; padding: 40px; max-width: 450px; width: 100%; text-align: center; box-shadow: 0 20px 60px rgba(0,0,0,0.3); }
				          h2 { color: #333; margin-bottom: 8px; }
				          .badge { color: #764ba2; font-size: 13px; margin-bottom: 24px; }
				          .amount { font-size: 28px; font-weight: bold; color: #333; margin: 16px 0 28px; }
				          button.button { display: block; width: 100%; border: 0; cursor: pointer; font-family: inherit; color: white; padding: 14px 30px; border-radius: 25px; font-size: 16px; margin-top: 12px; }
				          .pay { background: #4CAF50; }
				          .err { background: #f44336; }
				      </style>
				  </head>
				  <body>
				      <div class='container'>
				          <h2>درگاه پرداخت آزمایشی</h2>
				          <div class='badge'>این یک درگاه تستی است و پولی جابجا نمی‌شود</div>
				          <div class='amount'>{{amount:N0}} ریال</div>
				          <form method='post' action='{{callBackUrl}}'>
				              <input type='hidden' name='Token' value='FAKE'>
				              <input type='hidden' name='status' value='0'>
				              <input type='hidden' name='RRN' value='123456789'>
				              <input type='hidden' name='HashCardNumber' value='627412******2424'>
				              <button class='button pay' type='submit'>پرداخت موفق</button>
				          </form>
				          <form method='post' action='{{callBackUrl}}'>
				              <input type='hidden' name='Token' value='FAKE'>
				              <input type='hidden' name='status' value='-138'>
				              <button class='button err' type='submit'>پرداخت ناموفق / انصراف</button>
				          </form>
				      </div>
				  </body>
				  </html>
				  """,
				"text/html");
		});

		r.MapGet("Verify", ([FromQuery] short status, [FromQuery] string? trackingNumber) => Results.Content(
			$$"""
			  <!DOCTYPE html>
			  <html lang='fa' dir='rtl'>
			  <head>
			      <meta charset='UTF-8'>
			      <meta name='viewport' content='width=device-width, initial-scale=1.0'>
			      <title>نتیجه پرداخت</title>
			      <style>
			          body { font-family: Tahoma, Arial, sans-serif; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); min-height: 100vh; display: flex; justify-content: center; align-items: center; margin: 0; padding: 20px; }
			          .container { background: white; border-radius: 20px; padding: 40px; max-width: 450px; width: 100%; text-align: center; box-shadow: 0 20px 60px rgba(0,0,0,0.3); }
			          .icon { font-size: 72px; margin-bottom: 16px; }
			          .success { color: #4CAF50; }
			          .error { color: #f44336; }
			          h2 { color: #333; }
			      </style>
			  </head>
			  <body>
			      <div class='container'>
			          <div class='icon {{(status == 0 ? "success" : "error")}}'>{{(status == 0 ? "✅" : "❌")}}</div>
			          <h2>{{(status == 0 ? "پرداخت موفق" : "پرداخت ناموفق")}}</h2>
			      </div>
			      <script>
			          (function() {
			              try {
			                  window.parent.postMessage({ source: 'u_ipg', trackingNumber: {{JsonSerializer.Serialize(trackingNumber ?? "")}}, status: {{status}} }, '*');
			              } catch (e) {}
			          })();
			      </script>
			  </body>
			  </html>
			  """, "text/html")).DisableAntiforgery();
	}
}
