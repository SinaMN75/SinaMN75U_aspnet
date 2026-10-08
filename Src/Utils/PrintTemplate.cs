namespace SinaMN75U.Utils;

public static class PrintTemplate {
	public static string Date(DateTime? d) => d == null ? "" : d.Value.ToPersian().ToString("yyyy/MM/dd");

	public static string Money(decimal d) => d.ToString("#,0");

	public static string Build(
		OrganizationEntity? organization,
		string placeTitle,
		string title,
		IEnumerable<(string Label, string? Value)> fields,
		IReadOnlyList<string>? headers = null,
		IEnumerable<IReadOnlyList<string>>? rows = null,
		IEnumerable<string>? notes = null,
		IEnumerable<string>? signatures = null
	) {
		static string E(string? s) => WebUtility.HtmlEncode(s ?? "");
		StringBuilder sb = new();
		sb.Append("<!doctype html><html lang=\"fa\" dir=\"rtl\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><title>")
			.Append(E(title))
			.Append("</title><style>")
			.Append("body{font-family:Vazirmatn,Vazir,Tahoma,sans-serif;margin:24px;color:#111;background:#fff}")
			.Append("header{display:flex;align-items:center;gap:16px;border-bottom:2px solid #333;padding-bottom:12px}header img{max-height:72px;max-width:160px}")
			.Append("header small{display:block;color:#555;margin-top:4px}h1{font-size:20px;margin:18px 0 8px}")
			.Append("table{width:100%;border-collapse:collapse;margin-top:12px}td,th{border:1px solid #999;padding:6px 8px;text-align:right;font-size:13px}th{background:#eee}")
			.Append(".fields td:first-child{width:32%;background:#f7f7f7}ul{font-size:13px;line-height:1.9}")
			.Append(".sign{display:flex;justify-content:space-around;margin-top:56px;font-size:13px}.toolbar{margin-bottom:16px}")
			.Append("button{font-family:inherit;padding:8px 20px;cursor:pointer}@media print{.toolbar{display:none}body{margin:0}}")
			.Append("</style></head><body><div class=\"toolbar\"><button onclick=\"window.print()\">چاپ / ذخیره‌ی PDF</button></div><header>");

		if (organization?.JsonData.LogoUrl.IsNotNullOrEmpty() == true) sb.Append("<img src=\"").Append(E(organization.JsonData.LogoUrl)).Append("\" alt=\"\">");
		sb.Append("<div><b>").Append(E(organization?.Title ?? placeTitle)).Append("</b>");
		if (organization != null && organization.Title != placeTitle) sb.Append("<small>").Append(E(placeTitle)).Append("</small>");
		OrganizationJson? j = organization?.JsonData;
		string info = string.Join(" · ", new[] { j?.Address, j?.PhoneNumber, j?.NationalId == null ? null : $"شناسه‌ی ملی {j.NationalId}", j?.EconomicCode == null ? null : $"کد اقتصادی {j.EconomicCode}" }.Where(x => x.IsNotNullOrEmpty()));
		if (info.Length > 0) sb.Append("<small>").Append(E(info)).Append("</small>");
		sb.Append("</div></header><h1>").Append(E(title)).Append("</h1><table class=\"fields\">");
		foreach ((string label, string? value) in fields.Where(x => x.Value.IsNotNullOrEmpty())) sb.Append("<tr><td>").Append(E(label)).Append("</td><td>").Append(E(value)).Append("</td></tr>");
		sb.Append("</table>");

		if (headers != null && rows != null) {
			sb.Append("<table><tr>");
			foreach (string h in headers) sb.Append("<th>").Append(E(h)).Append("</th>");
			sb.Append("</tr>");
			foreach (IReadOnlyList<string> row in rows) {
				sb.Append("<tr>");
				foreach (string c in row) sb.Append("<td>").Append(E(c)).Append("</td>");
				sb.Append("</tr>");
			}

			sb.Append("</table>");
		}

		List<string> noteList = notes?.Where(x => x.IsNotNullOrEmpty()).ToList() ?? [];
		if (noteList.Count != 0) {
			sb.Append("<ul>");
			foreach (string n in noteList) sb.Append("<li>").Append(E(n)).Append("</li>");
			sb.Append("</ul>");
		}

		List<string> signs = signatures?.ToList() ?? [];
		if (signs.Count != 0) {
			sb.Append("<div class=\"sign\">");
			foreach (string s in signs) sb.Append("<span>").Append(E(s)).Append("</span>");
			sb.Append("</div>");
		}

		return sb.Append("<p style=\"font-size:11px;color:#777;margin-top:32px\">").Append(E(Date(DateTime.UtcNow))).Append("</p></body></html>").ToString();
	}
}
