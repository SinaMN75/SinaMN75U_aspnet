using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace SinaMN75U.Routes;

public static class IpgRoutes {
	public static void MapIpgRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Pay", async (IpgPayParams p, IIpgService s, CancellationToken c) => (await s.Pay(p, c)).ToResult()).Produces<UResponse<IpgPayResponse?>>();

		r.MapPost("Verify", async ([FromQuery] string additionalData, HttpContext ctx) => {
			Dictionary<string, StringValues> form = new();
			if (ctx.Request.HasFormContentType)
				try {
					form = QueryHelpers.ParseQuery(await ctx.ReadBodyOnceAsync());
				}
				catch (Exception ex) {
					ctx.CaptureForApiLog(ex);
				}

			string token = Field("Token") is { Length: > 0 } t ? t : Field("token");
			short status = short.TryParse(Field("status"), out short st) ? st : (short)1;
			long? rrn = long.TryParse(Field("RRN") is { Length: > 0 } rr ? rr : Field("rrn"), out long result) ? result : null;

			IpgAdditionalData data = JsonSerializer.Deserialize<IpgAdditionalData>(additionalData.FromBase58(), Core.Default)!;
			data.Status = status;
			data.Rrn = rrn.ToString();
			data.Token = token;

			return Results.Redirect($"{Core.App.BaseUrl}/{RouteTags.Ipg}Verify?additionalData={data.ToJson().ToBase58()}");

			string Field(string key) => form.TryGetValue(key, out StringValues v) && v.ToString() is { Length: > 0 } f ? f : ctx.Request.Query[key].ToString();
		}).DisableAntiforgery();
		
		r.MapGet("Verify", async ([FromQuery] string additionalData, IIpgService s, CancellationToken c) => {
			IpgAdditionalData data = JsonSerializer.Deserialize<IpgAdditionalData>(additionalData.FromBase58(), Core.Default)!;
			data.Paid = await s.Verify(data, c);
			return Results.Content(
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
				          <div class='icon {{(data.Paid ? "success" : "error")}}'>{{(data.Paid ? "✅" : "❌")}}</div>
				          <h2>{{(data.Paid ? "پرداخت موفق" : "پرداخت ناموفق")}}</h2>
				      </div>
				      <script>
				          (function() {
				              try {
				                  window.parent.postMessage({ source: 'u_ipg', additionalData: '{{additionalData}}' }, '*');
				              } catch (e) {}
				          })();
				      </script>
				  </body>
				  </html>
				  """, "text/html"
			);
		}).DisableAntiforgery();

		r.MapGet("Gateway", ([FromQuery] string additionalData) => {
			IpgAdditionalData data = JsonSerializer.Deserialize<IpgAdditionalData>(additionalData.FromBase58(), Core.Default)!;
			data.Token = "FAKE";

			data.Status = 0;
			data.Rrn = "123456789";
			string okUrl = $"{Core.App.BaseUrl}/{RouteTags.Ipg}Verify?additionalData={data.ToJson().ToBase58()}";

			data.Status = 1;
			data.Rrn = null;
			string failUrl = $"{Core.App.BaseUrl}/{RouteTags.Ipg}Verify?additionalData={data.ToJson().ToBase58()}";

			string kindTitle = data.Kind switch {
				TagIpgPayment.Bill => "پرداخت قبض",
				TagIpgPayment.TopUp => "شارژ مستقیم",
				TagIpgPayment.MultiplexedSale => "پرداخت تسهیمی",
				_ => "پرداخت"
			};
			string kindDetail = data.Kind switch {
				TagIpgPayment.Bill => $"<div class='badge'>شناسه قبض: {data.BillId} - شناسه پرداخت: {data.PaymentId}</div>",
				TagIpgPayment.TopUp => $"<div class='badge'>شماره شارژ شونده: {data.ChargeMobileNumber}</div>",
				_ => ""
			};
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
				          a.button { display: block; text-decoration: none; color: white; padding: 14px 30px; border-radius: 25px; font-size: 16px; margin-top: 12px; }
				          .pay { background: #4CAF50; }
				          .err { background: #f44336; }
				      </style>
				  </head>
				  <body>
				      <div class='container'>
				          <h2>درگاه پرداخت آزمایشی</h2>
				          <div class='badge'>این یک درگاه تستی است و پولی جابجا نمی‌شود</div>
				          <div class='badge'>{{kindTitle}}</div>
				          {{kindDetail}}
				          <div class='amount'>{{data.Amount:N0}} ریال</div>
				          <a class='button pay' href='{{okUrl}}'>پرداخت موفق</a>
				          <a class='button err' href='{{failUrl}}'>پرداخت ناموفق / انصراف</a>
				      </div>
				  </body>
				  </html>
				  """,
				"text/html");
		});
	}
}