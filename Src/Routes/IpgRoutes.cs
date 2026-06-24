namespace SinaMN75U.Routes;

public static class IpgRoutes {
	public static void MapIpgRoutes(this IEndpointRouteBuilder app, string tag) {
		RouteGroupBuilder r = app.MapGroup(tag).WithTags(tag).AddEndpointFilter<UValidationFilter>();
		r.MapPost("Pay", async (IpgSaleParams p, IIpgService s, CancellationToken c) => (await s.GetSaleIpgLink(p, c)).ToResult()).Produces<UResponse>();

		r.MapGet("CallBack", async (
			[FromQuery] string token,
			[FromQuery] short status,
			[FromQuery] string? cardNumberMasked,
			[FromQuery] long? rrn,
			[FromQuery] string? additionalData,
			IIpgService s,
			CancellationToken c) => {
			await s.IpgCallBack(token, status, cardNumberMasked, rrn, additionalData, c);

			bool success = status == 0;
			string html = $@"
<!DOCTYPE html>
<html lang='fa' dir='rtl'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>نتیجه پرداخت</title>
    <style>
        body {{
            font-family: Tahoma, Arial, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            margin: 0;
            padding: 20px;
        }}
        .container {{
            background: white;
            border-radius: 20px;
            padding: 40px;
            max-width: 450px;
            width: 100%;
            text-align: center;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
        }}
        .icon {{
            font-size: 80px;
            margin-bottom: 20px;
        }}
        .success {{ color: #4CAF50; }}
        .error {{ color: #f44336; }}
        h2 {{
            margin-bottom: 20px;
            color: #333;
        }}
        .details {{
            background: #f5f5f5;
            border-radius: 12px;
            padding: 20px;
            margin: 20px 0;
            text-align: right;
        }}
        .detail-row {{
            display: flex;
            justify-content: space-between;
            padding: 8px 0;
            border-bottom: 1px solid #ddd;
        }}
        .detail-row:last-child {{
            border-bottom: none;
        }}
        .label {{
            font-weight: bold;
            color: #555;
        }}
        .value {{
            color: #333;
            direction: ltr;
        }}
        .button {{
            background: {(success ? "#4CAF50" : "#f44336")};
            color: white;
            border: none;
            padding: 12px 30px;
            border-radius: 25px;
            font-size: 16px;
            cursor: pointer;
            margin-top: 20px;
        }}
        .button:hover {{
            opacity: 0.9;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='icon {(success ? "success" : "error")}'>{(success ? "✅" : "❌")}</div>
        <h2>{(success ? "پرداخت موفق" : "پرداخت ناموفق")}</h2>
        <div class='details'>
            <div class='detail-row'>
                <span class='label'>وضعیت:</span>
                <span class='value'>{(status == 0 ? "موفق" : status == -138 ? "لغو شده توسط کاربر" : "ناموفق")}</span>
            </div>
            {(cardNumberMasked != null ? $@"
            <div class='detail-row'>
                <span class='label'>شماره کارت:</span>
                <span class='value'>{cardNumberMasked}</span>
            </div>" : "")}
            {(rrn != null ? $@"
            <div class='detail-row'>
                <span class='label'>شماره پیگیری:</span>
                <span class='value'>{rrn}</span>
            </div>" : "")}
        </div>
        <button class='button' onclick='window.close()'>بستن</button>
    </div>
</body>
</html>";

			return Results.Content(html, "text/html");
		});
	}
}