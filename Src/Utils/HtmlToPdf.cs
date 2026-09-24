namespace SinaMN75U.Utils {
	public sealed class HtmlToPdfOptions {
		public string PaperSize { get; set; } = "A4";
		public double? PageWidthMm { get; set; }
		public double? PageHeightMm { get; set; }
		public bool Landscape { get; set; }
		public double? MarginTopMm { get; set; }
		public double? MarginRightMm { get; set; }
		public double? MarginBottomMm { get; set; }
		public double? MarginLeftMm { get; set; }
		public bool PrintBackground { get; set; } = true;
		public bool PreferCssPageSize { get; set; } = true;
		public string MediaType { get; set; } = "print";
		public double Scale { get; set; } = 1;
		public bool ShrinkToFit { get; set; } = true;
		public string? BaseDirectory { get; set; }
		public List<string> FontDirectories { get; } = new();
		public List<string> FontFiles { get; } = new();
		public bool UseSystemFonts { get; set; } = true;
		public string? DefaultFontFamily { get; set; }
		public string? UserStyleSheet { get; set; }
		public string? HeaderHtml { get; set; }
		public string? FooterHtml { get; set; }
		public string? Title { get; set; }
		public string? Author { get; set; }
		public string? Subject { get; set; }
		public string? Keywords { get; set; }
		public bool GenerateBookmarks { get; set; } = true;
		public bool CompressContent { get; set; } = true;
		public Func<string, byte[]?>? ResourceLoader { get; set; }
		internal readonly List<(byte[] data, string? family)> FontBlobs = new();

		public HtmlToPdfOptions AddFont(byte[] data, string? family = null) {
			FontBlobs.Add((data, family));
			return this;
		}

		public HtmlToPdfOptions AddFont(string path, string? family = null) {
			FontBlobs.Add((File.ReadAllBytes(path), family));
			return this;
		}
	}

	public static class HtmlToPdf {
		public static byte[] Convert(string html, HtmlToPdfOptions? options = null) => new HtmlToPdfEngine.Converter(options ?? new HtmlToPdfOptions()).Run(html);

		public static byte[] ConvertFile(string htmlPath, HtmlToPdfOptions? options = null) {
			options ??= new HtmlToPdfOptions();
			options.BaseDirectory ??= Path.GetDirectoryName(Path.GetFullPath(htmlPath));
			return Convert(File.ReadAllText(htmlPath), options);
		}

		public static void ConvertToFile(string html, string outputPath, HtmlToPdfOptions? options = null) => File.WriteAllBytes(outputPath, Convert(html, options));

		public static Task<byte[]> ConvertAsync(string html, HtmlToPdfOptions? options = null, CancellationToken ct = default) => Task.Run(() => Convert(html, options), ct);
	}

	public static class HtmlToPdfSample {
		const string WebPBase64 = "UklGRu4LAABXRUJQVlA4WAoAAAAwAAAAygAAggAASUNDUMgBAAAAAAHIAAAAAAQwAABtbnRyUkdCIFhZWiAH4AABAAEAAAAAAABhY3NwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAQAA9tYAAQAAAADTLQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAlkZXNjAAAA8AAAACRyWFlaAAABFAAAABRnWFlaAAABKAAAABRiWFlaAAABPAAAABR3dHB0AAABUAAAABRyVFJDAAABZAAAAChnVFJDAAABZAAAAChiVFJDAAABZAAAAChjcHJ0AAABjAAAADxtbHVjAAAAAAAAAAEAAAAMZW5VUwAAAAgAAAAcAHMAUgBHAEJYWVogAAAAAAAAb6IAADj1AAADkFhZWiAAAAAAAABimQAAt4UAABjaWFlaIAAAAAAAACSgAAAPhAAAts9YWVogAAAAAAAA9tYAAQAAAADTLXBhcmEAAAAAAAQAAAACZmYAAPKnAAANWQAAE9AAAApbAAAAAAAAAABtbHVjAAAAAAAAAAEAAAAMZW5VUwAAACAAAAAcAEcAbwBvAGcAbABlACAASQBuAGMALgAgADIAMAAxADZBTFBIFgYAAA3wh23/OTfbtm0Ts27QBgcnqd2kjn3YNmr3sJH0sO2jdmyjtidOZo4cnfSYcMIemT9+Ew36PY4TyxIRE8D/7deZLZkp+r/mNl5jPcaOcLS3H0Jze7tWo1QqlV1XazuVKqVKo21vb2aIvb3jCA8PjzGqEyfOd16NWfTQf4trp069ofLkSUXP1daAW/tOmaw7UFQvBIDMZ67/2V0qMQCsAm6v3VUqCGDhf0fNbw2CALLgO5NSegQBHB665sPLogDjF28+IAw4rWz4oUsUIHL+u1phwPeRjY3CwLXPfvqHMDBm5ddVwsCwVd/WCQMjV3zQIgy4P7OxXRiQx3wkDvi7xosDjxaXioPV2i+0woDrzd+LA8H1p8SBxT90i4PHjHhx4K7cy+LgcPsmcSCgVCUOljHx4kDQCY042M4uFAeCswXiWlmFOLCgUCD8DwiER/clcWDiGYGYdPrfCZfuBnHAQykQLmqB8FD+OzG8QSActQJh0yUQ1t0C4dQqEELZKhLdItElElqRaBAJ5X9TqBHI0/9SnBGISgSyQCB0JQKhQCALBUJXMBgyk7Qk0hBijCUNcxsfY2kAxvp3otlRVc81WSWY3723ykyUbo8ZKr0030TlYI5/utfBJLVvMUuXM+43SZsxz4k+chNUdt5M9Xz+rK3J6fwKc12TvNjkfI35zrZZYGKyMedf3ulqUmqSzZr2g+UjTEjD55j32m/XjDAZDe9j7mu/fc7ZRDS8j4Evcdt1SrJ8dN3XEr9oxaa+WL/cHtfbs2MkbY0Hj/fDd8k35ySWERNHVhacBWq/3+hh3aTpNLqWWAx9VsT5XcDwN3Df0Ao84rWttC+2z115t7en3CQOw/yuRGj6MnwTCdWARwJw7cKEF8HyJWDoiOOXjawuFoPPCgnIAwKBRflAONkMfNwJsJ743KzV9/fh2s/QK9vMsa9O3fHcC1VbWYM6dsZsuf+pNqMq/Q7DzwtYcKgTwtn0YFgGuNtzcBBy1dB95kxS+LF6fZaL16Bf7sodsHv4c4/8KlvOc5wNX+jiojamI7sxQpVWPu04slt489Xb98Bc4oEbnpxqe/yXU3puWDGrIufnHon+9HCmnNTnu5SN190l8VaUAuy+Y8JZm9cc7OiIDQyw6zCev3eWY5TJ0f4HmEgy3WGOF5lPOtwRBy0LH/3sR8A5n5YpQS53dfbBBzTo74x/hzdsJRnLkU7jOJ0/KwAXL8puMJq/PsBIc4LmFxFKKok3BWcTRg6ucSz/Wha094d3K4D6gFrPxOWjfu3NaQUtql7KXqKf7kvYhfSuJXLePvlwtIVR9KQmY6xF88MyddHkkBkalTLKRallCcdKIL5u8bIvgKfg3Defrf0QiJ0CrnJYw0AP30q6Ws8TgNrqtwtLrzeC8i8x3sYLoe51s9R/UTI3Om02STCJ8keBX5h4BloOA+nh8poOmO4KcOwFBtprC+di0ftk2s3rkpwfK//yxocdDazt94sYc2qkny27oT0n2ncUGeDNXVFIx52HSoArGhf3OniqCJr/ZMAn/0jxC+hXknDs+wf3tPWk7YuKtDWgzrQkjDs30M+JDCA1cpYT+dBuv+qoHhfArgOQjUYN1LQxmH4fsvtbAAs354tA3ngXv4PQvPNgVKStgXSmJWPsh2b7DyUfyA0M5QBwwdc6F5gSffESyNWNMA41gz73I94/gtQnp2XOFbCw5RJSzd6Dt4RZG0BnWjLG35n1CsUA50euIhVIuPnVTRU4bOLbGuDVN7F+g58Hzf474jTovTDi+ge+gedQN+oBzY4Mn+B59oPSUZytwCRmhpIs0e29nSyJ5b0nErpvofg0wOrwknDUPw3aYlj6qqTRR/fUD5/7JfnDffT1TPZBN7lcPlY2ADqVQqGow1TmBZAlITOUwwC3T359lTXJ20EnO5BZNJf055H2DIxOpgMWgrO9xLKOxB++vC2GU8/QzyuVCoWifqyHh8cIR3v7ITS3t2s1KqWqFpN6cir699yB3q7YNz2H1gBoneB9eRXSGccY2Id/B7g1nr4mLNO5VTCwXUqlUqnRtrc3Yz57apvovUOB4etUf/J/wAFWUDgg4gMAALAaAJ0BKssAgwA+bTKWSKQioiEmEXoAgA2JTdurnvfZn/zvOSc7TWX6vNfnp9G3mAc6bzAedV6AP8J6gH9V6kD0AP2A61H+5ec7qs6tY+wBdIkyNtIhLzLVh8+9ILmiGTXCgTsrisgUDpdKhfwqZ1gqdhP/gNiLZakaT/pmTi+3mdstegqcUDzzAdz5ta/9uYTrDml0GGiAv/5X9j7omjmCC+mxQld9Po2WqMxvlc5NpJuamQHHdPz4rQYWHJ9mldlpC02eFRprs4zqqvq4uo+D6xPPXkoqDJiRUGR4AAD+/34WX+Uzv//k24Nf/eqN88AgO4oOM+bbCK+JADpSDFqZUvTRGBvF9mxNYScVTln926yI39cqf6iey6JnwnZS6YdMAYxzmG1bgt7raz1b51hqVbOjF8GbBcHJac8wGjPgcJwhWEcuHNeFEa4GLdUK2MwinxJduj9/tXJ0wxP655uvFXkYBJxZc7UYRw6BjSi6w9stMVrwGySSxrH0PJTFianEFoRMHpjajAan8nXS5IACgd+kcdtOWZDC6ZcP0ImZxzJ2eb3UVMVvDcxoVv175vNx36YbOPC2IUJX+aFB3mZFmXlbrMmfVF6p4svWxr5N71hLsCiX3gYaC3d6cLlkv9HNt9CTPR69utahNja9tPbSHeYTcVXwzVZ/zsrsrNId6ztZ4lBNoLPAJULH+mArhxsnxq6wv/XEHeCkYbO4l/KmqFNXwm4tNnOuaBaGKMcdvdZw1H8LtYYbI8fp4QlHOvbpE3wvVG7bkXPOkiYVMMRaiRrCAwzift0YR6WpLrXpmDxb1U4ypSn4yHtiKh5eBS/XAUJpzXI7EH6boEWgsZjUHvH9k0QeORR9S48su0aoCmNzz5SiQP2ZnHHLzU5YYLpUCbtLsUmBAiVZcHFm7dMAJHsmCe9QRep3wklS6heRBtitx5bjofTspL5wQ4qiB5jh47XK/MfXF+X9Jxh/A6JtkGQIk2ZOUOePqESwl3pRD2/0B49BZrQ0q+uLl2nsBkP+qH0IZwGQbbEQKDYpJWmgG1ubaBttccNBFD/W2ynDq8Zs9d9g/2PVTKGf1Hju3Y31TBA5JvVM6ZEpMDb4JrLdFU6bUjEngqfS78Hl2LpYYTZFrCjT3AO5Dsbfj5D3pj+zJ9qduAsNsP/yg5Y3bcjCx0+etWj8rvhcp1IIN4J6tGXcaAAAAUs2fEx3L13IdAEgcbI3LxkCLNRsljEGvbZa3Icf2E+k6M6Z7fsF8RyyAhcQGkmnITDZRVsRb34E4TkZJDeANb+/5QWExA3acX+YgGK+rd1dfWLMSYAAAAAAAAA=";

		public static string Run(string outputDirectory, string? fontPath = null) {
			Directory.CreateDirectory(outputDirectory);
			string? family = null;
			if (fontPath != null && File.Exists(fontPath)) {
				family = "SampleFont";
				File.Copy(fontPath, Path.Combine(outputDirectory, "SampleFont" + Path.GetExtension(fontPath)), true);
			}
			string html = Html(family, family == null ? null : "SampleFont" + Path.GetExtension(fontPath));
			File.WriteAllText(Path.Combine(outputDirectory, "sample.html"), html);
			var options = new HtmlToPdfOptions { BaseDirectory = Path.GetFullPath(outputDirectory), Title = "HtmlToPdf Sample", Author = "SinaMN75U" };
			string pdfPath = Path.Combine(outputDirectory, "sample.pdf");
			File.WriteAllBytes(pdfPath, HtmlToPdf.Convert(html, options));
			return pdfPath;
		}

		public static string Html(string? fontFamily = null, string? fontUrl = null) {
			string font = fontFamily != null ? $"'{fontFamily}', " : "";
			string fontFace = fontUrl != null ? $"@font-face {{ font-family: '{fontFamily}'; src: url('{fontUrl}'); }}" : "";
			var sb = new StringBuilder();
			sb.Append($$"""
<!doctype html>
<html lang="fa" dir="rtl">
<head>
<meta charset="utf-8">
<title>نمونه‌ی کامل تبدیل HTML به PDF</title>
<style>
{{fontFace}}
@page { size: A4; margin: 16mm 14mm 18mm; @bottom-center { content: "صفحه " counter(page) " از " counter(pages); font-size: 9pt; color: #667; } @top-left { content: "SinaMN75U · HtmlToPdf"; font-size: 8pt; color: #99a; } }
:root { --brand: #1f5fbf; --soft: #eef3fb; }
body { font-family: {{font}}Tahoma, 'Segoe UI', sans-serif; font-size: 11pt; line-height: 1.7; color: #1d2330; margin: 0; }
h1 { color: var(--brand); font-size: 21pt; margin: 0 0 4mm; border-bottom: 4px double var(--brand); padding-bottom: 2mm; }
h2 { color: #fff; background: linear-gradient(90deg, var(--brand), #6a3fbf); padding: 1.5mm 4mm; border-radius: 6px; font-size: 13pt; margin: 7mm 0 3mm; page-break-after: avoid; }
code { font-family: Menlo, Consolas, 'DejaVu Sans Mono', monospace; background: #f1f3f7; padding: 0 1mm; border-radius: 2px; font-size: 9.5pt; direction: ltr; unicode-bidi: embed; }
.cards { display: flex; gap: 4mm; }
.card { flex: 1; background: var(--soft); border: 1px solid #c9d6ee; border-radius: 10px; padding: 3mm 4mm; box-shadow: 0 2px 6px rgba(0, 0, 0, .18); }
.card b { color: var(--brand); font-size: 18pt; display: block; line-height: 1.3; }
.langs { width: 100%; border-collapse: collapse; }
.langs td { border-bottom: 1px solid #dde3ee; padding: 1mm 2mm; vertical-align: top; }
.langs td:first-child { width: 30mm; color: #556; font-size: 9.5pt; }
table.inv { width: 100%; border-collapse: collapse; font-size: 9.5pt; }
.inv th { background: #243b63; color: #fff; padding: 1.5mm 2mm; font-weight: bold; }
.inv td { border: 1px solid #c7cfdc; padding: 1mm 2mm; }
.inv tbody tr:nth-child(even) td { background: #f3f6fb; }
.inv tfoot td { font-weight: bold; background: #e3eafa; }
.num { text-align: left; direction: ltr; }
.badge { display: inline-block; padding: 0 2mm; border-radius: 8px; font-size: 8.5pt; line-height: 1.6; }
.ok { background: #e3f4e8; color: #17703a; border: 1px solid #9fd3b0; }
.wait { background: #fff2dc; color: #9a5b00; border: 1px solid #f0c987; }
.sched { border-collapse: collapse; width: 100%; text-align: center; margin-top: 4mm; }
.sched th, .sched td { border: 1px solid #8a9bb8; padding: 1.5mm; }
.sched th { background: #dfe7f5; }
.lunch { background: #fff4d6; }
.flexrow { display: flex; gap: 3mm; align-items: center; justify-content: space-between; background: #f6f8fc; border: 1px solid #d7deea; border-radius: 6px; padding: 3mm; }
.flexrow div { background: #fff; border: 1px solid #b9c6de; border-radius: 4px; padding: 1mm 3mm; }
.flexrow .grow { flex: 1; text-align: center; background: #e9f0ff; }
.grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 3mm; margin-top: 3mm; }
.grid div { border: 2px dashed #9ab; padding: 2mm; text-align: center; border-radius: 4px; }
.grid .wide { grid-column: span 2; background: #fff7e6; border: 2px solid #f0b44c; }
.grid .tall { grid-row: span 2; background: #eaf7ef; border: 2px solid #62b37f; display: flex; align-items: center; justify-content: center; }
.imgs { display: flex; gap: 5mm; align-items: flex-end; flex-wrap: wrap; }
.imgs figure { margin: 0; text-align: center; font-size: 9pt; color: #556; }
.imgs img { display: block; border: 1px solid #aab; background: #fff; }
.cover { width: 34mm; height: 22mm; object-fit: cover; }
.borders { display: flex; flex-wrap: wrap; gap: 3mm; }
.borders div { width: 26mm; padding: 2mm 0; text-align: center; border: 5px solid #4a6fa5; font-size: 9pt; }
.stamp { display: inline-block; transform: rotate(-8deg); border: 3px solid #c0392b; color: #c0392b; padding: 0 4mm; border-radius: 6px; font-weight: bold; opacity: .85; margin: 0 4mm; }
.shadow { text-shadow: 1px 1px 0 #c8d4ea; color: var(--brand); font-weight: bold; font-size: 14pt; }
blockquote { margin: 3mm 0; padding: 2mm 4mm; border-right: 4px solid var(--brand); background: #f7f9fd; }
mark { background: #ffe98a; }
.cols { display: flex; gap: 6mm; }
.cols > div { flex: 1; }
ol.fa { list-style-type: persian; }
.page { break-before: page; }
.ltr { direction: ltr; text-align: left; }
.ltr blockquote { border-right: 0; border-left: 4px solid var(--brand); }
.justify { text-align: justify; }
pre { background: #1e2433; color: #e6e9ef; padding: 3mm 4mm; border-radius: 6px; font-family: Menlo, Consolas, 'DejaVu Sans Mono', monospace; font-size: 8.5pt; line-height: 1.5; direction: ltr; text-align: left; }
pre .k { color: #7fb4ff; } pre .s { color: #a5e07a; }
.form { display: grid; grid-template-columns: 35mm 1fr; gap: 2mm 4mm; align-items: center; }
.form input[type=text] { width: 70mm; }
</style>
</head>
<body>
<h1>نمونه‌ی کامل تبدیل HTML به PDF</h1>
<p>این سند به‌طور کامل با کد خالص <code>C#</code> و بدون هیچ پکیج، مرورگر یا اتصال اینترنتی تولید شده است. متن فارسی، عربی و عبری راست‌به‌چپ و متن انگلیسی چپ‌به‌راست در کنار هم و حتی داخل یک جمله درست نمایش داده می‌شوند؛ مثلاً نسخه‌ی 2.5.1 از متد <code>HtmlToPdf.Convert()</code> در تاریخ ۱۴۰۵/۰۷/۰۲ منتشر شد و کاربر <bdi>Sina_MN75</bdi> آن را تأیید کرد.</p>
<div class="cards">
<div class="card"><b>۱۸</b>بخش مستقل در موتور رندر</div>
<div class="card"><b>۰</b>وابستگی خارجی</div>
<div class="card"><b>A4</b>اندازه‌ی پیش‌فرض کاغذ</div>
<div class="card"><b>100%</b>پشتیبانی از راست‌به‌چپ</div>
</div>

<h2>۱. زبان‌ها و خط‌ها</h2>
<table class="langs">
<tr><td>فارسی</td><td>زندگی صحنه‌ی یکتای هنرمندی ماست؛ هر کسی نغمه‌ی خود خواند و از صحنه رود.</td></tr>
<tr><td>العربية</td><td>بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ — العلم نورٌ والجهل ظلام.</td></tr>
<tr><td>עברית</td><td>שלום עולם! זוהי דוגמה לטקסט בעברית עם המספר 2026.</td></tr>
<tr><td>English</td><td dir="ltr">The quick brown fox jumps over the lazy dog — 0123456789.</td></tr>
<tr><td>Русский</td><td dir="ltr">Съешь же ещё этих мягких французских булок, да выпей чаю.</td></tr>
<tr><td>Ελληνικά</td><td dir="ltr">Ξεσκεπάζω την ψυχοφθόρα βδελυγμία.</td></tr>
<tr><td>中文 / 日本語</td><td dir="ltr">你好，世界！ こんにちは、世界。</td></tr>
<tr><td>한국어</td><td dir="ltr">안녕하세요, 세계!</td></tr>
<tr><td>ترکیبی</td><td>قیمت این کالا <b>$1,250.00</b> است (حدود ۱۲۵ میلیون تومان) و کد آن <code>SKU-7781</code> می‌باشد.</td></tr>
</table>

<h2>۲. فاکتور و جدول چندصفحه‌ای</h2>
<table class="inv">
<thead><tr><th>ردیف</th><th>شرح کالا</th><th>تعداد</th><th>قیمت واحد (تومان)</th><th>مبلغ کل (تومان)</th><th>وضعیت</th></tr></thead>
<tbody>

""");
			string[] items = { "لپ‌تاپ ۱۵ اینچ", "مانیتور ۲۷ اینچ 4K", "کیبورد مکانیکی", "ماوس بی‌سیم", "حافظه‌ی SSD 1TB", "کابل HDMI 2.1", "هدفون نویزگیر", "وب‌کم Full HD", "روتر Wi-Fi 6", "پرینتر لیزری" };
			long total = 0;
			for (int i = 0; i < 34; i++) {
				int qty = 1 + i * 7 % 5;
				long price = 450_000L + (i * 7919L % 97) * 125_000L;
				long sum = qty * price;
				total += sum;
				bool paid = i % 3 != 1;
				sb.Append($"<tr><td>{Fa(i + 1)}</td><td>{items[i % items.Length]}</td><td>{Fa(qty)}</td><td class=\"num\">{Fa(price.ToString("N0", CultureInfo.InvariantCulture))}</td><td class=\"num\">{Fa(sum.ToString("N0", CultureInfo.InvariantCulture))}</td><td><span class=\"badge {(paid ? "ok" : "wait")}\">{(paid ? "پرداخت‌شده" : "در انتظار")}</span></td></tr>\n");
			}
			sb.Append($"</tbody>\n<tfoot><tr><td colspan=\"4\">جمع کل</td><td class=\"num\">{Fa(total.ToString("N0", CultureInfo.InvariantCulture))}</td><td></td></tr></tfoot>\n</table>\n");
			sb.Append($$"""
<table class="sched">
<tr><th>روز</th><th>۸ تا ۱۰</th><th>۱۰ تا ۱۲</th><th>۱۲ تا ۱۴</th></tr>
<tr><td>شنبه</td><td colspan="2">ریاضی پیشرفته</td><td rowspan="3" class="lunch">ناهار و استراحت</td></tr>
<tr><td>یکشنبه</td><td>فیزیک</td><td>شیمی</td></tr>
<tr><td>دوشنبه</td><td colspan="2">ادبیات فارسی</td></tr>
</table>

<h2>۳. چیدمان Flex و Grid</h2>
<div class="flexrow"><div>اول</div><div class="grow">این بخش با <code>flex: 1</code> فضای خالی را پر می‌کند</div><div>آخر</div></div>
<div class="grid">
<div class="wide">دو ستون</div><div class="tall">دو ردیف</div><div>۱</div>
<div>۲</div><div>۳</div><div>۴</div>
<div>۵</div><div class="wide">عرض دو ستون</div><div>۶</div>
</div>

<h2>۴. تصاویر و SVG</h2>
<div class="imgs">
<figure><img src="{{Png(120, 80)}}" width="120" height="80"><figcaption>PNG با شفافیت</figcaption></figure>
<figure><img src="{{Bmp(96, 64)}}" width="96" height="64"><figcaption>BMP</figcaption></figure>
<figure><img src="data:image/webp;base64,{{WebPBase64}}" width="140"><figcaption>WebP با آلفا</figcaption></figure>
<figure><img class="cover" src="{{Png(200, 60)}}"><figcaption>object-fit: cover</figcaption></figure>
<figure><img src="data:image/svg+xml;base64,{{Convert.ToBase64String(Encoding.UTF8.GetBytes(Star()))}}" width="80" height="80"><figcaption>SVG در img</figcaption></figure>
</div>
<p></p>
<svg width="100%" height="200" viewBox="0 0 640 200" xmlns="http://www.w3.org/2000/svg">
<defs><linearGradient id="bar" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#4f8df5"/><stop offset="1" stop-color="#1f5fbf"/></linearGradient></defs>
<rect x="0" y="0" width="640" height="200" rx="10" fill="#f6f8fc" stroke="#d7deea"/>
<g font-size="13" text-anchor="middle" fill="#334">
<rect x="60" y="110" width="60" height="60" fill="url(#bar)" rx="4"/><text x="90" y="190">فروردین</text>
<rect x="160" y="70" width="60" height="100" fill="url(#bar)" rx="4"/><text x="190" y="190">اردیبهشت</text>
<rect x="260" y="90" width="60" height="80" fill="url(#bar)" rx="4"/><text x="290" y="190">خرداد</text>
<rect x="360" y="40" width="60" height="130" fill="url(#bar)" rx="4"/><text x="390" y="190">تیر</text>
<rect x="460" y="55" width="60" height="115" fill="url(#bar)" rx="4"/><text x="490" y="190">مرداد</text>
</g>
<polyline points="90,100 190,60 290,80 390,30 490,45 580,20" fill="none" stroke="#e67e22" stroke-width="3" stroke-linejoin="round"/>
<circle cx="580" cy="20" r="6" fill="#e67e22"/>
<text x="620" y="30" font-size="15" text-anchor="end" fill="#1f5fbf" font-weight="bold">نمودار فروش ۱۴۰۵</text>
</svg>

<h2>۵. تایپوگرافی، حاشیه‌ها و فهرست‌ها</h2>
<p><b>ضخیم</b>، <i>مورب</i>، <u>زیرخط</u>، <s>خط‌خورده</s>، <mark>هایلایت</mark>، x<sup>2</sup> و H<sub>2</sub>O، <span class="shadow">سایه‌ی متن</span> <span class="stamp">تأیید شد</span></p>
<blockquote>«هر کسی کو دور ماند از اصل خویش / باز جوید روزگار وصل خویش» — مولانا</blockquote>
<div class="borders">
<div style="border-style: solid">solid</div><div style="border-style: dashed">dashed</div><div style="border-style: dotted">dotted</div><div style="border-style: double">double</div>
<div style="border-style: groove">groove</div><div style="border-style: ridge">ridge</div><div style="border-style: inset">inset</div><div style="border-style: outset">outset</div>
</div>
<div class="cols">
<div><ol class="fa"><li>مرحله‌ی اول</li><li>مرحله‌ی دوم<ul><li>زیرمورد دایره‌ای</li><li>زیرمورد دیگر<ul><li>سطح سوم</li></ul></li></ul></li><li>مرحله‌ی سوم</li></ol></div>
<div><ul><li>فونت‌ها از سیستم یا فایل</li><li>شکل‌دهی حروف عربی و فارسی</li><li>الگوریتم کامل دوجهته‌ی یونیکد</li><li>شکست خط و صفحه مانند Chrome</li></ul></div>
</div>

<section class="page ltr" dir="ltr">
<h2>6. Left-to-right section</h2>
<p class="justify">This page switches the whole block to <b>left-to-right</b>. Paragraphs are justified, and embedded right-to-left phrases such as <bdi>سلام دنیا</bdi> or <bdi>שלום</bdi> stay in their own direction without breaking the sentence. Numbers like 3.14159, dates like 2026-09-24 and prices like €1,999.99 keep their natural order. The layout engine implements the CSS box model, floats, positioned elements, tables, flexbox, grid, pagination with repeated table headers, and <a href="https://example.com">clickable links</a>.</p>
<blockquote>Everything here is produced by a single C# file with no dependencies.</blockquote>
<ol style="list-style-type: upper-roman"><li>Parse HTML and CSS</li><li>Resolve styles and fonts</li><li>Shape text, run bidi, break lines</li><li>Lay out boxes and paginate</li><li>Write a compact PDF with subset fonts</li></ol>
<pre><span class="k">var</span> pdf = HtmlToPdf.Convert(html, <span class="k">new</span> HtmlToPdfOptions { PaperSize = <span class="s">"A4"</span> });
File.WriteAllBytes(<span class="s">"out.pdf"</span>, pdf);</pre>
<div class="form">
<label>Full name</label><input type="text" value="Sina MohammadZadeh">
<label>Subscribe</label><div><input type="checkbox" checked> Yes, send me updates</div>
<label>Plan</label><select><option>Enterprise</option></select>
</div>
</section>
</body>
</html>
""");
			return sb.ToString();
		}

		static string Fa(long n) => Fa(n.ToString(CultureInfo.InvariantCulture));

		static string Fa(string s) {
			var c = s.ToCharArray();
			for (int i = 0; i < c.Length; i++) {
				if (c[i] >= '0' && c[i] <= '9') c[i] = (char)('۰' + c[i] - '0');
				else if (c[i] == ',') c[i] = '٬';
			}
			return new string(c);
		}

		static string Star() {
			var sb = new StringBuilder("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 100 100\"><polygon fill=\"#f5b301\" stroke=\"#b37f00\" stroke-width=\"3\" points=\"");
			for (int i = 0; i < 10; i++) {
				double r = i % 2 == 0 ? 46 : 19, a = Math.PI * i / 5 - Math.PI / 2;
				sb.Append(FormattableString.Invariant($"{50 + r * Math.Cos(a):0.##},{52 + r * Math.Sin(a):0.##} "));
			}
			return sb.Append("\"/></svg>").ToString();
		}

		static string Png(int w, int h) {
			var raw = new byte[h * (w * 4 + 1)];
			for (int y = 0; y < h; y++) {
				int o = y * (w * 4 + 1);
				for (int x = 0; x < w; x++) {
					double dx = (x - w / 2.0) / (w / 2.0), dy = (y - h / 2.0) / (h / 2.0), d = Math.Sqrt(dx * dx + dy * dy);
					int p = o + 1 + x * 4;
					raw[p] = (byte)(40 + 200 * x / w);
					raw[p + 1] = (byte)(90 + 120 * y / h);
					raw[p + 2] = (byte)(230 - 150 * x / w);
					raw[p + 3] = (byte)Math.Clamp(255 * (1.15 - d), 0, 255);
				}
			}
			byte[] idat;
			using (var ms = new MemoryStream()) {
				using (var z = new ZLibStream(ms, CompressionLevel.Optimal)) z.Write(raw);
				idat = ms.ToArray();
			}
			using var png = new MemoryStream();
			png.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
			var ihdr = new byte[13];
			ihdr[0] = (byte)(w >> 24); ihdr[1] = (byte)(w >> 16); ihdr[2] = (byte)(w >> 8); ihdr[3] = (byte)w;
			ihdr[4] = (byte)(h >> 24); ihdr[5] = (byte)(h >> 16); ihdr[6] = (byte)(h >> 8); ihdr[7] = (byte)h;
			ihdr[8] = 8; ihdr[9] = 6;
			Chunk(png, "IHDR", ihdr);
			Chunk(png, "IDAT", idat);
			Chunk(png, "IEND", Array.Empty<byte>());
			return "data:image/png;base64," + Convert.ToBase64String(png.ToArray());
		}

		static void Chunk(Stream s, string type, byte[] data) {
			var t = Encoding.ASCII.GetBytes(type);
			s.Write(new[] { (byte)(data.Length >> 24), (byte)(data.Length >> 16), (byte)(data.Length >> 8), (byte)data.Length });
			s.Write(t);
			s.Write(data);
			uint crc = 0xffffffff;
			foreach (var b in t.Concat(data)) {
				crc ^= b;
				for (int k = 0; k < 8; k++) crc = (crc & 1) != 0 ? 0xedb88320 ^ (crc >> 1) : crc >> 1;
			}
			crc = ~crc;
			s.Write(new[] { (byte)(crc >> 24), (byte)(crc >> 16), (byte)(crc >> 8), (byte)crc });
		}

		static string Bmp(int w, int h) {
			int stride = (w * 3 + 3) & ~3, size = 54 + stride * h;
			var b = new byte[size];
			void I32(int o, int v) { b[o] = (byte)v; b[o + 1] = (byte)(v >> 8); b[o + 2] = (byte)(v >> 16); b[o + 3] = (byte)(v >> 24); }
			b[0] = (byte)'B'; b[1] = (byte)'M';
			I32(2, size); I32(10, 54); I32(14, 40); I32(18, w); I32(22, h);
			b[26] = 1; b[28] = 24; I32(34, stride * h);
			for (int y = 0; y < h; y++)
				for (int x = 0; x < w; x++) {
					int o = 54 + (h - 1 - y) * stride + x * 3;
					bool check = ((x / 8) + (y / 8)) % 2 == 0;
					b[o] = (byte)(check ? 60 : 230);
					b[o + 1] = (byte)(check ? 160 : 120 + y);
					b[o + 2] = (byte)(check ? 70 : 40 + x * 2);
				}
			return "data:image/bmp;base64," + Convert.ToBase64String(b);
		}
	}
}

#nullable disable
namespace SinaMN75U.Utils.HtmlToPdfEngine {
	internal sealed class RenderContext {
		public HtmlToPdfOptions Opt;
		public FontProvider Fonts;
		public string BaseDir;
		public bool PrintBackground = true;
		public bool Outline = true;
		public List<CssRule> AuthorRules = new();
		public LayoutEngine Layout;
		readonly Dictionary<string, ImageData> _images = new();

		public RenderContext(HtmlToPdfOptions o) {
			Opt = o;
			BaseDir = o.BaseDirectory ?? Directory.GetCurrentDirectory();
			PrintBackground = o.PrintBackground;
			Outline = o.GenerateBookmarks;
			Fonts = new FontProvider(o.UseSystemFonts) { DefaultFamily = o.DefaultFontFamily };
			foreach (var dir in o.FontDirectories) if (Directory.Exists(dir)) foreach (var f in FontCache.ScanDirCached(dir)) Fonts.AddUserFace(f);
			foreach (var path in o.FontFiles) {
				try { foreach (var f in FontCache.ReadFaces(path)) Fonts.AddUserFace(f); } catch { }
			}
			foreach (var (data, family) in o.FontBlobs) {
				try {
					int n = FontFile.FaceCount(data);
					for (int i = 0; i < n; i++) {
						var ff = FontFile.Load(data, i);
						var info = FontCache.InfoFrom(ff, null, i);
						info.Data = data;
						if (family != null) info.Families = new List<string> { family }.Concat(ff.Families).ToList();
						Fonts.AddUserFace(info);
					}
				}
				catch { }
			}
		}

		public byte[] LoadBytes(string url) {
			if (string.IsNullOrWhiteSpace(url)) return null;
			url = url.Trim();
			try {
				if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) {
					int comma = url.IndexOf(',');
					if (comma < 0) return null;
					string meta = url.Substring(5, comma - 5);
					string payload = url.Substring(comma + 1);
					if (meta.EndsWith(";base64", StringComparison.OrdinalIgnoreCase)) {
						payload = new string(payload.Where(c => !char.IsWhiteSpace(c)).ToArray());
						payload = Uri.UnescapeDataString(payload);
						int pad = payload.Length % 4;
						if (pad > 0) payload += new string('=', 4 - pad);
						return global::System.Convert.FromBase64String(payload);
					}
					return Encoding.UTF8.GetBytes(Uri.UnescapeDataString(payload));
				}
				if (Opt.ResourceLoader != null) {
					var r = Opt.ResourceLoader(url);
					if (r != null) return r;
				}
				if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("//")) return null;
				string path = url;
				if (url.StartsWith("file:", StringComparison.OrdinalIgnoreCase)) path = new Uri(url).LocalPath;
				else {
					int q = path.IndexOfAny(new[] { '?', '#' });
					if (q >= 0) path = path.Substring(0, q);
					path = Uri.UnescapeDataString(path);
				}
				if (!Path.IsPathRooted(path) || path.StartsWith("/") && !File.Exists(path)) path = Path.Combine(BaseDir, path.TrimStart('/', '\\'));
				return File.Exists(path) ? File.ReadAllBytes(path) : null;
			}
			catch { return null; }
		}

		public string LoadText(string url) {
			var b = LoadBytes(url);
			if (b == null) return null;
			if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) return Encoding.UTF8.GetString(b, 3, b.Length - 3);
			return Encoding.UTF8.GetString(b);
		}

		public ImageData LoadImage(string url) {
			if (string.IsNullOrWhiteSpace(url)) return null;
			if (_images.TryGetValue(url, out var img)) return img;
			img = Images.Decode(LoadBytes(url));
			_images[url] = img;
			return img;
		}

		public void AddFontFaces(List<FontFaceRule> rules) {
			foreach (var r in rules) {
				foreach (var src in r.Src) {
					string s = src.Trim();
					byte[] data = null;
					if (s.StartsWith("local(", StringComparison.OrdinalIgnoreCase)) {
						string name = Css.Unquote(s.Substring(6, s.IndexOf(')') - 6));
						var face = Fonts.Match(name, r.WeightMin, r.Style);
						if (face?.Get() != null) {
							var info = new FaceInfo { Path = face.Path, Index = face.Index, Data = face.Data, Loaded = face.Get(), Families = new List<string> { r.Family }, Weight = r.WeightMin, WeightMax = r.WeightMax, Italic = r.Style != 0, Ranges = FontProvider.ParseUnicodeRange(r.UnicodeRange) };
							Fonts.AddWebFace(info);
							break;
						}
						continue;
					}
					string url = Css.ExtractUrl(s.Split(new[] { " format(", " tech(" }, StringSplitOptions.None)[0]);
					if (url == null) continue;
					data = LoadBytes(url);
					if (data == null) continue;
					try {
						var ff = FontFile.Load(data);
						var info = new FaceInfo { Data = data, Loaded = ff, Families = new List<string> { r.Family }, Weight = r.WeightMin, WeightMax = r.WeightMax, Italic = r.Style != 0, FullName = ff.FullName, PsName = ff.PsName, Ranges = FontProvider.ParseUnicodeRange(r.UnicodeRange) };
						ff.Key = "web#" + r.Family + "#" + r.WeightMin + "#" + r.Style + "#" + data.Length;
						Fonts.AddWebFace(info);
						break;
					}
					catch { }
				}
			}
		}
	}

	internal sealed class PageGeom {
		public double Wmm, Hmm, Mt, Mr, Mb, Ml;
		public double WPx => Wmm * 96 / 25.4;
		public double HPx => Hmm * 96 / 25.4;
		public double WPt => Wmm * 72 / 25.4;
		public double HPt => Hmm * 72 / 25.4;
		public double ContentW => Math.Max(10, WPx - Ml - Mr);
		public double ContentH => Math.Max(10, HPx - Mt - Mb);
		public double LayoutW => Math.Ceiling(ContentW - 1e-6);
		public double LayoutH => Math.Ceiling(ContentH - 1e-6);
		public double OriginX => Math.Round(Ml, MidpointRounding.AwayFromZero);
		public double OriginY => Math.Round(Mt, MidpointRounding.AwayFromZero);
		public double MediaWPt => Math.Floor(Math.Round(WPt, MidpointRounding.AwayFromZero) * 300 / 72) * 72 / 300;
		public double MediaHPt => Math.Floor(Math.Round(HPt, MidpointRounding.AwayFromZero) * 300 / 72) * 72 / 300;
	}

	internal sealed class Converter {
		readonly HtmlToPdfOptions _o;

		public Converter(HtmlToPdfOptions o) { _o = o; }

		static (double w, double h) Paper(string name) {
			switch ((name ?? "A4").Trim().ToLowerInvariant()) {
				case "a0": return (841, 1189);
				case "a1": return (594, 841);
				case "a2": return (420, 594);
				case "a3": return (297, 420);
				case "a5": return (148, 210);
				case "a6": return (105, 148);
				case "b4": return (250, 353);
				case "b5": return (176, 250);
				case "jis-b4": return (257, 364);
				case "jis-b5": return (182, 257);
				case "letter": return (215.9, 279.4);
				case "legal": return (215.9, 355.6);
				case "ledger": case "tabloid": return (279.4, 431.8);
				case "executive": return (184.15, 266.7);
				default: return (210, 297);
			}
		}

		public byte[] Run(string html) {
			var doc = HtmlParser.Parse(html ?? "");
			var ctx = new RenderContext(_o);
			var geom = new PageGeom();
			var (pw, ph) = Paper(_o.PaperSize);
			if (_o.PageWidthMm != null) pw = _o.PageWidthMm.Value;
			if (_o.PageHeightMm != null) ph = _o.PageHeightMm.Value;
			if (_o.Landscape) (pw, ph) = (ph, pw);
			geom.Wmm = pw; geom.Hmm = ph;
			double defM = 28 * 96.0 / 72;
			geom.Mt = geom.Mr = geom.Mb = geom.Ml = defM;
			var env = new MediaEnv { Type = _o.MediaType, Width = geom.ContentW, Height = geom.ContentH };
			var sheet = LoadSheets(doc, ctx, env);
			ApplyPageRules(sheet, geom);
			env.Width = geom.ContentW; env.Height = geom.ContentH;
			sheet = LoadSheets(doc, ctx, env);
			ApplyPageRules(sheet, geom);
			if (_o.MarginTopMm != null) geom.Mt = _o.MarginTopMm.Value * 96 / 25.4;
			if (_o.MarginRightMm != null) geom.Mr = _o.MarginRightMm.Value * 96 / 25.4;
			if (_o.MarginBottomMm != null) geom.Mb = _o.MarginBottomMm.Value * 96 / 25.4;
			if (_o.MarginLeftMm != null) geom.Ml = _o.MarginLeftMm.Value * 96 / 25.4;
			ctx.AddFontFaces(sheet.FontFaces);
			if (!ctx.Fonts.HasAnyFont()) throw new InvalidOperationException("No fonts are available. Add fonts with HtmlToPdfOptions.AddFont, FontFiles or FontDirectories.");
			ctx.AuthorRules = sheet.Rules.Where(r => r.Origin == 1).ToList();
			var resolver = new StyleResolver(sheet.Rules, env) { PrintBackground = ctx.PrintBackground };
			double scale = _o.Scale > 0 ? _o.Scale : 1;
			double layoutW = Math.Ceiling(geom.ContentW / scale - 1e-6), layoutH = Math.Ceiling(geom.ContentH / scale - 1e-6);
			resolver.ComputeTree(doc, new LenCtx { Em = 16, Rem = 16, Vw = geom.ContentW / scale, Vh = geom.ContentH / scale });
			var (engine, root) = LayoutAll(doc, ctx, layoutW, layoutH);
			if (_o.ShrinkToFit) {
				double overflow = MaxRight(root);
				if (overflow > layoutW + 1) {
					double newW = Math.Min(overflow, layoutW * 2);
					double f = newW / layoutW;
					scale /= f;
					layoutW = newW; layoutH *= f;
					(engine, root) = LayoutAll(doc, ctx, layoutW, layoutH);
				}
			}
			Painter.ComputeExtents(root);
			double bottom = ContentBottom(root);
			int pages = Math.Max(1, (int)Math.Ceiling((bottom - 0.5) / layoutH));
			var pdf = new PdfDoc {
				Compress = _o.CompressContent,
				Title = _o.Title ?? (doc.Title.Length > 0 ? doc.Title : null),
				Author = _o.Author, Subject = _o.Subject, Keywords = _o.Keywords,
				Lang = doc.Lang,
				Rtl = doc.Root.Style?.Rtl == true || doc.Body?.Style?.Rtl == true
			};
			var painter = new Painter(pdf, engine, ctx) {
				MarginLeft = geom.OriginX / scale, MarginTop = geom.OriginY / scale,
				PageWpx = geom.MediaWPt * 4 / 3 / scale, PageHpx = geom.MediaHPt * 4 / 3 / scale, PageHpt = geom.MediaHPt / scale,
				ClipH = geom.ContentH / scale, ClipW = geom.ContentW / scale
			};
			string title = pdf.Title ?? "";
			for (int p = 0; p < pages; p++) {
				var c = new PdfContent();
				if (Math.Abs(scale - 1) > 1e-9) c.Op(F.N(scale) + " 0 0 " + F.N(scale) + " 0 " + F.N(geom.MediaHPt - geom.MediaHPt * scale) + " cm");
				painter.PaintPage(c, root, p, layoutH, engine.Fixed);
				PaintMarginContent(c, pdf, ctx, sheet, geom, p, pages, title, scale);
				pdf.AddPage(c, geom.MediaWPt, geom.MediaHPt);
			}
			return pdf.Finish();
		}

		static double ContentBottom(Box root) {
			double bottom = root.Y + root.H + root.M[2];
			void Walk(Box b) {
				foreach (var k in b.Kids) {
					if (k.Kind == BK.Text || k.S.Position == Pos.Fixed) continue;
					if (k.S.Visibility == 0 || k.Kids.Count > 0) {
						double kb = k.Y + k.H;
						if (k.Kind == BK.Table && !double.IsNaN(k.TH)) kb = Math.Max(kb, k.Y + k.TY + k.TH);
						if (k.H > 0 || k.Kind == BK.Replaced) bottom = Math.Max(bottom, kb);
						if (k.Lines != null && k.Lines.Count > 0) bottom = Math.Max(bottom, k.Lines[k.Lines.Count - 1].Y + k.Lines[k.Lines.Count - 1].H);
					}
					if (k.S.OverflowX < 2 && k.S.OverflowY < 2) Walk(k);
				}
			}
			Walk(root);
			return bottom;
		}

		static double MaxRight(Box root) {
			double r = 0;
			void Walk(Box b, bool clipped) {
				if (b.Kind == BK.Text) return;
				if (!clipped && b.S.Position != Pos.Fixed && b.S.Position != Pos.Absolute) r = Math.Max(r, b.X + b.W);
				if (b.Lines != null && !clipped) foreach (var l in b.Lines) foreach (var g in l.Glyphs) r = Math.Max(r, g.X1);
				bool c = clipped || b.S.OverflowX >= 2;
				foreach (var k in b.Kids) Walk(k, c);
			}
			Walk(root, false);
			return r;
		}

		(LayoutEngine, Box) LayoutAll(HtmlDocument doc, RenderContext ctx, double w, double h) {
			var builder = new BoxBuilder(ctx);
			var root = builder.BuildRoot(doc.Root);
			BoxBuilder.AssignDecorations(root, null);
			var engine = new LayoutEngine(ctx) { PageW = w, PageH = h, Paginate = true };
			ctx.Layout = engine;
			engine.LayoutDocument(root);
			return (engine, root);
		}

		StyleSheet LoadSheets(HtmlDocument doc, RenderContext ctx, MediaEnv env) {
			var sheet = new StyleSheet();
			int order = 0;
			Css.ParseSheet(Css.UserAgentSheet, sheet, 0, env, ref order, null, null);
			foreach (var e in doc.All) {
				if (e.Tag == "style") {
					string media = e.Attr("media");
					if (media != null && !Css.MediaMatches(media, env)) continue;
					Css.ParseSheet(e.TextContent(), sheet, 1, env, ref order, u => ctx.LoadText(u), ctx.BaseDir);
				}
				else if (e.Tag == "link") {
					string rel = (e.Attr("rel") ?? "").ToLowerInvariant();
					if (!rel.Split(' ').Contains("stylesheet") || rel.Contains("alternate")) continue;
					string media = e.Attr("media");
					if (media != null && !Css.MediaMatches(media, env)) continue;
					string text = ctx.LoadText(e.Attr("href"));
					if (text != null) Css.ParseSheet(text, sheet, 1, env, ref order, u => ctx.LoadText(u), ctx.BaseDir);
				}
			}
			if (!string.IsNullOrWhiteSpace(_o.UserStyleSheet)) Css.ParseSheet(_o.UserStyleSheet, sheet, 1, env, ref order, u => ctx.LoadText(u), ctx.BaseDir);
			return sheet;
		}

		void ApplyPageRules(StyleSheet sheet, PageGeom g) {
			var ctx = new LenCtx { Em = 16, Rem = 16 };
			foreach (var pr in sheet.Pages) {
				string sel = (pr.Selector ?? "").Trim();
				if (sel.Length > 0 && !sel.StartsWith(":first") && sel != ":left" && sel != ":right") continue;
				if (sel.StartsWith(":first")) continue;
				foreach (var d in pr.Decls) {
					string v = d.Value.Trim().ToLowerInvariant();
					if (d.Prop == "size" && _o.PreferCssPageSize && _o.PageWidthMm == null) {
						var parts = v.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
						bool land = parts.Remove("landscape");
						parts.Remove("portrait");
						double w = g.Wmm, h = g.Hmm;
						if (parts.Count == 1 && parts[0] != "auto") {
							var l = Val.ParseLen(parts[0], ctx);
							if (l != null && l.Value.IsValue) { w = h = l.Value.Px * 25.4 / 96; }
							else (w, h) = Paper(parts[0]);
						}
						else if (parts.Count >= 2) {
							var lw = Val.ParseLen(parts[0], ctx); var lh = Val.ParseLen(parts[1], ctx);
							if (lw != null && lh != null) { w = lw.Value.Px * 25.4 / 96; h = lh.Value.Px * 25.4 / 96; }
							else { (w, h) = Paper(parts[0]); if (parts[1] == "landscape") land = true; }
						}
						if (land && w < h) (w, h) = (h, w);
						g.Wmm = w; g.Hmm = h;
					}
					else if (d.Prop.StartsWith("margin")) {
						foreach (var (p, val) in Shorthands.Expand(d.Prop, d.Value, false)) {
							var l = Val.ParseLen(val, new LenCtx { Em = 16, Rem = 16, Vw = g.WPx, Vh = g.HPx });
							if (l == null || !l.Value.IsValue) continue;
							double px = p.EndsWith("top") || p.EndsWith("bottom") ? l.Value.Resolve(g.WPx) : l.Value.Resolve(g.WPx);
							switch (p) {
								case "margin-top": g.Mt = px; break;
								case "margin-right": g.Mr = px; break;
								case "margin-bottom": g.Mb = px; break;
								case "margin-left": g.Ml = px; break;
							}
						}
					}
				}
			}
		}

		void PaintMarginContent(PdfContent c, PdfDoc pdf, RenderContext ctx, StyleSheet sheet, PageGeom g, int page, int pages, string title, double scale) {
			string date = DateTime.Now.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
			string Fill(string tpl) {
				if (tpl == null) return null;
				string r = tpl.Replace("{page}", (page + 1).ToString(CultureInfo.InvariantCulture)).Replace("{pages}", pages.ToString(CultureInfo.InvariantCulture)).Replace("{date}", date).Replace("{title}", title);
				r = ReplaceClass(r, "pageNumber", (page + 1).ToString(CultureInfo.InvariantCulture));
				r = ReplaceClass(r, "totalPages", pages.ToString(CultureInfo.InvariantCulture));
				r = ReplaceClass(r, "date", date);
				r = ReplaceClass(r, "title", System.Net.WebUtility.HtmlEncode(title));
				r = ReplaceClass(r, "url", "");
				return r;
			}
			if (_o.HeaderHtml != null) PaintSnippet(c, pdf, ctx, Fill(_o.HeaderHtml), 0, 0, g.WPx, g.Mt, false, g);
			if (_o.FooterHtml != null) PaintSnippet(c, pdf, ctx, Fill(_o.FooterHtml), 0, g.HPx - g.Mb, g.WPx, g.Mb, true, g);
			foreach (var pr in sheet.Pages) {
				string sel = (pr.Selector ?? "").Trim();
				if (sel == ":first" && page != 0) continue;
				if (sel == ":left" && page % 2 == 0 || sel == ":right" && page % 2 == 1) continue;
				if (sel.Length > 0 && sel != ":first" && sel != ":left" && sel != ":right") continue;
				foreach (var kv in pr.MarginBoxes) {
					var content = kv.Value.LastOrDefault(d => d.Prop == "content");
					if (content == null) continue;
					string text = EvalPageContent(content.Value, page + 1, pages, title);
					if (string.IsNullOrEmpty(text)) continue;
					var style = new StringBuilder();
					foreach (var d in kv.Value) if (d.Prop != "content") style.Append(d.Prop).Append(':').Append(d.Value).Append(';');
					string name = kv.Key;
					double x, y, w, h;
					bool top = name.StartsWith("top"), bottom = name.StartsWith("bottom");
					string align = name.EndsWith("left") ? "left" : name.EndsWith("right") ? "right" : "center";
					if (top || bottom) {
						x = g.Ml; w = g.WPx - g.Ml - g.Mr; h = top ? g.Mt : g.Mb; y = top ? 0 : g.HPx - g.Mb;
						if (name.EndsWith("left-corner")) { x = 0; w = g.Ml; align = "right"; }
						else if (name.EndsWith("right-corner")) { x = g.WPx - g.Mr; w = g.Mr; align = "left"; }
					}
					else {
						bool left = name.StartsWith("left");
						x = left ? 0 : g.WPx - g.Mr; w = left ? g.Ml : g.Mr; y = g.Mt; h = g.HPx - g.Mt - g.Mb;
						align = "center";
					}
					string htmlSnip = "<html><body style=\"margin:0\"><div style=\"display:flex;align-items:center;justify-content:" + (align == "left" ? "flex-start" : align == "right" ? "flex-end" : "center") + ";height:" + F.N(h) + "px;font-size:8pt;text-align:" + align + ";" + System.Net.WebUtility.HtmlEncode(style.ToString()) + "\"><div>" + System.Net.WebUtility.HtmlEncode(text) + "</div></div></body></html>";
					PaintSnippet(c, pdf, ctx, htmlSnip, x, y, w, h, false, g);
				}
			}
		}

		static string EvalPageContent(string v, int page, int pages, string title) {
			var sb = new StringBuilder();
			foreach (var t in Css.SplitWs(v)) {
				string lt = t.ToLowerInvariant();
				if (t.StartsWith("\"") || t.StartsWith("'")) sb.Append(Css.Unquote(t));
				else if (lt.StartsWith("counter(")) {
					var args = Css.SplitTopLevel(t.Substring(8, t.Length - 9), ',').Select(x => x.Trim()).ToList();
					int n = args[0] == "pages" ? pages : page;
					sb.Append(BoxBuilder.CounterText(n, args.Count > 1 ? args[1] : "decimal"));
				}
				else if (lt.StartsWith("string(")) sb.Append(title);
			}
			return sb.ToString();
		}

		static string ReplaceClass(string html, string cls, string value) {
			int i = 0;
			while (true) {
				int k = html.IndexOf("class=\"" + cls + "\"", i, StringComparison.Ordinal);
				if (k < 0) k = html.IndexOf("class='" + cls + "'", i, StringComparison.Ordinal);
				if (k < 0) return html;
				int gt = html.IndexOf('>', k);
				if (gt < 0) return html;
				int close = html.IndexOf("</", gt, StringComparison.Ordinal);
				if (close < 0) return html;
				html = html.Substring(0, gt + 1) + value + html.Substring(close);
				i = gt + 1 + value.Length;
			}
		}

		void PaintSnippet(PdfContent c, PdfDoc pdf, RenderContext parentCtx, string html, double x, double y, double w, double h, bool alignBottom, PageGeom g) {
			if (string.IsNullOrWhiteSpace(html)) return;
			var doc = HtmlParser.Parse(html);
			var o = _o;
			var ctx = new RenderContext(o) { Fonts = parentCtx.Fonts };
			var env = new MediaEnv { Type = "print", Width = w, Height = h };
			var sheet = new StyleSheet();
			int order = 0;
			Css.ParseSheet(Css.UserAgentSheet + " body { margin: 0 }", sheet, 0, env, ref order, null, null);
			foreach (var e in doc.All.Where(e => e.Tag == "style")) Css.ParseSheet(e.TextContent(), sheet, 1, env, ref order, null, null);
			ctx.AddFontFaces(sheet.FontFaces);
			var resolver = new StyleResolver(sheet.Rules, env);
			resolver.ComputeTree(doc, new LenCtx { Em = 16, Rem = 16, Vw = w, Vh = h });
			var builder = new BoxBuilder(ctx);
			var root = builder.BuildRoot(doc.Root);
			BoxBuilder.AssignDecorations(root, null);
			var engine = new LayoutEngine(ctx) { PageW = w, PageH = double.PositiveInfinity, Paginate = false };
			ctx.Layout = engine;
			engine.LayoutDocument(root);
			Painter.ComputeExtents(root);
			double contentH = ContentBottom(root);
			double oy = alignBottom ? y + h - contentH : y;
			if (alignBottom && contentH > h) oy = y;
			var p = new Painter(pdf, engine, ctx) { MarginLeft = 0, MarginTop = 0, PageWpx = g.MediaWPt * 4 / 3, PageHpx = g.MediaHPt * 4 / 3, PageHpt = g.MediaHPt };
			var sub = new PdfContent();
			sub.Op("q 0.75 0 0 -0.75 0 " + F.N(g.MediaHPt) + " cm 1 0 0 1 " + F.N(x) + " " + F.N(oy) + " cm");
			p.PaintSnippet(sub, root);
			sub.Op("Q");
			c.Sb.Append(sub.Sb);
			foreach (var f in sub.Fonts) c.Fonts.Add(f);
			foreach (var f in sub.Images) c.Images.Add(f);
			foreach (var f in sub.States) c.States.Add(f);
			foreach (var f in sub.Shadings) c.Shadings.Add(f);
			foreach (var f in sub.XObjects) c.XObjects.Add(f);
			foreach (var f in sub.Patterns) c.Patterns.Add(f);
		}
	}

	internal class Node {
		public Element Parent;
		public List<Node> Children = new();
		public int Index;
	}

	internal sealed class TextNode : Node {
		public string Text;
		public TextNode(string t) { Text = t; }
	}

	internal sealed class Element : Node {
		public string Tag;
		public Dictionary<string, string> Attrs = new(StringComparer.OrdinalIgnoreCase);
		public string Id;
		public string[] Classes = Array.Empty<string>();
		public Style Style;
		public Style Before, After, Marker;
		public bool IsSvgChild;
		public int ElementIndex;
		public Element(string tag) { Tag = tag; }

		public string Attr(string n) => Attrs.TryGetValue(n, out var v) ? v : null;

		public void SyncAttrs() {
			Id = Attr("id");
			string c = Attr("class");
			Classes = string.IsNullOrWhiteSpace(c) ? Array.Empty<string>() : c.Split(new[] { ' ', '\t', '\n', '\r', '\f' }, StringSplitOptions.RemoveEmptyEntries);
		}

		public IEnumerable<Element> ChildElements() {
			foreach (var c in Children) if (c is Element e) yield return e;
		}

		public Element PrevElementSibling() {
			if (Parent == null) return null;
			for (int i = Index - 1; i >= 0; i--) if (Parent.Children[i] is Element e) return e;
			return null;
		}

		public Element NextElementSibling() {
			if (Parent == null) return null;
			for (int i = Index + 1; i < Parent.Children.Count; i++) if (Parent.Children[i] is Element e) return e;
			return null;
		}

		public string TextContent() {
			var sb = new StringBuilder();
			void Walk(Node n) {
				if (n is TextNode t) sb.Append(t.Text);
				else foreach (var c in n.Children) Walk(c);
			}
			Walk(this);
			return sb.ToString();
		}

		public IEnumerable<Element> Descendants() {
			foreach (var c in Children) {
				if (c is Element e) {
					yield return e;
					foreach (var d in e.Descendants()) yield return d;
				}
			}
		}
	}

	internal sealed class HtmlDocument {
		public Element Root, Head, Body;
		public string Title = "";
		public string Lang;
		public List<Element> All = new();
	}

	internal static class HtmlParser {
		static readonly HashSet<string> Void = new(StringComparer.OrdinalIgnoreCase) { "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr", "keygen", "basefont", "frame" };
		static readonly HashSet<string> ClosesP = new(StringComparer.OrdinalIgnoreCase) { "address", "article", "aside", "blockquote", "center", "details", "dialog", "dir", "div", "dl", "fieldset", "figcaption", "figure", "footer", "form", "h1", "h2", "h3", "h4", "h5", "h6", "header", "hgroup", "hr", "main", "menu", "nav", "ol", "p", "pre", "section", "summary", "table", "ul", "listing", "xmp", "plaintext", "search" };
		static readonly HashSet<string> HeadTags = new(StringComparer.OrdinalIgnoreCase) { "meta", "link", "style", "title", "base", "script", "noscript", "template" };
		static readonly HashSet<string> RawText = new(StringComparer.OrdinalIgnoreCase) { "script", "style", "xmp", "iframe", "noembed", "noframes", "textarea", "title", "plaintext" };
		static readonly HashSet<string> Scoping = new(StringComparer.OrdinalIgnoreCase) { "html", "table", "td", "th", "caption", "marquee", "object", "applet", "template", "svg", "math" };

		public static HtmlDocument Parse(string html) {
			var doc = new HtmlDocument();
			var root = new Element("html");
			var head = new Element("head");
			var body = new Element("body");
			bool bodyStarted = false;
			var stack = new List<Element> { root };
			Element Cur() => stack[stack.Count - 1];

			void EnsureBody() {
				if (bodyStarted) return;
				bodyStarted = true;
				if (head.Parent == null) Append(root, head);
				Append(root, body);
				stack.Clear();
				stack.Add(root);
				stack.Add(body);
			}

			int Find(string tag, bool scoped) {
				for (int i = stack.Count - 1; i >= 0; i--) {
					if (stack[i].Tag == tag) return i;
					if (scoped && Scoping.Contains(stack[i].Tag)) return -1;
				}
				return -1;
			}

			void PopTo(int i) { if (i >= 1) stack.RemoveRange(i, stack.Count - i); }

			void CloseP() {
				int i = Find("p", true);
				if (i > 0) PopTo(i);
			}

			bool inSvg() => stack.Any(e => e.Tag == "svg");

			int pos = 0, len = html.Length;
			var text = new StringBuilder();

			void FlushText() {
				if (text.Length == 0) return;
				string t = text.ToString();
				text.Clear();
				if (!bodyStarted) {
					if (string.IsNullOrWhiteSpace(t)) return;
					EnsureBody();
				}
				var c = Cur();
				if (c.Tag == "table" || c.Tag == "tbody" || c.Tag == "thead" || c.Tag == "tfoot" || c.Tag == "tr") {
					if (string.IsNullOrWhiteSpace(t)) { AppendText(c, t); return; }
				}
				AppendText(c, t);
			}

			while (pos < len) {
				char ch = html[pos];
				if (ch == '<' && pos + 1 < len) {
					char nx = html[pos + 1];
					if (nx == '!') {
						FlushText();
						if (string.CompareOrdinal(html, pos, "<!--", 0, 4) == 0) {
							int e = html.IndexOf("-->", pos + 4, StringComparison.Ordinal);
							pos = e < 0 ? len : e + 3;
						}
						else if (pos + 9 <= len && string.Compare(html, pos, "<![CDATA[", 0, 9, StringComparison.Ordinal) == 0) {
							int e = html.IndexOf("]]>", pos + 9, StringComparison.Ordinal);
							string cd = e < 0 ? html.Substring(pos + 9) : html.Substring(pos + 9, e - pos - 9);
							text.Append(cd);
							pos = e < 0 ? len : e + 3;
						}
						else {
							int e = html.IndexOf('>', pos);
							pos = e < 0 ? len : e + 1;
						}
						continue;
					}
					if (nx == '?') {
						FlushText();
						int e = html.IndexOf('>', pos);
						pos = e < 0 ? len : e + 1;
						continue;
					}
					if (nx == '/') {
						int p2 = pos + 2;
						int ns = p2;
						while (p2 < len && !char.IsWhiteSpace(html[p2]) && html[p2] != '>' && html[p2] != '/') p2++;
						if (p2 == ns) { text.Append('<'); pos++; continue; }
						FlushText();
						string name = html.Substring(ns, p2 - ns);
						bool svg = inSvg();
						if (!svg) name = name.ToLowerInvariant();
						int e = html.IndexOf('>', p2);
						pos = e < 0 ? len : e + 1;
						string lname = name.ToLowerInvariant();
						if (lname == "html" || lname == "head") continue;
						if (lname == "body") continue;
						if (lname == "br") { EnsureBody(); Append(Cur(), new Element("br")); continue; }
						if (lname == "p") {
							int i = Find("p", true);
							if (i > 0) PopTo(i);
							else { EnsureBody(); Append(Cur(), new Element("p")); }
							continue;
						}
						for (int i = stack.Count - 1; i >= 1; i--) {
							if (string.Equals(stack[i].Tag, name, StringComparison.OrdinalIgnoreCase)) { PopTo(i); break; }
							if (!svg && Scoping.Contains(stack[i].Tag) && lname != "table" && lname != stack[i].Tag && lname != "td" && lname != "th" && lname != "tr" && lname != "tbody" && lname != "thead" && lname != "tfoot" && lname != "caption") break;
						}
						continue;
					}
					if (char.IsLetter(nx)) {
						FlushText();
						int p2 = pos + 1;
						int ns = p2;
						while (p2 < len && !char.IsWhiteSpace(html[p2]) && html[p2] != '>' && html[p2] != '/') p2++;
						string name = html.Substring(ns, p2 - ns);
						bool svgCtx = inSvg() || name.Equals("svg", StringComparison.OrdinalIgnoreCase);
						string lname = name.ToLowerInvariant();
						if (!svgCtx) name = lname;
						var el = new Element(svgCtx ? (lname == "svg" ? "svg" : name) : lname);
						el.IsSvgChild = svgCtx && lname != "svg";
						bool selfClose = false;
						while (p2 < len) {
							while (p2 < len && (char.IsWhiteSpace(html[p2]))) p2++;
							if (p2 >= len) break;
							if (html[p2] == '>') { p2++; break; }
							if (html[p2] == '/') { p2++; if (p2 < len && html[p2] == '>') { selfClose = true; p2++; break; } continue; }
							int an = p2;
							while (p2 < len && !char.IsWhiteSpace(html[p2]) && html[p2] != '=' && html[p2] != '>' && !(html[p2] == '/' && p2 + 1 < len && html[p2 + 1] == '>')) p2++;
							string aname = html.Substring(an, p2 - an);
							if (!svgCtx) aname = aname.ToLowerInvariant();
							while (p2 < len && char.IsWhiteSpace(html[p2])) p2++;
							string aval = "";
							if (p2 < len && html[p2] == '=') {
								p2++;
								while (p2 < len && char.IsWhiteSpace(html[p2])) p2++;
								if (p2 < len && (html[p2] == '"' || html[p2] == '\'')) {
									char q = html[p2++];
									int ve = html.IndexOf(q, p2);
									if (ve < 0) ve = len;
									aval = DecodeEntities(html.Substring(p2, ve - p2), true);
									p2 = Math.Min(len, ve + 1);
								}
								else {
									int vs = p2;
									while (p2 < len && !char.IsWhiteSpace(html[p2]) && html[p2] != '>') p2++;
									aval = DecodeEntities(html.Substring(vs, p2 - vs), true);
								}
							}
							if (aname.Length > 0 && !el.Attrs.ContainsKey(aname)) el.Attrs[aname] = aval;
						}
						pos = p2;
						el.SyncAttrs();
						string tag = el.Tag;
						if (tag == "html") {
							foreach (var kv in el.Attrs) if (!root.Attrs.ContainsKey(kv.Key)) root.Attrs[kv.Key] = kv.Value;
							root.SyncAttrs();
							continue;
						}
						if (tag == "head") { if (head.Parent == null && !bodyStarted) { Append(root, head); stack.Add(head); } continue; }
						if (tag == "body") {
							foreach (var kv in el.Attrs) if (!body.Attrs.ContainsKey(kv.Key)) body.Attrs[kv.Key] = kv.Value;
							body.SyncAttrs();
							EnsureBody();
							continue;
						}
						if (!bodyStarted && HeadTags.Contains(tag)) {
							if (head.Parent == null) Append(root, head);
							Append(head, el);
						}
						else {
							EnsureBody();
							if (!svgCtx) {
								if (ClosesP.Contains(tag)) CloseP();
								if (tag == "li") { int i = FindListItem(stack, "li"); if (i > 0) PopTo(i); }
								else if (tag == "dd" || tag == "dt") { int i = FindListItem(stack, "dd"); if (i < 0) i = FindListItem(stack, "dt"); if (i > 0) PopTo(i); }
								else if (tag == "option") { if (Cur().Tag == "option") PopTo(stack.Count - 1); }
								else if (tag == "optgroup") { if (Cur().Tag == "option") PopTo(stack.Count - 1); if (Cur().Tag == "optgroup") PopTo(stack.Count - 1); }
								else if (tag == "tr") {
									int i = FindInTable(stack, "tr"); if (i > 0) PopTo(i);
									var c = Cur();
									if (c.Tag == "table") { var tb = new Element("tbody"); Append(c, tb); stack.Add(tb); }
								}
								else if (tag == "td" || tag == "th") {
									int i = FindInTable(stack, "td"); if (i < 0) i = FindInTable(stack, "th"); if (i > 0) PopTo(i);
									var c = Cur();
									if (c.Tag == "table") { var tb = new Element("tbody"); Append(c, tb); stack.Add(tb); c = tb; }
									if (c.Tag == "tbody" || c.Tag == "thead" || c.Tag == "tfoot") { var tr = new Element("tr"); Append(c, tr); stack.Add(tr); }
								}
								else if (tag == "tbody" || tag == "thead" || tag == "tfoot") {
									int i = FindInTable(stack, "tbody"); if (i < 0) i = FindInTable(stack, "thead"); if (i < 0) i = FindInTable(stack, "tfoot"); if (i > 0) PopTo(i);
								}
								else if (tag == "col") { if (Cur().Tag == "table") { var cg = new Element("colgroup"); Append(Cur(), cg); stack.Add(cg); Append(cg, el); PopTo(stack.Count - 1); continue; } }
								else if (tag == "a") { int i = Find("a", false); if (i > 0) PopTo(i); }
								else if (tag == "h1" || tag == "h2" || tag == "h3" || tag == "h4" || tag == "h5" || tag == "h6") {
									var c = Cur(); if (c.Tag.Length == 2 && c.Tag[0] == 'h' && char.IsDigit(c.Tag[1])) PopTo(stack.Count - 1);
								}
							}
							Append(Cur(), el);
						}
						bool isVoid = !svgCtx && Void.Contains(tag);
						if (selfClose && svgCtx) isVoid = true;
						if (!isVoid) {
							if (!svgCtx && RawText.Contains(tag)) {
								string endTag = "</" + tag;
								int e = IndexOfCI(html, endTag, pos);
								string raw = e < 0 ? html.Substring(pos) : html.Substring(pos, e - pos);
								if (tag == "textarea" || tag == "title") raw = DecodeEntities(raw, false);
								if (tag == "textarea" && raw.StartsWith("\n")) raw = raw.Substring(1);
								if (raw.Length > 0) AppendText(el, raw);
								if (e < 0) pos = len;
								else { int gt = html.IndexOf('>', e); pos = gt < 0 ? len : gt + 1; }
							}
							else {
								if (el.Parent != head || bodyStarted) stack.Add(el);
								if (tag == "pre" || tag == "listing") {
									if (pos < len && html[pos] == '\n') pos++;
									else if (pos + 1 < len && html[pos] == '\r' && html[pos + 1] == '\n') pos += 2;
								}
							}
						}
						continue;
					}
				}
				if (ch == '&') {
					int e = pos + 1;
					while (e < len && e - pos < 34 && (char.IsLetterOrDigit(html[e]) || html[e] == '#')) e++;
					if (e < len && html[e] == ';') e++;
					text.Append(DecodeEntities(html.Substring(pos, e - pos), false));
					pos = e;
					continue;
				}
				if (ch == '\r') {
					text.Append('\n');
					pos++;
					if (pos < len && html[pos] == '\n') pos++;
					continue;
				}
				text.Append(ch);
				pos++;
			}
			FlushText();
			if (!bodyStarted) EnsureBody();
			doc.Root = root;
			doc.Head = head;
			doc.Body = body;
			var titleEl = head.Descendants().FirstOrDefault(x => x.Tag == "title");
			if (titleEl != null) doc.Title = CollapseWs(titleEl.TextContent()).Trim();
			doc.Lang = root.Attr("lang");
			int idx = 0;
			void Number(Element e) {
				e.ElementIndex = idx++;
				doc.All.Add(e);
				for (int i = 0; i < e.Children.Count; i++) {
					e.Children[i].Index = i;
					e.Children[i].Parent = e;
					if (e.Children[i] is Element c) Number(c);
				}
			}
			Number(root);
			return doc;
		}

		static int FindListItem(List<Element> stack, string tag) {
			for (int i = stack.Count - 1; i >= 1; i--) {
				string t = stack[i].Tag;
				if (t == tag) return i;
				if (t == "ul" || t == "ol" || t == "dl" || t == "menu" || Scoping.Contains(t)) return -1;
				if (t != "p" && t != "span" && t != "b" && t != "i" && t != "a" && t != "em" && t != "strong" && t != "font" && t != "small" && t != "u") {
					if (ClosesP.Contains(t) && t != "div" && t != "p") return -1;
				}
			}
			return -1;
		}

		static int FindInTable(List<Element> stack, string tag) {
			for (int i = stack.Count - 1; i >= 1; i--) {
				if (stack[i].Tag == tag) return i;
				if (stack[i].Tag == "table") return -1;
			}
			return -1;
		}

		static int IndexOfCI(string s, string v, int start) => s.IndexOf(v, start, StringComparison.OrdinalIgnoreCase);

		static void Append(Element parent, Node child) {
			child.Parent = parent;
			child.Index = parent.Children.Count;
			parent.Children.Add(child);
		}

		static void AppendText(Element parent, string t) {
			if (parent.Children.Count > 0 && parent.Children[parent.Children.Count - 1] is TextNode tn) { tn.Text += t; return; }
			Append(parent, new TextNode(t));
		}

		public static string CollapseWs(string s) {
			var sb = new StringBuilder(s.Length);
			bool ws = false;
			foreach (char c in s) {
				if (c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f') { if (!ws) sb.Append(' '); ws = true; }
				else { sb.Append(c); ws = false; }
			}
			return sb.ToString();
		}

		public static string DecodeEntities(string s, bool attr) {
			if (s.IndexOf('&') < 0) return s;
			var sb = new StringBuilder(s.Length);
			int i = 0;
			while (i < s.Length) {
				char c = s[i];
				if (c != '&') { sb.Append(c); i++; continue; }
				int j = i + 1;
				if (j < s.Length && s[j] == '#') {
					j++;
					bool hex = j < s.Length && (s[j] == 'x' || s[j] == 'X');
					if (hex) j++;
					int st = j;
					while (j < s.Length && (hex ? Uri.IsHexDigit(s[j]) : char.IsDigit(s[j]))) j++;
					if (j > st) {
						long v;
						if (!long.TryParse(s.AsSpan(st, Math.Min(j - st, 9)), hex ? NumberStyles.HexNumber : NumberStyles.Integer, CultureInfo.InvariantCulture, out v)) v = 0xFFFD;
						if (v == 0 || v > 0x10FFFF || v >= 0xD800 && v <= 0xDFFF) v = 0xFFFD;
						if (v >= 0x80 && v <= 0x9F) v = Win1252(v);
						sb.Append(char.ConvertFromUtf32((int)v));
						if (j < s.Length && s[j] == ';') j++;
						i = j;
						continue;
					}
					sb.Append('&');
					i++;
					continue;
				}
				int ns = j;
				while (j < s.Length && j - ns < 32 && char.IsLetterOrDigit(s[j])) j++;
				string name = s.Substring(ns, j - ns);
				bool semi = j < s.Length && s[j] == ';';
				string val = null;
				int take = name.Length;
				if (!Entities.TryGetValue(name, out val) && !semi) {
					for (int k = name.Length - 1; k >= 2; k--) {
						if (Entities.TryGetValue(name.Substring(0, k), out val) && LegacyNoSemi.Contains(name.Substring(0, k))) { take = k; break; }
						val = null;
					}
				}
				if (val != null && (semi || !attr || take == name.Length && !(j < s.Length && (char.IsLetterOrDigit(s[j]) || s[j] == '=')))) {
					sb.Append(val);
					i = ns + take;
					if (semi && take == name.Length) i++;
					continue;
				}
				sb.Append('&');
				i++;
			}
			return sb.ToString();
		}

		static long Win1252(long v) {
			int[] map = { 0x20AC, 0x81, 0x201A, 0x192, 0x201E, 0x2026, 0x2020, 0x2021, 0x2C6, 0x2030, 0x160, 0x2039, 0x152, 0x8D, 0x17D, 0x8F, 0x90, 0x2018, 0x2019, 0x201C, 0x201D, 0x2022, 0x2013, 0x2014, 0x2DC, 0x2122, 0x161, 0x203A, 0x153, 0x9D, 0x17E, 0x178 };
			return map[v - 0x80];
		}

		static readonly HashSet<string> LegacyNoSemi = new() { "amp", "lt", "gt", "quot", "nbsp", "copy", "reg", "AMP", "LT", "GT", "QUOT", "COPY", "REG", "shy", "deg", "plusmn", "times", "divide", "laquo", "raquo", "middot", "para", "sect", "cent", "pound", "yen", "euro", "not", "micro", "acute", "cedil", "uml", "ordf", "ordm", "sup1", "sup2", "sup3", "frac12", "frac14", "frac34", "iexcl", "iquest", "macr", "brvbar", "curren" };

		internal static readonly Dictionary<string, string> Entities = BuildEntities();

		static Dictionary<string, string> BuildEntities() {
			var d = new Dictionary<string, string>(StringComparer.Ordinal);
			string data = "amp:26 AMP:26 lt:3C LT:3C gt:3E GT:3E quot:22 QUOT:22 apos:27 nbsp:A0 iexcl:A1 cent:A2 pound:A3 curren:A4 yen:A5 brvbar:A6 sect:A7 uml:A8 copy:A9 COPY:A9 ordf:AA laquo:AB not:AC shy:AD reg:AE REG:AE macr:AF deg:B0 plusmn:B1 sup2:B2 sup3:B3 acute:B4 micro:B5 para:B6 middot:B7 cedil:B8 sup1:B9 ordm:BA raquo:BB frac14:BC frac12:BD frac34:BE iquest:BF " +
				"Agrave:C0 Aacute:C1 Acirc:C2 Atilde:C3 Auml:C4 Aring:C5 AElig:C6 Ccedil:C7 Egrave:C8 Eacute:C9 Ecirc:CA Euml:CB Igrave:CC Iacute:CD Icirc:CE Iuml:CF ETH:D0 Ntilde:D1 Ograve:D2 Oacute:D3 Ocirc:D4 Otilde:D5 Ouml:D6 times:D7 Oslash:D8 Ugrave:D9 Uacute:DA Ucirc:DB Uuml:DC Yacute:DD THORN:DE szlig:DF " +
				"agrave:E0 aacute:E1 acirc:E2 atilde:E3 auml:E4 aring:E5 aelig:E6 ccedil:E7 egrave:E8 eacute:E9 ecirc:EA euml:EB igrave:EC iacute:ED icirc:EE iuml:EF eth:F0 ntilde:F1 ograve:F2 oacute:F3 ocirc:F4 otilde:F5 ouml:F6 divide:F7 oslash:F8 ugrave:F9 uacute:FA ucirc:FB uuml:FC yacute:FD thorn:FE yuml:FF " +
				"OElig:152 oelig:153 Scaron:160 scaron:161 Yuml:178 fnof:192 circ:2C6 tilde:2DC Alpha:391 Beta:392 Gamma:393 Delta:394 Epsilon:395 Zeta:396 Eta:397 Theta:398 Iota:399 Kappa:39A Lambda:39B Mu:39C Nu:39D Xi:39E Omicron:39F Pi:3A0 Rho:3A1 Sigma:3A3 Tau:3A4 Upsilon:3A5 Phi:3A6 Chi:3A7 Psi:3A8 Omega:3A9 " +
				"alpha:3B1 beta:3B2 gamma:3B3 delta:3B4 epsilon:3B5 zeta:3B6 eta:3B7 theta:3B8 iota:3B9 kappa:3BA lambda:3BB mu:3BC nu:3BD xi:3BE omicron:3BF pi:3C0 rho:3C1 sigmaf:3C2 sigma:3C3 tau:3C4 upsilon:3C5 phi:3C6 chi:3C7 psi:3C8 omega:3C9 thetasym:3D1 upsih:3D2 piv:3D6 " +
				"ensp:2002 emsp:2003 thinsp:2009 hairsp:200A zwnj:200C zwj:200D lrm:200E rlm:200F ndash:2013 mdash:2014 horbar:2015 lsquo:2018 rsquo:2019 sbquo:201A ldquo:201C rdquo:201D bdquo:201E dagger:2020 Dagger:2021 bull:2022 bullet:2022 hellip:2026 mldr:2026 permil:2030 prime:2032 Prime:2033 lsaquo:2039 rsaquo:203A oline:203E frasl:2044 " +
				"euro:20AC image:2111 weierp:2118 real:211C trade:2122 TRADE:2122 alefsym:2135 larr:2190 uarr:2191 rarr:2192 darr:2193 harr:2194 crarr:21B5 lArr:21D0 uArr:21D1 rArr:21D2 dArr:21D3 hArr:21D4 forall:2200 part:2202 exist:2203 empty:2205 nabla:2207 isin:2208 notin:2209 ni:220B prod:220F sum:2211 minus:2212 lowast:2217 radic:221A prop:221D infin:221E " +
				"ang:2220 and:2227 or:2228 cap:2229 cup:222A int:222B there4:2234 sim:223C cong:2245 asymp:2248 ne:2260 equiv:2261 le:2264 ge:2265 sub:2282 sup:2283 nsub:2284 sube:2286 supe:2287 oplus:2295 otimes:2297 perp:22A5 sdot:22C5 lceil:2308 rceil:2309 lfloor:230A rfloor:230B lang:27E8 rang:27E9 loz:25CA spades:2660 clubs:2663 hearts:2665 diams:2666 " +
				"check:2713 cross:2717 star:2606 starf:2605 phone:260E female:2640 male:2642 sharp:266F flat:266D natural:266E laquo:AB Tab:9 NewLine:A excl:21 num:23 dollar:24 percnt:25 lpar:28 rpar:29 ast:2A plus:2B comma:2C period:2E sol:2F colon:3A semi:3B equals:3D quest:3F commat:40 lsqb:5B lbrack:5B bsol:5C rsqb:5D rbrack:5D Hat:5E lowbar:5F grave:60 lcub:7B lbrace:7B verbar:7C vert:7C rcub:7D rbrace:7D " +
				"half:BD centerdot:B7 middot:B7 dash:2010 hyphen:2010 nbhy:2011 ZeroWidthSpace:200B NoBreak:2060 numsp:2007 puncsp:2008 MediumSpace:205F emsp13:2004 emsp14:2005 caret:2041 bsemi:204F copysr:2117 incare:2105 ohm:3A9 mho:2127 angst:C5 larrb:21E4 rarrb:21E5 uparrow:2191 downarrow:2193 leftarrow:2190 rightarrow:2192 " +
				"Iuml:CF ldquor:201E rdquor:201D lsquor:201A rsquor:2019 squ:25A1 square:25A1 squf:25AA blacksquare:25AA rect:25AD marker:25AE utri:25B5 utrif:25B4 dtri:25BF dtrif:25BE ltri:25C3 rtri:25B9 cir:25CB bigcirc:25EF xcirc:25EF";
			foreach (var part in data.Split(' ', StringSplitOptions.RemoveEmptyEntries)) {
				int c = part.LastIndexOf(':');
				d[part.Substring(0, c)] = char.ConvertFromUtf32(int.Parse(part.Substring(c + 1), NumberStyles.HexNumber));
			}
			return d;
		}
	}

	internal struct Rgba : IEquatable<Rgba> {
		public byte R, G, B, A;
		public bool Current;
		public Rgba(int r, int g, int b, int a = 255) { R = (byte)Math.Clamp(r, 0, 255); G = (byte)Math.Clamp(g, 0, 255); B = (byte)Math.Clamp(b, 0, 255); A = (byte)Math.Clamp(a, 0, 255); Current = false; }
		public static readonly Rgba Black = new(0, 0, 0);
		public static readonly Rgba White = new(255, 255, 255);
		public static readonly Rgba Transparent = new(0, 0, 0, 0);
		public static Rgba CurrentColor => new Rgba { Current = true, A = 255 };
		public bool IsTransparent => A == 0 && !Current;
		public double Alpha => A / 255.0;
		public Rgba WithAlpha(double a) => new(R, G, B, (int)Math.Round(a * 255));
		public bool Equals(Rgba o) => R == o.R && G == o.G && B == o.B && A == o.A && Current == o.Current;
		public override bool Equals(object o) => o is Rgba r && Equals(r);
		public override int GetHashCode() => R | G << 8 | B << 16 | A << 24;
		public Rgba Dark() {
			if (R == 255 && G == 255 && B == 255) return new Rgba(0xAB, 0xAB, 0xAB, A);
			double r = R / 255.0, g = G / 255.0, b = B / 255.0;
			double v = Math.Max(r, Math.Max(g, b));
			double mult = v == 0 ? 0 : Math.Max(0, (v - 0.33) / v);
			const double sf = 255.99998;
			return new Rgba((int)(r * mult * sf), (int)(g * mult * sf), (int)(b * mult * sf), A);
		}
		public Rgba Light() {
			if (R == 0 && G == 0 && B == 0) return new Rgba(0x54, 0x54, 0x54, A);
			double r = R / 255.0, g = G / 255.0, b = B / 255.0;
			double v = Math.Max(r, Math.Max(g, b));
			double mult = Math.Min(1.0, v + 0.33) / v;
			const double sf = 255.99998;
			return new Rgba((int)(mult * r * sf), (int)(mult * g * sf), (int)(mult * b * sf), A);
		}
		static int Diff2(Rgba a, int r, int g, int b) => (a.R - r) * (a.R - r) + (a.G - g) * (a.G - g) + (a.B - b) * (a.B - b);
		public Rgba BorderStyleColor(bool darken) {
			if (darken) return Diff2(this, 0, 0, 0) > Diff2(this, 0x20, 0x20, 0x20) ? Dark() : this;
			return Diff2(this, 255, 255, 255) > Diff2(this, 0xEB, 0xEB, 0xEB) ? Light() : this;
		}
	}

	internal struct Len {
		public byte K;
		public double Px, Pct;
		public bool HasPct;
		public Func<double, double> Fn;
		public const byte Value = 0, KAuto = 1, KNone = 2, KNormal = 3, KMin = 4, KMax = 5, KFit = 6, KStretch = 7;
		public static Len Auto => new() { K = KAuto };
		public static Len None => new() { K = KNone };
		public static Len Normal => new() { K = KNormal };
		public static Len Zero => new() { K = Value };
		public static Len PxV(double v) => new() { K = Value, Px = v };
		public static Len PctV(double v) => new() { K = Value, Pct = v, HasPct = true };
		public bool IsAuto => K == KAuto;
		public bool IsNone => K == KNone;
		public bool IsValue => K == Value;
		public bool IsPctDep => K == Value && (HasPct || Fn != null);
		public bool IsFixed => K == Value && !HasPct && Fn == null;
		public double Resolve(double basis) => K != Value ? 0 : Fn != null ? Fn(double.IsNaN(basis) ? 0 : basis) : Px + (HasPct && !double.IsNaN(basis) ? Pct * basis / 100 : 0);
		public double? TryResolve(double basis) {
			if (K != Value) return null;
			if ((HasPct || Fn != null) && (double.IsNaN(basis) || double.IsInfinity(basis))) return null;
			return Resolve(basis);
		}
	}

	internal enum Disp : byte { None, Inline, Block, InlineBlock, ListItem, Table, InlineTable, RowGroup, HeaderGroup, FooterGroup, Row, Cell, Column, ColumnGroup, Caption, Flex, InlineFlex, Grid, InlineGrid, Contents, FlowRoot }
	internal enum Pos : byte { Static, Relative, Absolute, Fixed, Sticky }
	internal enum TA : byte { Start, End, Left, Right, Center, Justify, WebkitCenter, WebkitLeft, WebkitRight, MatchParent, Auto }
	internal enum WSp : byte { Normal, NoWrap, Pre, PreWrap, PreLine, BreakSpaces }
	internal enum BS : byte { None, Hidden, Dotted, Dashed, Solid, Double, Groove, Ridge, Inset, Outset }

	internal sealed class Shadow {
		public double X, Y, Blur, Spread;
		public Rgba Color;
		public bool Inset;
	}

	internal sealed class GradientStop { public Rgba Color; public Len? Pos; public bool Hint; }

	internal sealed class Gradient {
		public byte Type;
		public bool Repeating;
		public double Angle = 180;
		public bool ToCorner;
		public int CornerX, CornerY;
		public bool Circle;
		public byte SizeKw = 3;
		public Len RX, RY;
		public bool ExplicitSize;
		public Len PX = Len.PctV(50), PY = Len.PctV(50);
		public double FromAngle;
		public List<GradientStop> Stops = new();
	}

	internal sealed class BgLayer {
		public string Url;
		public Gradient Grad;
		public byte RepX, RepY;
		public Len PosX = Len.PctV(0), PosY = Len.PctV(0);
		public byte SizeKind;
		public Len SizeW = Len.Auto, SizeH = Len.Auto;
		public byte Origin, Clip;
		public BgLayer Clone() => (BgLayer)MemberwiseClone();
	}

	internal sealed class Style {
		public Rgba Color = Rgba.Black;
		public string[] FontFamily = { "serif" };
		public double FontSize = 16;
		public bool FontSizeKw = true;
		public int FontWeight = 400;
		public byte FontStyle;
		public int FontStretch = 100;
		public double LineHeightNum = double.NaN, LineHeightPx = double.NaN;
		public TA TextAlign = TA.Start, TextAlignLast = TA.Auto;
		public Len TextIndent = Len.Zero;
		public bool Rtl;
		public WSp Ws = WSp.Normal;
		public double WordSpacing, LetterSpacing;
		public byte TextTransform;
		public byte Visibility;
		public string ListStyleType = "disc";
		public bool ListInside;
		public string ListStyleImage;
		public byte WordBreak, OverflowWrap, LineBreakMode;
		public bool BorderCollapse;
		public double BorderSpacingH, BorderSpacingV;
		public bool CaptionBottom, EmptyCellsHide;
		public List<Shadow> TextShadow;
		public string Lang;
		public bool PrintExact;
		public int Orphans = 2, Widows = 2;
		public Dictionary<string, string> Vars;
		public double TabSize = 8;
		public bool KerningOff, LigaturesOff;
		public string FontFeatures;
		public byte Hyphens = 1;
		public string Quotes;
		public bool TabularNums;
		public byte FontVariantCaps;
		public bool PointerEventsNone;

		public Disp Display = Disp.Inline;
		public Pos Position = Pos.Static;
		public byte Float, Clear;
		public Len Top = Len.Auto, Right = Len.Auto, Bottom = Len.Auto, Left = Len.Auto;
		public Len Width = Len.Auto, Height = Len.Auto, MinWidth = Len.Auto, MinHeight = Len.Auto, MaxWidth = Len.None, MaxHeight = Len.None;
		public Len[] Margin = { Len.Zero, Len.Zero, Len.Zero, Len.Zero };
		public Len[] Padding = { Len.Zero, Len.Zero, Len.Zero, Len.Zero };
		public double[] BorderWidth = new double[4];
		public BS[] BorderStyle = new BS[4];
		public Rgba[] BorderColor = { Rgba.CurrentColor, Rgba.CurrentColor, Rgba.CurrentColor, Rgba.CurrentColor };
		public Len[] RadiusH = { Len.Zero, Len.Zero, Len.Zero, Len.Zero };
		public Len[] RadiusV = { Len.Zero, Len.Zero, Len.Zero, Len.Zero };
		public Rgba BackgroundColor = Rgba.Transparent;
		public List<BgLayer> Backgrounds;
		public bool BorderBox;
		public byte OverflowX, OverflowY;
		public int? ZIndex;
		public double Opacity = 1;
		public byte VAlign;
		public Len VAlignLen;
		public byte TextDecoLine;
		public Rgba TextDecoColor = Rgba.CurrentColor;
		public byte TextDecoStyle;
		public Len TextDecoThickness = Len.Auto;
		public Len UnderlineOffset = Len.Auto;
		public string Content;
		public string CounterReset, CounterIncrement, CounterSet;
		public byte FlexDirection, FlexWrap;
		public double FlexGrow, FlexShrink = 1;
		public Len FlexBasis = Len.Auto;
		public byte JustifyContent, AlignItems, AlignContent, JustifyItems;
		public byte AlignSelf = AlAuto, JustifySelf = AlAuto;
		public int Order;
		public Len RowGap = Len.Normal, ColumnGap = Len.Normal;
		public string GridTemplateColumns, GridTemplateRows, GridTemplateAreas, GridAutoColumns, GridAutoRows;
		public byte GridAutoFlow;
		public string GridColumnStart, GridColumnEnd, GridRowStart, GridRowEnd;
		public string Transform;
		public Len TransformOriginX = Len.PctV(50), TransformOriginY = Len.PctV(50);
		public List<Shadow> BoxShadow;
		public byte BreakBefore, BreakAfter, BreakInside;
		public bool TableFixed;
		public byte UnicodeBidi;
		public byte ObjectFit;
		public Len ObjectPosX = Len.PctV(50), ObjectPosY = Len.PctV(50);
		public double OutlineWidth = 3;
		public BS OutlineStyle;
		public Rgba OutlineColor = Rgba.CurrentColor;
		public double OutlineOffset;
		public double AspectRatio;
		public byte BoxDecorationBreak;
		public string ClipPath;
		public int ColumnCount;
		public string Page;
		public string FKey;

		public const byte AlNormal = 0, AlStretch = 1, AlStart = 2, AlEnd = 3, AlCenter = 4, AlBaseline = 5, AlFlexStart = 6, AlFlexEnd = 7, AlSpaceBetween = 8, AlSpaceAround = 9, AlSpaceEvenly = 10, AlLeft = 11, AlRight = 12, AlSelfStart = 13, AlSelfEnd = 14, AlAuto = 15, AlLastBaseline = 16;
		public const byte VaBaseline = 0, VaSub = 1, VaSuper = 2, VaTop = 3, VaTextTop = 4, VaMiddle = 5, VaBottom = 6, VaTextBottom = 7, VaLength = 8;
		public const byte BrAuto = 0, BrAvoid = 1, BrPage = 2, BrAvoidPage = 3, BrColumn = 4;

		public bool IsBlockLevel => Display == Disp.Block || Display == Disp.ListItem || Display == Disp.Table || Display == Disp.Flex || Display == Disp.Grid || Display == Disp.FlowRoot;
		public bool IsInlineLevel => Display == Disp.Inline || Display == Disp.InlineBlock || Display == Disp.InlineTable || Display == Disp.InlineFlex || Display == Disp.InlineGrid;
		public bool IsPositioned => Position != Pos.Static;
		public bool IsOutOfFlow => Position == Pos.Absolute || Position == Pos.Fixed;
		public bool IsFloat => Float != 0 && !IsOutOfFlow;
		public bool IsMonospace => FontFamily.Length == 1 && FontFamily[0] == "monospace";

		public double LineHeightFor(double fontNormal) {
			if (!double.IsNaN(LineHeightPx)) return LineHeightPx;
			if (!double.IsNaN(LineHeightNum)) return LineHeightNum * FontSize;
			return fontNormal;
		}

		public Style Clone() {
			var s = (Style)MemberwiseClone();
			s.Margin = (Len[])Margin.Clone();
			s.Padding = (Len[])Padding.Clone();
			s.BorderWidth = (double[])BorderWidth.Clone();
			s.BorderStyle = (BS[])BorderStyle.Clone();
			s.BorderColor = (Rgba[])BorderColor.Clone();
			s.RadiusH = (Len[])RadiusH.Clone();
			s.RadiusV = (Len[])RadiusV.Clone();
			return s;
		}

		public Style InheritFrom() {
			var s = new Style {
				Color = Color, FontFamily = FontFamily, FontSize = FontSize, FontSizeKw = FontSizeKw, FontWeight = FontWeight, FontStyle = FontStyle, FontStretch = FontStretch,
				LineHeightNum = LineHeightNum, LineHeightPx = LineHeightPx, TextAlign = TextAlign, TextAlignLast = TextAlignLast, TextIndent = TextIndent, Rtl = Rtl, Ws = Ws,
				WordSpacing = WordSpacing, LetterSpacing = LetterSpacing, TextTransform = TextTransform, Visibility = Visibility, ListStyleType = ListStyleType, ListInside = ListInside,
				ListStyleImage = ListStyleImage, WordBreak = WordBreak, OverflowWrap = OverflowWrap, LineBreakMode = LineBreakMode, BorderCollapse = BorderCollapse,
				BorderSpacingH = BorderSpacingH, BorderSpacingV = BorderSpacingV, CaptionBottom = CaptionBottom, EmptyCellsHide = EmptyCellsHide, TextShadow = TextShadow,
				Lang = Lang, PrintExact = PrintExact, Orphans = Orphans, Widows = Widows, Vars = Vars, TabSize = TabSize, KerningOff = KerningOff, LigaturesOff = LigaturesOff,
				FontFeatures = FontFeatures, Hyphens = Hyphens, Quotes = Quotes, TabularNums = TabularNums, FontVariantCaps = FontVariantCaps, PointerEventsNone = PointerEventsNone,
			};
			return s;
		}
	}

	internal sealed class Selector {
		public List<Compound> Parts = new();
		public List<char> Combs = new();
		public int Spec;
		public string PseudoElement;
	}

	internal sealed class Compound {
		public string Tag;
		public string Id;
		public List<string> Classes;
		public List<AttrSel> Attrs;
		public List<PseudoSel> Pseudos;
	}

	internal sealed class AttrSel { public string Name, Op, Value; public bool CI; }

	internal sealed class PseudoSel {
		public string Name;
		public string Arg;
		public List<Selector> Sub;
		public int A, B;
		public List<Selector> Of;
	}

	internal sealed class CssDecl {
		public string Prop, Value;
		public bool Important;
	}

	internal sealed class CssRule {
		public Selector Sel;
		public List<CssDecl> Decls;
		public int Order;
		public int Origin;
	}

	internal sealed class FontFaceRule {
		public string Family;
		public List<string> Src = new();
		public int WeightMin = 400, WeightMax = 400;
		public byte Style;
		public string UnicodeRange;
		public string BaseDir;
	}

	internal sealed class PageRule {
		public string Selector;
		public List<CssDecl> Decls = new();
		public Dictionary<string, List<CssDecl>> MarginBoxes = new();
	}

	internal sealed class StyleSheet {
		public List<CssRule> Rules = new();
		public List<FontFaceRule> FontFaces = new();
		public List<PageRule> Pages = new();
	}

	internal sealed class MediaEnv {
		public string Type = "print";
		public double Width = 794, Height = 1123;
	}

	internal static class Css {
		public static string StripComments(string s) {
			if (s.IndexOf("/*", StringComparison.Ordinal) < 0) return s;
			var sb = new StringBuilder(s.Length);
			int i = 0;
			while (i < s.Length) {
				char c = s[i];
				if (c == '"' || c == '\'') {
					int j = i + 1;
					while (j < s.Length && s[j] != c) { if (s[j] == '\\') j++; j++; }
					j = Math.Min(j + 1, s.Length);
					sb.Append(s, i, j - i);
					i = j;
					continue;
				}
				if (c == '/' && i + 1 < s.Length && s[i + 1] == '*') {
					int e = s.IndexOf("*/", i + 2, StringComparison.Ordinal);
					i = e < 0 ? s.Length : e + 2;
					sb.Append(' ');
					continue;
				}
				sb.Append(c);
				i++;
			}
			return sb.ToString();
		}

		static int SkipString(string s, int i) {
			char q = s[i];
			i++;
			while (i < s.Length && s[i] != q) { if (s[i] == '\\') i++; i++; }
			return Math.Min(i + 1, s.Length);
		}

		static int FindBlockEnd(string s, int i) {
			int depth = 1;
			while (i < s.Length) {
				char c = s[i];
				if (c == '"' || c == '\'') { i = SkipString(s, i); continue; }
				if (c == '{') depth++;
				else if (c == '}') { depth--; if (depth == 0) return i; }
				i++;
			}
			return s.Length;
		}

		public static void ParseSheet(string css, StyleSheet sheet, int origin, MediaEnv env, ref int order, Func<string, string> loadImport, string baseDir, int depth = 0) {
			css = StripComments(css);
			ParseBlockContents(css, sheet, origin, env, ref order, loadImport, baseDir, depth, null);
		}

		static void ParseBlockContents(string css, StyleSheet sheet, int origin, MediaEnv env, ref int order, Func<string, string> loadImport, string baseDir, int depth, string parentSel) {
			int i = 0;
			int n = css.Length;
			while (i < n) {
				while (i < n && (char.IsWhiteSpace(css[i]) || css[i] == ';')) i++;
				if (i >= n) break;
				if (css.Substring(i).StartsWith("<!--") ) { i += 4; continue; }
				if (css.Substring(i).StartsWith("-->")) { i += 3; continue; }
				if (css[i] == '}') { i++; continue; }
				int ps = i;
				while (i < n && css[i] != '{' && css[i] != ';' && css[i] != '}') {
					if (css[i] == '"' || css[i] == '\'') { i = SkipString(css, i); continue; }
					if (css[i] == '(') { int d = 1; i++; while (i < n && d > 0) { if (css[i] == '(') d++; else if (css[i] == ')') d--; else if (css[i] == '"' || css[i] == '\'') { i = SkipString(css, i); continue; } i++; } continue; }
					i++;
				}
				string prelude = css.Substring(ps, i - ps).Trim();
				if (i >= n || css[i] == ';' || css[i] == '}') {
					if (prelude.StartsWith("@import", StringComparison.OrdinalIgnoreCase) && loadImport != null && depth < 8) {
						string rest = prelude.Substring(7).Trim();
						string url = null;
						string media = "";
						if (rest.StartsWith("url(", StringComparison.OrdinalIgnoreCase)) {
							int e = rest.IndexOf(')');
							url = Unquote(rest.Substring(4, e - 4).Trim());
							media = rest.Substring(e + 1).Trim();
						}
						else if (rest.Length > 0 && (rest[0] == '"' || rest[0] == '\'')) {
							int e = SkipString(rest, 0);
							url = Unquote(rest.Substring(0, e));
							media = rest.Substring(e).Trim();
						}
						if (url != null && (media.Length == 0 || MediaMatches(media, env))) {
							string content = loadImport(url);
							if (content != null) ParseSheet(content, sheet, origin, env, ref order, loadImport, baseDir, depth + 1);
						}
					}
					if (i < n) i++;
					continue;
				}
				int bs = i + 1;
				int be = FindBlockEnd(css, bs);
				string block = css.Substring(bs, be - bs);
				i = be + 1;
				if (prelude.StartsWith("@")) {
					int sp = 1;
					while (sp < prelude.Length && (char.IsLetterOrDigit(prelude[sp]) || prelude[sp] == '-')) sp++;
					string at = prelude.Substring(1, sp - 1).ToLowerInvariant();
					string cond = prelude.Substring(sp).Trim();
					switch (at) {
						case "media":
							if (MediaMatches(cond, env)) ParseBlockContents(block, sheet, origin, env, ref order, loadImport, baseDir, depth, parentSel);
							break;
						case "supports":
							if (SupportsMatches(cond)) ParseBlockContents(block, sheet, origin, env, ref order, loadImport, baseDir, depth, parentSel);
							break;
						case "layer":
						case "container":
						case "scope":
						case "document":
						case "-moz-document":
							ParseBlockContents(block, sheet, origin, env, ref order, loadImport, baseDir, depth, parentSel);
							break;
						case "font-face": {
							var ff = new FontFaceRule { BaseDir = baseDir };
							foreach (var d in ParseDeclarations(block)) {
								switch (d.Prop) {
									case "font-family": ff.Family = Unquote(d.Value.Trim()); break;
									case "src": ff.Src = SplitTopLevel(d.Value, ',').Select(x => x.Trim()).ToList(); break;
									case "font-weight": {
										var parts = d.Value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
										ff.WeightMin = ParseWeightKw(parts[0], 400);
										ff.WeightMax = parts.Length > 1 ? ParseWeightKw(parts[1], ff.WeightMin) : ff.WeightMin;
										break;
									}
									case "font-style": ff.Style = (byte)(d.Value.Trim().StartsWith("italic") ? 1 : d.Value.Trim().StartsWith("oblique") ? 2 : 0); break;
									case "unicode-range": ff.UnicodeRange = d.Value; break;
								}
							}
							if (ff.Family != null) sheet.FontFaces.Add(ff);
							break;
						}
						case "page": {
							var pr = new PageRule { Selector = cond };
							ParsePageBlock(block, pr);
							sheet.Pages.Add(pr);
							break;
						}
						default:
							break;
					}
					continue;
				}
				string selText = prelude;
				if (parentSel != null) selText = ResolveNested(selText, parentSel);
				var nestedIdx = FindNestedRules(block);
				string declPart = nestedIdx.decls;
				var decls = ParseDeclarations(declPart);
				var sels = ParseSelectorList(selText);
				if (sels != null) {
					foreach (var sel in sels) {
						sheet.Rules.Add(new CssRule { Sel = sel, Decls = decls, Order = order, Origin = origin });
					}
					order++;
				}
				if (nestedIdx.nested.Length > 0) ParseBlockContents(nestedIdx.nested, sheet, origin, env, ref order, loadImport, baseDir, depth, selText);
			}
		}

		static string ResolveNested(string sel, string parent) {
			var parts = SplitTopLevel(sel, ',');
			var parents = SplitTopLevel(parent, ',');
			var outp = new List<string>();
			foreach (var p0 in parts) {
				string p = p0.Trim();
				foreach (var par0 in parents) {
					string par = par0.Trim();
					string wrapped = parents.Count > 1 ? ":is(" + parent + ")" : par;
					if (p.Contains('&')) outp.Add(p.Replace("&", wrapped));
					else outp.Add(wrapped + " " + p);
					if (parents.Count > 1) break;
				}
			}
			return string.Join(", ", outp);
		}

		static (string decls, string nested) FindNestedRules(string block) {
			if (block.IndexOf('{') < 0) return (block, "");
			var decls = new StringBuilder();
			var nested = new StringBuilder();
			int i = 0, n = block.Length;
			int segStart = 0;
			while (i < n) {
				char c = block[i];
				if (c == '"' || c == '\'') { i = SkipString(block, i); continue; }
				if (c == '(') { int d = 1; i++; while (i < n && d > 0) { if (block[i] == '(') d++; else if (block[i] == ')') d--; i++; } continue; }
				if (c == ';') { decls.Append(block, segStart, i - segStart + 1); i++; segStart = i; continue; }
				if (c == '{') {
					int e = FindBlockEnd(block, i + 1);
					nested.Append(block, segStart, Math.Min(n, e + 1) - segStart).Append('\n');
					i = e + 1;
					segStart = i;
					continue;
				}
				i++;
			}
			if (segStart < n) decls.Append(block, segStart, n - segStart);
			return (decls.ToString(), nested.ToString());
		}

		static void ParsePageBlock(string block, PageRule pr) {
			int i = 0, n = block.Length;
			var direct = new StringBuilder();
			while (i < n) {
				int at = block.IndexOf('@', i);
				if (at < 0) { direct.Append(block, i, n - i); break; }
				direct.Append(block, i, at - i);
				int ob = block.IndexOf('{', at);
				if (ob < 0) { direct.Append(block, at, n - at); break; }
				string name = block.Substring(at + 1, ob - at - 1).Trim().ToLowerInvariant();
				int e = FindBlockEnd(block, ob + 1);
				pr.MarginBoxes[name] = ParseDeclarations(block.Substring(ob + 1, e - ob - 1));
				i = e + 1;
			}
			pr.Decls = ParseDeclarations(direct.ToString());
		}

		public static int ParseWeightKw(string v, int def) {
			v = v.Trim().ToLowerInvariant();
			if (v == "normal") return 400;
			if (v == "bold") return 700;
			if (double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out double d)) return (int)Math.Clamp(d, 1, 1000);
			return def;
		}

		public static List<CssDecl> ParseDeclarations(string block) {
			var list = new List<CssDecl>();
			foreach (var part in SplitTopLevel(block, ';')) {
				int c = part.IndexOf(':');
				if (c <= 0) continue;
				string prop = part.Substring(0, c).Trim();
				if (!prop.StartsWith("--")) prop = prop.ToLowerInvariant();
				string val = part.Substring(c + 1).Trim();
				bool imp = false;
				int bang = val.LastIndexOf('!');
				if (bang >= 0 && val.Substring(bang + 1).Trim().Equals("important", StringComparison.OrdinalIgnoreCase)) { imp = true; val = val.Substring(0, bang).Trim(); }
				if (prop.Length == 0) continue;
				list.Add(new CssDecl { Prop = prop, Value = val, Important = imp });
			}
			return list;
		}

		public static List<string> SplitTopLevel(string s, char sep) {
			var res = new List<string>();
			int depth = 0, start = 0;
			for (int i = 0; i < s.Length; i++) {
				char c = s[i];
				if (c == '"' || c == '\'') { i = SkipString(s, i) - 1; continue; }
				if (c == '(' || c == '[' || c == '{') depth++;
				else if (c == ')' || c == ']' || c == '}') depth = Math.Max(0, depth - 1);
				else if (c == sep && depth == 0) { res.Add(s.Substring(start, i - start)); start = i + 1; }
			}
			res.Add(s.Substring(start));
			return res;
		}

		public static List<string> SplitWs(string s) {
			var res = new List<string>();
			int depth = 0, start = -1;
			for (int i = 0; i < s.Length; i++) {
				char c = s[i];
				if (c == '"' || c == '\'') { if (start < 0) start = i; i = SkipString(s, i) - 1; continue; }
				if (c == '(') { if (start < 0) start = i; depth++; continue; }
				if (c == ')') { depth = Math.Max(0, depth - 1); continue; }
				if (depth == 0 && (char.IsWhiteSpace(c) || c == '/' || c == ',')) {
					if (start >= 0) { res.Add(s.Substring(start, i - start)); start = -1; }
					if (c == '/' || c == ',') res.Add(c.ToString());
					continue;
				}
				if (start < 0) start = i;
			}
			if (start >= 0) res.Add(s.Substring(start));
			return res;
		}

		public static string Unquote(string s) {
			s = s.Trim();
			if (s.Length >= 2 && (s[0] == '"' || s[0] == '\'') && s[s.Length - 1] == s[0]) s = s.Substring(1, s.Length - 2);
			else if (s.Length >= 1 && (s[0] == '"' || s[0] == '\'')) s = s.Substring(1);
			if (s.IndexOf('\\') >= 0) s = Unescape(s);
			return s;
		}

		public static string Unescape(string s) {
			var sb = new StringBuilder();
			for (int i = 0; i < s.Length; i++) {
				if (s[i] == '\\' && i + 1 < s.Length) {
					int j = i + 1;
					int st = j;
					while (j < s.Length && j - st < 6 && Uri.IsHexDigit(s[j])) j++;
					if (j > st) {
						int cp = int.Parse(s.Substring(st, j - st), NumberStyles.HexNumber);
						if (cp == 0 || cp > 0x10FFFF || cp >= 0xD800 && cp <= 0xDFFF) cp = 0xFFFD;
						sb.Append(char.ConvertFromUtf32(cp));
						if (j < s.Length && char.IsWhiteSpace(s[j])) j++;
						i = j - 1;
					}
					else if (s[j] == '\n') i = j;
					else { sb.Append(s[j]); i = j; }
				}
				else sb.Append(s[i]);
			}
			return sb.ToString();
		}

		public static string ExtractUrl(string v) {
			v = v.Trim();
			if (v.StartsWith("url(", StringComparison.OrdinalIgnoreCase)) {
				int e = v.LastIndexOf(')');
				if (e < 0) e = v.Length;
				return Unquote(v.Substring(4, e - 4).Trim());
			}
			if (v.StartsWith("\"") || v.StartsWith("'")) return Unquote(v);
			return null;
		}

		static bool SupportsMatches(string cond) {
			cond = cond.Trim();
			if (cond.StartsWith("not", StringComparison.OrdinalIgnoreCase)) return !SupportsMatches(cond.Substring(3));
			if (cond.StartsWith("selector(")) return true;
			return !cond.Contains("-webkit-touch") && !cond.Contains("-moz-") && !cond.Contains("-ms-");
		}

		public static bool MediaMatches(string q, MediaEnv env) {
			if (string.IsNullOrWhiteSpace(q)) return true;
			foreach (var part in SplitTopLevel(q, ',')) if (MediaQueryMatches(part.Trim().ToLowerInvariant(), env)) return true;
			return false;
		}

		static bool MediaQueryMatches(string q, MediaEnv env) {
			if (q.Length == 0) return true;
			bool not = false;
			if (q.StartsWith("not ")) { not = true; q = q.Substring(4).Trim(); }
			else if (q.StartsWith("only ")) q = q.Substring(5).Trim();
			bool result = true;
			var tokens = new List<string>();
			int i = 0;
			while (i < q.Length) {
				if (char.IsWhiteSpace(q[i])) { i++; continue; }
				if (q[i] == '(') {
					int d = 1, s = i; i++;
					while (i < q.Length && d > 0) { if (q[i] == '(') d++; else if (q[i] == ')') d--; i++; }
					tokens.Add(q.Substring(s, i - s));
					continue;
				}
				int st = i;
				while (i < q.Length && !char.IsWhiteSpace(q[i]) && q[i] != '(') i++;
				tokens.Add(q.Substring(st, i - st));
			}
			bool orMode = tokens.Contains("or");
			if (orMode) result = false;
			foreach (var t in tokens) {
				if (t == "and" || t == "or") continue;
				bool m;
				if (t.StartsWith("(")) m = FeatureMatches(t.Substring(1, t.Length - 2).Trim(), env);
				else if (t == "all") m = true;
				else if (t == "print") m = env.Type == "print";
				else if (t == "screen") m = env.Type == "screen";
				else if (t == "not") { continue; }
				else m = false;
				if (orMode) result |= m; else result &= m;
			}
			return not ? !result : result;
		}

		static bool FeatureMatches(string f, MediaEnv env) {
			if (f.StartsWith("not ")) return !FeatureMatches(f.Substring(4).Trim().Trim('(', ')'), env);
			if (f.Contains(" and ")) return f.Split(" and ").All(x => FeatureMatches(x.Trim().Trim('(', ')'), env));
			if (f.Contains(" or ")) return f.Split(" or ").Any(x => FeatureMatches(x.Trim().Trim('(', ')'), env));
			if (f.Contains('<') || f.Contains('>') || f.Contains('=') && !f.Contains(':')) return RangeMatches(f, env);
			int c = f.IndexOf(':');
			string name = c < 0 ? f.Trim() : f.Substring(0, c).Trim();
			string val = c < 0 ? null : f.Substring(c + 1).Trim();
			double NumVal() => ParseAbsLength(val, 16);
			switch (name) {
				case "width": return val == null || Math.Abs(env.Width - NumVal()) < 0.5;
				case "min-width": return env.Width >= NumVal();
				case "max-width": return env.Width <= NumVal();
				case "height": return val == null || Math.Abs(env.Height - NumVal()) < 0.5;
				case "min-height": return env.Height >= NumVal();
				case "max-height": return env.Height <= NumVal();
				case "device-width": case "min-device-width": return name == "min-device-width" ? env.Width >= NumVal() : true;
				case "max-device-width": return env.Width <= NumVal();
				case "orientation": return val == (env.Height >= env.Width ? "portrait" : "landscape");
				case "prefers-color-scheme": return val == "light";
				case "prefers-reduced-motion": return val == "no-preference" || val == null;
				case "prefers-contrast": return val == "no-preference";
				case "color": return true;
				case "monochrome": return false;
				case "hover": return val == "none";
				case "any-hover": return val == "none";
				case "pointer": case "any-pointer": return val == "none";
				case "scripting": return val == "none";
				case "min-resolution": return ResolutionDppx(val) <= 1;
				case "max-resolution": return ResolutionDppx(val) >= 1;
				case "-webkit-min-device-pixel-ratio": case "min-device-pixel-ratio": return double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out double r1) && r1 <= 1;
				case "-webkit-max-device-pixel-ratio": case "max-device-pixel-ratio": return double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out double r2) && r2 >= 1;
				case "min-aspect-ratio": return AspectOk(val, env, true);
				case "max-aspect-ratio": return AspectOk(val, env, false);
				default: return false;
			}
		}

		static bool AspectOk(string val, MediaEnv env, bool min) {
			var p = val.Split('/');
			if (p.Length != 2 || !double.TryParse(p[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double a) || !double.TryParse(p[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double b) || b == 0) return false;
			double r = env.Width / env.Height;
			return min ? r >= a / b : r <= a / b;
		}

		static double ResolutionDppx(string v) {
			if (v == null) return 1;
			v = v.Trim();
			if (v.EndsWith("dppx")) return double.Parse(v[..^4], CultureInfo.InvariantCulture);
			if (v.EndsWith("dpi")) return double.Parse(v[..^3], CultureInfo.InvariantCulture) / 96;
			if (v.EndsWith("dpcm")) return double.Parse(v[..^4], CultureInfo.InvariantCulture) * 2.54 / 96;
			if (v.EndsWith("x")) return double.Parse(v[..^1], CultureInfo.InvariantCulture);
			return 1;
		}

		static bool RangeMatches(string f, MediaEnv env) {
			var toks = new List<string>();
			int i = 0;
			while (i < f.Length) {
				if (char.IsWhiteSpace(f[i])) { i++; continue; }
				if (f[i] == '<' || f[i] == '>' || f[i] == '=') {
					int s = i; i++;
					if (i < f.Length && f[i] == '=') i++;
					toks.Add(f.Substring(s, i - s));
					continue;
				}
				int st = i;
				while (i < f.Length && !char.IsWhiteSpace(f[i]) && f[i] != '<' && f[i] != '>' && f[i] != '=') i++;
				toks.Add(f.Substring(st, i - st));
			}
			double Val(string t) {
				if (t == "width") return env.Width;
				if (t == "height") return env.Height;
				return ParseAbsLength(t, 16);
			}
			bool Cmp(double a, string op, double b) => op switch { "<" => a < b, "<=" => a <= b, ">" => a > b, ">=" => a >= b, "=" => Math.Abs(a - b) < 0.5, _ => false };
			for (int k = 0; k + 2 < toks.Count; k += 2) if (!Cmp(Val(toks[k]), toks[k + 1], Val(toks[k + 2]))) return false;
			return true;
		}

		public static double ParseAbsLength(string v, double em) {
			if (v == null) return 0;
			v = v.Trim().ToLowerInvariant();
			int i = 0;
			while (i < v.Length && (char.IsDigit(v[i]) || v[i] == '.' || v[i] == '-' || v[i] == '+' || (v[i] == 'e' && i + 1 < v.Length && (char.IsDigit(v[i + 1]) || v[i + 1] == '-')))) i++;
			if (!double.TryParse(v.AsSpan(0, i), NumberStyles.Float, CultureInfo.InvariantCulture, out double d)) return 0;
			return d * UnitFactor(v.Substring(i), em, em, 1);
		}

		public static double UnitFactor(string u, double em, double rem, double ex) {
			switch (u) {
				case "": case "px": return 1;
				case "pt": return 96.0 / 72;
				case "pc": return 16;
				case "in": return 96;
				case "cm": return 96 / 2.54;
				case "mm": return 96 / 25.4;
				case "q": return 96 / 101.6;
				case "em": return em;
				case "rem": return rem;
				case "ex": return ex;
				case "ch": return em * 0.5;
				case "ic": return em;
				case "lh": return em * 1.2;
				case "rlh": return rem * 1.2;
				case "cap": return em * 0.7;
				default: return double.NaN;
			}
		}

		public static List<Selector> ParseSelectorList(string text) {
			var list = new List<Selector>();
			foreach (var part in SplitTopLevel(text, ',')) {
				string p = part.Trim();
				if (p.Length == 0) continue;
				var sel = ParseSelector(p);
				if (sel != null) list.Add(sel);
			}
			return list.Count == 0 ? null : list;
		}

		static bool IsIdentChar(char c) => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c > 127 || c == '\\';

		static string ReadIdent(string s, ref int i) {
			var sb = new StringBuilder();
			while (i < s.Length && IsIdentChar(s[i])) {
				if (s[i] == '\\' && i + 1 < s.Length) {
					int j = i + 1, hs = j;
					while (j < s.Length && j - hs < 6 && Uri.IsHexDigit(s[j])) j++;
					if (j > hs) { sb.Append(char.ConvertFromUtf32(int.Parse(s.Substring(hs, j - hs), NumberStyles.HexNumber))); if (j < s.Length && s[j] == ' ') j++; i = j; }
					else { sb.Append(s[j]); i = j + 1; }
					continue;
				}
				sb.Append(s[i]);
				i++;
			}
			return sb.ToString();
		}

		public static Selector ParseSelector(string s) {
			var sel = new Selector();
			int i = 0;
			char pendingComb = ' ';
			bool first = true;
			int a = 0, b = 0, c = 0;
			s = s.Trim();
			while (i < s.Length) {
				bool hadWs = false;
				while (i < s.Length && char.IsWhiteSpace(s[i])) { i++; hadWs = true; }
				if (i >= s.Length) break;
				char ch = s[i];
				if (ch == '>' || ch == '+' || ch == '~') {
					pendingComb = ch;
					i++;
					continue;
				}
				if (!first && hadWs && pendingComb == ' ') pendingComb = ' ';
				var comp = new Compound();
				bool any = false;
				while (i < s.Length) {
					ch = s[i];
					if (ch == '*') { i++; any = true; continue; }
					if (ch == '|') { i++; continue; }
					if (IsIdentChar(ch) && !any && comp.Tag == null && comp.Id == null && comp.Classes == null && comp.Attrs == null && comp.Pseudos == null) {
						comp.Tag = ReadIdent(s, ref i).ToLowerInvariant();
						c++;
						any = true;
						continue;
					}
					if (ch == '#') { i++; comp.Id = ReadIdent(s, ref i); a++; any = true; continue; }
					if (ch == '.') { i++; (comp.Classes ??= new()).Add(ReadIdent(s, ref i)); b++; any = true; continue; }
					if (ch == '[') {
						int e = i + 1;
						while (e < s.Length && s[e] != ']') { if (s[e] == '"' || s[e] == '\'') { e = SkipString(s, e); continue; } e++; }
						string inner = s.Substring(i + 1, e - i - 1).Trim();
						i = Math.Min(e + 1, s.Length);
						var at = new AttrSel();
						int k = 0;
						while (k < inner.Length && (IsIdentChar(inner[k]) || inner[k] == '|')) k++;
						at.Name = inner.Substring(0, k).Trim().TrimStart('|').ToLowerInvariant();
						string rest = inner.Substring(k).Trim();
						if (rest.Length > 0) {
							int opEnd = rest.IndexOf('=');
							if (opEnd >= 0) {
								at.Op = rest.Substring(0, opEnd + 1).Trim();
								string v = rest.Substring(opEnd + 1).Trim();
								if (v.EndsWith(" i") || v.EndsWith(" I")) { at.CI = true; v = v.Substring(0, v.Length - 2).Trim(); }
								else if (v.EndsWith(" s")) v = v.Substring(0, v.Length - 2).Trim();
								at.Value = Unquote(v);
							}
						}
						(comp.Attrs ??= new()).Add(at);
						b++;
						any = true;
						continue;
					}
					if (ch == ':') {
						i++;
						bool pe = false;
						if (i < s.Length && s[i] == ':') { pe = true; i++; }
						string name = ReadIdent(s, ref i).ToLowerInvariant();
						string arg = null;
						if (i < s.Length && s[i] == '(') {
							int d = 1, st = i + 1; i++;
							while (i < s.Length && d > 0) { if (s[i] == '(') d++; else if (s[i] == ')') d--; else if (s[i] == '"' || s[i] == '\'') { i = SkipString(s, i); continue; } i++; }
							arg = s.Substring(st, i - st - 1);
						}
						if (pe || name == "before" || name == "after" || name == "first-line" || name == "first-letter" || name == "marker") {
							sel.PseudoElement = name;
							c++;
							continue;
						}
						var p = new PseudoSel { Name = name, Arg = arg };
						switch (name) {
							case "not": case "is": case "matches": case "any": case "-webkit-any": case "where": case "has": {
								p.Sub = new List<Selector>();
								foreach (var sp in SplitTopLevel(arg ?? "", ',')) {
									string t = sp.Trim();
									if (name == "has" && t.Length > 0 && (t[0] == '>' || t[0] == '+' || t[0] == '~')) {
										var sub = ParseSelector(t.Substring(1));
										if (sub != null) { sub.Combs.Insert(0, t[0]); sub.PseudoElement = "has-rel"; p.Sub.Add(sub); }
									}
									else {
										var sub = ParseSelector(t);
										if (sub != null) p.Sub.Add(sub);
									}
								}
								if (name != "where") {
									int best = 0;
									foreach (var sub in p.Sub) best = Math.Max(best, sub.Spec);
									a += best / 1000000; b += best / 1000 % 1000; c += best % 1000;
								}
								break;
							}
							case "nth-child": case "nth-last-child": case "nth-of-type": case "nth-last-of-type": {
								string ar = arg ?? "";
								int ofIdx = ar.IndexOf(" of ", StringComparison.OrdinalIgnoreCase);
								if (ofIdx >= 0) { p.Of = ParseSelectorList(ar.Substring(ofIdx + 4)); ar = ar.Substring(0, ofIdx); }
								ParseNth(ar.Trim().ToLowerInvariant(), out p.A, out p.B);
								b++;
								break;
							}
							default:
								b++;
								break;
						}
						(comp.Pseudos ??= new()).Add(p);
						any = true;
						continue;
					}
					break;
				}
				if (!any) return null;
				if (!first) sel.Combs.Add(pendingComb);
				sel.Parts.Add(comp);
				pendingComb = ' ';
				first = false;
			}
			if (sel.Parts.Count == 0) {
				if (sel.PseudoElement != null) sel.Parts.Add(new Compound());
				else return null;
			}
			sel.Spec = a * 1000000 + b * 1000 + c;
			return sel;
		}

		static void ParseNth(string s, out int a, out int b) {
			a = 0; b = 0;
			if (s == "odd") { a = 2; b = 1; return; }
			if (s == "even") { a = 2; b = 0; return; }
			s = s.Replace(" ", "");
			int n = s.IndexOf('n');
			if (n < 0) { int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out b); return; }
			string ap = s.Substring(0, n);
			a = ap == "" || ap == "+" ? 1 : ap == "-" ? -1 : int.TryParse(ap, NumberStyles.Integer, CultureInfo.InvariantCulture, out int av) ? av : 0;
			string bp = s.Substring(n + 1);
			if (bp.Length > 0) int.TryParse(bp, NumberStyles.Integer, CultureInfo.InvariantCulture, out b);
		}

		public static bool Matches(Selector sel, Element e) => MatchFrom(sel, sel.Parts.Count - 1, e);

		static bool MatchFrom(Selector sel, int idx, Element e) {
			if (!MatchCompound(sel.Parts[idx], e)) return false;
			if (idx == 0) return true;
			char comb = sel.Combs[idx - 1];
			switch (comb) {
				case '>': return e.Parent != null && MatchFrom(sel, idx - 1, e.Parent);
				case '+': { var p = e.PrevElementSibling(); return p != null && MatchFrom(sel, idx - 1, p); }
				case '~': {
					for (var p = e.PrevElementSibling(); p != null; p = p.PrevElementSibling()) if (MatchFrom(sel, idx - 1, p)) return true;
					return false;
				}
				default: {
					for (var p = e.Parent; p != null; p = p.Parent) if (MatchFrom(sel, idx - 1, p)) return true;
					return false;
				}
			}
		}

		static bool MatchCompound(Compound c, Element e) {
			if (c.Tag != null && !string.Equals(c.Tag, e.Tag, StringComparison.OrdinalIgnoreCase)) return false;
			if (c.Id != null && e.Id != c.Id) return false;
			if (c.Classes != null) foreach (var cl in c.Classes) if (Array.IndexOf(e.Classes, cl) < 0) return false;
			if (c.Attrs != null) foreach (var a in c.Attrs) if (!MatchAttr(a, e)) return false;
			if (c.Pseudos != null) foreach (var p in c.Pseudos) if (!MatchPseudo(p, e)) return false;
			return true;
		}

		static bool MatchAttr(AttrSel a, Element e) {
			string v = e.Attr(a.Name);
			if (v == null) return false;
			if (a.Op == null) return true;
			var cmp = a.CI || a.Name == "type" || a.Name == "lang" || a.Name == "dir" ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			string t = a.Value ?? "";
			switch (a.Op) {
				case "=": return string.Equals(v, t, cmp);
				case "~=": return v.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries).Any(x => string.Equals(x, t, cmp));
				case "|=": return string.Equals(v, t, cmp) || v.StartsWith(t + "-", cmp);
				case "^=": return t.Length > 0 && v.StartsWith(t, cmp);
				case "$=": return t.Length > 0 && v.EndsWith(t, cmp);
				case "*=": return t.Length > 0 && v.IndexOf(t, cmp) >= 0;
			}
			return false;
		}

		static bool NthMatch(int a, int b, int pos) {
			if (a == 0) return pos == b;
			int diff = pos - b;
			return diff / a >= 0 && diff % a == 0;
		}

		static bool MatchPseudo(PseudoSel p, Element e) {
			switch (p.Name) {
				case "root": return e.Parent == null;
				case "scope": return e.Parent == null;
				case "first-child": return e.Parent != null && e.PrevElementSibling() == null;
				case "last-child": return e.Parent != null && e.NextElementSibling() == null;
				case "only-child": return e.Parent != null && e.PrevElementSibling() == null && e.NextElementSibling() == null;
				case "first-of-type": { for (var s = e.PrevElementSibling(); s != null; s = s.PrevElementSibling()) if (s.Tag == e.Tag) return false; return e.Parent != null; }
				case "last-of-type": { for (var s = e.NextElementSibling(); s != null; s = s.NextElementSibling()) if (s.Tag == e.Tag) return false; return e.Parent != null; }
				case "only-of-type": return MatchPseudo(new PseudoSel { Name = "first-of-type" }, e) && MatchPseudo(new PseudoSel { Name = "last-of-type" }, e);
				case "nth-child": case "nth-last-child": case "nth-of-type": case "nth-last-of-type": {
					if (e.Parent == null) return false;
					if (p.Of != null && !p.Of.Any(x => Matches(x, e))) return false;
					bool last = p.Name.Contains("last");
					bool ofType = p.Name.EndsWith("of-type");
					int pos = 1;
					var sib = last ? e.NextElementSibling() : e.PrevElementSibling();
					while (sib != null) {
						bool count = ofType ? sib.Tag == e.Tag : p.Of == null || p.Of.Any(x => Matches(x, sib));
						if (count) pos++;
						sib = last ? sib.NextElementSibling() : sib.PrevElementSibling();
					}
					return NthMatch(p.A, p.B, pos);
				}
				case "empty": return e.Children.All(c => c is TextNode t && t.Text.Length == 0);
				case "not": return p.Sub != null && !p.Sub.Any(s => Matches(s, e));
				case "is": case "matches": case "any": case "-webkit-any": case "where": return p.Sub != null && p.Sub.Any(s => Matches(s, e));
				case "has": return p.Sub != null && p.Sub.Any(s => HasMatch(s, e));
				case "link": case "any-link": case "-webkit-any-link": return (e.Tag == "a" || e.Tag == "area") && e.Attr("href") != null;
				case "visited": case "hover": case "active": case "focus": case "focus-within": case "focus-visible": case "target": case "placeholder-shown": case "indeterminate": case "invalid": case "user-invalid": case "autofill": case "fullscreen": case "modal": case "popover-open": case "open": return false;
				case "checked": return e.Attr("checked") != null || e.Tag == "option" && e.Attr("selected") != null;
				case "disabled": return e.Attr("disabled") != null;
				case "enabled": return (e.Tag == "input" || e.Tag == "button" || e.Tag == "select" || e.Tag == "textarea") && e.Attr("disabled") == null;
				case "required": return e.Attr("required") != null;
				case "optional": return (e.Tag == "input" || e.Tag == "select" || e.Tag == "textarea") && e.Attr("required") == null;
				case "read-only": return e.Tag != "input" && e.Tag != "textarea" || e.Attr("readonly") != null;
				case "read-write": return (e.Tag == "input" || e.Tag == "textarea") && e.Attr("readonly") == null;
				case "valid": case "defined": case "first": case "in-range": return true;
				case "lang": {
					string want = Unquote(p.Arg ?? "").ToLowerInvariant();
					for (var x = e; x != null; x = x.Parent) {
						string l = x.Attr("lang");
						if (l != null) { l = l.ToLowerInvariant(); return l == want || l.StartsWith(want + "-"); }
					}
					return false;
				}
				case "dir": {
					string want = (p.Arg ?? "").Trim().ToLowerInvariant();
					bool rtl = e.Style != null ? e.Style.Rtl : DirOf(e);
					return want == "rtl" ? rtl : !rtl;
				}
				default: return false;
			}
		}

		static bool DirOf(Element e) {
			for (var x = e; x != null; x = x.Parent) {
				string d = x.Attr("dir");
				if (d != null) return d.Equals("rtl", StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		static bool HasMatch(Selector s, Element e) {
			if (s.PseudoElement == "has-rel") {
				char comb = s.Combs[0];
				var inner = new Selector { Parts = s.Parts, Combs = s.Combs.Skip(1).ToList(), Spec = s.Spec };
				if (comb == '>') return e.ChildElements().Any(ch => MatchesAnchored(inner, ch, e, true));
				if (comb == '+') { var n = e.NextElementSibling(); return n != null && Matches(inner, n); }
				if (comb == '~') { for (var n = e.NextElementSibling(); n != null; n = n.NextElementSibling()) if (Matches(inner, n)) return true; return false; }
			}
			return e.Descendants().Any(d => Matches(s, d));
		}

		static bool MatchesAnchored(Selector s, Element e, Element anchor, bool direct) => Matches(s, e) && (!direct || e.Parent == anchor);

		static readonly Dictionary<string, Rgba> Named = BuildNamed();

		static Dictionary<string, Rgba> BuildNamed() {
			string data = "aliceblue:f0f8ff antiquewhite:faebd7 aqua:00ffff aquamarine:7fffd4 azure:f0ffff beige:f5f5dc bisque:ffe4c4 black:000000 blanchedalmond:ffebcd blue:0000ff blueviolet:8a2be2 brown:a52a2a burlywood:deb887 cadetblue:5f9ea0 chartreuse:7fff00 chocolate:d2691e coral:ff7f50 cornflowerblue:6495ed cornsilk:fff8dc crimson:dc143c cyan:00ffff darkblue:00008b darkcyan:008b8b darkgoldenrod:b8860b darkgray:a9a9a9 darkgreen:006400 darkgrey:a9a9a9 darkkhaki:bdb76b darkmagenta:8b008b darkolivegreen:556b2f darkorange:ff8c00 darkorchid:9932cc darkred:8b0000 darksalmon:e9967a darkseagreen:8fbc8f darkslateblue:483d8b darkslategray:2f4f4f darkslategrey:2f4f4f darkturquoise:00ced1 darkviolet:9400d3 deeppink:ff1493 deepskyblue:00bfff dimgray:696969 dimgrey:696969 dodgerblue:1e90ff firebrick:b22222 floralwhite:fffaf0 forestgreen:228b22 fuchsia:ff00ff gainsboro:dcdcdc ghostwhite:f8f8ff gold:ffd700 goldenrod:daa520 gray:808080 green:008000 greenyellow:adff2f grey:808080 honeydew:f0fff0 hotpink:ff69b4 indianred:cd5c5c indigo:4b0082 ivory:fffff0 khaki:f0e68c lavender:e6e6fa lavenderblush:fff0f5 lawngreen:7cfc00 lemonchiffon:fffacd lightblue:add8e6 lightcoral:f08080 lightcyan:e0ffff lightgoldenrodyellow:fafad2 lightgray:d3d3d3 lightgreen:90ee90 lightgrey:d3d3d3 lightpink:ffb6c1 lightsalmon:ffa07a lightseagreen:20b2aa lightskyblue:87cefa lightslategray:778899 lightslategrey:778899 lightsteelblue:b0c4de lightyellow:ffffe0 lime:00ff00 limegreen:32cd32 linen:faf0e6 magenta:ff00ff maroon:800000 mediumaquamarine:66cdaa mediumblue:0000cd mediumorchid:ba55d3 mediumpurple:9370db mediumseagreen:3cb371 mediumslateblue:7b68ee mediumspringgreen:00fa9a mediumturquoise:48d1cc mediumvioletred:c71585 midnightblue:191970 mintcream:f5fffa mistyrose:ffe4e1 moccasin:ffe4b5 navajowhite:ffdead navy:000080 oldlace:fdf5e6 olive:808000 olivedrab:6b8e23 orange:ffa500 orangered:ff4500 orchid:da70d6 palegoldenrod:eee8aa palegreen:98fb98 paleturquoise:afeeee palevioletred:db7093 papayawhip:ffefd5 peachpuff:ffdab9 peru:cd853f pink:ffc0cb plum:dda0dd powderblue:b0e0e6 purple:800080 rebeccapurple:663399 red:ff0000 rosybrown:bc8f8f royalblue:4169e1 saddlebrown:8b4513 salmon:fa8072 sandybrown:f4a460 seagreen:2e8b57 seashell:fff5ee sienna:a0522d silver:c0c0c0 skyblue:87ceeb slateblue:6a5acd slategray:708090 slategrey:708090 snow:fffafa springgreen:00ff7f steelblue:4682b4 tan:d2b48c teal:008080 thistle:d8bfd8 tomato:ff6347 turquoise:40e0d0 violet:ee82ee wheat:f5deb3 white:ffffff whitesmoke:f5f5f5 yellow:ffff00 yellowgreen:9acd32 " +
				"canvas:ffffff canvastext:000000 linktext:0000ee visitedtext:551a8b activetext:ff0000 buttonface:efefef buttontext:000000 buttonborder:767676 field:ffffff fieldtext:000000 highlight:b5d5ff highlighttext:000000 graytext:808080 mark:ffff00 marktext:000000 threedface:c0c0c0 threedshadow:808080 threedhighlight:ffffff windowtext:000000 window:ffffff -webkit-link:0000ee";
			var d = new Dictionary<string, Rgba>(StringComparer.OrdinalIgnoreCase);
			foreach (var p in data.Split(' ', StringSplitOptions.RemoveEmptyEntries)) {
				int c = p.IndexOf(':');
				int v = int.Parse(p.Substring(c + 1), NumberStyles.HexNumber);
				d[p.Substring(0, c)] = new Rgba(v >> 16 & 255, v >> 8 & 255, v & 255);
			}
			return d;
		}

		public static Rgba? ParseColor(string v) {
			if (string.IsNullOrEmpty(v)) return null;
			v = v.Trim();
			if (v.Length == 0) return null;
			if (v[0] == '#') {
				string h = v.Substring(1);
				if (!h.All(Uri.IsHexDigit)) return null;
				int H(int i, int l) => int.Parse(h.Substring(i, l), NumberStyles.HexNumber);
				switch (h.Length) {
					case 3: return new Rgba(H(0, 1) * 17, H(1, 1) * 17, H(2, 1) * 17);
					case 4: return new Rgba(H(0, 1) * 17, H(1, 1) * 17, H(2, 1) * 17, H(3, 1) * 17);
					case 6: return new Rgba(H(0, 2), H(2, 2), H(4, 2));
					case 8: return new Rgba(H(0, 2), H(2, 2), H(4, 2), H(6, 2));
				}
				return null;
			}
			string lv = v.ToLowerInvariant();
			if (lv == "transparent") return Rgba.Transparent;
			if (lv == "currentcolor") return Rgba.CurrentColor;
			if (Named.TryGetValue(lv, out var nc)) return nc;
			int p = lv.IndexOf('(');
			if (p < 0 || !lv.EndsWith(")")) return null;
			string fn = lv.Substring(0, p);
			string inner = lv.Substring(p + 1, lv.Length - p - 2);
			var parts = inner.Replace(",", " ").Replace("/", " / ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			int slash = parts.IndexOf("/");
			string alphaS = null;
			if (slash >= 0) { alphaS = slash + 1 < parts.Count ? parts[slash + 1] : null; parts.RemoveRange(slash, parts.Count - slash); }
			else if (parts.Count == 4) { alphaS = parts[3]; parts.RemoveAt(3); }
			if (parts.Count < 3) return null;
			double A() {
				if (alphaS == null) return 1;
				if (alphaS.EndsWith("%")) return Num(alphaS[..^1]) / 100;
				return Num(alphaS);
			}
			double Num(string s) => s == "none" ? 0 : double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double d) ? d : 0;
			double Ch(string s, double scale) => s.EndsWith("%") ? Num(s[..^1]) * scale / 100 : Num(s);
			double Hue(string s) {
				if (s.EndsWith("deg")) return Num(s[..^3]);
				if (s.EndsWith("grad")) return Num(s[..^4]) * 0.9;
				if (s.EndsWith("rad")) return Num(s[..^3]) * 180 / Math.PI;
				if (s.EndsWith("turn")) return Num(s[..^4]) * 360;
				return Num(s);
			}
			int Alpha255() => (int)Math.Round(Math.Clamp(A(), 0, 1) * 255);
			switch (fn) {
				case "rgb": case "rgba":
					return new Rgba((int)Math.Round(Ch(parts[0], 255)), (int)Math.Round(Ch(parts[1], 255)), (int)Math.Round(Ch(parts[2], 255)), Alpha255());
				case "hsl": case "hsla": {
					double h = Hue(parts[0]), s = Ch(parts[1], 100) / 100, l = Ch(parts[2], 100) / 100;
					HslToRgb(h, s, l, out double r, out double g, out double b);
					return new Rgba((int)Math.Round(r * 255), (int)Math.Round(g * 255), (int)Math.Round(b * 255), Alpha255());
				}
				case "hwb": {
					double h = Hue(parts[0]), w = Ch(parts[1], 100) / 100, bl = Ch(parts[2], 100) / 100;
					if (w + bl >= 1) { int gr = (int)Math.Round(w / (w + bl) * 255); return new Rgba(gr, gr, gr, Alpha255()); }
					HslToRgb(h, 1, 0.5, out double r, out double g, out double b);
					double f = 1 - w - bl;
					return new Rgba((int)Math.Round((r * f + w) * 255), (int)Math.Round((g * f + w) * 255), (int)Math.Round((b * f + w) * 255), Alpha255());
				}
				case "lab": case "lch": case "oklab": case "oklch": {
					double L = Ch(parts[0], fn.StartsWith("ok") ? 1 : 100);
					double a, bb;
					if (fn.EndsWith("ch")) { double C = Ch(parts[1], fn.StartsWith("ok") ? 0.4 : 150), H = Hue(parts[2]) * Math.PI / 180; a = C * Math.Cos(H); bb = C * Math.Sin(H); }
					else { a = Ch(parts[1], fn.StartsWith("ok") ? 0.4 : 125); bb = Ch(parts[2], fn.StartsWith("ok") ? 0.4 : 125); }
					double r, g, b2;
					if (fn.StartsWith("ok")) OklabToSrgb(L, a, bb, out r, out g, out b2);
					else LabToSrgb(L, a, bb, out r, out g, out b2);
					return new Rgba((int)Math.Round(Math.Clamp(r, 0, 1) * 255), (int)Math.Round(Math.Clamp(g, 0, 1) * 255), (int)Math.Round(Math.Clamp(b2, 0, 1) * 255), Alpha255());
				}
			}
			return null;
		}

		static double Gamma(double x) => x <= 0.0031308 ? 12.92 * x : 1.055 * Math.Pow(x, 1 / 2.4) - 0.055;

		static void OklabToSrgb(double L, double a, double b, out double r, out double g, out double bl) {
			double l_ = L + 0.3963377774 * a + 0.2158037573 * b, m_ = L - 0.1055613458 * a - 0.0638541728 * b, s_ = L - 0.0894841775 * a - 1.2914855480 * b;
			double l = l_ * l_ * l_, m = m_ * m_ * m_, s = s_ * s_ * s_;
			r = Gamma(4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s);
			g = Gamma(-1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s);
			bl = Gamma(-0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s);
		}

		static void LabToSrgb(double L, double a, double b, out double r, out double g, out double bl) {
			double fy = (L + 16) / 116, fx = fy + a / 500, fz = fy - b / 200;
			double F(double t) => t * t * t > 0.008856 ? t * t * t : (116 * t - 16) / 903.3;
			double X = F(fx) * 0.96422, Y = F(fy), Z = F(fz) * 0.82521;
			double lr = 3.1338561 * X - 1.6168667 * Y - 0.4906146 * Z, lg = -0.9787684 * X + 1.9161415 * Y + 0.0334540 * Z, lb = 0.0719453 * X - 0.2289914 * Y + 1.4052427 * Z;
			r = Gamma(lr); g = Gamma(lg); bl = Gamma(lb);
		}

		static void HslToRgb(double h, double s, double l, out double r, out double g, out double b) {
			h = ((h % 360) + 360) % 360 / 360;
			s = Math.Clamp(s, 0, 1); l = Math.Clamp(l, 0, 1);
			double q = l < 0.5 ? l * (1 + s) : l + s - l * s, p = 2 * l - q;
			double Hue2(double t) {
				if (t < 0) t += 1;
				if (t > 1) t -= 1;
				if (t < 1.0 / 6) return p + (q - p) * 6 * t;
				if (t < 0.5) return q;
				if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
				return p;
			}
			r = Hue2(h + 1.0 / 3); g = Hue2(h); b = Hue2(h - 1.0 / 3);
		}

		public const string UserAgentSheet = @"
html, address, blockquote, body, center, dialog, div, figure, figcaption, footer, form, header, hr, legend, listing, main, p, plaintext, pre, search, xmp, article, aside, h1, h2, h3, h4, h5, h6, hgroup, nav, section, dir, dd, dl, dt, menu, ol, ul, details, summary, fieldset, optgroup, address, frameset, frame { display: block }
head, script, style, link, meta, title, template, noscript, datalist, area, base, param, noframes, rp, [hidden], input[type=hidden], dialog:not([open]), template, source, track, colgroup > col[hidden] { display: none }
html { direction: ltr }
body { margin: 8px }
p { margin-top: 1em; margin-bottom: 1em }
blockquote { margin: 1em 40px }
figure { margin: 1em 40px }
address { font-style: italic }
center { text-align: -webkit-center }
hr { color: gray; border-style: inset; border-width: 1px; margin: 0.5em auto; overflow: hidden }
h1 { font-size: 2em; margin-top: 0.67em; margin-bottom: 0.67em; font-weight: bold }
h2 { font-size: 1.5em; margin-top: 0.83em; margin-bottom: 0.83em; font-weight: bold }
h3 { font-size: 1.17em; margin-top: 1em; margin-bottom: 1em; font-weight: bold }
h4 { margin-top: 1.33em; margin-bottom: 1.33em; font-weight: bold }
h5 { font-size: 0.83em; margin-top: 1.67em; margin-bottom: 1.67em; font-weight: bold }
h6 { font-size: 0.67em; margin-top: 2.33em; margin-bottom: 2.33em; font-weight: bold }
ul, menu, dir { list-style-type: disc; margin-top: 1em; margin-bottom: 1em; padding-inline-start: 40px }
ol { list-style-type: decimal; margin-top: 1em; margin-bottom: 1em; padding-inline-start: 40px }
li { display: list-item; text-align: match-parent }
ul ul, ol ul, menu ul, dir ul, ul menu, ol menu { list-style-type: circle }
ul ul ul, ul ol ul, ol ul ul, ol ol ul { list-style-type: square }
ul ul, ul ol, ol ul, ol ol, ul menu, ol menu { margin-top: 0; margin-bottom: 0 }
dd { margin-inline-start: 40px }
dl { margin-top: 1em; margin-bottom: 1em }
dl dl, ol dl, ul dl { margin-top: 0; margin-bottom: 0 }
pre, xmp, plaintext, listing { font-family: monospace; white-space: pre; margin-top: 1em; margin-bottom: 1em }
code, kbd, samp, tt { font-family: monospace }
b, strong { font-weight: bolder }
i, cite, em, var, dfn { font-style: italic }
u, ins { text-decoration: underline }
s, strike, del { text-decoration: line-through }
abbr[title], acronym[title] { text-decoration: underline dotted }
big { font-size: larger }
small { font-size: smaller }
sub { vertical-align: sub; font-size: smaller }
sup { vertical-align: super; font-size: smaller }
nobr { white-space: nowrap }
mark { background-color: yellow; color: black }
a:any-link { color: #0000ee; text-decoration: underline }
q::before { content: open-quote }
q::after { content: close-quote }
table { display: table; border-collapse: separate; border-spacing: 2px; border-color: gray; box-sizing: border-box; text-indent: initial }
thead { display: table-header-group; vertical-align: middle; border-color: inherit }
tbody { display: table-row-group; vertical-align: middle; border-color: inherit }
tfoot { display: table-footer-group; vertical-align: middle; border-color: inherit }
col { display: table-column }
colgroup { display: table-column-group }
tr { display: table-row; vertical-align: inherit; border-color: inherit }
td, th { display: table-cell; vertical-align: inherit; padding: 1px }
th { font-weight: bold; text-align: -internal-center }
caption { display: table-caption; text-align: -webkit-center }
img, svg, video, canvas, iframe, embed, object { overflow: clip }
iframe { border: 2px inset }
video, audio, canvas, iframe, object, embed { display: inline-block }
fieldset { margin-inline: 2px; padding-block: 0.35em 0.625em; padding-inline: 0.75em; border: 2px groove threedface; min-width: min-content }
legend { padding-inline: 2px }
input, textarea, select, button { margin: 0; font: 13.333px Arial; letter-spacing: normal; word-spacing: normal; text-transform: none; text-indent: 0; text-align: start; display: inline-block; color: fieldtext }
input { padding: 1px 2px; border: 2px inset #767676; background-color: field }
input[type=checkbox], input[type=radio] { margin: 3px 3px 3px 4px; padding: 0; border: 1px solid #767676; width: 13px; height: 13px; box-sizing: border-box }
input[type=radio] { border-radius: 50%; margin: 3px 3px 0 5px }
input[type=submit], input[type=button], input[type=reset], button { padding: 1px 6px; border: 2px outset buttonborder; background-color: buttonface; color: buttontext; text-align: center; border-radius: 0 }
textarea { padding: 2px; border: 1px solid #767676; white-space: pre-wrap; font-family: monospace; background-color: field }
select { border: 1px solid #767676; border-radius: 0; padding: 0; background-color: field }
option { display: block; padding-inline: 2px 1px; min-height: 1.2em; white-space: nowrap }
select option { display: none }
select option[selected], select option:first-child { display: block }
select:has(option[selected]) option:first-child:not([selected]) { display: none }
progress, meter { display: inline-block; width: 10em; height: 1em; vertical-align: -0.2em }
bdi, output { unicode-bidi: isolate }
bdo { unicode-bidi: bidi-override }
bdo[dir] { unicode-bidi: isolate-override }
[dir] { unicode-bidi: isolate }
address, blockquote, center, div, figure, figcaption, footer, form, header, hr, legend, listing, main, p, plaintext, pre, summary, xmp, article, aside, h1, h2, h3, h4, h5, h6, hgroup, nav, section, table, caption, colgroup, col, thead, tbody, tfoot, tr, td, th, dir, dd, dl, dt, menu, ol, ul, li, bdi, output, [dir=ltr i], [dir=rtl i], [dir=auto i] { unicode-bidi: isolate }
textarea[dir=auto i], pre[dir=auto i] { unicode-bidi: plaintext }
ruby { display: ruby }
rt { display: ruby-text; font-size: 50% }
summary { display: block }
details > summary:first-of-type { display: list-item; list-style: disclosure-closed inside }
details[open] > summary:first-of-type { list-style-type: disclosure-open }
details:not([open]) > :not(summary:first-of-type) { display: none }
dialog { position: absolute; inset-inline-start: 0; inset-inline-end: 0; width: fit-content; height: fit-content; margin: auto; border: solid; padding: 1em; background-color: canvas; color: canvastext }
slot { display: contents }
wbr { display: inline }
br { display: inline }
";
	}

	internal struct LenCtx {
		public double Em, Rem, Vw, Vh;
	}

	internal static class Val {
		public static bool Num(string s, out double d) => double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out d);

		static int NumEnd(string v) {
			int i = 0;
			if (i < v.Length && (v[i] == '+' || v[i] == '-')) i++;
			bool digits = false;
			while (i < v.Length && char.IsDigit(v[i])) { i++; digits = true; }
			if (i < v.Length && v[i] == '.') { i++; while (i < v.Length && char.IsDigit(v[i])) { i++; digits = true; } }
			if (!digits) return -1;
			if (i < v.Length && (v[i] == 'e' || v[i] == 'E') && i + 1 < v.Length && (char.IsDigit(v[i + 1]) || (v[i + 1] == '-' || v[i + 1] == '+') && i + 2 < v.Length && char.IsDigit(v[i + 2]))) {
				i += 2;
				while (i < v.Length && char.IsDigit(v[i])) i++;
			}
			return i;
		}

		public static Len? ParseLen(string v, LenCtx c, bool allowUnitless = false) {
			if (v == null) return null;
			v = v.Trim();
			if (v.Length == 0) return null;
			string lv = v.ToLowerInvariant();
			switch (lv) {
				case "auto": return Len.Auto;
				case "none": return Len.None;
				case "normal": return Len.Normal;
				case "min-content": case "-webkit-min-content": return new Len { K = Len.KMin };
				case "max-content": case "-webkit-max-content": return new Len { K = Len.KMax };
				case "fit-content": case "-webkit-fit-content": case "-moz-fit-content": return new Len { K = Len.KFit };
				case "stretch": case "-webkit-fill-available": case "-moz-available": return new Len { K = Len.KStretch };
			}
			if (lv.StartsWith("calc(") || lv.StartsWith("min(") || lv.StartsWith("max(") || lv.StartsWith("clamp(") || lv.StartsWith("-webkit-calc(")) {
				var e = new CalcParser(lv, c).ParseTop();
				if (e == null) return null;
				return e.Value;
			}
			if (lv.StartsWith("fit-content(")) return new Len { K = Len.KFit };
			int ne = NumEnd(lv);
			if (ne < 0) return null;
			if (!Num(lv.Substring(0, ne), out double d)) return null;
			string unit = lv.Substring(ne);
			if (unit == "%") return Len.PctV(d);
			if (unit.Length == 0) {
				if (d == 0 || allowUnitless) return Len.PxV(d);
				return null;
			}
			double f = Factor(unit, c);
			if (double.IsNaN(f)) return null;
			return Len.PxV(d * f);
		}

		public static double Factor(string unit, LenCtx c) {
			switch (unit) {
				case "vw": return c.Vw / 100;
				case "vh": return c.Vh / 100;
				case "vmin": return Math.Min(c.Vw, c.Vh) / 100;
				case "vmax": return Math.Max(c.Vw, c.Vh) / 100;
				case "svw": case "lvw": case "dvw": return c.Vw / 100;
				case "svh": case "lvh": case "dvh": return c.Vh / 100;
			}
			return Css.UnitFactor(unit, c.Em, c.Rem, c.Em * 0.5);
		}

		public static double? ParseNumber(string v) {
			if (v == null) return null;
			v = v.Trim();
			if (v.StartsWith("calc(", StringComparison.OrdinalIgnoreCase)) {
				var e = new CalcParser(v.ToLowerInvariant(), new LenCtx { Em = 16, Rem = 16 }).ParseTop();
				if (e == null) return null;
				return e.Value.Px;
			}
			return Num(v, out double d) ? d : null;
		}

		public static double ParseAngle(string v) {
			v = v.Trim().ToLowerInvariant();
			int ne = NumEnd(v);
			if (ne < 0) return double.NaN;
			if (!Num(v.Substring(0, ne), out double d)) return double.NaN;
			string u = v.Substring(ne);
			return u switch { "deg" => d, "rad" => d * 180 / Math.PI, "grad" => d * 0.9, "turn" => d * 360, "" => d == 0 ? 0 : double.NaN, _ => double.NaN };
		}

		sealed class CalcParser {
			readonly string s;
			int i;
			readonly LenCtx c;
			public CalcParser(string s, LenCtx c) { this.s = s; this.c = c; }

			void Ws() { while (i < s.Length && char.IsWhiteSpace(s[i])) i++; }

			public Len? ParseTop() {
				var r = Primary();
				return r;
			}

			Len? Primary() {
				Ws();
				if (i >= s.Length) return null;
				if (s[i] == '(') { i++; var r = Sum(); Ws(); if (i < s.Length && s[i] == ')') i++; return r; }
				int st = i;
				while (i < s.Length && (char.IsLetter(s[i]) || s[i] == '-') && !(s[i] == '-' && i + 1 < s.Length && (char.IsDigit(s[i + 1]) || s[i + 1] == '.'))) i++;
				string fn = s.Substring(st, i - st);
				if (fn.Length > 0 && i < s.Length && s[i] == '(') {
					i++;
					if (fn == "calc" || fn == "-webkit-calc") { var r = Sum(); Ws(); if (i < s.Length && s[i] == ')') i++; return r; }
					var args = new List<Len>();
					while (true) {
						var a = Sum();
						if (a == null) return null;
						args.Add(a.Value);
						Ws();
						if (i < s.Length && s[i] == ',') { i++; continue; }
						if (i < s.Length && s[i] == ')') { i++; break; }
						return null;
					}
					return Combine(fn, args);
				}
				if (fn.Length > 0) {
					if (fn == "pi") return Len.PxV(Math.PI);
					if (fn == "e") return Len.PxV(Math.E);
					i = st;
					return null;
				}
				int ne = i;
				if (ne < s.Length && (s[ne] == '+' || s[ne] == '-')) ne++;
				while (ne < s.Length && (char.IsDigit(s[ne]) || s[ne] == '.')) ne++;
				if (ne < s.Length && s[ne] == 'e' && ne + 1 < s.Length && (char.IsDigit(s[ne + 1]) || s[ne + 1] == '-')) { ne += 2; while (ne < s.Length && char.IsDigit(s[ne])) ne++; }
				if (!Num(s.Substring(i, ne - i), out double d)) return null;
				i = ne;
				int us = i;
				while (i < s.Length && (char.IsLetter(s[i]) || s[i] == '%')) i++;
				string unit = s.Substring(us, i - us);
				if (unit == "%") return Len.PctV(d);
				if (unit.Length == 0) return new Len { K = Len.Value, Px = d, Pct = double.NaN };
				double f = Factor(unit, c);
				if (double.IsNaN(f)) return null;
				return Len.PxV(d * f);
			}

			static bool IsNumber(Len l) => double.IsNaN(l.Pct) && !l.HasPct && l.Fn == null;

			Len? Product() {
				var a = Primary();
				if (a == null) return null;
				while (true) {
					Ws();
					if (i >= s.Length) break;
					char op = s[i];
					if (op != '*' && op != '/') break;
					i++;
					var b = Primary();
					if (b == null) return null;
					if (op == '*') {
						if (IsNumber(b.Value)) a = Scale(a.Value, b.Value.Px);
						else if (IsNumber(a.Value)) a = Scale(b.Value, a.Value.Px);
						else return null;
					}
					else {
						if (!IsNumber(b.Value) || b.Value.Px == 0) return null;
						a = Scale(a.Value, 1 / b.Value.Px);
					}
				}
				return a;
			}

			Len? Sum() {
				var a = Product();
				if (a == null) return null;
				while (true) {
					Ws();
					if (i >= s.Length) break;
					char op = s[i];
					if (op != '+' && op != '-') break;
					i++;
					var b = Product();
					if (b == null) return null;
					a = Add(a.Value, op == '-' ? Scale(b.Value, -1) : b.Value);
				}
				return a;
			}

			static Len Scale(Len l, double k) {
				if (l.Fn != null) { var f = l.Fn; return new Len { K = Len.Value, Fn = b => f(b) * k, HasPct = true }; }
				return new Len { K = Len.Value, Px = l.Px * k, Pct = double.IsNaN(l.Pct) ? double.NaN : l.Pct * k, HasPct = l.HasPct };
			}

			static Len Add(Len a, Len b) {
				if (a.Fn != null || b.Fn != null) {
					Func<double, double> fa = a.Fn ?? (x => a.Px + (a.HasPct ? a.Pct * x / 100 : 0));
					Func<double, double> fb = b.Fn ?? (x => b.Px + (b.HasPct ? b.Pct * x / 100 : 0));
					return new Len { K = Len.Value, Fn = x => fa(x) + fb(x), HasPct = true };
				}
				double pa = double.IsNaN(a.Pct) ? 0 : a.Pct, pb = double.IsNaN(b.Pct) ? 0 : b.Pct;
				bool bothNum = double.IsNaN(a.Pct) && double.IsNaN(b.Pct);
				return new Len { K = Len.Value, Px = a.Px + b.Px, Pct = bothNum ? double.NaN : pa + pb, HasPct = a.HasPct || b.HasPct };
			}

			static Len Combine(string fn, List<Len> args) {
				bool anyPct = args.Any(a => a.HasPct || a.Fn != null);
				Func<double, double> F(Len l) => l.Fn ?? (x => l.Px + (l.HasPct ? l.Pct * x / 100 : 0));
				var fs = args.Select(F).ToList();
				Func<double, double> res = fn switch {
					"min" => x => fs.Min(f => f(x)),
					"max" => x => fs.Max(f => f(x)),
					"clamp" when fs.Count == 3 => x => Math.Max(fs[0](x), Math.Min(fs[1](x), fs[2](x))),
					"abs" => x => Math.Abs(fs[0](x)),
					"round" => x => fs.Count > 1 ? Math.Round(fs[0](x) / fs[1](x)) * fs[1](x) : Math.Round(fs[0](x)),
					"sqrt" => x => Math.Sqrt(fs[0](x)),
					"pow" when fs.Count == 2 => x => Math.Pow(fs[0](x), fs[1](x)),
					"sin" => x => Math.Sin(fs[0](x)),
					"cos" => x => Math.Cos(fs[0](x)),
					_ => x => fs[0](x)
				};
				if (!anyPct) {
					double v = res(0);
					bool num = args.All(a => double.IsNaN(a.Pct) && !a.HasPct);
					return new Len { K = Len.Value, Px = v, Pct = num ? double.NaN : 0 };
				}
				return new Len { K = Len.Value, Fn = res, HasPct = true };
			}
		}
	}

	internal sealed class StyleResolver {
		readonly List<CssRule> _all = new();
		readonly Dictionary<string, List<CssRule>> _byId = new(), _byClass = new(), _byTag = new(StringComparer.OrdinalIgnoreCase);
		readonly List<CssRule> _universal = new();
		readonly MediaEnv _env;
		double _rootFontSize = 16;
		public bool PrintBackground = true;

		public StyleResolver(IEnumerable<CssRule> rules, MediaEnv env) {
			_env = env;
			foreach (var r in rules) {
				_all.Add(r);
				var last = r.Sel.Parts[r.Sel.Parts.Count - 1];
				if (last.Id != null) Add(_byId, last.Id, r);
				else if (last.Classes != null) Add(_byClass, last.Classes[0], r);
				else if (last.Tag != null) Add(_byTag, last.Tag, r);
				else _universal.Add(r);
			}
		}

		static void Add(Dictionary<string, List<CssRule>> d, string k, CssRule r) {
			if (!d.TryGetValue(k, out var l)) d[k] = l = new List<CssRule>();
			l.Add(r);
		}

		List<(CssDecl d, int origin, int spec, int order)> Collect(Element e, string pseudo) {
			var res = new List<(CssDecl, int, int, int)>();
			void Try(List<CssRule> rules) {
				if (rules == null) return;
				foreach (var r in rules) {
					if (r.Sel.PseudoElement != pseudo) continue;
					if (!Css.Matches(r.Sel, e)) continue;
					foreach (var d in r.Decls) res.Add((d, r.Origin, r.Sel.Spec, r.Order));
				}
			}
			if (e.Id != null && _byId.TryGetValue(e.Id, out var l1)) Try(l1);
			foreach (var c in e.Classes.Distinct()) if (_byClass.TryGetValue(c, out var l2)) Try(l2);
			if (_byTag.TryGetValue(e.Tag, out var l3)) Try(l3);
			Try(_universal);
			return res;
		}

		public void ComputeTree(HtmlDocument doc, LenCtx viewport) {
			var rootStyle = Compute(doc.Root, null, viewport);
			_rootFontSize = rootStyle.FontSize;
			doc.Root.Style = rootStyle;
			ComputePseudos(doc.Root, viewport);
			foreach (var c in doc.Root.ChildElements()) ComputeRec(c, viewport);
		}

		void ComputeRec(Element e, LenCtx vp) {
			e.Style = Compute(e, e.Parent.Style, vp);
			ComputePseudos(e, vp);
			foreach (var c in e.ChildElements()) ComputeRec(c, vp);
		}

		void ComputePseudos(Element e, LenCtx vp) {
			if (e.Style.Display == Disp.None) return;
			e.Before = ComputePseudo(e, "before", vp);
			e.After = ComputePseudo(e, "after", vp);
			if (e.Style.Display == Disp.ListItem) e.Marker = ComputePseudo(e, "marker", vp, true);
		}

		Style ComputePseudo(Element e, string pe, LenCtx vp, bool always = false) {
			var decls = Collect(e, pe);
			if (decls.Count == 0 && !always) return null;
			var s = Build(e, e.Style, decls, null, vp, true);
			if (!always && (s.Content == null || s.Content == "none" || s.Content == "normal")) return null;
			return s;
		}

		public Style Compute(Element e, Style parent, LenCtx vp) {
			var decls = Collect(e, null);
			var hints = PresentationalHints(e);
			string inline = e.Attr("style");
			List<CssDecl> inl = string.IsNullOrWhiteSpace(inline) ? null : Css.ParseDeclarations(Css.StripComments(inline));
			return Build(e, parent, decls, hints, vp, false, inl);
		}

		Style Build(Element e, Style parent, List<(CssDecl d, int origin, int spec, int order)> decls, List<CssDecl> hints, LenCtx vp, bool pseudo, List<CssDecl> inline = null) {
			var all = new List<(CssDecl d, int rank, int spec, int order)>();
			foreach (var x in decls) all.Add((x.d, x.origin == 0 ? (x.d.Important ? 5 : 0) : (x.d.Important ? 4 : 2), x.spec, x.order));
			if (hints != null) foreach (var h in hints) all.Add((h, 1, 0, 0));
			if (inline != null) { int k = 0; foreach (var d in inline) all.Add((d, d.Important ? 4 : 2, int.MaxValue, k++)); }
			all.Sort((a, b) => a.rank != b.rank ? a.rank.CompareTo(b.rank) : a.spec != b.spec ? a.spec.CompareTo(b.spec) : a.order.CompareTo(b.order));

			var s = parent != null ? parent.InheritFrom() : new Style();
			if (parent == null) s.Vars = null;
			Dictionary<string, string> vars = null;
			foreach (var x in all) {
				if (!x.d.Prop.StartsWith("--")) continue;
				vars ??= parent?.Vars != null ? new Dictionary<string, string>(parent.Vars) : new Dictionary<string, string>();
				string v = x.d.Value;
				if (v.Trim().Equals("initial", StringComparison.OrdinalIgnoreCase)) vars.Remove(x.d.Prop);
				else if (v.Trim().Equals("inherit", StringComparison.OrdinalIgnoreCase)) { if (parent?.Vars != null && parent.Vars.TryGetValue(x.d.Prop, out var pv)) vars[x.d.Prop] = pv; }
				else vars[x.d.Prop] = v;
			}
			if (vars != null) {
				var keys = vars.Keys.ToList();
				foreach (var k in keys) {
					string r = Substitute(vars[k], vars, 0, k);
					if (r == null) vars.Remove(k); else vars[k] = r;
				}
				s.Vars = vars;
			}

			var spec = new List<(string p, string v)>();
			string dirVal = null;
			foreach (var x in all) {
				if (x.d.Prop.StartsWith("--")) continue;
				string v = x.d.Value;
				if (v.IndexOf("var(", StringComparison.OrdinalIgnoreCase) >= 0 || v.IndexOf("env(", StringComparison.OrdinalIgnoreCase) >= 0) {
					v = Substitute(v, s.Vars, 0, null);
					if (v == null) v = "unset";
				}
				string p = NormalizeProp(x.d.Prop);
				if (p == null) continue;
				if (p == "direction") dirVal = v;
				spec.Add((p, v));
			}
			bool rtl = parent?.Rtl ?? false;
			if (dirVal != null) {
				string dv = dirVal.Trim().ToLowerInvariant();
				if (dv == "rtl") rtl = true; else if (dv == "ltr") rtl = false; else if (dv == "initial" || dv == "unset" && parent == null) rtl = false;
			}
			var final = new Dictionary<string, string>();
			var orderKeys = new List<string>();
			foreach (var (p, v) in spec) {
				foreach (var (lp, lv) in Shorthands.Expand(p, v, rtl)) {
					if (!final.ContainsKey(lp)) orderKeys.Add(lp);
					final[lp] = lv;
				}
			}

			double parentFs = parent?.FontSize ?? 16;
			if (final.TryGetValue("font-family", out var ffv)) ApplyProp(s, parent, "font-family", ffv, vp);
			if (final.TryGetValue("font-size", out var fsv)) ApplyProp(s, parent, "font-size", fsv, vp);
			else if (parent != null && parent.FontSizeKw && s.IsMonospace != parent.IsMonospace) {
				s.FontSize = parent.FontSize * (s.IsMonospace ? 13.0 / 16 : 16.0 / 13);
			}
			if (parent == null) _rootFontSize = s.FontSize;
			foreach (var k in orderKeys) {
				if (k == "font-size" || k == "font-family") continue;
				ApplyProp(s, parent, k, final[k], vp);
			}
			s.Rtl = rtl;
			if (e != null && e.Attr("lang") is string lg && !pseudo) s.Lang = lg;

			if (s.Position == Pos.Absolute || s.Position == Pos.Fixed) s.Float = 0;
			if (parent == null || s.Float != 0 || s.Position == Pos.Absolute || s.Position == Pos.Fixed) s.Display = Blockify(s.Display);
			if (parent != null && (parent.Display == Disp.Flex || parent.Display == Disp.InlineFlex || parent.Display == Disp.Grid || parent.Display == Disp.InlineGrid) && !pseudo) s.Display = Blockify(s.Display);
			if (pseudo && parent != null && (parent.Display == Disp.Flex || parent.Display == Disp.InlineFlex || parent.Display == Disp.Grid || parent.Display == Disp.InlineGrid)) s.Display = Blockify(s.Display);

			for (int i = 0; i < 4; i++) {
				if (s.BorderColor[i].Current) s.BorderColor[i] = s.Color;
				if (s.BorderStyle[i] == BS.None || s.BorderStyle[i] == BS.Hidden) s.BorderWidth[i] = 0;
			}
			if (s.TextDecoColor.Current) s.TextDecoColor = s.Color;
			if (s.OutlineColor.Current) s.OutlineColor = s.Color;
			if (s.OutlineStyle == BS.None) s.OutlineWidth = 0;
			if (s.BoxShadow != null) foreach (var sh in s.BoxShadow) if (sh.Color.Current) sh.Color = s.Color;
			if (s.TextShadow != null && s.TextShadow.Any(t => t.Color.Current)) s.TextShadow = s.TextShadow.Select(t => new Shadow { X = t.X, Y = t.Y, Blur = t.Blur, Color = t.Color.Current ? s.Color : t.Color }).ToList();
			if (s.Backgrounds != null) foreach (var bl in s.Backgrounds) if (bl.Grad != null) foreach (var st in bl.Grad.Stops) if (st.Color.Current) st.Color = s.Color;
			if (s.BackgroundColor.Current) s.BackgroundColor = s.Color;
			if (s.OverflowX != 0 && s.OverflowY == 0) s.OverflowY = 1;
			if (s.OverflowY != 0 && s.OverflowX == 0) s.OverflowX = 1;
			return s;
		}

		static Disp Blockify(Disp d) => d switch {
			Disp.Inline => Disp.Block, Disp.InlineBlock => Disp.Block, Disp.InlineTable => Disp.Table, Disp.InlineFlex => Disp.Flex, Disp.InlineGrid => Disp.Grid,
			Disp.RowGroup or Disp.HeaderGroup or Disp.FooterGroup or Disp.Row or Disp.Cell or Disp.Caption or Disp.Column or Disp.ColumnGroup => Disp.Block,
			_ => d
		};

		static string NormalizeProp(string p) {
			if (p.StartsWith("-webkit-") || p.StartsWith("-moz-") || p.StartsWith("-ms-") || p.StartsWith("-o-")) {
				string b = p.Substring(p.IndexOf('-', 1) + 1);
				switch (b) {
					case "print-color-adjust": return "print-color-adjust";
					case "box-shadow": case "transform": case "transform-origin": case "border-radius": case "box-sizing": case "flex": case "flex-direction": case "flex-wrap": case "flex-flow":
					case "flex-grow": case "flex-shrink": case "flex-basis": case "justify-content": case "align-items": case "align-self": case "align-content": case "order":
					case "border-top-left-radius": case "border-top-right-radius": case "border-bottom-left-radius": case "border-bottom-right-radius": case "hyphens":
					case "column-count": case "columns": case "background-size": case "background-clip": case "background-origin": case "text-decoration": case "text-decoration-line":
					case "text-decoration-color": case "text-decoration-style": case "font-feature-settings": case "font-kerning": case "font-variant-ligatures": case "box-decoration-break":
					case "clip-path": case "tab-size": case "line-break": case "text-align-last": case "margin-start": case "margin-end": case "padding-start": case "padding-end":
					case "border-start": case "border-end": case "text-size-adjust": case "user-select": case "appearance":
						return b switch { "margin-start" => "margin-inline-start", "margin-end" => "margin-inline-end", "padding-start" => "padding-inline-start", "padding-end" => "padding-inline-end", _ => b };
					case "box-orient": case "box-pack": case "box-align": case "box-flex": case "line-clamp": case "text-fill-color": case "text-stroke": case "text-stroke-width": case "text-stroke-color":
						return p;
				}
				return null;
			}
			switch (p) {
				case "word-wrap": return "overflow-wrap";
				case "grid-gap": return "gap";
				case "grid-row-gap": return "row-gap";
				case "grid-column-gap": return "column-gap";
				case "color-adjust": return "print-color-adjust";
				case "page-break-before": return "break-before";
				case "page-break-after": return "break-after";
				case "page-break-inside": return "break-inside";
			}
			return p;
		}

		string Substitute(string v, Dictionary<string, string> vars, int depth, string self) {
			if (depth > 20) return null;
			var sb = new StringBuilder();
			int i = 0;
			while (i < v.Length) {
				int k1 = v.IndexOf("var(", i, StringComparison.OrdinalIgnoreCase);
				int k2 = v.IndexOf("env(", i, StringComparison.OrdinalIgnoreCase);
				int k = k1 < 0 ? k2 : k2 < 0 ? k1 : Math.Min(k1, k2);
				if (k < 0) { sb.Append(v, i, v.Length - i); break; }
				sb.Append(v, i, k - i);
				bool env = k == k2;
				int st = k + 4, d = 1, j = st;
				while (j < v.Length && d > 0) { if (v[j] == '(') d++; else if (v[j] == ')') d--; j++; }
				string inner = v.Substring(st, Math.Max(0, j - st - 1));
				int comma = -1;
				{
					int dd = 0;
					for (int q = 0; q < inner.Length; q++) { if (inner[q] == '(') dd++; else if (inner[q] == ')') dd--; else if (inner[q] == ',' && dd == 0) { comma = q; break; } }
				}
				string name = (comma < 0 ? inner : inner.Substring(0, comma)).Trim();
				string fallback = comma < 0 ? null : inner.Substring(comma + 1).Trim();
				string rep = null;
				if (!env && vars != null && name != self && vars.TryGetValue(name, out var val)) rep = Substitute(val, vars, depth + 1, self);
				if (rep == null && fallback != null) rep = Substitute(fallback, vars, depth + 1, self);
				if (rep == null) return null;
				sb.Append(rep);
				i = j;
			}
			return sb.ToString();
		}

		List<CssDecl> PresentationalHints(Element e) {
			List<CssDecl> l = null;
			void H(string p, string v) => (l ??= new()).Add(new CssDecl { Prop = p, Value = v });
			string Dim(string v) {
				if (v == null) return null;
				v = v.Trim();
				if (v.EndsWith("%")) return Val.Num(v[..^1], out _) ? v : null;
				if (v.EndsWith("px")) v = v[..^2];
				if (Val.Num(v, out double d) && d >= 0) return d.ToString(CultureInfo.InvariantCulture) + "px";
				int k = 0; while (k < v.Length && (char.IsDigit(v[k]) || v[k] == '.')) k++;
				if (k > 0 && Val.Num(v.Substring(0, k), out d)) return d.ToString(CultureInfo.InvariantCulture) + "px";
				return null;
			}
			string tag = e.Tag;
			string a;
			if ((a = e.Attr("dir")) != null) { string d = a.ToLowerInvariant(); if (d == "rtl" || d == "ltr") H("direction", d); }
			if ((a = e.Attr("align")) != null) {
				string al = a.Trim().ToLowerInvariant();
				if (tag == "img" || tag == "object" || tag == "embed" || tag == "iframe" || tag == "input" && e.Attr("type") == "image") {
					if (al == "left" || al == "right") H("float", al);
					else if (al == "middle" || al == "absmiddle") H("vertical-align", "middle");
					else if (al == "top" || al == "bottom") H("vertical-align", al);
				}
				else if (tag == "table") {
					if (al == "left" || al == "right") H("float", al);
					else if (al == "center") { H("margin-left", "auto"); H("margin-right", "auto"); }
				}
				else if (tag == "hr") {
					if (al == "left") { H("margin-left", "0"); H("margin-right", "auto"); }
					else if (al == "right") { H("margin-left", "auto"); H("margin-right", "0"); }
				}
				else if (tag == "caption") H("caption-side", al == "bottom" ? "bottom" : "top");
				else {
					if (al == "center" || al == "middle") H("text-align", "-webkit-center");
					else if (al == "left") H("text-align", "-webkit-left");
					else if (al == "right") H("text-align", "-webkit-right");
					else if (al == "justify") H("text-align", "justify");
				}
			}
			if ((a = e.Attr("valign")) != null && (tag == "td" || tag == "th" || tag == "tr" || tag == "tbody" || tag == "thead" || tag == "tfoot" || tag == "col")) {
				string va = a.Trim().ToLowerInvariant();
				H("vertical-align", va == "center" ? "middle" : va);
			}
			if (tag == "img" || tag == "table" || tag == "td" || tag == "th" || tag == "col" || tag == "colgroup" || tag == "hr" || tag == "iframe" || tag == "video" || tag == "canvas" || tag == "embed" || tag == "object" || tag == "svg" || tag == "input" && e.Attr("type") == "image" || tag == "pre") {
				if ((a = Dim(e.Attr("width"))) != null && !(tag == "pre")) H("width", a);
				if ((a = Dim(e.Attr("height"))) != null && tag != "col" && tag != "colgroup" && !(tag == "table" && a.EndsWith("%"))) H("height", a);
			}
			if (tag == "img" || tag == "svg" || tag == "video" || tag == "canvas") {
				string w = e.Attr("width"), h = e.Attr("height");
				if (Val.Num(w?.Replace("px", ""), out double ww) && Val.Num(h?.Replace("px", ""), out double hh) && ww > 0 && hh > 0) H("aspect-ratio", (ww / hh).ToString(CultureInfo.InvariantCulture));
			}
			if ((a = e.Attr("bgcolor")) != null) { var c = LegacyColor(a); if (c != null) H("background-color", c); }
			if ((a = e.Attr("background")) != null && a.Trim().Length > 0 && tag != "img") H("background-image", "url(\"" + a.Trim() + "\")");
			if (tag == "font") {
				if ((a = e.Attr("color")) != null) { var c = LegacyColor(a); if (c != null) H("color", c); }
				if ((a = e.Attr("face")) != null) H("font-family", a);
				if ((a = e.Attr("size")) != null) {
					a = a.Trim();
					int n;
					if (a.StartsWith("+") && int.TryParse(a.Substring(1), out n)) n = 3 + n;
					else if (a.StartsWith("-") && int.TryParse(a.Substring(1), out n)) n = 3 - n;
					else if (!int.TryParse(a, out n)) n = 3;
					n = Math.Clamp(n, 1, 7);
					H("font-size", new[] { "x-small", "small", "medium", "large", "x-large", "xx-large", "xxx-large" }[n - 1]);
				}
			}
			if (tag == "body" && (a = e.Attr("text")) != null) { var c = LegacyColor(a); if (c != null) H("color", c); }
			if (tag == "table") {
				if ((a = e.Attr("border")) != null) {
					double bw = Val.Num(a.Trim(), out double bv) ? bv : a.Trim().Length == 0 ? 1 : 1;
					if (bw > 0) { H("border-width", bw + "px"); H("border-style", "outset"); H("border-color", "gray"); }
				}
				if ((a = e.Attr("cellspacing")) != null && Val.Num(a.Trim(), out double cs)) H("border-spacing", cs.ToString(CultureInfo.InvariantCulture) + "px");
				if (e.Attr("frame") is string fr) {
					string f = fr.ToLowerInvariant();
					if (f == "void") H("border-style", "hidden");
				}
				if (e.Attr("rules") != null) H("border-collapse", "collapse");
			}
			if (tag == "td" || tag == "th") {
				Element table = null;
				for (var p = e.Parent; p != null; p = p.Parent) if (p.Tag == "table") { table = p; break; }
				if (table != null) {
					if ((a = table.Attr("border")) != null && (!Val.Num(a.Trim(), out double bv) || bv > 0)) { H("border-width", "1px"); H("border-style", "inset"); H("border-color", "gray"); }
					if ((a = table.Attr("cellpadding")) != null && Val.Num(a.Trim(), out double cp)) H("padding", cp.ToString(CultureInfo.InvariantCulture) + "px");
					if (table.Attr("rules") is string rules) {
						string r = rules.ToLowerInvariant();
						if (r == "all") { H("border-width", "1px"); H("border-style", "solid"); }
						else if (r == "rows") { H("border-top-width", "1px"); H("border-top-style", "solid"); H("border-bottom-width", "1px"); H("border-bottom-style", "solid"); }
						else if (r == "cols") { H("border-left-width", "1px"); H("border-left-style", "solid"); H("border-right-width", "1px"); H("border-right-style", "solid"); }
					}
				}
				if (e.Attr("nowrap") != null) H("white-space", "nowrap");
			}
			if (tag == "ol" || tag == "ul" || tag == "li") {
				if ((a = e.Attr("type")) != null) {
					string t = a switch { "1" => "decimal", "a" => "lower-alpha", "A" => "upper-alpha", "i" => "lower-roman", "I" => "upper-roman", _ => a.ToLowerInvariant() switch { "disc" => "disc", "circle" => "circle", "square" => "square", "none" => "none", _ => null } };
					if (t != null) H("list-style-type", t);
				}
			}
			if (tag == "img" || tag == "object") {
				if ((a = e.Attr("hspace")) != null && Dim(a) is string hs) { H("margin-left", hs); H("margin-right", hs); }
				if ((a = e.Attr("vspace")) != null && Dim(a) is string vs) { H("margin-top", vs); H("margin-bottom", vs); }
				if ((a = e.Attr("border")) != null && Dim(a) is string bw) { H("border-width", bw); H("border-style", "solid"); }
			}
			if (tag == "hr") {
				if ((a = e.Attr("size")) != null && Val.Num(a, out double sz) && sz > 0) H("height", (sz - 2).ToString(CultureInfo.InvariantCulture) + "px");
				if (e.Attr("noshade") != null) { H("border-style", "solid"); H("background-color", "gray"); H("border-color", "gray"); }
				if ((a = e.Attr("color")) != null) { var c = LegacyColor(a); if (c != null) { H("border-style", "solid"); H("background-color", c); H("border-color", c); } }
			}
			if (tag == "br" && (a = e.Attr("clear")) != null) H("clear", a.ToLowerInvariant() == "all" ? "both" : a.ToLowerInvariant());
			if ((tag == "input" || tag == "textarea" || tag == "select") && (a = e.Attr("size")) != null && tag == "input" && Val.Num(a, out double isz)) H("width", (isz * 7 + 4).ToString(CultureInfo.InvariantCulture) + "px");
			return l;
		}

		static string LegacyColor(string v) {
			v = v.Trim();
			if (Css.ParseColor(v) != null) return v;
			if (v.Length == 6 && v.All(Uri.IsHexDigit)) return "#" + v;
			if (v.Length == 3 && v.All(Uri.IsHexDigit)) return "#" + v;
			return null;
		}

		public void ApplyProp(Style s, Style p, string prop, string value, LenCtx vp) {
			string v = value.Trim();
			string lv = v.ToLowerInvariant();
			bool inherited = IsInherited(prop);
			if (lv == "inherit" || lv == "unset" && inherited || lv == "revert" && inherited) { if (p != null) CopyProp(s, p, prop); else CopyProp(s, new Style(), prop); return; }
			if (lv == "initial" || lv == "unset" || lv == "revert" || lv == "revert-layer") { CopyProp(s, new Style(), prop); return; }
			var ctx = new LenCtx { Em = s.FontSize, Rem = _rootFontSize, Vw = vp.Vw, Vh = vp.Vh };
			Len? L(bool unitless = false) => Val.ParseLen(v, ctx, unitless);
			switch (prop) {
				case "color": { var c = Css.ParseColor(v); if (c != null) s.Color = c.Value.Current ? (p?.Color ?? Rgba.Black) : c.Value; break; }
				case "font-family": {
					var fams = Css.SplitTopLevel(v, ',').Select(x => Css.Unquote(x.Trim())).Where(x => x.Length > 0).ToArray();
					if (fams.Length > 0) s.FontFamily = fams.Select(f => f.ToLowerInvariant() switch { "serif" => "serif", "sans-serif" => "sans-serif", "monospace" => "monospace", "cursive" => "cursive", "fantasy" => "fantasy", "system-ui" => "system-ui", "-apple-system" => "system-ui", "blinkmacsystemfont" => "system-ui", "ui-sans-serif" => "sans-serif", "ui-serif" => "serif", "ui-monospace" => "monospace", "math" => "serif", _ => f }).ToArray();
					break;
				}
				case "font-size": {
					double pfs = p?.FontSize ?? 16;
					bool pmono = p?.IsMonospace ?? false;
					double baseMedium = s.IsMonospace ? 13 : 16;
					double[] kw = { 9, 10, 13, 16, 18, 24, 32, 48 };
					int ki = lv switch { "xx-small" => 0, "x-small" => 1, "small" => 2, "medium" => 3, "large" => 4, "x-large" => 5, "xx-large" => 6, "xxx-large" => 7, _ => -1 };
					if (ki >= 0) { s.FontSize = kw[ki] * baseMedium / 16; s.FontSizeKw = true; break; }
					if (lv == "smaller") { s.FontSize = pfs / 1.2; s.FontSizeKw = false; break; }
					if (lv == "larger") { s.FontSize = pfs * 1.2; s.FontSizeKw = false; break; }
					var fctx = new LenCtx { Em = pfs, Rem = p == null ? 16 : _rootFontSize, Vw = vp.Vw, Vh = vp.Vh };
					var l = Val.ParseLen(v, fctx);
					if (l != null && l.Value.IsValue) {
						double fs = l.Value.Resolve(pfs);
						if (lv.EndsWith("em") && !lv.EndsWith("rem") && p != null && p.FontSizeKw && s.IsMonospace != pmono) fs = fs / pfs * (s.IsMonospace ? pfs * 13 / 16 : pfs * 16 / 13);
						if (fs >= 0) { s.FontSize = fs; s.FontSizeKw = false; }
					}
					break;
				}
				case "font-weight": {
					int pw = p?.FontWeight ?? 400;
					if (lv == "bolder") s.FontWeight = pw < 350 ? 400 : pw < 550 ? 700 : pw < 900 ? 900 : pw;
					else if (lv == "lighter") s.FontWeight = pw < 550 ? 100 : pw < 750 ? 400 : 700;
					else s.FontWeight = Css.ParseWeightKw(lv, s.FontWeight);
					break;
				}
				case "font-style": s.FontStyle = (byte)(lv.StartsWith("italic") ? 1 : lv.StartsWith("oblique") ? 2 : 0); break;
				case "font-stretch": {
					s.FontStretch = lv switch { "ultra-condensed" => 50, "extra-condensed" => 62, "condensed" => 75, "semi-condensed" => 87, "normal" => 100, "semi-expanded" => 112, "expanded" => 125, "extra-expanded" => 150, "ultra-expanded" => 200, _ => lv.EndsWith("%") && Val.Num(lv[..^1], out double fsx) ? (int)fsx : 100 };
					break;
				}
				case "font-variant-caps": s.FontVariantCaps = (byte)(lv == "small-caps" ? 1 : lv == "all-small-caps" ? 2 : 0); break;
				case "line-height": {
					if (lv == "normal") { s.LineHeightNum = double.NaN; s.LineHeightPx = double.NaN; break; }
					if (Val.Num(lv, out double n)) { s.LineHeightNum = n; s.LineHeightPx = double.NaN; break; }
					var l = L();
					if (l != null && l.Value.IsValue) { s.LineHeightPx = l.Value.Resolve(s.FontSize); s.LineHeightNum = double.NaN; }
					break;
				}
				case "text-align": s.TextAlign = ParseTA(lv, p); break;
				case "text-align-last": s.TextAlignLast = lv == "auto" ? TA.Auto : ParseTA(lv, p); break;
				case "text-indent": { var l = L(); if (l != null) s.TextIndent = l.Value; break; }
				case "direction": break;
				case "white-space":
					s.Ws = lv switch { "nowrap" => WSp.NoWrap, "pre" => WSp.Pre, "pre-wrap" => WSp.PreWrap, "pre-line" => WSp.PreLine, "break-spaces" => WSp.BreakSpaces, _ => WSp.Normal };
					break;
				case "white-space-collapse":
					s.Ws = lv switch { "preserve" => s.Ws == WSp.NoWrap || s.Ws == WSp.Pre ? WSp.Pre : WSp.PreWrap, "preserve-breaks" => WSp.PreLine, "break-spaces" => WSp.BreakSpaces, _ => s.Ws == WSp.Pre ? WSp.NoWrap : s.Ws == WSp.PreWrap || s.Ws == WSp.PreLine || s.Ws == WSp.BreakSpaces ? WSp.Normal : s.Ws };
					break;
				case "text-wrap": case "text-wrap-mode":
					if (lv.StartsWith("nowrap")) s.Ws = s.Ws == WSp.PreWrap || s.Ws == WSp.Pre ? WSp.Pre : WSp.NoWrap;
					else if (lv.StartsWith("wrap")) s.Ws = s.Ws == WSp.Pre ? WSp.PreWrap : s.Ws == WSp.NoWrap ? WSp.Normal : s.Ws;
					break;
				case "word-spacing": { if (lv == "normal") s.WordSpacing = 0; else { var l = L(); if (l != null) s.WordSpacing = l.Value.Resolve(s.FontSize); } break; }
				case "letter-spacing": { if (lv == "normal") s.LetterSpacing = 0; else { var l = L(); if (l != null) s.LetterSpacing = l.Value.Resolve(s.FontSize); } break; }
				case "text-transform": s.TextTransform = (byte)(lv == "uppercase" ? 1 : lv == "lowercase" ? 2 : lv == "capitalize" ? 3 : lv == "full-width" ? 4 : 0); break;
				case "visibility": s.Visibility = (byte)(lv == "hidden" ? 1 : lv == "collapse" ? 2 : 0); break;
				case "list-style-type": s.ListStyleType = lv.StartsWith("\"") || lv.StartsWith("'") ? v : lv; break;
				case "list-style-position": s.ListInside = lv == "inside"; break;
				case "list-style-image": s.ListStyleImage = lv == "none" ? null : Css.ExtractUrl(v); break;
				case "word-break": s.WordBreak = (byte)(lv == "break-all" ? 1 : lv == "keep-all" ? 2 : lv == "break-word" ? 3 : 0); break;
				case "overflow-wrap": s.OverflowWrap = (byte)(lv == "break-word" ? 1 : lv == "anywhere" ? 2 : 0); break;
				case "line-break": s.LineBreakMode = (byte)(lv == "anywhere" ? 1 : lv == "strict" ? 2 : lv == "loose" ? 3 : 0); break;
				case "border-collapse": s.BorderCollapse = lv == "collapse"; break;
				case "border-spacing": {
					var parts = Css.SplitWs(v);
					var h = Val.ParseLen(parts[0], ctx);
					var vv = parts.Count > 1 ? Val.ParseLen(parts[1], ctx) : h;
					if (h != null && vv != null) { s.BorderSpacingH = h.Value.Resolve(0); s.BorderSpacingV = vv.Value.Resolve(0); }
					break;
				}
				case "caption-side": s.CaptionBottom = lv == "bottom"; break;
				case "empty-cells": s.EmptyCellsHide = lv == "hide"; break;
				case "text-shadow": s.TextShadow = lv == "none" ? null : ParseShadows(v, ctx, false); break;
				case "print-color-adjust": s.PrintExact = lv == "exact"; break;
				case "orphans": if (int.TryParse(lv, out int o) && o > 0) s.Orphans = o; break;
				case "widows": if (int.TryParse(lv, out int w) && w > 0) s.Widows = w; break;
				case "tab-size": { if (Val.Num(lv, out double ts)) s.TabSize = ts; else { var l = L(); if (l != null) s.TabSize = -l.Value.Resolve(0); } break; }
				case "font-kerning": s.KerningOff = lv == "none"; break;
				case "font-variant-ligatures": s.LigaturesOff = lv == "none" || lv.Contains("no-common-ligatures"); break;
				case "font-variant-numeric": s.TabularNums = lv.Contains("tabular-nums"); break;
				case "font-feature-settings": s.FontFeatures = lv == "normal" ? null : v; break;
				case "hyphens": s.Hyphens = (byte)(lv == "none" ? 0 : lv == "auto" ? 2 : 1); break;
				case "quotes": s.Quotes = lv == "auto" ? null : v; break;
				case "pointer-events": s.PointerEventsNone = lv == "none"; break;
				case "display": s.Display = ParseDisplay(lv); break;
				case "position": s.Position = lv switch { "relative" => Pos.Relative, "absolute" => Pos.Absolute, "fixed" => Pos.Fixed, "sticky" or "-webkit-sticky" => Pos.Sticky, _ => Pos.Static }; break;
				case "float": s.Float = (byte)(lv == "left" ? 1 : lv == "right" ? 2 : lv == "inline-start" ? (s.Rtl ? 2 : 1) : lv == "inline-end" ? (s.Rtl ? 1 : 2) : 0); break;
				case "clear": s.Clear = (byte)(lv == "left" ? 1 : lv == "right" ? 2 : lv == "both" ? 3 : lv == "inline-start" ? (s.Rtl ? 2 : 1) : lv == "inline-end" ? (s.Rtl ? 1 : 2) : 0); break;
				case "top": { var l = L(); if (l != null) s.Top = l.Value; break; }
				case "right": { var l = L(); if (l != null) s.Right = l.Value; break; }
				case "bottom": { var l = L(); if (l != null) s.Bottom = l.Value; break; }
				case "left": { var l = L(); if (l != null) s.Left = l.Value; break; }
				case "width": { var l = L(); if (l != null && !l.Value.IsNone) s.Width = l.Value; break; }
				case "height": { var l = L(); if (l != null && !l.Value.IsNone) s.Height = l.Value; break; }
				case "min-width": { var l = L(); if (l != null) s.MinWidth = l.Value; break; }
				case "min-height": { var l = L(); if (l != null) s.MinHeight = l.Value; break; }
				case "max-width": { var l = L(); if (l != null) s.MaxWidth = l.Value; break; }
				case "max-height": { var l = L(); if (l != null) s.MaxHeight = l.Value; break; }
				case "margin-top": case "margin-right": case "margin-bottom": case "margin-left": { var l = L(); if (l != null) s.Margin[Side(prop)] = l.Value; break; }
				case "padding-top": case "padding-right": case "padding-bottom": case "padding-left": { var l = L(); if (l != null && !l.Value.IsAuto) s.Padding[Side(prop)] = l.Value; break; }
				case "border-top-width": case "border-right-width": case "border-bottom-width": case "border-left-width": {
					double bw = lv switch { "thin" => 1, "medium" => 3, "thick" => 5, _ => L()?.Resolve(0) ?? -1 };
					if (bw >= 0) s.BorderWidth[Side(prop)] = bw > 0 && bw < 1 ? 1 : Math.Floor(bw);
					break;
				}
				case "border-top-style": case "border-right-style": case "border-bottom-style": case "border-left-style": s.BorderStyle[Side(prop)] = ParseBS(lv); break;
				case "border-top-color": case "border-right-color": case "border-bottom-color": case "border-left-color": { var c = Css.ParseColor(v); if (c != null) s.BorderColor[Side(prop)] = c.Value; break; }
				case "border-top-left-radius": case "border-top-right-radius": case "border-bottom-right-radius": case "border-bottom-left-radius": {
					int i = prop switch { "border-top-left-radius" => 0, "border-top-right-radius" => 1, "border-bottom-right-radius" => 2, _ => 3 };
					var parts = Css.SplitWs(v);
					var h = Val.ParseLen(parts[0], ctx);
					var vv = parts.Count > 1 ? Val.ParseLen(parts[1], ctx) : h;
					if (h != null && vv != null) { s.RadiusH[i] = h.Value; s.RadiusV[i] = vv.Value; }
					break;
				}
				case "background-color": { var c = Css.ParseColor(v); if (c != null) s.BackgroundColor = c.Value; break; }
				case "background-image": {
					if (lv == "none") { s.Backgrounds = null; break; }
					var imgs = Css.SplitTopLevel(v, ',');
					var layers = new List<BgLayer>();
					foreach (var im in imgs) {
						var layer = new BgLayer();
						string t = im.Trim();
						if (t.StartsWith("url(", StringComparison.OrdinalIgnoreCase)) layer.Url = Css.ExtractUrl(t);
						else if (t.Contains("gradient(")) layer.Grad = ParseGradient(t, ctx);
						else if (t.StartsWith("image-set(", StringComparison.OrdinalIgnoreCase) || t.StartsWith("-webkit-image-set(", StringComparison.OrdinalIgnoreCase)) {
							int u = t.IndexOf("url(", StringComparison.OrdinalIgnoreCase);
							if (u >= 0) { int e = t.IndexOf(')', u); layer.Url = Css.ExtractUrl(t.Substring(u, e - u + 1)); }
							else { int q = t.IndexOfAny(new[] { '"', '\'' }); if (q >= 0) { int e = t.IndexOf(t[q], q + 1); layer.Url = t.Substring(q + 1, e - q - 1); } }
						}
						layers.Add(layer);
					}
					s.Backgrounds = layers;
					break;
				}
				case "background-repeat": ForLayers(s, v, (l, t) => {
					var parts = t.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
					byte R(string x) => (byte)(x == "no-repeat" ? 1 : x == "space" ? 2 : x == "round" ? 3 : 0);
					if (parts.Length == 1) {
						if (parts[0] == "repeat-x") { l.RepX = 0; l.RepY = 1; }
						else if (parts[0] == "repeat-y") { l.RepX = 1; l.RepY = 0; }
						else l.RepX = l.RepY = R(parts[0]);
					}
					else if (parts.Length >= 2) { l.RepX = R(parts[0]); l.RepY = R(parts[1]); }
				}); break;
				case "background-position": ForLayers(s, v, (l, t) => { ParsePosition(t, ctx, out l.PosX, out l.PosY); }); break;
				case "background-position-x": ForLayers(s, v, (l, t) => { ParsePosition(t + " top", ctx, out l.PosX, out _); }); break;
				case "background-position-y": ForLayers(s, v, (l, t) => { ParsePosition("left " + t, ctx, out _, out l.PosY); }); break;
				case "background-size": ForLayers(s, v, (l, t) => {
					string x = t.Trim().ToLowerInvariant();
					if (x == "cover") l.SizeKind = 1;
					else if (x == "contain") l.SizeKind = 2;
					else {
						l.SizeKind = 0;
						var parts = Css.SplitWs(x);
						l.SizeW = Val.ParseLen(parts[0], ctx) ?? Len.Auto;
						l.SizeH = parts.Count > 1 ? Val.ParseLen(parts[1], ctx) ?? Len.Auto : Len.Auto;
					}
				}); break;
				case "background-origin": ForLayers(s, v, (l, t) => l.Origin = BoxKw(t)); break;
				case "background-clip": ForLayers(s, v, (l, t) => l.Clip = t.Trim().ToLowerInvariant() == "text" ? (byte)3 : BoxKw(t) switch { 0 => (byte)1, 1 => (byte)0, _ => (byte)2 }); break;
				case "box-sizing": s.BorderBox = lv == "border-box"; break;
				case "overflow-x": s.OverflowX = ParseOverflow(lv); break;
				case "overflow-y": s.OverflowY = ParseOverflow(lv); break;
				case "z-index": s.ZIndex = lv == "auto" ? null : int.TryParse(lv, out int zi) ? zi : null; break;
				case "opacity": { if (lv.EndsWith("%") && Val.Num(lv[..^1], out double op)) s.Opacity = Math.Clamp(op / 100, 0, 1); else if (Val.Num(lv, out op)) s.Opacity = Math.Clamp(op, 0, 1); break; }
				case "vertical-align": {
					byte va = lv switch { "baseline" => Style.VaBaseline, "sub" => Style.VaSub, "super" => Style.VaSuper, "top" => Style.VaTop, "text-top" => Style.VaTextTop, "middle" => Style.VaMiddle, "bottom" => Style.VaBottom, "text-bottom" => Style.VaTextBottom, _ => 255 };
					if (va != 255) s.VAlign = va;
					else { var l = L(); if (l != null && l.Value.IsValue) { s.VAlign = Style.VaLength; s.VAlignLen = l.Value; } }
					break;
				}
				case "text-decoration-line": {
					byte d = 0;
					foreach (var t in lv.Split(' ', StringSplitOptions.RemoveEmptyEntries)) d |= t switch { "underline" => (byte)1, "overline" => (byte)2, "line-through" => (byte)4, "blink" => (byte)0, _ => (byte)0 };
					s.TextDecoLine = d;
					break;
				}
				case "text-decoration-color": { var c = Css.ParseColor(v); if (c != null) s.TextDecoColor = c.Value; break; }
				case "text-decoration-style": s.TextDecoStyle = (byte)(lv == "double" ? 1 : lv == "dotted" ? 2 : lv == "dashed" ? 3 : lv == "wavy" ? 4 : 0); break;
				case "text-decoration-thickness": { if (lv == "auto" || lv == "from-font") s.TextDecoThickness = Len.Auto; else { var l = L(); if (l != null) s.TextDecoThickness = l.Value; } break; }
				case "text-underline-offset": { if (lv == "auto") s.UnderlineOffset = Len.Auto; else { var l = L(); if (l != null) s.UnderlineOffset = l.Value; } break; }
				case "content": s.Content = v; break;
				case "counter-reset": s.CounterReset = lv == "none" ? null : v; break;
				case "counter-increment": s.CounterIncrement = lv == "none" ? null : v; break;
				case "counter-set": s.CounterSet = lv == "none" ? null : v; break;
				case "flex-direction": s.FlexDirection = (byte)(lv == "row-reverse" ? 1 : lv == "column" ? 2 : lv == "column-reverse" ? 3 : 0); break;
				case "flex-wrap": s.FlexWrap = (byte)(lv == "wrap" ? 1 : lv == "wrap-reverse" ? 2 : 0); break;
				case "flex-grow": if (Val.Num(lv, out double fg) && fg >= 0) s.FlexGrow = fg; break;
				case "flex-shrink": if (Val.Num(lv, out double fsh) && fsh >= 0) s.FlexShrink = fsh; break;
				case "flex-basis": { if (lv == "content") s.FlexBasis = new Len { K = Len.KMax }; else { var l = L(); if (l != null) s.FlexBasis = l.Value; } break; }
				case "justify-content": s.JustifyContent = ParseAlign(lv); break;
				case "align-items": s.AlignItems = ParseAlign(lv); break;
				case "align-self": s.AlignSelf = lv == "auto" ? Style.AlAuto : ParseAlign(lv); break;
				case "align-content": s.AlignContent = ParseAlign(lv); break;
				case "justify-items": s.JustifyItems = ParseAlign(lv); break;
				case "justify-self": s.JustifySelf = lv == "auto" ? Style.AlAuto : ParseAlign(lv); break;
				case "order": if (int.TryParse(lv, out int ord)) s.Order = ord; break;
				case "row-gap": { var l = L(); if (l != null) s.RowGap = l.Value; break; }
				case "column-gap": { var l = L(); if (l != null) s.ColumnGap = l.Value; break; }
				case "grid-template-columns": s.GridTemplateColumns = lv == "none" ? null : v; break;
				case "grid-template-rows": s.GridTemplateRows = lv == "none" ? null : v; break;
				case "grid-template-areas": s.GridTemplateAreas = lv == "none" ? null : v; break;
				case "grid-auto-columns": s.GridAutoColumns = v; break;
				case "grid-auto-rows": s.GridAutoRows = v; break;
				case "grid-auto-flow": s.GridAutoFlow = (byte)((lv.Contains("column") ? 1 : 0) | (lv.Contains("dense") ? 2 : 0)); break;
				case "grid-column-start": s.GridColumnStart = v; break;
				case "grid-column-end": s.GridColumnEnd = v; break;
				case "grid-row-start": s.GridRowStart = v; break;
				case "grid-row-end": s.GridRowEnd = v; break;
				case "transform": s.Transform = lv == "none" ? null : v; break;
				case "translate": case "rotate": case "scale": if (lv != "none") s.Transform = (s.Transform ?? "") + " " + prop + "(" + v.Replace(' ', ',') + ")"; break;
				case "transform-origin": { ParsePosition(v, ctx, out s.TransformOriginX, out s.TransformOriginY); break; }
				case "box-shadow": s.BoxShadow = lv == "none" ? null : ParseShadows(v, ctx, true); break;
				case "break-before": s.BreakBefore = ParseBreak(lv); break;
				case "break-after": s.BreakAfter = ParseBreak(lv); break;
				case "break-inside": s.BreakInside = (byte)(lv == "avoid" || lv == "avoid-page" ? Style.BrAvoid : Style.BrAuto); break;
				case "table-layout": s.TableFixed = lv == "fixed"; break;
				case "unicode-bidi": s.UnicodeBidi = (byte)(lv == "embed" ? 1 : lv == "isolate" || lv == "-webkit-isolate" ? 2 : lv == "bidi-override" ? 3 : lv == "isolate-override" || lv == "-webkit-isolate-override" ? 4 : lv == "plaintext" || lv == "-webkit-plaintext" ? 5 : 0); break;
				case "object-fit": s.ObjectFit = (byte)(lv == "contain" ? 1 : lv == "cover" ? 2 : lv == "none" ? 3 : lv == "scale-down" ? 4 : 0); break;
				case "object-position": ParsePosition(v, ctx, out s.ObjectPosX, out s.ObjectPosY); break;
				case "outline-width": { double ow = lv switch { "thin" => 1, "medium" => 3, "thick" => 5, _ => L()?.Resolve(0) ?? -1 }; if (ow >= 0) s.OutlineWidth = ow; break; }
				case "outline-style": s.OutlineStyle = lv == "auto" ? BS.Solid : ParseBS(lv); break;
				case "outline-color": { var c = Css.ParseColor(v); if (c != null) s.OutlineColor = c.Value; break; }
				case "outline-offset": { var l = L(); if (l != null) s.OutlineOffset = l.Value.Resolve(0); break; }
				case "aspect-ratio": {
					string x = lv.Replace("auto", "").Trim();
					if (x.Length == 0) { s.AspectRatio = 0; break; }
					var parts = x.Split('/');
					if (parts.Length == 2 && Val.Num(parts[0].Trim(), out double a1) && Val.Num(parts[1].Trim(), out double a2) && a2 > 0) s.AspectRatio = a1 / a2;
					else if (Val.Num(x, out double a3)) s.AspectRatio = a3;
					break;
				}
				case "box-decoration-break": s.BoxDecorationBreak = (byte)(lv == "clone" ? 1 : 0); break;
				case "clip-path": s.ClipPath = lv == "none" ? null : v; break;
				case "column-count": s.ColumnCount = int.TryParse(lv, out int cc) ? cc : 0; break;
				case "page": s.Page = lv == "auto" ? null : v; break;
			}
		}

		static byte BoxKw(string t) => t.Trim().ToLowerInvariant() switch { "border-box" => 1, "content-box" => 2, _ => 0 };

		static void ForLayers(Style s, string v, Action<BgLayer, string> f) {
			var parts = Css.SplitTopLevel(v, ',');
			s.Backgrounds ??= new List<BgLayer> { new BgLayer() };
			var list = new List<BgLayer>();
			for (int i = 0; i < s.Backgrounds.Count; i++) {
				var l = s.Backgrounds[i].Clone();
				f(l, parts[i % parts.Count]);
				list.Add(l);
			}
			s.Backgrounds = list;
		}

		static byte ParseOverflow(string lv) => lv switch { "hidden" => 2, "clip" => 2, "scroll" => 3, "auto" => 4, "overlay" => 4, _ => 0 };

		static byte ParseBreak(string lv) => lv switch { "avoid" => Style.BrAvoid, "avoid-page" => Style.BrAvoid, "page" or "always" or "left" or "right" or "recto" or "verso" => Style.BrPage, "column" => Style.BrColumn, _ => Style.BrAuto };

		static byte ParseAlign(string lv) {
			lv = lv.Replace("safe ", "").Replace("unsafe ", "").Replace("legacy ", "").Trim();
			return lv switch {
				"normal" => Style.AlNormal, "stretch" => Style.AlStretch, "start" => Style.AlStart, "end" => Style.AlEnd, "center" => Style.AlCenter, "baseline" or "first baseline" => Style.AlBaseline,
				"last baseline" => Style.AlLastBaseline, "flex-start" => Style.AlFlexStart, "flex-end" => Style.AlFlexEnd, "space-between" => Style.AlSpaceBetween, "space-around" => Style.AlSpaceAround,
				"space-evenly" => Style.AlSpaceEvenly, "left" => Style.AlLeft, "right" => Style.AlRight, "self-start" => Style.AlSelfStart, "self-end" => Style.AlSelfEnd, "auto" => Style.AlAuto, _ => Style.AlNormal
			};
		}

		static TA ParseTA(string lv, Style p) => lv switch {
			"left" => TA.Left, "right" => TA.Right, "center" => TA.Center, "justify" => TA.Justify, "end" => TA.End, "start" => TA.Start,
			"-webkit-center" or "-moz-center" or "-internal-center" => TA.WebkitCenter, "-webkit-left" or "-moz-left" => TA.WebkitLeft, "-webkit-right" or "-moz-right" => TA.WebkitRight,
			"match-parent" => p == null ? TA.Start : p.TextAlign == TA.Start ? (p.Rtl ? TA.Right : TA.Left) : p.TextAlign == TA.End ? (p.Rtl ? TA.Left : TA.Right) : p.TextAlign,
			"-webkit-auto" or "auto" => TA.Start, _ => TA.Start
		};

		static BS ParseBS(string lv) => lv switch {
			"hidden" => BS.Hidden, "dotted" => BS.Dotted, "dashed" => BS.Dashed, "solid" => BS.Solid, "double" => BS.Double, "groove" => BS.Groove, "ridge" => BS.Ridge, "inset" => BS.Inset, "outset" => BS.Outset, _ => BS.None
		};

		static Disp ParseDisplay(string lv) {
			switch (lv) {
				case "none": return Disp.None;
				case "block": case "block flow": case "flow": return Disp.Block;
				case "inline": case "inline flow": case "ruby": case "ruby-text": case "ruby-base": return Disp.Inline;
				case "inline-block": case "inline flow-root": return Disp.InlineBlock;
				case "list-item": case "block list-item": case "list-item block": case "flow list-item": return Disp.ListItem;
				case "table": case "block table": return Disp.Table;
				case "inline-table": case "inline table": return Disp.InlineTable;
				case "table-row-group": return Disp.RowGroup;
				case "table-header-group": return Disp.HeaderGroup;
				case "table-footer-group": return Disp.FooterGroup;
				case "table-row": return Disp.Row;
				case "table-cell": return Disp.Cell;
				case "table-column": return Disp.Column;
				case "table-column-group": return Disp.ColumnGroup;
				case "table-caption": return Disp.Caption;
				case "flex": case "block flex": case "-webkit-flex": case "-webkit-box": case "-ms-flexbox": return Disp.Flex;
				case "inline-flex": case "inline flex": case "-webkit-inline-flex": case "-webkit-inline-box": return Disp.InlineFlex;
				case "grid": case "block grid": case "-ms-grid": return Disp.Grid;
				case "inline-grid": case "inline grid": return Disp.InlineGrid;
				case "contents": return Disp.Contents;
				case "flow-root": case "block flow-root": return Disp.FlowRoot;
				default: return lv.Contains("list-item") ? Disp.ListItem : Disp.Inline;
			}
		}

		public static int Side(string prop) => prop.Contains("top") ? 0 : prop.Contains("right") ? 1 : prop.Contains("bottom") ? 2 : 3;

		public static void ParsePosition(string v, LenCtx ctx, out Len x, out Len y) {
			var parts = Css.SplitWs(v.Trim().ToLowerInvariant()).Where(t => t != ",").ToList();
			x = Len.PctV(50); y = Len.PctV(50);
			if (parts.Count == 0) return;
			Len? K(string t, bool horiz) => t switch { "left" => horiz ? Len.PctV(0) : null, "right" => horiz ? Len.PctV(100) : null, "top" => horiz ? null : Len.PctV(0), "bottom" => horiz ? null : Len.PctV(100), "center" => Len.PctV(50), _ => Val.ParseLen(t, ctx) };
			if (parts.Count == 1) {
				string t = parts[0];
				if (t == "top" || t == "bottom") { y = K(t, false).Value; x = Len.PctV(50); }
				else { x = K(t, true) ?? Len.PctV(50); y = Len.PctV(50); }
				return;
			}
			if (parts.Count == 2) {
				string a = parts[0], b = parts[1];
				if (a == "top" || a == "bottom" || b == "left" || b == "right") (a, b) = (b, a);
				x = K(a, true) ?? Len.PctV(50);
				y = K(b, false) ?? Len.PctV(50);
				return;
			}
			Len hx = Len.PctV(50), vy = Len.PctV(50);
			for (int i = 0; i < parts.Count; i++) {
				string t = parts[i];
				Len? off = i + 1 < parts.Count ? Val.ParseLen(parts[i + 1], ctx) : null;
				if (t == "left" || t == "right") {
					if (off != null && off.Value.IsValue) { hx = t == "left" ? off.Value : new Len { K = Len.Value, Px = -off.Value.Px, Pct = 100 - off.Value.Pct, HasPct = true }; i++; }
					else hx = K(t, true).Value;
				}
				else if (t == "top" || t == "bottom") {
					if (off != null && off.Value.IsValue) { vy = t == "top" ? off.Value : new Len { K = Len.Value, Px = -off.Value.Px, Pct = 100 - off.Value.Pct, HasPct = true }; i++; }
					else vy = K(t, false).Value;
				}
			}
			x = hx; y = vy;
		}

		static List<Shadow> ParseShadows(string v, LenCtx ctx, bool box) {
			var list = new List<Shadow>();
			foreach (var part in Css.SplitTopLevel(v, ',')) {
				var sh = new Shadow { Color = Rgba.CurrentColor };
				var lens = new List<double>();
				foreach (var t in Css.SplitWs(part.Trim())) {
					if (t.Equals("inset", StringComparison.OrdinalIgnoreCase)) { sh.Inset = true; continue; }
					var c = Css.ParseColor(t);
					if (c != null) { sh.Color = c.Value; continue; }
					var l = Val.ParseLen(t, ctx);
					if (l != null) lens.Add(l.Value.Resolve(0));
				}
				if (lens.Count < 2) continue;
				sh.X = lens[0]; sh.Y = lens[1];
				if (lens.Count > 2) sh.Blur = Math.Max(0, lens[2]);
				if (lens.Count > 3 && box) sh.Spread = lens[3];
				list.Add(sh);
			}
			return list.Count > 0 ? list : null;
		}

		public static Gradient ParseGradient(string t, LenCtx ctx) {
			t = t.Trim();
			string lt = t.ToLowerInvariant();
			int p = lt.IndexOf('(');
			string fn = lt.Substring(0, p).Replace("-webkit-", "").Replace("-moz-", "");
			string inner = t.Substring(p + 1, t.LastIndexOf(')') - p - 1);
			var g = new Gradient { Repeating = fn.StartsWith("repeating-") };
			if (fn.EndsWith("linear-gradient")) g.Type = 0;
			else if (fn.EndsWith("radial-gradient")) g.Type = 1;
			else if (fn.EndsWith("conic-gradient")) g.Type = 2;
			else return null;
			var args = Css.SplitTopLevel(inner, ',').Select(x => x.Trim()).ToList();
			int start = 0;
			string first = args[0].ToLowerInvariant();
			bool legacy = lt.StartsWith("-webkit-") || lt.StartsWith("-moz-");
			if (g.Type == 0) {
				if (first.StartsWith("to ")) {
					var w = first.Substring(3).Split(' ', StringSplitOptions.RemoveEmptyEntries);
					int cx = 0, cy = 0;
					foreach (var x in w) { if (x == "left") cx = -1; else if (x == "right") cx = 1; else if (x == "top") cy = -1; else if (x == "bottom") cy = 1; }
					if (cx != 0 && cy != 0) { g.ToCorner = true; g.CornerX = cx; g.CornerY = cy; }
					else g.Angle = cx == 1 ? 90 : cx == -1 ? 270 : cy == -1 ? 0 : 180;
					start = 1;
				}
				else {
					double a = Val.ParseAngle(first);
					if (!double.IsNaN(a)) { g.Angle = legacy ? 90 - a : a; start = 1; }
					else if (legacy && (first == "top" || first == "left" || first == "right" || first == "bottom")) { g.Angle = first switch { "top" => 180, "bottom" => 0, "left" => 90, _ => 270 }; start = 1; }
				}
			}
			else if (g.Type == 1) {
				bool isStop = Css.ParseColor(Css.SplitWs(first)[0]) != null;
				if (!isStop) {
					start = 1;
					string shape = first;
					string pos = null;
					int at = first.IndexOf("at ", StringComparison.Ordinal);
					if (at >= 0) { pos = first.Substring(at + 3); shape = first.Substring(0, at); }
					var words = Css.SplitWs(shape);
					var lens = new List<Len>();
					foreach (var w in words) {
						switch (w) {
							case "circle": g.Circle = true; break;
							case "ellipse": break;
							case "closest-side": g.SizeKw = 0; break;
							case "closest-corner": g.SizeKw = 1; break;
							case "farthest-side": g.SizeKw = 2; break;
							case "farthest-corner": g.SizeKw = 3; break;
							default: { var l = Val.ParseLen(w, ctx); if (l != null) lens.Add(l.Value); break; }
						}
					}
					if (lens.Count > 0) { g.ExplicitSize = true; g.RX = lens[0]; g.RY = lens.Count > 1 ? lens[1] : lens[0]; if (lens.Count == 1) g.Circle = true; }
					if (pos != null) ParsePosition(pos, ctx, out g.PX, out g.PY);
				}
			}
			else {
				if (first.StartsWith("from ") || first.StartsWith("at ")) {
					start = 1;
					string f = first;
					int at = f.IndexOf("at ", StringComparison.Ordinal);
					if (at >= 0) { ParsePosition(f.Substring(at + 3), ctx, out g.PX, out g.PY); f = f.Substring(0, at); }
					if (f.StartsWith("from ")) { double a = Val.ParseAngle(f.Substring(5).Trim()); if (!double.IsNaN(a)) g.FromAngle = a; }
				}
			}
			for (int i = start; i < args.Count; i++) {
				var parts = Css.SplitWs(args[i]);
				if (parts.Count == 0) continue;
				var c = Css.ParseColor(parts[0]);
				if (c == null) {
					var hint = g.Type == 2 ? AngleLen(parts[0]) : Val.ParseLen(parts[0], ctx);
					if (hint != null) g.Stops.Add(new GradientStop { Hint = true, Pos = hint });
					continue;
				}
				if (parts.Count == 1) g.Stops.Add(new GradientStop { Color = c.Value });
				for (int k = 1; k < parts.Count; k++) {
					var l = g.Type == 2 ? AngleLen(parts[k]) : Val.ParseLen(parts[k], ctx);
					g.Stops.Add(new GradientStop { Color = c.Value, Pos = l });
				}
			}
			if (g.Stops.Count(s => !s.Hint) == 0) return null;
			if (g.Stops.Count(s => !s.Hint) == 1) g.Stops.Add(new GradientStop { Color = g.Stops.First(s => !s.Hint).Color });
			return g;
		}

		static Len? AngleLen(string t) {
			if (t.EndsWith("%")) return Val.ParseLen(t, default);
			double a = Val.ParseAngle(t);
			if (double.IsNaN(a)) return null;
			return Len.PctV(a / 360 * 100);
		}

		public static bool IsInherited(string p) {
			switch (p) {
				case "color": case "font-family": case "font-size": case "font-weight": case "font-style": case "font-stretch": case "line-height": case "text-align": case "text-align-last":
				case "text-indent": case "direction": case "white-space": case "word-spacing": case "letter-spacing": case "text-transform": case "visibility": case "list-style-type":
				case "list-style-position": case "list-style-image": case "word-break": case "overflow-wrap": case "line-break": case "border-collapse": case "border-spacing": case "caption-side":
				case "empty-cells": case "text-shadow": case "print-color-adjust": case "orphans": case "widows": case "tab-size": case "font-kerning": case "font-variant-ligatures":
				case "font-feature-settings": case "hyphens": case "quotes": case "font-variant-numeric": case "font-variant-caps": case "pointer-events": case "white-space-collapse": case "text-wrap-mode": case "text-wrap":
					return true;
			}
			return false;
		}

		static void CopyProp(Style d, Style s, string p) {
			switch (p) {
				case "color": d.Color = s.Color; break;
				case "font-family": d.FontFamily = s.FontFamily; break;
				case "font-size": d.FontSize = s.FontSize; d.FontSizeKw = s.FontSizeKw; break;
				case "font-weight": d.FontWeight = s.FontWeight; break;
				case "font-style": d.FontStyle = s.FontStyle; break;
				case "font-stretch": d.FontStretch = s.FontStretch; break;
				case "line-height": d.LineHeightNum = s.LineHeightNum; d.LineHeightPx = s.LineHeightPx; break;
				case "text-align": d.TextAlign = s.TextAlign; break;
				case "text-align-last": d.TextAlignLast = s.TextAlignLast; break;
				case "text-indent": d.TextIndent = s.TextIndent; break;
				case "white-space": case "white-space-collapse": case "text-wrap-mode": case "text-wrap": d.Ws = s.Ws; break;
				case "word-spacing": d.WordSpacing = s.WordSpacing; break;
				case "letter-spacing": d.LetterSpacing = s.LetterSpacing; break;
				case "text-transform": d.TextTransform = s.TextTransform; break;
				case "visibility": d.Visibility = s.Visibility; break;
				case "list-style-type": d.ListStyleType = s.ListStyleType; break;
				case "list-style-position": d.ListInside = s.ListInside; break;
				case "list-style-image": d.ListStyleImage = s.ListStyleImage; break;
				case "word-break": d.WordBreak = s.WordBreak; break;
				case "overflow-wrap": d.OverflowWrap = s.OverflowWrap; break;
				case "line-break": d.LineBreakMode = s.LineBreakMode; break;
				case "border-collapse": d.BorderCollapse = s.BorderCollapse; break;
				case "border-spacing": d.BorderSpacingH = s.BorderSpacingH; d.BorderSpacingV = s.BorderSpacingV; break;
				case "caption-side": d.CaptionBottom = s.CaptionBottom; break;
				case "empty-cells": d.EmptyCellsHide = s.EmptyCellsHide; break;
				case "text-shadow": d.TextShadow = s.TextShadow; break;
				case "print-color-adjust": d.PrintExact = s.PrintExact; break;
				case "orphans": d.Orphans = s.Orphans; break;
				case "widows": d.Widows = s.Widows; break;
				case "tab-size": d.TabSize = s.TabSize; break;
				case "font-kerning": d.KerningOff = s.KerningOff; break;
				case "font-variant-ligatures": d.LigaturesOff = s.LigaturesOff; break;
				case "font-feature-settings": d.FontFeatures = s.FontFeatures; break;
				case "hyphens": d.Hyphens = s.Hyphens; break;
				case "quotes": d.Quotes = s.Quotes; break;
				case "font-variant-numeric": d.TabularNums = s.TabularNums; break;
				case "font-variant-caps": d.FontVariantCaps = s.FontVariantCaps; break;
				case "display": d.Display = s.Display; break;
				case "position": d.Position = s.Position; break;
				case "float": d.Float = s.Float; break;
				case "clear": d.Clear = s.Clear; break;
				case "top": d.Top = s.Top; break;
				case "right": d.Right = s.Right; break;
				case "bottom": d.Bottom = s.Bottom; break;
				case "left": d.Left = s.Left; break;
				case "width": d.Width = s.Width; break;
				case "height": d.Height = s.Height; break;
				case "min-width": d.MinWidth = s.MinWidth; break;
				case "min-height": d.MinHeight = s.MinHeight; break;
				case "max-width": d.MaxWidth = s.MaxWidth; break;
				case "max-height": d.MaxHeight = s.MaxHeight; break;
				case "margin-top": case "margin-right": case "margin-bottom": case "margin-left": d.Margin[Side(p)] = s.Margin[Side(p)]; break;
				case "padding-top": case "padding-right": case "padding-bottom": case "padding-left": d.Padding[Side(p)] = s.Padding[Side(p)]; break;
				case "border-top-width": case "border-right-width": case "border-bottom-width": case "border-left-width": d.BorderWidth[Side(p)] = s.BorderWidth[Side(p)]; break;
				case "border-top-style": case "border-right-style": case "border-bottom-style": case "border-left-style": d.BorderStyle[Side(p)] = s.BorderStyle[Side(p)]; break;
				case "border-top-color": case "border-right-color": case "border-bottom-color": case "border-left-color": d.BorderColor[Side(p)] = s.BorderColor[Side(p)]; break;
				case "border-top-left-radius": d.RadiusH[0] = s.RadiusH[0]; d.RadiusV[0] = s.RadiusV[0]; break;
				case "border-top-right-radius": d.RadiusH[1] = s.RadiusH[1]; d.RadiusV[1] = s.RadiusV[1]; break;
				case "border-bottom-right-radius": d.RadiusH[2] = s.RadiusH[2]; d.RadiusV[2] = s.RadiusV[2]; break;
				case "border-bottom-left-radius": d.RadiusH[3] = s.RadiusH[3]; d.RadiusV[3] = s.RadiusV[3]; break;
				case "background-color": d.BackgroundColor = s.BackgroundColor; break;
				case "background-image": d.Backgrounds = s.Backgrounds; break;
				case "box-sizing": d.BorderBox = s.BorderBox; break;
				case "overflow-x": d.OverflowX = s.OverflowX; break;
				case "overflow-y": d.OverflowY = s.OverflowY; break;
				case "z-index": d.ZIndex = s.ZIndex; break;
				case "opacity": d.Opacity = s.Opacity; break;
				case "vertical-align": d.VAlign = s.VAlign; d.VAlignLen = s.VAlignLen; break;
				case "text-decoration-line": d.TextDecoLine = s.TextDecoLine; break;
				case "text-decoration-color": d.TextDecoColor = s.TextDecoColor; break;
				case "text-decoration-style": d.TextDecoStyle = s.TextDecoStyle; break;
				case "content": d.Content = s.Content; break;
				case "flex-direction": d.FlexDirection = s.FlexDirection; break;
				case "flex-wrap": d.FlexWrap = s.FlexWrap; break;
				case "flex-grow": d.FlexGrow = s.FlexGrow; break;
				case "flex-shrink": d.FlexShrink = s.FlexShrink; break;
				case "flex-basis": d.FlexBasis = s.FlexBasis; break;
				case "justify-content": d.JustifyContent = s.JustifyContent; break;
				case "align-items": d.AlignItems = s.AlignItems; break;
				case "align-self": d.AlignSelf = s.AlignSelf; break;
				case "align-content": d.AlignContent = s.AlignContent; break;
				case "order": d.Order = s.Order; break;
				case "row-gap": d.RowGap = s.RowGap; break;
				case "column-gap": d.ColumnGap = s.ColumnGap; break;
				case "transform": d.Transform = s.Transform; break;
				case "box-shadow": d.BoxShadow = s.BoxShadow; break;
				case "break-before": d.BreakBefore = s.BreakBefore; break;
				case "break-after": d.BreakAfter = s.BreakAfter; break;
				case "break-inside": d.BreakInside = s.BreakInside; break;
				case "table-layout": d.TableFixed = s.TableFixed; break;
				case "unicode-bidi": d.UnicodeBidi = s.UnicodeBidi; break;
			}
		}
	}

	internal static class Shorthands {
		static readonly string[] Sides = { "top", "right", "bottom", "left" };

		public static IEnumerable<(string, string)> Expand(string p, string v, bool rtl) {
			string lv = v.Trim().ToLowerInvariant();
			bool global = lv == "inherit" || lv == "initial" || lv == "unset" || lv == "revert" || lv == "revert-layer";
			string Start() => rtl ? "right" : "left";
			string End() => rtl ? "left" : "right";
			switch (p) {
				case "margin": case "padding": {
					if (global) { foreach (var s in Sides) yield return (p + "-" + s, v); yield break; }
					var t = Box4(v);
					for (int i = 0; i < 4; i++) yield return (p + "-" + Sides[i], t[i]);
					yield break;
				}
				case "inset": {
					var t = global ? new[] { v, v, v, v } : Box4(v);
					for (int i = 0; i < 4; i++) yield return (Sides[i], t[i]);
					yield break;
				}
				case "margin-inline": case "padding-inline": case "inset-inline": {
					string b = p.Substring(0, p.IndexOf('-'));
					var parts = global ? new List<string> { v } : Css.SplitWs(v);
					string pre = b == "inset" ? "" : b + "-";
					yield return (pre + Start(), parts[0]);
					yield return (pre + End(), parts.Count > 1 ? parts[1] : parts[0]);
					yield break;
				}
				case "margin-block": case "padding-block": case "inset-block": {
					string b = p.Substring(0, p.IndexOf('-'));
					var parts = global ? new List<string> { v } : Css.SplitWs(v);
					string pre = b == "inset" ? "" : b + "-";
					yield return (pre + "top", parts[0]);
					yield return (pre + "bottom", parts.Count > 1 ? parts[1] : parts[0]);
					yield break;
				}
				case "margin-inline-start": yield return ("margin-" + Start(), v); yield break;
				case "margin-inline-end": yield return ("margin-" + End(), v); yield break;
				case "margin-block-start": yield return ("margin-top", v); yield break;
				case "margin-block-end": yield return ("margin-bottom", v); yield break;
				case "padding-inline-start": yield return ("padding-" + Start(), v); yield break;
				case "padding-inline-end": yield return ("padding-" + End(), v); yield break;
				case "padding-block-start": yield return ("padding-top", v); yield break;
				case "padding-block-end": yield return ("padding-bottom", v); yield break;
				case "inset-inline-start": yield return (Start(), v); yield break;
				case "inset-inline-end": yield return (End(), v); yield break;
				case "inset-block-start": yield return ("top", v); yield break;
				case "inset-block-end": yield return ("bottom", v); yield break;
				case "inline-size": yield return ("width", v); yield break;
				case "block-size": yield return ("height", v); yield break;
				case "min-inline-size": yield return ("min-width", v); yield break;
				case "min-block-size": yield return ("min-height", v); yield break;
				case "max-inline-size": yield return ("max-width", v); yield break;
				case "max-block-size": yield return ("max-height", v); yield break;
				case "border-width": case "border-style": case "border-color": {
					string kind = p.Substring(7);
					var t = global ? new[] { v, v, v, v } : Box4(v);
					for (int i = 0; i < 4; i++) yield return ("border-" + Sides[i] + "-" + kind, t[i]);
					yield break;
				}
				case "border": case "border-top": case "border-right": case "border-bottom": case "border-left":
				case "border-inline": case "border-block": case "border-inline-start": case "border-inline-end": case "border-block-start": case "border-block-end": case "outline": {
					string w = "medium", s = "none", c = "currentcolor";
					if (global) { w = s = c = v; }
					else {
						foreach (var t in Css.SplitWs(v)) {
							string lt = t.ToLowerInvariant();
							if (IsBorderStyle(lt)) s = lt;
							else if (lt == "thin" || lt == "medium" || lt == "thick" || Val.ParseLen(lt, new LenCtx { Em = 16, Rem = 16 }) != null && !lt.StartsWith("#")) w = t;
							else c = t;
						}
					}
					IEnumerable<string> sides = p switch {
						"border" => Sides, "border-inline" => new[] { "left", "right" }, "border-block" => new[] { "top", "bottom" },
						"border-inline-start" => new[] { Start() }, "border-inline-end" => new[] { End() }, "border-block-start" => new[] { "top" }, "border-block-end" => new[] { "bottom" },
						"outline" => null, _ => new[] { p.Substring(7) }
					};
					if (sides == null) {
						yield return ("outline-width", w); yield return ("outline-style", s); yield return ("outline-color", c);
						yield break;
					}
					foreach (var sd in sides) {
						yield return ("border-" + sd + "-width", w);
						yield return ("border-" + sd + "-style", s);
						yield return ("border-" + sd + "-color", c);
					}
					yield break;
				}
				case "border-inline-start-width": case "border-inline-start-style": case "border-inline-start-color":
					yield return ("border-" + Start() + p.Substring(p.LastIndexOf('-')), v); yield break;
				case "border-inline-end-width": case "border-inline-end-style": case "border-inline-end-color":
					yield return ("border-" + End() + p.Substring(p.LastIndexOf('-')), v); yield break;
				case "border-block-start-width": case "border-block-start-style": case "border-block-start-color":
					yield return ("border-top" + p.Substring(p.LastIndexOf('-')), v); yield break;
				case "border-block-end-width": case "border-block-end-style": case "border-block-end-color":
					yield return ("border-bottom" + p.Substring(p.LastIndexOf('-')), v); yield break;
				case "border-inline-width": case "border-inline-style": case "border-inline-color": {
					var parts = global ? new List<string> { v } : Css.SplitWs(v);
					string kind = p.Substring(p.LastIndexOf('-'));
					yield return ("border-" + Start() + kind, parts[0]);
					yield return ("border-" + End() + kind, parts.Count > 1 ? parts[1] : parts[0]);
					yield break;
				}
				case "border-block-width": case "border-block-style": case "border-block-color": {
					var parts = global ? new List<string> { v } : Css.SplitWs(v);
					string kind = p.Substring(p.LastIndexOf('-'));
					yield return ("border-top" + kind, parts[0]);
					yield return ("border-bottom" + kind, parts.Count > 1 ? parts[1] : parts[0]);
					yield break;
				}
				case "border-start-start-radius": yield return (rtl ? "border-top-right-radius" : "border-top-left-radius", v); yield break;
				case "border-start-end-radius": yield return (rtl ? "border-top-left-radius" : "border-top-right-radius", v); yield break;
				case "border-end-start-radius": yield return (rtl ? "border-bottom-right-radius" : "border-bottom-left-radius", v); yield break;
				case "border-end-end-radius": yield return (rtl ? "border-bottom-left-radius" : "border-bottom-right-radius", v); yield break;
				case "border-radius": {
					string[] names = { "border-top-left-radius", "border-top-right-radius", "border-bottom-right-radius", "border-bottom-left-radius" };
					if (global) { foreach (var n in names) yield return (n, v); yield break; }
					int slash = v.IndexOf('/');
					var h = Box4Radius(slash < 0 ? v : v.Substring(0, slash));
					var vv = slash < 0 ? h : Box4Radius(v.Substring(slash + 1));
					for (int i = 0; i < 4; i++) yield return (names[i], h[i] + " " + vv[i]);
					yield break;
				}
				case "background": {
					if (global) {
						foreach (var n in new[] { "background-color", "background-image", "background-repeat", "background-position", "background-size", "background-origin", "background-clip" }) yield return (n, v);
						yield break;
					}
					var layers = Css.SplitTopLevel(v, ',');
					var imgs = new List<string>(); var reps = new List<string>(); var poss = new List<string>(); var sizes = new List<string>(); var origs = new List<string>(); var clips = new List<string>();
					string color = "transparent";
					for (int li = 0; li < layers.Count; li++) {
						string img = "none", rep = "repeat", pos = "0% 0%", size = "auto", orig = "padding-box", clip = "border-box";
						var toks = Css.SplitWs(layers[li].Trim());
						var posParts = new List<string>();
						bool afterSlash = false;
						var sizeParts = new List<string>();
						var repToks = new List<string>();
						int boxCount = 0;
						foreach (var t in toks) {
							string lt = t.ToLowerInvariant();
							if (lt == "/") { afterSlash = true; continue; }
							if (lt.StartsWith("url(") || lt.Contains("gradient(") || lt.Contains("image-set(") || lt == "none") { img = t; continue; }
							if (lt == "repeat" || lt == "no-repeat" || lt == "repeat-x" || lt == "repeat-y" || lt == "space" || lt == "round") { repToks.Add(lt); continue; }
							if (lt == "scroll" || lt == "fixed" || lt == "local") continue;
							if (lt == "border-box" || lt == "padding-box" || lt == "content-box" || lt == "text") { if (boxCount == 0) { orig = lt; clip = lt; } else clip = lt; boxCount++; continue; }
							if (afterSlash) { sizeParts.Add(t); continue; }
							if (lt == "left" || lt == "right" || lt == "top" || lt == "bottom" || lt == "center" || Val.ParseLen(lt, new LenCtx { Em = 16, Rem = 16 }) != null) { posParts.Add(t); continue; }
							var c = Css.ParseColor(t);
							if (c != null) { color = t; continue; }
						}
						if (repToks.Count > 0) rep = string.Join(" ", repToks);
						if (orig == "text") orig = "border-box";
						if (posParts.Count > 0) pos = string.Join(" ", posParts);
						if (sizeParts.Count > 0) size = string.Join(" ", sizeParts);
						imgs.Add(img); reps.Add(rep); poss.Add(pos); sizes.Add(size); origs.Add(orig); clips.Add(clip);
					}
					yield return ("background-color", color);
					if (imgs.All(x => x == "none")) { yield return ("background-image", "none"); }
					else {
						yield return ("background-image", string.Join(", ", imgs));
						yield return ("background-repeat", string.Join(", ", reps));
						yield return ("background-position", string.Join(", ", poss));
						yield return ("background-size", string.Join(", ", sizes));
						yield return ("background-origin", string.Join(", ", origs));
						yield return ("background-clip", string.Join(", ", clips));
					}
					yield break;
				}
				case "font": {
					if (global) {
						foreach (var n in new[] { "font-style", "font-weight", "font-size", "line-height", "font-family", "font-stretch", "font-variant-caps" }) yield return (n, v);
						yield break;
					}
					string sys = lv;
					if (sys == "caption" || sys == "icon" || sys == "menu" || sys == "message-box" || sys == "small-caption" || sys == "status-bar" || sys.StartsWith("-webkit-")) {
						yield return ("font-family", "system-ui"); yield return ("font-size", "13px"); yield return ("font-style", "normal"); yield return ("font-weight", "400"); yield return ("line-height", "normal");
						yield break;
					}
					var toks = Css.SplitWs(v);
					string style = "normal", weight = "normal", size = null, lh = "normal", stretch = "normal", caps = "normal";
					int i = 0;
					for (; i < toks.Count; i++) {
						string lt = toks[i].ToLowerInvariant();
						if (lt == "normal") continue;
						if (lt == "italic" || lt == "oblique") { style = lt; continue; }
						if (lt == "small-caps") { caps = lt; continue; }
						if (lt == "bold" || lt == "bolder" || lt == "lighter" || int.TryParse(lt, out int wv) && wv >= 1 && wv <= 1000 && lt.Length == 3) { weight = lt; continue; }
						if (lt.EndsWith("condensed") || lt.EndsWith("expanded")) { stretch = lt; continue; }
						break;
					}
					if (i < toks.Count) { size = toks[i]; i++; }
					if (i < toks.Count && toks[i] == "/") { i++; if (i < toks.Count) { lh = toks[i]; i++; } }
					var fam = new List<string>();
					for (; i < toks.Count; i++) fam.Add(toks[i]);
					string family = string.Join(" ", fam).Replace(" , ", ", ").Replace(" ,", ",");
					if (size == null || family.Length == 0) yield break;
					yield return ("font-style", style); yield return ("font-weight", weight); yield return ("font-size", size); yield return ("line-height", lh);
					yield return ("font-family", family); yield return ("font-stretch", stretch); yield return ("font-variant-caps", caps);
					yield break;
				}
				case "font-variant": yield return ("font-variant-caps", lv.Contains("small-caps") ? "small-caps" : "normal"); if (lv.Contains("tabular-nums")) yield return ("font-variant-numeric", "tabular-nums"); yield break;
				case "list-style": {
					if (global) { yield return ("list-style-type", v); yield return ("list-style-position", v); yield return ("list-style-image", v); yield break; }
					string type = null, pos = "outside", img = "none";
					int noneCount = 0;
					foreach (var t in Css.SplitWs(v)) {
						string lt = t.ToLowerInvariant();
						if (lt == "inside" || lt == "outside") pos = lt;
						else if (lt.StartsWith("url(")) img = t;
						else if (lt == "none") noneCount++;
						else type = t;
					}
					if (type == null) type = noneCount > 0 && img == "none" ? "none" : noneCount > 0 ? "none" : "disc";
					yield return ("list-style-type", type); yield return ("list-style-position", pos); yield return ("list-style-image", img);
					yield break;
				}
				case "flex": {
					if (global) { yield return ("flex-grow", v); yield return ("flex-shrink", v); yield return ("flex-basis", v); yield break; }
					if (lv == "none") { yield return ("flex-grow", "0"); yield return ("flex-shrink", "0"); yield return ("flex-basis", "auto"); yield break; }
					if (lv == "auto") { yield return ("flex-grow", "1"); yield return ("flex-shrink", "1"); yield return ("flex-basis", "auto"); yield break; }
					if (lv == "initial") { yield return ("flex-grow", "0"); yield return ("flex-shrink", "1"); yield return ("flex-basis", "auto"); yield break; }
					var toks = Css.SplitWs(lv);
					var nums = new List<string>();
					string basis = null;
					foreach (var t in toks) { if (Val.Num(t, out _) && basis == null && nums.Count < 2) nums.Add(t); else basis = t; }
					yield return ("flex-grow", nums.Count > 0 ? nums[0] : "1");
					yield return ("flex-shrink", nums.Count > 1 ? nums[1] : "1");
					yield return ("flex-basis", basis ?? "0%");
					yield break;
				}
				case "flex-flow": {
					foreach (var t in Css.SplitWs(lv)) {
						if (t.StartsWith("row") || t.StartsWith("column")) yield return ("flex-direction", t);
						else yield return ("flex-wrap", t);
					}
					yield break;
				}
				case "gap": {
					var parts = global ? new List<string> { v } : Css.SplitWs(v);
					yield return ("row-gap", parts[0]);
					yield return ("column-gap", parts.Count > 1 ? parts[1] : parts[0]);
					yield break;
				}
				case "place-items": { var parts = Css.SplitWs(v); yield return ("align-items", parts[0]); yield return ("justify-items", parts.Count > 1 ? parts[1] : parts[0]); yield break; }
				case "place-content": { var parts = Css.SplitWs(v); yield return ("align-content", parts[0]); yield return ("justify-content", parts.Count > 1 ? parts[1] : parts[0]); yield break; }
				case "place-self": { var parts = Css.SplitWs(v); yield return ("align-self", parts[0]); yield return ("justify-self", parts.Count > 1 ? parts[1] : parts[0]); yield break; }
				case "overflow": { var parts = Css.SplitWs(v); yield return ("overflow-x", parts[0]); yield return ("overflow-y", parts.Count > 1 ? parts[1] : parts[0]); yield break; }
				case "text-decoration": {
					if (global) { yield return ("text-decoration-line", v); yield return ("text-decoration-style", v); yield return ("text-decoration-color", v); yield break; }
					var lines = new List<string>();
					string style = "solid", color = "currentcolor", thick = "auto";
					foreach (var t in Css.SplitWs(v)) {
						string lt = t.ToLowerInvariant();
						if (lt == "underline" || lt == "overline" || lt == "line-through" || lt == "blink" || lt == "none") lines.Add(lt);
						else if (lt == "solid" || lt == "double" || lt == "dotted" || lt == "dashed" || lt == "wavy") style = lt;
						else if (Css.ParseColor(t) != null) color = t;
						else thick = t;
					}
					yield return ("text-decoration-line", lines.Count == 0 ? "none" : string.Join(" ", lines));
					yield return ("text-decoration-style", style);
					yield return ("text-decoration-color", color);
					yield return ("text-decoration-thickness", thick);
					yield break;
				}
				case "grid-area": {
					var parts = Css.SplitTopLevel(v, '/').Select(x => x.Trim()).ToList();
					bool ident = parts.Count == 1 && !Val.Num(parts[0], out _) && !parts[0].StartsWith("span") && parts[0] != "auto";
					if (ident) { yield return ("grid-row-start", parts[0]); yield return ("grid-column-start", parts[0]); yield return ("grid-row-end", parts[0]); yield return ("grid-column-end", parts[0]); yield break; }
					yield return ("grid-row-start", parts[0]);
					yield return ("grid-column-start", parts.Count > 1 ? parts[1] : "auto");
					yield return ("grid-row-end", parts.Count > 2 ? parts[2] : "auto");
					yield return ("grid-column-end", parts.Count > 3 ? parts[3] : "auto");
					yield break;
				}
				case "grid-column": case "grid-row": {
					var parts = Css.SplitTopLevel(v, '/').Select(x => x.Trim()).ToList();
					yield return (p + "-start", parts[0]);
					string endDefault = !Val.Num(parts[0], out _) && !parts[0].StartsWith("span") && parts[0] != "auto" ? parts[0] : "auto";
					yield return (p + "-end", parts.Count > 1 ? parts[1] : endDefault);
					yield break;
				}
				case "grid-template": case "grid": {
					if (lv == "none") { yield return ("grid-template-rows", "none"); yield return ("grid-template-columns", "none"); yield return ("grid-template-areas", "none"); yield break; }
					var parts = Css.SplitTopLevel(v, '/');
					string rows = parts[0].Trim();
					if (rows.Contains('"') || rows.Contains('\'')) {
						var areas = new List<string>();
						var rowSizes = new List<string>();
						int i = 0;
						while (i < rows.Length) {
							if (rows[i] == '"' || rows[i] == '\'') {
								char q = rows[i];
								int e = rows.IndexOf(q, i + 1);
								areas.Add(rows.Substring(i, e - i + 1));
								i = e + 1;
								int ns = i;
								while (i < rows.Length && rows[i] != '"' && rows[i] != '\'') i++;
								string sz = rows.Substring(ns, i - ns).Trim();
								rowSizes.Add(sz.Length == 0 ? "auto" : sz);
							}
							else i++;
						}
						yield return ("grid-template-areas", string.Join(" ", areas));
						yield return ("grid-template-rows", string.Join(" ", rowSizes));
					}
					else if (!rows.StartsWith("auto-flow")) yield return ("grid-template-rows", rows);
					if (parts.Count > 1) {
						string cols = parts[1].Trim();
						if (cols.StartsWith("auto-flow")) { yield return ("grid-auto-flow", "column" + (cols.Contains("dense") ? " dense" : "")); string rest = cols.Replace("auto-flow", "").Replace("dense", "").Trim(); if (rest.Length > 0) yield return ("grid-auto-columns", rest); }
						else yield return ("grid-template-columns", cols);
					}
					if (rows.StartsWith("auto-flow")) { yield return ("grid-auto-flow", "row" + (rows.Contains("dense") ? " dense" : "")); string rest = rows.Replace("auto-flow", "").Replace("dense", "").Trim(); if (rest.Length > 0) yield return ("grid-auto-rows", rest); }
					yield break;
				}
				case "columns": {
					foreach (var t in Css.SplitWs(lv)) if (int.TryParse(t, out _)) yield return ("column-count", t);
					yield break;
				}
				case "text-emphasis": case "column-rule": case "mask": case "transition": case "animation": case "will-change": case "cursor": case "resize": case "user-select": case "appearance": case "text-size-adjust":
					yield break;
				default:
					yield return (p, v);
					yield break;
			}
		}

		static bool IsBorderStyle(string s) => s == "none" || s == "hidden" || s == "dotted" || s == "dashed" || s == "solid" || s == "double" || s == "groove" || s == "ridge" || s == "inset" || s == "outset";

		static string[] Box4(string v) {
			var t = Css.SplitWs(v).Where(x => x != "/" && x != ",").ToList();
			if (t.Count == 0) t.Add("0");
			return t.Count switch {
				1 => new[] { t[0], t[0], t[0], t[0] },
				2 => new[] { t[0], t[1], t[0], t[1] },
				3 => new[] { t[0], t[1], t[2], t[1] },
				_ => new[] { t[0], t[1], t[2], t[3] }
			};
		}

		static string[] Box4Radius(string v) {
			var t = Css.SplitWs(v.Trim()).Where(x => x != "/" && x != ",").ToList();
			if (t.Count == 0) t.Add("0");
			return t.Count switch {
				1 => new[] { t[0], t[0], t[0], t[0] },
				2 => new[] { t[0], t[1], t[0], t[1] },
				3 => new[] { t[0], t[1], t[2], t[1] },
				_ => new[] { t[0], t[1], t[2], t[3] }
			};
		}
	}

	internal static class BE {
		public static ushort U16(byte[] d, int o) => (ushort)(d[o] << 8 | d[o + 1]);
		public static short S16(byte[] d, int o) => (short)(d[o] << 8 | d[o + 1]);
		public static uint U32(byte[] d, int o) => (uint)(d[o] << 24 | d[o + 1] << 16 | d[o + 2] << 8 | d[o + 3]);
		public static int S32(byte[] d, int o) => d[o] << 24 | d[o + 1] << 16 | d[o + 2] << 8 | d[o + 3];
		public static string Tag(byte[] d, int o) => Encoding.ASCII.GetString(d, o, 4);
		public static void W16(List<byte> l, int v) { l.Add((byte)(v >> 8)); l.Add((byte)v); }
		public static void W32(List<byte> l, uint v) { l.Add((byte)(v >> 24)); l.Add((byte)(v >> 16)); l.Add((byte)(v >> 8)); l.Add((byte)v); }
		public static void P16(byte[] d, int o, int v) { d[o] = (byte)(v >> 8); d[o + 1] = (byte)v; }
		public static void P32(byte[] d, int o, uint v) { d[o] = (byte)(v >> 24); d[o + 1] = (byte)(v >> 16); d[o + 2] = (byte)(v >> 8); d[o + 3] = (byte)v; }
	}

	internal sealed class FontFile {
		public byte[] D;
		public int DirOff;
		public readonly Dictionary<string, (int off, int len)> Tables = new();
		public bool IsCff;
		public int UnitsPerEm = 1000;
		public short XMin, YMin, XMax, YMax;
		public int IndexToLoc;
		public ushort MacStyle;
		public short HheaAsc, HheaDesc, HheaGap;
		public short TypoAsc, TypoDesc, TypoGap;
		public ushort WinAsc, WinDesc, FsSelection, WeightClass = 400, WidthClass = 5;
		public short XHeight, CapHeight, StrikePos, StrikeSize;
		public bool HasOs2;
		public int NumGlyphs, NumHMetrics;
		public ushort[] Adv;
		public double ItalicAngle;
		public short UnderlinePos, UnderlineThick;
		public bool FixedPitch;
		public List<string> Families = new();
		public string Subfamily = "", FullName = "", PsName = "";
		public Dictionary<int, ushort> Cmap = new();
		public OtLayout Gsub, Gpos;
		public Gdef Gdef;
		public Dictionary<uint, short> Kern;
		public string Key;
		public readonly object Lock = new();
		public bool HasColorBitmaps;
		public bool IsSymbolCmap;

		public static FontFile Load(byte[] data, int index = 0) {
			data = Unwrap(data);
			var f = new FontFile { D = data };
			uint sig = BE.U32(data, 0);
			int dir = 0;
			if (sig == 0x74746366) {
				int n = (int)BE.U32(data, 8);
				if (index >= n) index = 0;
				dir = (int)BE.U32(data, 12 + 4 * index);
			}
			f.DirOff = dir;
			int numTables = BE.U16(data, dir + 4);
			for (int i = 0; i < numTables; i++) {
				int r = dir + 12 + i * 16;
				string tag = BE.Tag(data, r);
				int off = (int)BE.U32(data, r + 8), len = (int)BE.U32(data, r + 12);
				if (off + len > data.Length) len = Math.Max(0, data.Length - off);
				f.Tables[tag] = (off, len);
			}
			f.IsCff = f.Tables.ContainsKey("CFF ") || f.Tables.ContainsKey("CFF2");
			f.ParseHead();
			f.ParseMaxp();
			f.ParseHhea();
			f.ParseOs2();
			f.ParsePost();
			f.ParseHmtx();
			f.ParseName();
			f.ParseCmap();
			if (f.Tables.TryGetValue("GDEF", out var gd)) f.Gdef = new Gdef(data, gd.off);
			if (f.Tables.TryGetValue("GSUB", out var gs)) f.Gsub = new OtLayout(data, gs.off, false);
			if (f.Tables.TryGetValue("GPOS", out var gp)) f.Gpos = new OtLayout(data, gp.off, true);
			f.ParseKern();
			f.HasColorBitmaps = f.Tables.ContainsKey("sbix") || f.Tables.ContainsKey("CBDT") || f.Tables.ContainsKey("COLR");
			return f;
		}

		public static int FaceCount(byte[] data) {
			if (data.Length >= 12 && BE.U32(data, 0) == 0x74746366) return (int)BE.U32(data, 8);
			return 1;
		}

		public static byte[] Unwrap(byte[] data) {
			if (data.Length < 12) throw new InvalidDataException("Font too small");
			uint sig = BE.U32(data, 0);
			if (sig == 0x774F4646) return Woff.Decode(data);
			if (sig == 0x774F4632) return Woff2.Decode(data);
			return data;
		}

		public bool Has(string t) => Tables.ContainsKey(t);
		public int T(string t) => Tables.TryGetValue(t, out var v) ? v.off : -1;

		void ParseHead() {
			int o = T("head");
			if (o < 0) return;
			UnitsPerEm = BE.U16(D, o + 18);
			if (UnitsPerEm == 0) UnitsPerEm = 1000;
			XMin = BE.S16(D, o + 36); YMin = BE.S16(D, o + 38); XMax = BE.S16(D, o + 40); YMax = BE.S16(D, o + 42);
			MacStyle = BE.U16(D, o + 44);
			IndexToLoc = BE.S16(D, o + 50);
		}

		void ParseMaxp() {
			int o = T("maxp");
			NumGlyphs = o < 0 ? 0 : BE.U16(D, o + 4);
		}

		void ParseHhea() {
			int o = T("hhea");
			if (o < 0) return;
			HheaAsc = BE.S16(D, o + 4); HheaDesc = BE.S16(D, o + 6); HheaGap = BE.S16(D, o + 8);
			NumHMetrics = BE.U16(D, o + 34);
		}

		void ParseOs2() {
			int o = T("OS/2");
			if (o < 0) { TypoAsc = HheaAsc; TypoDesc = HheaDesc; TypoGap = HheaGap; WinAsc = (ushort)Math.Max(0, (int)HheaAsc); WinDesc = (ushort)Math.Max(0, -HheaDesc); return; }
			HasOs2 = true;
			int len = Tables["OS/2"].len;
			WeightClass = BE.U16(D, o + 4);
			WidthClass = BE.U16(D, o + 6);
			StrikeSize = BE.S16(D, o + 26); StrikePos = BE.S16(D, o + 28);
			FsSelection = BE.U16(D, o + 62);
			TypoAsc = BE.S16(D, o + 68); TypoDesc = BE.S16(D, o + 70); TypoGap = BE.S16(D, o + 72);
			WinAsc = BE.U16(D, o + 74); WinDesc = BE.U16(D, o + 76);
			if (len >= 90) { XHeight = BE.S16(D, o + 86); CapHeight = BE.S16(D, o + 88); }
			if (WeightClass < 100 && WeightClass > 0) WeightClass = (ushort)(WeightClass * 100);
			if (WeightClass == 0) WeightClass = 400;
		}

		void ParsePost() {
			int o = T("post");
			if (o < 0) return;
			ItalicAngle = BE.S32(D, o + 4) / 65536.0;
			UnderlinePos = BE.S16(D, o + 8);
			UnderlineThick = BE.S16(D, o + 10);
			FixedPitch = BE.U32(D, o + 12) != 0;
		}

		void ParseHmtx() {
			int o = T("hmtx");
			if (NumGlyphs == 0) NumGlyphs = Math.Max(1, NumHMetrics);
			Adv = new ushort[NumGlyphs];
			if (o < 0 || NumHMetrics == 0) return;
			int len = Tables["hmtx"].len;
			ushort last = 0;
			for (int i = 0; i < NumGlyphs; i++) {
				if (i < NumHMetrics && o + i * 4 + 2 <= o + len) last = BE.U16(D, o + i * 4);
				Adv[i] = last;
			}
		}

		internal void ParseName() {
			int o = T("name");
			if (o < 0) return;
			int count = BE.U16(D, o + 2), strOff = o + BE.U16(D, o + 4);
			var fam1 = new List<string>();
			var fam16 = new List<string>();
			for (int i = 0; i < count; i++) {
				int r = o + 6 + i * 12;
				if (r + 12 > D.Length) break;
				int pid = BE.U16(D, r), eid = BE.U16(D, r + 2), lid = BE.U16(D, r + 4), nid = BE.U16(D, r + 6), len = BE.U16(D, r + 8), off = BE.U16(D, r + 10);
				if (nid != 1 && nid != 2 && nid != 4 && nid != 6 && nid != 16 && nid != 21) continue;
				int so = strOff + off;
				if (so + len > D.Length) continue;
				string s;
				if (pid == 3 || pid == 0) s = Encoding.BigEndianUnicode.GetString(D, so, len);
				else if (pid == 1 && eid == 0) s = Encoding.Latin1.GetString(D, so, len);
				else continue;
				s = s.Trim('\0', ' ');
				if (s.Length == 0) continue;
				switch (nid) {
					case 1: fam1.Add(s); break;
					case 16: case 21: fam16.Add(s); break;
					case 2: if (Subfamily.Length == 0 || pid == 3 && lid == 0x409) Subfamily = s; break;
					case 4: if (FullName.Length == 0 || pid == 3 && lid == 0x409) FullName = s; break;
					case 6: if (PsName.Length == 0) PsName = s; break;
				}
			}
			foreach (var n in fam16.Concat(fam1)) if (!Families.Contains(n, StringComparer.OrdinalIgnoreCase)) Families.Add(n);
		}

		void ParseCmap() {
			int o = T("cmap");
			if (o < 0) return;
			int n = BE.U16(D, o + 2);
			int best = -1, bestScore = -1;
			for (int i = 0; i < n; i++) {
				int r = o + 4 + i * 8;
				int pid = BE.U16(D, r), eid = BE.U16(D, r + 2);
				int sub = o + (int)BE.U32(D, r + 4);
				if (sub >= D.Length) continue;
				int fmt = BE.U16(D, sub);
				int score = -1;
				if (pid == 3 && eid == 10 && fmt == 12) score = 10;
				else if (pid == 0 && (eid == 4 || eid == 6) && fmt == 12) score = 9;
				else if (pid == 3 && eid == 1 && fmt == 4) score = 8;
				else if (pid == 0 && fmt == 4) score = 7;
				else if (pid == 0 && fmt == 12) score = 7;
				else if (pid == 3 && eid == 0 && fmt == 4) score = 5;
				else if (pid == 1 && eid == 0 && (fmt == 0 || fmt == 6)) score = 2;
				else if (fmt == 4 || fmt == 12 || fmt == 6 || fmt == 0) score = 1;
				if (score > bestScore) { bestScore = score; best = sub; if (pid == 3 && eid == 0) IsSymbolCmap = true; else IsSymbolCmap = false; }
			}
			if (best < 0) return;
			int f = BE.U16(D, best);
			switch (f) {
				case 0:
					for (int c = 0; c < 256; c++) { byte g = D[best + 6 + c]; if (g != 0) Cmap[c] = g; }
					break;
				case 4: {
					int segX2 = BE.U16(D, best + 6);
					int ends = best + 14, starts = ends + segX2 + 2, deltas = starts + segX2, ranges = deltas + segX2;
					for (int s = 0; s < segX2 / 2; s++) {
						int end = BE.U16(D, ends + s * 2), start = BE.U16(D, starts + s * 2);
						short delta = BE.S16(D, deltas + s * 2);
						int ro = BE.U16(D, ranges + s * 2);
						if (start > end) continue;
						for (int c = start; c <= end && c != 0xFFFF; c++) {
							int g;
							if (ro == 0) g = (c + delta) & 0xFFFF;
							else {
								int addr = ranges + s * 2 + ro + (c - start) * 2;
								if (addr + 1 >= D.Length) continue;
								g = BE.U16(D, addr);
								if (g != 0) g = (g + delta) & 0xFFFF;
							}
							if (g != 0) Cmap[c] = (ushort)g;
						}
					}
					break;
				}
				case 6: {
					int first = BE.U16(D, best + 6), cnt = BE.U16(D, best + 8);
					for (int k = 0; k < cnt; k++) { ushort g = BE.U16(D, best + 10 + k * 2); if (g != 0) Cmap[first + k] = g; }
					break;
				}
				case 12: {
					int groups = (int)BE.U32(D, best + 12);
					for (int k = 0; k < groups; k++) {
						int r = best + 16 + k * 12;
						uint sc = BE.U32(D, r), ec = BE.U32(D, r + 4), sg = BE.U32(D, r + 8);
						if (ec - sc > 0x30000) continue;
						for (uint c = sc; c <= ec; c++) { uint g = sg + (c - sc); if (g != 0 && g < 65536) Cmap[(int)c] = (ushort)g; }
					}
					break;
				}
			}
			if (IsSymbolCmap) {
				var extra = new List<KeyValuePair<int, ushort>>();
				foreach (var kv in Cmap) if (kv.Key >= 0xF000 && kv.Key <= 0xF0FF) extra.Add(new(kv.Key - 0xF000, kv.Value));
				foreach (var kv in extra) if (!Cmap.ContainsKey(kv.Key)) Cmap[kv.Key] = kv.Value;
			}
		}

		void ParseKern() {
			int o = T("kern");
			if (o < 0) return;
			int ver = BE.U16(D, o);
			if (ver != 0) return;
			int n = BE.U16(D, o + 2);
			int p = o + 4;
			for (int t = 0; t < n; t++) {
				int len = BE.U16(D, p + 2), cov = BE.U16(D, p + 4);
				if ((cov >> 8) == 0 && (cov & 1) == 1 && (cov & 4) == 0) {
					int np = BE.U16(D, p + 6);
					Kern ??= new Dictionary<uint, short>();
					for (int k = 0; k < np; k++) {
						int r = p + 14 + k * 6;
						if (r + 6 > D.Length) break;
						Kern[BE.U32(D, r)] = BE.S16(D, r + 4);
					}
				}
				p += len;
			}
		}

		public ushort Glyph(int cp) => Cmap.TryGetValue(cp, out ushort g) ? g : (ushort)0;
		public bool HasGlyph(int cp) => Cmap.ContainsKey(cp);
		public int AdvanceUnits(int gid) => gid < Adv.Length ? Adv[gid] : 0;

		public void Metrics(out double asc, out double desc, out double gap) {
			bool useTypo = (FsSelection & 0x80) != 0;
			if (useTypo && HasOs2) { asc = TypoAsc; desc = -TypoDesc; gap = TypoGap; }
			else if (HheaAsc != 0 || HheaDesc != 0) { asc = HheaAsc; desc = -HheaDesc; gap = HheaGap; }
			else if (HasOs2) { asc = TypoAsc; desc = -TypoDesc; gap = TypoGap; if (asc == 0 && desc == 0) { asc = WinAsc; desc = WinDesc; gap = 0; } }
			else { asc = UnitsPerEm * 0.8; desc = UnitsPerEm * 0.2; gap = 0; }
			double u = UnitsPerEm;
			asc /= u; desc /= u; gap /= u;
		}

		public double XHeightEm() {
			if (XHeight > 0) return (double)XHeight / UnitsPerEm;
			return 0.5;
		}

		public double CapHeightEm() {
			if (CapHeight > 0) return (double)CapHeight / UnitsPerEm;
			return 0.7;
		}

		public (int off, int len) GlyphData(int gid) {
			int loca = T("loca"), glyf = T("glyf");
			if (loca < 0 || glyf < 0 || gid >= NumGlyphs) return (0, 0);
			int a, b;
			if (IndexToLoc == 0) { a = BE.U16(D, loca + gid * 2) * 2; b = BE.U16(D, loca + gid * 2 + 2) * 2; }
			else { a = (int)BE.U32(D, loca + gid * 4); b = (int)BE.U32(D, loca + gid * 4 + 4); }
			if (b < a) return (0, 0);
			return (glyf + a, b - a);
		}

		public bool GlyphBBox(int gid, out short x0, out short y0, out short x1, out short y1) {
			x0 = y0 = x1 = y1 = 0;
			var (o, l) = GlyphData(gid);
			if (l < 10) return false;
			x0 = BE.S16(D, o + 2); y0 = BE.S16(D, o + 4); x1 = BE.S16(D, o + 6); y1 = BE.S16(D, o + 8);
			return true;
		}

		public bool IsBold => WeightClass >= 600;
		public bool IsItalic => (FsSelection & 1) != 0 || (MacStyle & 2) != 0 || ItalicAngle != 0 && Subfamily.IndexOf("italic", StringComparison.OrdinalIgnoreCase) >= 0 || Subfamily.IndexOf("oblique", StringComparison.OrdinalIgnoreCase) >= 0;
	}

	internal static class Woff {
		public static byte[] Decode(byte[] d) {
			uint flavor = BE.U32(d, 4);
			int n = BE.U16(d, 12);
			var tables = new List<(uint tag, byte[] data)>();
			for (int i = 0; i < n; i++) {
				int r = 44 + i * 20;
				uint tag = BE.U32(d, r);
				int off = (int)BE.U32(d, r + 4), comp = (int)BE.U32(d, r + 8), orig = (int)BE.U32(d, r + 12);
				byte[] td;
				if (comp < orig) {
					using var ms = new MemoryStream(d, off, comp);
					using var z = new ZLibStream(ms, CompressionMode.Decompress);
					td = new byte[orig];
					int read = 0;
					while (read < orig) { int k = z.Read(td, read, orig - read); if (k <= 0) break; read += k; }
				}
				else { td = new byte[orig]; Buffer.BlockCopy(d, off, td, 0, orig); }
				tables.Add((tag, td));
			}
			return Sfnt.Build(flavor, tables);
		}
	}

	internal static class Sfnt {
		public static byte[] Build(uint flavor, List<(uint tag, byte[] data)> tables) {
			tables.Sort((a, b) => a.tag.CompareTo(b.tag));
			int n = tables.Count;
			int es = 1, sel = 0;
			while (es * 2 <= n) { es *= 2; sel++; }
			var o = new List<byte>();
			BE.W32(o, flavor);
			BE.W16(o, n); BE.W16(o, es * 16); BE.W16(o, sel); BE.W16(o, n * 16 - es * 16);
			int off = 12 + n * 16;
			var offsets = new int[n];
			for (int i = 0; i < n; i++) { offsets[i] = off; off += (tables[i].data.Length + 3) & ~3; }
			for (int i = 0; i < n; i++) {
				BE.W32(o, tables[i].tag);
				BE.W32(o, Checksum(tables[i].data));
				BE.W32(o, (uint)offsets[i]);
				BE.W32(o, (uint)tables[i].data.Length);
			}
			foreach (var t in tables) {
				o.AddRange(t.data);
				while (o.Count % 4 != 0) o.Add(0);
			}
			return o.ToArray();
		}

		public static uint Checksum(byte[] d) {
			uint sum = 0;
			int i = 0;
			for (; i + 4 <= d.Length; i += 4) sum += BE.U32(d, i);
			if (i < d.Length) {
				uint last = 0;
				for (int k = 0; k < 4; k++) last = last << 8 | (uint)(i + k < d.Length ? d[i + k] : 0);
				sum += last;
			}
			return sum;
		}
	}

	internal static class Woff2 {
		static readonly string[] KnownTags = { "cmap", "head", "hhea", "hmtx", "maxp", "name", "OS/2", "post", "cvt ", "fpgm", "glyf", "loca", "prep", "CFF ", "VORG", "EBDT", "EBLC", "gasp", "hdmx", "kern", "LTSH", "PCLT", "VDMX", "vhea", "vmtx", "BASE", "GDEF", "GPOS", "GSUB", "EBSC", "JSTF", "MATH", "CBDT", "CBLC", "COLR", "CPAL", "SVG ", "sbix", "acnt", "avar", "bdat", "bloc", "bsln", "cvar", "fdsc", "feat", "fmtx", "fvar", "gvar", "hsty", "just", "lcar", "mort", "morx", "opbd", "prop", "trak", "Zapf", "Silf", "Glat", "Gloc", "Feat", "Sill" };

		sealed class R {
			public byte[] D; public int P;
			public R(byte[] d, int p) { D = d; P = p; }
			public byte U8() => D[P++];
			public ushort U16() { ushort v = BE.U16(D, P); P += 2; return v; }
			public short S16() { short v = BE.S16(D, P); P += 2; return v; }
			public uint U32() { uint v = BE.U32(D, P); P += 4; return v; }
			public uint B128() {
				uint acc = 0;
				for (int i = 0; i < 5; i++) {
					byte b = D[P++];
					acc = acc << 7 | (uint)(b & 0x7F);
					if ((b & 0x80) == 0) return acc;
				}
				return acc;
			}
			public int U255() {
				byte c = D[P++];
				if (c == 253) return U16();
				if (c == 255) return D[P++] + 253;
				if (c == 254) return D[P++] + 506;
				return c;
			}
		}

		public static byte[] Decode(byte[] d) {
			var r = new R(d, 4);
			uint flavor = r.U32();
			r.U32();
			int numTables = r.U16();
			r.U16();
			r.U32();
			uint totalCompressed = r.U32();
			r.P = 48;
			var dir = new List<(string tag, int flags, int orig, int trans)>();
			for (int i = 0; i < numTables; i++) {
				byte fl = r.U8();
				int ti = fl & 63;
				string tag = ti == 63 ? Encoding.ASCII.GetString(d, r.P, 4) : KnownTags[ti];
				if (ti == 63) r.P += 4;
				int ver = fl >> 6 & 3;
				int orig = (int)r.B128();
				bool transformed = tag == "glyf" || tag == "loca" ? ver == 0 : ver != 0;
				int trans = transformed ? (int)r.B128() : orig;
				dir.Add((tag, transformed ? 1 : 0, orig, trans));
			}
			if (flavor == 0x74746366) throw new NotSupportedException("WOFF2 collections are not supported");
			byte[] data;
			using (var ms = new MemoryStream(d, r.P, (int)totalCompressed))
			using (var br = new BrotliStream(ms, CompressionMode.Decompress))
			using (var outp = new MemoryStream()) {
				br.CopyTo(outp);
				data = outp.ToArray();
			}
			var raw = new Dictionary<string, byte[]>();
			int p = 0;
			foreach (var t in dir) {
				var b = new byte[t.trans];
				Buffer.BlockCopy(data, p, b, 0, Math.Min(t.trans, data.Length - p));
				p += t.trans;
				raw[t.tag] = b;
			}
			var result = new List<(uint, byte[])>();
			byte[] glyf = null, loca = null;
			int numGlyphs = 0, indexFormat = 0;
			short[] xMins = null;
			var glyfEntry = dir.FirstOrDefault(x => x.tag == "glyf");
			if (glyfEntry.tag != null && glyfEntry.flags == 1) {
				ReconstructGlyf(raw["glyf"], out glyf, out loca, out numGlyphs, out indexFormat, out xMins);
				raw["glyf"] = glyf;
				raw["loca"] = loca;
				if (raw.TryGetValue("head", out var head)) BE.P16(head, 50, indexFormat);
			}
			var hmtxEntry = dir.FirstOrDefault(x => x.tag == "hmtx");
			if (hmtxEntry.tag != null && hmtxEntry.flags == 1) {
				int nh = BE.U16(raw["hhea"], 34);
				int ng = numGlyphs > 0 ? numGlyphs : BE.U16(raw["maxp"], 4);
				raw["hmtx"] = ReconstructHmtx(raw["hmtx"], nh, ng, xMins);
			}
			foreach (var t in dir) {
				uint tag = BE.U32(Encoding.ASCII.GetBytes(t.tag), 0);
				result.Add((tag, raw[t.tag]));
			}
			return Sfnt.Build(flavor, result);
		}

		static int WithSign(int flag, int v) => (flag & 1) != 0 ? v : -v;

		static void ReconstructGlyf(byte[] t, out byte[] glyf, out byte[] loca, out int numGlyphs, out int indexFormat, out short[] xMins) {
			var r = new R(t, 0);
			r.U16();
			r.U16();
			numGlyphs = r.U16();
			indexFormat = r.U16();
			int nContourSize = (int)r.U32(), nPointsSize = (int)r.U32(), flagSize = (int)r.U32(), glyphSize = (int)r.U32(), compositeSize = (int)r.U32(), bboxSize = (int)r.U32(), instrSize = (int)r.U32();
			int off = r.P;
			var nContour = new R(t, off); off += nContourSize;
			var nPoints = new R(t, off); off += nPointsSize;
			var flagS = new R(t, off); off += flagSize;
			var glyphS = new R(t, off); off += glyphSize;
			var compS = new R(t, off); off += compositeSize;
			int bboxStart = off;
			int bitmapLen = ((numGlyphs + 31) >> 5) << 2;
			var bboxS = new R(t, bboxStart + bitmapLen); off += bboxSize;
			var instrS = new R(t, off);
			var outG = new List<byte>();
			var offsets = new int[numGlyphs + 1];
			xMins = new short[numGlyphs];
			for (int g = 0; g < numGlyphs; g++) {
				offsets[g] = outG.Count;
				short nc = nContour.S16();
				bool hasBbox = (t[bboxStart + (g >> 3)] & (0x80 >> (g & 7))) != 0;
				if (nc == 0) continue;
				var gl = new List<byte>();
				if (nc > 0) {
					var endPts = new int[nc];
					int total = 0;
					for (int c = 0; c < nc; c++) { total += nPoints.U255(); endPts[c] = total - 1; }
					var xs = new int[total]; var ys = new int[total]; var on = new bool[total];
					int x = 0, y = 0;
					for (int i = 0; i < total; i++) {
						int flag = flagS.U8();
						on[i] = (flag & 0x80) == 0;
						flag &= 0x7F;
						int dx, dy;
						if (flag < 10) { dx = 0; dy = WithSign(flag, ((flag & 14) << 7) + glyphS.U8()); }
						else if (flag < 20) { dx = WithSign(flag, (((flag - 10) & 14) << 7) + glyphS.U8()); dy = 0; }
						else if (flag < 84) { int b0 = flag - 20, b1 = glyphS.U8(); dx = WithSign(flag, 1 + (b0 & 0x30) + (b1 >> 4)); dy = WithSign(flag >> 1, 1 + ((b0 & 0x0c) << 2) + (b1 & 0x0f)); }
						else if (flag < 120) { int b0 = flag - 84; int i0 = glyphS.U8(), i1 = glyphS.U8(); dx = WithSign(flag, 1 + ((b0 / 12) << 8) + i0); dy = WithSign(flag >> 1, 1 + (((b0 % 12) >> 2) << 8) + i1); }
						else if (flag < 124) { int i0 = glyphS.U8(), i1 = glyphS.U8(), i2 = glyphS.U8(); dx = WithSign(flag, (i0 << 4) + (i1 >> 4)); dy = WithSign(flag >> 1, ((i1 & 0x0f) << 8) + i2); }
						else { int i0 = glyphS.U8(), i1 = glyphS.U8(), i2 = glyphS.U8(), i3 = glyphS.U8(); dx = WithSign(flag, (i0 << 8) + i1); dy = WithSign(flag >> 1, (i2 << 8) + i3); }
						x += dx; y += dy;
						xs[i] = x; ys[i] = y;
					}
					int instLen = glyphS.U255();
					short bx0, by0, bx1, by1;
					if (hasBbox) { bx0 = bboxS.S16(); by0 = bboxS.S16(); bx1 = bboxS.S16(); by1 = bboxS.S16(); }
					else if (total > 0) { bx0 = (short)xs.Min(); by0 = (short)ys.Min(); bx1 = (short)xs.Max(); by1 = (short)ys.Max(); }
					else { bx0 = by0 = bx1 = by1 = 0; }
					xMins[g] = bx0;
					BE.W16(gl, nc); BE.W16(gl, bx0); BE.W16(gl, by0); BE.W16(gl, bx1); BE.W16(gl, by1);
					foreach (var e in endPts) BE.W16(gl, e);
					BE.W16(gl, instLen);
					for (int k = 0; k < instLen; k++) gl.Add(instrS.U8());
					for (int i = 0; i < total; i++) gl.Add((byte)(on[i] ? 1 : 0));
					int px = 0;
					for (int i = 0; i < total; i++) { BE.W16(gl, xs[i] - px); px = xs[i]; }
					int py = 0;
					for (int i = 0; i < total; i++) { BE.W16(gl, ys[i] - py); py = ys[i]; }
				}
				else {
					short bx0 = bboxS.S16(), by0 = bboxS.S16(), bx1 = bboxS.S16(), by1 = bboxS.S16();
					xMins[g] = bx0;
					BE.W16(gl, -1); BE.W16(gl, bx0); BE.W16(gl, by0); BE.W16(gl, bx1); BE.W16(gl, by1);
					bool haveInstr = false;
					while (true) {
						ushort fl = compS.U16();
						BE.W16(gl, fl);
						BE.W16(gl, compS.U16());
						int argBytes = (fl & 1) != 0 ? 4 : 2;
						int trBytes = (fl & 8) != 0 ? 2 : (fl & 0x40) != 0 ? 4 : (fl & 0x80) != 0 ? 8 : 0;
						for (int k = 0; k < argBytes + trBytes; k++) gl.Add(compS.U8());
						if ((fl & 0x100) != 0) haveInstr = true;
						if ((fl & 0x20) == 0) break;
					}
					if (haveInstr) {
						int instLen = glyphS.U255();
						BE.W16(gl, instLen);
						for (int k = 0; k < instLen; k++) gl.Add(instrS.U8());
					}
				}
				outG.AddRange(gl);
				while (outG.Count % 4 != 0) outG.Add(0);
			}
			offsets[numGlyphs] = outG.Count;
			glyf = outG.ToArray();
			var l = new List<byte>();
			if (indexFormat == 0) foreach (var o in offsets) BE.W16(l, o / 2);
			else foreach (var o in offsets) BE.W32(l, (uint)o);
			loca = l.ToArray();
		}

		static byte[] ReconstructHmtx(byte[] t, int numH, int numGlyphs, short[] xMins) {
			var r = new R(t, 0);
			int flags = r.U8();
			var adv = new ushort[numH];
			for (int i = 0; i < numH; i++) adv[i] = r.U16();
			var lsb = new short[numGlyphs];
			for (int i = 0; i < numH; i++) lsb[i] = (flags & 1) != 0 ? (xMins != null ? xMins[i] : (short)0) : r.S16();
			for (int i = numH; i < numGlyphs; i++) lsb[i] = (flags & 2) != 0 ? (xMins != null ? xMins[i] : (short)0) : r.S16();
			var o = new List<byte>();
			for (int i = 0; i < numH; i++) { BE.W16(o, adv[i]); BE.W16(o, lsb[i]); }
			for (int i = numH; i < numGlyphs; i++) BE.W16(o, lsb[i]);
			return o.ToArray();
		}
	}

	internal sealed class FaceInfo {
		public string Path;
		public int Index;
		public byte[] Data;
		public List<string> Families = new();
		public string FullName, PsName;
		public int Weight = 400, WeightMax = 400;
		public bool Italic;
		public int Stretch = 100;
		public List<(int a, int b)> Ranges;
		public FontFile Loaded;
		public bool Failed;

		public FontFile Get() {
			if (Loaded != null || Failed) return Loaded;
			lock (this) {
				if (Loaded != null || Failed) return Loaded;
				try {
					var key = (Path ?? "mem") + "#" + Index + "#" + (Data?.Length ?? 0);
					if (Path != null && FontCache.Files.TryGetValue(key, out var cached)) { Loaded = cached; return Loaded; }
					var data = Data ?? File.ReadAllBytes(Path);
					Loaded = FontFile.Load(data, Index);
					Loaded.Key = key;
					if (Path != null) FontCache.Files[key] = Loaded;
				}
				catch { Failed = true; }
				return Loaded;
			}
		}

		public bool Covers(int cp) {
			if (Ranges == null) return true;
			foreach (var (a, b) in Ranges) if (cp >= a && cp <= b) return true;
			return false;
		}
	}

	internal static class FontCache {
		public static readonly System.Collections.Concurrent.ConcurrentDictionary<string, FontFile> Files = new();
		static List<FaceInfo> _system;
		static readonly object Lock = new();
		static readonly Dictionary<string, List<FaceInfo>> _dirCache = new();

		public static List<FaceInfo> System {
			get {
				if (_system != null) return _system;
				lock (Lock) {
					if (_system != null) return _system;
					var list = new List<FaceInfo>();
					foreach (var d in SystemDirs()) list.AddRange(ScanDir(d));
					_system = list;
					return _system;
				}
			}
		}

		public static List<FaceInfo> ScanDirCached(string dir) {
			lock (Lock) {
				if (_dirCache.TryGetValue(dir, out var l)) return l;
				l = ScanDir(dir).ToList();
				_dirCache[dir] = l;
				return l;
			}
		}

		static IEnumerable<string> SystemDirs() {
			var dirs = new List<string>();
			string win = Environment.GetEnvironmentVariable("WINDIR");
			if (!string.IsNullOrEmpty(win)) dirs.Add(global::System.IO.Path.Combine(win, "Fonts"));
			string local = Environment.GetEnvironmentVariable("LOCALAPPDATA");
			if (!string.IsNullOrEmpty(local)) dirs.Add(global::System.IO.Path.Combine(local, "Microsoft", "Windows", "Fonts"));
			string home = Environment.GetEnvironmentVariable("HOME") ?? "";
			dirs.AddRange(new[] { "/System/Library/Fonts", "/Library/Fonts", home + "/Library/Fonts", "/usr/share/fonts", "/usr/local/share/fonts", home + "/.fonts", home + "/.local/share/fonts", "/usr/X11R6/lib/X11/fonts" });
			dirs.Add(global::System.IO.Path.Combine(AppContext.BaseDirectory, "Fonts"));
			dirs.Add(global::System.IO.Path.Combine(AppContext.BaseDirectory, "Templates", "Fonts"));
			return dirs.Where(Directory.Exists).Distinct();
		}

		public static IEnumerable<FaceInfo> ScanDir(string dir) {
			IEnumerable<string> files;
			try { files = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories).ToList(); }
			catch { yield break; }
			foreach (var f in files) {
				string ext = global::System.IO.Path.GetExtension(f).ToLowerInvariant();
				if (ext != ".ttf" && ext != ".otf" && ext != ".ttc" && ext != ".otc" && ext != ".woff" && ext != ".woff2") continue;
				List<FaceInfo> faces = null;
				try { faces = ReadFaces(f); } catch { }
				if (faces != null) foreach (var x in faces) yield return x;
			}
		}

		public static List<FaceInfo> ReadFaces(string path) {
			var res = new List<FaceInfo>();
			string ext = global::System.IO.Path.GetExtension(path).ToLowerInvariant();
			if (ext == ".woff" || ext == ".woff2") {
				var ff = FontFile.Load(File.ReadAllBytes(path));
				res.Add(InfoFrom(ff, path, 0));
				return res;
			}
			using var fs = File.OpenRead(path);
			var head = new byte[12];
			if (fs.Read(head, 0, 12) < 12) return res;
			uint sig = BE.U32(head, 0);
			var dirs = new List<int>();
			if (sig == 0x74746366) {
				int n = (int)BE.U32(head, 8);
				var offs = new byte[4 * n];
				fs.ReadExactly(offs, 0, offs.Length);
				for (int i = 0; i < n; i++) dirs.Add((int)BE.U32(offs, i * 4));
			}
			else dirs.Add(0);
			for (int idx = 0; idx < dirs.Count; idx++) {
				var info = ReadHeaderInfo(fs, dirs[idx]);
				if (info == null) continue;
				info.Path = path;
				info.Index = idx;
				res.Add(info);
			}
			return res;
		}

		static byte[] ReadAt(FileStream fs, long off, int len) {
			var b = new byte[len];
			fs.Seek(off, SeekOrigin.Begin);
			int r = 0;
			while (r < len) { int k = fs.Read(b, r, len - r); if (k <= 0) break; r += k; }
			return b;
		}

		static FaceInfo ReadHeaderInfo(FileStream fs, int dir) {
			var h = ReadAt(fs, dir, 12);
			int n = BE.U16(h, 4);
			var d = ReadAt(fs, dir + 12, n * 16);
			var tables = new Dictionary<string, (int, int)>();
			for (int i = 0; i < n; i++) tables[BE.Tag(d, i * 16)] = ((int)BE.U32(d, i * 16 + 8), (int)BE.U32(d, i * 16 + 12));
			if (!tables.ContainsKey("name") || !tables.ContainsKey("cmap")) return null;
			var info = new FaceInfo();
			var (no, nl) = tables["name"];
			var nd = ReadAt(fs, no, nl);
			var tmp = new FontFile { D = nd };
			tmp.Tables["name"] = (0, nl);
			tmp.ParseName();
			info.Families = tmp.Families;
			info.FullName = tmp.FullName;
			info.PsName = tmp.PsName;
			string sub = tmp.Subfamily ?? "";
			if (tables.TryGetValue("OS/2", out var os2) && os2.Item2 >= 64) {
				var od = ReadAt(fs, os2.Item1, Math.Min(os2.Item2, 96));
				int w = BE.U16(od, 4);
				if (w > 0 && w < 10) w *= 100;
				info.Weight = info.WeightMax = w == 0 ? 400 : w;
				info.Stretch = BE.U16(od, 6) switch { 1 => 50, 2 => 62, 3 => 75, 4 => 87, 5 => 100, 6 => 112, 7 => 125, 8 => 150, 9 => 200, _ => 100 };
				ushort fsSel = BE.U16(od, 62);
				info.Italic = (fsSel & 1) != 0 || (fsSel & 0x200) != 0;
			}
			else {
				info.Weight = info.WeightMax = sub.IndexOf("bold", StringComparison.OrdinalIgnoreCase) >= 0 ? 700 : 400;
			}
			if (!info.Italic && (sub.IndexOf("italic", StringComparison.OrdinalIgnoreCase) >= 0 || sub.IndexOf("oblique", StringComparison.OrdinalIgnoreCase) >= 0)) info.Italic = true;
			if (tables.ContainsKey("fvar") && tables.TryGetValue("fvar", out var fv)) {
				var fd = ReadAt(fs, fv.Item1, fv.Item2);
				int axesOff = BE.U16(fd, 4), axisCount = BE.U16(fd, 8), axisSize = BE.U16(fd, 10);
				for (int a = 0; a < axisCount; a++) {
					int r = axesOff + a * axisSize;
					if (r + 20 > fd.Length) break;
					if (BE.Tag(fd, r) == "wght") { info.Weight = (int)(BE.S32(fd, r + 4) / 65536.0); info.WeightMax = (int)(BE.S32(fd, r + 12) / 65536.0); }
				}
			}
			return info;
		}

		public static FaceInfo InfoFrom(FontFile f, string path, int idx) {
			return new FaceInfo {
				Path = path, Index = idx, Families = f.Families, FullName = f.FullName, PsName = f.PsName,
				Weight = f.WeightClass, WeightMax = f.WeightClass, Italic = f.IsItalic, Loaded = f,
				Stretch = f.WidthClass switch { 1 => 50, 2 => 62, 3 => 75, 4 => 87, 6 => 112, 7 => 125, 8 => 150, 9 => 200, _ => 100 }
			};
		}
	}

	internal sealed class ResolvedFont {
		public FontFile File;
		public bool SynthBold, SynthItalic;
		public double Size;
		public FaceInfo Face;
		public double Asc, Desc, Gap, XH;

		public void Init() {
			File.Metrics(out double a, out double d, out double g);
			Asc = Math.Round(a * Size);
			Desc = Math.Round(d * Size);
			Gap = Math.Round(g * Size);
			if (Asc + Desc <= 0) { Asc = Math.Round(Size * 0.8); Desc = Math.Round(Size * 0.2); }
			XH = File.XHeightEm() * Size;
		}

		public double NormalLineHeight => Asc + Desc + Gap;
	}

	internal sealed class FontProvider {
		readonly List<FaceInfo> _web = new();
		readonly List<FaceInfo> _user = new();
		readonly bool _useSystem;
		readonly Dictionary<string, List<FaceInfo>> _familyCache = new(StringComparer.OrdinalIgnoreCase);
		readonly Dictionary<(string, int, byte), FaceInfo> _matchCache = new();
		readonly Dictionary<(int, int, byte, Sc), FaceInfo> _fallbackCache = new();
		readonly Dictionary<int, FaceInfo> _anyCoverCache = new();
		public string DefaultFamily;

		public FontProvider(bool useSystem) { _useSystem = useSystem; }

		public void AddUserFace(FaceInfo f) => _user.Add(f);
		public void AddWebFace(FaceInfo f) => _web.Add(f);

		IEnumerable<FaceInfo> AllFaces() {
			foreach (var f in _web) yield return f;
			foreach (var f in _user) yield return f;
			if (_useSystem) foreach (var f in FontCache.System) yield return f;
		}

		public bool HasAnyFont() => AllFaces().Any();

		List<FaceInfo> FacesFor(string family) {
			if (_familyCache.TryGetValue(family, out var l)) return l;
			var web = _web.Where(f => f.Families.Any(n => n.Equals(family, StringComparison.OrdinalIgnoreCase))).ToList();
			if (web.Count > 0) { _familyCache[family] = web; return web; }
			l = _user.Concat(_useSystem ? FontCache.System : Enumerable.Empty<FaceInfo>())
				.Where(f => f.Families.Any(n => n.Equals(family, StringComparison.OrdinalIgnoreCase)) || string.Equals(f.FullName, family, StringComparison.OrdinalIgnoreCase) || string.Equals(f.PsName, family, StringComparison.OrdinalIgnoreCase)).ToList();
			if (l.Count == 0 && family.Contains(' ')) {
				string compact = family.Replace(" ", "");
				l = _user.Concat(_useSystem ? FontCache.System : Enumerable.Empty<FaceInfo>()).Where(f => f.Families.Any(n => n.Replace(" ", "").Equals(compact, StringComparison.OrdinalIgnoreCase))).ToList();
			}
			_familyCache[family] = l;
			return l;
		}

		static readonly bool IsWin = OperatingSystem.IsWindows(), IsMac = OperatingSystem.IsMacOS();
		public static readonly string[] Serif = IsWin ? new[] { "Times New Roman", "Times", "Georgia", "Liberation Serif", "DejaVu Serif", "Noto Serif" }
			: IsMac ? new[] { "Times", "Times New Roman", "Georgia", "Liberation Serif", "DejaVu Serif", "Noto Serif" }
			: new[] { "DejaVu Serif", "Liberation Serif", "Noto Serif", "Tinos", "Times New Roman", "Times", "FreeSerif", "Nimbus Roman" };
		public static readonly string[] Sans = IsWin ? new[] { "Arial", "Helvetica", "Liberation Sans", "DejaVu Sans", "Noto Sans", "Segoe UI", "Tahoma" }
			: IsMac ? new[] { "Helvetica", "Arial", "Helvetica Neue", "Liberation Sans", "DejaVu Sans", "Noto Sans" }
			: new[] { "DejaVu Sans", "Liberation Sans", "Noto Sans", "Arimo", "Arial", "Helvetica", "FreeSans", "Nimbus Sans" };
		public static readonly string[] Mono = IsWin ? new[] { "Consolas", "Courier New", "Lucida Console", "Liberation Mono", "DejaVu Sans Mono" }
			: IsMac ? new[] { "Menlo", "Courier", "Courier New", "Monaco", "Liberation Mono", "DejaVu Sans Mono" }
			: new[] { "DejaVu Sans Mono", "Liberation Mono", "Noto Sans Mono", "Cousine", "Courier New", "FreeMono", "Nimbus Mono PS" };
		public static readonly string[] SystemUi = IsWin ? new[] { "Segoe UI", "Tahoma", "Arial" } : IsMac ? new[] { "SF Pro Text", ".SF NS", "Helvetica Neue", "Helvetica" } : new[] { "Ubuntu", "Cantarell", "Noto Sans", "DejaVu Sans", "Liberation Sans" };

		public IEnumerable<string> Expand(string fam) {
			switch (fam) {
				case "serif": return DefaultFamily != null ? new[] { DefaultFamily }.Concat(Serif) : Serif;
				case "sans-serif": return Sans;
				case "monospace": return Mono;
				case "cursive": return (IsMac ? new[] { "Apple Chancery", "Comic Sans MS" } : new[] { "Comic Sans MS", "URW Chancery L" }).Concat(Sans);
				case "fantasy": return (IsMac ? new[] { "Papyrus", "Impact" } : new[] { "Impact", "Papyrus" }).Concat(Sans);
				case "system-ui": return SystemUi;
				default: return new[] { fam };
			}
		}

		public FaceInfo Match(string family, int weight, byte style) {
			var key = (family, weight, style);
			if (_matchCache.TryGetValue(key, out var r)) return r;
			FaceInfo best = null;
			foreach (var fam in Expand(family)) {
				var faces = FacesFor(fam);
				if (faces.Count == 0) continue;
				best = Pick(faces, weight, style);
				if (best != null) break;
			}
			_matchCache[key] = best;
			return best;
		}

		static FaceInfo Pick(List<FaceInfo> faces, int weight, byte style) {
			IEnumerable<FaceInfo> cand = faces;
			var stretchNormal = cand.Where(f => f.Stretch == 100).ToList();
			if (stretchNormal.Count > 0) cand = stretchNormal;
			else { int minDiff = cand.Min(f => Math.Abs(f.Stretch - 100)); cand = cand.Where(f => Math.Abs(f.Stretch - 100) == minDiff).ToList(); }
			var styled = cand.Where(f => f.Italic == (style != 0)).ToList();
			if (styled.Count > 0) cand = styled;
			var list = cand.ToList();
			var exact = list.FirstOrDefault(f => weight >= f.Weight && weight <= f.WeightMax);
			if (exact != null) return exact;
			FaceInfo Nearest(IEnumerable<FaceInfo> xs, bool desc) {
				var arr = xs.ToList();
				if (arr.Count == 0) return null;
				return desc ? arr.OrderByDescending(f => f.WeightMax).First() : arr.OrderBy(f => f.Weight).First();
			}
			if (weight >= 400 && weight <= 500) {
				var up = Nearest(list.Where(f => f.Weight > weight && f.Weight <= 500), false);
				if (up != null) return up;
				var down = Nearest(list.Where(f => f.WeightMax < weight), true);
				if (down != null) return down;
				return Nearest(list.Where(f => f.Weight > 500), false);
			}
			if (weight < 400) {
				var down = Nearest(list.Where(f => f.WeightMax < weight), true);
				if (down != null) return down;
				return Nearest(list.Where(f => f.Weight > weight), false);
			}
			var up2 = Nearest(list.Where(f => f.Weight > weight), false);
			if (up2 != null) return up2;
			return Nearest(list.Where(f => f.WeightMax < weight), true);
		}

		static string[] ScriptFallbacks(Sc sc) {
			switch (sc) {
				case Sc.Arabic: return IsMac ? new[] { "Geeza Pro", "SF Arabic", "Tahoma", "Arial", "Noto Naskh Arabic", "Noto Sans Arabic", "Arial Unicode MS", "Times New Roman" }
					: IsWin ? new[] { "Segoe UI", "Tahoma", "Arial", "Times New Roman", "Arial Unicode MS" }
					: new[] { "Noto Sans Arabic", "Noto Naskh Arabic", "DejaVu Sans", "Vazirmatn", "Vazir", "FreeSerif", "Arial" };
				case Sc.Hebrew: return new[] { "Arial", "Arial Hebrew", "Tahoma", "Noto Sans Hebrew", "DejaVu Sans", "Times New Roman", "Arial Unicode MS" };
				case Sc.Han: case Sc.Bopomofo:
					return new[] { "PingFang SC", "Microsoft YaHei", "Noto Sans CJK SC", "Noto Sans SC", "Source Han Sans SC", "WenQuanYi Micro Hei", "Hiragino Sans GB", "SimSun", "Hiragino Sans", "Yu Gothic", "Songti SC", "Arial Unicode MS" };
				case Sc.Hiragana: case Sc.Katakana:
					return new[] { "Hiragino Sans", "Hiragino Kaku Gothic ProN", "Yu Gothic", "Meiryo", "MS Gothic", "Noto Sans CJK JP", "Noto Sans JP", "Arial Unicode MS" };
				case Sc.Hangul: return new[] { "Apple SD Gothic Neo", "Malgun Gothic", "Noto Sans CJK KR", "Noto Sans KR", "NanumGothic", "Arial Unicode MS" };
				case Sc.Thai: return new[] { "Thonburi", "Tahoma", "Leelawadee UI", "Noto Sans Thai", "Arial Unicode MS" };
				case Sc.Devanagari: return new[] { "Kohinoor Devanagari", "Devanagari Sangam MN", "Nirmala UI", "Mangal", "Noto Sans Devanagari", "Arial Unicode MS" };
				case Sc.Armenian: case Sc.Georgian: return new[] { "Arial", "Sylfaen", "Noto Sans Armenian", "Noto Sans Georgian", "DejaVu Sans", "Arial Unicode MS" };
				case Sc.Syriac: return new[] { "Noto Sans Syriac", "Estrangelo Edessa", "Arial Unicode MS" };
				case Sc.Ethiopic: return new[] { "Kefa", "Nyala", "Noto Sans Ethiopic" };
				case Sc.Emoji: return new[] { "Segoe UI Emoji", "Segoe UI Symbol", "Noto Emoji", "Apple Symbols", "Noto Sans Symbols 2", "DejaVu Sans", "Arial Unicode MS", "Apple Color Emoji", "Noto Color Emoji" };
				default: return IsMac ? new[] { "Lucida Grande", "Helvetica", "Arial", "Apple Symbols", "Menlo", "Arial Unicode MS", "Times New Roman" }
					: new[] { "Segoe UI", "Segoe UI Symbol", "Arial", "DejaVu Sans", "Liberation Sans", "Noto Sans", "Noto Sans Symbols", "Noto Sans Symbols 2", "Tahoma", "Times New Roman", "Arial Unicode MS" };
			}
		}

		public FaceInfo Fallback(int cp, int weight, byte style, Sc hint = Sc.Common) {
			var own = Uni.Script(cp);
			if (own != Sc.Common && own != Sc.Inherited) hint = Sc.Common;
			var key = (cp >> 7, weight, style, hint);
			if (_fallbackCache.TryGetValue(key, out var cached) && cached != null && cached.Get() != null && cached.Get().HasGlyph(cp)) return cached;
			var fams = hint != Sc.Common && hint != Sc.Inherited ? ScriptFallbacks(hint).Concat(ScriptFallbacks(own)) : ScriptFallbacks(own);
			foreach (var fam in fams) {
				var f = Match(fam, weight, style);
				var ff = f?.Get();
				if (ff != null && ff.HasGlyph(cp) && !(ff.HasColorBitmaps && !ff.Has("glyf") && !ff.Has("CFF "))) { _fallbackCache[key] = f; return f; }
			}
			foreach (var f in _web.Concat(_user)) {
				var ff = f.Get();
				if (ff != null && ff.HasGlyph(cp) && f.Covers(cp)) { _fallbackCache[key] = f; return f; }
			}
			if (_anyCoverCache.TryGetValue(cp, out var any)) return any;
			if (_useSystem) {
				foreach (var f in FontCache.System) {
					if (f.Path != null && new FileInfo(f.Path).Length > 40_000_000) continue;
					var ff = f.Get();
					if (ff == null || !ff.HasGlyph(cp)) continue;
					if (!ff.Has("glyf") && !ff.Has("CFF ")) continue;
					_anyCoverCache[cp] = f;
					return f;
				}
			}
			_anyCoverCache[cp] = null;
			return null;
		}

		public FaceInfo AnyFace() {
			foreach (var fam in Sans.Concat(Serif)) { var f = Match(fam, 400, 0); if (f?.Get() != null) return f; }
			foreach (var f in AllFaces()) if (f.Get() != null && (f.Get().Has("glyf") || f.Get().Has("CFF "))) return f;
			return null;
		}

		public static List<(int, int)> ParseUnicodeRange(string s) {
			if (string.IsNullOrWhiteSpace(s)) return null;
			var list = new List<(int, int)>();
			foreach (var part0 in s.Split(',')) {
				string part = part0.Trim().ToUpperInvariant();
				if (part.StartsWith("U+")) part = part.Substring(2);
				int dash = part.IndexOf('-');
				try {
					if (dash > 0) list.Add((int.Parse(part.Substring(0, dash), NumberStyles.HexNumber), int.Parse(part.Substring(dash + 1), NumberStyles.HexNumber)));
					else if (part.Contains('?')) list.Add((int.Parse(part.Replace('?', '0'), NumberStyles.HexNumber), int.Parse(part.Replace('?', 'F'), NumberStyles.HexNumber)));
					else { int v = int.Parse(part, NumberStyles.HexNumber); list.Add((v, v)); }
				}
				catch { }
			}
			return list.Count > 0 ? list : null;
		}
	}

	internal enum BC : byte { L, R, AL, EN, ES, ET, AN, CS, NSM, BN, B, S, WS, ON, LRE, LRO, RLE, RLO, PDF, LRI, RLI, FSI, PDI }

	internal enum LBC : byte { AL, BK, CR, LF, CM, SP, ZW, GL, WJ, ZWJ, BA, BB, HY, CL, CP, OP, QU, IS, NU, PR, PO, SY, EX, NS, ID, IN, CB, RI, SA, H2, H3, JL, JV, JT, EM, B2 }

	internal enum Sc : byte { Common, Inherited, Latin, Greek, Cyrillic, Armenian, Hebrew, Arabic, Syriac, Thaana, Nko, Devanagari, Bengali, Gurmukhi, Gujarati, Oriya, Tamil, Telugu, Kannada, Malayalam, Sinhala, Thai, Lao, Tibetan, Myanmar, Georgian, Hangul, Ethiopic, Khmer, Mongolian, Han, Hiragana, Katakana, Bopomofo, Emoji, Symbol, Other }

	internal static class Uni {
		static readonly int[] BidiRanges = {
			0x0000,0x0008,(int)BC.BN, 0x0009,0x0009,(int)BC.S, 0x000A,0x000A,(int)BC.B, 0x000B,0x000B,(int)BC.S, 0x000C,0x000C,(int)BC.WS, 0x000D,0x000D,(int)BC.B,
			0x000E,0x001B,(int)BC.BN, 0x001C,0x001E,(int)BC.B, 0x001F,0x001F,(int)BC.S, 0x0020,0x0020,(int)BC.WS, 0x0021,0x0022,(int)BC.ON, 0x0023,0x0025,(int)BC.ET,
			0x0026,0x002A,(int)BC.ON, 0x002B,0x002B,(int)BC.ES, 0x002C,0x002C,(int)BC.CS, 0x002D,0x002D,(int)BC.ES, 0x002E,0x002F,(int)BC.CS, 0x0030,0x0039,(int)BC.EN,
			0x003A,0x003A,(int)BC.CS, 0x003B,0x0040,(int)BC.ON, 0x005B,0x0060,(int)BC.ON, 0x007B,0x007E,(int)BC.ON, 0x007F,0x0084,(int)BC.BN, 0x0085,0x0085,(int)BC.B,
			0x0086,0x009F,(int)BC.BN, 0x00A0,0x00A0,(int)BC.CS, 0x00A1,0x00A1,(int)BC.ON, 0x00A2,0x00A5,(int)BC.ET, 0x00A6,0x00A9,(int)BC.ON, 0x00AB,0x00AC,(int)BC.ON,
			0x00AD,0x00AD,(int)BC.BN, 0x00AE,0x00AF,(int)BC.ON, 0x00B0,0x00B1,(int)BC.ET, 0x00B2,0x00B3,(int)BC.EN, 0x00B4,0x00B4,(int)BC.ON, 0x00B6,0x00B8,(int)BC.ON,
			0x00B9,0x00B9,(int)BC.EN, 0x00BB,0x00BF,(int)BC.ON, 0x00D7,0x00D7,(int)BC.ON, 0x00F7,0x00F7,(int)BC.ON, 0x02B9,0x02BA,(int)BC.ON, 0x02C2,0x02CF,(int)BC.ON,
			0x02D2,0x02DF,(int)BC.ON, 0x02E5,0x02ED,(int)BC.ON, 0x02EF,0x02FF,(int)BC.ON, 0x0300,0x036F,(int)BC.NSM, 0x0374,0x0375,(int)BC.ON, 0x037E,0x037E,(int)BC.ON,
			0x0384,0x0385,(int)BC.ON, 0x0387,0x0387,(int)BC.ON, 0x03F6,0x03F6,(int)BC.ON, 0x0483,0x0489,(int)BC.NSM, 0x058A,0x058A,(int)BC.ON, 0x058D,0x058E,(int)BC.ON,
			0x058F,0x058F,(int)BC.ET, 0x0590,0x0590,(int)BC.R, 0x0591,0x05BD,(int)BC.NSM, 0x05BE,0x05BE,(int)BC.R, 0x05BF,0x05BF,(int)BC.NSM, 0x05C0,0x05C0,(int)BC.R,
			0x05C1,0x05C2,(int)BC.NSM, 0x05C3,0x05C3,(int)BC.R, 0x05C4,0x05C5,(int)BC.NSM, 0x05C6,0x05C6,(int)BC.R, 0x05C7,0x05C7,(int)BC.NSM, 0x05C8,0x05FF,(int)BC.R,
			0x0600,0x0605,(int)BC.AN, 0x0606,0x0607,(int)BC.ON, 0x0608,0x0608,(int)BC.AL, 0x0609,0x060A,(int)BC.ET, 0x060B,0x060B,(int)BC.AL, 0x060C,0x060C,(int)BC.CS,
			0x060D,0x060D,(int)BC.AL, 0x060E,0x060F,(int)BC.ON, 0x0610,0x061A,(int)BC.NSM, 0x061B,0x064A,(int)BC.AL, 0x064B,0x065F,(int)BC.NSM, 0x0660,0x0669,(int)BC.AN,
			0x066A,0x066A,(int)BC.ET, 0x066B,0x066C,(int)BC.AN, 0x066D,0x066F,(int)BC.AL, 0x0670,0x0670,(int)BC.NSM, 0x0671,0x06D5,(int)BC.AL, 0x06D6,0x06DC,(int)BC.NSM,
			0x06DD,0x06DD,(int)BC.AN, 0x06DE,0x06DE,(int)BC.ON, 0x06DF,0x06E4,(int)BC.NSM, 0x06E5,0x06E6,(int)BC.AL, 0x06E7,0x06E8,(int)BC.NSM, 0x06E9,0x06E9,(int)BC.ON,
			0x06EA,0x06ED,(int)BC.NSM, 0x06EE,0x06EF,(int)BC.AL, 0x06F0,0x06F9,(int)BC.EN, 0x06FA,0x070F,(int)BC.AL, 0x0711,0x0711,(int)BC.NSM, 0x0710,0x0710,(int)BC.AL,
			0x0712,0x072F,(int)BC.AL, 0x0730,0x074A,(int)BC.NSM, 0x074B,0x07A5,(int)BC.AL, 0x07A6,0x07B0,(int)BC.NSM, 0x07B1,0x07BF,(int)BC.AL, 0x07C0,0x07EA,(int)BC.R,
			0x07EB,0x07F3,(int)BC.NSM, 0x07F4,0x07F5,(int)BC.R, 0x07F6,0x07F9,(int)BC.ON, 0x07FA,0x07FF,(int)BC.R, 0x0800,0x085F,(int)BC.R, 0x0860,0x08C9,(int)BC.AL,
			0x08CA,0x08E1,(int)BC.NSM, 0x08E2,0x08E2,(int)BC.AN, 0x08E3,0x0902,(int)BC.NSM, 0x093A,0x093A,(int)BC.NSM, 0x093C,0x093C,(int)BC.NSM, 0x0941,0x0948,(int)BC.NSM,
			0x094D,0x094D,(int)BC.NSM, 0x0951,0x0957,(int)BC.NSM, 0x0962,0x0963,(int)BC.NSM, 0x0E31,0x0E31,(int)BC.NSM, 0x0E34,0x0E3A,(int)BC.NSM, 0x0E3F,0x0E3F,(int)BC.ET,
			0x0E47,0x0E4E,(int)BC.NSM, 0x1680,0x1680,(int)BC.WS, 0x169B,0x169C,(int)BC.ON, 0x1AB0,0x1AFF,(int)BC.NSM, 0x1DC0,0x1DFF,(int)BC.NSM,
			0x2000,0x200A,(int)BC.WS, 0x200B,0x200D,(int)BC.BN, 0x200E,0x200E,(int)BC.L, 0x200F,0x200F,(int)BC.R, 0x2010,0x2027,(int)BC.ON, 0x2028,0x2028,(int)BC.WS,
			0x2029,0x2029,(int)BC.B, 0x202A,0x202A,(int)BC.LRE, 0x202B,0x202B,(int)BC.RLE, 0x202C,0x202C,(int)BC.PDF, 0x202D,0x202D,(int)BC.LRO, 0x202E,0x202E,(int)BC.RLO,
			0x202F,0x202F,(int)BC.CS, 0x2030,0x2034,(int)BC.ET, 0x2035,0x2043,(int)BC.ON, 0x2044,0x2044,(int)BC.CS, 0x2045,0x205E,(int)BC.ON, 0x205F,0x205F,(int)BC.WS,
			0x2060,0x2064,(int)BC.BN, 0x2066,0x2066,(int)BC.LRI, 0x2067,0x2067,(int)BC.RLI, 0x2068,0x2068,(int)BC.FSI, 0x2069,0x2069,(int)BC.PDI, 0x206A,0x206F,(int)BC.BN,
			0x2070,0x2070,(int)BC.EN, 0x2074,0x2079,(int)BC.EN, 0x207A,0x207B,(int)BC.ES, 0x207C,0x207E,(int)BC.ON, 0x2080,0x2089,(int)BC.EN, 0x208A,0x208B,(int)BC.ES,
			0x208C,0x208E,(int)BC.ON, 0x20A0,0x20CF,(int)BC.ET, 0x20D0,0x20F0,(int)BC.NSM, 0x2100,0x2101,(int)BC.ON, 0x2103,0x2106,(int)BC.ON, 0x2108,0x2109,(int)BC.ON,
			0x2114,0x2114,(int)BC.ON, 0x2116,0x2118,(int)BC.ON, 0x211E,0x2123,(int)BC.ON, 0x2125,0x2125,(int)BC.ON, 0x2127,0x2127,(int)BC.ON, 0x2129,0x2129,(int)BC.ON,
			0x212E,0x212E,(int)BC.ET, 0x213A,0x213B,(int)BC.ON, 0x2140,0x2144,(int)BC.ON, 0x214A,0x214D,(int)BC.ON, 0x2150,0x215F,(int)BC.ON, 0x2189,0x218B,(int)BC.ON,
			0x2190,0x2211,(int)BC.ON, 0x2212,0x2212,(int)BC.ES, 0x2213,0x2213,(int)BC.ET, 0x2214,0x2335,(int)BC.ON, 0x237B,0x2394,(int)BC.ON, 0x2396,0x2426,(int)BC.ON,
			0x2440,0x244A,(int)BC.ON, 0x2460,0x2487,(int)BC.ON, 0x2488,0x249B,(int)BC.EN, 0x24EA,0x26AB,(int)BC.ON, 0x26AD,0x27FF,(int)BC.ON, 0x2900,0x2B73,(int)BC.ON,
			0x2B76,0x2BFF,(int)BC.ON, 0x2CE5,0x2CEA,(int)BC.ON, 0x2CEF,0x2CF1,(int)BC.NSM, 0x2CF9,0x2CFF,(int)BC.ON, 0x2DE0,0x2DFF,(int)BC.NSM, 0x2E00,0x2E5D,(int)BC.ON,
			0x2E80,0x2FFB,(int)BC.ON, 0x3000,0x3000,(int)BC.WS, 0x3001,0x3004,(int)BC.ON, 0x3008,0x3020,(int)BC.ON, 0x302A,0x302D,(int)BC.NSM, 0x3030,0x3030,(int)BC.ON,
			0x3036,0x3037,(int)BC.ON, 0x303D,0x303F,(int)BC.ON, 0x3099,0x309A,(int)BC.NSM, 0x309B,0x309C,(int)BC.ON, 0x30A0,0x30A0,(int)BC.ON, 0x30FB,0x30FB,(int)BC.ON,
			0x31C0,0x31E3,(int)BC.ON, 0x321D,0x321E,(int)BC.ON, 0x3250,0x325F,(int)BC.ON, 0x327C,0x327E,(int)BC.ON, 0x32B1,0x32BF,(int)BC.ON, 0x32CC,0x32CF,(int)BC.ON,
			0x3377,0x337A,(int)BC.ON, 0x33DE,0x33DF,(int)BC.ON, 0x33FF,0x33FF,(int)BC.ON, 0x4DC0,0x4DFF,(int)BC.ON, 0xA490,0xA4C6,(int)BC.ON, 0xA60D,0xA60F,(int)BC.ON,
			0xA66F,0xA672,(int)BC.NSM, 0xA673,0xA673,(int)BC.ON, 0xA674,0xA67D,(int)BC.NSM, 0xA67E,0xA67F,(int)BC.ON, 0xA700,0xA721,(int)BC.ON, 0xA788,0xA788,(int)BC.ON,
			0xFB1D,0xFB1D,(int)BC.R, 0xFB1E,0xFB1E,(int)BC.NSM, 0xFB1F,0xFB28,(int)BC.R, 0xFB29,0xFB29,(int)BC.ES, 0xFB2A,0xFB4F,(int)BC.R, 0xFB50,0xFD3D,(int)BC.AL,
			0xFD3E,0xFD4F,(int)BC.ON, 0xFD50,0xFDCF,(int)BC.AL, 0xFDF0,0xFDFC,(int)BC.AL, 0xFDFD,0xFDFF,(int)BC.ON, 0xFE00,0xFE0F,(int)BC.NSM, 0xFE10,0xFE19,(int)BC.ON,
			0xFE20,0xFE2F,(int)BC.NSM, 0xFE30,0xFE4F,(int)BC.ON, 0xFE50,0xFE50,(int)BC.CS, 0xFE51,0xFE51,(int)BC.ON, 0xFE52,0xFE52,(int)BC.CS, 0xFE54,0xFE54,(int)BC.ON,
			0xFE55,0xFE55,(int)BC.CS, 0xFE56,0xFE5E,(int)BC.ON, 0xFE5F,0xFE5F,(int)BC.ET, 0xFE60,0xFE61,(int)BC.ON, 0xFE62,0xFE63,(int)BC.ES, 0xFE64,0xFE68,(int)BC.ON,
			0xFE69,0xFE6A,(int)BC.ET, 0xFE6B,0xFE6B,(int)BC.ON, 0xFE70,0xFEFE,(int)BC.AL, 0xFEFF,0xFEFF,(int)BC.BN, 0xFF01,0xFF02,(int)BC.ON, 0xFF03,0xFF05,(int)BC.ET,
			0xFF06,0xFF0A,(int)BC.ON, 0xFF0B,0xFF0B,(int)BC.ES, 0xFF0C,0xFF0C,(int)BC.CS, 0xFF0D,0xFF0D,(int)BC.ES, 0xFF0E,0xFF0F,(int)BC.CS, 0xFF10,0xFF19,(int)BC.EN,
			0xFF1A,0xFF1A,(int)BC.CS, 0xFF1B,0xFF20,(int)BC.ON, 0xFF3B,0xFF40,(int)BC.ON, 0xFF5B,0xFF65,(int)BC.ON, 0xFFE0,0xFFE1,(int)BC.ET, 0xFFE2,0xFFE4,(int)BC.ON,
			0xFFE5,0xFFE6,(int)BC.ET, 0xFFE8,0xFFEE,(int)BC.ON, 0xFFF9,0xFFFD,(int)BC.ON, 0x10800,0x10FFF,(int)BC.R, 0x1D167,0x1D169,(int)BC.NSM, 0x1D173,0x1D17A,(int)BC.BN,
			0x1D17B,0x1D182,(int)BC.NSM, 0x1E800,0x1EDFF,(int)BC.R, 0x1EE00,0x1EEFF,(int)BC.AL, 0x1EF00,0x1EFFF,(int)BC.R, 0x1F000,0x1F0FF,(int)BC.ON, 0x1F100,0x1F10A,(int)BC.EN,
			0x1F10B,0x1F10F,(int)BC.ON, 0x1F12F,0x1F12F,(int)BC.ON, 0x1F16A,0x1F16F,(int)BC.ON, 0x1F1AD,0x1F1AD,(int)BC.ON, 0x1F260,0x1F265,(int)BC.ON, 0x1F300,0x1FBFF,(int)BC.ON,
			0xE0001,0xE007F,(int)BC.BN, 0xE0100,0xE01EF,(int)BC.NSM
		};

		public static BC Bidi(int cp) {
			if (cp >= 'A' && cp <= 'Z' || cp >= 'a' && cp <= 'z') return BC.L;
			int lo = 0, hi = BidiRanges.Length / 3 - 1;
			while (lo <= hi) {
				int mid = (lo + hi) >> 1;
				int s = BidiRanges[mid * 3], e = BidiRanges[mid * 3 + 1];
				if (cp < s) hi = mid - 1;
				else if (cp > e) lo = mid + 1;
				else return (BC)BidiRanges[mid * 3 + 2];
			}
			return BC.L;
		}

		static readonly Dictionary<int, int> Mirrors = BuildMirrors();

		static Dictionary<int, int> BuildMirrors() {
			int[] p = { 0x28, 0x29, 0x3C, 0x3E, 0x5B, 0x5D, 0x7B, 0x7D, 0xAB, 0xBB, 0x2039, 0x203A, 0x2045, 0x2046, 0x207D, 0x207E, 0x208D, 0x208E, 0x2208, 0x220B, 0x2209, 0x220C,
				0x220A, 0x220D, 0x2264, 0x2265, 0x2266, 0x2267, 0x226A, 0x226B, 0x226E, 0x226F, 0x2270, 0x2271, 0x2272, 0x2273, 0x2276, 0x2277, 0x2282, 0x2283, 0x2286, 0x2287,
				0x228A, 0x228B, 0x228F, 0x2290, 0x2291, 0x2292, 0x22A2, 0x22A3, 0x22D6, 0x22D7, 0x2308, 0x2309, 0x230A, 0x230B, 0x2329, 0x232A, 0x2768, 0x2769, 0x276A, 0x276B,
				0x276C, 0x276D, 0x276E, 0x276F, 0x2770, 0x2771, 0x2772, 0x2773, 0x2774, 0x2775, 0x27E6, 0x27E7, 0x27E8, 0x27E9, 0x27EA, 0x27EB, 0x2983, 0x2984, 0x2985, 0x2986,
				0x3008, 0x3009, 0x300A, 0x300B, 0x300C, 0x300D, 0x300E, 0x300F, 0x3010, 0x3011, 0x3014, 0x3015, 0x3016, 0x3017, 0x3018, 0x3019, 0x301A, 0x301B,
				0xFE59, 0xFE5A, 0xFE5B, 0xFE5C, 0xFE5D, 0xFE5E, 0xFE64, 0xFE65, 0xFF08, 0xFF09, 0xFF1C, 0xFF1E, 0xFF3B, 0xFF3D, 0xFF5B, 0xFF5D, 0xFF5F, 0xFF60, 0xFF62, 0xFF63 };
			var d = new Dictionary<int, int>();
			for (int i = 0; i < p.Length; i += 2) { d[p[i]] = p[i + 1]; d[p[i + 1]] = p[i]; }
			return d;
		}

		public static int Mirror(int cp) => Mirrors.TryGetValue(cp, out int m) ? m : cp;

		static readonly int[] BracketOpen = { 0x28, 0x5B, 0x7B, 0x0F3A, 0x0F3C, 0x169B, 0x2045, 0x207D, 0x208D, 0x2308, 0x230A, 0x2329, 0x2768, 0x276A, 0x276C, 0x276E, 0x2770, 0x2772, 0x2774, 0x27E6, 0x27E8, 0x27EA, 0x2983, 0x2985, 0x3008, 0x300A, 0x300C, 0x300E, 0x3010, 0x3014, 0x3016, 0x3018, 0x301A, 0xFE59, 0xFE5B, 0xFE5D, 0xFF08, 0xFF3B, 0xFF5B, 0xFF5F, 0xFF62 };
		static readonly int[] BracketClose = { 0x29, 0x5D, 0x7D, 0x0F3B, 0x0F3D, 0x169C, 0x2046, 0x207E, 0x208E, 0x2309, 0x230B, 0x232A, 0x2769, 0x276B, 0x276D, 0x276F, 0x2771, 0x2773, 0x2775, 0x27E7, 0x27E9, 0x27EB, 0x2984, 0x2986, 0x3009, 0x300B, 0x300D, 0x300F, 0x3011, 0x3015, 0x3017, 0x3019, 0x301B, 0xFE5A, 0xFE5C, 0xFE5E, 0xFF09, 0xFF3D, 0xFF5D, 0xFF60, 0xFF63 };

		public static int BracketType(int cp, out int pair) {
			if (cp == 0x2329) cp = 0x3008; else if (cp == 0x232A) cp = 0x3009;
			int i = Array.IndexOf(BracketOpen, cp);
			if (i >= 0) { pair = BracketClose[i]; return 1; }
			i = Array.IndexOf(BracketClose, cp);
			if (i >= 0) { pair = BracketOpen[i]; return 2; }
			pair = 0;
			return 0;
		}

		public static bool IsMark(int cp) {
			if (cp < 0x300) return false;
			return cp <= 0x36F || cp >= 0x483 && cp <= 0x489 || cp >= 0x591 && cp <= 0x5BD || cp == 0x5BF || cp == 0x5C1 || cp == 0x5C2 || cp == 0x5C4 || cp == 0x5C5 || cp == 0x5C7
				|| cp >= 0x610 && cp <= 0x61A || cp >= 0x64B && cp <= 0x65F || cp == 0x670 || cp >= 0x6D6 && cp <= 0x6DC || cp >= 0x6DF && cp <= 0x6E4 || cp == 0x6E7 || cp == 0x6E8
				|| cp >= 0x6EA && cp <= 0x6ED || cp == 0x711 || cp >= 0x730 && cp <= 0x74A || cp >= 0x7A6 && cp <= 0x7B0 || cp >= 0x7EB && cp <= 0x7F3 || cp >= 0x816 && cp <= 0x82D
				|| cp >= 0x859 && cp <= 0x85B || cp >= 0x8CA && cp <= 0x8E1 || cp >= 0x8E3 && cp <= 0x903 || cp >= 0x93A && cp <= 0x93C || cp >= 0x93E && cp <= 0x94F
				|| cp >= 0x951 && cp <= 0x957 || cp == 0x962 || cp == 0x963 || cp >= 0x981 && cp <= 0x983 || cp == 0x9BC || cp >= 0x9BE && cp <= 0x9CD || cp == 0x9D7
				|| cp >= 0xA01 && cp <= 0xA03 || cp >= 0xA3C && cp <= 0xA51 || cp >= 0xA81 && cp <= 0xA83 || cp >= 0xABC && cp <= 0xACD || cp >= 0xB01 && cp <= 0xB03
				|| cp >= 0xB3C && cp <= 0xB57 || cp >= 0xB82 && cp <= 0xB83 || cp >= 0xBBE && cp <= 0xBD7 || cp >= 0xC00 && cp <= 0xC04 || cp >= 0xC3C && cp <= 0xC56
				|| cp >= 0xC81 && cp <= 0xC83 || cp >= 0xCBC && cp <= 0xCD6 || cp >= 0xD00 && cp <= 0xD03 || cp >= 0xD3B && cp <= 0xD57 || cp >= 0xD81 && cp <= 0xD83
				|| cp >= 0xDCA && cp <= 0xDDF || cp == 0xE31 || cp >= 0xE34 && cp <= 0xE3A || cp >= 0xE47 && cp <= 0xE4E || cp == 0xEB1 || cp >= 0xEB4 && cp <= 0xEBC
				|| cp >= 0xEC8 && cp <= 0xECE || cp >= 0xF18 && cp <= 0xF19 || cp >= 0xF71 && cp <= 0xF84 || cp >= 0x102B && cp <= 0x103E || cp >= 0x17B4 && cp <= 0x17D3
				|| cp >= 0x1AB0 && cp <= 0x1AFF || cp >= 0x1DC0 && cp <= 0x1DFF || cp >= 0x20D0 && cp <= 0x20F0 || cp >= 0x302A && cp <= 0x302F || cp == 0x3099 || cp == 0x309A
				|| cp >= 0xFE00 && cp <= 0xFE0F || cp >= 0xFE20 && cp <= 0xFE2F || cp >= 0x1F3FB && cp <= 0x1F3FF || cp >= 0xE0100 && cp <= 0xE01EF;
		}

		public static bool IsDefaultIgnorable(int cp) =>
			cp == 0xAD || cp == 0x34F || cp == 0x61C || cp >= 0x115F && cp <= 0x1160 || cp >= 0x17B4 && cp <= 0x17B5 || cp >= 0x180B && cp <= 0x180F
			|| cp >= 0x200B && cp <= 0x200F || cp >= 0x202A && cp <= 0x202E || cp >= 0x2060 && cp <= 0x206F || cp == 0x3164 || cp >= 0xFE00 && cp <= 0xFE0F
			|| cp == 0xFEFF || cp == 0xFFA0 || cp >= 0xFFF0 && cp <= 0xFFF8 || cp >= 0x1BCA0 && cp <= 0x1BCA3 || cp >= 0x1D173 && cp <= 0x1D17A || cp >= 0xE0000 && cp <= 0xE0FFF;

		public static bool IsVariationSelector(int cp) => cp >= 0xFE00 && cp <= 0xFE0F || cp >= 0xE0100 && cp <= 0xE01EF;

		public static Sc Script(int cp) {
			if (cp < 0x41) return Sc.Common;
			if (cp <= 0x24F) {
				if (cp <= 0x7A) return cp >= 0x61 || cp >= 0x41 && cp <= 0x5A ? Sc.Latin : Sc.Common;
				if (cp < 0xC0) return cp == 0xAA || cp == 0xBA ? Sc.Latin : Sc.Common;
				return cp == 0xD7 || cp == 0xF7 ? Sc.Common : Sc.Latin;
			}
			if (cp < 0x2B0) return Sc.Latin;
			if (cp < 0x300) return Sc.Common;
			if (cp < 0x370) return Sc.Inherited;
			if (cp < 0x400) return cp == 0x37E || cp == 0x387 ? Sc.Common : Sc.Greek;
			if (cp < 0x530) return cp >= 0x483 && cp <= 0x489 ? Sc.Inherited : Sc.Cyrillic;
			if (cp < 0x590) return cp == 0x589 ? Sc.Common : Sc.Armenian;
			if (cp < 0x600) return Sc.Hebrew;
			if (cp < 0x700) {
				if (cp == 0x60C || cp == 0x61B || cp == 0x61F || cp == 0x640 || cp == 0x6DD) return Sc.Common;
				if (cp >= 0x64B && cp <= 0x655 || cp == 0x670) return Sc.Inherited;
				if (cp >= 0x660 && cp <= 0x669) return Sc.Arabic;
				return Sc.Arabic;
			}
			if (cp < 0x750) return Sc.Syriac;
			if (cp < 0x780) return Sc.Arabic;
			if (cp < 0x7C0) return Sc.Thaana;
			if (cp < 0x800) return Sc.Nko;
			if (cp < 0x870) return Sc.Other;
			if (cp < 0x900) return Sc.Arabic;
			if (cp < 0x980) return cp == 0x964 || cp == 0x965 ? Sc.Common : Sc.Devanagari;
			if (cp < 0xA00) return Sc.Bengali;
			if (cp < 0xA80) return Sc.Gurmukhi;
			if (cp < 0xB00) return Sc.Gujarati;
			if (cp < 0xB80) return Sc.Oriya;
			if (cp < 0xC00) return Sc.Tamil;
			if (cp < 0xC80) return Sc.Telugu;
			if (cp < 0xD00) return Sc.Kannada;
			if (cp < 0xD80) return Sc.Malayalam;
			if (cp < 0xE00) return Sc.Sinhala;
			if (cp < 0xE80) return cp == 0xE3F ? Sc.Common : Sc.Thai;
			if (cp < 0xF00) return Sc.Lao;
			if (cp < 0x1000) return Sc.Tibetan;
			if (cp < 0x10A0) return Sc.Myanmar;
			if (cp < 0x1100) return Sc.Georgian;
			if (cp < 0x1200) return Sc.Hangul;
			if (cp < 0x13A0) return Sc.Ethiopic;
			if (cp >= 0x1780 && cp < 0x1800) return Sc.Khmer;
			if (cp >= 0x1800 && cp < 0x18B0) return Sc.Mongolian;
			if (cp >= 0x1AB0 && cp < 0x1B00) return Sc.Inherited;
			if (cp >= 0x1C90 && cp < 0x1CC0) return Sc.Georgian;
			if (cp >= 0x1D00 && cp < 0x1DC0) return Sc.Latin;
			if (cp >= 0x1DC0 && cp < 0x1E00) return Sc.Inherited;
			if (cp >= 0x1E00 && cp < 0x1F00) return Sc.Latin;
			if (cp >= 0x1F00 && cp < 0x2000) return Sc.Greek;
			if (cp >= 0x2000 && cp < 0x2070) return cp == 0x200C || cp == 0x200D ? Sc.Inherited : Sc.Common;
			if (cp >= 0x20D0 && cp < 0x2100) return Sc.Inherited;
			if (cp >= 0x2C60 && cp < 0x2C80) return Sc.Latin;
			if (cp >= 0x2D00 && cp < 0x2D30) return Sc.Georgian;
			if (cp >= 0x2DE0 && cp < 0x2E00) return Sc.Cyrillic;
			if (cp >= 0x2000 && cp < 0x2E80) return Sc.Symbol;
			if (cp >= 0x2E80 && cp < 0x3000) return Sc.Han;
			if (cp >= 0x3000 && cp < 0x3040) return cp == 0x3005 || cp == 0x3007 || cp >= 0x3021 && cp <= 0x3029 || cp >= 0x3038 && cp <= 0x303B ? Sc.Han : cp >= 0x302A && cp <= 0x302D ? Sc.Inherited : Sc.Common;
			if (cp >= 0x3040 && cp < 0x30A0) return cp == 0x3099 || cp == 0x309A ? Sc.Inherited : cp == 0x309B || cp == 0x309C ? Sc.Common : Sc.Hiragana;
			if (cp >= 0x30A0 && cp < 0x3100) return cp == 0x30A0 || cp == 0x30FB || cp == 0x30FC ? Sc.Common : Sc.Katakana;
			if (cp >= 0x3100 && cp < 0x3130) return Sc.Bopomofo;
			if (cp >= 0x3130 && cp < 0x3190) return Sc.Hangul;
			if (cp >= 0x31A0 && cp < 0x31C0) return Sc.Bopomofo;
			if (cp >= 0x31F0 && cp < 0x3200) return Sc.Katakana;
			if (cp >= 0x3200 && cp < 0x3400) return Sc.Common;
			if (cp >= 0x3400 && cp < 0xA000) return cp >= 0x4DC0 && cp < 0x4E00 ? Sc.Common : Sc.Han;
			if (cp >= 0xA640 && cp < 0xA6A0) return Sc.Cyrillic;
			if (cp >= 0xA720 && cp < 0xA800) return Sc.Latin;
			if (cp >= 0xA960 && cp < 0xA980) return Sc.Hangul;
			if (cp >= 0xAC00 && cp < 0xD7FF) return Sc.Hangul;
			if (cp >= 0xF900 && cp < 0xFB00) return Sc.Han;
			if (cp >= 0xFB00 && cp < 0xFB07) return Sc.Latin;
			if (cp >= 0xFB13 && cp < 0xFB18) return Sc.Armenian;
			if (cp >= 0xFB1D && cp < 0xFB50) return Sc.Hebrew;
			if (cp >= 0xFB50 && cp < 0xFE00) return cp == 0xFD3E || cp == 0xFD3F ? Sc.Common : Sc.Arabic;
			if (cp >= 0xFE00 && cp < 0xFE10) return Sc.Inherited;
			if (cp >= 0xFE20 && cp < 0xFE30) return Sc.Inherited;
			if (cp >= 0xFE70 && cp < 0xFEFF) return Sc.Arabic;
			if (cp >= 0xFF21 && cp <= 0xFF3A || cp >= 0xFF41 && cp <= 0xFF5A) return Sc.Latin;
			if (cp >= 0xFF66 && cp <= 0xFF9D) return Sc.Katakana;
			if (cp >= 0xFFA0 && cp <= 0xFFDC) return Sc.Hangul;
			if (cp >= 0xFF00 && cp < 0x10000) return Sc.Common;
			if (cp >= 0x1EE00 && cp < 0x1EF00) return Sc.Arabic;
			if (cp >= 0x1F000 && cp < 0x1FB00) return Sc.Emoji;
			if (cp >= 0x20000 && cp < 0x323B0) return Sc.Han;
			if (cp >= 0xE0100 && cp < 0xE01F0) return Sc.Inherited;
			return Sc.Other;
		}

		public static string OtScriptTag(Sc s) => s switch {
			Sc.Latin => "latn", Sc.Greek => "grek", Sc.Cyrillic => "cyrl", Sc.Armenian => "armn", Sc.Hebrew => "hebr", Sc.Arabic => "arab", Sc.Syriac => "syrc",
			Sc.Thaana => "thaa", Sc.Nko => "nko ", Sc.Devanagari => "dev2", Sc.Bengali => "bng2", Sc.Gurmukhi => "gur2", Sc.Gujarati => "gjr2", Sc.Oriya => "ory2",
			Sc.Tamil => "tml2", Sc.Telugu => "tel2", Sc.Kannada => "knd2", Sc.Malayalam => "mlm2", Sc.Sinhala => "sinh", Sc.Thai => "thai", Sc.Lao => "lao ",
			Sc.Tibetan => "tibt", Sc.Myanmar => "mym2", Sc.Georgian => "geor", Sc.Hangul => "hang", Sc.Ethiopic => "ethi", Sc.Khmer => "khmr", Sc.Mongolian => "mong",
			Sc.Han => "hani", Sc.Hiragana => "kana", Sc.Katakana => "kana", Sc.Bopomofo => "bopo", _ => "DFLT"
		};

		public static string OtScriptTagOld(Sc s) => s switch {
			Sc.Devanagari => "deva", Sc.Bengali => "beng", Sc.Gurmukhi => "guru", Sc.Gujarati => "gujr", Sc.Oriya => "orya", Sc.Tamil => "taml", Sc.Telugu => "telu",
			Sc.Kannada => "knda", Sc.Malayalam => "mlym", Sc.Myanmar => "mymr", _ => null
		};

		public static string OtLangTag(string lang) {
			if (string.IsNullOrEmpty(lang)) return null;
			string l = lang.ToLowerInvariant();
			int dash = l.IndexOf('-');
			if (dash > 0) l = l.Substring(0, dash);
			return l switch {
				"fa" => "FAR ", "ar" => "ARA ", "ur" => "URD ", "ps" => "PAS ", "ku" => "KUR ", "sd" => "SND ", "ug" => "UYG ", "he" => "IWR ", "yi" => "JII ",
				"en" => "ENG ", "de" => "DEU ", "fr" => "FRA ", "tr" => "TRK ", "ru" => "RUS ", "zh" => "ZHS ", "ja" => "JAN ", "ko" => "KOR ", "vi" => "VIT ",
				"az" => "AZE ", "ro" => "ROM ", "pl" => "PLK ", "nl" => "NLD ", "sr" => "SRB ", "bg" => "BGR ", "mk" => "MKD ", "hi" => "HIN ", "th" => "THA ", _ => null
			};
		}

		public enum JT : byte { U, R, D, C, T, L }

		public static JT Joining(int cp) {
			if (cp == 0x200D) return JT.C;
			if (cp == 0x200C) return JT.U;
			if (cp < 0x600 || cp > 0x8FF) {
				if (IsMark(cp)) return JT.T;
				if (cp >= 0x10AC0 && cp < 0x10B00) return JT.D;
				return JT.U;
			}
			if (IsMark(cp)) return JT.T;
			if (cp >= 0x610 && cp <= 0x61A || cp >= 0x64B && cp <= 0x65F || cp == 0x670 || cp >= 0x6D6 && cp <= 0x6DC || cp >= 0x6DF && cp <= 0x6E4 || cp == 0x6E7 || cp == 0x6E8 || cp >= 0x6EA && cp <= 0x6ED) return JT.T;
			if (cp >= 0x700 && cp <= 0x74F) {
				if (cp == 0x70F) return JT.T;
				if (cp == 0x710 || cp >= 0x715 && cp <= 0x719 || cp == 0x71E || cp == 0x728 || cp == 0x72A || cp == 0x72C || cp == 0x72F || cp == 0x74D) return JT.R;
				if (cp >= 0x711 && cp <= 0x72F || cp >= 0x74E) return JT.D;
				return JT.U;
			}
			if (cp >= 0x780 && cp <= 0x7BF) return JT.U;
			if (cp >= 0x7C0 && cp <= 0x7FF) return cp >= 0x7CA && cp <= 0x7EA ? JT.D : cp == 0x7FA ? JT.C : JT.U;
			if (cp >= 0x800 && cp <= 0x85F) return cp >= 0x840 && cp <= 0x858 ? JT.D : JT.U;
			switch (cp) {
				case 0x620: case 0x626: case 0x628: return JT.D;
				case 0x621: return JT.U;
				case 0x622: case 0x623: case 0x624: case 0x625: case 0x627: case 0x629: return JT.R;
				case 0x640: return JT.C;
				case 0x648: return JT.R;
				case 0x649: case 0x64A: return JT.D;
				case 0x674: return JT.U;
				case 0x6C0: return JT.R;
				case 0x6C1: case 0x6C2: return JT.D;
				case 0x6CC: return JT.D;
				case 0x6CD: return JT.R;
				case 0x6CE: return JT.D;
				case 0x6CF: return JT.R;
				case 0x6D0: case 0x6D1: return JT.D;
				case 0x6D2: case 0x6D3: case 0x6D5: return JT.R;
				case 0x6DD: return JT.U;
				case 0x6EE: case 0x6EF: return JT.R;
				case 0x6FF: return JT.D;
				case 0x759: case 0x75A: case 0x75B: case 0x76B: case 0x76C: case 0x771: case 0x773: case 0x774: case 0x778: case 0x779: return JT.R;
			}
			if (cp >= 0x62A && cp <= 0x62E) return JT.D;
			if (cp >= 0x62F && cp <= 0x632) return JT.R;
			if (cp >= 0x633 && cp <= 0x63F) return JT.D;
			if (cp >= 0x641 && cp <= 0x647) return JT.D;
			if (cp == 0x66E || cp == 0x66F) return JT.D;
			if (cp >= 0x671 && cp <= 0x673 || cp >= 0x675 && cp <= 0x677) return JT.R;
			if (cp >= 0x678 && cp <= 0x687) return JT.D;
			if (cp >= 0x688 && cp <= 0x699) return JT.R;
			if (cp >= 0x69A && cp <= 0x6BF) return JT.D;
			if (cp >= 0x6C3 && cp <= 0x6CB) return JT.R;
			if (cp >= 0x6FA && cp <= 0x6FC) return JT.D;
			if (cp >= 0x750 && cp <= 0x77F) return JT.D;
			if (cp >= 0x860 && cp <= 0x86A) return cp == 0x867 || cp == 0x869 ? JT.R : JT.D;
			if (cp >= 0x870 && cp <= 0x882) return cp <= 0x88E ? JT.R : JT.D;
			if (cp >= 0x8A0 && cp <= 0x8C8) {
				if (cp == 0x8AA || cp == 0x8AB || cp == 0x8AC || cp == 0x8AE || cp == 0x8B1 || cp == 0x8B2 || cp == 0x8B9) return JT.R;
				if (cp == 0x8AD) return JT.U;
				return JT.D;
			}
			return JT.U;
		}

		public static LBC LineBreak(int cp) {
			switch (cp) {
				case 0x0A: return LBC.LF;
				case 0x0D: return LBC.CR;
				case 0x0B: case 0x0C: case 0x85: case 0x2028: case 0x2029: return LBC.BK;
				case 0x20: return LBC.SP;
				case 0x09: return LBC.BA;
				case 0x200B: return LBC.ZW;
				case 0x200D: return LBC.ZWJ;
				case 0x200C: return LBC.CM;
				case 0xA0: case 0x202F: case 0x2007: case 0x2011: case 0x0F0C: case 0x180E: case 0x034F: return LBC.GL;
				case 0x2060: case 0xFEFF: return LBC.WJ;
				case 0x2D: return LBC.HY;
				case 0x21: case 0x3F: case 0x5C6: case 0x61B: case 0x61E: case 0x61F: case 0x6D4: case 0x7F9: case 0xFE15: case 0xFE16: case 0xFF01: case 0xFF1F: return LBC.EX;
				case 0x2C: case 0x2E: case 0x3A: case 0x3B: case 0x37E: case 0x589: case 0x60C: case 0x60D: case 0x7F8: case 0x2044: case 0xFE10: case 0xFE13: case 0xFE14: return LBC.IS;
				case 0x2F: return LBC.SY;
				case 0x28: case 0x5B: case 0x7B: case 0xA1: case 0xBF: case 0x201A: case 0x201E: case 0x2045: case 0x207D: case 0x208D: case 0x2329: case 0x2768: case 0x276A:
				case 0x276C: case 0x276E: case 0x2770: case 0x2772: case 0x2774: case 0x27E6: case 0x27E8: case 0x27EA: case 0x3008: case 0x300A: case 0x300C: case 0x300E:
				case 0x3010: case 0x3014: case 0x3016: case 0x3018: case 0x301A: case 0x301D: case 0xFE59: case 0xFE5B: case 0xFE5D: case 0xFF08: case 0xFF3B: case 0xFF5B:
				case 0xFF5F: case 0xFF62: return LBC.OP;
				case 0x29: case 0x5D: return LBC.CP;
				case 0x7D: case 0x2046: case 0x207E: case 0x208E: case 0x232A: case 0x2769: case 0x276B: case 0x276D: case 0x276F: case 0x2771: case 0x2773: case 0x2775:
				case 0x27E7: case 0x27E9: case 0x27EB: case 0x3001: case 0x3002: case 0x3009: case 0x300B: case 0x300D: case 0x300F: case 0x3011: case 0x3015: case 0x3017:
				case 0x3019: case 0x301B: case 0x301E: case 0x301F: case 0xFE11: case 0xFE12: case 0xFE5A: case 0xFE5C: case 0xFE5E: case 0xFF09: case 0xFF0C: case 0xFF0E:
				case 0xFF3D: case 0xFF5D: case 0xFF60: case 0xFF61: case 0xFF63: case 0xFF64: return LBC.CL;
				case 0x22: case 0x27: case 0xAB: case 0xBB: case 0x2018: case 0x2019: case 0x201B: case 0x201C: case 0x201D: case 0x201F: case 0x2039: case 0x203A:
				case 0x275B: case 0x275C: case 0x275D: case 0x275E: return LBC.QU;
				case 0x24: case 0x2B: case 0x5C: case 0xA3: case 0xA4: case 0xA5: case 0xB1: case 0x2116: case 0x2212: case 0x2213: case 0xFE69: case 0xFF04: case 0xFFE1:
				case 0xFFE5: case 0xFFE6: return LBC.PR;
				case 0x25: case 0xA2: case 0xB0: case 0x60B: case 0x66A: case 0x2030: case 0x2031: case 0x2032: case 0x2033: case 0x2034: case 0x2035: case 0x2036: case 0x2037:
				case 0x20A7: case 0x2103: case 0x2109: case 0xFDFC: case 0xFE6A: case 0xFF05: case 0xFFE0: return LBC.PO;
				case 0x7C: case 0xAD: case 0x58A: case 0x5BE: case 0x1680: case 0x2010: case 0x2012: case 0x2013: case 0x2027: case 0x205F: case 0x3000: return LBC.BA;
				case 0xB4: case 0x2C8: case 0x2CC: case 0x2DF: case 0x1FFD: return LBC.BB;
				case 0x2024: case 0x2025: case 0x2026: case 0xFE19: return LBC.IN;
				case 0x17D6: case 0x203C: case 0x203D: case 0x2047: case 0x2048: case 0x2049: case 0x3005: case 0x301C: case 0x303B: case 0x303C: case 0x309B: case 0x309C:
				case 0x309D: case 0x309E: case 0x30A0: case 0x30FB: case 0x30FC: case 0x30FD: case 0x30FE: case 0xA015: case 0xFE54: case 0xFE55: case 0xFF1A: case 0xFF1B:
				case 0xFF65: case 0xFF70: case 0xFF9E: case 0xFF9F: return LBC.NS;
				case 0xFFFC: return LBC.CB;
				case 0x2014: return LBC.B2;
			}
			if (cp >= 0x2000 && cp <= 0x2006 || cp >= 0x2008 && cp <= 0x200A) return LBC.BA;
			if (cp >= 0x30 && cp <= 0x39 || cp >= 0x660 && cp <= 0x669 || cp == 0x66B || cp == 0x66C || cp >= 0x6F0 && cp <= 0x6F9 || cp >= 0x7C0 && cp <= 0x7C9
				|| cp >= 0x966 && cp <= 0x96F || cp >= 0x9E6 && cp <= 0x9EF || cp >= 0xE50 && cp <= 0xE59) return LBC.NU;
			if (cp < 0x20 || cp >= 0x7F && cp < 0xA0) return LBC.CM;
			if (IsMark(cp)) return LBC.CM;
			if (cp >= 0x20A0 && cp <= 0x20CF) return LBC.PR;
			if (cp >= 0x1F1E6 && cp <= 0x1F1FF) return LBC.RI;
			if (cp >= 0x3041 && cp <= 0x3096) {
				switch (cp) { case 0x3041: case 0x3043: case 0x3045: case 0x3047: case 0x3049: case 0x3063: case 0x3083: case 0x3085: case 0x3087: case 0x308E: case 0x3095: case 0x3096: return LBC.NS; }
				return LBC.ID;
			}
			if (cp >= 0x30A1 && cp <= 0x30FA) {
				switch (cp) { case 0x30A1: case 0x30A3: case 0x30A5: case 0x30A7: case 0x30A9: case 0x30C3: case 0x30E3: case 0x30E5: case 0x30E7: case 0x30EE: case 0x30F5: case 0x30F6: return LBC.NS; }
				return LBC.ID;
			}
			if (cp >= 0x1100 && cp <= 0x115F || cp >= 0xA960 && cp <= 0xA97C) return LBC.JL;
			if (cp >= 0x1160 && cp <= 0x11A7 || cp >= 0xD7B0 && cp <= 0xD7C6) return LBC.JV;
			if (cp >= 0x11A8 && cp <= 0x11FF || cp >= 0xD7CB && cp <= 0xD7FB) return LBC.JT;
			if (cp >= 0xAC00 && cp <= 0xD7A3) return (cp - 0xAC00) % 28 == 0 ? LBC.H2 : LBC.H3;
			if (cp >= 0x2E80 && cp <= 0x2FFF || cp >= 0x3003 && cp <= 0x3004 || cp >= 0x3006 && cp <= 0x3007 || cp >= 0x3012 && cp <= 0x3013 || cp >= 0x3020 && cp <= 0x3029
				|| cp >= 0x3030 && cp <= 0x303A || cp >= 0x303D && cp <= 0x303F || cp >= 0x3097 && cp <= 0x309F || cp >= 0x3105 && cp <= 0x31FF || cp >= 0x3200 && cp <= 0x4DBF
				|| cp >= 0x4E00 && cp <= 0x9FFF || cp >= 0xA000 && cp <= 0xA4CF || cp >= 0xF900 && cp <= 0xFAFF || cp >= 0xFE30 && cp <= 0xFE4F || cp >= 0xFF02 && cp <= 0xFF07
				|| cp >= 0xFF0A && cp <= 0xFF0B || cp == 0xFF0D || cp >= 0xFF0F && cp <= 0xFF19 || cp >= 0xFF1C && cp <= 0xFF1E || cp >= 0xFF20 && cp <= 0xFF3A || cp >= 0xFF3C && cp <= 0xFF5A
				|| cp >= 0xFFE2 && cp <= 0xFFE4 || cp >= 0x1F000 && cp <= 0x1FAFF || cp >= 0x20000 && cp <= 0x3FFFD) {
				if (cp >= 0x1F3FB && cp <= 0x1F3FF) return LBC.CM;
				return LBC.ID;
			}
			if (cp >= 0x0E00 && cp <= 0x0EFF || cp >= 0x1000 && cp <= 0x109F || cp >= 0x1780 && cp <= 0x17FF || cp >= 0x1950 && cp <= 0x19DF || cp >= 0x1A20 && cp <= 0x1AAF) return LBC.SA;
			return LBC.AL;
		}

		public static bool IsSpaceLike(int cp) => cp == ' ' || cp == 0xA0 || cp == 0x1680 || cp >= 0x2000 && cp <= 0x200A || cp == 0x202F || cp == 0x205F || cp == 0x3000;

		public static bool IsWide(int cp) => cp >= 0x1100 && cp <= 0x115F || cp >= 0x2E80 && cp <= 0xA4CF || cp >= 0xAC00 && cp <= 0xD7A3 || cp >= 0xF900 && cp <= 0xFAFF
			|| cp >= 0xFE30 && cp <= 0xFE4F || cp >= 0xFF00 && cp <= 0xFF60 || cp >= 0xFFE0 && cp <= 0xFFE6 || cp >= 0x20000 && cp <= 0x3FFFD;

		public static int CodePointAt(string s, int i) {
			char c = s[i];
			if (char.IsHighSurrogate(c) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1])) return char.ConvertToUtf32(c, s[i + 1]);
			return c;
		}
	}

	internal static class BidiAlgo {
		struct Stk { public byte Level; public sbyte Override; public bool Isolate; }

		public static byte ParagraphLevel(BC[] t, int start, int end) {
			int depth = 0;
			for (int i = start; i < end; i++) {
				BC c = t[i];
				if (c == BC.LRI || c == BC.RLI || c == BC.FSI) depth++;
				else if (c == BC.PDI) { if (depth > 0) depth--; }
				else if (depth == 0) {
					if (c == BC.L) return 0;
					if (c == BC.R || c == BC.AL) return 1;
				}
				else if (c == BC.B) break;
			}
			return 0;
		}

		static bool Removed(BC c) => c == BC.RLE || c == BC.LRE || c == BC.RLO || c == BC.LRO || c == BC.PDF || c == BC.BN;
		static bool IsIsoInit(BC c) => c == BC.LRI || c == BC.RLI || c == BC.FSI;
		static bool IsNI(BC c) => c == BC.B || c == BC.S || c == BC.WS || c == BC.ON || c == BC.LRI || c == BC.RLI || c == BC.FSI || c == BC.PDI;

		public static byte[] Resolve(int[] cps, BC[] initial, byte paraLevel) {
			int n = initial.Length;
			var levels = new byte[n];
			var types = (BC[])initial.Clone();
			if (n == 0) return levels;
			var matchingPDI = new int[n];
			var matchingIso = new int[n];
			for (int i = 0; i < n; i++) { matchingPDI[i] = -1; matchingIso[i] = -1; }
			{
				var st = new Stack<int>();
				for (int i = 0; i < n; i++) {
					if (IsIsoInit(types[i])) st.Push(i);
					else if (types[i] == BC.PDI) { if (st.Count > 0) { int o = st.Pop(); matchingPDI[o] = i; matchingIso[i] = o; } }
					else if (types[i] == BC.B) st.Clear();
				}
			}
			var stack = new Stk[127];
			int sp = 0;
			stack[sp++] = new Stk { Level = paraLevel, Override = -1, Isolate = false };
			int overflowIso = 0, overflowEmb = 0, validIso = 0;
			for (int i = 0; i < n; i++) {
				BC t = types[i];
				switch (t) {
					case BC.RLE: case BC.LRE: case BC.RLO: case BC.LRO: {
						bool rtl = t == BC.RLE || t == BC.RLO;
						byte cur = stack[sp - 1].Level;
						byte nl = (byte)(rtl ? (cur + 1) | 1 : (cur + 2) & ~1);
						levels[i] = cur;
						if (nl <= 125 && overflowIso == 0 && overflowEmb == 0) stack[sp++] = new Stk { Level = nl, Override = (sbyte)(t == BC.RLO ? 1 : t == BC.LRO ? 0 : -1), Isolate = false };
						else if (overflowIso == 0) overflowEmb++;
						break;
					}
					case BC.RLI: case BC.LRI: case BC.FSI: {
						bool rtl = t == BC.RLI;
						if (t == BC.FSI) {
							int end = matchingPDI[i] >= 0 ? matchingPDI[i] : n;
							rtl = ParagraphLevel(types, i + 1, end) == 1;
						}
						levels[i] = stack[sp - 1].Level;
						if (stack[sp - 1].Override >= 0) types[i] = stack[sp - 1].Override == 1 ? BC.R : BC.L;
						byte cur = stack[sp - 1].Level;
						byte nl = (byte)(rtl ? (cur + 1) | 1 : (cur + 2) & ~1);
						if (nl <= 125 && overflowIso == 0 && overflowEmb == 0) { validIso++; stack[sp++] = new Stk { Level = nl, Override = -1, Isolate = true }; }
						else overflowIso++;
						break;
					}
					case BC.PDI:
						if (overflowIso > 0) overflowIso--;
						else if (validIso > 0) {
							overflowEmb = 0;
							while (!stack[sp - 1].Isolate) sp--;
							sp--;
							validIso--;
						}
						levels[i] = stack[sp - 1].Level;
						if (stack[sp - 1].Override >= 0) types[i] = stack[sp - 1].Override == 1 ? BC.R : BC.L;
						break;
					case BC.PDF:
						levels[i] = stack[sp - 1].Level;
						if (overflowIso == 0 && overflowEmb > 0) overflowEmb--;
						else if (overflowIso == 0 && !stack[sp - 1].Isolate && sp >= 2) sp--;
						break;
					case BC.B:
						levels[i] = paraLevel;
						sp = 1; overflowIso = 0; overflowEmb = 0; validIso = 0;
						break;
					case BC.BN:
						levels[i] = stack[sp - 1].Level;
						break;
					default:
						levels[i] = stack[sp - 1].Level;
						if (stack[sp - 1].Override >= 0) types[i] = stack[sp - 1].Override == 1 ? BC.R : BC.L;
						break;
				}
			}

			var runs = new List<(int s, int e)>();
			{
				int i = 0;
				while (i < n) {
					while (i < n && Removed(types[i])) i++;
					if (i >= n) break;
					int s = i;
					byte lv = levels[i];
					int last = i;
					i++;
					while (i < n) {
						if (Removed(types[i])) { i++; continue; }
						if (levels[i] != lv) break;
						last = i;
						i++;
					}
					runs.Add((s, last + 1));
				}
			}
			var runOf = new int[n];
			for (int r = 0; r < runs.Count; r++) for (int k = runs[r].s; k < runs[r].e; k++) runOf[k] = r;
			var seqs = new List<List<int>>();
			var used = new bool[runs.Count];
			for (int r = 0; r < runs.Count; r++) {
				if (used[r]) continue;
				int first = FirstNonRemoved(types, runs[r].s, runs[r].e);
				if (first >= 0 && types[first] == BC.PDI && matchingIso[first] >= 0) continue;
				var seq = new List<int>();
				int cur = r;
				while (true) {
					used[cur] = true;
					for (int k = runs[cur].s; k < runs[cur].e; k++) if (!Removed(types[k])) seq.Add(k);
					int lastC = seq.Count > 0 ? seq[seq.Count - 1] : -1;
					if (lastC >= 0 && IsIsoInit(types[lastC]) && matchingPDI[lastC] >= 0) {
						int nr = runOf[matchingPDI[lastC]];
						if (nr == cur || used[nr]) break;
						cur = nr;
					}
					else break;
				}
				if (seq.Count > 0) seqs.Add(seq);
			}
			foreach (var seq in seqs) ResolveSequence(seq, types, levels, initial, cps, paraLevel, matchingPDI, n);

			for (int i = 0; i < n; i++) {
				if (Removed(types[i])) levels[i] = i > 0 ? levels[i - 1] : paraLevel;
			}
			return levels;
		}

		static int FirstNonRemoved(BC[] t, int s, int e) {
			for (int i = s; i < e; i++) if (!Removed(t[i])) return i;
			return -1;
		}

		static BC Dir(int level) => (level & 1) == 1 ? BC.R : BC.L;

		static void ResolveSequence(List<int> seq, BC[] types, byte[] levels, BC[] initial, int[] cps, byte para, int[] matchingPDI, int n) {
			int cnt = seq.Count;
			byte lvl = levels[seq[0]];
			int prev = seq[0] - 1;
			while (prev >= 0 && Removed(types[prev])) prev--;
			byte prevLevel = prev >= 0 ? levels[prev] : para;
			BC sos = Dir(Math.Max(lvl, prevLevel));
			int lastIdx = seq[cnt - 1];
			byte nextLevel;
			if (IsIsoInit(types[lastIdx])) nextLevel = para;
			else {
				int nx = lastIdx + 1;
				while (nx < n && Removed(types[nx])) nx++;
				nextLevel = nx < n ? levels[nx] : para;
			}
			BC eos = Dir(Math.Max(levels[lastIdx], nextLevel));
			var t = new BC[cnt];
			for (int i = 0; i < cnt; i++) t[i] = types[seq[i]];

			for (int i = 0; i < cnt; i++) {
				if (t[i] == BC.NSM) {
					if (i == 0) t[i] = sos;
					else t[i] = IsIsoInit(t[i - 1]) || t[i - 1] == BC.PDI ? BC.ON : t[i - 1];
				}
			}
			BC lastStrong = sos;
			for (int i = 0; i < cnt; i++) {
				BC c = t[i];
				if (c == BC.L || c == BC.R || c == BC.AL) lastStrong = c;
				else if (c == BC.EN && lastStrong == BC.AL) t[i] = BC.AN;
			}
			for (int i = 0; i < cnt; i++) if (t[i] == BC.AL) t[i] = BC.R;
			for (int i = 1; i < cnt - 1; i++) {
				if (t[i] == BC.ES && t[i - 1] == BC.EN && t[i + 1] == BC.EN) t[i] = BC.EN;
				else if (t[i] == BC.CS && t[i - 1] == BC.EN && t[i + 1] == BC.EN) t[i] = BC.EN;
				else if (t[i] == BC.CS && t[i - 1] == BC.AN && t[i + 1] == BC.AN) t[i] = BC.AN;
			}
			for (int i = 0; i < cnt; i++) {
				if (t[i] != BC.ET) continue;
				int s = i;
				while (i < cnt && t[i] == BC.ET) i++;
				bool en = s > 0 && t[s - 1] == BC.EN || i < cnt && t[i] == BC.EN;
				if (en) for (int k = s; k < i; k++) t[k] = BC.EN;
				i--;
			}
			for (int i = 0; i < cnt; i++) if (t[i] == BC.ES || t[i] == BC.ET || t[i] == BC.CS) t[i] = BC.ON;
			lastStrong = sos;
			for (int i = 0; i < cnt; i++) {
				BC c = t[i];
				if (c == BC.L || c == BC.R) lastStrong = c;
				else if (c == BC.EN && lastStrong == BC.L) t[i] = BC.L;
			}

			BC e = Dir(lvl);
			var pairs = new List<(int o, int c)>();
			var bs = new List<(int pos, int closeCp)>();
			for (int i = 0; i < cnt; i++) {
				if (t[i] != BC.ON) continue;
				int cp = cps[seq[i]];
				int bt = Uni.BracketType(cp, out int pr);
				if (bt == 1) {
					if (bs.Count >= 63) break;
					bs.Add((i, pr));
				}
				else if (bt == 2) {
					int ncp = cp == 0x232A ? 0x3009 : cp;
					for (int k = bs.Count - 1; k >= 0; k--) {
						int want = bs[k].closeCp;
						if (want == ncp || want == 0x232A && ncp == 0x3009) {
							pairs.Add((bs[k].pos, i));
							bs.RemoveRange(k, bs.Count - k);
							break;
						}
					}
				}
			}
			pairs.Sort((a, b) => a.o.CompareTo(b.o));
			foreach (var (o, c) in pairs) {
				bool foundE = false, foundO = false;
				for (int k = o + 1; k < c; k++) {
					BC d = StrongOf(t[k]);
					if (d == BC.ON) continue;
					if (d == e) { foundE = true; break; }
					foundO = true;
				}
				BC set = BC.ON;
				if (foundE) set = e;
				else if (foundO) {
					BC before = sos;
					for (int k = o - 1; k >= 0; k--) {
						BC d = StrongOf(t[k]);
						if (d != BC.ON) { before = d; break; }
					}
					set = before != e ? before : e;
				}
				if (set != BC.ON) {
					t[o] = set; t[c] = set;
					for (int k = o + 1; k < cnt && initial[seq[k]] == BC.NSM; k++) t[k] = set;
					for (int k = c + 1; k < cnt && initial[seq[k]] == BC.NSM; k++) t[k] = set;
				}
			}

			for (int i = 0; i < cnt; i++) {
				if (!IsNI(t[i])) continue;
				int s = i;
				while (i < cnt && IsNI(t[i])) i++;
				BC before = s == 0 ? sos : StrongOrNum(t[s - 1]);
				BC after = i >= cnt ? eos : StrongOrNum(t[i]);
				BC res = before == after ? before : e;
				for (int k = s; k < i; k++) t[k] = res;
				i--;
			}
			for (int i = 0; i < cnt; i++) {
				int idx = seq[i];
				byte lv = levels[idx];
				BC c = t[i];
				if ((lv & 1) == 0) {
					if (c == BC.R) levels[idx] = (byte)(lv + 1);
					else if (c == BC.AN || c == BC.EN) levels[idx] = (byte)(lv + 2);
				}
				else if (c == BC.L || c == BC.EN || c == BC.AN) levels[idx] = (byte)(lv + 1);
			}
		}

		static BC StrongOf(BC c) => c == BC.L ? BC.L : c == BC.R || c == BC.AL || c == BC.EN || c == BC.AN ? BC.R : BC.ON;
		static BC StrongOrNum(BC c) => c == BC.L ? BC.L : c == BC.EN || c == BC.AN || c == BC.R || c == BC.AL ? BC.R : c;

		public static int[] VisualOrder(byte[] levels) {
			int n = levels.Length;
			var idx = new int[n];
			for (int i = 0; i < n; i++) idx[i] = i;
			if (n == 0) return idx;
			byte max = 0, minOdd = 255;
			foreach (byte l in levels) { if (l > max) max = l; if ((l & 1) == 1 && l < minOdd) minOdd = l; }
			for (int lv = max; lv >= minOdd && lv > 0; lv--) {
				int i = 0;
				while (i < n) {
					if (LevelAt(levels, idx, i) >= lv) {
						int s = i;
						while (i < n && LevelAt(levels, idx, i) >= lv) i++;
						Array.Reverse(idx, s, i - s);
					}
					else i++;
				}
			}
			return idx;
		}

		static byte LevelAt(byte[] levels, int[] idx, int i) => levels[idx[i]];
	}

	internal sealed class Gdef {
		readonly byte[] d;
		readonly int classDef, markAttach, markSets;
		public bool HasClasses => classDef > 0;

		public Gdef(byte[] data, int off) {
			d = data;
			int c = BE.U16(d, off + 4);
			classDef = c == 0 ? 0 : off + c;
			int m = BE.U16(d, off + 10);
			markAttach = m == 0 ? 0 : off + m;
			uint ver = BE.U32(d, off);
			if (ver >= 0x00010002) { int ms = BE.U16(d, off + 12); markSets = ms == 0 ? 0 : off + ms; }
		}

		public int GlyphClass(ushort g) => classDef == 0 ? 0 : Ot.ClassOf(d, classDef, g);
		public int MarkAttachClass(ushort g) => markAttach == 0 ? 0 : Ot.ClassOf(d, markAttach, g);

		public bool InMarkSet(int set, ushort g) {
			if (markSets == 0) return false;
			int cnt = BE.U16(d, markSets + 2);
			if (set >= cnt) return false;
			int cov = markSets + (int)BE.U32(d, markSets + 4 + set * 4);
			return Ot.Coverage(d, cov, g) >= 0;
		}
	}

	internal static class Ot {
		public static int Coverage(byte[] d, int off, ushort g) {
			int fmt = BE.U16(d, off);
			if (fmt == 1) {
				int n = BE.U16(d, off + 2);
				int lo = 0, hi = n - 1;
				while (lo <= hi) {
					int mid = (lo + hi) >> 1;
					int v = BE.U16(d, off + 4 + mid * 2);
					if (g < v) hi = mid - 1; else if (g > v) lo = mid + 1; else return mid;
				}
				return -1;
			}
			if (fmt == 2) {
				int n = BE.U16(d, off + 2);
				int lo = 0, hi = n - 1;
				while (lo <= hi) {
					int mid = (lo + hi) >> 1;
					int r = off + 4 + mid * 6;
					int s = BE.U16(d, r), e = BE.U16(d, r + 2);
					if (g < s) hi = mid - 1; else if (g > e) lo = mid + 1; else return BE.U16(d, r + 4) + g - s;
				}
				return -1;
			}
			return -1;
		}

		public static int ClassOf(byte[] d, int off, ushort g) {
			int fmt = BE.U16(d, off);
			if (fmt == 1) {
				int start = BE.U16(d, off + 2), n = BE.U16(d, off + 4);
				if (g >= start && g < start + n) return BE.U16(d, off + 6 + (g - start) * 2);
				return 0;
			}
			if (fmt == 2) {
				int n = BE.U16(d, off + 2);
				int lo = 0, hi = n - 1;
				while (lo <= hi) {
					int mid = (lo + hi) >> 1;
					int r = off + 4 + mid * 6;
					int s = BE.U16(d, r), e = BE.U16(d, r + 2);
					if (g < s) hi = mid - 1; else if (g > e) lo = mid + 1; else return BE.U16(d, r + 4);
				}
			}
			return 0;
		}

		public static int ValueSize(int fmt) {
			int n = 0;
			for (int i = 0; i < 8; i++) if ((fmt & 1 << i) != 0) n++;
			return n * 2;
		}

		public static void ReadValue(byte[] d, int off, int fmt, out int xp, out int yp, out int xa, out int ya) {
			xp = yp = xa = ya = 0;
			int p = off;
			if ((fmt & 1) != 0) { xp = BE.S16(d, p); p += 2; }
			if ((fmt & 2) != 0) { yp = BE.S16(d, p); p += 2; }
			if ((fmt & 4) != 0) { xa = BE.S16(d, p); p += 2; }
			if ((fmt & 8) != 0) { ya = BE.S16(d, p); }
		}

		public static void Anchor(byte[] d, int off, out int x, out int y) {
			x = BE.S16(d, off + 2);
			y = BE.S16(d, off + 4);
		}
	}

	internal sealed class OtLayout {
		public readonly byte[] D;
		readonly int scriptList, featureList, lookupList;
		public readonly bool IsGpos;
		readonly Dictionary<string, List<int>> _cache = new();
		readonly int _variationsOff;

		public OtLayout(byte[] d, int off, bool gpos) {
			D = d;
			IsGpos = gpos;
			scriptList = off + BE.U16(d, off + 4);
			featureList = off + BE.U16(d, off + 6);
			lookupList = off + BE.U16(d, off + 8);
			if (BE.U32(d, off) >= 0x00010001) { uint fv = BE.U32(d, off + 10); _variationsOff = fv == 0 ? 0 : off + (int)fv; }
		}

		int FindScript(string tag) {
			int n = BE.U16(D, scriptList);
			for (int i = 0; i < n; i++) {
				int r = scriptList + 2 + i * 6;
				if (BE.Tag(D, r) == tag) return scriptList + BE.U16(D, r + 4);
			}
			return -1;
		}

		public bool HasScript(string tag) => FindScript(tag) >= 0;

		int LangSys(int script, string lang) {
			if (lang != null) {
				int n = BE.U16(D, script + 2);
				for (int i = 0; i < n; i++) {
					int r = script + 4 + i * 6;
					if (BE.Tag(D, r) == lang) return script + BE.U16(D, r + 4);
				}
			}
			int def = BE.U16(D, script);
			return def == 0 ? -1 : script + def;
		}

		public List<int> Lookups(string[] scripts, string lang, string feature) {
			string key = string.Join(",", scripts) + "|" + lang + "|" + feature;
			lock (_cache) {
				if (_cache.TryGetValue(key, out var cached)) return cached;
			}
			var res = new List<int>();
			int script = -1;
			foreach (var s in scripts) { if (s == null) continue; script = FindScript(s); if (script >= 0) break; }
			if (script < 0) script = FindScript("DFLT");
			if (script < 0) script = FindScript("dflt");
			if (script < 0) script = FindScript("latn");
			if (script >= 0) {
				int ls = LangSys(script, lang);
				if (ls >= 0) {
					int req = BE.U16(D, ls + 2);
					int n = BE.U16(D, ls + 4);
					var idxs = new List<int>();
					if (req != 0xFFFF) idxs.Add(req);
					for (int i = 0; i < n; i++) idxs.Add(BE.U16(D, ls + 6 + i * 2));
					int fc = BE.U16(D, featureList);
					foreach (int fi in idxs) {
						if (fi >= fc) continue;
						int r = featureList + 2 + fi * 6;
						if (BE.Tag(D, r) != feature) continue;
						int fo = featureList + BE.U16(D, r + 4);
						int lc = BE.U16(D, fo + 2);
						for (int k = 0; k < lc; k++) res.Add(BE.U16(D, fo + 4 + k * 2));
					}
				}
			}
			res = res.Distinct().OrderBy(x => x).ToList();
			lock (_cache) _cache[key] = res;
			return res;
		}

		public int LookupCount => BE.U16(D, lookupList);

		public void Lookup(int i, out int type, out int flag, out int[] subs, out int markSet) {
			int lo = lookupList + BE.U16(D, lookupList + 2 + i * 2);
			type = BE.U16(D, lo);
			flag = BE.U16(D, lo + 2);
			int n = BE.U16(D, lo + 4);
			subs = new int[n];
			for (int k = 0; k < n; k++) subs[k] = lo + BE.U16(D, lo + 6 + k * 2);
			markSet = (flag & 0x10) != 0 ? BE.U16(D, lo + 6 + n * 2) : 0;
			int ext = IsGpos ? 9 : 7;
			if (type == ext && n > 0) {
				int realType = BE.U16(D, subs[0] + 2);
				for (int k = 0; k < n; k++) subs[k] = subs[k] + (int)BE.U32(D, subs[k] + 4);
				type = realType;
			}
		}
	}

	internal struct GI {
		public ushort G;
		public int Cl;
		public uint Mask;
		public byte Cls;
		public int LigId;
		public int LigComp;
		public int Cp;
		public int XAdv, YAdv, XOff, YOff;
		public int Attach;
	}

	internal sealed class ShapedRun {
		public FontFile Font;
		public ushort[] G;
		public int[] Cl;
		public double[] Adv, XOff, YOff;
		public int[] Attach;
		public int Count => G.Length;
	}

	internal static class Shaper {
		public const uint MIsol = 1, MFina = 2, MFin2 = 4, MFin3 = 8, MMedi = 16, MMed2 = 32, MInit = 64, MGlobal = 1u << 20;

		public static byte[] JoiningForms(string text, int start, int end) {
			int n = end - start;
			var forms = new byte[n];
			var jt = new Uni.JT[n];
			for (int i = 0; i < n; i++) {
				char c = text[start + i];
				if (char.IsLowSurrogate(c)) { jt[i] = Uni.JT.T; continue; }
				int cp = Uni.CodePointAt(text, start + i);
				if (cp >= 0x202A && cp <= 0x202E || cp >= 0x2066 && cp <= 0x2069 || cp == 0x200E || cp == 0x200F || cp == 0x61C) { jt[i] = Uni.JT.T; continue; }
				jt[i] = Uni.Joining(cp);
			}
			int prev = -1;
			for (int i = 0; i < n; i++) {
				var t = jt[i];
				if (t == Uni.JT.T) continue;
				if (t == Uni.JT.U) { prev = -1; continue; }
				bool prevJoins = prev >= 0 && (jt[prev] == Uni.JT.D || jt[prev] == Uni.JT.C || jt[prev] == Uni.JT.L);
				bool canJoinPrev = t == Uni.JT.R || t == Uni.JT.D || t == Uni.JT.C;
				if (prevJoins && canJoinPrev) {
					if (jt[prev] != Uni.JT.C) forms[prev] = forms[prev] == 2 ? (byte)4 : forms[prev] == 1 ? (byte)3 : forms[prev];
					forms[i] = 2;
				}
				else forms[i] = 1;
				prev = i;
			}
			for (int i = 0; i < n; i++) {
				if (jt[i] == Uni.JT.C) forms[i] = 0;
				else if (jt[i] == Uni.JT.T || jt[i] == Uni.JT.U) forms[i] = 0;
			}
			return forms;
		}

		static readonly Dictionary<int, int[]> Presentation = BuildPresentation();

		static Dictionary<int, int[]> BuildPresentation() {
			var d = new Dictionary<int, int[]>();
			void A(int cp, params int[] f) => d[cp] = f;
			A(0x621, 0xFE80, 0, 0, 0); A(0x622, 0xFE81, 0xFE82, 0, 0); A(0x623, 0xFE83, 0xFE84, 0, 0); A(0x624, 0xFE85, 0xFE86, 0, 0); A(0x625, 0xFE87, 0xFE88, 0, 0);
			A(0x626, 0xFE89, 0xFE8A, 0xFE8B, 0xFE8C); A(0x627, 0xFE8D, 0xFE8E, 0, 0); A(0x628, 0xFE8F, 0xFE90, 0xFE91, 0xFE92); A(0x629, 0xFE93, 0xFE94, 0, 0);
			int b = 0xFE95;
			foreach (int cp in new[] { 0x62A, 0x62B, 0x62C, 0x62D, 0x62E }) { A(cp, b, b + 1, b + 2, b + 3); b += 4; }
			A(0x62F, 0xFEA9, 0xFEAA, 0, 0); A(0x630, 0xFEAB, 0xFEAC, 0, 0); A(0x631, 0xFEAD, 0xFEAE, 0, 0); A(0x632, 0xFEAF, 0xFEB0, 0, 0);
			b = 0xFEB1;
			foreach (int cp in new[] { 0x633, 0x634, 0x635, 0x636, 0x637, 0x638, 0x639, 0x63A }) { A(cp, b, b + 1, b + 2, b + 3); b += 4; }
			b = 0xFED1;
			foreach (int cp in new[] { 0x641, 0x642, 0x643, 0x644, 0x645, 0x646, 0x647 }) { A(cp, b, b + 1, b + 2, b + 3); b += 4; }
			A(0x648, 0xFEED, 0xFEEE, 0, 0); A(0x649, 0xFEEF, 0xFEF0, 0xFBE8, 0xFBE9); A(0x64A, 0xFEF1, 0xFEF2, 0xFEF3, 0xFEF4);
			A(0x67E, 0xFB56, 0xFB57, 0xFB58, 0xFB59); A(0x686, 0xFB7A, 0xFB7B, 0xFB7C, 0xFB7D); A(0x698, 0xFB8A, 0xFB8B, 0, 0);
			A(0x6A9, 0xFB8E, 0xFB8F, 0xFB90, 0xFB91); A(0x6AF, 0xFB92, 0xFB93, 0xFB94, 0xFB95); A(0x6CC, 0xFBFC, 0xFBFD, 0xFBFE, 0xFBFF);
			A(0x6C0, 0xFBA4, 0xFBA5, 0, 0); A(0x679, 0xFB66, 0xFB67, 0xFB68, 0xFB69); A(0x688, 0xFB88, 0xFB89, 0, 0); A(0x691, 0xFB8C, 0xFB8D, 0, 0);
			A(0x6BA, 0xFB9E, 0xFB9F, 0, 0); A(0x6BE, 0xFBAA, 0xFBAB, 0xFBAC, 0xFBAD); A(0x6C1, 0xFBA6, 0xFBA7, 0xFBA8, 0xFBA9); A(0x6D2, 0xFBAE, 0xFBAF, 0, 0); A(0x6D3, 0xFBB0, 0xFBB1, 0, 0);
			A(0x671, 0xFB50, 0xFB51, 0, 0); A(0x6A4, 0xFB6A, 0xFB6B, 0xFB6C, 0xFB6D); A(0x6A6, 0xFB6E, 0xFB6F, 0xFB70, 0xFB71); A(0x684, 0xFB72, 0xFB73, 0xFB74, 0xFB75);
			A(0x683, 0xFB76, 0xFB77, 0xFB78, 0xFB79); A(0x687, 0xFB7E, 0xFB7F, 0xFB80, 0xFB81); A(0x68D, 0xFB82, 0xFB83, 0, 0); A(0x68C, 0xFB84, 0xFB85, 0, 0);
			A(0x68E, 0xFB86, 0xFB87, 0, 0); A(0x6AD, 0xFBD3, 0xFBD4, 0xFBD5, 0xFBD6); A(0x6C6, 0xFBD9, 0xFBDA, 0, 0); A(0x6C8, 0xFBDB, 0xFBDC, 0, 0); A(0x6CB, 0xFBDE, 0xFBDF, 0, 0);
			A(0x6D0, 0xFBE4, 0xFBE5, 0xFBE6, 0xFBE7); A(0x6BB, 0xFBA0, 0xFBA1, 0xFBA2, 0xFBA3); A(0x6B1, 0xFB9A, 0xFB9B, 0xFB9C, 0xFB9D); A(0x6B3, 0xFB96, 0xFB97, 0xFB98, 0xFB99);
			return d;
		}

		static readonly string[] ArabicStage1 = { "ccmp", "locl" };
		static readonly string[] ArabicForms = { "isol", "fina", "fin2", "fin3", "medi", "med2", "init" };

		public sealed class Opts {
			public bool Kerning = true, Ligatures = true, SmallCaps, Tabular;
			public string Lang;
			public List<(string tag, int val)> Features;
		}

		public static ShapedRun Shape(FontFile f, string text, int start, int end, bool rtl, Sc script, byte[] forms, int formsBase, Opts o) {
			var buf = new List<GI>(end - start);
			bool arabic = script == Sc.Arabic || script == Sc.Syriac || script == Sc.Nko;
			string st = Uni.OtScriptTag(script), stOld = Uni.OtScriptTagOld(script);
			string[] scripts = stOld != null ? new[] { st, stOld } : new[] { st };
			string lang = Uni.OtLangTag(o.Lang);
			bool hasArabicGsub = arabic && f.Gsub != null && (f.Gsub.HasScript("arab") || f.Gsub.HasScript("syrc")) && f.Gsub.Lookups(scripts, lang, "init").Count + f.Gsub.Lookups(scripts, lang, "fina").Count > 0;
			for (int i = start; i < end; i++) {
				char c = text[i];
				if (char.IsLowSurrogate(c) && i > start && char.IsHighSurrogate(text[i - 1])) continue;
				int cp = Uni.CodePointAt(text, i);
				if (Uni.IsDefaultIgnorable(cp) || cp == '\n' || cp == '\r' || cp == 0x2028 || cp == 0x2029) continue;
				if (rtl) cp = Uni.Mirror(cp);
				byte form = forms != null && i - formsBase >= 0 && i - formsBase < forms.Length ? forms[i - formsBase] : (byte)0;
				int mapCp = cp;
				if (arabic && !hasArabicGsub && form != 0 && Presentation.TryGetValue(cp, out var pf)) {
					int alt = form switch { 1 => pf[0], 2 => pf[1], 3 => pf[2], 4 => pf[3], _ => 0 };
					if (alt != 0 && f.HasGlyph(alt)) mapCp = alt;
				}
				ushort g = f.Glyph(mapCp);
				if (g == 0) {
					if (cp == 0xA0 || cp == 0x202F || cp == 0x2007) g = f.Glyph(' ');
					else if (cp == 0x2011) g = f.Glyph('-');
					else if (cp >= 0x2000 && cp <= 0x200A) g = f.Glyph(' ');
					else if (cp == 9) g = f.Glyph(' ');
				}
				uint mask = MGlobal;
				if (arabic) mask |= form switch { 1 => MIsol, 2 => MFina, 3 => MInit, 4 => MMedi, _ => 0u };
				var gi = new GI { G = g, Cl = i, Mask = mask, Cp = cp, Attach = -1 };
				buf.Add(gi);
			}
			if (arabic && !hasArabicGsub) LamAlefFallback(f, buf, forms, formsBase);
			SetClasses(f, buf);

			if (f.Gsub != null) {
				var stages = new List<(string[] feats, uint mask)>();
				if (arabic) {
					stages.Add((ArabicStage1, MGlobal));
					stages.Add((new[] { "stch" }, MGlobal));
					stages.Add((new[] { "isol" }, MIsol));
					stages.Add((new[] { "fina" }, MFina));
					stages.Add((new[] { "fin2" }, MFin2));
					stages.Add((new[] { "fin3" }, MFin3));
					stages.Add((new[] { "medi" }, MMedi));
					stages.Add((new[] { "med2" }, MMed2));
					stages.Add((new[] { "init" }, MInit));
					stages.Add((new[] { "rlig" }, MGlobal));
					stages.Add((new[] { "calt" }, MGlobal));
					var rest = new List<string>();
					if (o.Ligatures) { rest.Add("liga"); rest.Add("clig"); }
					rest.Add("mset");
					AddUserFeatures(rest, o);
					stages.Add((rest.ToArray(), MGlobal));
				}
				else {
					stages.Add((new[] { "ccmp", "locl" }, MGlobal));
					var main = new List<string> { "rlig", "rclt" };
					if (o.Ligatures) { main.Add("calt"); main.Add("liga"); main.Add("clig"); }
					if (rtl) main.Add("rtlm");
					if (o.SmallCaps) main.Add("smcp");
					if (o.Tabular) main.Add("tnum");
					AddUserFeatures(main, o);
					stages.Add((main.ToArray(), MGlobal));
				}
				foreach (var (feats, mask) in stages) {
					var lk = new SortedSet<int>();
					foreach (var ft in feats) foreach (var l in f.Gsub.Lookups(scripts, lang, ft)) lk.Add(l);
					if (o.Features != null) foreach (var (tag, val) in o.Features) if (val == 0 && feats.Contains(tag)) foreach (var l in f.Gsub.Lookups(scripts, lang, tag)) lk.Remove(l);
					foreach (int l in lk) new GsubApplier(f, buf).ApplyLookup(l, mask);
				}
			}

			for (int i = 0; i < buf.Count; i++) {
				var g = buf[i];
				g.XAdv = f.AdvanceUnits(g.G);
				buf[i] = g;
			}
			bool gposKern = false, gposMark = false;
			if (f.Gpos != null) {
				var feats = new List<string>();
				if (o.Kerning) feats.Add("kern");
				feats.AddRange(new[] { "mark", "mkmk", "curs", "dist", "abvm", "blwm" });
				if (o.Features != null) foreach (var (tag, val) in o.Features) if (val != 0 && tag != "kern" && !feats.Contains(tag)) feats.Add(tag);
				var lk = new SortedSet<int>();
				foreach (var ft in feats) {
					var l = f.Gpos.Lookups(scripts, lang, ft);
					if (l.Count > 0 && ft == "kern") gposKern = true;
					if (l.Count > 0 && ft == "mark") gposMark = true;
					foreach (var x in l) lk.Add(x);
				}
				var ap = new GposApplier(f, buf);
				foreach (int l in lk) ap.ApplyLookup(l);
			}
			if (!gposKern && o.Kerning && f.Kern != null) {
				for (int i = 0; i + 1 < buf.Count; i++) {
					if (buf[i].Cls == 3) continue;
					int j = i + 1;
					while (j < buf.Count && buf[j].Cls == 3) j++;
					if (j >= buf.Count) break;
					if (f.Kern.TryGetValue((uint)buf[i].G << 16 | buf[j].G, out short kv)) { var g = buf[i]; g.XAdv += kv; buf[i] = g; }
				}
			}
			for (int i = 0; i < buf.Count; i++) {
				var g = buf[i];
				if (g.Cls == 3 && (f.Gdef != null && f.Gdef.HasClasses || Uni.IsMark(g.Cp))) {
					if (g.Attach < 0 && !gposMark) {
						int b = i - 1;
						while (b >= 0 && buf[b].Cls == 3) b--;
						if (b >= 0 && Uni.IsMark(g.Cp)) {
							int markW = g.XAdv;
							g.Attach = b;
							double bc = buf[b].XAdv / 2.0;
							if (f.GlyphBBox(buf[b].G, out var bx0, out _, out var bx1, out _)) bc = (bx0 + bx1) / 2.0;
							double mc = markW / 2.0;
							if (f.GlyphBBox(g.G, out var mx0, out _, out var mx1, out _)) mc = (mx0 + mx1) / 2.0;
							g.XOff = (int)(bc - mc);
						}
					}
					g.XAdv = 0;
					buf[i] = g;
				}
			}
			double scale = 1.0;
			var run = new ShapedRun { Font = f, G = new ushort[buf.Count], Cl = new int[buf.Count], Adv = new double[buf.Count], XOff = new double[buf.Count], YOff = new double[buf.Count], Attach = new int[buf.Count] };
			for (int i = 0; i < buf.Count; i++) {
				run.G[i] = buf[i].G;
				run.Cl[i] = buf[i].Cl;
				run.Adv[i] = buf[i].XAdv * scale;
				run.XOff[i] = buf[i].XOff * scale;
				run.YOff[i] = buf[i].YOff * scale;
				run.Attach[i] = buf[i].Attach;
			}
			return run;
		}

		static void AddUserFeatures(List<string> l, Opts o) {
			if (o.Features == null) return;
			foreach (var (tag, val) in o.Features) {
				if (val == 0) l.Remove(tag);
				else if (!l.Contains(tag)) l.Add(tag);
			}
		}

		static void LamAlefFallback(FontFile f, List<GI> buf, byte[] forms, int formsBase) {
			for (int i = 0; i + 1 < buf.Count; i++) {
				if (buf[i].Cp != 0x644) continue;
				int j = i + 1;
				int a = buf[j].Cp;
				int iso = a switch { 0x622 => 0xFEF5, 0x623 => 0xFEF7, 0x625 => 0xFEF9, 0x627 => 0xFEFB, _ => 0 };
				if (iso == 0) continue;
				byte lf = forms != null && buf[i].Cl - formsBase < forms.Length ? forms[buf[i].Cl - formsBase] : (byte)1;
				int lig = lf == 2 || lf == 4 ? iso + 1 : iso;
				ushort g = f.Glyph(lig);
				if (g == 0) continue;
				var gi = buf[i];
				gi.G = g;
				buf[i] = gi;
				buf.RemoveAt(j);
			}
		}

		static void SetClasses(FontFile f, List<GI> buf) {
			for (int i = 0; i < buf.Count; i++) {
				var g = buf[i];
				g.Cls = ClassOf(f, g.G, g.Cp);
				buf[i] = g;
			}
		}

		public static byte ClassOf(FontFile f, ushort g, int cp) {
			if (f.Gdef != null && f.Gdef.HasClasses) {
				int c = f.Gdef.GlyphClass(g);
				return (byte)(c == 0 ? 1 : c);
			}
			return (byte)(cp != 0 && Uni.IsMark(cp) ? 3 : 1);
		}
	}

	internal abstract class LookupApplier {
		protected readonly FontFile F;
		protected readonly List<GI> B;
		protected readonly OtLayout T;
		protected int Flag, MarkSet;
		protected int Depth;

		protected LookupApplier(FontFile f, List<GI> b, OtLayout t) { F = f; B = b; T = t; }

		protected bool Skip(int i) {
			var g = B[i];
			byte c = g.Cls;
			if ((Flag & 2) != 0 && c == 1) return true;
			if ((Flag & 4) != 0 && c == 2) return true;
			if (c == 3) {
				if ((Flag & 8) != 0) return true;
				if ((Flag & 0x10) != 0 && F.Gdef != null && !F.Gdef.InMarkSet(MarkSet, g.G)) return true;
				int mat = Flag >> 8;
				if (mat != 0 && F.Gdef != null && F.Gdef.MarkAttachClass(g.G) != mat) return true;
			}
			return false;
		}

		protected int Next(int i) {
			i++;
			while (i < B.Count && Skip(i)) i++;
			return i < B.Count ? i : -1;
		}

		protected int Prev(int i) {
			i--;
			while (i >= 0 && Skip(i)) i--;
			return i;
		}

		protected abstract bool ApplySub(int type, int sub, ref int i);

		protected bool ApplyAt(int lookup, ref int i) {
			T.Lookup(lookup, out int type, out int flag, out int[] subs, out int ms);
			int of = Flag, om = MarkSet;
			Flag = flag; MarkSet = ms;
			bool ok = false;
			foreach (var s in subs) { if (ApplySub(type, s, ref i)) { ok = true; break; } }
			Flag = of; MarkSet = om;
			return ok;
		}

		protected bool MatchSeq(int start, int count, Func<int, int, bool> match, out int[] pos) {
			pos = new int[count + 1];
			pos[0] = start;
			int p = start;
			for (int k = 1; k <= count; k++) {
				p = Next(p);
				if (p < 0 || !match(k - 1, p)) return false;
				pos[k] = p;
			}
			return true;
		}

		protected bool MatchBack(int start, int count, Func<int, int, bool> match) {
			int p = start;
			for (int k = 0; k < count; k++) {
				p = Prev(p);
				if (p < 0 || !match(k, p)) return false;
			}
			return true;
		}

		protected bool MatchAhead(int last, int count, Func<int, int, bool> match) {
			int p = last;
			for (int k = 0; k < count; k++) {
				p = Next(p);
				if (p < 0 || !match(k, p)) return false;
			}
			return true;
		}

		protected void ApplyRecords(int[] pos, int recOff, int recCount, ref int i) {
			if (Depth > 8) return;
			Depth++;
			var positions = pos.ToList();
			int lastIndex = pos[pos.Length - 1];
			for (int r = 0; r < recCount; r++) {
				int seq = BE.U16(T.D, recOff + r * 4), lk = BE.U16(T.D, recOff + r * 4 + 2);
				if (seq >= positions.Count) continue;
				int at = positions[seq];
				int before = B.Count;
				int ii = at;
				ApplyAt(lk, ref ii);
				int delta = B.Count - before;
				if (delta != 0) {
					for (int k = seq + 1; k < positions.Count; k++) positions[k] += delta;
					lastIndex += delta;
				}
			}
			Depth--;
			i = Math.Max(i, lastIndex);
		}

		protected bool Context(int type, int sub, ref int i, bool chain) {
			var d = T.D;
			int fmt = BE.U16(d, sub);
			ushort g = B[i].G;
			if (!chain) {
				if (fmt == 1) {
					int ci = Ot.Coverage(d, sub + BE.U16(d, sub + 2), g);
					if (ci < 0 || ci >= BE.U16(d, sub + 4)) return false;
					int rs = sub + BE.U16(d, sub + 6 + ci * 2);
					int rc = BE.U16(d, rs);
					for (int r = 0; r < rc; r++) {
						int ro = rs + BE.U16(d, rs + 2 + r * 2);
						int gc = BE.U16(d, ro), sc = BE.U16(d, ro + 2);
						if (MatchSeq(i, gc - 1, (k, p) => B[p].G == BE.U16(d, ro + 4 + k * 2), out var pos)) { ApplyRecords(pos, ro + 4 + (gc - 1) * 2, sc, ref i); return true; }
					}
					return false;
				}
				if (fmt == 2) {
					if (Ot.Coverage(d, sub + BE.U16(d, sub + 2), g) < 0) return false;
					int cd = sub + BE.U16(d, sub + 4);
					int cls = Ot.ClassOf(d, cd, g);
					if (cls >= BE.U16(d, sub + 6)) return false;
					int so = BE.U16(d, sub + 8 + cls * 2);
					if (so == 0) return false;
					int rs = sub + so;
					int rc = BE.U16(d, rs);
					for (int r = 0; r < rc; r++) {
						int ro = rs + BE.U16(d, rs + 2 + r * 2);
						int gc = BE.U16(d, ro), sc = BE.U16(d, ro + 2);
						if (MatchSeq(i, gc - 1, (k, p) => Ot.ClassOf(d, cd, B[p].G) == BE.U16(d, ro + 4 + k * 2), out var pos)) { ApplyRecords(pos, ro + 4 + (gc - 1) * 2, sc, ref i); return true; }
					}
					return false;
				}
				if (fmt == 3) {
					int gc = BE.U16(d, sub + 2), sc = BE.U16(d, sub + 4);
					if (Ot.Coverage(d, sub + BE.U16(d, sub + 6), g) < 0) return false;
					if (MatchSeq(i, gc - 1, (k, p) => Ot.Coverage(d, sub + BE.U16(d, sub + 8 + (k + 1) * 2), B[p].G) >= 0, out var pos)) { ApplyRecords(pos, sub + 6 + gc * 2, sc, ref i); return true; }
					return false;
				}
				return false;
			}
			if (fmt == 1) {
				int ci = Ot.Coverage(d, sub + BE.U16(d, sub + 2), g);
				if (ci < 0 || ci >= BE.U16(d, sub + 4)) return false;
				int rs = sub + BE.U16(d, sub + 6 + ci * 2);
				int rc = BE.U16(d, rs);
				for (int r = 0; r < rc; r++) {
					int ro = rs + BE.U16(d, rs + 2 + r * 2);
					int bc = BE.U16(d, ro);
					int ip = ro + 2 + bc * 2;
					int ic = BE.U16(d, ip);
					int lp = ip + 2 + (ic - 1) * 2;
					int lc = BE.U16(d, lp);
					int sp = lp + 2 + lc * 2;
					int sc = BE.U16(d, sp);
					if (!MatchBack(i, bc, (k, p) => B[p].G == BE.U16(d, ro + 2 + k * 2))) continue;
					if (!MatchSeq(i, ic - 1, (k, p) => B[p].G == BE.U16(d, ip + 2 + k * 2), out var pos)) continue;
					if (!MatchAhead(pos[pos.Length - 1], lc, (k, p) => B[p].G == BE.U16(d, lp + 2 + k * 2))) continue;
					ApplyRecords(pos, sp + 2, sc, ref i);
					return true;
				}
				return false;
			}
			if (fmt == 2) {
				if (Ot.Coverage(d, sub + BE.U16(d, sub + 2), g) < 0) return false;
				int bcd = sub + BE.U16(d, sub + 4), icd = sub + BE.U16(d, sub + 6), lcd = sub + BE.U16(d, sub + 8);
				int cls = Ot.ClassOf(d, icd, g);
				if (cls >= BE.U16(d, sub + 10)) return false;
				int so = BE.U16(d, sub + 12 + cls * 2);
				if (so == 0) return false;
				int rs = sub + so;
				int rc = BE.U16(d, rs);
				for (int r = 0; r < rc; r++) {
					int ro = rs + BE.U16(d, rs + 2 + r * 2);
					int bc = BE.U16(d, ro);
					int ip = ro + 2 + bc * 2;
					int ic = BE.U16(d, ip);
					int lp = ip + 2 + (ic - 1) * 2;
					int lc = BE.U16(d, lp);
					int sp = lp + 2 + lc * 2;
					int sc = BE.U16(d, sp);
					if (!MatchBack(i, bc, (k, p) => Ot.ClassOf(d, bcd, B[p].G) == BE.U16(d, ro + 2 + k * 2))) continue;
					if (!MatchSeq(i, ic - 1, (k, p) => Ot.ClassOf(d, icd, B[p].G) == BE.U16(d, ip + 2 + k * 2), out var pos)) continue;
					if (!MatchAhead(pos[pos.Length - 1], lc, (k, p) => Ot.ClassOf(d, lcd, B[p].G) == BE.U16(d, lp + 2 + k * 2))) continue;
					ApplyRecords(pos, sp + 2, sc, ref i);
					return true;
				}
				return false;
			}
			if (fmt == 3) {
				int bc = BE.U16(d, sub + 2);
				int ip = sub + 4 + bc * 2;
				int ic = BE.U16(d, ip);
				int lp = ip + 2 + ic * 2;
				int lc = BE.U16(d, lp);
				int sp = lp + 2 + lc * 2;
				int sc = BE.U16(d, sp);
				if (ic == 0) return false;
				if (Ot.Coverage(d, sub + BE.U16(d, ip + 2), g) < 0) return false;
				if (!MatchBack(i, bc, (k, p) => Ot.Coverage(d, sub + BE.U16(d, sub + 4 + k * 2), B[p].G) >= 0)) return false;
				if (!MatchSeq(i, ic - 1, (k, p) => Ot.Coverage(d, sub + BE.U16(d, ip + 2 + (k + 1) * 2), B[p].G) >= 0, out var pos)) return false;
				if (!MatchAhead(pos[pos.Length - 1], lc, (k, p) => Ot.Coverage(d, sub + BE.U16(d, lp + 2 + k * 2), B[p].G) >= 0)) return false;
				ApplyRecords(pos, sp + 2, sc, ref i);
				return true;
			}
			return false;
		}
	}

	internal sealed class GsubApplier : LookupApplier {
		int _ligId = 1;
		public GsubApplier(FontFile f, List<GI> b) : base(f, b, f.Gsub) { }

		public void ApplyLookup(int lookup, uint mask) {
			T.Lookup(lookup, out int type, out int flag, out int[] subs, out int ms);
			Flag = flag; MarkSet = ms;
			if (type == 8) {
				for (int i = B.Count - 1; i >= 0; i--) {
					if ((B[i].Mask & mask) == 0 || Skip(i)) continue;
					int ii = i;
					foreach (var s in subs) if (ApplySub(type, s, ref ii)) break;
				}
				return;
			}
			for (int i = 0; i < B.Count; i++) {
				if ((B[i].Mask & mask) == 0 || Skip(i)) continue;
				int ii = i;
				foreach (var s in subs) {
					if (ApplySub(type, s, ref ii)) { i = ii; break; }
				}
			}
		}

		void Set(int i, ushort g) {
			var x = B[i];
			x.G = g;
			x.Cls = Shaper.ClassOf(F, g, x.Cls == 3 ? x.Cp : 0);
			if (F.Gdef == null || !F.Gdef.HasClasses) x.Cls = B[i].Cls;
			B[i] = x;
		}

		protected override bool ApplySub(int type, int sub, ref int i) {
			var d = T.D;
			ushort g = B[i].G;
			switch (type) {
				case 1: {
					int fmt = BE.U16(d, sub);
					int ci = Ot.Coverage(d, sub + BE.U16(d, sub + 2), g);
					if (ci < 0) return false;
					if (fmt == 1) Set(i, (ushort)(g + BE.S16(d, sub + 4)));
					else { if (ci >= BE.U16(d, sub + 4)) return false; Set(i, BE.U16(d, sub + 6 + ci * 2)); }
					return true;
				}
				case 2: {
					int ci = Ot.Coverage(d, sub + BE.U16(d, sub + 2), g);
					if (ci < 0 || ci >= BE.U16(d, sub + 4)) return false;
					int so = sub + BE.U16(d, sub + 6 + ci * 2);
					int n = BE.U16(d, so);
					if (n == 0) { B.RemoveAt(i); i--; return true; }
					var baseG = B[i];
					Set(i, BE.U16(d, so + 2));
					for (int k = 1; k < n; k++) {
						var ng = baseG;
						ng.G = BE.U16(d, so + 2 + k * 2);
						ng.Cls = Shaper.ClassOf(F, ng.G, 0);
						B.Insert(i + k, ng);
					}
					i += n - 1;
					return true;
				}
				case 3: {
					int ci = Ot.Coverage(d, sub + BE.U16(d, sub + 2), g);
					if (ci < 0 || ci >= BE.U16(d, sub + 4)) return false;
					int so = sub + BE.U16(d, sub + 6 + ci * 2);
					if (BE.U16(d, so) == 0) return false;
					Set(i, BE.U16(d, so + 2));
					return true;
				}
				case 4: {
					int ci = Ot.Coverage(d, sub + BE.U16(d, sub + 2), g);
					if (ci < 0 || ci >= BE.U16(d, sub + 4)) return false;
					int ls = sub + BE.U16(d, sub + 6 + ci * 2);
					int lc = BE.U16(d, ls);
					for (int l = 0; l < lc; l++) {
						int lo = ls + BE.U16(d, ls + 2 + l * 2);
						ushort lig = BE.U16(d, lo);
						int cc = BE.U16(d, lo + 2);
						if (!MatchSeq(i, cc - 1, (k, p) => B[p].G == BE.U16(d, lo + 4 + k * 2), out var pos)) continue;
						int id = _ligId++;
						var first = B[i];
						first.G = lig;
						first.Cls = Shaper.ClassOf(F, lig, 0);
						if (F.Gdef == null || !F.Gdef.HasClasses) first.Cls = 2;
						first.LigId = id;
						first.LigComp = 0;
						int minCl = first.Cl;
						for (int k = 1; k < pos.Length; k++) minCl = Math.Min(minCl, B[pos[k]].Cl);
						first.Cl = minCl;
						B[i] = first;
						int comp = 0;
						for (int p = i + 1; p <= pos[pos.Length - 1]; p++) {
							if (Array.IndexOf(pos, p) >= 0) comp++;
							else if (B[p].Cls == 3) { var m = B[p]; m.LigId = id; m.LigComp = comp; B[p] = m; }
						}
						for (int k = pos.Length - 1; k >= 1; k--) B.RemoveAt(pos[k]);
						return true;
					}
					return false;
				}
				case 5: return Context(type, sub, ref i, false);
				case 6: return Context(type, sub, ref i, true);
				case 8: {
					if (Ot.Coverage(d, sub + BE.U16(d, sub + 2), g) is int ci && ci < 0) return false;
					int bc = BE.U16(d, sub + 4);
					int lp = sub + 6 + bc * 2;
					int lc = BE.U16(d, lp);
					int gp = lp + 2 + lc * 2;
					int gc = BE.U16(d, gp);
					if (!MatchBack(i, bc, (k, p) => Ot.Coverage(d, sub + BE.U16(d, sub + 6 + k * 2), B[p].G) >= 0)) return false;
					if (!MatchAhead(i, lc, (k, p) => Ot.Coverage(d, sub + BE.U16(d, lp + 2 + k * 2), B[p].G) >= 0)) return false;
					int c2 = Ot.Coverage(d, sub + BE.U16(d, sub + 2), g);
					if (c2 < gc) Set(i, BE.U16(d, gp + 2 + c2 * 2));
					return true;
				}
			}
			return false;
		}
	}

	internal sealed class GposApplier : LookupApplier {
		public GposApplier(FontFile f, List<GI> b) : base(f, b, f.Gpos) { }

		public void ApplyLookup(int lookup) {
			T.Lookup(lookup, out int type, out int flag, out int[] subs, out int ms);
			Flag = flag; MarkSet = ms;
			for (int i = 0; i < B.Count; i++) {
				if (Skip(i)) continue;
				int ii = i;
				foreach (var s in subs) if (ApplySub(type, s, ref ii)) { if (type == 2) i = ii - 1 >= i ? ii - 1 : i; break; }
			}
		}

		void AddValue(int i, int vo, int fmt) {
			Ot.ReadValue(T.D, vo, fmt, out int xp, out int yp, out int xa, out int ya);
			var g = B[i];
			g.XOff += xp; g.YOff += yp; g.XAdv += xa; g.YAdv += ya;
			B[i] = g;
		}

		protected override bool ApplySub(int type, int sub, ref int i) {
			var d = T.D;
			ushort g = B[i].G;
			switch (type) {
				case 1: {
					int fmt = BE.U16(d, sub);
					int ci = Ot.Coverage(d, sub + BE.U16(d, sub + 2), g);
					if (ci < 0) return false;
					int vf = BE.U16(d, sub + 4);
					if (fmt == 1) AddValue(i, sub + 6, vf);
					else { if (ci >= BE.U16(d, sub + 6)) return false; AddValue(i, sub + 8 + ci * Ot.ValueSize(vf), vf); }
					return true;
				}
				case 2: {
					int fmt = BE.U16(d, sub);
					int ci = Ot.Coverage(d, sub + BE.U16(d, sub + 2), g);
					if (ci < 0) return false;
					int j = Next(i);
					if (j < 0) return false;
					int vf1 = BE.U16(d, sub + 4), vf2 = BE.U16(d, sub + 6);
					int s1 = Ot.ValueSize(vf1), s2 = Ot.ValueSize(vf2);
					ushort g2 = B[j].G;
					if (fmt == 1) {
						if (ci >= BE.U16(d, sub + 8)) return false;
						int ps = sub + BE.U16(d, sub + 10 + ci * 2);
						int n = BE.U16(d, ps);
						int rec = 2 + s1 + s2;
						int lo = 0, hi = n - 1;
						while (lo <= hi) {
							int mid = (lo + hi) >> 1;
							int r = ps + 2 + mid * rec;
							int sg = BE.U16(d, r);
							if (g2 < sg) hi = mid - 1;
							else if (g2 > sg) lo = mid + 1;
							else {
								AddValue(i, r + 2, vf1);
								AddValue(j, r + 2 + s1, vf2);
								i = vf2 != 0 ? j + 1 : j;
								return true;
							}
						}
						return false;
					}
					if (fmt == 2) {
						int c1 = Ot.ClassOf(d, sub + BE.U16(d, sub + 8), g);
						int c2 = Ot.ClassOf(d, sub + BE.U16(d, sub + 10), g2);
						int n1 = BE.U16(d, sub + 12), n2 = BE.U16(d, sub + 14);
						if (c1 >= n1 || c2 >= n2) return false;
						int r = sub + 16 + (c1 * n2 + c2) * (s1 + s2);
						AddValue(i, r, vf1);
						AddValue(j, r + s1, vf2);
						i = vf2 != 0 ? j + 1 : j;
						return true;
					}
					return false;
				}
				case 3: {
					int ci = Ot.Coverage(d, sub + BE.U16(d, sub + 2), g);
					if (ci < 0) return false;
					int entry = BE.U16(d, sub + 6 + ci * 4);
					if (entry == 0) return false;
					int pi = Prev(i);
					if (pi < 0) return false;
					int pci = Ot.Coverage(d, sub + BE.U16(d, sub + 2), B[pi].G);
					if (pci < 0) return false;
					int exit = BE.U16(d, sub + 6 + pci * 4 + 2);
					if (exit == 0) return false;
					Ot.Anchor(d, sub + entry, out int ex, out int ey);
					Ot.Anchor(d, sub + exit, out int xx, out int xy);
					var cur = B[i];
					var prev = B[pi];
					bool rtl = (Flag & 1) != 0;
					if (!rtl) {
						prev.XAdv = xx + prev.XOff;
						int dd = ex + cur.XOff;
						cur.XAdv -= dd;
						cur.XOff -= dd;
					}
					else {
						int dd = xx + prev.XOff;
						prev.XAdv -= dd;
						prev.XOff -= dd;
						cur.XAdv = ex + cur.XOff;
					}
					cur.YOff = prev.YOff + xy - ey;
					B[i] = cur;
					B[pi] = prev;
					return true;
				}
				case 4: case 5: case 6: {
					int markCov = sub + BE.U16(d, sub + 2), baseCov = sub + BE.U16(d, sub + 4);
					int mi = Ot.Coverage(d, markCov, g);
					if (mi < 0) return false;
					int classCount = BE.U16(d, sub + 6);
					int markArr = sub + BE.U16(d, sub + 8), baseArr = sub + BE.U16(d, sub + 10);
					int b = i - 1;
					if (type == 6) {
						while (b >= 0 && Skip(b)) b--;
						if (b < 0 || B[b].Cls != 3) return false;
					}
					else {
						while (b >= 0 && (B[b].Cls == 3 || type == 4 && B[b].Cls == 4)) b--;
						if (b < 0) return false;
					}
					int bi = Ot.Coverage(d, baseCov, B[b].G);
					if (bi < 0) return false;
					int markClass = BE.U16(d, markArr + 2 + mi * 4);
					int markAnchor = markArr + BE.U16(d, markArr + 2 + mi * 4 + 2);
					if (markClass >= classCount) return false;
					int anchorOff;
					if (type == 5) {
						int lat = baseArr + BE.U16(d, baseArr + 2 + bi * 2);
						int comps = BE.U16(d, lat);
						if (comps == 0) return false;
						int comp = B[i].LigId != 0 && B[i].LigId == B[b].LigId ? Math.Min(B[i].LigComp, comps - 1) : comps - 1;
						if (B[i].LigId != 0 && B[i].LigId == B[b].LigId && B[i].LigComp > 0) comp = Math.Min(B[i].LigComp - 1, comps - 1);
						int ao = BE.U16(d, lat + 2 + (comp * classCount + markClass) * 2);
						if (ao == 0) return false;
						anchorOff = lat + ao;
					}
					else {
						int ao = BE.U16(d, baseArr + 2 + (bi * classCount + markClass) * 2);
						if (ao == 0) return false;
						anchorOff = baseArr + ao;
					}
					Ot.Anchor(d, anchorOff, out int bx, out int by);
					Ot.Anchor(d, markAnchor, out int mx, out int my);
					var gm = B[i];
					gm.Attach = b;
					gm.XOff = bx - mx;
					gm.YOff = by - my;
					B[i] = gm;
					return true;
				}
				case 7: return Context(type, sub, ref i, false);
				case 8: return Context(type, sub, ref i, true);
			}
			return false;
		}
	}

	internal enum BK : byte { Block, Inline, Text, Replaced, Br, Wbr, Table, RowGroup, Row, Cell, Caption, Column, ColGroup, Flex, Grid }

	internal sealed class Deco {
		public byte Lines;
		public Rgba Color;
		public byte Style;
		public Len Thickness;
		public Len Offset;
	}

	internal sealed class Box {
		public BK Kind;
		public Element El;
		public Style S;
		public Box Parent;
		public List<Box> Kids = new();
		public string Text;
		public bool Anon;
		public ImageData Img;
		public Element SvgEl;
		public byte Control;
		public double IntrW = -1, IntrH = -1;
		public List<Deco> Decos;
		public string Href;
		public string Alt;

		public double X, Y, W, H;
		public double[] M = new double[4], Bd = new double[4], P = new double[4];
		public List<LineBox> Lines;
		public double Baseline = double.NaN, LastBaseline = double.NaN;
		public InlineData IData;
		public double CachedMin = -1, CachedMax = -1;
		public bool TopCollapsed;
		public List<Box> AbsList;
		public double StaticX, StaticY;
		public bool StaticRtl;
		public int ColSpan = 1, RowSpan = 1, Col, Row;
		public string MarkerText;
		public Style MarkerStyle;
		public List<GlyphFrag> MarkerFrags;
		public ImageData MarkerImg;
		public double MarkerImgX, MarkerImgY, MarkerImgW, MarkerImgH;
		public byte MarkerShape;
		public double MarkerSX, MarkerSY, MarkerSW;
		public Rgba MarkerColor;
		public double ClipBottom = double.NaN;
		public List<(double y, double h)> RepeatHeaders, RepeatFooters;
		public double ContentH;
		public double ForceW = double.NaN, ForceH = double.NaN;
		public double TY, TH = double.NaN;
		public (double w, BS s, Rgba c)[] CB;
		public bool IsHeaderGroup, IsFooterGroup;
		public double MinY, MaxY;

		public bool IsAtomicInline => S.IsInlineLevel && S.Display != Disp.Inline || Kind == BK.Replaced && S.Display == Disp.Inline;
		public bool IsInlineLevel => Kind == BK.Text || Kind == BK.Br || Kind == BK.Wbr || S.IsInlineLevel || Kind == BK.Replaced && S.Display == Disp.Inline;
		public bool IsOutOfFlow => S.IsOutOfFlow && Kind != BK.Text;
		public bool IsFloat => S.IsFloat && Kind != BK.Text;
		public bool IsBlockLevel => !IsInlineLevel && !IsOutOfFlow && !IsFloat;
		public bool IsReplaced => Kind == BK.Replaced;
		public double ContentX => X + Bd[3] + P[3];
		public double ContentY => Y + Bd[0] + P[0];
		public double ContentW => W - Bd[1] - Bd[3] - P[1] - P[3];
		public double ContentHgt => H - Bd[0] - Bd[2] - P[0] - P[2];
		public double MarginTopEdge => Y - M[0];

		public Box(BK k, Element e, Style s) { Kind = k; El = e; S = s; }

		public void Add(Box b) { b.Parent = this; Kids.Add(b); }

		public bool IsBfcRoot() {
			if (Parent == null) return true;
			if (S.IsFloat || S.IsOutOfFlow) return true;
			if (S.Display == Disp.InlineBlock || S.Display == Disp.FlowRoot || S.Display == Disp.Table || S.Display == Disp.InlineTable) return true;
			if (Kind == BK.Cell || Kind == BK.Caption || Kind == BK.Flex || Kind == BK.Grid || Kind == BK.Table) return true;
			if (S.OverflowX != 0 || S.OverflowY != 0) return true;
			if (Parent != null && (Parent.Kind == BK.Flex || Parent.Kind == BK.Grid)) return true;
			if (S.ColumnCount > 1) return true;
			return false;
		}

		public IEnumerable<Box> Descendants() {
			foreach (var k in Kids) {
				yield return k;
				foreach (var d in k.Descendants()) yield return d;
			}
		}
	}

	internal sealed class BoxBuilder {
		readonly RenderContext _ctx;
		readonly Dictionary<string, List<int>> _counters = new();
		readonly List<List<string>> _frames = new();
		int _quoteDepth;

		public BoxBuilder(RenderContext ctx) { _ctx = ctx; }

		public Box BuildRoot(Element root) {
			_frames.Add(new List<string>());
			var list = BuildElement(root, null);
			_frames.RemoveAt(_frames.Count - 1);
			var b = list.FirstOrDefault(x => x.Kind != BK.Text) ?? new Box(BK.Block, root, root.Style);
			return b;
		}

		void CounterReset(string name, int v) {
			if (!_counters.TryGetValue(name, out var st)) _counters[name] = st = new List<int>();
			st.Add(v);
			_frames[Math.Max(0, _frames.Count - 2)].Add(name);
		}

		void CounterAdd(string name, int v) {
			if (!_counters.TryGetValue(name, out var st) || st.Count == 0) { CounterReset(name, 0); st = _counters[name]; }
			st[st.Count - 1] += v;
		}

		void CounterSet(string name, int v) {
			if (!_counters.TryGetValue(name, out var st) || st.Count == 0) { CounterReset(name, v); return; }
			st[st.Count - 1] = v;
		}

		int CounterGet(string name) => _counters.TryGetValue(name, out var st) && st.Count > 0 ? st[st.Count - 1] : 0;
		List<int> CounterAll(string name) => _counters.TryGetValue(name, out var st) ? st : new List<int>();

		static IEnumerable<(string name, int v)> ParseCounterList(string s, int def) {
			if (string.IsNullOrWhiteSpace(s)) yield break;
			var t = Css.SplitWs(s);
			for (int i = 0; i < t.Count; i++) {
				string name = t[i];
				int v = def;
				if (i + 1 < t.Count && int.TryParse(t[i + 1], out int n)) { v = n; i++; }
				yield return (name, v);
			}
		}

		void ApplyCounters(Element e, Style s) {
			bool isOl = e != null && (e.Tag == "ol" || e.Tag == "ul" || e.Tag == "menu" || e.Tag == "dir");
			bool resetsListItem = s.CounterReset != null && ParseCounterList(s.CounterReset, 0).Any(x => x.name == "list-item");
			if (isOl && !resetsListItem) {
				int start = 1;
				bool reversed = e.Attr("reversed") != null;
				if (e.Tag == "ol" && e.Attr("start") is string st && int.TryParse(st.Trim(), out int sv)) start = sv;
				else if (reversed) start = e.ChildElements().Count(c => c.Style != null && c.Style.Display == Disp.ListItem);
				CounterReset("list-item", reversed ? start + 1 : start - 1);
			}
			foreach (var (n, v) in ParseCounterList(s.CounterReset, 0)) CounterReset(n, v);
			foreach (var (n, v) in ParseCounterList(s.CounterSet, 0)) CounterSet(n, v);
			bool incListItem = s.Display == Disp.ListItem && !(s.CounterIncrement != null && ParseCounterList(s.CounterIncrement, 1).Any(x => x.name == "list-item"));
			foreach (var (n, v) in ParseCounterList(s.CounterIncrement, 1)) CounterAdd(n, v);
			if (incListItem) {
				bool reversed = e?.Parent != null && e.Parent.Tag == "ol" && e.Parent.Attr("reversed") != null;
				if (e != null && e.Tag == "li" && e.Attr("value") is string vv && int.TryParse(vv.Trim(), out int val)) CounterSet("list-item", val);
				else CounterAdd("list-item", reversed ? -1 : 1);
			}
		}

		List<Box> BuildElement(Element e, Box parentBox) {
			var res = new List<Box>();
			var s = e.Style;
			if (s == null || s.Display == Disp.None) return res;
			if (e.IsSvgChild) return res;
			_frames.Add(new List<string>());
			try {
				ApplyCounters(e, s);
				if (s.Display == Disp.Contents) {
					res.AddRange(BuildChildren(e, null));
					return res;
				}
				Box b = MakeBox(e, s);
				if (b.Kind == BK.Replaced || b.Kind == BK.Br || b.Kind == BK.Wbr) {
					res.Add(b);
					return res;
				}
				foreach (var k in BuildChildren(e, b)) b.Add(k);
				if (e.Tag == "a" && e.Attr("href") != null) b.Href = e.Attr("href");
				Fixup(b);
				res.Add(b);
				return res;
			}
			finally {
				var frame = _frames[_frames.Count - 1]; _frames.RemoveAt(_frames.Count - 1);
				foreach (var n in frame) { var st = _counters[n]; st.RemoveAt(st.Count - 1); }
			}
		}

		List<Box> BuildChildren(Element e, Box owner) {
			var list = new List<Box>();
			var s = e.Style;
			if (e.Tag == "textarea" || e.Tag == "select" || e.Tag == "svg") return list;
			if (e.Before != null) { var pb = Pseudo(e, e.Before, "before"); if (pb != null) list.Add(pb); }
			foreach (var c in e.Children) {
				if (c is TextNode t) {
					if (t.Text.Length == 0) continue;
					list.Add(new Box(BK.Text, null, s) { Text = t.Text });
				}
				else if (c is Element ce) {
					list.AddRange(BuildElement(ce, owner));
				}
			}
			if (e.After != null) { var pa = Pseudo(e, e.After, "after"); if (pa != null) list.Add(pa); }
			if (s.Display == Disp.ListItem) {
				string mt = MarkerText(e, s);
				if (owner != null) {
					owner.MarkerText = mt;
					owner.MarkerStyle = e.Marker ?? s;
					if (s.ListStyleImage != null) owner.MarkerImg = _ctx.LoadImage(s.ListStyleImage);
					if (s.ListInside && (mt != null || owner.MarkerImg != null)) {
						var ms = (e.Marker ?? s).InheritFrom();
						ms.Display = Disp.Inline;
						ms.Ws = WSp.Pre;
						if (owner.MarkerImg != null) {
							var ib = new Box(BK.Replaced, null, ms) { Img = owner.MarkerImg, IntrW = owner.MarkerImg.W, IntrH = owner.MarkerImg.H };
							ms.Margin[s.Rtl ? 3 : 1] = Len.PxV(ms.FontSize * 0.5);
							list.Insert(0, ib);
						}
						else list.Insert(0, new Box(BK.Text, null, ms) { Text = mt, Anon = true });
						owner.MarkerText = null;
						owner.MarkerImg = null;
					}
				}
			}
			return list;
		}

		string MarkerText(Element e, Style s) {
			var ms = e.Marker;
			if (ms != null && ms.Content != null && ms.Content != "normal" && ms.Content != "none") return EvalContent(ms.Content, e, ms);
			if (s.ListStyleImage != null) return null;
			return MarkerFor(s.ListStyleType, CounterGet("list-item"));
		}

		public static string MarkerFor(string type, int n) {
			if (type == null) return null;
			if (type.StartsWith("\"") || type.StartsWith("'")) return Css.Unquote(type);
			switch (type) {
				case "none": return null;
				case "disc": return "• ";
				case "circle": return "◦ ";
				case "square": return "▪ ";
				case "disclosure-open": return "▾ ";
				case "disclosure-closed": return "▸ ";
			}
			return CounterText(n, type) + ". ";
		}

		public static string CounterText(int n, string type) {
			switch (type) {
				case "none": return "";
				case "disc": return "•";
				case "circle": return "◦";
				case "square": return "▪";
				case "decimal-leading-zero": return n < 10 && n >= 0 ? "0" + n : n.ToString(CultureInfo.InvariantCulture);
				case "lower-roman": return n > 0 && n < 4000 ? Roman(n).ToLowerInvariant() : n.ToString(CultureInfo.InvariantCulture);
				case "upper-roman": return n > 0 && n < 4000 ? Roman(n) : n.ToString(CultureInfo.InvariantCulture);
				case "lower-alpha": case "lower-latin": return n > 0 ? Alpha(n, "abcdefghijklmnopqrstuvwxyz") : n.ToString(CultureInfo.InvariantCulture);
				case "upper-alpha": case "upper-latin": return n > 0 ? Alpha(n, "ABCDEFGHIJKLMNOPQRSTUVWXYZ") : n.ToString(CultureInfo.InvariantCulture);
				case "lower-greek": return n > 0 ? Alpha(n, "αβγδεζηθικλμνξοπρστυφχψω") : n.ToString(CultureInfo.InvariantCulture);
				case "persian": return Digits(n, '۰');
				case "arabic-indic": return Digits(n, '٠');
				case "devanagari": return Digits(n, '०');
				case "bengali": return Digits(n, '০');
				case "thai": return Digits(n, '๐');
				case "cjk-decimal": return Digits(n, '〇');
				case "hebrew": return n > 0 && n < 1000 ? Hebrew(n) : n.ToString(CultureInfo.InvariantCulture);
				case "armenian": case "upper-armenian": return n.ToString(CultureInfo.InvariantCulture);
				case "abjad": case "arabic-abjad": return n > 0 ? Alpha(n, "أبجدهوزحطيكلمنسعفصقرشتثخذضظغ") : n.ToString(CultureInfo.InvariantCulture);
				case "alef": return n > 0 ? Alpha(n, "أبتثجحخدذرزسشصضطظعغفقكلمنهوي") : n.ToString(CultureInfo.InvariantCulture);
				default: return n.ToString(CultureInfo.InvariantCulture);
			}
		}

		static string Digits(int n, char zero) {
			string s = Math.Abs(n).ToString(CultureInfo.InvariantCulture);
			var sb = new StringBuilder();
			if (n < 0) sb.Append('-');
			foreach (char c in s) sb.Append((char)(zero + (c - '0')));
			return sb.ToString();
		}

		static string Roman(int n) {
			var v = new[] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
			var r = new[] { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
			var sb = new StringBuilder();
			for (int i = 0; i < v.Length; i++) while (n >= v[i]) { sb.Append(r[i]); n -= v[i]; }
			return sb.ToString();
		}

		static string Alpha(int n, string letters) {
			var chars = new List<string>();
			var e = StringInfo.GetTextElementEnumerator(letters);
			while (e.MoveNext()) chars.Add((string)e.Current);
			string s = "";
			while (n > 0) { n--; s = chars[n % chars.Count] + s; n /= chars.Count; }
			return s;
		}

		static string Hebrew(int n) {
			var vals = new[] { 400, 300, 200, 100, 90, 80, 70, 60, 50, 40, 30, 20, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
			var ch = new[] { "ת", "ש", "ר", "ק", "צ", "פ", "ע", "ס", "נ", "מ", "ל", "כ", "י", "ט", "ח", "ז", "ו", "ה", "ד", "ג", "ב", "א" };
			var sb = new StringBuilder();
			if (n % 100 == 15 || n % 100 == 16) { n -= n % 100; }
			for (int i = 0; i < vals.Length; i++) while (n >= vals[i]) { sb.Append(ch[i]); n -= vals[i]; }
			return sb.ToString();
		}

		Box Pseudo(Element e, Style ps, string which) {
			if (ps.Display == Disp.None) return null;
			if (ps.Content == null || ps.Content == "none" || ps.Content == "normal") return null;
			_frames.Add(new List<string>());
			try {
				ApplyCounters(null, ps);
				var b = new Box(ps.IsBlockLevel ? BK.Block : ps.Display == Disp.Flex ? BK.Flex : ps.Display == Disp.Grid ? BK.Grid : BK.Inline, null, ps) { Anon = false };
				if (ps.Display == Disp.Flex || ps.Display == Disp.InlineFlex) b.Kind = BK.Flex;
				else if (ps.Display == Disp.Grid || ps.Display == Disp.InlineGrid) b.Kind = BK.Grid;
				else if (ps.Display == Disp.InlineBlock || ps.IsBlockLevel || ps.Display == Disp.ListItem) b.Kind = BK.Block;
				var parts = ContentParts(ps.Content, e, ps);
				foreach (var p in parts) {
					if (p.img != null) b.Add(new Box(BK.Replaced, null, InlineChild(ps)) { Img = p.img, IntrW = p.img.W, IntrH = p.img.H });
					else if (p.text.Length > 0) b.Add(new Box(BK.Text, null, ps) { Text = p.text });
				}
				Fixup(b);
				return b;
			}
			finally {
				var frame = _frames[_frames.Count - 1]; _frames.RemoveAt(_frames.Count - 1);
				foreach (var n in frame) { var st = _counters[n]; st.RemoveAt(st.Count - 1); }
			}
		}

		static Style InlineChild(Style p) {
			var s = p.InheritFrom();
			s.Display = Disp.Inline;
			return s;
		}

		string EvalContent(string content, Element e, Style s) => string.Concat(ContentParts(content, e, s).Select(x => x.text));

		List<(string text, ImageData img)> ContentParts(string content, Element e, Style s) {
			var res = new List<(string, ImageData)>();
			foreach (var tok in Css.SplitWs(content)) {
				string t = tok.Trim();
				string lt = t.ToLowerInvariant();
				if (t == "/") break;
				if (t.StartsWith("\"") || t.StartsWith("'")) { res.Add((Css.Unquote(t), null)); continue; }
				if (lt.StartsWith("counter(")) {
					var args = Css.SplitTopLevel(t.Substring(8, t.Length - 9), ',').Select(x => x.Trim()).ToList();
					string style = args.Count > 1 ? args[1] : "decimal";
					res.Add((CounterText(CounterGet(args[0]), style), null));
					continue;
				}
				if (lt.StartsWith("counters(")) {
					var args = Css.SplitTopLevel(t.Substring(9, t.Length - 10), ',').Select(x => x.Trim()).ToList();
					string sep = args.Count > 1 ? Css.Unquote(args[1]) : ".";
					string style = args.Count > 2 ? args[2] : "decimal";
					res.Add((string.Join(sep, CounterAll(args[0]).Select(v => CounterText(v, style))), null));
					continue;
				}
				if (lt.StartsWith("attr(")) {
					string name = t.Substring(5, t.Length - 6).Trim().Split(' ', ',')[0];
					res.Add((e?.Attr(name) ?? "", null));
					continue;
				}
				if (lt.StartsWith("url(")) {
					var img = _ctx.LoadImage(Css.ExtractUrl(t));
					if (img != null) res.Add(("", img));
					continue;
				}
				if (lt == "open-quote" || lt == "close-quote" || lt == "no-open-quote" || lt == "no-close-quote") {
					bool open = lt.EndsWith("open-quote");
					var quotes = QuotesFor(s);
					if (!open) _quoteDepth = Math.Max(0, _quoteDepth - 1);
					int lvl = Math.Min(_quoteDepth, quotes.Count / 2 - 1);
					if (!lt.StartsWith("no-")) res.Add((quotes[lvl * 2 + (open ? 0 : 1)], null));
					if (open) _quoteDepth++;
					continue;
				}
			}
			return res;
		}

		static List<string> QuotesFor(Style s) {
			if (s.Quotes != null && s.Quotes != "none") {
				var q = Css.SplitWs(s.Quotes).Select(Css.Unquote).ToList();
				if (q.Count >= 2) return q;
			}
			string lang = (s.Lang ?? "").ToLowerInvariant();
			if (lang.StartsWith("fa") || lang.StartsWith("ar") || lang.StartsWith("ur") || lang.StartsWith("fr") || lang.StartsWith("ru")) return new List<string> { "«", "»", "«", "»" };
			if (lang.StartsWith("de")) return new List<string> { "„", "“", "‚", "‘" };
			return new List<string> { "“", "”", "‘", "’" };
		}

		Box MakeBox(Element e, Style s) {
			string tag = e.Tag;
			switch (tag) {
				case "br": return new Box(BK.Br, e, s);
				case "wbr": return new Box(BK.Wbr, e, s);
				case "img": {
					var b = new Box(BK.Replaced, e, s) { Alt = e.Attr("alt") };
					string src = e.Attr("src");
					if (string.IsNullOrWhiteSpace(src) && e.Attr("srcset") is string ss) src = ss.Split(',')[0].Trim().Split(' ')[0];
					if (!string.IsNullOrWhiteSpace(src)) b.Img = _ctx.LoadImage(src);
					if (b.Img != null) { b.IntrW = b.Img.W; b.IntrH = b.Img.H; }
					else if (!string.IsNullOrEmpty(b.Alt)) {
						var ib = new Box(s.IsInlineLevel ? BK.Inline : BK.Block, e, s);
						ib.Add(new Box(BK.Text, null, s) { Text = b.Alt });
						return ib;
					}
					else { b.IntrW = 0; b.IntrH = 0; }
					return b;
				}
				case "svg": {
					var b = new Box(BK.Replaced, e, s) { SvgEl = e };
					Svg.IntrinsicSize(e, out double w, out double h);
					b.IntrW = w; b.IntrH = h;
					return b;
				}
				case "input": {
					string type = (e.Attr("type") ?? "text").ToLowerInvariant();
					if (type == "hidden") return new Box(BK.Inline, e, s);
					if (type == "checkbox" || type == "radio") return new Box(BK.Replaced, e, s) { Control = (byte)(type == "checkbox" ? 1 : 2), IntrW = 13, IntrH = 13 };
					if (type == "image") {
						var b = new Box(BK.Replaced, e, s);
						var src = e.Attr("src");
						if (src != null) b.Img = _ctx.LoadImage(src);
						if (b.Img != null) { b.IntrW = b.Img.W; b.IntrH = b.Img.H; } else { b.IntrW = 0; b.IntrH = 0; }
						return b;
					}
					string text;
					bool button = type == "submit" || type == "button" || type == "reset";
					if (button) text = e.Attr("value") ?? (type == "submit" ? "Submit" : type == "reset" ? "Reset" : "");
					else if (type == "range" || type == "color" || type == "file") text = "";
					else {
						text = e.Attr("value") ?? "";
						if (type == "password") text = new string('•', text.Length);
					}
					var cs = s.Clone();
					var ib2 = new Box(BK.Block, e, cs);
					var ts = cs.InheritFrom();
					ts.Ws = WSp.Pre;
					if (text.Length == 0 && !button && e.Attr("placeholder") is string ph) { text = ph; ts.Color = new Rgba(117, 117, 117); }
					if (text.Length == 0) text = "​";
					ib2.Add(new Box(BK.Text, null, ts) { Text = text });
					if (!button && s.Width.IsAuto) { cs.Width = Len.PxV(e.Attr("size") is string sz && int.TryParse(sz, out int n) ? n * 7.5 + 3 : 153); }
					if (!button) cs.OverflowX = cs.OverflowY = 2;
					return ib2;
				}
				case "textarea": {
					var cs = s.Clone();
					var b = new Box(BK.Block, e, cs);
					string text = e.TextContent();
					if (text.Length == 0 && e.Attr("placeholder") is string ph) text = ph;
					if (text.Length == 0) text = "​";
					b.Add(new Box(BK.Text, null, cs) { Text = text });
					int rows = int.TryParse(e.Attr("rows"), out int r) ? r : 2;
					int cols = int.TryParse(e.Attr("cols"), out int c) ? c : 20;
					if (s.Width.IsAuto) cs.Width = Len.PxV(cols * s.FontSize * 0.6 + 4);
					if (s.Height.IsAuto) cs.Height = Len.PxV(rows * s.FontSize * 1.2 + 4);
					cs.OverflowX = cs.OverflowY = 2;
					return b;
				}
				case "select": {
					var cs = s.Clone();
					var b = new Box(BK.Block, e, cs);
					var opts = e.Descendants().Where(x => x.Tag == "option").ToList();
					var sel = opts.FirstOrDefault(o => o.Attr("selected") != null) ?? opts.FirstOrDefault();
					string text = sel != null ? HtmlParser.CollapseWs(sel.TextContent()).Trim() : "";
					var ts = cs.InheritFrom();
					ts.Ws = WSp.Pre;
					b.Add(new Box(BK.Text, null, ts) { Text = (text.Length == 0 ? "​" : text) + "  ▾" });
					if (!s.Padding.Any(p => p.IsValue && p.Resolve(0) > 0)) { cs.Padding[0] = Len.PxV(1); cs.Padding[2] = Len.PxV(1); cs.Padding[1] = Len.PxV(4); cs.Padding[3] = Len.PxV(4); }
					return b;
				}
				case "progress": case "meter": {
					var b = new Box(BK.Replaced, e, s) { Control = 3, IntrW = 160, IntrH = 16 };
					return b;
				}
				case "iframe": case "video": case "canvas": case "embed": case "object": case "audio": {
					if (tag == "object" && e.Attr("data") is string data) {
						var img = _ctx.LoadImage(data);
						if (img != null) return new Box(BK.Replaced, e, s) { Img = img, IntrW = img.W, IntrH = img.H };
					}
					if (tag == "video" && e.Attr("poster") is string poster) {
						var img = _ctx.LoadImage(poster);
						if (img != null) return new Box(BK.Replaced, e, s) { Img = img, IntrW = 300, IntrH = 150 };
					}
					return new Box(BK.Replaced, e, s) { IntrW = tag == "audio" ? 300 : 300, IntrH = tag == "audio" ? 54 : 150 };
				}
			}
			switch (s.Display) {
				case Disp.Table: case Disp.InlineTable: return new Box(BK.Table, e, s);
				case Disp.RowGroup: case Disp.HeaderGroup: case Disp.FooterGroup: return new Box(BK.RowGroup, e, s) { };
				case Disp.Row: return new Box(BK.Row, e, s);
				case Disp.Cell: {
					var b = new Box(BK.Cell, e, s);
					if (int.TryParse(e.Attr("colspan"), out int cs) && cs > 0) b.ColSpan = Math.Min(cs, 1000);
					if (int.TryParse(e.Attr("rowspan"), out int rs) && rs >= 0) b.RowSpan = rs == 0 ? 65534 : Math.Min(rs, 65534);
					return b;
				}
				case Disp.Caption: return new Box(BK.Caption, e, s);
				case Disp.Column: {
					var b = new Box(BK.Column, e, s);
					if (int.TryParse(e.Attr("span"), out int sp) && sp > 0) b.ColSpan = sp;
					return b;
				}
				case Disp.ColumnGroup: {
					var b = new Box(BK.ColGroup, e, s);
					if (int.TryParse(e.Attr("span"), out int sp) && sp > 0) b.ColSpan = sp;
					return b;
				}
				case Disp.Flex: case Disp.InlineFlex: return new Box(BK.Flex, e, s);
				case Disp.Grid: case Disp.InlineGrid: return new Box(BK.Grid, e, s);
				case Disp.Inline: return new Box(BK.Inline, e, s);
				default: return new Box(BK.Block, e, s);
			}
		}

		static bool IsCollapsibleWs(Box b) {
			if (b.Kind != BK.Text) return false;
			if (b.S.Ws == WSp.Pre || b.S.Ws == WSp.PreWrap || b.S.Ws == WSp.BreakSpaces) return false;
			foreach (char c in b.Text) if (c != ' ' && c != '\t' && c != '\n' && c != '\r' && c != '\f') return false;
			if (b.S.Ws == WSp.PreLine && b.Text.Contains('\n')) return false;
			return true;
		}

		static Style AnonStyle(Style parent, Disp d) {
			var s = parent.InheritFrom();
			s.Display = d;
			return s;
		}

		public void Fixup(Box b) {
			foreach (var k in b.Kids.ToList()) {
				if (k.Kind == BK.Inline && k.Kids.Any(c => c.IsBlockLevel)) {
					k.Kind = BK.Block;
					var ns = k.S.Clone();
					ns.Display = Disp.Block;
					k.S = ns;
					WrapInlineRuns(k);
				}
			}
			switch (b.Kind) {
				case BK.Table: FixTable(b); return;
				case BK.RowGroup: FixRowGroup(b); return;
				case BK.Row: FixRow(b); return;
				case BK.Flex: case BK.Grid: FixFlexItems(b); return;
				case BK.Inline: return;
			}
			if (b.Kind == BK.Block || b.Kind == BK.Cell || b.Kind == BK.Caption) {
				if (b.Kids.Any(k => k.Kind == BK.Row || k.Kind == BK.Cell || k.Kind == BK.RowGroup || k.Kind == BK.Caption && false)) WrapOrphanTableParts(b);
				WrapInlineRuns(b);
			}
		}

		void WrapInlineRuns(Box b) {
			bool anyBlock = b.Kids.Any(k => k.IsBlockLevel);
			if (!anyBlock) return;
			var res = new List<Box>();
			var run = new List<Box>();
			void Flush() {
				if (run.Count == 0) return;
				bool meaningful = run.Any(k => k.IsInlineLevel && !IsCollapsibleWs(k));
				if (meaningful) {
					var ab = new Box(BK.Block, null, AnonStyle(b.S, Disp.Block)) { Anon = true };
					foreach (var k in run) ab.Add(k);
					res.Add(ab);
				}
				else foreach (var k in run) if (!k.IsInlineLevel) res.Add(k);
				run.Clear();
			}
			foreach (var k in b.Kids) {
				if (k.IsBlockLevel) { Flush(); res.Add(k); }
				else run.Add(k);
			}
			Flush();
			b.Kids.Clear();
			foreach (var k in res) b.Add(k);
		}

		void WrapOrphanTableParts(Box b) {
			var res = new List<Box>();
			Box cur = null;
			foreach (var k in b.Kids) {
				bool part = k.Kind == BK.Row || k.Kind == BK.Cell || k.Kind == BK.RowGroup;
				if (part) {
					if (cur == null) { cur = new Box(BK.Table, null, AnonStyle(b.S, Disp.Table)) { Anon = true }; res.Add(cur); }
					cur.Add(k);
				}
				else if (cur == null || !IsCollapsibleWs(k)) { cur = null; res.Add(k); }
			}
			b.Kids.Clear();
			foreach (var k in res) { b.Add(k); if (k.Anon && k.Kind == BK.Table) FixTable(k); }
		}

		void FixTable(Box t) {
			var res = new List<Box>();
			Box grp = null;
			foreach (var k in t.Kids) {
				if (k.Kind == BK.Caption || k.Kind == BK.Column || k.Kind == BK.ColGroup || k.Kind == BK.RowGroup) { grp = null; res.Add(k); continue; }
				if (k.IsOutOfFlow) { res.Add(k); continue; }
				if (k.Kind == BK.Text && IsCollapsibleWs(k)) continue;
				if (grp == null) { grp = new Box(BK.RowGroup, null, AnonStyle(t.S, Disp.RowGroup)) { Anon = true }; res.Add(grp); }
				grp.Add(k);
			}
			t.Kids.Clear();
			foreach (var k in res) { t.Add(k); if (k.Kind == BK.RowGroup && k.Anon) FixRowGroup(k); }
		}

		void FixRowGroup(Box g) {
			var res = new List<Box>();
			Box row = null;
			foreach (var k in g.Kids) {
				if (k.Kind == BK.Row) { row = null; res.Add(k); continue; }
				if (k.Kind == BK.Text && IsCollapsibleWs(k)) continue;
				if (k.IsOutOfFlow) { res.Add(k); continue; }
				if (row == null) { row = new Box(BK.Row, null, AnonStyle(g.S, Disp.Row)) { Anon = true }; res.Add(row); }
				row.Add(k);
			}
			g.Kids.Clear();
			foreach (var k in res) { g.Add(k); if (k.Kind == BK.Row && k.Anon) FixRow(k); }
		}

		void FixRow(Box r) {
			var res = new List<Box>();
			Box cell = null;
			foreach (var k in r.Kids) {
				if (k.Kind == BK.Cell) { cell = null; res.Add(k); continue; }
				if (k.Kind == BK.Text && IsCollapsibleWs(k)) continue;
				if (k.IsOutOfFlow) { res.Add(k); continue; }
				if (cell == null) { cell = new Box(BK.Cell, null, AnonStyle(r.S, Disp.Cell)) { Anon = true }; res.Add(cell); }
				cell.Add(k);
			}
			r.Kids.Clear();
			foreach (var k in res) { r.Add(k); if (k.Kind == BK.Cell && k.Anon) Fixup(k); }
		}

		void FixFlexItems(Box b) {
			var res = new List<Box>();
			var run = new List<Box>();
			void Flush() {
				if (run.Count == 0) return;
				if (run.Any(k => !IsCollapsibleWs(k))) {
					var ab = new Box(BK.Block, null, AnonStyle(b.S, Disp.Block)) { Anon = true };
					foreach (var k in run) ab.Add(k);
					res.Add(ab);
				}
				run.Clear();
			}
			foreach (var k in b.Kids) {
				if (k.Kind == BK.Text || k.Kind == BK.Br || k.Kind == BK.Wbr) { run.Add(k); continue; }
				Flush();
				if (k.Kind == BK.Inline) {
					k.Kind = BK.Block;
					var ns = k.S.Clone(); ns.Display = Disp.Block; k.S = ns;
					WrapInlineRuns(k);
				}
				res.Add(k);
			}
			Flush();
			b.Kids.Clear();
			foreach (var k in res) b.Add(k);
		}

		public static void AssignDecorations(Box b, List<Deco> inherited) {
			var list = inherited;
			if (b.S.TextDecoLine != 0 && b.Kind != BK.Text) {
				list = inherited == null ? new List<Deco>() : new List<Deco>(inherited);
				list.Add(new Deco { Lines = b.S.TextDecoLine, Color = b.S.TextDecoColor, Style = b.S.TextDecoStyle, Thickness = b.S.TextDecoThickness, Offset = b.S.UnderlineOffset });
			}
			b.Decos = list;
			foreach (var k in b.Kids) {
				bool stop = k.IsAtomicInline || k.IsFloat || k.IsOutOfFlow || k.Kind == BK.Table;
				AssignDecorations(k, stop ? null : list);
			}
		}
	}

	internal enum IK : byte { Text, Open, Close, Atomic, Br, Float, Abs }

	internal sealed class IItem {
		public IK K;
		public Box Box;
		public Style S;
		public int Start, End;
		public int Parent = -1;
		public int Match = -1;
		public double AtomicW;
		public List<Deco> Decos;
	}

	internal sealed class ShapeSeg {
		public int Start, End, Item;
		public ResolvedFont F;
		public ShapedRun Run;
		public byte Level;
		public Style S;
	}

	internal sealed class InlineData {
		public string Text;
		public List<IItem> Items = new();
		public int[] ItemAt;
		public byte[] Levels;
		public BC[] Bidi;
		public byte ParaLevel;
		public double[] Adv;
		public List<ShapeSeg> Segs = new();
		public int[] SegAt;
		public byte[] Brk;
		public bool[] Coll;
		public bool[] Hang;
		public double[] OpenSp, CloseSp;
		public List<int>[] OpenAt, CloseAt, ZeroAt;
	}

	internal sealed class GlyphFrag {
		public ResolvedFont RF;
		public ushort[] G;
		public double[] X, Y;
		public string[] U;
		public Rgba Color;
		public Style S;
		public double Baseline, X0, X1;
		public List<Deco> Decos;
		public bool Hidden;
		public Box Owner;
		public double AnchorTop = double.NaN;
		public void Shift(double dx, double dy) {
			for (int i = 0; i < X.Length; i++) { X[i] += dx; Y[i] += dy; }
			Baseline += dy; X0 += dx; X1 += dx; AnchorTop += dy;
		}
	}

	internal sealed class InlineBoxFrag {
		public Box Box;
		public double X, W, Top, Bottom;
		public bool First, Last;
	}

	internal sealed class LineBox {
		public double Y, H, Baseline, X, W;
		public List<GlyphFrag> Glyphs = new();
		public List<InlineBoxFrag> Boxes = new();
		public List<Box> Atomics = new();
		public void Shift(double dx, double dy) {
			Y += dy; Baseline += dy; X += dx;
			foreach (var g in Glyphs) g.Shift(dx, dy);
			foreach (var b in Boxes) { b.X += dx; b.Top += dy; b.Bottom += dy; }
		}
	}

	internal sealed class Piece {
		public byte K;
		public int S, E, Item;
		public ShapeSeg Seg;
		public byte Level;
		public double W, X;
		public int Spaces;
	}

	internal sealed class LineInfo {
		public int Start, End, ContentEnd;
		public List<Piece> Pieces;
		public double H, BaseOff, Left, Avail, Indent;
		public bool Empty, LastInPara, EndsForced;
		public Dictionary<int, (double shift, double ta, double td)> BoxShift;
		public double Width;
		public double Y;
	}

	internal sealed partial class LayoutEngine {
		public RenderContext Ctx;
		public FontProvider Fonts => Ctx.Fonts;
		public double PageH = double.PositiveInfinity, PageW;
		public bool Paginate;
		public double Reserve, ReserveBottom;
		public List<Box> Fixed = new();
		readonly Dictionary<(FaceInfo, double, bool, bool), ResolvedFont> _rfCache = new();
		readonly Dictionary<(string, int, Sc), ResolvedFont> _pickCache = new();
		readonly Dictionary<string, ResolvedFont> _primaryCache = new();

		public LayoutEngine(RenderContext ctx) { Ctx = ctx; }

		public static double LU(double v) => Math.Floor(v * 64 + 1e-7) / 64;

		public double NextPageTop(double y) {
			if (!Paginate) return y;
			double p = Math.Floor((y + 1e-6) / PageH) + 1;
			return p * PageH + Reserve;
		}

		public bool Crosses(double y, double h) {
			if (!Paginate || h <= 0) return false;
			return y + h - 1e-6 > PageLimit(y);
		}

		public double PageLimit(double y) => (Math.Floor((y + 1e-6) / PageH) + 1) * PageH - ReserveBottom;

		public bool AtPageTop(double y) {
			if (!Paginate) return false;
			double off = y - Math.Floor((y + 1e-6) / PageH) * PageH;
			return off <= Reserve + 0.01;
		}

		string FontKey(Style s) {
			if (s.FKey != null) return s.FKey;
			s.FKey = string.Join(",", s.FontFamily) + "|" + s.FontWeight + "|" + s.FontStyle + "|" + s.FontSize.ToString("R", CultureInfo.InvariantCulture);
			return s.FKey;
		}

		ResolvedFont Resolve(FaceInfo face, Style s) {
			var ff = face.Get();
			bool sb = s.FontWeight >= 600 && face.Weight < 600 && face.WeightMax < 600;
			bool si = s.FontStyle != 0 && !face.Italic;
			var key = (face, s.FontSize, sb, si);
			if (_rfCache.TryGetValue(key, out var r)) return r;
			r = new ResolvedFont { File = ff, Face = face, Size = s.FontSize, SynthBold = sb, SynthItalic = si };
			r.Init();
			_rfCache[key] = r;
			return r;
		}

		public ResolvedFont Primary(Style s) {
			string k = FontKey(s);
			if (_primaryCache.TryGetValue(k, out var r)) return r;
			FaceInfo face = null;
			foreach (var fam in s.FontFamily) {
				face = Fonts.Match(fam, s.FontWeight, s.FontStyle);
				if (face?.Get() != null) break;
				face = null;
			}
			if (face == null) face = Fonts.Match("serif", s.FontWeight, s.FontStyle);
			if (face?.Get() == null) face = Fonts.AnyFace();
			if (face == null) throw new InvalidOperationException("No usable font was found. Add font files via HtmlToPdfOptions.FontFiles or FontDirectories.");
			r = Resolve(face, s);
			_primaryCache[k] = r;
			return r;
		}

		public ResolvedFont PickFont(Style s, int cp, Sc hint = Sc.Common) {
			string k = FontKey(s);
			var key = (k, cp, hint);
			if (_pickCache.TryGetValue(key, out var r)) return r;
			foreach (var fam in s.FontFamily) {
				var face = Fonts.Match(fam, s.FontWeight, s.FontStyle);
				var ff = face?.Get();
				if (ff != null && face.Covers(cp) && ff.HasGlyph(cp)) { r = Resolve(face, s); _pickCache[key] = r; return r; }
			}
			var fb = Fonts.Fallback(cp, s.FontWeight, s.FontStyle, hint);
			r = fb != null ? Resolve(fb, s) : Primary(s);
			_pickCache[key] = r;
			return r;
		}

		static bool Wraps(Style s) => s.Ws != WSp.NoWrap && s.Ws != WSp.Pre;
		static bool Collapses(Style s) => s.Ws == WSp.Normal || s.Ws == WSp.NoWrap || s.Ws == WSp.PreLine;

		public InlineData BuildInline(Box c) {
			if (c.IData != null) return c.IData;
			var d = new InlineData();
			var sb = new StringBuilder();
			var coll = new List<bool>();
			var hang = new List<bool>();
			bool lastSpace = true;
			void Append(char ch, bool isColl, bool isHang) { sb.Append(ch); coll.Add(isColl); hang.Add(isHang); }

			void Walk(Box b, int parent) {
				foreach (var k in b.Kids) {
					if (k.IsOutOfFlow) {
						d.Items.Add(new IItem { K = IK.Abs, Box = k, S = k.S, Start = sb.Length, End = sb.Length, Parent = parent });
						continue;
					}
					if (k.IsFloat) {
						d.Items.Add(new IItem { K = IK.Float, Box = k, S = k.S, Start = sb.Length, End = sb.Length, Parent = parent });
						continue;
					}
					switch (k.Kind) {
						case BK.Text: {
							var s = k.S;
							int st = sb.Length;
							bool collapse = Collapses(s);
							bool keepNl = s.Ws != WSp.Normal && s.Ws != WSp.NoWrap;
							string t = k.Text;
							bool capNext = true;
							if (b.Kind == BK.Inline || b == c) {
								for (int i = st - 1; i >= 0; i--) { char pc = sb[i]; if (char.IsLetterOrDigit(pc)) { capNext = false; break; } if (pc == ' ' || pc == '\n') break; }
							}
							for (int i = 0; i < t.Length; i++) {
								char ch = t[i];
								if (ch == '\r') continue;
								if (ch == '\n' && keepNl) {
									if (s.Ws == WSp.PreLine) { while (sb.Length > st && coll[sb.Length - 1]) { sb.Length--; coll.RemoveAt(coll.Count - 1); hang.RemoveAt(hang.Count - 1); } }
									Append('\n', false, false);
									lastSpace = collapse;
									capNext = true;
									continue;
								}
								if (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\f') {
									if (collapse) {
										if (lastSpace) continue;
										Append(' ', true, false);
										lastSpace = true;
									}
									else {
										Append(ch == '\n' || ch == '\f' ? ' ' : ch, false, s.Ws == WSp.PreWrap);
										lastSpace = false;
									}
									capNext = true;
									continue;
								}
								lastSpace = false;
								if (s.TextTransform != 0) {
									if (char.IsHighSurrogate(ch) && i + 1 < t.Length) { Append(ch, false, false); Append(t[++i], false, false); capNext = false; continue; }
									switch (s.TextTransform) {
										case 1: ch = char.ToUpperInvariant(ch); break;
										case 2: ch = char.ToLowerInvariant(ch); break;
										case 3: if (capNext && char.IsLetter(ch)) ch = char.ToUpperInvariant(ch); break;
										case 4: if (ch >= '!' && ch <= '~') ch = (char)(ch - 0x21 + 0xFF01); break;
									}
									capNext = !char.IsLetterOrDigit(ch) && ch != '\'' && ch != '’';
									if (s.TextTransform == 1 && ch == 'ß') { Append('S', false, false); ch = 'S'; }
								}
								Append(ch, false, false);
							}
							if (sb.Length > st) d.Items.Add(new IItem { K = IK.Text, Box = k, S = s, Start = st, End = sb.Length, Parent = parent, Decos = b.Decos });
							break;
						}
						case BK.Br:
							d.Items.Add(new IItem { K = IK.Br, Box = k, S = k.S, Start = sb.Length, End = sb.Length + 1, Parent = parent });
							Append('\n', false, false);
							lastSpace = true;
							break;
						case BK.Wbr:
							Append('​', false, false);
							break;
						case BK.Inline: {
							int oi = d.Items.Count;
							var open = new IItem { K = IK.Open, Box = k, S = k.S, Start = sb.Length, Parent = parent, Decos = k.Decos };
							d.Items.Add(open);
							string pre = null, post = null;
							bool r = k.S.Rtl;
							string dirAttr = k.El?.Attr("dir")?.Trim().ToLowerInvariant();
							bool autoDir = k.El != null && (dirAttr == "auto" || k.El.Tag == "bdi" && dirAttr == null);
							if (autoDir && (k.S.UnicodeBidi == 2 || k.S.UnicodeBidi == 0)) { pre = "\u2068"; post = "\u2069"; }
							else switch (k.S.UnicodeBidi) {
								case 1: pre = r ? "‫" : "‪"; post = "‬"; break;
								case 2: pre = r ? "⁧" : "⁦"; post = "⁩"; break;
								case 3: pre = r ? "‮" : "‭"; post = "‬"; break;
								case 4: pre = (r ? "⁧" : "⁦") + (r ? "‮" : "‭"); post = "‬⁩"; break;
								case 5: pre = "⁨"; post = "⁩"; break;
							}
							if (pre != null) foreach (char ch in pre) Append(ch, false, false);
							open.End = sb.Length;
							Walk(k, oi);
							var close = new IItem { K = IK.Close, Box = k, S = k.S, Start = sb.Length, Parent = parent, Match = oi };
							if (post != null) foreach (char ch in post) Append(ch, false, false);
							close.End = sb.Length;
							open.Match = d.Items.Count;
							d.Items.Add(close);
							break;
						}
						default:
							d.Items.Add(new IItem { K = IK.Atomic, Box = k, S = k.S, Start = sb.Length, End = sb.Length + 1, Parent = parent });
							Append('￼', false, false);
							lastSpace = false;
							break;
					}
				}
			}
			Walk(c, -1);
			d.Text = sb.ToString();
			d.Coll = coll.ToArray();
			d.Hang = hang.ToArray();
			int n = d.Text.Length;
			d.ItemAt = new int[n];
			for (int i = 0; i < n; i++) d.ItemAt[i] = -1;
			for (int ii = 0; ii < d.Items.Count; ii++) {
				var it = d.Items[ii];
				for (int p = it.Start; p < it.End; p++) d.ItemAt[p] = ii;
			}
			d.OpenSp = new double[n + 1];
			d.CloseSp = new double[n + 1];
			d.OpenAt = new List<int>[n + 1];
			d.CloseAt = new List<int>[n + 1];
			d.ZeroAt = new List<int>[n + 1];
			for (int ii = 0; ii < d.Items.Count; ii++) {
				var it = d.Items[ii];
				if (it.K == IK.Open) (d.OpenAt[it.Start] ??= new()).Add(ii);
				else if (it.K == IK.Close) (d.CloseAt[it.Start] ??= new()).Add(ii);
				else if (it.K == IK.Float || it.K == IK.Abs) (d.ZeroAt[it.Start] ??= new()).Add(ii);
			}

			var ps = c.S;
			d.ParaLevel = (byte)(ps.Rtl ? 1 : 0);
			d.Bidi = new BC[n];
			var cps = new int[n];
			for (int i = 0; i < n; i++) {
				char ch = d.Text[i];
				if (char.IsLowSurrogate(ch) && i > 0 && char.IsHighSurrogate(d.Text[i - 1])) { d.Bidi[i] = BC.BN; cps[i] = ch; continue; }
				int cp = Uni.CodePointAt(d.Text, i);
				cps[i] = cp;
				d.Bidi[i] = ch == '\n' ? BC.B : Uni.Bidi(cp);
			}
			d.Levels = new byte[n];
			int ps0 = 0;
			for (int i = 0; i <= n; i++) {
				if (i == n || d.Bidi[i] == BC.B) {
					if (i > ps0 || i < n) {
						int len = i - ps0 + (i < n ? 1 : 0);
						var sub = new BC[len];
						var subCp = new int[len];
						Array.Copy(d.Bidi, ps0, sub, 0, len);
						Array.Copy(cps, ps0, subCp, 0, len);
						byte pl = d.ParaLevel;
						if (ps.UnicodeBidi == 5) pl = BidiAlgo.ParagraphLevel(sub, 0, len);
						bool needs = pl == 1 || sub.Any(x => x == BC.R || x == BC.AL || x == BC.AN || x == BC.RLE || x == BC.RLO || x == BC.RLI || x == BC.FSI || x == BC.LRO || x == BC.LRE || x == BC.LRI);
						var lv = needs ? BidiAlgo.Resolve(subCp, sub, pl) : new byte[len];
						Array.Copy(lv, 0, d.Levels, ps0, len);
					}
					ps0 = i + 1;
				}
			}
			var forms = Shaper.JoiningForms(d.Text, 0, n);
			d.Adv = new double[n];
			d.SegAt = new int[n];
			ShapeAll(d, forms);
			ComputeBreaks(d);
			c.IData = d;
			return d;
		}

		void ShapeAll(InlineData d, byte[] forms) {
			string t = d.Text;
			int n = t.Length;
			var fonts = new ResolvedFont[n];
			var scripts = new Sc[n];
			for (int i = 0; i < n; i++) {
				if (char.IsLowSurrogate(t[i]) && i > 0 && char.IsHighSurrogate(t[i - 1])) { scripts[i] = scripts[i - 1]; continue; }
				scripts[i] = Uni.Script(Uni.CodePointAt(t, i));
			}
			var own = (Sc[])scripts.Clone();
			Sc last = Sc.Common;
			for (int i = 0; i < n; i++) {
				if (scripts[i] == Sc.Common || scripts[i] == Sc.Inherited) scripts[i] = last;
				else last = scripts[i];
			}
			last = Sc.Common;
			for (int i = n - 1; i >= 0; i--) {
				if (scripts[i] == Sc.Common) scripts[i] = last;
				else last = scripts[i];
			}
			ResolvedFont prev = null;
			Style prevStyle = null;
			for (int i = 0; i < n; i++) {
				int ii = d.ItemAt[i];
				if (ii < 0 || d.Items[ii].K != IK.Text) { prev = null; continue; }
				var s = d.Items[ii].S;
				char ch = t[i];
				if (char.IsLowSurrogate(ch) && i > 0 && char.IsHighSurrogate(t[i - 1])) { fonts[i] = fonts[i - 1]; continue; }
				int cp = Uni.CodePointAt(t, i);
				bool attach = Uni.IsMark(cp) || cp == 0x200C || cp == 0x200D || Uni.IsVariationSelector(cp) || cp >= 0x1F3FB && cp <= 0x1F3FF;
				if (attach && prev != null && prevStyle == s) { fonts[i] = prev; continue; }
				if (Uni.IsDefaultIgnorable(cp) || cp == '\n' || cp == '\t') { fonts[i] = prev != null && prevStyle == s ? prev : Primary(s); continue; }
				fonts[i] = PickFont(s, cp, own[i] == Sc.Common || own[i] == Sc.Inherited ? scripts[i] : Sc.Common);
				prev = fonts[i];
				prevStyle = s;
			}
			int segStart = -1;
			for (int i = 0; i <= n; i++) {
				bool boundary = i == n || segStart < 0;
				if (!boundary) {
					int ii = d.ItemAt[i];
					bool isText = ii >= 0 && d.Items[ii].K == IK.Text;
					int pi = d.ItemAt[segStart];
					if (!isText || ii != pi || fonts[i] != fonts[segStart] || d.Levels[i] != d.Levels[segStart] || scripts[i] != scripts[segStart] && !(char.IsLowSurrogate(t[i]))) boundary = true;
				}
				if (boundary) {
					if (segStart >= 0) MakeSeg(d, segStart, i, fonts[segStart], scripts[segStart], forms);
					segStart = -1;
					if (i < n) {
						int ii = d.ItemAt[i];
						if (ii >= 0 && d.Items[ii].K == IK.Text && fonts[i] != null) segStart = i;
						else d.SegAt[i] = -1;
					}
				}
			}
			for (int i = 0; i < n; i++) {
				int ii = d.ItemAt[i];
				if (ii < 0 || d.Items[ii].K != IK.Text) continue;
				var s = d.Items[ii].S;
				char ch = t[i];
				if (s.LetterSpacing != 0 && (!Cursive(scripts[i]) || ch == ' ' || ch == '\u00A0' || ch == '\t') && !char.IsLowSurrogate(ch) && !Uni.IsMark(Uni.CodePointAt(t, i)) && !Uni.IsDefaultIgnorable(ch) && ch != '\n') d.Adv[i] += s.LetterSpacing;
				if (s.WordSpacing != 0 && (ch == ' ' || ch == ' ' || ch == '፡')) d.Adv[i] += s.WordSpacing;
			}
		}

		static bool Cursive(Sc s) => s == Sc.Arabic || s == Sc.Syriac || s == Sc.Nko || s == Sc.Mongolian;

		void MakeSeg(InlineData d, int s, int e, ResolvedFont f, Sc script, byte[] forms) {
			var it = d.Items[d.ItemAt[s]];
			var st = it.S;
			var o = new Shaper.Opts {
				Kerning = !st.KerningOff,
				Ligatures = !st.LigaturesOff && (st.LetterSpacing == 0 || Cursive(script)),
				Lang = st.Lang,
				Tabular = st.TabularNums,
				SmallCaps = st.FontVariantCaps != 0,
				Features = ParseFeatures(st.FontFeatures)
			};
			var run = Shaper.Shape(f.File, d.Text, s, e, (d.Levels[s] & 1) == 1, script, forms, 0, o);
			double scale = f.Size / f.File.UnitsPerEm;
			for (int k = 0; k < run.Count; k++) {
				int cl = run.Cl[k];
				if (cl >= s && cl < e) d.Adv[cl] += run.Adv[k] * scale;
			}
			for (int i = s; i < e; i++) if (d.Text[i] == '\t' && !Collapses(st)) d.Adv[i] = 0;
			var seg = new ShapeSeg { Start = s, End = e, Item = d.ItemAt[s], F = f, Run = run, Level = d.Levels[s], S = st };
			int si = d.Segs.Count;
			d.Segs.Add(seg);
			for (int i = s; i < e; i++) d.SegAt[i] = si;
		}

		static List<(string, int)> ParseFeatures(string ff) {
			if (string.IsNullOrWhiteSpace(ff)) return null;
			var res = new List<(string, int)>();
			foreach (var p in Css.SplitTopLevel(ff, ',')) {
				var t = Css.SplitWs(p.Trim());
				if (t.Count == 0) continue;
				string tag = Css.Unquote(t[0]);
				if (tag.Length != 4) continue;
				int v = 1;
				if (t.Count > 1) { if (t[1] == "off") v = 0; else if (t[1] == "on") v = 1; else int.TryParse(t[1], out v); }
				res.Add((tag, v));
			}
			return res;
		}

		void ComputeBreaks(InlineData d) {
			string t = d.Text;
			int n = t.Length;
			d.Brk = new byte[n];
			if (n == 0) return;
			var cls = new LBC[n];
			for (int i = 0; i < n; i++) {
				char ch = t[i];
				if (char.IsLowSurrogate(ch) && i > 0 && char.IsHighSurrogate(t[i - 1])) { cls[i] = LBC.CM; continue; }
				int cp = Uni.CodePointAt(t, i);
				if (cp >= 0x202A && cp <= 0x202E || cp >= 0x2066 && cp <= 0x2069 || cp == 0x200E || cp == 0x200F || cp == 0x61C) { cls[i] = LBC.CM; continue; }
				var c = Uni.LineBreak(cp);
				if (c == LBC.SA) c = LBC.AL;
				cls[i] = c;
			}
			for (int i = 0; i < n; i++) {
				if (cls[i] == LBC.CM || cls[i] == LBC.ZWJ) {
					if (i == 0 || cls[i - 1] == LBC.SP || cls[i - 1] == LBC.BK || cls[i - 1] == LBC.CR || cls[i - 1] == LBC.LF || cls[i - 1] == LBC.ZW) cls[i] = LBC.AL;
				}
			}
			var eff = new LBC[n];
			for (int i = 0; i < n; i++) eff[i] = cls[i] == LBC.CM || cls[i] == LBC.ZWJ ? (i > 0 ? eff[i - 1] : LBC.AL) : cls[i];
			int riCount = 0;
			for (int i = 0; i < n - 1; i++) {
				LBC a = eff[i], b = cls[i + 1];
				byte r = Rule(a, b, i);
				d.Brk[i] = r;
				byte Rule(LBC a0, LBC b0, int idx) {
					LBC orig = cls[idx];
					if (orig == LBC.BK || orig == LBC.LF || orig == LBC.CR && b0 != LBC.LF) return 2;
					if (orig == LBC.CR && b0 == LBC.LF) return 0;
					if (b0 == LBC.BK || b0 == LBC.CR || b0 == LBC.LF) return 0;
					if (b0 == LBC.SP || b0 == LBC.ZW) return 0;
					if (orig == LBC.ZW) return 1;
					if (a0 == LBC.SP) {
						int k = idx; while (k >= 0 && cls[k] == LBC.SP) k--;
						if (k >= 0 && cls[k] == LBC.ZW) return 1;
					}
					if (orig == LBC.ZWJ) return 0;
					if (b0 == LBC.CM || b0 == LBC.ZWJ) return 0;
					if (a0 == LBC.WJ || b0 == LBC.WJ) return 0;
					if (a0 == LBC.GL) return 0;
					if (b0 == LBC.GL && a0 != LBC.SP && a0 != LBC.BA && a0 != LBC.HY) return 0;
					if (b0 == LBC.CL || b0 == LBC.CP || b0 == LBC.EX || b0 == LBC.IS || b0 == LBC.SY) return 0;
					LBC bs = a0;
					if (a0 == LBC.SP) { int k = idx; while (k >= 0 && cls[k] == LBC.SP) k--; bs = k >= 0 ? eff[k] : LBC.AL; }
					if (bs == LBC.OP) return 0;
					if (bs == LBC.QU && b0 == LBC.OP) return 0;
					if ((bs == LBC.CL || bs == LBC.CP) && b0 == LBC.NS) return 0;
					if (bs == LBC.B2 && b0 == LBC.B2) return 0;
					if (a0 == LBC.SP) return 1;
					if (b0 == LBC.QU || a0 == LBC.QU) return 0;
					if (b0 == LBC.CB || a0 == LBC.CB) return 1;
					if (b0 == LBC.BA || b0 == LBC.HY || b0 == LBC.NS || a0 == LBC.BB) return 0;
					if (b0 == LBC.IN) return 0;
					if (a0 == LBC.AL && b0 == LBC.NU || a0 == LBC.NU && b0 == LBC.AL) return 0;
					if (a0 == LBC.PR && (b0 == LBC.ID || b0 == LBC.EM) || (a0 == LBC.ID || a0 == LBC.EM) && b0 == LBC.PO) return 0;
					if ((a0 == LBC.PR || a0 == LBC.PO) && b0 == LBC.AL || a0 == LBC.AL && (b0 == LBC.PR || b0 == LBC.PO)) return 0;
					if ((a0 == LBC.PR || a0 == LBC.PO) && (b0 == LBC.NU || b0 == LBC.OP && idx + 2 < n && cls[idx + 2] == LBC.NU)) return 0;
					if ((a0 == LBC.OP || a0 == LBC.HY) && b0 == LBC.NU) return 0;
					if (a0 == LBC.NU && (b0 == LBC.NU || b0 == LBC.SY || b0 == LBC.IS)) return 0;
					if ((a0 == LBC.SY || a0 == LBC.IS || a0 == LBC.CL || a0 == LBC.CP) && b0 == LBC.NU) { int k = idx; while (k >= 0 && (eff[k] == LBC.SY || eff[k] == LBC.IS || eff[k] == LBC.CL || eff[k] == LBC.CP)) k--; if (k >= 0 && eff[k] == LBC.NU && (a0 == LBC.SY || a0 == LBC.IS)) return 0; }
					if ((a0 == LBC.NU || a0 == LBC.SY || a0 == LBC.IS || a0 == LBC.CL || a0 == LBC.CP) && (b0 == LBC.PO || b0 == LBC.PR)) { int k = idx; while (k >= 0 && (eff[k] == LBC.SY || eff[k] == LBC.IS || eff[k] == LBC.CL || eff[k] == LBC.CP)) k--; if (k >= 0 && eff[k] == LBC.NU) return 0; }
					if (a0 == LBC.JL && (b0 == LBC.JL || b0 == LBC.JV || b0 == LBC.H2 || b0 == LBC.H3)) return 0;
					if ((a0 == LBC.JV || a0 == LBC.H2) && (b0 == LBC.JV || b0 == LBC.JT)) return 0;
					if ((a0 == LBC.JT || a0 == LBC.H3) && b0 == LBC.JT) return 0;
					if ((a0 == LBC.JL || a0 == LBC.JV || a0 == LBC.JT || a0 == LBC.H2 || a0 == LBC.H3) && b0 == LBC.PO || a0 == LBC.PR && (b0 == LBC.JL || b0 == LBC.JV || b0 == LBC.JT || b0 == LBC.H2 || b0 == LBC.H3)) return 0;
					if (a0 == LBC.AL && b0 == LBC.AL) return 0;
					if (a0 == LBC.IS && b0 == LBC.AL) return 0;
					if ((a0 == LBC.AL || a0 == LBC.NU) && b0 == LBC.OP) return 0;
					if (a0 == LBC.CP && (b0 == LBC.AL || b0 == LBC.NU)) return 0;
					if (a0 == LBC.RI && b0 == LBC.RI) return (riCount++ % 2 == 0) ? (byte)0 : (byte)1;
					if (a0 == LBC.EM && b0 == LBC.EM) return 0;
					return 1;
				}
			}
			d.Brk[n - 1] = cls[n - 1] == LBC.BK || cls[n - 1] == LBC.LF || cls[n - 1] == LBC.CR ? (byte)2 : (byte)1;
			for (int i = 0; i < n - 1; i++) {
				if (d.Brk[i] == 2) continue;
				Style sa = StyleAt(d, i), sb2 = StyleAt(d, i + 1);
				if (sa != null && !Wraps(sa)) { d.Brk[i] = 0; continue; }
				if (d.Brk[i] == 0 && sa != null) {
					bool anywhere = sa.LineBreakMode == 1 || sb2 != null && sb2.LineBreakMode == 1;
					bool breakAll = sa.WordBreak == 1 && sb2 != null && sb2.WordBreak == 1;
					if (anywhere && !char.IsLowSurrogate(t[i + 1]) && !Uni.IsMark(t[i + 1]) && cls[i + 1] != LBC.ZWJ && cls[i + 1] != LBC.CM) d.Brk[i] = 1;
					else if (breakAll) {
						LBC x = eff[i], y = cls[i + 1];
						bool lx = x == LBC.AL || x == LBC.NU || x == LBC.ID || x == LBC.H2 || x == LBC.H3 || x == LBC.JL || x == LBC.JV || x == LBC.JT;
						bool ly = y == LBC.AL || y == LBC.NU || y == LBC.ID || y == LBC.H2 || y == LBC.H3 || y == LBC.JL || y == LBC.JV || y == LBC.JT;
						if (lx && ly) d.Brk[i] = 1;
					}
				}
				else if (d.Brk[i] == 1 && sa != null && sa.WordBreak == 2) {
					LBC x = eff[i], y = cls[i + 1];
					if ((x == LBC.ID || x == LBC.AL || x == LBC.NU || x == LBC.H2 || x == LBC.H3) && (y == LBC.ID || y == LBC.AL || y == LBC.NU || y == LBC.H2 || y == LBC.H3)) d.Brk[i] = 0;
				}
			}
		}

		static Style StyleAt(InlineData d, int i) {
			int ii = d.ItemAt[i];
			if (ii < 0) {
				for (int k = i; k >= 0; k--) if (d.ItemAt[k] >= 0) return d.Items[d.ItemAt[k]].S;
				return null;
			}
			var it = d.Items[ii];
			if (it.K == IK.Open || it.K == IK.Close) return it.Parent >= 0 ? d.Items[it.Parent].S : it.S;
			return it.S;
		}

		public double AtomicWidth(Box b, double cbW) {
			LayoutAtomic(b, cbW);
			return b.W + b.M[1] + b.M[3];
		}

		void LayoutAtomic(Box b, double cbW) {
			bool pg = Paginate;
			Paginate = false;
			try { LayoutShrinkToFit(b, cbW, 0, 0); }
			finally { Paginate = pg; }
		}

		public double TabWidth(Style s) {
			if (s.TabSize < 0) return -s.TabSize;
			var f = Primary(s);
			ushort g = f.File.Glyph(' ');
			double sp = f.File.AdvanceUnits(g) * f.Size / f.File.UnitsPerEm + s.LetterSpacing + s.WordSpacing;
			return Math.Max(1, sp * s.TabSize);
		}

		public void InlineIntrinsic(Box c, out double min, out double max) {
			var d = BuildInline(c);
			int n = d.Text.Length;
			min = 0; max = 0;
			double cur = 0, word = 0;
			double indent = c.S.TextIndent.IsPctDep ? 0 : c.S.TextIndent.Resolve(0);
			cur = indent; word = indent;
			double trail = 0;
			for (int i = 0; i <= n; i++) {
				if (i <= n && d.OpenAt[Math.Min(i, n)] != null) foreach (var ii in d.OpenAt[i]) { double w = OpenSpacer(d.Items[ii]); cur += w; word += w; }
				if (d.CloseAt[Math.Min(i, n)] != null) foreach (var ii in d.CloseAt[i]) { double w = CloseSpacer(d.Items[d.Items[ii].Match]); cur += w; word += w; }
				if (d.ZeroAt[Math.Min(i, n)] != null) foreach (var ii in d.ZeroAt[i]) {
					var it = d.Items[ii];
					if (it.K == IK.Float) {
						IntrinsicOuter(it.Box, out double fmn, out double fmx);
						min = Math.Max(min, fmn);
						cur += fmx;
					}
				}
				if (i == n) break;
				char ch = d.Text[i];
				int itx = d.ItemAt[i];
				var item = itx >= 0 ? d.Items[itx] : null;
				double w2;
				if (item != null && item.K == IK.Atomic) {
					IntrinsicOuter(item.Box, out double amn, out double amx);
					w2 = amx;
					word += amn;
					cur += amx;
					trail = 0;
				}
				else if (ch == '\n') {
					max = Math.Max(max, cur - trail);
					min = Math.Max(min, word - trail);
					cur = 0; word = 0; trail = 0;
					continue;
				}
				else {
					w2 = ch == '\t' && item != null ? TabWidth(item.S) : d.Adv[i];
					if (d.Coll[i] || d.Hang[i]) trail += w2; else trail = 0;
					cur += w2;
					word += w2;
				}
				if (d.Brk[i] != 0 || i == n - 1) {
					double wt = word;
					int k = i;
					while (k >= 0 && (d.Coll[k] || d.Hang[k] || d.Text[k] == ' ' && item != null && Collapses(item.S))) { wt -= d.Adv[k]; k--; if (k < 0 || d.Brk[k] != 0 && k < i) break; }
					min = Math.Max(min, wt);
					word = 0;
				}
			}
			max = Math.Max(max, cur - trail);
			min = Math.Max(min, word);
			if (min > max) max = min;
		}

		double OpenSpacer(IItem it) {
			var s = it.S;
			int side = s.Rtl ? 1 : 3;
			return s.Margin[side].IsAuto ? 0 : s.Margin[side].Resolve(0) + s.BorderWidth[side] + s.Padding[side].Resolve(0);
		}

		double CloseSpacer(IItem it) {
			var s = it.S;
			int side = s.Rtl ? 3 : 1;
			return s.Margin[side].IsAuto ? 0 : s.Margin[side].Resolve(0) + s.BorderWidth[side] + s.Padding[side].Resolve(0);
		}

		double OpenSpacerW(IItem it, double cbW) {
			var s = it.S;
			int side = s.Rtl ? 1 : 3;
			return (s.Margin[side].IsAuto ? 0 : s.Margin[side].Resolve(cbW)) + s.BorderWidth[side] + s.Padding[side].Resolve(cbW);
		}

		double CloseSpacerW(IItem it, double cbW) {
			var s = it.S;
			int side = s.Rtl ? 3 : 1;
			return (s.Margin[side].IsAuto ? 0 : s.Margin[side].Resolve(cbW)) + s.BorderWidth[side] + s.Padding[side].Resolve(cbW);
		}

		(double a, double d, double ta, double td) Strut(Style s, ResolvedFont f) {
			double lh = s.LineHeightFor(f.NormalLineHeight);
			bool normal = double.IsNaN(s.LineHeightNum) && double.IsNaN(s.LineHeightPx);
			double lhU = normal ? f.NormalLineHeight : LU(lh);
			double ta = f.Asc, td = f.Desc;
			double avail = lhU - (ta + td);
			double half = Math.Floor(Math.Truncate(avail * 64) / 2 / 64);
			double a = ta + half;
			return (a, lhU - a, ta, td);
		}

		public double LayoutInlineContent(Box c, double cx, double cw, double y0, FloatMgr fm) {
			var d = BuildInline(c);
			string t = d.Text;
			int n = t.Length;
			var cs = c.S;
			var rootFont = Primary(cs);
			var rootStrut = Strut(cs, rootFont);
			double strutH = rootStrut.a + rootStrut.d;
			foreach (var it in d.Items) {
				if (it.K == IK.Atomic) it.AtomicW = AtomicWidth(it.Box, cw);
			}
			for (int i = 0; i <= n; i++) {
				d.OpenSp[i] = 0; d.CloseSp[i] = 0;
				if (d.OpenAt[i] != null) foreach (var ii in d.OpenAt[i]) d.OpenSp[i] += OpenSpacerW(d.Items[ii], cw);
				if (d.CloseAt[i] != null) foreach (var ii in d.CloseAt[i]) d.CloseSp[i] += CloseSpacerW(d.Items[d.Items[ii].Match], cw);
			}
			double W(int i) {
				int ii = d.ItemAt[i];
				if (ii >= 0 && d.Items[ii].K == IK.Atomic) return d.Items[ii].AtomicW;
				if (t[i] == '\t' && ii >= 0 && !Collapses(d.Items[ii].S)) return TabWidth(d.Items[ii].S);
				return d.Adv[i];
			}
			double indentPx = cs.TextIndent.Resolve(cw);
			var lines = new List<LineInfo>();
			int pos = 0;
			double y = y0;
			bool firstLine = true;
			var placedFloats = new HashSet<int>();
			var pendingFloats = new List<int>();

			void PlaceFloat(int ii, double atY, ref double left, ref double right, double lineW, bool lineEmpty) {
				var fb = d.Items[ii].Box;
				placedFloats.Add(ii);
				LayoutFloat(fb, c, cx, cw, atY, fm);
				var band = fm.Band(atY, Math.Max(1, strutH), cx, cx + cw);
				left = band.left; right = band.right;
			}

			while (true) {
				int ls = pos;
				while (ls < n && d.Coll[ls]) ls++;
				for (int p = pos; p <= Math.Min(ls, n); p++) {
					if (d.ZeroAt[p] == null) continue;
					foreach (var ii in d.ZeroAt[p]) if (d.Items[ii].K == IK.Float && !placedFloats.Contains(ii)) { double l0 = 0, r0 = 0; PlaceFloat(ii, y, ref l0, ref r0, 0, true); }
				}
				if (ls >= n) break;
				var bandNow = fm.Band(y, strutH, cx, cx + cw);
				double left = bandNow.left, right = bandNow.right;
				double indent = firstLine ? indentPx : 0;
				double avail = right - left - indent;
				double lineW = 0;
				int lineEnd = ls;
				bool forced = false;
				int cur = ls;
				while (cur < n) {
					int j = cur;
					while (j < n - 1 && d.Brk[j] == 0) j++;
					double segW = d.OpenSp[cur];
					for (int k = cur; k <= j; k++) {
						if (k > cur) segW += d.OpenSp[k] + d.CloseSp[k];
						if (t[k] == '\n') continue;
						segW += W(k);
					}
					segW += d.CloseSp[j + 1];
					double trailW = 0;
					for (int k = j; k >= cur; k--) {
						if (d.Coll[k] || d.Hang[k]) trailW += W(k);
						else if (t[k] == '\n') continue;
						else break;
					}
					if (t[j] == '­') trailW -= HyphenWidth(d, j);
					double fit = lineW + segW - trailW;
					bool empty = lineEnd == ls;
					if (fit <= avail + 0.001 || empty) {
						if (empty && fit > avail + 0.001) {
							var st0 = StyleAt(d, cur);
							if (st0 != null && (st0.OverflowWrap != 0 || st0.WordBreak == 3) && Wraps(st0)) {
								double acc = d.OpenSp[cur];
								int k = cur;
								int lastOk = -1;
								while (k <= j) {
									int kn = k + 1;
									while (kn <= j && (char.IsLowSurrogate(t[kn]) || Uni.IsMark(t[kn]) || t[kn] == '‍' || t[kn] == '‌')) kn++;
									double gw = 0;
									for (int q = k; q < kn; q++) gw += W(q) + (q > cur ? d.OpenSp[q] + d.CloseSp[q] : 0);
									if (acc + gw > avail + 0.001 && lastOk >= 0) break;
									acc += gw;
									lastOk = kn - 1;
									k = kn;
								}
								if (lastOk >= 0 && lastOk < j) {
									lineW = acc;
									lineEnd = lastOk + 1;
									cur = lineEnd;
									break;
								}
							}
						}
						for (int p = cur; p <= j; p++) {
							if (d.ZeroAt[p] == null || p == ls) continue;
							foreach (var ii in d.ZeroAt[p]) {
								if (d.Items[ii].K != IK.Float || placedFloats.Contains(ii)) continue;
								IntrinsicOuter(d.Items[ii].Box, out _, out double fw);
								if (lineW + fw <= avail) {
									double l1 = left, r1 = right;
									PlaceFloat(ii, y, ref l1, ref r1, lineW, false);
									left = l1; right = r1;
									avail = right - left - indent;
								}
								else pendingFloats.Add(ii);
							}
						}
						lineW += segW;
						lineEnd = j + 1;
						cur = j + 1;
						if (d.Brk[j] == 2) { forced = true; break; }
					}
					else break;
				}
				var li = BuildLine(c, d, ls, lineEnd, left, avail, indent, firstLine, forced, lineEnd >= n, cw, rootStrut, rootFont);
				li.Y = y;
				lines.Add(li);
				if (!li.Empty) y += li.H;
				foreach (var ii in pendingFloats) if (!placedFloats.Contains(ii)) { double l2 = 0, r2 = 0; PlaceFloat(ii, y, ref l2, ref r2, 0, true); }
				pendingFloats.Clear();
				pos = lineEnd;
				if (!li.Empty) firstLine = false;
				if (pos >= n) break;
			}
			for (int p = 0; p <= n; p++) {
				if (d.ZeroAt[p] == null) continue;
				foreach (var ii in d.ZeroAt[p]) if (d.Items[ii].K == IK.Float && !placedFloats.Contains(ii)) { double l3 = 0, r3 = 0; PlaceFloat(ii, y, ref l3, ref r3, 0, true); }
			}

			PlaceLinesPaginated(c, lines, y0);
			c.Lines = new List<LineBox>();
			double bottom = y0;
			foreach (var li in lines) {
				if (li.Empty) {
					PlaceAbsInEmptyLine(c, d, li, cx, cw);
					continue;
				}
				var lb = EmitLine(c, d, li, cx, cw);
				c.Lines.Add(lb);
				bottom = li.Y + li.H;
			}
			if (c.Lines.Count > 0) {
				c.Baseline = c.Lines[0].Baseline;
				c.LastBaseline = c.Lines[c.Lines.Count - 1].Baseline;
			}
			return bottom - y0;
		}

		void PlaceAbsInEmptyLine(Box c, InlineData d, LineInfo li, double cx, double cw) {
			for (int p = li.Start; p <= Math.Min(li.End, d.Text.Length); p++) {
				if (d.ZeroAt[p] == null) continue;
				foreach (var ii in d.ZeroAt[p]) {
					var it = d.Items[ii];
					if (it.K != IK.Abs) continue;
					it.Box.StaticX = c.S.Rtl ? cx + cw : cx;
					it.Box.StaticY = li.Y;
					it.Box.StaticRtl = c.S.Rtl;
					RegisterAbs(it.Box);
				}
			}
		}

		double HyphenWidth(InlineData d, int i) {
			var s = StyleAt(d, i);
			if (s == null) return 0;
			var f = PickFont(s, '-');
			ushort g = f.File.Glyph('‐');
			if (g == 0) g = f.File.Glyph('-');
			return f.File.AdvanceUnits(g) * f.Size / f.File.UnitsPerEm;
		}

		void PlaceLinesPaginated(Box c, List<LineInfo> lines, double y0) {
			if (!Paginate) {
				double y = y0;
				foreach (var li in lines) { li.Y = y; if (!li.Empty) y += li.H; }
				return;
			}
			var forcedBefore = new HashSet<int>();
			int orphans = c.S.Orphans, widows = c.S.Widows;
			for (int attempt = 0; attempt < 6; attempt++) {
				double y = y0;
				var breaks = new List<int>();
				for (int i = 0; i < lines.Count; i++) {
					var li = lines[i];
					if (li.Empty) { li.Y = y; continue; }
					if (forcedBefore.Contains(i) && !AtPageTop(y)) { y = NextPageTop(y); breaks.Add(i); }
					else if (Crosses(y, li.H) && !AtPageTop(y) && li.H <= PageH) { y = NextPageTop(y); breaks.Add(i); }
					li.Y = y;
					y += li.H;
				}
				bool changed = false;
				var content = lines.Select((l, i) => (l, i)).Where(x => !x.l.Empty).Select(x => x.i).ToList();
				foreach (int b in breaks) {
					if (forcedBefore.Contains(b) && b == content.FirstOrDefault()) continue;
					int idx = content.IndexOf(b);
					if (idx < 0) continue;
					int before = idx, after = content.Count - idx;
					int prevBreak = breaks.Where(x => x < b).DefaultIfEmpty(-1).Max();
					if (prevBreak >= 0) before = idx - content.IndexOf(prevBreak);
					int nextBreak = breaks.Where(x => x > b).DefaultIfEmpty(-1).Min();
					if (nextBreak > 0) after = content.IndexOf(nextBreak) - idx;
					if (before < orphans && prevBreak < 0 && before > 0) {
						int first = content[0];
						if (!forcedBefore.Contains(first) && !AtPageTop(y0)) { forcedBefore.Add(first); changed = true; break; }
					}
					else if (after < widows && nextBreak < 0) {
						int need = widows - after;
						int newIdx = idx - need;
						int minIdx = (prevBreak >= 0 ? content.IndexOf(prevBreak) : 0) + orphans;
						if (newIdx >= minIdx && newIdx > 0) { forcedBefore.Add(content[newIdx]); changed = true; break; }
					}
				}
				if (!changed) break;
			}
		}

		LineInfo BuildLine(Box c, InlineData d, int ls, int le, double left, double avail, double indent, bool first, bool forced, bool lastOfPara, double cw, (double a, double d, double ta, double td) rootStrut, ResolvedFont rootFont) {
			string t = d.Text;
			var li = new LineInfo { Start = ls, End = le, Left = left, Avail = avail, Indent = indent, EndsForced = forced, LastInPara = lastOfPara || forced };
			int te = le;
			while (te > ls && (t[te - 1] == '\n')) te--;
			int ce = te;
			while (ce > ls && d.Coll[ce - 1]) ce--;
			li.ContentEnd = ce;
			bool softHyphen = !forced && ce > ls && t[ce - 1] == '­' && le < t.Length;
			var pieces = new List<Piece>();
			byte para = d.ParaLevel;
			var lvl = new byte[Math.Max(0, te - ls)];
			for (int i = ls; i < te; i++) lvl[i - ls] = d.Levels[i];
			for (int i = te - 1; i >= ls; i--) {
				char ch = t[i];
				if (ch == ' ' || ch == '\t' || d.Hang[i] || d.Coll[i] || char.IsWhiteSpace(ch) || d.Bidi[i] == BC.BN || d.Bidi[i] >= BC.LRE) lvl[i - ls] = para;
				else break;
			}
			for (int i = ls; i < te; i++) if (t[i] == '\t' || d.Bidi[i] == BC.S) { lvl[i - ls] = para; for (int k = i - 1; k >= ls && (t[k] == ' ' || d.Bidi[k] == BC.WS); k--) lvl[k - ls] = para; }
			byte LevelAt(int i) => i >= ls && i < te ? lvl[i - ls] : para;
			bool Control(int i) => d.Bidi[i] == BC.BN || d.Bidi[i] >= BC.LRE;
			byte InnerLevelFwd(int i) {
				for (int k = i; k < te; k++) if (!Control(k)) return LevelAt(k);
				return LevelAt(i);
			}
			byte InnerLevelBack(int i) {
				for (int k = i; k >= ls; k--) if (!Control(k)) return LevelAt(k);
				return LevelAt(i);
			}
			bool hasContent = false;
			for (int p = ls; p <= te; p++) {
				if (p <= te && d.CloseAt[p] != null && p > ls) foreach (var ii in d.CloseAt[p]) {
					var open = d.Items[d.Items[ii].Match];
					double w = CloseSpacerW(open, cw);
					pieces.Add(new Piece { K = 3, S = p, E = p, Item = d.Items[ii].Match, W = w, Level = InnerLevelBack(Math.Max(ls, p - 1)) });
					if (w > 0) hasContent = true;
				}
				if (p < te && d.OpenAt[p] != null) foreach (var ii in d.OpenAt[p]) {
					double w = OpenSpacerW(d.Items[ii], cw);
					pieces.Add(new Piece { K = 2, S = p, E = p, Item = ii, W = w, Level = InnerLevelFwd(p) });
					if (w > 0) hasContent = true;
				}
				if (p >= te) break;
				if (p >= ce && d.Coll[p]) continue;
				int itx = d.ItemAt[p];
				var it = itx >= 0 ? d.Items[itx] : null;
				if (it != null && it.K == IK.Atomic) {
					pieces.Add(new Piece { K = 1, S = p, E = p + 1, Item = itx, W = it.AtomicW, Level = LevelAt(p) });
					hasContent = true;
					continue;
				}
				if (it != null && it.K == IK.Br) { hasContent = true; continue; }
				if (it == null || it.K != IK.Text) continue;
				int segI = d.SegAt[p];
				int e = p + 1;
				int lim = p < ce ? ce : te;
				while (e < lim && d.SegAt[e] == segI && d.ItemAt[e] == itx && LevelAt(e) == LevelAt(p) && d.OpenAt[e] == null && d.CloseAt[e] == null) e++;
				double w2 = 0;
				int spaces = 0;
				for (int k = p; k < e; k++) {
					if (t[k] == '\n') continue;
					w2 += t[k] == '\t' && !Collapses(it.S) ? TabWidth(it.S) : d.Adv[k];
					if (k < ce && (t[k] == ' ' || t[k] == ' ' || t[k] == '　')) spaces++;
					if (!d.Coll[k] && !Uni.IsDefaultIgnorable(t[k])) hasContent = true;
					if (d.Hang[k]) hasContent = true;
				}
				if (segI >= 0) pieces.Add(new Piece { K = 0, S = p, E = e, Item = itx, Seg = d.Segs[segI], W = w2, Level = LevelAt(p), Spaces = spaces });
				p = e - 1;
			}
			if (le > ls && t[le - 1] == '\n' && d.ItemAt[le - 1] >= 0 && d.Items[d.ItemAt[le - 1]].K == IK.Br) hasContent = true;
			if (le > ls && t[le - 1] == '\n' && d.ItemAt[le - 1] >= 0 && d.Items[d.ItemAt[le - 1]].K == IK.Text) hasContent = true;
			if (softHyphen) {
				pieces.Add(new Piece { K = 5, S = ce - 1, E = ce, Item = d.ItemAt[ce - 1], W = HyphenWidth(d, ce - 1), Level = LevelAt(ce - 1) });
			}
			li.Pieces = pieces;
			li.Empty = !hasContent;
			double width = 0;
			double hangW = 0;
			foreach (var p in pieces) width += p.W;
			for (int k = ce; k < te; k++) if (d.Hang[k]) hangW += d.Adv[k];
			li.Width = width - hangW;
			ComputeLineMetrics(c, d, li, rootStrut, rootFont, cw);
			return li;
		}

		void ComputeLineMetrics(Box c, InlineData d, LineInfo li, (double a, double d, double ta, double td) rootStrut, ResolvedFont rootFont, double cw) {
			var boxes = new Dictionary<int, (double shift, double a, double d, double ta, double td, double fs, double xh, bool top, bool bottom)>();
			var rootXh = rootFont.XH;
			double lineTop = -rootStrut.a, lineBot = rootStrut.d;
			var present = new HashSet<int>();
			foreach (var p in li.Pieces) {
				int ii = p.K == 2 || p.K == 3 ? p.Item : (d.Items[p.Item].K == IK.Text || d.Items[p.Item].K == IK.Atomic ? d.Items[p.Item].Parent : -1);
				for (int k = ii; k >= 0; k = d.Items[k].Parent) present.Add(k);
			}
			for (int i = li.Start; i < li.End && i < d.Text.Length; i++) {
				int ii = d.ItemAt[i];
				if (ii >= 0 && d.Items[ii].K == IK.Br) for (int k = d.Items[ii].Parent; k >= 0; k = d.Items[k].Parent) present.Add(k);
			}
			(double shift, double a, double d, double ta, double td, double fs, double xh, bool top, bool bottom) Get(int ii) {
				if (ii < 0) return (0, rootStrut.a, rootStrut.d, rootStrut.ta, rootStrut.td, c.S.FontSize, rootXh, false, false);
				if (boxes.TryGetValue(ii, out var v)) return v;
				var it = d.Items[ii];
				var par = Get(it.Parent);
				var f = Primary(it.S);
				var st = Strut(it.S, f);
				double shift = VAlignShift(it.S, par.shift, par.fs, par.ta, par.td, par.xh, st.a, st.d, st.a + st.d);
				bool top = it.S.VAlign == Style.VaTop, bottom = it.S.VAlign == Style.VaBottom;
				v = (shift, st.a, st.d, st.ta, st.td, it.S.FontSize, f.XH, top || par.top, bottom || par.bottom);
				boxes[ii] = v;
				return v;
			}
			var tops = new List<(double a, double d, int ii, Box atom)>();
			var bots = new List<(double a, double d, int ii, Box atom)>();
			foreach (int ii in present) {
				var v = Get(ii);
				var it = d.Items[ii];
				if (it.S.VAlign == Style.VaTop) { tops.Add((v.a, v.d, ii, null)); continue; }
				if (it.S.VAlign == Style.VaBottom) { bots.Add((v.a, v.d, ii, null)); continue; }
				if (v.top || v.bottom) continue;
				lineTop = Math.Min(lineTop, v.shift - v.a);
				lineBot = Math.Max(lineBot, v.shift + v.d);
			}
			var atomShift = new Dictionary<int, double>();
			foreach (var p in li.Pieces) {
				if (p.K == 0) {
					var it = d.Items[p.Item];
					bool normalLh = double.IsNaN(it.S.LineHeightNum) && double.IsNaN(it.S.LineHeightPx);
					if (normalLh && p.Seg != null && p.Seg.F != Primary(it.S)) {
						var par = Get(it.Parent >= 0 ? it.Parent : -1);
						var ownerStyle = it.S;
						var st = Strut(ownerStyle, p.Seg.F);
						double shift = it.Parent >= 0 ? par.shift : 0;
						if (!par.top && !par.bottom) {
							lineTop = Math.Min(lineTop, shift - st.a);
							lineBot = Math.Max(lineBot, shift + st.d);
						}
					}
					continue;
				}
				if (p.K != 1) continue;
				var ai = d.Items[p.Item];
				var ab = ai.Box;
				double mh = ab.H + ab.M[0] + ab.M[2];
				double bl = AtomicBaseline(ab);
				var par2 = Get(ai.Parent);
				double sh = VAlignShift(ab.S, par2.shift, par2.fs, par2.ta, par2.td, par2.xh, bl, mh - bl, mh);
				if (ab.S.VAlign == Style.VaTop) { tops.Add((bl, mh - bl, -1, ab)); continue; }
				if (ab.S.VAlign == Style.VaBottom) { bots.Add((bl, mh - bl, -1, ab)); continue; }
				if (par2.top || par2.bottom) { atomShift[p.Item] = sh; continue; }
				atomShift[p.Item] = sh;
				lineTop = Math.Min(lineTop, sh - bl);
				lineBot = Math.Max(lineBot, sh + mh - bl);
			}
			foreach (var tp in tops) { double h = tp.a + tp.d; if (lineTop + h > lineBot) lineBot = lineTop + h; }
			foreach (var bt in bots) { double h = bt.a + bt.d; if (lineBot - h < lineTop) lineTop = lineBot - h; }
			foreach (var tp in tops) {
				double sh = lineTop + tp.a;
				if (tp.ii >= 0) { var v = boxes[tp.ii]; boxes[tp.ii] = (sh, v.a, v.d, v.ta, v.td, v.fs, v.xh, false, false); }
				else foreach (var p in li.Pieces) if (p.K == 1 && d.Items[p.Item].Box == tp.atom) atomShift[p.Item] = sh;
			}
			foreach (var bt in bots) {
				double sh = lineBot - bt.d;
				if (bt.ii >= 0) { var v = boxes[bt.ii]; boxes[bt.ii] = (sh, v.a, v.d, v.ta, v.td, v.fs, v.xh, false, false); }
				else foreach (var p in li.Pieces) if (p.K == 1 && d.Items[p.Item].Box == bt.atom) atomShift[p.Item] = sh;
			}
			li.H = lineBot - lineTop;
			li.BaseOff = -lineTop;
			li.BoxShift = new Dictionary<int, (double, double, double)>();
			foreach (var kv in boxes) li.BoxShift[kv.Key] = (kv.Value.shift, kv.Value.ta, kv.Value.td);
			foreach (var kv in atomShift) li.BoxShift[kv.Key] = (kv.Value, 0, 0);
		}

		static double VAlignShift(Style s, double pShift, double pFs, double pTa, double pTd, double pXh, double a, double d, double lh) {
			switch (s.VAlign) {
				case Style.VaBaseline: return pShift;
				case Style.VaSub: return pShift + LU(pFs / 5 + 1);
				case Style.VaSuper: return pShift - LU(pFs / 3 + 1);
				case Style.VaTextTop: return pShift - pTa + a;
				case Style.VaTextBottom: return pShift + pTd - d;
				case Style.VaMiddle: return pShift - pXh / 2 + (a - d) / 2;
				case Style.VaLength: return pShift - s.VAlignLen.Resolve(lh);
				default: return pShift;
			}
		}

		double AtomicBaseline(Box b) {
			double mt = b.M[0];
			if (b.Kind == BK.Replaced) return b.H + b.M[0] + b.M[2];
			if (b.S.OverflowX != 0 && b.Kind != BK.Table && b.Kind != BK.Flex) return b.H + b.M[0] + b.M[2];
			double bl = b.Kind == BK.Table || b.Kind == BK.Flex || b.Kind == BK.Grid ? b.Baseline : LastBaselineOf(b);
			if (double.IsNaN(bl)) return b.H + b.M[0] + b.M[2];
			return bl - b.Y + mt;
		}

		static double LastBaselineOf(Box b) {
			if (b.Lines != null && b.Lines.Count > 0) return b.Lines[b.Lines.Count - 1].Baseline;
			for (int i = b.Kids.Count - 1; i >= 0; i--) {
				var k = b.Kids[i];
				if (k.IsOutOfFlow || k.IsFloat || k.Kind == BK.Text) continue;
				if (k.S.OverflowX != 0 && k.Kind == BK.Block) continue;
				double v = k.Kind == BK.Table ? k.LastBaseline : LastBaselineOf(k);
				if (!double.IsNaN(v)) return v;
			}
			return b.LastBaseline;
		}

		public static double FirstBaselineOf(Box b) {
			if (b.Lines != null && b.Lines.Count > 0) return b.Lines[0].Baseline;
			if (b.Kind == BK.Table || b.Kind == BK.Flex || b.Kind == BK.Grid) return b.Baseline;
			foreach (var k in b.Kids) {
				if (k.IsOutOfFlow || k.IsFloat || k.Kind == BK.Text) continue;
				double v = FirstBaselineOf(k);
				if (!double.IsNaN(v)) return v;
			}
			return double.NaN;
		}

		LineBox EmitLine(Box c, InlineData d, LineInfo li, double cx, double cw) {
			string t = d.Text;
			var lb = new LineBox { Y = li.Y, H = li.H, Baseline = li.Y + li.BaseOff, X = cx, W = cw };
			var cs = c.S;
			bool rtl = cs.Rtl;
			var order = VisualOrder(li.Pieces);
			TA align = cs.TextAlign;
			if (li.LastInPara) {
				if (cs.TextAlignLast != TA.Auto) align = cs.TextAlignLast;
				else if (align == TA.Justify) align = TA.Start;
			}
			double free = li.Avail - li.Width;
			int spaces = 0;
			foreach (var p in li.Pieces) spaces += p.Spaces;
			double extra = 0;
			double offset;
			switch (align) {
				case TA.Left: case TA.WebkitLeft: offset = 0; break;
				case TA.Right: case TA.WebkitRight: offset = free; break;
				case TA.Center: case TA.WebkitCenter: offset = free / 2; break;
				case TA.End: offset = rtl ? 0 : free; break;
				case TA.Justify:
					if (free > 0 && spaces > 0) { extra = free / spaces; offset = 0; }
					else offset = rtl ? free : 0;
					break;
				default: offset = rtl ? free : 0; break;
			}
			if (free < 0 && align != TA.Justify) offset = rtl ? free : 0;
			double x = li.Left + (rtl ? 0 : li.Indent) + offset;
			var boxExt = new Dictionary<int, (double x0, double x1)>();
			void Ext(int ii, double a, double b) {
				for (int k = ii; k >= 0; k = d.Items[k].Parent) {
					if (d.Items[k].K != IK.Open) continue;
					if (boxExt.TryGetValue(k, out var e)) boxExt[k] = (Math.Min(e.x0, a), Math.Max(e.x1, b));
					else boxExt[k] = (a, b);
				}
			}
			double baseY = lb.Baseline;
			foreach (var p in order) {
				p.X = x;
				switch (p.K) {
					case 0: {
						var gf = MakeGlyphs(c, d, li, p, x, extra, baseY);
						double w = p.W + extra * p.Spaces;
						if (gf != null) lb.Glyphs.Add(gf);
						Ext(d.Items[p.Item].Parent, x, x + w);
						x += w;
						break;
					}
					case 1: {
						var it = d.Items[p.Item];
						var ab = it.Box;
						double sh = li.BoxShift.TryGetValue(p.Item, out var v) ? v.shift : 0;
						double bl = AtomicBaseline(ab);
						double targetX = x + ab.M[3];
						double targetY = baseY + sh - bl + ab.M[0];
						Translate(ab, targetX - ab.X, targetY - ab.Y);
						lb.Atomics.Add(ab);
						Ext(it.Parent, x, x + p.W);
						x += p.W;
						break;
					}
					case 2: case 3: {
						Ext(p.Item, x, x + p.W);
						x += p.W;
						break;
					}
					case 5: {
						var s = StyleAt(d, p.S);
						var f = PickFont(s, '-');
						ushort g = f.File.Glyph('‐');
						string u = "‐";
						if (g == 0) { g = f.File.Glyph('-'); u = "-"; }
						lb.Glyphs.Add(new GlyphFrag { RF = f, G = new[] { g }, X = new[] { x }, Y = new[] { baseY + (li.BoxShift.TryGetValue(d.Items[p.Item].Parent, out var hv) ? hv.shift : 0) }, U = new[] { u }, Color = s.Color, S = s, Baseline = baseY, X0 = x, X1 = x + p.W, Decos = d.Items[p.Item].Decos });
						x += p.W;
						break;
					}
				}
			}
			foreach (var kv in boxExt) {
				var it = d.Items[kv.Key];
				var s = it.S;
				var sh = li.BoxShift.TryGetValue(kv.Key, out var v) ? v : (0, Primary(s).Asc, Primary(s).Desc);
				bool first = it.Start >= li.Start && it.Start < li.End || it.Start == li.Start;
				bool last = it.Match >= 0 && d.Items[it.Match].Start <= li.ContentEnd && d.Items[it.Match].Start >= li.Start;
				int sSide = s.Rtl ? 1 : 3, eSide = s.Rtl ? 3 : 1;
				double x0 = kv.Value.x0, x1 = kv.Value.x1;
				double mStart = first ? (s.Margin[sSide].IsAuto ? 0 : s.Margin[sSide].Resolve(cw)) : 0;
				double mEnd = last ? (s.Margin[eSide].IsAuto ? 0 : s.Margin[eSide].Resolve(cw)) : 0;
				if (s.Rtl) { x1 -= mStart; x0 += mEnd; } else { x0 += mStart; x1 -= mEnd; }
				double top = baseY + sh.Item1 - sh.Item2 - s.Padding[0].Resolve(cw) - s.BorderWidth[0];
				double bot = baseY + sh.Item1 + sh.Item3 + s.Padding[2].Resolve(cw) + s.BorderWidth[2];
				lb.Boxes.Add(new InlineBoxFrag { Box = it.Box, X = x0, W = x1 - x0, Top = top, Bottom = bot, First = first, Last = last });
			}
			lb.Boxes.Sort((a, b) => Depth(a.Box).CompareTo(Depth(b.Box)));
			for (int p = li.Start; p <= Math.Min(li.End, d.Text.Length); p++) {
				if (d.ZeroAt[p] == null) continue;
				foreach (var ii in d.ZeroAt[p]) {
					var it = d.Items[ii];
					if (it.K != IK.Abs) continue;
					double sx = rtl ? li.Left + li.Avail : li.Left;
					foreach (var pc in order) if (pc.S >= p) { sx = rtl ? pc.X + pc.W : pc.X; break; }
					it.Box.StaticX = sx;
					it.Box.StaticY = li.Y;
					it.Box.StaticRtl = rtl;
					RegisterAbs(it.Box);
				}
			}
			ApplyInlineRelative(d, lb);
			return lb;
		}

		static bool HasBoxFragment(Style s) =>
			s.BorderWidth[0] > 0 || s.BorderWidth[1] > 0 || s.BorderWidth[2] > 0 || s.BorderWidth[3] > 0 ||
			!s.Padding[0].IsFixed || s.Padding[0].Px != 0 || !s.Padding[1].IsFixed || s.Padding[1].Px != 0 || !s.Padding[2].IsFixed || s.Padding[2].Px != 0 || !s.Padding[3].IsFixed || s.Padding[3].Px != 0 ||
			s.BackgroundColor.A > 0 || s.Backgrounds != null || s.BoxShadow != null || s.OutlineWidth > 0 && s.OutlineStyle != BS.None || s.Position == Pos.Relative ||
			!s.Margin[1].IsFixed || s.Margin[1].Px != 0 || !s.Margin[3].IsFixed || s.Margin[3].Px != 0;

		static int Depth(Box b) { int n = 0; for (var x = b; x != null; x = x.Parent) n++; return n; }

		void ApplyInlineRelative(InlineData d, LineBox lb) {
			bool any = false;
			foreach (var it in d.Items) if (it.K == IK.Open && it.S.Position == Pos.Relative) { any = true; break; }
			if (!any) return;
			(double dx, double dy) Off(Box b) {
				double dx = 0, dy = 0;
				for (var x = b; x != null && x.Kind == BK.Inline; x = x.Parent) {
					if (x.S.Position != Pos.Relative) continue;
					if (!x.S.Left.IsAuto) dx += x.S.Left.Resolve(0); else if (!x.S.Right.IsAuto) dx -= x.S.Right.Resolve(0);
					if (!x.S.Top.IsAuto) dy += x.S.Top.Resolve(0); else if (!x.S.Bottom.IsAuto) dy -= x.S.Bottom.Resolve(0);
				}
				return (dx, dy);
			}
			foreach (var bf in lb.Boxes) { var o = Off(bf.Box); bf.X += o.dx; bf.Top += o.dy; bf.Bottom += o.dy; }
			foreach (var g in lb.Glyphs) {
				if (g.Owner == null) continue;
				var o = Off(g.Owner);
				if (o.dx != 0 || o.dy != 0) g.Shift(o.dx, o.dy);
			}
			foreach (var a in lb.Atomics) {
				var o = Off(a.Parent);
				if (o.dx != 0 || o.dy != 0) Translate(a, o.dx, o.dy);
			}
		}

		static List<Piece> VisualOrder(List<Piece> pieces) {
			if (pieces.Count == 0) return pieces;
			var levels = pieces.Select(p => p.Level).ToArray();
			var idx = BidiAlgo.VisualOrder(levels);
			return idx.Select(i => pieces[i]).ToList();
		}

		GlyphFrag MakeGlyphs(Box c, InlineData d, LineInfo li, Piece p, double x, double extra, double baseY) {
			var seg = p.Seg;
			var run = seg.Run;
			var it = d.Items[p.Item];
			var s = it.S;
			var f = seg.F;
			double scale = f.Size / f.File.UnitsPerEm;
			string t = d.Text;
			var idx = new List<int>();
			for (int k = 0; k < run.Count; k++) if (run.Cl[k] >= p.S && run.Cl[k] < p.E) idx.Add(k);
			bool rtl = (seg.Level & 1) == 1;
			double shift = li.BoxShift.TryGetValue(it.Parent, out var v) ? v.shift : 0;
			double by = baseY + shift;
			var clusters = new List<(int cl, int end, List<int> glyphs)>();
			foreach (int k in idx) {
				int cl = run.Cl[k];
				if (clusters.Count > 0 && clusters[clusters.Count - 1].cl == cl) clusters[clusters.Count - 1].glyphs.Add(k);
				else clusters.Add((cl, 0, new List<int> { k }));
			}
			var sorted = clusters.Select(x2 => x2.cl).OrderBy(z => z).ToList();
			var ends = new Dictionary<int, int>();
			for (int i = 0; i < sorted.Count; i++) ends[sorted[i]] = i + 1 < sorted.Count ? sorted[i + 1] : p.E;
			var vis = rtl ? Enumerable.Reverse(clusters).ToList() : clusters;
			var gs = new List<ushort>();
			var xs = new List<double>();
			var ys = new List<double>();
			var us = new List<string>();
			var basePos = new Dictionary<int, (double x, double y)>();
			var pendingMarks = new List<int>();
			double pen = x;
			double leading = 0;
			int firstCl = sorted.Count > 0 ? sorted[0] : p.E;
			for (int k = p.S; k < firstCl; k++) leading += t[k] == '\t' && !Collapses(s) ? TabWidth(s) : d.Adv[k];
			if (!rtl) pen += leading;
			foreach (var (cl, _, glyphs) in vis) {
				int end = ends[cl];
				double cw2 = 0;
				int spc = 0;
				for (int k = cl; k < end; k++) {
					if (t[k] == '\t' && !Collapses(s)) {
						double tw = TabWidth(s);
						double rel = pen - li.Left;
						cw2 += Math.Max(1, Math.Ceiling((rel + 1e-6) / tw) * tw - rel);
					}
					else cw2 += d.Adv[k];
					if (k < li.ContentEnd && (t[k] == ' ' || t[k] == ' ' || t[k] == '　')) spc++;
				}
				var gl = rtl ? Enumerable.Reverse(glyphs).ToList() : glyphs;
				double gx = pen;
				bool firstGlyph = true;
				bool isTab = end - cl == 1 && t[cl] == '\t';
				foreach (int k in gl) {
					if (isTab) break;
					if (run.Attach[k] >= 0) { pendingMarks.Add(k); continue; }
					double px = gx + run.XOff[k] * scale;
					double py = by - run.YOff[k] * scale;
					basePos[k] = (px - run.XOff[k] * scale, py + run.YOff[k] * scale);
					gs.Add(run.G[k]); xs.Add(px); ys.Add(py);
					us.Add(firstGlyph ? ClusterText(t, cl, end) : "");
					firstGlyph = false;
					gx += run.Adv[k] * scale;
				}
				if (firstGlyph && glyphs.Count > 0) {
					foreach (int k in gl) { if (run.Attach[k] >= 0) { } }
				}
				pen += cw2 + extra * spc;
			}
			foreach (int k in pendingMarks) {
				int b = run.Attach[k];
				(double x, double y) bp;
				if (!basePos.TryGetValue(b, out bp)) {
					int bb = b;
					while (bb >= 0 && run.Attach[bb] >= 0) bb = run.Attach[bb];
					if (bb < 0 || !basePos.TryGetValue(bb, out bp)) bp = (x, by);
				}
				double mx = bp.x + run.XOff[k] * scale;
				double my = bp.y - run.YOff[k] * scale;
				if (run.Attach[b] >= 0 && basePos.TryGetValue(b, out var mb)) { mx = mb.x + run.XOff[k] * scale; my = mb.y - run.YOff[k] * scale; }
				basePos[k] = (mx, my);
				gs.Add(run.G[k]); xs.Add(mx); ys.Add(my); us.Add(us.Count == 0 ? ClusterText(t, run.Cl[k], run.Cl[k] + 1) : "");
			}
			if (gs.Count == 0) return null;
			double anchor = li.Y;
			for (int k = it.Parent; k >= 0; k = d.Items[k].Parent) {
				var ob = d.Items[k];
				if (!HasBoxFragment(ob.S)) continue;
				if (li.BoxShift.TryGetValue(k, out var bs)) anchor = baseY + bs.shift - bs.ta - ob.S.Padding[0].Resolve(0) - ob.S.BorderWidth[0];
				break;
			}
			return new GlyphFrag {
				AnchorTop = anchor, RF = f, G = gs.ToArray(), X = xs.ToArray(), Y = ys.ToArray(), U = us.ToArray(), Color = s.Color, S = s, Baseline = by, X0 = x, X1 = x + p.W + extra * p.Spaces, Decos = it.Decos, Hidden = s.Visibility != 0, Owner = it.Box.Parent != null && it.Box.Parent.Kind == BK.Inline ? it.Box.Parent : null };
		}

		public List<GlyphFrag> TextRun(string text, Style s, double x, double baseline, out double width) {
			var st = s.InheritFrom();
			st.Display = Disp.Block;
			st.Ws = WSp.Pre;
			st.TextAlign = TA.Left;
			var holder = new Box(BK.Block, null, st) { Anon = true };
			holder.Add(new Box(BK.Text, null, st) { Text = text });
			bool pg = Paginate;
			Paginate = false;
			LayoutInlineContent(holder, 0, 1e7, 0, new FloatMgr());
			Paginate = pg;
			width = 0;
			var res = new List<GlyphFrag>();
			if (holder.Lines == null || holder.Lines.Count == 0) return res;
			var line = holder.Lines[0];
			double minX = double.PositiveInfinity, maxX = 0;
			foreach (var g in line.Glyphs) { minX = Math.Min(minX, g.X0); maxX = Math.Max(maxX, g.X1); }
			if (double.IsInfinity(minX)) return res;
			width = maxX - minX;
			foreach (var g in line.Glyphs) { g.Shift(x - minX, baseline - line.Baseline); g.Decos = null; res.Add(g); }
			return res;
		}

		static string ClusterText(string t, int s, int e) {
			var sb = new StringBuilder();
			for (int i = s; i < e && i < t.Length; i++) {
				char ch = t[i];
				if (char.IsSurrogate(ch)) { sb.Append(ch); continue; }
				if (Uni.IsDefaultIgnorable(ch) && ch != '­') continue;
				if (ch == '\n' || ch == '￼') continue;
				sb.Append(ch == ' ' ? ' ' : ch);
			}
			return sb.ToString();
		}
	}

	internal struct MC {
		public double Pos, Neg;
		public static MC Of(double m) { var x = new MC(); x.Add(m); return x; }
		public void Add(double m) { if (m > 0) Pos = Math.Max(Pos, m); else Neg = Math.Min(Neg, m); }
		public void Add(MC o) { Pos = Math.Max(Pos, o.Pos); Neg = Math.Min(Neg, o.Neg); }
		public double Sum => Pos + Neg;
	}

	internal sealed class FloatMgr {
		public readonly List<(double x0, double y0, double x1, double y1, bool left)> Rects = new();
		public double LastTop = double.NegativeInfinity;

		public (double left, double right) Band(double y, double h, double l, double r) {
			if (h <= 0) h = 0.01;
			foreach (var f in Rects) {
				if (f.y1 <= y + 1e-6 || f.y0 >= y + h - 1e-6) continue;
				if (f.left) l = Math.Max(l, f.x1); else r = Math.Min(r, f.x0);
			}
			return (l, r);
		}

		public double ClearY(byte clear, double y) {
			foreach (var f in Rects) {
				if (clear == 3 || clear == 1 && f.left || clear == 2 && !f.left) y = Math.Max(y, f.y1);
			}
			return y;
		}

		public double Bottom() {
			double b = double.NegativeInfinity;
			foreach (var f in Rects) b = Math.Max(b, f.y1);
			return b;
		}

		public (double x, double y) Place(double w, double h, bool left, double y, double l, double r) {
			y = Math.Max(y, LastTop);
			for (int guard = 0; guard < 1000; guard++) {
				var (bl, br) = Band(y, Math.Max(h, 0.01), l, r);
				bool anyInBand = Rects.Any(f => !(f.y1 <= y + 1e-6 || f.y0 >= y + Math.Max(h, 0.01) - 1e-6));
				if (br - bl >= w - 1e-6 || !anyInBand) {
					double x = left ? bl : br - w;
					return (x, y);
				}
				double next = double.PositiveInfinity;
				foreach (var f in Rects) if (f.y1 > y + 1e-6 && f.y1 < next) next = f.y1;
				if (double.IsInfinity(next)) return (left ? bl : br - w, y);
				y = next;
			}
			return (left ? l : r - w, y);
		}

		public void Add(double x0, double y0, double x1, double y1, bool left) {
			Rects.Add((x0, y0, x1, y1, left));
			LastTop = Math.Max(LastTop, y0);
		}
	}

	internal sealed partial class LayoutEngine {
		public Box Root;

		public void LayoutDocument(Box root) {
			Root = root;
			root.AbsList = new List<Box>();
			var fm = new FloatMgr();
			LayoutBlockBox(root, 0, PageW, 0 + MarginVal(root, 0, PageW), fm, PageH, out _);
			LayoutAbsKids(root, true);
			foreach (var f in Fixed) LayoutAbs(f, 0, 0, PageW, PageH);
		}

		double MarginVal(Box b, int side, double cbW) {
			var m = b.S.Margin[side];
			return m.IsAuto ? 0 : m.Resolve(cbW);
		}

		public void ResolveBoxModel(Box b, double cbW) {
			var s = b.S;
			for (int i = 0; i < 4; i++) {
				b.P[i] = Math.Max(0, s.Padding[i].Resolve(cbW));
				b.Bd[i] = s.BorderWidth[i];
				b.M[i] = s.Margin[i].IsAuto ? 0 : s.Margin[i].Resolve(cbW);
			}
			if (b.Kind == BK.Table && b.S.BorderCollapse) CollapsedTableBorders(b);
		}

		double BP(Box b) => b.P[1] + b.P[3] + b.Bd[1] + b.Bd[3];
		double BPV(Box b) => b.P[0] + b.P[2] + b.Bd[0] + b.Bd[2];

		double ClampW(Box b, double w, double cbW) {
			var s = b.S;
			double adj = s.BorderBox ? BP(b) : 0;
			if (!s.MaxWidth.IsNone && !s.MaxWidth.IsAuto) { var mx = s.MaxWidth.K == Len.Value ? s.MaxWidth.TryResolve(cbW) : SizeKeyword(b, s.MaxWidth, cbW); if (mx != null) w = Math.Min(w, Math.Max(0, mx.Value - adj)); }
			if (!s.MinWidth.IsAuto) { var mn = s.MinWidth.K == Len.Value ? s.MinWidth.TryResolve(cbW) : SizeKeyword(b, s.MinWidth, cbW); if (mn != null) w = Math.Max(w, mn.Value - adj); }
			return Math.Max(0, w);
		}

		double? SizeKeyword(Box b, Len l, double cbW) {
			if (l.K == Len.KMin || l.K == Len.KMax || l.K == Len.KFit) {
				ContentIntrinsic(b, out double mn, out double mx);
				double adj = b.S.BorderBox ? BP(b) : 0;
				if (l.K == Len.KMin) return mn + adj;
				if (l.K == Len.KMax) return mx + adj;
				double avail = cbW - b.M[1] - b.M[3] - BP(b);
				return Math.Min(Math.Max(mn, avail), mx) + adj;
			}
			if (l.K == Len.KStretch) return cbW - b.M[1] - b.M[3] - (b.S.BorderBox ? 0 : BP(b));
			return null;
		}

		double ClampH(Box b, double h, double cbH) {
			var s = b.S;
			double adj = s.BorderBox ? BPV(b) : 0;
			if (!s.MaxHeight.IsNone && !s.MaxHeight.IsAuto) { var mx = s.MaxHeight.TryResolve(cbH); if (mx != null) h = Math.Min(h, Math.Max(0, mx.Value - adj)); }
			if (!s.MinHeight.IsAuto) { var mn = s.MinHeight.TryResolve(cbH); if (mn != null) h = Math.Max(h, mn.Value - adj); }
			return Math.Max(0, h);
		}

		double? SpecifiedHeight(Box b, double cbH) {
			if (!double.IsNaN(b.ForceH)) return b.ForceH;
			var s = b.S;
			if (s.Height.K != Len.Value) return null;
			var h = s.Height.TryResolve(cbH);
			if (h == null) return null;
			double v = h.Value - (s.BorderBox ? BPV(b) : 0);
			return Math.Max(0, v);
		}

		public void Translate(Box b, double dx, double dy) {
			if (dx == 0 && dy == 0) return;
			b.X += dx; b.Y += dy;
			if (!double.IsNaN(b.Baseline)) b.Baseline += dy;
			if (!double.IsNaN(b.LastBaseline)) b.LastBaseline += dy;
			b.StaticX += dx; b.StaticY += dy;
			if (b.Lines != null) foreach (var l in b.Lines) l.Shift(dx, dy);
			if (b.MarkerFrags != null) foreach (var g in b.MarkerFrags) g.Shift(dx, dy);
			b.MarkerImgX += dx; b.MarkerImgY += dy;
			b.MarkerSX += dx; b.MarkerSY += dy;
			if (b.RepeatHeaders != null) for (int i = 0; i < b.RepeatHeaders.Count; i++) b.RepeatHeaders[i] = (b.RepeatHeaders[i].y + dy, b.RepeatHeaders[i].h);
			if (b.RepeatFooters != null) for (int i = 0; i < b.RepeatFooters.Count; i++) b.RepeatFooters[i] = (b.RepeatFooters[i].y + dy, b.RepeatFooters[i].h);
			foreach (var k in b.Kids) Translate(k, dx, dy);
		}

		bool CollapsesTopWithChild(Box b) {
			if (b.IsBfcRoot() || b.Kind != BK.Block || b.IsReplaced) return false;
			if (b.S.BorderWidth[0] > 0 || b.S.Padding[0].Resolve(0) > 0 || b.S.Padding[0].IsPctDep) return false;
			return true;
		}

		static Box FirstFlowChild(Box b) {
			foreach (var k in b.Kids) {
				if (k.IsOutOfFlow || k.IsFloat) continue;
				if (!k.IsBlockLevel) return null;
				return k;
			}
			return null;
		}

		bool HasInlineContent(Box b) => b.Kids.Count > 0 && b.Kids.All(k => !k.IsBlockLevel);

		MC TopChain(Box b, double cbW) {
			var m = MC.Of(MarginVal(b, 0, cbW));
			if (CollapsesTopWithChild(b) && !HasInlineContent(b)) {
				var f = FirstFlowChild(b);
				if (f != null && f.S.Clear == 0 && f.S.BreakBefore != Style.BrPage) {
					f.TopCollapsed = true;
					m.Add(TopChain(f, cbW));
				}
				else if (f != null) f.TopCollapsed = false;
			}
			return m;
		}

		public void LayoutBlockBox(Box b, double cbX, double cbW, double top, FloatMgr fm, double cbH, out MC bottomOut) {
			ResolveBoxModel(b, cbW);
			var s = b.S;
			bool rtlCb = b.Parent?.S.Rtl ?? s.Rtl;
			double bp = BP(b);
			double w;
			bool autoW = double.IsNaN(b.ForceW) && (s.Width.IsAuto || s.Width.IsPctDep && double.IsNaN(cbW));
			double availX = cbX, availW = cbW;
			if (fm != null && b.IsBfcRoot() && b.Parent != null && !b.IsFloat && !b.IsOutOfFlow && fm.Rects.Count > 0) {
				var band = fm.Band(top, 1, cbX, cbX + cbW);
				availX = band.left; availW = band.right - band.left;
			}
			if (b.IsReplaced) {
				ReplacedSize(b, cbW, cbH, out double rw, out double rh);
				w = rw;
				b.ContentH = rh;
			}
			else if (!double.IsNaN(b.ForceW)) w = b.ForceW;
			else if (autoW) {
				if (b.Kind == BK.Table) w = TableAutoWidth(b, availW - b.M[1] - b.M[3]);
				else w = availW - b.M[1] - b.M[3] - bp;
				if (s.Width.K == Len.KMin || s.Width.K == Len.KMax || s.Width.K == Len.KFit) { w = (SizeKeyword(b, s.Width, availW) ?? w) - (s.BorderBox ? bp : 0); }
			}
			else {
				var sw = s.Width.K == Len.Value ? s.Width.Resolve(cbW) : SizeKeyword(b, s.Width, availW) ?? 0;
				w = s.BorderBox ? sw - bp : sw;
				if (b.Kind == BK.Table) w = Math.Max(w, TableMinWidth(b) - bp);
			}
			if (!b.IsReplaced) w = ClampW(b, w, cbW);
			w = Math.Max(0, w);
			double used = w + bp;
			double ml = b.M[3], mr = b.M[1];
			bool mlAuto = s.Margin[3].IsAuto, mrAuto = s.Margin[1].IsAuto;
			if (!b.IsFloat && !b.IsOutOfFlow && !b.IsAtomicInline) {
				double rest = availW - used;
				var pa = b.Parent?.S.TextAlign ?? TA.Start;
				if (mlAuto && mrAuto) { ml = mr = Math.Max(0, rest) / 2; if (rest < 0) { if (rtlCb) { mr = 0; ml = rest; } else { ml = 0; mr = rest; } } }
				else if (mlAuto) { ml = rest - mr; }
				else if (mrAuto) { mr = rest - ml; }
				else if ((pa == TA.WebkitCenter) && b.Parent != null && !autoW) { double r2 = rest - ml - mr; ml += r2 / 2; mr += r2 / 2; }
				else if (pa == TA.WebkitRight && !autoW) ml = rest - mr;
				else if (pa == TA.WebkitLeft && !autoW) mr = rest - ml;
				else if (rtlCb) ml = rest - mr;
				else mr = rest - ml;
				b.M[3] = ml; b.M[1] = mr;
			}
			b.X = availX + ml;
			b.Y = top;
			b.W = used;
			double cx = b.X + b.Bd[3] + b.P[3];
			double cy = top + b.Bd[0] + b.P[0];
			b.AbsList ??= b.S.IsPositioned || b.Parent == null ? new List<Box>() : null;
			if (b.AbsList != null) b.AbsList.Clear();
			b.Lines = null;
			b.Baseline = double.NaN; b.LastBaseline = double.NaN;
			double? specH = b.IsReplaced ? b.ContentH : SpecifiedHeight(b, cbH);
			double contentCbH = specH ?? double.NaN;
			double contentH;
			MC lastOut = default;
			bool collapseBottom = false;
			switch (b.Kind) {
				case BK.Replaced:
					contentH = b.ContentH;
					break;
				case BK.Table:
					contentH = LayoutTable(b, cx, w, cy, contentCbH);
					break;
				case BK.Flex:
					contentH = LayoutFlex(b, cx, w, cy, specH, cbH);
					break;
				case BK.Grid:
					contentH = LayoutGrid(b, cx, w, cy, specH, cbH);
					break;
				default: {
					var cfm = b.IsBfcRoot() ? new FloatMgr() : fm;
					if (b.Kids.Count == 0) contentH = 0;
					else if (HasInlineContent(b)) contentH = LayoutInlineContent(b, cx, w, cy, cfm);
					else {
						collapseBottom = !b.IsBfcRoot() && specH == null && b.Bd[2] == 0 && b.P[2] == 0 && b.Kind == BK.Block && s.MinHeight.IsAuto;
						contentH = LayoutBlockChildren(b, cx, w, cy, cfm, contentCbH, collapseBottom, out lastOut);
					}
					if (b.IsBfcRoot()) {
						double fb = cfm.Bottom();
						if (!double.IsInfinity(fb) && fb - cy > contentH) contentH = fb - cy;
					}
					break;
				}
			}
			double h = specH ?? contentH;
			if (!b.IsReplaced) h = ClampH(b, h, cbH);
			if (h != contentH && collapseBottom) { collapseBottom = false; }
			b.H = h + BPV(b);
			b.ContentH = contentH;
			if (collapseBottom && b.H == 0 && !(b.Lines?.Count > 0)) {
				var m = lastOut;
				m.Add(MarginVal(b, 2, cbW));
				bottomOut = m;
			}
			else if (collapseBottom) {
				var m = lastOut;
				m.Add(MarginVal(b, 2, cbW));
				bottomOut = m;
			}
			else bottomOut = MC.Of(b.M[2]);
			if (double.IsNaN(b.Baseline)) b.Baseline = FirstBaselineOf(b);
			if (b.MarkerText != null || b.MarkerImg != null) PlaceMarker(b);
			if (s.IsPositioned && b.AbsList != null) LayoutAbsKids(b, false);
			if (s.Position == Pos.Relative || s.Position == Pos.Sticky) ApplyRelative(b, cbW, cbH);
		}

		public void ApplyRelative(Box b, double cbW, double cbH) {
			var s = b.S;
			double dx = 0, dy = 0;
			bool rtl = b.Parent?.S.Rtl ?? false;
			if (!s.Left.IsAuto && (!rtl || s.Right.IsAuto)) dx = s.Left.Resolve(cbW);
			else if (!s.Right.IsAuto) dx = -s.Right.Resolve(cbW);
			if (!s.Top.IsAuto) dy = s.Top.TryResolve(cbH) ?? 0;
			else if (!s.Bottom.IsAuto) dy = -(s.Bottom.TryResolve(cbH) ?? 0);
			if (s.Position == Pos.Sticky) { dx = 0; dy = 0; }
			Translate(b, dx, dy);
		}

		double LayoutBlockChildren(Box b, double cx, double cw, double top, FloatMgr fm, double cbH, bool collapseBottom, out MC endMargin) {
			double y = top;
			MC pending = default;
			Box prev = null;
			foreach (var k in b.Kids) {
				if (k.IsOutOfFlow) {
					double sy = y + pending.Sum;
					k.StaticX = b.S.Rtl ? cx + cw : cx;
					k.StaticY = sy;
					k.StaticRtl = b.S.Rtl;
					RegisterAbs(k);
					continue;
				}
				if (k.IsFloat) {
					LayoutFloat(k, b, cx, cw, y + pending.Sum, fm);
					continue;
				}
				MC topM = k.TopCollapsed && prev == null && CollapsesTopWithChild(b) ? default : TopChain(k, cw);
				if (k.TopCollapsed && prev == null && CollapsesTopWithChild(b)) TopChain(k, cw);
				MC comb = pending;
				comb.Add(topM);
				double childY = y + comb.Sum;
				bool forced = (k.S.BreakBefore == Style.BrPage || prev != null && prev.S.BreakAfter == Style.BrPage) && Paginate;
				if (forced && !AtPageTop(y)) {
					y = NextPageTop(y);
					childY = y + topM.Sum;
					pending = default;
				}
				else if (Paginate && y > 0.01 && comb.Sum > 0) {
					double boundary = Math.Floor((y - 1e-6) / PageH + 1) * PageH;
					if (childY >= boundary - 1e-6) childY = boundary + Reserve;
				}
				bool cleared = false;
				if (k.S.Clear != 0) {
					double clearY = fm.ClearY(k.S.Clear, double.NegativeInfinity);
					if (clearY > childY) { childY = clearY; cleared = true; }
				}
				int floatMark = fm.Rects.Count;
				LayoutBlockBox(k, cx, cw, childY, fm, cbH, out MC bo);
				if (Paginate && !AtPageTop(y) && !AtPageTop(childY) && Crosses(childY, k.H)) {
					bool avoid = k.S.BreakInside == Style.BrAvoid || k.IsReplaced;
					double fct = avoid ? 0 : FirstContentTop(k);
					if (avoid ? k.H <= PageH - Reserve - ReserveBottom : !double.IsInfinity(fct) && fct >= PageLimit(childY) - 1e-6) {
						fm.Rects.RemoveRange(floatMark, fm.Rects.Count - floatMark);
						double ny = NextPageTop(childY);
						LayoutBlockBox(k, cx, cw, ny, fm, cbH, out bo);
						childY = ny;
					}
				}
				bool selfCollapse = !cleared && k.H == 0 && k.Kind == BK.Block && !k.IsBfcRoot() && (k.Lines == null || k.Lines.Count == 0) && k.Kids.All(c => c.IsOutOfFlow || c.IsFloat || c.H == 0 && c.Kind == BK.Block);
				if (selfCollapse) {
					comb.Add(bo);
					pending = comb;
					k.Y = y + comb.Sum - k.M[2];
				}
				else {
					y = childY + k.H;
					pending = bo;
				}
				prev = k;
			}
			if (collapseBottom) {
				endMargin = pending;
				return y - top;
			}
			endMargin = default;
			return y + pending.Sum - top;
		}

		public void RegisterAbs(Box k) {
			Box cb = null;
			if (k.S.Position == Pos.Fixed) { if (!Fixed.Contains(k)) Fixed.Add(k); return; }
			for (var p = k.Parent; p != null; p = p.Parent) {
				if (p.S.IsPositioned || p.S.Transform != null || p.Parent == null) { cb = p; break; }
			}
			cb ??= Root;
			cb.AbsList ??= new List<Box>();
			if (!cb.AbsList.Contains(k)) cb.AbsList.Add(k);
		}

		void LayoutAbsKids(Box cb, bool isRoot) {
			if (cb.AbsList == null) return;
			double x, y, w, h;
			if (isRoot) { x = 0; y = 0; w = PageW; h = Paginate ? PageH : Math.Max(cb.H, PageH); }
			else if (cb.Kind == BK.Inline) {
				var blk = cb.Parent;
				while (blk != null && blk.Kind == BK.Inline) blk = blk.Parent;
				x = blk.X + blk.Bd[3]; y = blk.Y + blk.Bd[0]; w = blk.W - blk.Bd[1] - blk.Bd[3]; h = blk.H - blk.Bd[0] - blk.Bd[2];
			}
			else { x = cb.X + cb.Bd[3]; y = cb.Y + cb.Bd[0]; w = cb.W - cb.Bd[1] - cb.Bd[3]; h = cb.H - cb.Bd[0] - cb.Bd[2]; }
			foreach (var k in cb.AbsList.ToList()) LayoutAbs(k, x, y, w, h);
		}

		void LayoutAbs(Box k, double cbx, double cby, double cbw, double cbh) {
			var s = k.S;
			ResolveBoxModel(k, cbw);
			double bp = BP(k), bpv = BPV(k);
			bool lA = s.Left.IsAuto, rA = s.Right.IsAuto, wA = s.Width.IsAuto && !k.IsReplaced;
			double left = lA ? 0 : s.Left.Resolve(cbw), right = rA ? 0 : s.Right.Resolve(cbw);
			double w;
			if (k.IsReplaced) { ReplacedSize(k, cbw, cbh, out w, out double rh0); k.ContentH = rh0; }
			else if (!wA) { double sw = s.Width.K == Len.Value ? s.Width.Resolve(cbw) : SizeKeyword(k, s.Width, cbw) ?? 0; w = s.BorderBox ? sw - bp : sw; }
			else if (!lA && !rA) w = cbw - left - right - k.M[1] - k.M[3] - bp;
			else {
				ContentIntrinsic(k, out double mn, out double mx);
				double avail = cbw - (lA ? 0 : left) - (rA ? 0 : right) - k.M[1] - k.M[3] - bp;
				if (lA && rA) avail = k.StaticRtl ? k.StaticX - cbx - k.M[1] - k.M[3] - bp : cbx + cbw - k.StaticX - k.M[1] - k.M[3] - bp;
				w = Math.Min(Math.Max(mn, avail), mx);
			}
			w = k.IsReplaced ? w : ClampW(k, w, cbw);
			double outer = w + bp;
			if (!lA && !rA && !wA && s.Margin[3].IsAuto && s.Margin[1].IsAuto) {
				double rest = cbw - left - right - outer;
				k.M[3] = k.M[1] = Math.Max(0, rest / 2);
			}
			double x;
			if (lA && rA) {
				if (k.StaticRtl) x = k.StaticX - outer - k.M[1];
				else x = k.StaticX + k.M[3];
			}
			else if (!lA && (rA || !(k.Parent?.S.Rtl ?? false) || !wA)) x = cbx + left + k.M[3];
			else x = cbx + cbw - right - k.M[1] - outer;
			if (!lA && !rA && !wA && (k.Parent?.S.Rtl ?? false) && !(s.Margin[3].IsAuto && s.Margin[1].IsAuto)) x = cbx + cbw - right - k.M[1] - outer;
			if (!lA && !rA && !wA && !(k.Parent?.S.Rtl ?? false)) x = cbx + left + k.M[3];
			bool tA = s.Top.IsAuto, bA = s.Bottom.IsAuto;
			double topv = tA ? 0 : s.Top.Resolve(cbh), botv = bA ? 0 : s.Bottom.Resolve(cbh);
			double? specH = k.IsReplaced ? k.ContentH : SpecifiedHeight(k, cbh);
			if (specH == null && !tA && !bA) specH = Math.Max(0, cbh - topv - botv - k.M[0] - k.M[2] - bpv);
			double y0 = tA ? (bA ? k.StaticY + k.M[0] : 0) : cby + topv + k.M[0];
			k.ForceW = w;
			if (specH != null) k.ForceH = specH.Value;
			var saveM = (double[])k.M.Clone();
			LayoutBlockBox(k, x - k.M[3], w + bp + k.M[1] + k.M[3], y0, null, cbh, out _);
			k.M = saveM;
			k.ForceW = double.NaN; k.ForceH = double.NaN;
			double dxAdj = x - k.X;
			if (dxAdj != 0) Translate(k, dxAdj, 0);
			if (tA && !bA) {
				double ny = cby + cbh - botv - k.M[2] - k.H;
				Translate(k, 0, ny - k.Y);
			}
			if (!tA && !bA && specH != null && s.Margin[0].IsAuto && s.Margin[2].IsAuto) {
				double rest = cbh - topv - botv - k.H;
				Translate(k, 0, Math.Max(0, rest / 2));
			}
		}

		void LayoutFloat(Box f, Box container, double cx, double cw, double y, FloatMgr fm) {
			bool left = f.S.Float == 1;
			LayoutShrinkToFit(f, cw, cx, y);
			double ow = f.W + f.M[1] + f.M[3], oh = f.H + f.M[0] + f.M[2];
			double clearY = f.S.Clear != 0 ? fm.ClearY(f.S.Clear, y) : y;
			var (px, py) = fm.Place(ow, oh, left, clearY, cx, cx + cw);
			if (Paginate && Crosses(py, oh) && oh <= PageH && !AtPageTop(py)) py = NextPageTop(py);
			Translate(f, px + f.M[3] - f.X, py + f.M[0] - f.Y);
			fm.Add(px, py, px + ow, py + oh, left);
		}

		public void LayoutShrinkToFit(Box b, double availW, double x, double y) {
			ResolveBoxModel(b, availW);
			var s = b.S;
			double bp = BP(b);
			double w;
			if (b.IsReplaced) {
				ReplacedSize(b, availW, double.NaN, out w, out double rh);
				b.ContentH = rh;
			}
			else if (s.Width.IsAuto || s.Width.K == Len.KFit || s.Width.IsPctDep && double.IsNaN(availW)) {
				ContentIntrinsic(b, out double mn, out double mx);
				double avail = availW - b.M[1] - b.M[3] - bp;
				w = Math.Min(Math.Max(mn, avail), mx);
			}
			else {
				double sw = s.Width.K == Len.Value ? s.Width.Resolve(availW) : SizeKeyword(b, s.Width, availW) ?? 0;
				w = s.BorderBox ? sw - bp : sw;
			}
			if (!b.IsReplaced) w = ClampW(b, w, availW);
			if (b.Kind == BK.Table) w = Math.Max(w, TableMinWidth(b) - bp);
			b.ForceW = Math.Max(0, w);
			var mm = (double[])b.M.Clone();
			LayoutBlockBox(b, x, b.ForceW + bp + mm[1] + mm[3], y + mm[0], null, double.NaN, out _);
			b.M = mm;
			b.ForceW = double.NaN;
			double tx = x + mm[3] - b.X;
			if (tx != 0) Translate(b, tx, 0);
		}

		public void IntrinsicOuter(Box b, out double min, out double max) {
			var s = b.S;
			double mh = (s.Margin[1].IsFixed ? s.Margin[1].Px : 0) + (s.Margin[3].IsFixed ? s.Margin[3].Px : 0);
			double bp = s.BorderWidth[1] + s.BorderWidth[3] + (s.Padding[1].IsFixed ? s.Padding[1].Px : 0) + (s.Padding[3].IsFixed ? s.Padding[3].Px : 0);
			if (b.Kind == BK.Table && s.BorderCollapse) bp = (s.Padding[1].IsFixed ? s.Padding[1].Px : 0) + (s.Padding[3].IsFixed ? s.Padding[3].Px : 0);
			if (b.IsReplaced) {
				ReplacedSize(b, double.NaN, double.NaN, out double rw, out _);
				if (s.Width.IsPctDep && !s.Width.IsFixed) { min = mh + bp; max = rw + bp + mh; if (s.MaxWidth.IsPctDep) min = mh + bp; else min = max; return; }
				min = max = rw + bp + mh;
				return;
			}
			if (s.Width.IsFixed) {
				double w = s.Width.Px - (s.BorderBox ? bp : 0);
				if (b.Kind == BK.Table) { ContentIntrinsic(b, out double tmn, out _); w = Math.Max(w, tmn); }
				w = ClampWFixed(s, w, bp);
				min = max = w + bp + mh;
				return;
			}
			ContentIntrinsic(b, out min, out max);
			if (s.Width.K == Len.KMin) max = min;
			if (s.Width.K == Len.KMax) min = max;
			min = ClampWFixed(s, min, bp); max = ClampWFixed(s, max, bp);
			min += bp + mh; max += bp + mh;
		}

		static double ClampWFixed(Style s, double w, double bp) {
			double adj = s.BorderBox ? bp : 0;
			if (s.MaxWidth.IsFixed) w = Math.Min(w, s.MaxWidth.Px - adj);
			if (s.MinWidth.IsFixed) w = Math.Max(w, s.MinWidth.Px - adj);
			return Math.Max(0, w);
		}

		public void ContentIntrinsic(Box b, out double min, out double max) {
			if (b.CachedMin >= 0) { min = b.CachedMin; max = b.CachedMax; return; }
			min = 0; max = 0;
			switch (b.Kind) {
				case BK.Replaced:
					ReplacedSize(b, double.NaN, double.NaN, out double rw, out _);
					min = max = rw;
					break;
				case BK.Table:
					TableIntrinsic(b, out min, out max);
					break;
				case BK.Flex:
					FlexIntrinsic(b, out min, out max);
					break;
				case BK.Grid:
					GridIntrinsic(b, out min, out max);
					break;
				default:
					if (b.Kids.Count == 0) break;
					if (HasInlineContent(b)) { InlineIntrinsic(b, out min, out max); break; }
					double floatRun = 0;
					foreach (var k in b.Kids) {
						if (k.IsOutOfFlow) continue;
						IntrinsicOuter(k, out double kmn, out double kmx);
						if (k.IsFloat) { floatRun += kmx; max = Math.Max(max, floatRun); min = Math.Max(min, kmn); continue; }
						floatRun = 0;
						min = Math.Max(min, kmn);
						max = Math.Max(max, kmx);
					}
					break;
			}
			if (max < min) max = min;
			b.CachedMin = min; b.CachedMax = max;
		}

		public void ReplacedSize(Box b, double cbW, double cbH, out double w, out double h) {
			var s = b.S;
			double iw = b.IntrW, ih = b.IntrH;
			if (b.SvgEl != null && (iw <= 0 || ih <= 0)) { Svg.IntrinsicSize(b.SvgEl, out iw, out ih); }
			if (b.Img != null && b.Img.Orientation >= 5) (iw, ih) = (ih, iw);
			double ratio = s.AspectRatio > 0 ? s.AspectRatio : iw > 0 && ih > 0 ? iw / ih : 0;
			double bp = BP(b), bpv = BPV(b);
			double? sw = null, sh = null;
			if (s.Width.K == Len.Value) { var r = s.Width.TryResolve(cbW); if (r != null) sw = r.Value - (s.BorderBox ? bp : 0); }
			else if (s.Width.K == Len.KStretch && !double.IsNaN(cbW)) sw = cbW - b.M[1] - b.M[3] - bp;
			if (!double.IsNaN(b.ForceW)) sw = b.ForceW;
			if (s.Height.K == Len.Value) { var r = s.Height.TryResolve(cbH); if (r != null) sh = r.Value - (s.BorderBox ? bpv : 0); }
			if (!double.IsNaN(b.ForceH)) sh = b.ForceH;
			double adjW = s.BorderBox ? bp : 0, adjH = s.BorderBox ? bpv : 0;
			double minW = s.MinWidth.IsAuto ? 0 : Math.Max(0, (s.MinWidth.TryResolve(cbW) ?? 0) - adjW);
			double maxW = s.MaxWidth.IsNone ? double.PositiveInfinity : Math.Max(0, (s.MaxWidth.TryResolve(cbW) ?? double.PositiveInfinity) - adjW);
			double minH = s.MinHeight.IsAuto ? 0 : Math.Max(0, (s.MinHeight.TryResolve(cbH) ?? 0) - adjH);
			double maxH = s.MaxHeight.IsNone ? double.PositiveInfinity : Math.Max(0, (s.MaxHeight.TryResolve(cbH) ?? double.PositiveInfinity) - adjH);
			if (maxW < minW) maxW = minW;
			if (maxH < minH) maxH = minH;
			if (sw != null && sh != null) { w = sw.Value; h = sh.Value; }
			else if (sw != null) { w = sw.Value; h = ratio > 0 ? w / ratio : ih >= 0 ? ih : 150; }
			else if (sh != null) { h = sh.Value; w = ratio > 0 ? h * ratio : iw >= 0 ? iw : 300; }
			else if (iw >= 0 && ih >= 0) {
				w = iw; h = ih;
				if (ratio > 0 && (w > 0 && h > 0)) {
					Constrain(ref w, ref h, minW, maxW, minH, maxH);
					w = Math.Max(0, w); h = Math.Max(0, h);
					return;
				}
			}
			else if (ratio > 0) { w = !double.IsNaN(cbW) ? cbW - b.M[1] - b.M[3] - bp : 300; h = w / ratio; }
			else { w = iw >= 0 ? iw : 300; h = ih >= 0 ? ih : 150; }
			w = Math.Min(Math.Max(w, minW), maxW);
			h = Math.Min(Math.Max(h, minH), maxH);
			if (sw == null && sh != null && ratio > 0) w = Math.Min(Math.Max(h * ratio, minW), maxW);
			if (sh == null && sw != null && ratio > 0) h = Math.Min(Math.Max(w / ratio, minH), maxH);
		}

		static void Constrain(ref double w, ref double h, double minW, double maxW, double minH, double maxH) {
			double w0 = w, h0 = h;
			bool wMax = w0 > maxW, wMin = w0 < minW, hMax = h0 > maxH, hMin = h0 < minH;
			if (wMax && hMax) {
				if (maxW / w0 <= maxH / h0) { w = maxW; h = Math.Max(minH, maxW * h0 / w0); }
				else { w = Math.Max(minW, maxH * w0 / h0); h = maxH; }
			}
			else if (wMin && hMin) {
				if (minW / w0 <= minH / h0) { w = Math.Min(maxW, minH * w0 / h0); h = minH; }
				else { w = minW; h = Math.Min(maxH, minW * h0 / w0); }
			}
			else if (wMin && hMax) { w = minW; h = maxH; }
			else if (wMax && hMin) { w = maxW; h = minH; }
			else if (wMax) { w = maxW; h = Math.Max(maxW * h0 / w0, minH); }
			else if (wMin) { w = minW; h = Math.Min(minW * h0 / w0, maxH); }
			else if (hMax) { w = Math.Max(maxH * w0 / h0, minW); h = maxH; }
			else if (hMin) { w = Math.Min(minH * w0 / h0, maxW); h = minH; }
		}

		void PlaceMarker(Box li) {
			var ms = li.MarkerStyle ?? li.S;
			double baseline = double.NaN;
			LineBox firstLine = FindFirstLine(li);
			if (firstLine != null) baseline = firstLine.Baseline;
			if (li.MarkerImg != null) {
				var img = li.MarkerImg;
				double iw = img.W, ih = img.H;
				double bl = double.IsNaN(baseline) ? li.ContentY + Primary(ms).Asc : baseline;
				li.MarkerImgW = iw; li.MarkerImgH = ih;
				li.MarkerImgX = li.S.Rtl ? li.ContentX + li.ContentW + ms.FontSize * 0.5 : li.ContentX - iw - ms.FontSize * 0.5;
				li.MarkerImgY = bl - ih;
				return;
			}
			string lst = li.S.ListStyleType;
			bool normalContent = li.MarkerStyle == null || li.MarkerStyle.Content == null || li.MarkerStyle.Content == "normal";
			if (normalContent && (lst == "disc" || lst == "circle" || lst == "square")) {
				var mf = Primary(ms);
				int asc = (int)mf.Asc;
				int offset = asc * 2 / 3;
				int bw = (offset + 1) / 2;
				double top = (double.IsNaN(baseline) ? li.ContentY + mf.Asc : baseline) - asc;
				double yOff = 3 * (asc - offset) / 2;
				li.MarkerShape = (byte)(lst == "disc" ? 1 : lst == "circle" ? 2 : 3);
				li.MarkerSW = bw;
				li.MarkerSY = top + yOff;
				li.MarkerSX = li.S.Rtl ? li.ContentX + li.ContentW + offset + 8 - 1 - bw : li.ContentX - offset - 8 + 1;
				li.MarkerColor = ms.Color;
				li.MarkerFrags = null;
				return;
			}
			var tmpStyle = ms.InheritFrom();
			tmpStyle.Rtl = li.S.Rtl;
			tmpStyle.Ws = WSp.Pre;
			tmpStyle.TextAlign = TA.Start;
			tmpStyle.TextIndent = Len.Zero;
			tmpStyle.TabularNums = true;
			tmpStyle.TextTransform = 0;
			tmpStyle.LetterSpacing = 0;
			tmpStyle.WordSpacing = 0;
			tmpStyle.Display = Disp.Block;
			if (ms.Content == null || ms.Content == "normal") { tmpStyle.Color = ms.Color; }
			var holder = new Box(BK.Block, null, tmpStyle) { Anon = true };
			var tb = new Box(BK.Text, null, tmpStyle) { Text = li.MarkerText };
			holder.Add(tb);
			holder.Parent = li;
			bool pg = Paginate;
			Paginate = false;
			LayoutInlineContent(holder, 0, 100000, 0, new FloatMgr());
			Paginate = pg;
			if (holder.Lines == null || holder.Lines.Count == 0) return;
			var line = holder.Lines[0];
			double mw = 0, minX = double.PositiveInfinity, maxX = double.NegativeInfinity;
			foreach (var g in line.Glyphs) { minX = Math.Min(minX, g.X0); maxX = Math.Max(maxX, g.X1); }
			if (double.IsInfinity(minX)) return;
			mw = maxX - minX;
			double bl2 = double.IsNaN(baseline) ? li.ContentY + (line.Baseline - line.Y) : baseline;
			double targetX = li.S.Rtl ? li.X + li.W - li.Bd[1] - li.P[1] + 0 : li.X + li.Bd[3] + li.P[3] - mw;
			if (!li.S.Rtl) targetX = li.ContentX - mw;
			else targetX = li.ContentX + li.ContentW;
			double dx = targetX - minX, dy = bl2 - line.Baseline;
			foreach (var g in line.Glyphs) g.Shift(dx, dy);
			li.MarkerFrags = line.Glyphs;
		}

		static LineBox FindFirstLine(Box b) {
			if (b.Lines != null && b.Lines.Count > 0) return b.Lines[0];
			foreach (var k in b.Kids) {
				if (k.IsOutOfFlow || k.IsFloat || k.Kind == BK.Text || k.IsAtomicInline) continue;
				var l = FindFirstLine(k);
				if (l != null) return l;
			}
			return null;
		}
	}

	internal sealed class TGrid {
		public List<Box> Rows = new();
		public List<Box> Groups = new();
		public List<Box> RowGroup = new();
		public List<Box> Cells = new();
		public List<Box> Captions = new();
		public List<Box> Cols = new();
		public int NCols;
		public Box[,] At;
		public Box Head, Foot;
	}

	internal sealed partial class LayoutEngine {
		readonly Dictionary<Box, TGrid> _grids = new();

		public TGrid GridOf(Box t) => _grids.TryGetValue(t, out var g) ? g : null;

		TGrid Grid(Box t) {
			if (_grids.TryGetValue(t, out var g)) return g;
			g = new TGrid();
			Box head = null, foot = null;
			var bodies = new List<Box>();
			foreach (var k in t.Kids) {
				if (k.Kind == BK.Caption) g.Captions.Add(k);
				else if (k.Kind == BK.RowGroup) {
					if (k.S.Display == Disp.HeaderGroup && head == null) { head = k; k.IsHeaderGroup = true; }
					else if (k.S.Display == Disp.FooterGroup && foot == null) { foot = k; k.IsFooterGroup = true; }
					else bodies.Add(k);
				}
				else if (k.Kind == BK.ColGroup) {
					if (k.Kids.Any(c => c.Kind == BK.Column)) foreach (var c in k.Kids.Where(c => c.Kind == BK.Column)) for (int i = 0; i < c.ColSpan; i++) g.Cols.Add(c);
					else for (int i = 0; i < k.ColSpan; i++) g.Cols.Add(k);
				}
				else if (k.Kind == BK.Column) for (int i = 0; i < k.ColSpan; i++) g.Cols.Add(k);
			}
			g.Head = head;
			g.Foot = foot;
			var groups = new List<Box>();
			if (head != null) groups.Add(head);
			groups.AddRange(bodies);
			if (foot != null) groups.Add(foot);
			g.Groups = groups;
			foreach (var grp in groups) foreach (var r in grp.Kids) if (r.Kind == BK.Row) { g.Rows.Add(r); g.RowGroup.Add(grp); }
			var occ = new List<List<Box>>();
			int maxCols = g.Cols.Count;
			for (int ri = 0; ri < g.Rows.Count; ri++) {
				while (occ.Count <= ri) occ.Add(new List<Box>());
				int col = 0;
				foreach (var c in g.Rows[ri].Kids) {
					if (c.Kind != BK.Cell) continue;
					while (col < occ[ri].Count && occ[ri][col] != null) col++;
					c.Row = ri; c.Col = col;
					int rs = c.RowSpan == 65534 ? Math.Max(1, CountRowsInGroup(g, ri)) : c.RowSpan;
					rs = Math.Max(1, Math.Min(rs, CountRowsInGroup(g, ri)));
					c.RowSpan = rs;
					for (int r = ri; r < ri + rs; r++) {
						while (occ.Count <= r) occ.Add(new List<Box>());
						while (occ[r].Count < col + c.ColSpan) occ[r].Add(null);
						for (int cc = col; cc < col + c.ColSpan; cc++) occ[r][cc] = c;
					}
					g.Cells.Add(c);
					col += c.ColSpan;
					maxCols = Math.Max(maxCols, col);
				}
			}
			g.NCols = maxCols;
			g.At = new Box[g.Rows.Count, Math.Max(1, maxCols)];
			for (int r = 0; r < g.Rows.Count && r < occ.Count; r++) for (int c = 0; c < occ[r].Count; c++) g.At[r, c] = occ[r][c];
			_grids[t] = g;
			return g;
		}

		static int CountRowsInGroup(TGrid g, int ri) {
			var grp = g.RowGroup[ri];
			int n = 0;
			for (int r = ri; r < g.Rows.Count && g.RowGroup[r] == grp; r++) n++;
			return n;
		}

		double HSpace(Box t) => t.S.BorderCollapse ? 0 : t.S.BorderSpacingH;
		double VSpace(Box t) => t.S.BorderCollapse ? 0 : t.S.BorderSpacingV;

		void CellBP(Box c, Box t, out double bpH) {
			var s = c.S;
			double pl = s.Padding[3].IsFixed ? s.Padding[3].Px : 0, pr = s.Padding[1].IsFixed ? s.Padding[1].Px : 0;
			double bl, br;
			if (t.S.BorderCollapse && c.CB != null) { bl = c.CB[3].w / 2; br = c.CB[1].w / 2; }
			else { bl = s.BorderWidth[3]; br = s.BorderWidth[1]; }
			bpH = pl + pr + bl + br;
		}

		void ColumnConstraints(Box t, TGrid g, out double[] min, out double[] max, out double[] pct, out bool[] fixedCol) {
			int n = g.NCols;
			min = new double[n]; max = new double[n]; pct = new double[n]; fixedCol = new bool[n];
			if (t.S.BorderCollapse) ResolveCollapsed(t, g);
			for (int c = 0; c < n && c < g.Cols.Count; c++) {
				var cs = g.Cols[c].S;
				if (cs.Width.IsFixed) { min[c] = Math.Max(min[c], 0); max[c] = Math.Max(max[c], cs.Width.Px); fixedCol[c] = true; }
				else if (cs.Width.IsPctDep && cs.Width.Fn == null) pct[c] = Math.Max(pct[c], cs.Width.Pct);
			}
			var spanning = new List<Box>();
			foreach (var cell in g.Cells) {
				if (cell.ColSpan > 1) { spanning.Add(cell); continue; }
				CellConstraint(t, cell, out double cmin, out double cmax, out double cpct, out bool cfix);
				int c = cell.Col;
				min[c] = Math.Max(min[c], cmin);
				max[c] = Math.Max(max[c], cmax);
				if (cpct > 0) pct[c] = Math.Max(pct[c], cpct);
				if (cfix) fixedCol[c] = true;
			}
			for (int c = 0; c < n; c++) if (max[c] < min[c]) max[c] = min[c];
			double hs = HSpace(t);
			foreach (var cell in spanning.OrderBy(x => x.ColSpan)) {
				CellConstraint(t, cell, out double cmin, out double cmax, out double cpct, out bool cfix);
				int c0 = cell.Col, c1 = Math.Min(n, cell.Col + cell.ColSpan);
				double sp = hs * (c1 - c0 - 1);
				double curMin = 0, curMax = 0;
				for (int c = c0; c < c1; c++) { curMin += min[c]; curMax += max[c]; }
				if (cmin - sp > curMin) {
					double extra = cmin - sp - curMin;
					double tot = curMax > 0 ? curMax : c1 - c0;
					for (int c = c0; c < c1; c++) min[c] += extra * (curMax > 0 ? max[c] / tot : 1.0 / (c1 - c0));
				}
				if (cmax - sp > curMax) {
					double extra = cmax - sp - curMax;
					double tot = curMax > 0 ? curMax : c1 - c0;
					for (int c = c0; c < c1; c++) max[c] += extra * (curMax > 0 ? max[c] / tot : 1.0 / (c1 - c0));
				}
				if (cpct > 0) {
					double have = 0; int nonPct = 0;
					for (int c = c0; c < c1; c++) { have += pct[c]; if (pct[c] == 0) nonPct++; }
					if (cpct > have && nonPct > 0) for (int c = c0; c < c1; c++) if (pct[c] == 0) pct[c] = (cpct - have) / nonPct;
				}
				for (int c = c0; c < c1; c++) if (max[c] < min[c]) max[c] = min[c];
			}
			double sumPct = 0;
			for (int c = 0; c < n; c++) {
				if (sumPct + pct[c] > 100) pct[c] = Math.Max(0, 100 - sumPct);
				sumPct += pct[c];
			}
		}

		void CellConstraint(Box t, Box cell, out double min, out double max, out double pct, out bool fix) {
			ContentIntrinsic(cell, out double cmn, out double cmx);
			CellBP(cell, t, out double bp);
			var s = cell.S;
			pct = 0; fix = false;
			min = cmn + bp;
			max = cmx + bp;
			if (s.Width.IsFixed) {
				double w = s.Width.Px + (s.BorderBox ? 0 : bp);
				max = Math.Max(min, w);
				fix = true;
			}
			else if (s.Width.IsPctDep && s.Width.Fn == null) pct = s.Width.Pct;
			if (s.MinWidth.IsFixed) { min = Math.Max(min, s.MinWidth.Px + bp); max = Math.Max(max, min); }
			if (s.MaxWidth.IsFixed && !s.Width.IsFixed) max = Math.Max(min, Math.Min(max, s.MaxWidth.Px + bp));
		}

		public void TableIntrinsic(Box t, out double min, out double max) {
			var g = Grid(t);
			ColumnConstraints(t, g, out var mn, out var mx, out var pct, out _);
			double hs = HSpace(t);
			double sp = hs * (g.NCols + 1);
			if (g.NCols == 0) sp = 0;
			min = mn.Sum() + sp;
			max = mx.Sum() + sp;
			double sumPct = pct.Sum();
			if (sumPct > 0) {
				double nonPctMax = 0;
				for (int c = 0; c < g.NCols; c++) if (pct[c] == 0) nonPctMax += mx[c];
				double need = max;
				if (sumPct < 100) need = Math.Max(need, nonPctMax / (1 - sumPct / 100) + sp);
				for (int c = 0; c < g.NCols; c++) if (pct[c] > 0) need = Math.Max(need, mx[c] * 100 / pct[c] + sp);
				max = need;
			}
			foreach (var cap in g.Captions) { IntrinsicOuter(cap, out double cmn, out _); min = Math.Max(min, cmn - BP(t)); }
			if (max < min) max = min;
		}

		public double TableAutoWidth(Box t, double availOuter) {
			TableIntrinsic(t, out double mn, out double mx);
			double bp = BP(t);
			double avail = availOuter - bp;
			return Math.Min(Math.Max(mn, avail), mx);
		}

		public double TableMinWidth(Box t) {
			TableIntrinsic(t, out double mn, out _);
			return mn + BP(t);
		}

		double[] DistributeColumns(Box t, TGrid g, double target) {
			int n = g.NCols;
			var w = new double[n];
			if (n == 0) return w;
			ColumnConstraints(t, g, out var min, out var max, out var pct, out var fix);
			if (t.S.TableFixed && !t.S.Width.IsAuto) return FixedColumns(t, g, target, min);
			var pctW = new double[n];
			for (int c = 0; c < n; c++) pctW[c] = pct[c] > 0 ? Math.Max(min[c], pct[c] * target / 100) : min[c];
			double gMin = min.Sum();
			double gPct = 0, gSpec = 0, gMax = 0;
			for (int c = 0; c < n; c++) {
				gPct += pct[c] > 0 ? pctW[c] : min[c];
				gSpec += pct[c] > 0 ? pctW[c] : fix[c] ? max[c] : min[c];
				gMax += pct[c] > 0 ? pctW[c] : max[c];
			}
			if (target <= gMin) { Array.Copy(min, w, n); return w; }
			if (target <= gPct) {
				double r = gPct > gMin ? (target - gMin) / (gPct - gMin) : 0;
				for (int c = 0; c < n; c++) w[c] = pct[c] > 0 ? min[c] + (pctW[c] - min[c]) * r : min[c];
				return w;
			}
			if (target <= gSpec) {
				double r = gSpec > gPct ? (target - gPct) / (gSpec - gPct) : 0;
				for (int c = 0; c < n; c++) w[c] = pct[c] > 0 ? pctW[c] : fix[c] ? min[c] + (max[c] - min[c]) * r : min[c];
				return w;
			}
			if (target <= gMax) {
				double r = gMax > gSpec ? (target - gSpec) / (gMax - gSpec) : 0;
				for (int c = 0; c < n; c++) w[c] = pct[c] > 0 ? pctW[c] : fix[c] ? max[c] : min[c] + (max[c] - min[c]) * r;
				return w;
			}
			for (int c = 0; c < n; c++) w[c] = pct[c] > 0 ? pctW[c] : max[c];
			double excess = target - gMax;
			var auto = Enumerable.Range(0, n).Where(c => pct[c] == 0 && !fix[c]).ToList();
			if (auto.Count > 0) {
				double tot = auto.Sum(c => max[c]);
				foreach (var c in auto) w[c] += tot > 0 ? excess * max[c] / tot : excess / auto.Count;
				return w;
			}
			var fixedCols = Enumerable.Range(0, n).Where(c => pct[c] == 0 && fix[c]).ToList();
			if (fixedCols.Count > 0) {
				double tot = fixedCols.Sum(c => max[c]);
				foreach (var c in fixedCols) w[c] += tot > 0 ? excess * max[c] / tot : excess / fixedCols.Count;
				return w;
			}
			double tp = pct.Sum();
			for (int c = 0; c < n; c++) w[c] += tp > 0 ? excess * pct[c] / tp : excess / n;
			return w;
		}

		double[] FixedColumns(Box t, TGrid g, double target, double[] min) {
			int n = g.NCols;
			var w = new double[n];
			var set = new bool[n];
			for (int c = 0; c < n && c < g.Cols.Count; c++) {
				var cs = g.Cols[c].S;
				if (cs.Width.IsValue && !cs.Width.IsAuto) { w[c] = cs.Width.Resolve(target); set[c] = true; }
			}
			if (g.Rows.Count > 0) {
				foreach (var cell in g.Cells.Where(x => x.Row == 0)) {
					if (!cell.S.Width.IsValue) continue;
					CellBP(cell, t, out double bp);
					double cw = cell.S.Width.Resolve(target) + (cell.S.BorderBox ? 0 : bp);
					int span = Math.Min(cell.ColSpan, n - cell.Col);
					for (int c = cell.Col; c < cell.Col + span; c++) if (!set[c]) { w[c] = cw / span; set[c] = true; }
				}
			}
			double used = w.Sum();
			int unset = set.Count(x => !x);
			if (unset > 0) {
				double each = Math.Max(0, (target - used) / unset);
				for (int c = 0; c < n; c++) if (!set[c]) w[c] = each;
			}
			else if (used < target && used > 0) {
				for (int c = 0; c < n; c++) w[c] += (target - used) * w[c] / used;
			}
			return w;
		}

		public double LayoutTable(Box t, double cx, double w, double cy, double cbH) {
			var g = Grid(t);
			if (t.S.BorderCollapse) ResolveCollapsed(t, g);
			double hs = HSpace(t), vs = VSpace(t);
			int n = g.NCols;
			double gridW = w - hs * (n + 1);
			if (n == 0) gridW = 0;
			var colW = DistributeColumns(t, g, Math.Max(0, gridW));
			bool rtl = t.S.Rtl;
			var colX = new double[n + 1];
			double acc = hs;
			for (int c = 0; c < n; c++) { colX[c] = acc; acc += colW[c] + hs; }
			colX[n] = acc;
			double X(int c0, int span) {
				double left = colX[c0];
				double width = 0;
				for (int c = c0; c < c0 + span && c < n; c++) width += colW[c];
				width += hs * (Math.Min(span, n - c0) - 1);
				if (rtl) return cx + w - left - width;
				return cx + left;
			}
			double capTop = 0;
			double borderTop = t.Y;
			foreach (var cap in g.Captions.Where(c => !c.S.CaptionBottom)) {
				ResolveBoxModel(cap, t.W);
				LayoutBlockBox(cap, t.X, t.W, borderTop + capTop + cap.M[0], null, double.NaN, out _);
				capTop += cap.H + cap.M[0] + cap.M[2];
			}
			t.TY = capTop;
			double gridTop = cy + capTop;
			double y = gridTop + (g.Rows.Count > 0 ? vs : 0);
			var rowTop = new double[g.Rows.Count];
			var rowH = new double[g.Rows.Count];
			double headH = 0;
			int headRows = g.Head != null ? g.Head.Kids.Count(k => k.Kind == BK.Row) : 0;
			double savedReserve = Reserve, savedReserveBottom = ReserveBottom;
			double tableStartPage = Paginate ? Math.Floor((t.Y + 1e-6) / PageH) : 0;
			t.RepeatHeaders = null;
			t.RepeatFooters = null;
			int footRows = g.Foot != null ? g.Foot.Kids.Count(k => k.Kind == BK.Row) : 0;
			int footStart = g.Rows.Count - footRows;
			double footH = 0;
			if (Paginate && footRows > 0 && footStart > headRows) {
				Paginate = false;
				try {
					for (int r = footStart; r < g.Rows.Count; r++) {
						ResolveBoxModel(g.Rows[r], w);
						double rh = g.Rows[r].S.Height.IsValue ? g.Rows[r].S.Height.TryResolve(cbH) ?? 0 : 0;
						foreach (var cell in g.Cells.Where(c => c.Row == r)) {
							LayoutCell(t, g, cell, X(cell.Col, cell.ColSpan), colW, cell.Col, cell.ColSpan, hs, 0, w);
							if (cell.RowSpan == 1) rh = Math.Max(rh, Math.Max(cell.H, CellSpecifiedHeight(cell, cbH)));
						}
						footH += rh + vs;
					}
				}
				finally { Paginate = true; }
			}
			bool repeatFoot = footH > 0 && footH <= PageH / 4;
			if (repeatFoot) footH += t.Bd[2] + t.P[2];
			int clStart = -1, clEnd = -1, clPushed = -1;
			Box curGroup = null;
			double groupTop = 0;
			for (int r = 0; r < g.Rows.Count; r++) {
				var row = g.Rows[r];
				var grp = g.RowGroup[r];
				if (grp != curGroup) {
					if (curGroup != null) FinishGroup(curGroup, t, groupTop, rowTop, rowH, r, g, cx, w);
					curGroup = grp;
					groupTop = y;
				}
				if (Paginate && r == headRows && headRows > 0) {
					headH = y - (gridTop + vs);
					if (headH <= PageH / 4) Reserve = savedReserve + headH + vs;
				}
				if (repeatFoot) ReserveBottom = r >= headRows && r < footStart ? savedReserveBottom + footH : savedReserveBottom;
				if (r > clEnd) {
					clStart = clEnd = r;
					for (int q = r; q <= clEnd; q++)
						foreach (var c in g.Cells)
							if (c.Row == q && c.Row + c.RowSpan - 1 > clEnd) clEnd = Math.Min(g.Rows.Count - 1, c.Row + c.RowSpan - 1);
				}
				if (Paginate && Reserve > savedReserve && r >= headRows) {
					double off = y - Math.Floor((y + 1e-6) / PageH) * PageH;
					if (off < 0.01 && Math.Floor((y + 1e-6) / PageH) > tableStartPage) y += Reserve - savedReserve;
				}
				ResolveBoxModel(row, w);
				double rTop = y;
				double h = 0;
				if (row.S.Height.IsValue) h = Math.Max(h, row.S.Height.TryResolve(cbH) ?? 0);
				var cellsHere = g.Cells.Where(c => c.Row == r).ToList();
				foreach (var cell in cellsHere) LayoutCell(t, g, cell, X(cell.Col, cell.ColSpan), colW, cell.Col, cell.ColSpan, hs, rTop, w);
				if (Paginate && row.S.BreakInside == Style.BrAvoid) {
					double maxH = cellsHere.Count > 0 ? cellsHere.Max(c => c.RowSpan == 1 ? c.H : 0) : 0;
					if (Crosses(rTop, maxH) && maxH <= PageH - Reserve && !AtPageTop(rTop)) {
						rTop = NextPageTop(rTop);
						foreach (var cell in cellsHere) LayoutCell(t, g, cell, X(cell.Col, cell.ColSpan), colW, cell.Col, cell.ColSpan, hs, rTop, w);
					}
				}
				if (Paginate && !AtPageTop(rTop) && cellsHere.Count > 0) {
					double maxH = cellsHere.Max(c => c.RowSpan == 1 ? c.H : 0);
					if (Crosses(rTop, maxH)) {
						double boundary = (Math.Floor((rTop + 1e-6) / PageH) + 1) * PageH;
						bool anyFits = cellsHere.Any(c => FirstContentTop(c) < boundary - 1e-6);
						if (!anyFits) {
							rTop = NextPageTop(rTop);
							foreach (var cell in cellsHere) LayoutCell(t, g, cell, X(cell.Col, cell.ColSpan), colW, cell.Col, cell.ColSpan, hs, rTop, w);
						}
					}
				}
				foreach (var cell in cellsHere) {
					if (cell.RowSpan == 1) h = Math.Max(h, cell.H);
					double ch = CellSpecifiedHeight(cell, cbH);
					if (cell.RowSpan == 1 && ch > h) h = ch;
				}
				rowTop[r] = rTop;
				rowH[r] = h;
				foreach (var cell in g.Cells.Where(c => c.RowSpan > 1 && c.Row + c.RowSpan - 1 == r)) {
					double need = Math.Max(cell.H, CellSpecifiedHeight(cell, cbH));
					double bottom = rowTop[cell.Row] + need;
					if (bottom > rTop + rowH[r]) rowH[r] = bottom - rTop;
				}
				row.X = cx; row.W = w; row.Y = rTop; row.H = rowH[r];
				y = rTop + rowH[r] + vs;
				if (Paginate && r == clEnd && clEnd > clStart && clPushed != clStart) {
					double top0 = rowTop[clStart], ch = rowTop[r] + rowH[r] - top0;
					if (Crosses(top0, ch) && !AtPageTop(top0) && ch <= PageH - Reserve - ReserveBottom) {
						clPushed = clStart;
						y = NextPageTop(top0);
						r = clStart - 1;
						clEnd = r;
						continue;
					}
				}
			}
			ReserveBottom = savedReserveBottom;
			if (curGroup != null) FinishGroup(curGroup, t, groupTop, rowTop, rowH, g.Rows.Count, g, cx, w);
			Reserve = savedReserve;
			foreach (var cell in g.Cells) {
				int last = Math.Min(g.Rows.Count - 1, cell.Row + cell.RowSpan - 1);
				double top = rowTop[cell.Row];
				double bottom = rowTop[last] + rowH[last];
				double contentH = cell.H;
				double fullH = bottom - top;
				cell.H = fullH;
				double innerOld = contentH - BPV(cell);
				double innerNew = fullH - BPV(cell);
				double dy = 0;
				bool crossing = Paginate && Crosses(top, fullH);
				byte va = cell.S.VAlign;
				if (va == Style.VaMiddle) dy = (innerNew - innerOld) / 2;
				else if (va == Style.VaBottom) dy = innerNew - innerOld;
				else if (va == Style.VaBaseline || va == Style.VaSub || va == Style.VaSuper || va == Style.VaTextTop || va == Style.VaTextBottom || va == Style.VaLength) {
					double rb = RowBaseline(g, cell.Row, rowTop);
					double cb = CellBaseline(cell);
					if (!double.IsNaN(rb) && !double.IsNaN(cb)) dy = rb - cb;
				}
				if (dy > 0.01 && !crossing) ShiftContent(cell, dy);
			}
			double gridBottom = g.Rows.Count > 0 ? y : gridTop;
			if (g.Rows.Count == 0) gridBottom = gridTop;
			double gridContentH = gridBottom - gridTop;
			if (t.S.Height.IsValue) {
				var sh = SpecifiedHeight(t, cbH);
				if (sh != null && sh.Value > gridContentH && g.Rows.Count > 0) {
					double extra = sh.Value - gridContentH;
					double sumH = rowH.Sum();
					for (int r = 0; r < g.Rows.Count; r++) {
						double add = sumH > 0 ? extra * rowH[r] / sumH : extra / g.Rows.Count;
						double shift = 0;
						for (int k = 0; k < r; k++) shift += sumH > 0 ? extra * rowH[k] / sumH : extra / g.Rows.Count;
						foreach (var cell in g.Cells.Where(c => c.Row == r)) { Translate(cell, 0, shift); cell.H += add; }
						g.Rows[r].Y += shift; g.Rows[r].H += add;
					}
					gridContentH = sh.Value;
				}
			}
			t.TH = t.Bd[0] + t.P[0] + gridContentH + t.P[2] + t.Bd[2];
			double capBottomY = t.Y + t.TY + t.TH;
			double capBottom = 0;
			foreach (var cap in g.Captions.Where(c => c.S.CaptionBottom)) {
				ResolveBoxModel(cap, t.W);
				LayoutBlockBox(cap, t.X, t.W, capBottomY + capBottom + cap.M[0], null, double.NaN, out _);
				capBottom += cap.H + cap.M[0] + cap.M[2];
			}
			if (g.Head != null && Paginate) {
				double headTop = g.Head.Y, hh = g.Head.H;
				int firstPage = (int)Math.Floor((headTop + 1e-6) / PageH);
				int lastPage = (int)Math.Floor((gridBottom - 1e-6) / PageH);
				for (int p = firstPage + 1; p <= lastPage; p++) {
					double py = p * PageH + savedReserve;
					(t.RepeatHeaders ??= new()).Add((py, hh));
				}
			}
			if (repeatFoot && g.Foot != null && footStart > headRows) {
				int p0 = (int)Math.Floor((rowTop[headRows] + 1e-6) / PageH);
				int pEnd = (int)Math.Floor((g.Foot.Y + 1e-6) / PageH);
				for (int p = p0; p < pEnd; p++) {
					double bottom = double.NaN;
					for (int r = headRows; r < footStart; r++)
						if (Math.Floor((rowTop[r] + 1e-6) / PageH) == p) bottom = double.IsNaN(bottom) ? rowTop[r] + rowH[r] : Math.Max(bottom, rowTop[r] + rowH[r]);
					if (double.IsNaN(bottom)) continue;
					double fy = Math.Min(bottom, (p + 1) * PageH - savedReserveBottom - footH) + vs;
					(t.RepeatFooters ??= new()).Add((fy, g.Foot.H));
				}
			}
			t.Baseline = g.Rows.Count > 0 ? RowBaseline(g, 0, rowTop) : double.NaN;
			if (double.IsNaN(t.Baseline) && g.Rows.Count > 0) t.Baseline = rowTop[0] + rowH[0];
			t.LastBaseline = t.Baseline;
			return capTop + gridContentH + capBottom;
		}

		void FinishGroup(Box grp, Box t, double top, double[] rowTop, double[] rowH, int endRow, TGrid g, double cx, double w) {
			int first = -1, last = -1;
			for (int r = 0; r < endRow; r++) if (g.RowGroup[r] == grp) { if (first < 0) first = r; last = r; }
			grp.X = cx; grp.W = w;
			if (first < 0) { grp.Y = top; grp.H = 0; return; }
			grp.Y = rowTop[first];
			grp.H = rowTop[last] + rowH[last] - rowTop[first];
		}

		static double FirstContentTop(Box b) {
			if (b.Lines != null && b.Lines.Count > 0) return b.Lines[0].Y;
			foreach (var k in b.Kids) {
				if (k.IsOutOfFlow || k.Kind == BK.Text) continue;
				if (k.IsReplaced || k.IsAtomicInline || k.IsFloat) return k.Y;
				double v = FirstContentTop(k);
				if (!double.IsInfinity(v)) return v;
			}
			return double.PositiveInfinity;
		}

		double CellSpecifiedHeight(Box cell, double cbH) {
			var s = cell.S;
			if (!s.Height.IsValue) return 0;
			var h = s.Height.TryResolve(cbH);
			if (h == null) return 0;
			return s.BorderBox ? h.Value : h.Value + BPV(cell);
		}

		void LayoutCell(Box t, TGrid g, Box cell, double x, double[] colW, int c0, int span, double hs, double top, double tableW) {
			double width = 0;
			for (int c = c0; c < c0 + span && c < colW.Length; c++) width += colW[c];
			width += hs * (Math.Min(span, colW.Length - c0) - 1);
			ResolveBoxModel(cell, tableW);
			if (t.S.BorderCollapse && cell.CB != null) for (int i = 0; i < 4; i++) cell.Bd[i] = cell.CB[i].w / 2;
			cell.M[0] = cell.M[1] = cell.M[2] = cell.M[3] = 0;
			double bp = cell.P[1] + cell.P[3] + cell.Bd[1] + cell.Bd[3];
			cell.ForceW = Math.Max(0, width - bp);
			var h0 = cell.S.Height;
			cell.S.Height = Len.Auto;
			try {
				if (t.S.BorderCollapse && cell.CB != null) {
					var orig = (double[])cell.S.BorderWidth.Clone();
					for (int i = 0; i < 4; i++) cell.S.BorderWidth[i] = cell.CB[i].w / 2;
					LayoutBlockBox(cell, x, width, top, null, double.NaN, out _);
					for (int i = 0; i < 4; i++) cell.S.BorderWidth[i] = orig[i];
				}
				else LayoutBlockBox(cell, x, width, top, null, double.NaN, out _);
			}
			finally { cell.S.Height = h0; cell.ForceW = double.NaN; }
			cell.X = x; cell.W = width;
		}

		double RowBaseline(TGrid g, int r, double[] rowTop) {
			double best = double.NaN;
			foreach (var c in g.Cells) {
				if (c.Row != r) continue;
				if (c.S.VAlign != Style.VaBaseline) continue;
				double cb = CellBaseline(c);
				if (double.IsNaN(cb)) continue;
				best = double.IsNaN(best) ? cb : Math.Max(best, cb);
			}
			return best;
		}

		double CellBaseline(Box c) {
			double b = FirstBaselineOf(c);
			if (double.IsNaN(b)) return double.NaN;
			return b;
		}

		void ShiftContent(Box b, double dy) {
			if (b.Lines != null) foreach (var l in b.Lines) l.Shift(0, dy);
			if (!double.IsNaN(b.Baseline)) b.Baseline += dy;
			foreach (var k in b.Kids) Translate(k, 0, dy);
		}

		static int StylePri(BS s) => s switch { BS.Hidden => 100, BS.Double => 9, BS.Solid => 8, BS.Dashed => 7, BS.Dotted => 6, BS.Ridge => 5, BS.Outset => 4, BS.Groove => 3, BS.Inset => 2, BS.None => 0, _ => 1 };

		static (double w, BS s, Rgba c) Border(Style st, int side) => (st.BorderStyle[side] == BS.None || st.BorderStyle[side] == BS.Hidden ? 0 : st.BorderWidth[side], st.BorderStyle[side], st.BorderColor[side]);

		static (double w, BS s, Rgba c) Win((double w, BS s, Rgba c) a, (double w, BS s, Rgba c) b) {
			if (a.s == BS.Hidden) return (0, BS.Hidden, a.c);
			if (b.s == BS.Hidden) return (0, BS.Hidden, b.c);
			if (a.s == BS.None) return b;
			if (b.s == BS.None) return a;
			if (b.w > a.w) return b;
			if (a.w > b.w) return a;
			if (StylePri(b.s) > StylePri(a.s)) return b;
			return a;
		}

		void CollapsedTableBorders(Box t) {
			var g = Grid(t);
			ResolveCollapsed(t, g);
			double[] mx = new double[4];
			foreach (var c in g.Cells) {
				if (c.CB == null) continue;
				if (c.Row == 0) mx[0] = Math.Max(mx[0], c.CB[0].w);
				if (c.Row + c.RowSpan >= g.Rows.Count) mx[2] = Math.Max(mx[2], c.CB[2].w);
				if (c.Col == 0) mx[t.S.Rtl ? 1 : 3] = Math.Max(mx[t.S.Rtl ? 1 : 3], c.CB[t.S.Rtl ? 1 : 3].w);
				if (c.Col + c.ColSpan >= g.NCols) mx[t.S.Rtl ? 3 : 1] = Math.Max(mx[t.S.Rtl ? 3 : 1], c.CB[t.S.Rtl ? 3 : 1].w);
			}
			if (g.Cells.Count == 0) { for (int i = 0; i < 4; i++) mx[i] = t.S.BorderWidth[i] * 2; }
			for (int i = 0; i < 4; i++) t.Bd[i] = mx[i] / 2;
			t.P[0] = t.P[1] = t.P[2] = t.P[3] = 0;
		}

		void ResolveCollapsed(Box t, TGrid g) {
			bool rtl = t.S.Rtl;
			int L = rtl ? 1 : 3, R = rtl ? 3 : 1;
			foreach (var c in g.Cells) {
				c.CB = new (double, BS, Rgba)[4];
				var row = g.Rows[c.Row];
				var grp = g.RowGroup[c.Row];
				int lastRow = Math.Min(g.Rows.Count - 1, c.Row + c.RowSpan - 1);
				var top = Border(c.S, 0);
				top = Win(top, Border(row.S, 0));
				if (c.Row == 0 || g.RowGroup[c.Row - 1] != grp) top = Win(top, Border(grp.S, 0));
				if (c.Row > 0) {
					for (int cc = c.Col; cc < c.Col + c.ColSpan && cc < g.NCols; cc++) {
						var above = g.At[c.Row - 1, cc];
						if (above != null && above != c) top = Win(top, Border(above.S, 2));
					}
					top = Win(top, Border(g.Rows[c.Row - 1].S, 2));
					if (g.RowGroup[c.Row - 1] != grp) top = Win(top, Border(g.RowGroup[c.Row - 1].S, 2));
				}
				else top = Win(top, Border(t.S, 0));
				var bot = Border(c.S, 2);
				bot = Win(bot, Border(g.Rows[lastRow].S, 2));
				if (lastRow == g.Rows.Count - 1 || g.RowGroup[lastRow + 1] != g.RowGroup[lastRow]) bot = Win(bot, Border(g.RowGroup[lastRow].S, 2));
				if (lastRow + 1 < g.Rows.Count) {
					for (int cc = c.Col; cc < c.Col + c.ColSpan && cc < g.NCols; cc++) {
						var below = g.At[lastRow + 1, cc];
						if (below != null && below != c) bot = Win(bot, Border(below.S, 0));
					}
					bot = Win(bot, Border(g.Rows[lastRow + 1].S, 0));
				}
				else bot = Win(bot, Border(t.S, 2));
				var left = Border(c.S, L);
				if (c.Col == 0) { left = Win(left, Border(t.S, L)); left = Win(left, Border(row.S, L)); left = Win(left, Border(grp.S, L)); }
				else {
					var prev = g.At[c.Row, c.Col - 1];
					if (prev != null && prev != c) left = Win(left, Border(prev.S, R));
				}
				if (c.Col < g.Cols.Count) left = Win(left, Border(g.Cols[c.Col].S, L));
				var right = Border(c.S, R);
				int lastCol = c.Col + c.ColSpan - 1;
				if (lastCol >= g.NCols - 1) { right = Win(right, Border(t.S, R)); right = Win(right, Border(row.S, R)); right = Win(right, Border(grp.S, R)); }
				else {
					var next = g.At[c.Row, lastCol + 1];
					if (next != null && next != c) right = Win(right, Border(next.S, L));
				}
				if (lastCol < g.Cols.Count) right = Win(right, Border(g.Cols[lastCol].S, R));
				c.CB[0] = top; c.CB[2] = bot; c.CB[L] = left; c.CB[R] = right;
				for (int i = 0; i < 4; i++) if (c.CB[i].s == BS.Hidden || c.CB[i].s == BS.None) c.CB[i] = (0, BS.None, c.CB[i].c);
			}
		}
	}

	internal sealed class FlexItem {
		public Box B;
		public double Base, Hyp, Target, MinMain, MaxMain;
		public double MainMargins, CrossMargins, MainBP, CrossBP;
		public bool Frozen;
		public double Cross, MainPos;
		public double Baseline;
		public bool AutoMainStart, AutoMainEnd, AutoCrossStart, AutoCrossEnd;
		public double OuterBase => Base + MainMargins + MainBP;
		public double OuterTarget => Target + MainMargins + MainBP;
	}

	internal sealed class GridTrack {
		public byte MinK, MaxK;
		public double MinV, MaxV;
		public double Base, Limit;
		public double Pos, Size;
		public const byte Fixed = 0, Pct = 1, Fr = 2, Auto = 3, MinC = 4, MaxC = 5, FitC = 6;
	}

	internal sealed partial class LayoutEngine {
		double Gap(Len g, double basis) => g.IsValue ? g.Resolve(basis) : 0;

		public void FlexIntrinsic(Box b, out double min, out double max) {
			var items = b.Kids.Where(k => !k.IsOutOfFlow).ToList();
			bool row = b.S.FlexDirection < 2;
			min = 0; max = 0;
			double gap = Gap(row ? b.S.ColumnGap : b.S.RowGap, 0);
			foreach (var k in items) {
				IntrinsicOuter(k, out double mn, out double mx);
				if (row) {
					max += mx;
					if (b.S.FlexWrap == 0) min += k.S.FlexShrink > 0 ? mn : mx; else min = Math.Max(min, mn);
				}
				else { max = Math.Max(max, mx); min = Math.Max(min, mn); }
			}
			if (row && items.Count > 1) { max += gap * (items.Count - 1); if (b.S.FlexWrap == 0) min += gap * (items.Count - 1); }
		}

		double MainContentSize(Box k, bool row, double crossAvail) {
			if (row) {
				if (k.IsReplaced) { ReplacedSize(k, crossAvail, double.NaN, out double rw, out _); return rw; }
				ContentIntrinsic(k, out _, out double mx);
				return mx;
			}
			bool pg = Paginate;
			Paginate = false;
			try {
				var saveW = k.ForceW;
				LayoutBlockBox(k, 0, crossAvail, 0, null, double.NaN, out _);
				return k.H - BPV(k);
			}
			finally { Paginate = pg; }
		}

		double MinContentMain(Box k, bool row, double crossAvail) {
			if (row) {
				if (k.IsReplaced) { ReplacedSize(k, crossAvail, double.NaN, out double rw, out _); return rw; }
				ContentIntrinsic(k, out double mn, out _);
				return mn;
			}
			return MainContentSize(k, row, crossAvail);
		}

		public double LayoutFlex(Box b, double cx, double cw, double cy, double? specH, double cbH) {
			var s = b.S;
			bool row = s.FlexDirection < 2;
			bool reverse = s.FlexDirection == 1 || s.FlexDirection == 3;
			bool rtl = s.Rtl;
			double mainGap = Gap(row ? s.ColumnGap : s.RowGap, row ? cw : specH ?? 0);
			double crossGap = Gap(row ? s.RowGap : s.ColumnGap, row ? specH ?? 0 : cw);
			double? mainSize = row ? cw : specH;
			var kids = b.Kids.Where(k => !k.IsOutOfFlow).OrderBy(k => k.S.Order).ToList();
			foreach (var k in b.Kids.Where(k => k.IsOutOfFlow)) {
				k.StaticX = rtl ? cx + cw : cx; k.StaticY = cy; k.StaticRtl = rtl;
				RegisterAbs(k);
			}
			var items = new List<FlexItem>();
			foreach (var k in kids) {
				ResolveBoxModel(k, cw);
				var ks = k.S;
				var it = new FlexItem { B = k };
				it.MainMargins = row ? k.M[1] + k.M[3] : k.M[0] + k.M[2];
				it.CrossMargins = row ? k.M[0] + k.M[2] : k.M[1] + k.M[3];
				it.MainBP = row ? BP(k) : BPV(k);
				it.CrossBP = row ? BPV(k) : BP(k);
				it.AutoMainStart = row ? ks.Margin[rtl ? 1 : 3].IsAuto : ks.Margin[0].IsAuto;
				it.AutoMainEnd = row ? ks.Margin[rtl ? 3 : 1].IsAuto : ks.Margin[2].IsAuto;
				it.AutoCrossStart = row ? ks.Margin[0].IsAuto : ks.Margin[rtl ? 1 : 3].IsAuto;
				it.AutoCrossEnd = row ? ks.Margin[2].IsAuto : ks.Margin[rtl ? 3 : 1].IsAuto;
				double crossAvail = row ? double.NaN : cw - it.CrossMargins;
				Len mainProp = row ? ks.Width : ks.Height;
				double mainBasis = row ? cw : mainSize ?? double.NaN;
				double adj = ks.BorderBox ? it.MainBP : 0;
				double? basis = null;
				var fb = ks.FlexBasis;
				if (fb.IsValue && fb.TryResolve(mainBasis) is double fbv) basis = fbv - adj;
				else if (fb.IsAuto && mainProp.IsValue && mainProp.TryResolve(mainBasis) is double mpv) basis = mpv - adj;
				if (basis == null && k.IsReplaced) {
					ReplacedSize(k, row ? cw : cw - it.CrossMargins, cbH, out double rw, out double rh);
					if (row && ks.Height.IsValue && ks.Height.TryResolve(specH ?? double.NaN) is double hh && k.IntrW > 0 && k.IntrH > 0) basis = rw;
					else basis = row ? rw : rh;
				}
				if (basis == null) {
					if (!row) {
						double w = ks.Width.IsValue && ks.Width.TryResolve(cw) is double ww ? ww - (ks.BorderBox ? it.CrossBP : 0) : cw - it.CrossMargins - it.CrossBP;
						if (ks.Width.IsAuto && (ks.AlignSelf != Style.AlAuto ? ks.AlignSelf : s.AlignItems) is byte al && al != Style.AlStretch && al != Style.AlNormal) {
							ContentIntrinsic(k, out double mn2, out double mx2);
							w = Math.Min(Math.Max(mn2, w), mx2);
						}
						w = ClampW(k, w, cw);
						k.ForceW = w;
						basis = MainContentSize(k, false, w + it.CrossBP + it.CrossMargins);
						k.ForceW = double.NaN;
					}
					else basis = MainContentSize(k, true, cw);
				}
				it.Base = Math.Max(0, basis.Value);
				double minMain = 0, maxMain = double.PositiveInfinity;
				Len minP = row ? ks.MinWidth : ks.MinHeight, maxP = row ? ks.MaxWidth : ks.MaxHeight;
				if (minP.IsValue) minMain = (minP.TryResolve(mainBasis) ?? 0) - adj;
				else if (minP.IsAuto && ks.OverflowX == 0 && ks.OverflowY == 0) {
					double contentMin = row ? MinContentMain(k, true, cw) : it.Base;
					if (k.IsReplaced) contentMin = row ? Math.Min(contentMin, it.Base) : it.Base;
					if (mainProp.IsValue && mainProp.TryResolve(mainBasis) is double spec) contentMin = Math.Min(contentMin, spec - adj);
					if (maxP.IsValue && maxP.TryResolve(mainBasis) is double mxv) contentMin = Math.Min(contentMin, mxv - adj);
					minMain = contentMin;
				}
				if (maxP.IsValue && maxP.TryResolve(mainBasis) is double mx3) maxMain = mx3 - adj;
				it.MinMain = Math.Max(0, minMain);
				it.MaxMain = Math.Max(it.MinMain, maxMain);
				it.Hyp = Math.Min(Math.Max(it.Base, it.MinMain), it.MaxMain);
				items.Add(it);
			}
			double avail = mainSize ?? double.PositiveInfinity;
			if (!row && mainSize == null) {
				if (s.MaxHeight.IsValue && s.MaxHeight.TryResolve(cbH) is double mh2) avail = mh2 - (s.BorderBox ? BPV(b) : 0);
			}
			var lines = new List<List<FlexItem>>();
			if (s.FlexWrap == 0 || double.IsInfinity(avail)) lines.Add(items);
			else {
				var cur = new List<FlexItem>();
				double used = 0;
				foreach (var it in items) {
					double o = it.Hyp + it.MainMargins + it.MainBP;
					if (cur.Count > 0 && used + mainGap + o > avail + 0.001) { lines.Add(cur); cur = new List<FlexItem>(); used = 0; }
					used += (cur.Count > 0 ? mainGap : 0) + o;
					cur.Add(it);
				}
				if (cur.Count > 0) lines.Add(cur);
			}
			if (s.FlexWrap == 2) lines.Reverse();
			double containerMain = mainSize ?? 0;
			if (mainSize == null) {
				foreach (var ln in lines) {
					double sum = ln.Sum(i => i.Hyp + i.MainMargins + i.MainBP) + mainGap * Math.Max(0, ln.Count - 1);
					containerMain = Math.Max(containerMain, sum);
				}
				if (!row) containerMain = ClampH(b, containerMain, cbH);
			}
			foreach (var ln in lines) ResolveFlexible(ln, containerMain, mainGap);
			double crossPos = row ? cy : cx;
			var lineCross = new double[lines.Count];
			var lineBase = new double[lines.Count];
			double y0 = cy;
			double lineStart = row ? cy : 0;
			for (int li = 0; li < lines.Count; li++) {
				var ln = lines[li];
				double maxCross = 0, maxAbove = 0, maxBelow = 0;
				foreach (var it in ln) {
					var k = it.B;
					if (row) {
						k.ForceW = it.Target;
						LayoutBlockBox(k, cx, cw, lineStart + k.M[0], null, specH ?? double.NaN, out _);
						k.ForceW = double.NaN;
						it.Cross = k.H;
					}
					else {
						double w;
						byte al = k.S.AlignSelf != Style.AlAuto ? k.S.AlignSelf : s.AlignItems;
						if (k.S.Width.IsValue && k.S.Width.TryResolve(cw) is double ww) w = ww - (k.S.BorderBox ? it.CrossBP : 0);
						else if (k.IsReplaced) { ReplacedSize(k, cw, double.NaN, out w, out _); }
						else if (al == Style.AlStretch || al == Style.AlNormal) w = cw - it.CrossMargins - it.CrossBP;
						else { ContentIntrinsic(k, out double mn, out double mx); w = Math.Min(Math.Max(mn, cw - it.CrossMargins - it.CrossBP), mx); }
						w = ClampW(k, w, cw);
						it.Cross = w + it.CrossBP;
					}
					double bl = row ? (double.IsNaN(k.Baseline) ? k.H : k.Baseline - k.Y) + k.M[0] : 0;
					it.Baseline = bl;
					byte a2 = k.S.AlignSelf != Style.AlAuto ? k.S.AlignSelf : s.AlignItems;
					if (row && (a2 == Style.AlBaseline || a2 == Style.AlLastBaseline) && !it.AutoCrossStart && !it.AutoCrossEnd) {
						maxAbove = Math.Max(maxAbove, bl);
						maxBelow = Math.Max(maxBelow, it.Cross + it.CrossMargins - bl);
					}
					else maxCross = Math.Max(maxCross, it.Cross + it.CrossMargins);
				}
				lineCross[li] = Math.Max(maxCross, maxAbove + maxBelow);
				lineBase[li] = maxAbove;
				if (row) lineStart += lineCross[li] + crossGap;
			}
			double? crossDef = row ? specH : (double?)cw;
			if (lines.Count == 1 && crossDef != null) lineCross[0] = crossDef.Value;
			if (row && lines.Count == 1 && crossDef == null) {
				double minH = s.MinHeight.IsValue ? (s.MinHeight.TryResolve(cbH) ?? 0) - (s.BorderBox ? BPV(b) : 0) : 0;
				if (lineCross[0] < minH) lineCross[0] = minH;
				double maxH = s.MaxHeight.IsValue && s.MaxHeight.TryResolve(cbH) is double mhv ? mhv - (s.BorderBox ? BPV(b) : 0) : double.PositiveInfinity;
				if (lineCross[0] > maxH) lineCross[0] = maxH;
			}
			double totalCross = lineCross.Sum() + crossGap * Math.Max(0, lines.Count - 1);
			double containerCross = crossDef ?? totalCross;
			var lineOffset = new double[lines.Count];
			{
				double free = containerCross - totalCross;
				double start = 0, between = crossGap;
				byte ac = s.AlignContent;
				if (lines.Count > 0 && crossDef != null && s.FlexWrap != 0) {
					switch (ac) {
						case Style.AlFlexEnd: case Style.AlEnd: start = free; break;
						case Style.AlCenter: start = free / 2; break;
						case Style.AlSpaceBetween: if (lines.Count > 1 && free > 0) between += free / (lines.Count - 1); break;
						case Style.AlSpaceAround: if (free > 0) { start = free / lines.Count / 2; between += free / lines.Count; } break;
						case Style.AlSpaceEvenly: if (free > 0) { start = free / (lines.Count + 1); between += free / (lines.Count + 1); } break;
						case Style.AlNormal: case Style.AlStretch: if (free > 0) { double add = free / lines.Count; for (int i = 0; i < lines.Count; i++) lineCross[i] += add; } break;
					}
				}
				double p = start;
				for (int i = 0; i < lines.Count; i++) { lineOffset[i] = p; p += lineCross[i] + between; }
			}
			double pushAcc = 0;
			for (int li = 0; li < lines.Count; li++) {
				lineOffset[li] += pushAcc;
				bool relayoutAll = false;
			retryLine:
				var ln = lines[li];
				double lc = lineCross[li];
				double used = ln.Sum(i => i.OuterTarget) + mainGap * Math.Max(0, ln.Count - 1);
				double free = containerMain - used;
				int autoMargins = ln.Sum(i => (i.AutoMainStart ? 1 : 0) + (i.AutoMainEnd ? 1 : 0));
				double start = 0, between = mainGap;
				double perAuto = 0;
				if (autoMargins > 0 && free > 0) perAuto = free / autoMargins;
				else {
					byte jc = s.JustifyContent;
					switch (jc) {
						case Style.AlFlexEnd: start = free; break;
						case Style.AlEnd: start = free; break;
						case Style.AlCenter: start = free / 2; break;
						case Style.AlSpaceBetween: if (ln.Count > 1 && free > 0) between += free / (ln.Count - 1); break;
						case Style.AlSpaceAround: if (free > 0) { start = free / ln.Count / 2; between += free / ln.Count; } else start = free / 2; break;
						case Style.AlSpaceEvenly: if (free > 0) { start = free / (ln.Count + 1); between += free / (ln.Count + 1); } else start = free / 2; break;
						case Style.AlLeft: start = row && (rtl != reverse) ? free : 0; break;
						case Style.AlRight: start = row && (rtl != reverse) ? 0 : free; break;
						case Style.AlStart: start = reverse ? free : 0; break;
					}
				}
				double pos = start;
				var order = ln;
				foreach (var it in order) {
					if (it.AutoMainStart) pos += perAuto;
					it.MainPos = pos + (row ? (rtl ? it.B.M[1] : it.B.M[3]) : it.B.M[0]);
					pos += it.OuterTarget;
					if (it.AutoMainEnd) pos += perAuto;
					pos += between;
				}
				foreach (var it in ln) {
					var k = it.B;
					byte al = k.S.AlignSelf != Style.AlAuto ? k.S.AlignSelf : s.AlignItems;
					double crossSize = it.Cross;
					Len crossProp = row ? k.S.Height : k.S.Width;
					bool stretch = (al == Style.AlStretch || al == Style.AlNormal) && crossProp.IsAuto && !it.AutoCrossStart && !it.AutoCrossEnd && !(k.IsReplaced && row && k.S.Width.IsValue && k.IntrW > 0 && k.IntrH > 0 && false);
					if (stretch) {
						double target = lc - it.CrossMargins;
						if (row) target = ClampH(k, target - it.CrossBP, cbH) + it.CrossBP;
						else target = ClampW(k, target - it.CrossBP, cw) + it.CrossBP;
						crossSize = Math.Max(target, row && k.IsReplaced ? 0 : 0);
					}
					double freeC = lc - crossSize - it.CrossMargins;
					double off = 0;
					if (it.AutoCrossStart && it.AutoCrossEnd) off = Math.Max(0, freeC) / 2;
					else if (it.AutoCrossStart) off = Math.Max(0, freeC);
					else if (it.AutoCrossEnd) off = 0;
					else switch (al) {
						case Style.AlFlexEnd: case Style.AlEnd: case Style.AlSelfEnd: off = freeC; break;
						case Style.AlCenter: off = freeC / 2; break;
						case Style.AlBaseline: case Style.AlLastBaseline: off = row ? lineBase[li] - it.Baseline : 0; break;
						default: off = 0; break;
					}
					if (s.FlexWrap == 2) off = freeC - off;
					double crossStart = lineOffset[li] + off + (row ? k.M[0] : (rtl ? k.M[1] : k.M[3]));
					if (row) {
						double xMain = rtl != reverse ? cx + containerMain - it.MainPos - it.Target - it.MainBP : cx + it.MainPos;
						if (reverse && !rtl || rtl && !reverse) xMain = cx + containerMain - it.MainPos - it.Target - it.MainBP;
						double yTarget = cy + crossStart;
						if (stretch && Math.Abs(crossSize - k.H) > 0.01) {
							k.ForceW = it.Target;
							k.ForceH = crossSize - it.CrossBP;
							LayoutBlockBox(k, xMain - k.M[3], it.Target + it.MainBP + it.MainMargins, yTarget, null, specH ?? double.NaN, out _);
							k.ForceW = double.NaN; k.ForceH = double.NaN;
						}
						else if (relayoutAll || Math.Abs(k.Y - yTarget) > 0.001 && Paginate && Crosses(yTarget, k.H) && !Crosses(k.Y, k.H)) {
							k.ForceW = it.Target;
							LayoutBlockBox(k, xMain - k.M[3], it.Target + it.MainBP + it.MainMargins, yTarget, null, specH ?? double.NaN, out _);
							k.ForceW = double.NaN;
						}
						Translate(k, xMain - k.X, yTarget - k.Y);
					}
					else {
						double w = crossSize - it.CrossBP;
						double yMain = reverse ? cy + containerMain - it.MainPos - it.Target - it.MainBP : cy + it.MainPos;
						double xCross = rtl ? cx + cw - crossStart - crossSize : cx + crossStart;
						k.ForceW = w;
						k.ForceH = it.Target;
						LayoutBlockBox(k, xCross - k.M[3], crossSize + it.CrossMargins, yMain, null, specH ?? double.NaN, out _);
						k.ForceW = double.NaN; k.ForceH = double.NaN;
						Translate(k, xCross - k.X, 0);
					}
				}
				if (row && Paginate && !relayoutAll) {
					double lt = cy + lineOffset[li];
					if (!AtPageTop(lt) && lc <= PageH - Reserve - ReserveBottom) {
						double lim = PageLimit(lt);
						if (ln.Any(it => Crosses(it.B.Y, it.B.H) && FirstContentTop(it.B) is double f && !double.IsInfinity(f) && f >= lim - 1e-6)) {
							double d = NextPageTop(lt) - lt;
							lineOffset[li] += d;
							pushAcc += d;
							relayoutAll = true;
							goto retryLine;
						}
					}
				}
			}
			double baseline = double.NaN;
			if (lines.Count > 0 && lines[0].Count > 0) {
				var firstBl = lines[0].FirstOrDefault(i => (i.B.S.AlignSelf != Style.AlAuto ? i.B.S.AlignSelf : s.AlignItems) == Style.AlBaseline) ?? lines[0][0];
				double bl = FirstBaselineOf(firstBl.B);
				baseline = double.IsNaN(bl) ? firstBl.B.Y + firstBl.B.H : bl;
			}
			b.Baseline = baseline;
			if (row) return specH ?? totalCross + pushAcc;
			return specH ?? containerMain;
		}

		void ResolveFlexible(List<FlexItem> ln, double main, double gap) {
			double sumHyp = ln.Sum(i => i.Hyp + i.MainMargins + i.MainBP) + gap * Math.Max(0, ln.Count - 1);
			bool grow = sumHyp < main;
			foreach (var it in ln) {
				it.Frozen = false;
				it.Target = it.Base;
				double factor = grow ? it.B.S.FlexGrow : it.B.S.FlexShrink;
				if (factor == 0 || grow && it.Base > it.Hyp || !grow && it.Base < it.Hyp) { it.Frozen = true; it.Target = it.Hyp; }
			}
			double gaps = gap * Math.Max(0, ln.Count - 1);
			double initialFree = main - gaps - ln.Sum(i => i.Frozen ? i.Target + i.MainMargins + i.MainBP : i.Base + i.MainMargins + i.MainBP);
			for (int guard = 0; guard < 50; guard++) {
				var active = ln.Where(i => !i.Frozen).ToList();
				if (active.Count == 0) break;
				double free = main - gaps - ln.Sum(i => i.Frozen ? i.Target + i.MainMargins + i.MainBP : i.Base + i.MainMargins + i.MainBP);
				double sumF = active.Sum(i => grow ? i.B.S.FlexGrow : i.B.S.FlexShrink);
				if (sumF < 1) { double lim = initialFree * sumF; if (Math.Abs(lim) < Math.Abs(free)) free = lim; }
				if (grow) {
					foreach (var it in active) it.Target = it.Base + (sumF > 0 ? free * it.B.S.FlexGrow / sumF : 0);
				}
				else {
					double sumScaled = active.Sum(i => i.B.S.FlexShrink * i.Base);
					foreach (var it in active) it.Target = it.Base + (sumScaled > 0 ? free * it.B.S.FlexShrink * it.Base / sumScaled : 0);
				}
				double totalViolation = 0;
				foreach (var it in active) {
					double clamped = Math.Min(Math.Max(it.Target, it.MinMain), it.MaxMain);
					clamped = Math.Max(0, clamped);
					totalViolation += clamped - it.Target;
					it.Hyp = clamped;
				}
				if (Math.Abs(totalViolation) < 1e-6) { foreach (var it in active) { it.Target = it.Hyp; it.Frozen = true; } break; }
				foreach (var it in active) {
					bool minV = it.Hyp > it.Target + 1e-9, maxV = it.Hyp < it.Target - 1e-9;
					if (totalViolation > 0 && minV || totalViolation < 0 && maxV) { it.Target = it.Hyp; it.Frozen = true; }
				}
			}
			foreach (var it in ln) it.Target = Math.Max(0, it.Target);
		}

		List<GridTrack> ParseTracks(string v, double avail, double gap, out Dictionary<string, List<int>> lineNames) {
			lineNames = new Dictionary<string, List<int>>();
			var res = new List<GridTrack>();
			if (string.IsNullOrWhiteSpace(v)) return res;
			var toks = TokenizeTracks(v);
			ExpandTracks(toks, res, avail, gap, lineNames);
			return res;
		}

		static List<string> TokenizeTracks(string v) {
			var res = new List<string>();
			int i = 0;
			while (i < v.Length) {
				if (char.IsWhiteSpace(v[i])) { i++; continue; }
				if (v[i] == '[') { int e = v.IndexOf(']', i); if (e < 0) e = v.Length - 1; res.Add(v.Substring(i, e - i + 1)); i = e + 1; continue; }
				int st = i, depth = 0;
				while (i < v.Length && (depth > 0 || !char.IsWhiteSpace(v[i]) && v[i] != '[')) { if (v[i] == '(') depth++; else if (v[i] == ')') depth--; i++; }
				res.Add(v.Substring(st, i - st));
			}
			return res;
		}

		void ExpandTracks(List<string> toks, List<GridTrack> res, double avail, double gap, Dictionary<string, List<int>> names) {
			foreach (var t0 in toks) {
				string t = t0.Trim();
				if (t.StartsWith("[")) {
					foreach (var nm in t.Trim('[', ']').Split(' ', StringSplitOptions.RemoveEmptyEntries)) { if (!names.TryGetValue(nm, out var l)) names[nm] = l = new List<int>(); l.Add(res.Count + 1); }
					continue;
				}
				if (t.StartsWith("repeat(", StringComparison.OrdinalIgnoreCase)) {
					string inner = t.Substring(7, t.Length - 8);
					int comma = inner.IndexOf(',');
					string cnt = inner.Substring(0, comma).Trim().ToLowerInvariant();
					var sub = TokenizeTracks(inner.Substring(comma + 1));
					int n;
					if (cnt == "auto-fill" || cnt == "auto-fit") {
						var tmp = new List<GridTrack>();
						ExpandTracks(sub, tmp, avail, gap, new Dictionary<string, List<int>>());
						double one = tmp.Sum(x => x.MaxK == GridTrack.Fixed ? x.MaxV : x.MinK == GridTrack.Fixed ? x.MinV : x.MaxK == GridTrack.Pct ? x.MaxV * avail / 100 : x.MinK == GridTrack.Pct ? x.MinV * avail / 100 : 0) + gap * tmp.Count;
						n = one > 0 && !double.IsInfinity(avail) && !double.IsNaN(avail) ? Math.Max(1, (int)Math.Floor((avail + gap) / one)) : 1;
					}
					else n = int.TryParse(cnt, out int c) ? Math.Max(1, c) : 1;
					for (int k = 0; k < n; k++) ExpandTracks(sub, res, avail, gap, names);
					continue;
				}
				res.Add(ParseTrack(t));
			}
		}

		GridTrack ParseTrack(string t) {
			var tr = new GridTrack();
			string lt = t.ToLowerInvariant();
			if (lt.StartsWith("minmax(")) {
				var parts = Css.SplitTopLevel(lt.Substring(7, lt.Length - 8), ',');
				var a = ParseTrack(parts[0].Trim());
				var b = ParseTrack(parts.Count > 1 ? parts[1].Trim() : "auto");
				tr.MinK = a.MinK == GridTrack.Fr ? GridTrack.Auto : a.MinK; tr.MinV = a.MinV;
				tr.MaxK = b.MaxK; tr.MaxV = b.MaxV;
				return tr;
			}
			if (lt.StartsWith("fit-content(")) {
				var l = Val.ParseLen(lt.Substring(12, lt.Length - 13), new LenCtx { Em = 16, Rem = 16 });
				tr.MinK = GridTrack.Auto; tr.MaxK = GridTrack.FitC;
				tr.MaxV = l?.Px ?? 0;
				return tr;
			}
			if (lt.EndsWith("fr") && Val.Num(lt[..^2], out double fr)) { tr.MinK = GridTrack.Auto; tr.MaxK = GridTrack.Fr; tr.MaxV = fr; return tr; }
			if (lt == "auto") { tr.MinK = tr.MaxK = GridTrack.Auto; return tr; }
			if (lt == "min-content") { tr.MinK = tr.MaxK = GridTrack.MinC; return tr; }
			if (lt == "max-content") { tr.MinK = tr.MaxK = GridTrack.MaxC; return tr; }
			var len = Val.ParseLen(lt, new LenCtx { Em = 16, Rem = 16 });
			if (len != null && len.Value.IsValue) {
				if (len.Value.HasPct && len.Value.Fn == null && len.Value.Px == 0) { tr.MinK = tr.MaxK = GridTrack.Pct; tr.MinV = tr.MaxV = len.Value.Pct; }
				else { tr.MinK = tr.MaxK = GridTrack.Fixed; tr.MinV = tr.MaxV = len.Value.Px; }
				return tr;
			}
			tr.MinK = tr.MaxK = GridTrack.Auto;
			return tr;
		}

		sealed class GItem { public Box B; public int C0, C1, R0, R1; }

		static (int s, int e) Area(Dictionary<string, (int r0, int c0, int r1, int c1)> areas, string name, bool col) {
			if (areas.TryGetValue(name, out var a)) return col ? (a.c0, a.c1) : (a.r0, a.r1);
			return (-1, -1);
		}

		(int start, int span, bool auto) Placement(string startS, string endS, int nLines, Dictionary<string, List<int>> names, Dictionary<string, (int r0, int c0, int r1, int c1)> areas, bool col) {
			int Line(string v, bool isEnd, out int span, out bool isAuto) {
				span = 0; isAuto = false;
				if (string.IsNullOrWhiteSpace(v) || v.Trim().Equals("auto", StringComparison.OrdinalIgnoreCase)) { isAuto = true; return 0; }
				var toks = v.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
				if (toks[0].Equals("span", StringComparison.OrdinalIgnoreCase)) {
					span = toks.Count > 1 && int.TryParse(toks[1], out int sp) ? Math.Max(1, sp) : 1;
					isAuto = true;
					return 0;
				}
				int num = 0; string name = null;
				foreach (var t in toks) { if (int.TryParse(t, out int n2)) num = n2; else name = t; }
				if (name != null) {
					var ar = Area(areas, name, col);
					if (ar.s >= 0 && num == 0) return isEnd ? ar.e : ar.s;
					if (areas.ContainsKey(name.Replace("-start", "").Replace("-end", ""))) {
						var ar2 = Area(areas, name.Replace("-start", "").Replace("-end", ""), col);
						return name.EndsWith("-end") ? ar2.e : ar2.s;
					}
					if (names.TryGetValue(name, out var lines) && lines.Count > 0) return lines[Math.Clamp((num == 0 ? 1 : num) - 1, 0, lines.Count - 1)];
					isAuto = true;
					return 0;
				}
				if (num < 0) return nLines + 1 + num;
				return num == 0 ? 1 : num;
			}
			int s0 = Line(startS, false, out int sspan, out bool sa);
			int e0 = Line(endS, true, out int espan, out bool ea);
			if (!sa && !ea) { if (e0 < s0) (s0, e0) = (e0, s0); if (e0 == s0) e0 = s0 + 1; return (s0, e0 - s0, false); }
			if (!sa) return (s0, Math.Max(1, espan), false);
			if (!ea) { int span = Math.Max(1, sspan); return (Math.Max(1, e0 - span), span, false); }
			return (0, Math.Max(1, Math.Max(sspan, espan)), true);
		}

		public void GridIntrinsic(Box b, out double min, out double max) {
			var cols = ParseTracks(b.S.GridTemplateColumns, double.NaN, 0, out _);
			var kids = b.Kids.Where(k => !k.IsOutOfFlow).ToList();
			double gap = Gap(b.S.ColumnGap, 0);
			if (cols.Count == 0) {
				min = max = 0;
				foreach (var k in kids) { IntrinsicOuter(k, out double mn, out double mx); min = Math.Max(min, mn); max = Math.Max(max, mx); }
				return;
			}
			int n = cols.Count;
			var cmin = new double[n]; var cmax = new double[n];
			int idx = 0;
			foreach (var k in kids) {
				IntrinsicOuter(k, out double mn, out double mx);
				int c = idx % n;
				cmin[c] = Math.Max(cmin[c], mn); cmax[c] = Math.Max(cmax[c], mx);
				idx++;
			}
			min = 0; max = 0;
			for (int c = 0; c < n; c++) {
				var t = cols[c];
				if (t.MaxK == GridTrack.Fixed && t.MinK == GridTrack.Fixed) { min += t.MinV; max += t.MaxV; }
				else { min += cmin[c]; max += cmax[c]; }
			}
			min += gap * (n - 1); max += gap * (n - 1);
		}

		public double LayoutGrid(Box b, double cx, double cw, double cy, double? specH, double cbH) {
			var s = b.S;
			bool rtl = s.Rtl;
			double colGap = Gap(s.ColumnGap, cw), rowGap = Gap(s.RowGap, specH ?? 0);
			var cols = ParseTracks(s.GridTemplateColumns, cw, colGap, out var colNames);
			var rows = ParseTracks(s.GridTemplateRows, specH ?? double.NaN, rowGap, out var rowNames);
			var areas = new Dictionary<string, (int r0, int c0, int r1, int c1)>();
			if (s.GridTemplateAreas != null) {
				var rowsStr = new List<string>();
				var sv = s.GridTemplateAreas;
				int i = 0;
				while (i < sv.Length) {
					if (sv[i] == '"' || sv[i] == '\'') { int e = sv.IndexOf(sv[i], i + 1); if (e < 0) e = sv.Length; rowsStr.Add(sv.Substring(i + 1, e - i - 1)); i = e + 1; }
					else i++;
				}
				for (int r = 0; r < rowsStr.Count; r++) {
					var cells = rowsStr[r].Split(' ', StringSplitOptions.RemoveEmptyEntries);
					for (int c = 0; c < cells.Length; c++) {
						string nm = cells[c];
						if (nm.All(ch => ch == '.')) continue;
						if (areas.TryGetValue(nm, out var a)) areas[nm] = (Math.Min(a.r0, r + 1), Math.Min(a.c0, c + 1), Math.Max(a.r1, r + 2), Math.Max(a.c1, c + 2));
						else areas[nm] = (r + 1, c + 1, r + 2, c + 2);
					}
					while (cols.Count < cells.Length) cols.Add(ParseTrack(s.GridAutoColumns ?? "auto"));
				}
				while (rows.Count < rowsStr.Count) rows.Add(ParseTrack(s.GridAutoRows ?? "auto"));
			}
			int explicitCols = Math.Max(1, cols.Count), explicitRows = rows.Count;
			if (cols.Count == 0) cols.Add(ParseTrack(s.GridAutoColumns ?? "auto"));
			var kids = b.Kids.Where(k => !k.IsOutOfFlow).OrderBy(k => k.S.Order).ToList();
			foreach (var k in b.Kids.Where(k => k.IsOutOfFlow)) { k.StaticX = rtl ? cx + cw : cx; k.StaticY = cy; k.StaticRtl = rtl; RegisterAbs(k); }
			var gitems = new List<GItem>();
			var occupied = new HashSet<(int, int)>();
			bool colFlow = (s.GridAutoFlow & 1) != 0;
			bool dense = (s.GridAutoFlow & 2) != 0;
			var pending = new List<(GItem gi, (int, int, bool) cp, (int, int, bool) rp)>();
			foreach (var k in kids) {
				var ks = k.S;
				string cs = ks.GridColumnStart, ce = ks.GridColumnEnd, rs = ks.GridRowStart, re = ks.GridRowEnd;
				var cp = Placement(cs, ce, explicitCols + 1, colNames, areas, true);
				var rp = Placement(rs, re, explicitRows + 1, rowNames, areas, false);
				var gi = new GItem { B = k };
				pending.Add((gi, cp, rp));
				gitems.Add(gi);
			}
			void Occupy(GItem gi) { for (int r = gi.R0; r < gi.R1; r++) for (int c = gi.C0; c < gi.C1; c++) occupied.Add((r, c)); }
			bool Free(int r0, int r1, int c0, int c1) { for (int r = r0; r < r1; r++) for (int c = c0; c < c1; c++) if (occupied.Contains((r, c))) return false; return true; }
			foreach (var (gi, cp, rp) in pending) {
				if (!cp.Item3 && !rp.Item3) { gi.C0 = cp.Item1; gi.C1 = cp.Item1 + cp.Item2; gi.R0 = rp.Item1; gi.R1 = rp.Item1 + rp.Item2; Occupy(gi); }
			}
			int cursorR = 1, cursorC = 1;
			int ncols = cols.Count;
			foreach (var (gi, cp, rp) in pending) {
				if (!cp.Item3 && !rp.Item3) continue;
				if (dense) { cursorR = 1; cursorC = 1; }
				if (!colFlow) {
					if (!rp.Item3) {
						int r0 = rp.Item1;
						int c = 1;
						while (!Free(r0, r0 + rp.Item2, c, c + cp.Item2)) c++;
						gi.R0 = r0; gi.R1 = r0 + rp.Item2; gi.C0 = cp.Item3 ? c : cp.Item1; gi.C1 = gi.C0 + cp.Item2;
					}
					else if (!cp.Item3) {
						int c0 = cp.Item1;
						int r = cursorR;
						if (c0 < cursorC && !dense) r++;
						while (!Free(r, r + rp.Item2, c0, c0 + cp.Item2)) r++;
						gi.C0 = c0; gi.C1 = c0 + cp.Item2; gi.R0 = r; gi.R1 = r + rp.Item2;
						cursorR = r; cursorC = c0 + cp.Item2;
					}
					else {
						int span = cp.Item2;
						int r = cursorR, c = cursorC;
						while (true) {
							if (c + span - 1 > Math.Max(ncols, span)) { r++; c = 1; continue; }
							if (Free(r, r + rp.Item2, c, c + span)) break;
							c++;
						}
						gi.R0 = r; gi.R1 = r + rp.Item2; gi.C0 = c; gi.C1 = c + span;
						cursorR = r; cursorC = c + span;
					}
				}
				else {
					int span = rp.Item2;
					int nrows = Math.Max(1, rows.Count);
					int r = cursorR, c = cursorC;
					if (!cp.Item3) c = cp.Item1;
					while (true) {
						if (r + span - 1 > Math.Max(nrows, span)) { c++; r = 1; continue; }
						if (Free(r, r + span, c, c + cp.Item2)) break;
						r++;
					}
					gi.R0 = r; gi.R1 = r + span; gi.C0 = c; gi.C1 = c + cp.Item2;
					cursorR = r + span; cursorC = c;
				}
				Occupy(gi);
			}
			int maxC = gitems.Count > 0 ? gitems.Max(g => g.C1) - 1 : 0, maxR = gitems.Count > 0 ? gitems.Max(g => g.R1) - 1 : 0;
			while (cols.Count < maxC) cols.Add(ParseTrack(s.GridAutoColumns ?? "auto"));
			while (rows.Count < maxR) rows.Add(ParseTrack(s.GridAutoRows ?? "auto"));
			foreach (var gi in gitems) ResolveBoxModel(gi.B, cw);
			SizeTracks(cols, gitems, true, cw, colGap, s.JustifyContent);
			double x = 0;
			foreach (var t in cols) { t.Pos = x; x += t.Size + colGap; }
			foreach (var gi in gitems) {
				var k = gi.B;
				double areaW = cols[gi.C1 - 2].Pos + cols[gi.C1 - 2].Size - cols[gi.C0 - 1].Pos;
				byte js = k.S.JustifySelf != Style.AlAuto ? k.S.JustifySelf : s.JustifyItems;
				double w;
				if (k.S.Width.IsValue && k.S.Width.TryResolve(areaW) is double ww) w = ww - (k.S.BorderBox ? BP(k) : 0);
				else if (k.IsReplaced) ReplacedSize(k, areaW, double.NaN, out w, out _);
				else if (js == Style.AlNormal || js == Style.AlStretch) w = areaW - k.M[1] - k.M[3] - BP(k);
				else { ContentIntrinsic(k, out double mn, out double mx); w = Math.Min(Math.Max(mn, areaW - k.M[1] - k.M[3] - BP(k)), mx); }
				w = ClampW(k, w, areaW);
				k.ForceW = Math.Max(0, w);
				bool pg = Paginate;
				Paginate = false;
				LayoutBlockBox(k, 0, areaW, 0, null, double.NaN, out _);
				Paginate = pg;
			}
			SizeRowsFromItems(rows, gitems, rowGap, specH);
			double y = 0;
			foreach (var t in rows) { t.Pos = y; y += t.Size + rowGap; }
			double totalH = rows.Count > 0 ? rows.Sum(t => t.Size) + rowGap * (rows.Count - 1) : 0;
			double totalW = cols.Sum(t => t.Size) + colGap * (cols.Count - 1);
			double offX = 0;
			double freeW = cw - totalW;
			if (freeW > 0) {
				switch (s.JustifyContent) {
					case Style.AlCenter: offX = freeW / 2; break;
					case Style.AlEnd: case Style.AlFlexEnd: offX = freeW; break;
					case Style.AlSpaceBetween: if (cols.Count > 1) { double add = freeW / (cols.Count - 1); for (int i = 0; i < cols.Count; i++) cols[i].Pos += add * i; } break;
					case Style.AlSpaceAround: { double add = freeW / cols.Count; for (int i = 0; i < cols.Count; i++) cols[i].Pos += add * i + add / 2; break; }
					case Style.AlSpaceEvenly: { double add = freeW / (cols.Count + 1); for (int i = 0; i < cols.Count; i++) cols[i].Pos += add * (i + 1); break; }
				}
			}
			double gridH = specH ?? totalH;
			double offY = 0;
			if (specH != null && gridH > totalH) {
				double freeH = gridH - totalH;
				switch (s.AlignContent) {
					case Style.AlCenter: offY = freeH / 2; break;
					case Style.AlEnd: case Style.AlFlexEnd: offY = freeH; break;
				}
			}
			double baseline = double.NaN;
			foreach (var gi in gitems) {
				var k = gi.B;
				var c0 = cols[gi.C0 - 1]; var c1 = cols[gi.C1 - 2];
				var r0 = rows[gi.R0 - 1]; var r1 = rows[gi.R1 - 2];
				double areaX = c0.Pos + offX, areaW = c1.Pos + c1.Size - c0.Pos;
				double areaY = r0.Pos + offY, areaH = r1.Pos + r1.Size - r0.Pos;
				byte js = k.S.JustifySelf != Style.AlAuto ? k.S.JustifySelf : s.JustifyItems;
				byte als = k.S.AlignSelf != Style.AlAuto ? k.S.AlignSelf : s.AlignItems;
				double outerW = k.W + k.M[1] + k.M[3];
				double dx = 0;
				if (js == Style.AlCenter) dx = (areaW - outerW) / 2;
				else if (js == Style.AlEnd || js == Style.AlFlexEnd || js == Style.AlSelfEnd) dx = areaW - outerW;
				else if (js == Style.AlRight) dx = rtl ? 0 : areaW - outerW;
				else if (js == Style.AlLeft) dx = rtl ? areaW - outerW : 0;
				double left = rtl ? cx + cw - areaX - areaW + (areaW - outerW - dx) : cx + areaX + dx;
				double top = cy + areaY;
				bool stretchH = (als == Style.AlNormal || als == Style.AlStretch) && k.S.Height.IsAuto && !k.IsReplaced;
				double outerH = k.H + k.M[0] + k.M[2];
				double dy = 0;
				if (!stretchH) {
					if (als == Style.AlCenter) dy = (areaH - outerH) / 2;
					else if (als == Style.AlEnd || als == Style.AlFlexEnd || als == Style.AlSelfEnd) dy = areaH - outerH;
				}
				k.ForceW = k.W - BP(k);
				if (stretchH) k.ForceH = Math.Max(0, areaH - k.M[0] - k.M[2] - BPV(k));
				LayoutBlockBox(k, left, outerW, top + dy + k.M[0], null, areaH, out _);
				k.ForceW = double.NaN; k.ForceH = double.NaN;
				Translate(k, left + k.M[3] - k.X, 0);
				if (double.IsNaN(baseline) && gi.R0 == 1) { double bl = FirstBaselineOf(k); baseline = double.IsNaN(bl) ? k.Y + k.H : bl; }
			}
			b.Baseline = baseline;
			return gridH;
		}

		void SizeTracks(List<GridTrack> tracks, List<GItem> items, bool col, double avail, double gap, byte justify) {
			int n = tracks.Count;
			foreach (var t in tracks) {
				t.Base = t.MinK == GridTrack.Fixed ? t.MinV : t.MinK == GridTrack.Pct ? (double.IsNaN(avail) ? 0 : t.MinV * avail / 100) : 0;
				t.Limit = t.MaxK == GridTrack.Fixed ? t.MaxV : t.MaxK == GridTrack.Pct ? (double.IsNaN(avail) ? double.PositiveInfinity : t.MaxV * avail / 100) : t.MaxK == GridTrack.Fr ? double.PositiveInfinity : 0;
				if (t.MaxK == GridTrack.Fixed || t.MaxK == GridTrack.Pct) t.Limit = Math.Max(t.Limit, t.Base);
			}
			var singles = items.Where(i => (col ? i.C1 - i.C0 : i.R1 - i.R0) == 1).ToList();
			var multi = items.Where(i => (col ? i.C1 - i.C0 : i.R1 - i.R0) > 1).OrderBy(i => col ? i.C1 - i.C0 : i.R1 - i.R0).ToList();
			foreach (var it in singles) {
				var t = tracks[(col ? it.C0 : it.R0) - 1];
				if (t.MinK == GridTrack.Fixed && t.MaxK == GridTrack.Fixed) continue;
				IntrinsicOuter(it.B, out double mn, out double mx);
				if (t.MinK == GridTrack.Auto || t.MinK == GridTrack.MinC) t.Base = Math.Max(t.Base, mn);
				else if (t.MinK == GridTrack.MaxC) t.Base = Math.Max(t.Base, mx);
				if (t.MaxK == GridTrack.Auto || t.MaxK == GridTrack.MaxC) t.Limit = Math.Max(t.Limit, mx);
				else if (t.MaxK == GridTrack.MinC) t.Limit = Math.Max(t.Limit, mn);
				else if (t.MaxK == GridTrack.FitC) t.Limit = Math.Max(t.Limit, Math.Min(mx, Math.Max(mn, t.MaxV)));
			}
			foreach (var t in tracks) if (!double.IsInfinity(t.Limit) && t.Limit < t.Base) t.Limit = t.Base;
			foreach (var it in multi) {
				int s0 = (col ? it.C0 : it.R0) - 1, s1 = (col ? it.C1 : it.R1) - 1;
				var span = tracks.Skip(s0).Take(s1 - s0).ToList();
				if (span.Any(t => t.MaxK == GridTrack.Fr)) continue;
				IntrinsicOuter(it.B, out double mn, out double mx);
				double have = span.Sum(t => t.Base) + gap * (span.Count - 1);
				var grow = span.Where(t => t.MinK == GridTrack.Auto || t.MinK == GridTrack.MinC || t.MinK == GridTrack.MaxC).ToList();
				if (mn > have && grow.Count > 0) foreach (var t in grow) t.Base += (mn - have) / grow.Count;
				double haveL = span.Sum(t => double.IsInfinity(t.Limit) ? t.Base : t.Limit) + gap * (span.Count - 1);
				var growL = span.Where(t => t.MaxK == GridTrack.Auto || t.MaxK == GridTrack.MaxC).ToList();
				if (mx > haveL && growL.Count > 0) foreach (var t in growL) t.Limit += (mx - haveL) / growL.Count;
			}
			foreach (var t in tracks) if (!double.IsInfinity(t.Limit) && t.Limit < t.Base) t.Limit = t.Base;
			double gaps = gap * Math.Max(0, n - 1);
			double free = double.IsNaN(avail) ? double.PositiveInfinity : avail - gaps - tracks.Sum(t => t.Base);
			if (free > 0) {
				var growable = tracks.Where(t => t.MaxK != GridTrack.Fr && !double.IsInfinity(t.Limit) && t.Limit > t.Base).ToList();
				while (free > 1e-6 && growable.Count > 0) {
					double share = free / growable.Count;
					var next = new List<GridTrack>();
					double used = 0;
					foreach (var t in growable) {
						double add = Math.Min(share, t.Limit - t.Base);
						t.Base += add; used += add;
						if (t.Limit - t.Base > 1e-6) next.Add(t);
					}
					free -= used;
					if (used < 1e-9) break;
					growable = next;
				}
			}
			var frs = tracks.Where(t => t.MaxK == GridTrack.Fr).ToList();
			if (frs.Count > 0) {
				double leftover = double.IsNaN(avail) || double.IsInfinity(avail) ? 0 : avail - gaps - tracks.Where(t => t.MaxK != GridTrack.Fr).Sum(t => t.Base);
				if (double.IsNaN(avail) || double.IsInfinity(avail)) {
					double unit = 0;
					foreach (var t in frs) unit = Math.Max(unit, t.MaxV > 0 ? t.Base / t.MaxV : 0);
					foreach (var t in frs) t.Base = Math.Max(t.Base, unit * t.MaxV);
				}
				else {
					var flex = frs.ToList();
					for (int guard = 0; guard < 20; guard++) {
						double sumFr = flex.Sum(t => t.MaxV);
						double fixedPart = frs.Except(flex).Sum(t => t.Base);
						double unit = sumFr > 0 ? Math.Max(0, leftover - fixedPart) / Math.Max(1, sumFr) : 0;
						if (sumFr < 1 && sumFr > 0) unit = Math.Max(0, leftover - fixedPart);
						var inflex = flex.Where(t => t.MaxV * unit < t.Base).ToList();
						if (inflex.Count == 0) { foreach (var t in flex) t.Base = Math.Max(t.Base, t.MaxV * unit); break; }
						foreach (var t in inflex) flex.Remove(t);
						if (flex.Count == 0) break;
					}
				}
			}
			else if ((justify == Style.AlNormal || justify == Style.AlStretch) && !double.IsNaN(avail) && !double.IsInfinity(avail)) {
				double rem = avail - gaps - tracks.Sum(t => t.Base);
				var autos = tracks.Where(t => t.MaxK == GridTrack.Auto).ToList();
				if (rem > 0 && autos.Count > 0) foreach (var t in autos) t.Base += rem / autos.Count;
			}
			foreach (var t in tracks) t.Size = t.Base;
		}

		void SizeRowsFromItems(List<GridTrack> rows, List<GItem> items, double gap, double? specH) {
			foreach (var t in rows) {
				t.Base = t.MinK == GridTrack.Fixed ? t.MinV : t.MinK == GridTrack.Pct && specH != null ? t.MinV * specH.Value / 100 : 0;
			}
			foreach (var it in items.OrderBy(i => i.R1 - i.R0)) {
				double h = it.B.H + it.B.M[0] + it.B.M[2];
				int s0 = it.R0 - 1, s1 = it.R1 - 1;
				var span = rows.Skip(s0).Take(s1 - s0).ToList();
				if (span.Count == 1) {
					var t = span[0];
					if (!(t.MinK == GridTrack.Fixed && t.MaxK == GridTrack.Fixed)) t.Base = Math.Max(t.Base, h);
					continue;
				}
				double have = span.Sum(t => t.Base) + gap * (span.Count - 1);
				var grow = span.Where(t => !(t.MinK == GridTrack.Fixed && t.MaxK == GridTrack.Fixed)).ToList();
				if (h > have && grow.Count > 0) foreach (var t in grow) t.Base += (h - have) / grow.Count;
			}
			if (specH != null) {
				double gaps = gap * Math.Max(0, rows.Count - 1);
				var frs = rows.Where(t => t.MaxK == GridTrack.Fr).ToList();
				double leftover = specH.Value - gaps - rows.Where(t => t.MaxK != GridTrack.Fr).Sum(t => t.Base);
				if (frs.Count > 0 && leftover > 0) {
					double sumFr = frs.Sum(t => t.MaxV);
					foreach (var t in frs) t.Base = Math.Max(t.Base, leftover * t.MaxV / Math.Max(1, sumFr));
				}
				else if (frs.Count == 0) {
					double rem = specH.Value - gaps - rows.Sum(t => t.Base);
					var autos = rows.Where(t => t.MaxK == GridTrack.Auto).ToList();
					if (rem > 0 && autos.Count > 0) foreach (var t in autos) t.Base += rem / autos.Count;
				}
			}
			foreach (var t in rows) t.Size = t.Base;
		}
	}

	internal static class F {
		public static string N(double v) {
			if (double.IsNaN(v) || double.IsInfinity(v)) return "0";
			double r = Math.Round(v, 4);
			if (Math.Abs(r) < 0.00005) return "0";
			return r.ToString("0.####", CultureInfo.InvariantCulture);
		}

		public static string Str(string s) {
			var sb = new StringBuilder("(");
			foreach (char c in s) {
				if (c == '(' || c == ')' || c == '\\') sb.Append('\\').Append(c);
				else if (c < 32 || c > 126) sb.Append('\\').Append(Convert.ToString(c & 0xFF, 8).PadLeft(3, '0'));
				else sb.Append(c);
			}
			return sb.Append(')').ToString();
		}

		public static string Text(string s) {
			if (s.All(c => c >= 32 && c < 127)) return Str(s);
			var b = Encoding.BigEndianUnicode.GetBytes(s);
			var sb = new StringBuilder("<FEFF");
			foreach (var x in b) sb.Append(x.ToString("X2"));
			return sb.Append('>').ToString();
		}
	}

	internal sealed class PdfFontRes {
		public FontFile File;
		public string Name;
		public int Obj;
		public readonly HashSet<ushort> Used = new() { 0 };
		public readonly Dictionary<ushort, string> Uni = new();
	}

	internal sealed class PdfImageRes {
		public ImageData Img;
		public string Name;
		public int Obj;
	}

	internal sealed class PdfContent {
		public readonly StringBuilder Sb = new();
		public readonly HashSet<string> Fonts = new(), Images = new(), States = new(), Shadings = new(), XObjects = new(), Patterns = new();
		public PdfContent Op(string s) { Sb.Append(s).Append('\n'); return this; }
	}

	internal sealed class PdfDoc {
		readonly List<byte[]> _objs = new();
		readonly Dictionary<FontFile, PdfFontRes> _fonts = new();
		readonly Dictionary<ImageData, PdfImageRes> _images = new();
		readonly Dictionary<string, (string name, int obj)> _states = new();
		readonly Dictionary<string, int> _shadings = new(), _xobjects = new(), _patterns = new();
		readonly List<(int obj, double w, double h)> _pages = new();
		public readonly List<(int page, double x0, double y0, double x1, double y1, string uri, string dest)> Links = new();
		public readonly Dictionary<string, (int page, double y)> Anchors = new();
		public readonly List<(int level, string title, int page, double y)> Outline = new();
		public bool Compress = true;
		public string Title, Author, Subject, Keywords, Creator = "SinaMN75U HtmlToPdf", Lang;
		public bool Rtl;
		int _pagesObj;
		int _nameSeq;

		public PdfDoc() { _pagesObj = Reserve(); }

		public int Reserve() { _objs.Add(null); return _objs.Count; }
		public void Set(int n, string s) => _objs[n - 1] = Encoding.Latin1.GetBytes(n + " 0 obj\n" + s + "\nendobj\n");

		public void SetStream(int n, string dict, byte[] data, bool compress = true) {
			byte[] d = data;
			string filter = "";
			if (compress && Compress && data.Length > 32) {
				d = Deflate(data);
				filter = "/Filter /FlateDecode ";
			}
			SetRawStream(n, dict.Length > 0 ? dict.Substring(0, dict.Length - 2) + " " + filter + "/Length " + d.Length + " >>" : "<< " + filter + "/Length " + d.Length + " >>", d);
		}

		public void SetRawStream(int n, string dict, byte[] d) {
			using var ms = new MemoryStream();
			var head = Encoding.Latin1.GetBytes(n + " 0 obj\n" + dict + "\nstream\n");
			ms.Write(head);
			ms.Write(d);
			ms.Write(Encoding.Latin1.GetBytes("\nendstream\nendobj\n"));
			_objs[n - 1] = ms.ToArray();
		}

		public static byte[] Deflate(byte[] data) {
			using var ms = new MemoryStream();
			using (var z = new ZLibStream(ms, CompressionLevel.Optimal, true)) z.Write(data, 0, data.Length);
			return ms.ToArray();
		}

		public PdfFontRes Font(FontFile f) {
			if (_fonts.TryGetValue(f, out var r)) return r;
			r = new PdfFontRes { File = f, Name = "F" + (++_nameSeq), Obj = Reserve() };
			_fonts[f] = r;
			return r;
		}

		public PdfImageRes Image(ImageData img) {
			if (_images.TryGetValue(img, out var r)) return r;
			r = new PdfImageRes { Img = img, Name = "Im" + (++_nameSeq), Obj = Reserve() };
			WriteImage(r);
			_images[img] = r;
			return r;
		}

		public string State(double fill, double stroke, string blend = null, int smask = 0) {
			string key = F.N(fill) + "|" + F.N(stroke) + "|" + blend + "|" + smask;
			if (_states.TryGetValue(key, out var v)) return v.name;
			int n = Reserve();
			string name = "GS" + (++_nameSeq);
			var sb = new StringBuilder("<< /Type /ExtGState");
			if (fill < 1) sb.Append(" /ca ").Append(F.N(fill));
			if (stroke < 1) sb.Append(" /CA ").Append(F.N(stroke));
			if (blend != null) sb.Append(" /BM /").Append(blend);
			if (smask > 0) sb.Append(" /SMask << /Type /Mask /S /Luminosity /G ").Append(smask).Append(" 0 R >>");
			sb.Append(" >>");
			Set(n, sb.ToString());
			_states[key] = (name, n);
			return name;
		}

		public string AddShading(string dict) {
			int n = Reserve();
			Set(n, dict);
			string name = "Sh" + (++_nameSeq);
			_shadings[name] = n;
			return name;
		}

		public string AddPattern(string dict) {
			int n = Reserve();
			Set(n, dict);
			string name = "P" + (++_nameSeq);
			_patterns[name] = n;
			return name;
		}

		public int AddFunction(string dict) {
			int n = Reserve();
			Set(n, dict);
			return n;
		}

		public string AddForm(PdfContent c, double x0, double y0, double x1, double y1, bool group, bool luminosity = false) {
			int n = Reserve();
			string res = Resources(c);
			string g = group ? " /Group << /S /Transparency /CS /DeviceRGB " + (luminosity ? "" : "/I true ") + ">>" : "";
			SetStream(n, "<< /Type /XObject /Subtype /Form /BBox [" + F.N(x0) + " " + F.N(y0) + " " + F.N(x1) + " " + F.N(y1) + "] /Resources " + res + g + " >>", Encoding.Latin1.GetBytes(c.Sb.ToString()));
			string name = "Fm" + (++_nameSeq);
			_xobjects[name] = n;
			return name;
		}

		public int FormObj(string name) => _xobjects[name];

		string Resources(PdfContent c) {
			var sb = new StringBuilder("<< /ProcSet [/PDF /Text /ImageB /ImageC /ImageI]");
			if (c.Fonts.Count > 0) {
				sb.Append(" /Font <<");
				foreach (var f in _fonts.Values) if (c.Fonts.Contains(f.Name)) sb.Append(" /").Append(f.Name).Append(' ').Append(f.Obj).Append(" 0 R");
				sb.Append(" >>");
			}
			if (c.Images.Count > 0 || c.XObjects.Count > 0) {
				sb.Append(" /XObject <<");
				foreach (var i in _images.Values) if (c.Images.Contains(i.Name)) sb.Append(" /").Append(i.Name).Append(' ').Append(i.Obj).Append(" 0 R");
				foreach (var x in c.XObjects) if (_xobjects.TryGetValue(x, out int xo)) sb.Append(" /").Append(x).Append(' ').Append(xo).Append(" 0 R");
				sb.Append(" >>");
			}
			if (c.States.Count > 0) {
				sb.Append(" /ExtGState <<");
				foreach (var s in _states.Values) if (c.States.Contains(s.name)) sb.Append(" /").Append(s.name).Append(' ').Append(s.obj).Append(" 0 R");
				sb.Append(" >>");
			}
			if (c.Shadings.Count > 0) {
				sb.Append(" /Shading <<");
				foreach (var s in c.Shadings) if (_shadings.TryGetValue(s, out int so)) sb.Append(" /").Append(s).Append(' ').Append(so).Append(" 0 R");
				sb.Append(" >>");
			}
			if (c.Patterns.Count > 0) {
				sb.Append(" /Pattern <<");
				foreach (var s in c.Patterns) if (_patterns.TryGetValue(s, out int po)) sb.Append(" /").Append(s).Append(' ').Append(po).Append(" 0 R");
				sb.Append(" >>");
			}
			sb.Append(" >>");
			return sb.ToString();
		}

		public int AddPage(PdfContent c, double wPt, double hPt) {
			int page = Reserve();
			int content = Reserve();
			SetStream(content, "<< >>", Encoding.Latin1.GetBytes(c.Sb.ToString()));
			string res = Resources(c);
			_pages.Add((page, wPt, hPt));
			Set(page, "<< /Type /Page /Parent " + _pagesObj + " 0 R /MediaBox [0 0 " + F.N(wPt) + " " + F.N(hPt) + "] /Resources " + res + " /Contents " + content + " 0 R ANNOTS_" + page + " >>");
			return _pages.Count - 1;
		}

		void WriteImage(PdfImageRes r) {
			var img = r.Img;
			if (img.Jpeg != null) {
				string cs = img.Components == 1 ? "/DeviceGray" : img.Components == 4 ? "/DeviceCMYK" : "/DeviceRGB";
				string decode = img.Components == 4 && img.AdobeInverted ? " /Decode [1 0 1 0 1 0 1 0]" : "";
				SetRawStream(r.Obj, "<< /Type /XObject /Subtype /Image /Width " + img.PixW + " /Height " + img.PixH + " /ColorSpace " + cs + " /BitsPerComponent 8 /Interpolate true /Filter /DCTDecode" + decode + " /Length " + img.Jpeg.Length + " >>", img.Jpeg);
				return;
			}
			string smask = "";
			if (img.Alpha != null) {
				int sm = Reserve();
				SetStream(sm, "<< /Type /XObject /Subtype /Image /Width " + img.PixW + " /Height " + img.PixH + " /ColorSpace /DeviceGray /BitsPerComponent 8 /Interpolate true >>", img.Alpha);
				smask = " /SMask " + sm + " 0 R";
			}
			string cspace = img.Gray ? "/DeviceGray" : "/DeviceRGB";
			SetStream(r.Obj, "<< /Type /XObject /Subtype /Image /Width " + img.PixW + " /Height " + img.PixH + " /ColorSpace " + cspace + " /BitsPerComponent 8 /Interpolate true" + smask + " >>", img.Rgb);
		}

		public byte[] Finish() {
			foreach (var f in _fonts.Values) WriteFont(f);
			var kids = new StringBuilder();
			foreach (var p in _pages) kids.Append(p.obj).Append(" 0 R ");
			Set(_pagesObj, "<< /Type /Pages /Kids [" + kids + "] /Count " + _pages.Count + " >>");
			var annotsByPage = new Dictionary<int, List<int>>();
			foreach (var l in Links) {
				if (l.page < 0 || l.page >= _pages.Count) continue;
				var pg = _pages[l.page];
				string action;
				if (l.uri != null) action = "/A << /S /URI /URI " + F.Str(l.uri) + " >>";
				else if (l.dest != null && Anchors.TryGetValue(l.dest, out var a) && a.page < _pages.Count) action = "/Dest [" + _pages[a.page].obj + " 0 R /XYZ 0 " + F.N(a.y) + " 0]";
				else continue;
				int n = Reserve();
				Set(n, "<< /Type /Annot /Subtype /Link /Rect [" + F.N(l.x0) + " " + F.N(l.y0) + " " + F.N(l.x1) + " " + F.N(l.y1) + "] /Border [0 0 0] " + action + " >>");
				if (!annotsByPage.TryGetValue(l.page, out var list)) annotsByPage[l.page] = list = new List<int>();
				list.Add(n);
			}
			for (int i = 0; i < _pages.Count; i++) {
				int obj = _pages[i].obj;
				string s = Encoding.Latin1.GetString(_objs[obj - 1]);
				string rep = annotsByPage.TryGetValue(i, out var l) ? "/Annots [" + string.Join(" ", l.Select(x => x + " 0 R")) + "]" : "";
				_objs[obj - 1] = Encoding.Latin1.GetBytes(s.Replace("ANNOTS_" + obj, rep));
			}
			int outlineRoot = 0;
			if (Outline.Count > 0) outlineRoot = WriteOutline();
			int info = Reserve();
			var ib = new StringBuilder("<< /Producer (SinaMN75U HtmlToPdf) /Creator ").Append(F.Text(Creator ?? "")).Append(" /CreationDate (D:").Append(DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture)).Append("Z)");
			if (!string.IsNullOrEmpty(Title)) ib.Append(" /Title ").Append(F.Text(Title));
			if (!string.IsNullOrEmpty(Author)) ib.Append(" /Author ").Append(F.Text(Author));
			if (!string.IsNullOrEmpty(Subject)) ib.Append(" /Subject ").Append(F.Text(Subject));
			if (!string.IsNullOrEmpty(Keywords)) ib.Append(" /Keywords ").Append(F.Text(Keywords));
			ib.Append(" >>");
			Set(info, ib.ToString());
			int catalog = Reserve();
			var cb = new StringBuilder("<< /Type /Catalog /Pages " + _pagesObj + " 0 R");
			if (outlineRoot > 0) cb.Append(" /Outlines ").Append(outlineRoot).Append(" 0 R /PageMode /UseOutlines");
			if (!string.IsNullOrEmpty(Lang)) cb.Append(" /Lang ").Append(F.Text(Lang));
			cb.Append(" /ViewerPreferences << /DisplayDocTitle true").Append(Rtl ? " /Direction /R2L" : "").Append(" >>");
			cb.Append(" >>");
			Set(catalog, cb.ToString());
			using var ms = new MemoryStream();
			var header = new byte[] { (byte)'%', (byte)'P', (byte)'D', (byte)'F', (byte)'-', (byte)'1', (byte)'.', (byte)'7', (byte)'\n', (byte)'%', 0xE2, 0xE3, 0xCF, 0xD3, (byte)'\n' };
			ms.Write(header);
			var offsets = new long[_objs.Count];
			for (int i = 0; i < _objs.Count; i++) {
				offsets[i] = ms.Position;
				var o = _objs[i] ?? Encoding.Latin1.GetBytes((i + 1) + " 0 obj\nnull\nendobj\n");
				ms.Write(o);
			}
			long xref = ms.Position;
			var x = new StringBuilder();
			x.Append("xref\n0 ").Append(_objs.Count + 1).Append("\n0000000000 65535 f \n");
			foreach (var off in offsets) x.Append(off.ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
			var id = Guid.NewGuid().ToString("N").ToUpperInvariant();
			x.Append("trailer\n<< /Size ").Append(_objs.Count + 1).Append(" /Root ").Append(catalog).Append(" 0 R /Info ").Append(info).Append(" 0 R /ID [<").Append(id).Append("> <").Append(id).Append(">] >>\nstartxref\n").Append(xref).Append("\n%%EOF\n");
			ms.Write(Encoding.Latin1.GetBytes(x.ToString()));
			return ms.ToArray();
		}

		int WriteOutline() {
			int root = Reserve();
			var nodes = Outline.Select(o => (o, obj: Reserve(), parent: 0, kids: new List<int>())).ToList();
			var stack = new List<int>();
			var parentOf = new int[nodes.Count];
			for (int i = 0; i < nodes.Count; i++) {
				while (stack.Count > 0 && Outline[stack[stack.Count - 1]].level >= Outline[i].level) stack.RemoveAt(stack.Count - 1);
				parentOf[i] = stack.Count > 0 ? stack[stack.Count - 1] : -1;
				stack.Add(i);
			}
			var children = new Dictionary<int, List<int>>();
			for (int i = 0; i < nodes.Count; i++) { int p = parentOf[i]; if (!children.TryGetValue(p, out var l)) children[p] = l = new List<int>(); l.Add(i); }
			int Count(int i) => children.TryGetValue(i, out var l) ? l.Count + l.Sum(Count) : 0;
			for (int i = 0; i < nodes.Count; i++) {
				int p = parentOf[i];
				var sibs = children[p];
				int idx = sibs.IndexOf(i);
				var sb = new StringBuilder("<< /Title ").Append(F.Text(Outline[i].title)).Append(" /Parent ").Append(p < 0 ? root : nodes[p].obj).Append(" 0 R");
				if (idx > 0) sb.Append(" /Prev ").Append(nodes[sibs[idx - 1]].obj).Append(" 0 R");
				if (idx < sibs.Count - 1) sb.Append(" /Next ").Append(nodes[sibs[idx + 1]].obj).Append(" 0 R");
				if (children.TryGetValue(i, out var kids) && kids.Count > 0) sb.Append(" /First ").Append(nodes[kids[0]].obj).Append(" 0 R /Last ").Append(nodes[kids[kids.Count - 1]].obj).Append(" 0 R /Count -").Append(Count(i));
				int pg = Math.Clamp(Outline[i].page, 0, _pages.Count - 1);
				sb.Append(" /Dest [").Append(_pages[pg].obj).Append(" 0 R /XYZ 0 ").Append(F.N(Outline[i].y)).Append(" 0] >>");
				Set(nodes[i].obj, sb.ToString());
			}
			var top = children[-1];
			Set(root, "<< /Type /Outlines /First " + nodes[top[0]].obj + " 0 R /Last " + nodes[top[top.Count - 1]].obj + " 0 R /Count " + nodes.Count + " >>");
			return root;
		}

		void WriteFont(PdfFontRes r) {
			var f = r.File;
			double k = 1000.0 / f.UnitsPerEm;
			string tag = "";
			var rnd = new Random(r.Name.GetHashCode() ^ f.PsName.GetHashCode());
			for (int i = 0; i < 6; i++) tag += (char)('A' + rnd.Next(26));
			string ps = new string((f.PsName.Length > 0 ? f.PsName : f.Families.FirstOrDefault() ?? "Font").Where(c => c > 32 && c < 127 && c != '/' && c != '(' && c != ')' && c != '[' && c != ']' && c != '<' && c != '>' && c != '{' && c != '}' && c != '%').ToArray());
			if (ps.Length == 0) ps = "Font";
			string baseFont = tag + "+" + ps;
			int cid = Reserve(), desc = Reserve(), file = Reserve(), tu = Reserve();
			var used = r.Used.OrderBy(g => g).ToList();
			var w = new StringBuilder("[");
			int prev = -2;
			foreach (var g in used) {
				if (g != prev + 1) { if (prev >= 0) w.Append("] "); w.Append(g).Append(" ["); }
				else w.Append(' ');
				w.Append(F.N(f.AdvanceUnits(g) * k));
				prev = g;
			}
			if (prev >= 0) w.Append(']');
			w.Append(']');
			int flags = 4;
			if (f.FixedPitch) flags |= 1;
			if (f.ItalicAngle != 0) flags |= 64;
			f.Metrics(out double asc, out double dsc, out _);
			string bbox = "[" + F.N(f.XMin * k) + " " + F.N(f.YMin * k) + " " + F.N(f.XMax * k) + " " + F.N(f.YMax * k) + "]";
			string ffKey;
			if (f.IsCff) {
				SetStream(file, "<< /Subtype /OpenType >>", ExtractFace(f));
				ffKey = "/FontFile3";
			}
			else {
				var sub = TtfSubset.Build(f, r.Used);
				SetStream(file, "<< /Length1 " + sub.Length + " >>", sub);
				ffKey = "/FontFile2";
			}
			Set(desc, "<< /Type /FontDescriptor /FontName /" + baseFont + " /Flags " + flags + " /FontBBox " + bbox + " /ItalicAngle " + F.N(f.ItalicAngle) + " /Ascent " + F.N(asc * 1000) + " /Descent " + F.N(-dsc * 1000) + " /CapHeight " + F.N(f.CapHeightEm() * 1000) + " /StemV 80 " + ffKey + " " + file + " 0 R >>");
			string sub2 = f.IsCff ? "/CIDFontType0" : "/CIDFontType2";
			string map = f.IsCff ? "" : " /CIDToGIDMap /Identity";
			Set(cid, "<< /Type /Font /Subtype " + sub2 + " /BaseFont /" + baseFont + " /CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >> /FontDescriptor " + desc + " 0 R /DW " + F.N(f.AdvanceUnits(0) * k) + " /W " + w + map + " >>");
			SetStream(tu, "<< >>", Encoding.Latin1.GetBytes(ToUnicode(r)));
			Set(r.Obj, "<< /Type /Font /Subtype /Type0 /BaseFont /" + baseFont + " /Encoding /Identity-H /DescendantFonts [" + cid + " 0 R] /ToUnicode " + tu + " 0 R >>");
		}

		static byte[] ExtractFace(FontFile f) {
			if (f.DirOff == 0 && BE.U32(f.D, 0) != 0x74746366) return f.D;
			var tables = new List<(uint, byte[])>();
			foreach (var kv in f.Tables) {
				var b = new byte[kv.Value.len];
				Buffer.BlockCopy(f.D, kv.Value.off, b, 0, kv.Value.len);
				tables.Add((BE.U32(Encoding.ASCII.GetBytes(kv.Key), 0), b));
			}
			return Sfnt.Build(BE.U32(f.D, f.DirOff), tables);
		}

		static string ToUnicode(PdfFontRes r) {
			var sb = new StringBuilder();
			sb.Append("/CIDInit /ProcSet findresource begin\n12 dict begin\nbegincmap\n/CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def\n/CMapName /Adobe-Identity-UCS def\n/CMapType 2 def\n1 begincodespacerange\n<0000> <FFFF>\nendcodespacerange\n");
			var entries = r.Uni.Where(kv => !string.IsNullOrEmpty(kv.Value)).OrderBy(kv => kv.Key).ToList();
			for (int i = 0; i < entries.Count; i += 100) {
				var chunk = entries.Skip(i).Take(100).ToList();
				sb.Append(chunk.Count).Append(" beginbfchar\n");
				foreach (var kv in chunk) {
					sb.Append('<').Append(kv.Key.ToString("X4")).Append("> <");
					foreach (var b in Encoding.BigEndianUnicode.GetBytes(kv.Value)) sb.Append(b.ToString("X2"));
					sb.Append(">\n");
				}
				sb.Append("endbfchar\n");
			}
			sb.Append("endcmap\nCMapName currentdict /CMap defineresource pop\nend\nend\n");
			return sb.ToString();
		}
	}

	internal static class TtfSubset {
		public static byte[] Build(FontFile f, HashSet<ushort> used) {
			var keep = new HashSet<int>(used.Select(x => (int)x)) { 0 };
			var queue = new Queue<int>(keep);
			while (queue.Count > 0) {
				int g = queue.Dequeue();
				var (o, l) = f.GlyphData(g);
				if (l < 10) continue;
				if (BE.S16(f.D, o) >= 0) continue;
				int p = o + 10;
				while (true) {
					if (p + 4 > f.D.Length) break;
					int flags = BE.U16(f.D, p);
					int comp = BE.U16(f.D, p + 2);
					if (keep.Add(comp)) queue.Enqueue(comp);
					p += 4 + ((flags & 1) != 0 ? 4 : 2);
					if ((flags & 8) != 0) p += 2; else if ((flags & 0x40) != 0) p += 4; else if ((flags & 0x80) != 0) p += 8;
					if ((flags & 0x20) == 0) break;
				}
			}
			int n = f.NumGlyphs;
			var glyf = new List<byte>();
			var loca = new List<byte>();
			for (int g = 0; g < n; g++) {
				BE.W32(loca, (uint)glyf.Count);
				if (!keep.Contains(g)) continue;
				var (o, l) = f.GlyphData(g);
				if (l <= 0) continue;
				for (int i = 0; i < l; i++) glyf.Add(f.D[o + i]);
				while (glyf.Count % 4 != 0) glyf.Add(0);
			}
			BE.W32(loca, (uint)glyf.Count);
			var tables = new List<(uint, byte[])>();
			void Copy(string tag) {
				if (!f.Tables.TryGetValue(tag, out var t)) return;
				var b = new byte[t.len];
				Buffer.BlockCopy(f.D, t.off, b, 0, t.len);
				if (tag == "head") { BE.P16(b, 50, 1); BE.P32(b, 8, 0); }
				tables.Add((BE.U32(Encoding.ASCII.GetBytes(tag), 0), b));
			}
			foreach (var t in new[] { "head", "hhea", "maxp", "hmtx", "cvt ", "fpgm", "prep", "OS/2" }) Copy(t);
			if (f.Tables.ContainsKey("post")) {
				var t = f.Tables["post"];
				var b = new byte[32];
				Buffer.BlockCopy(f.D, t.off, b, 0, Math.Min(32, t.len));
				BE.P32(b, 0, 0x00030000);
				tables.Add((BE.U32(Encoding.ASCII.GetBytes("post"), 0), b));
			}
			tables.Add((BE.U32(Encoding.ASCII.GetBytes("glyf"), 0), glyf.ToArray()));
			tables.Add((BE.U32(Encoding.ASCII.GetBytes("loca"), 0), loca.ToArray()));
			var cmap = new List<byte>();
			BE.W16(cmap, 0); BE.W16(cmap, 1); BE.W16(cmap, 3); BE.W16(cmap, 1); BE.W32(cmap, 12);
			BE.W16(cmap, 4); BE.W16(cmap, 24); BE.W16(cmap, 0); BE.W16(cmap, 2); BE.W16(cmap, 2); BE.W16(cmap, 0); BE.W16(cmap, 0);
			BE.W16(cmap, 0xFFFF); BE.W16(cmap, 0); BE.W16(cmap, 0xFFFF); BE.W16(cmap, 1); BE.W16(cmap, 0);
			tables.Add((BE.U32(Encoding.ASCII.GetBytes("cmap"), 0), cmap.ToArray()));
			return Sfnt.Build(0x00010000, tables);
		}
	}

	internal sealed class Painter {
		readonly PdfDoc _pdf;
		readonly LayoutEngine _L;
		readonly RenderContext _ctx;
		PdfContent _c;
		int _page;
		double _pageTop, _pageBottom;
		public double MarginLeft, MarginTop, PageWpx, PageHpx, PageHpt, ClipH = double.NaN, ClipW = double.NaN;
		readonly HashSet<Box> _linked = new();

		public Painter(PdfDoc pdf, LayoutEngine l, RenderContext ctx) { _pdf = pdf; _L = l; _ctx = ctx; }

		bool PrintBg(Style s) => _ctx.PrintBackground || s.PrintExact;

		public static void ComputeExtents(Box b) {
			double min = b.Y, max = b.Y + b.H;
			if (b.Kind == BK.Text) { b.MinY = double.PositiveInfinity; b.MaxY = double.NegativeInfinity; return; }
			if (b.Lines != null) foreach (var l in b.Lines) { min = Math.Min(min, l.Y); max = Math.Max(max, l.Y + l.H); foreach (var bf in l.Boxes) { min = Math.Min(min, bf.Top); max = Math.Max(max, bf.Bottom); } }
			if (b.S.BoxShadow != null) foreach (var sh in b.S.BoxShadow) { max = Math.Max(max, b.Y + b.H + sh.Y + sh.Spread + sh.Blur); min = Math.Min(min, b.Y + sh.Y - sh.Spread - sh.Blur); }
			if (b.RepeatHeaders != null) foreach (var r in b.RepeatHeaders) max = Math.Max(max, r.y + r.h);
			if (b.RepeatFooters != null) foreach (var r in b.RepeatFooters) max = Math.Max(max, r.y + r.h);
			foreach (var k in b.Kids) {
				ComputeExtents(k);
				if (k.Kind == BK.Text) continue;
				min = Math.Min(min, k.MinY); max = Math.Max(max, k.MaxY);
			}
			if (b.MarkerFrags != null) foreach (var g in b.MarkerFrags) { min = Math.Min(min, g.Baseline - 50); max = Math.Max(max, g.Baseline + 20); }
			if (b.S.Transform != null) { min -= 2000; max += 2000; }
			b.MinY = min; b.MaxY = max;
		}

		public void PaintPage(PdfContent c, Box root, int page, double pageH, IEnumerable<Box> fixedBoxes) {
			_c = c;
			_page = page;
			_pageTop = page * pageH;
			_pageBottom = _pageTop + pageH;
			c.Op("q");
			c.Op("0.75 0 0 -0.75 0 " + F.N(PageHpt) + " cm");
			PaintCanvas(root);
			c.Op("1 0 0 1 " + F.N(MarginLeft) + " " + F.N(MarginTop) + " cm");
			if (double.IsNaN(ClipW)) c.Op(F.N(-MarginLeft) + " 0 " + F.N(PageWpx) + " " + F.N(double.IsNaN(ClipH) ? pageH : Math.Min(ClipH, pageH)) + " re W n");
			else c.Op("0 0 " + F.N(ClipW) + " " + F.N(double.IsNaN(ClipH) ? pageH : Math.Min(ClipH, pageH)) + " re W n");
			c.Op("1 0 0 1 0 " + F.N(-_pageTop) + " cm");
			PaintWrapped(root, true);
			foreach (var f in fixedBoxes) {
				c.Op("q 1 0 0 1 0 " + F.N(_pageTop) + " cm");
				double saveT = _pageTop, saveB = _pageBottom;
				_pageTop = double.NegativeInfinity; _pageBottom = double.PositiveInfinity;
				PaintWrapped(f, false);
				_pageTop = saveT; _pageBottom = saveB;
				c.Op("Q");
			}
			c.Op("Q");
		}

		void PaintCanvas(Box root) {
			var s = root.S;
			Box src = root;
			if (s.BackgroundColor.A == 0 && s.Backgrounds == null) {
				var body = root.Kids.FirstOrDefault(k => k.El != null && k.El.Tag == "body");
				if (body != null) src = body;
			}
			if (!PrintBg(src.S)) return;
			if (src.S.BackgroundColor.A > 0) {
				Fill(src.S.BackgroundColor);
				_c.Op("0 0 " + F.N(PageWpx) + " " + F.N(PageHpx) + " re f");
			}
			if (src.S.Backgrounds != null) {
				_c.Op("q 1 0 0 1 " + F.N(MarginLeft) + " " + F.N(MarginTop - _pageTop) + " cm");
				PaintBgLayers(src, src.S, -MarginLeft, _pageTop - MarginTop, PageWpx, PageHpx, new double[8], true);
				_c.Op("Q");
			}
		}

		sealed class Gathered {
			public List<(Box b, List<Box> clips)> Neg = new(), Blocks = new(), Floats = new(), Inlines = new(), Pos = new(), PosZ = new();
		}

		static bool CreatesSC(Box k) => k.Kind != BK.Inline && (k.S.Opacity < 1 || k.S.Transform != null || k.S.IsPositioned || k.S.ClipPath != null);

		bool Visible(Box k) => k.MaxY > _pageTop + 0.01 && k.MinY < _pageBottom - 0.01 || k.MaxY - k.MinY <= 0.01 && k.MinY >= _pageTop && k.MinY < _pageBottom;

		void Gather(Box b, List<Box> clips, Gathered g) {
			foreach (var k in b.Kids) {
				if (k.Kind == BK.Text) continue;
				if (!Visible(k) && k.S.Position != Pos.Fixed) continue;
				if (k.S.Position == Pos.Fixed) continue;
				if (CreatesSC(k)) {
					int z = k.S.ZIndex ?? 0;
					if (k.S.IsPositioned && z < 0) g.Neg.Add((k, clips));
					else if (k.S.IsPositioned && z > 0) g.PosZ.Add((k, clips));
					else g.Pos.Add((k, clips));
					continue;
				}
				if (k.IsFloat) { g.Floats.Add((k, clips)); continue; }
				if (k.IsAtomicInline) continue;
				if (k.Kind != BK.Inline && k.Kind != BK.Br && k.Kind != BK.Wbr) g.Blocks.Add((k, clips));
				var kc = clips;
				if ((k.S.OverflowX >= 2 || k.S.OverflowY >= 2) && k.Kind != BK.Inline) kc = new List<Box>(clips) { k };
				if (k.Lines != null) g.Inlines.Add((k, kc));
				Gather(k, kc, g);
			}
		}

		public void PaintWrapped(Box b, bool isRoot) {
			var s = b.S;
			bool transform = s.Transform != null && !isRoot;
			bool opacity = s.Opacity < 1;
			bool clipPath = s.ClipPath != null;
			if (s.Opacity <= 0) return;
			_c.Op("q");
			if (transform) {
				var m = TransformMatrix(b);
				if (m != null) _c.Op(string.Join(" ", m.Select(F.N)) + " cm");
				_transformDepth++;
			}
			if (clipPath) ApplyClipPath(b);
			if (opacity) {
				var outer = _c;
				var inner = new PdfContent();
				_c = inner;
				PaintSC(b, isRoot);
				_c = outer;
				string form = _pdf.AddForm(inner, -1e5, -1e6, 1e5, 1e7, true);
				_c.XObjects.Add(form);
				string gs = _pdf.State(s.Opacity, s.Opacity);
				_c.States.Add(gs);
				_c.Op("/" + gs + " gs /" + form + " Do");
			}
			else PaintSC(b, isRoot);
			_c.Op("Q");
			if (transform) _transformDepth--;
		}

		void PaintSC(Box root, bool isRoot) {
			var g = new Gathered();
			var clips = new List<Box>();
			if (!isRoot && (root.S.OverflowX >= 2 || root.S.OverflowY >= 2) && root.Kind != BK.Inline) clips.Add(root);
			Gather(root, clips, g);
			if (!isRoot) PaintBoxDecor(root, null);
			else PaintBoxDecor(root, null, true);
			if (root.Lines != null) g.Inlines.Insert(0, (root, clips));
			foreach (var e in g.Neg.OrderBy(x => x.b.S.ZIndex ?? 0)) WithClips(e.clips, () => PaintWrapped(e.b, false));
			foreach (var e in g.Blocks) WithClips(e.clips, () => PaintBoxDecor(e.b, null));
			if (root.MarkerFrags != null || root.MarkerImg != null || root.MarkerShape != 0) WithClips(clips, () => PaintMarker(root));
			foreach (var e in g.Blocks) if (e.b.MarkerFrags != null || e.b.MarkerImg != null || e.b.MarkerShape != 0) WithClips(e.clips, () => PaintMarker(e.b));
			foreach (var e in g.Floats) WithClips(e.clips, () => PaintWrapped(e.b, false));
			foreach (var e in g.Inlines) WithClips(e.clips, () => PaintLines(e.b));
			foreach (var e in g.Pos) WithClips(e.clips, () => PaintWrapped(e.b, false));
			foreach (var e in g.PosZ.OrderBy(x => x.b.S.ZIndex ?? 0)) WithClips(e.clips, () => PaintWrapped(e.b, false));
			RecordAnchor(root);
		}

		readonly List<(double x0, double y0, double x1, double y1)> _clipStack = new();
		int _transformDepth;

		void WithClips(List<Box> clips, Action a) {
			if (clips == null || clips.Count == 0) { a(); return; }
			int pushed = 0;
			foreach (var cb in clips) {
				if (cb.S.OverflowX < 2 && cb.S.OverflowY < 2) continue;
				double x0 = cb.S.OverflowX >= 2 ? cb.X + cb.Bd[3] : double.NegativeInfinity, x1 = cb.S.OverflowX >= 2 ? cb.X + cb.W - cb.Bd[1] : double.PositiveInfinity;
				double y0 = cb.S.OverflowY >= 2 ? cb.Y + cb.Bd[0] : double.NegativeInfinity, y1 = cb.S.OverflowY >= 2 ? cb.Y + cb.H - cb.Bd[2] : double.PositiveInfinity;
				_clipStack.Add((x0, y0, x1, y1));
				pushed++;
			}
			try { WithClipsInner(clips, a); }
			finally { _clipStack.RemoveRange(_clipStack.Count - pushed, pushed); }
		}

		void WithClipsInner(List<Box> clips, Action a) {
			_c.Op("q");
			foreach (var cb in clips) {
				double x = cb.X + cb.Bd[3], y = cb.Y + cb.Bd[0], w = cb.W - cb.Bd[1] - cb.Bd[3], h = cb.H - cb.Bd[0] - cb.Bd[2];
				if (cb.S.OverflowX < 2) { x -= 1e5; w += 2e5; }
				if (cb.S.OverflowY < 2) { y -= 1e6; h += 2e6; }
				var r = Radii(cb, cb.X, cb.Y, cb.W, cb.H);
				var ir = InnerRadii(r, cb.Bd);
				PathRoundRect(x, y, w, h, ir);
				_c.Op("W n");
			}
			a();
			_c.Op("Q");
		}

		void RecordAnchor(Box b) {
			if (b.El == null) return;
			string id = b.El.Id ?? (b.El.Tag == "a" ? b.El.Attr("name") : null);
			if (id != null && !_pdf.Anchors.ContainsKey(id) && b.Y >= _pageTop - 0.5 && b.Y < _pageBottom) _pdf.Anchors[id] = (_page, ToPtY(b.Y));
		}

		double ToPtX(double x) => (x + MarginLeft) * 0.75;
		double ToPtY(double y) => PageHpt - (y - _pageTop + MarginTop) * 0.75;

		void AddLink(Box b, double x0, double y0, double x1, double y1) {
			string href = b.Href;
			if (href == null) return;
			double t = Math.Max(y0, _pageTop), bt = Math.Min(y1, _pageBottom);
			if (bt <= t) return;
			string uri = null, dest = null;
			if (href.StartsWith("#")) dest = href.Substring(1);
			else if (href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)) return;
			else uri = href;
			_pdf.Links.Add((_page, ToPtX(x0), ToPtY(bt), ToPtX(x1), ToPtY(t), uri, dest));
		}

		public void Fill(Rgba c) {
			_c.Op(F.N(c.R / 255.0) + " " + F.N(c.G / 255.0) + " " + F.N(c.B / 255.0) + " rg");
			if (c.A < 255) { string gs = _pdf.State(c.A / 255.0, 1); _c.States.Add(gs); _c.Op("/" + gs + " gs"); }
		}

		public void Stroke(Rgba c) {
			_c.Op(F.N(c.R / 255.0) + " " + F.N(c.G / 255.0) + " " + F.N(c.B / 255.0) + " RG");
			if (c.A < 255) { string gs = _pdf.State(1, c.A / 255.0); _c.States.Add(gs); _c.Op("/" + gs + " gs"); }
		}

		static double[] InnerRadii(double[] r, double[] bd) {
			var ir = new double[8];
			ir[0] = Math.Max(0, r[0] - bd[3]); ir[1] = Math.Max(0, r[1] - bd[0]);
			ir[2] = Math.Max(0, r[2] - bd[1]); ir[3] = Math.Max(0, r[3] - bd[0]);
			ir[4] = Math.Max(0, r[4] - bd[1]); ir[5] = Math.Max(0, r[5] - bd[2]);
			ir[6] = Math.Max(0, r[6] - bd[3]); ir[7] = Math.Max(0, r[7] - bd[2]);
			return ir;
		}

		public static double[] Radii(Box b, double x, double y, double w, double h) {
			var s = b.S;
			var r = new double[8];
			for (int i = 0; i < 4; i++) {
				r[i * 2] = Math.Max(0, s.RadiusH[i].Resolve(w));
				r[i * 2 + 1] = Math.Max(0, s.RadiusV[i].Resolve(h));
			}
			double f = 1;
			double top = r[0] + r[2], bottom = r[6] + r[4], left = r[1] + r[7], right = r[3] + r[5];
			if (top > w && top > 0) f = Math.Min(f, w / top);
			if (bottom > w && bottom > 0) f = Math.Min(f, w / bottom);
			if (left > h && left > 0) f = Math.Min(f, h / left);
			if (right > h && right > 0) f = Math.Min(f, h / right);
			if (f < 1) for (int i = 0; i < 8; i++) r[i] *= f;
			return r;
		}

		const double K = 0.5522847498;

		public void PathRoundRect(double x, double y, double w, double h, double[] r) {
			if (r == null || r.All(v => v <= 0)) { _c.Op(F.N(x) + " " + F.N(y) + " " + F.N(w) + " " + F.N(h) + " re"); return; }
			double tlx = r[0], tly = r[1], trx = r[2], tr_ = r[3], brx = r[4], bry = r[5], blx = r[6], bly = r[7];
			var sb = new StringBuilder();
			sb.Append(F.N(x + tlx)).Append(' ').Append(F.N(y)).Append(" m\n");
			sb.Append(F.N(x + w - trx)).Append(' ').Append(F.N(y)).Append(" l\n");
			if (trx > 0 || tr_ > 0) sb.Append(F.N(x + w - trx + trx * K)).Append(' ').Append(F.N(y)).Append(' ').Append(F.N(x + w)).Append(' ').Append(F.N(y + tr_ - tr_ * K)).Append(' ').Append(F.N(x + w)).Append(' ').Append(F.N(y + tr_)).Append(" c\n");
			sb.Append(F.N(x + w)).Append(' ').Append(F.N(y + h - bry)).Append(" l\n");
			if (brx > 0 || bry > 0) sb.Append(F.N(x + w)).Append(' ').Append(F.N(y + h - bry + bry * K)).Append(' ').Append(F.N(x + w - brx + brx * K)).Append(' ').Append(F.N(y + h)).Append(' ').Append(F.N(x + w - brx)).Append(' ').Append(F.N(y + h)).Append(" c\n");
			sb.Append(F.N(x + blx)).Append(' ').Append(F.N(y + h)).Append(" l\n");
			if (blx > 0 || bly > 0) sb.Append(F.N(x + blx - blx * K)).Append(' ').Append(F.N(y + h)).Append(' ').Append(F.N(x)).Append(' ').Append(F.N(y + h - bly + bly * K)).Append(' ').Append(F.N(x)).Append(' ').Append(F.N(y + h - bly)).Append(" c\n");
			sb.Append(F.N(x)).Append(' ').Append(F.N(y + tly)).Append(" l\n");
			if (tlx > 0 || tly > 0) sb.Append(F.N(x)).Append(' ').Append(F.N(y + tly - tly * K)).Append(' ').Append(F.N(x + tlx - tlx * K)).Append(' ').Append(F.N(y)).Append(' ').Append(F.N(x + tlx)).Append(' ').Append(F.N(y)).Append(" c\n");
			sb.Append('h');
			_c.Op(sb.ToString());
		}

		void PaintBoxDecor(Box b, List<Box> _, bool isRoot = false) {
			if (b.Kind == BK.Inline || b.Kind == BK.Text) return;
			var s = b.S;
			double x = b.X, y = b.Y, w = b.W, h = b.H;
			if (b.Kind == BK.Table && !double.IsNaN(b.TH)) { y = b.Y + b.TY; h = b.TH; }
			Snap(ref x, ref y, ref w, ref h);
			if (b.Href != null && !_linked.Contains(b)) { AddLink(b, x, y, x + w, y + h); }
			RecordAnchor(b);
			if (b.El != null && b.El.Tag.Length == 2 && b.El.Tag[0] == 'h' && b.El.Tag[1] >= '1' && b.El.Tag[1] <= '6' && _ctx.Outline && b.Y >= _pageTop - 0.5 && b.Y < _pageBottom) {
				string title = HtmlParser.CollapseWs(b.El.TextContent()).Trim();
				if (title.Length > 0 && !_pdf.Outline.Any(o => o.title == title && o.page == _page && Math.Abs(o.y - ToPtY(b.Y)) < 0.1)) _pdf.Outline.Add((b.El.Tag[1] - '0', title, _page, ToPtY(b.Y)));
			}
			if (s.Visibility != 0) return;
			if (b.Kind == BK.Cell && b.Parent != null) {
				var t = FindTable(b);
				if (t != null && t.S.EmptyCellsHide && !t.S.BorderCollapse && IsEmptyCell(b)) return;
				PaintCellAncestorsBg(b, t);
			}
			if (isRoot) {
				PaintBorders(b, x, y, w, h);
				return;
			}
			var r = Radii(b, x, y, w, h);
			if (PrintBg(s)) PaintShadows(b, x, y, w, h, r, false);
			if (PrintBg(s) && b.Kind != BK.Row && b.Kind != BK.RowGroup && b.Kind != BK.Column && b.Kind != BK.ColGroup) PaintBackground(b, x, y, w, h, r);
			if (PrintBg(s)) PaintShadows(b, x, y, w, h, r, true);
			if (b.Kind == BK.Cell && b.CB != null && FindTable(b)?.S.BorderCollapse == true) PaintCollapsedBorders(b);
			else if (b.Kind != BK.Row && b.Kind != BK.RowGroup) PaintBorders(b, x, y, w, h);
			if (b.Kind == BK.Replaced) PaintReplaced(b);
			if (s.OutlineWidth > 0 && s.OutlineStyle != BS.None) PaintOutline(b, x, y, w, h);
			if (b.Kind == BK.Table && (b.RepeatHeaders != null || b.RepeatFooters != null)) PaintRepeatedHeaders(b);
		}

		static Box FindTable(Box cell) {
			for (var p = cell.Parent; p != null; p = p.Parent) if (p.Kind == BK.Table) return p;
			return null;
		}

		static bool IsEmptyCell(Box c) => c.Kids.All(k => k.Kind == BK.Text && string.IsNullOrWhiteSpace(k.Text));

		void PaintCellAncestorsBg(Box cell, Box t) {
			if (t == null) return;
			var row = cell.Parent;
			var grp = row?.Parent;
			var list = new List<Box>();
			var g = _L.GridOf(t);
			if (g != null && cell.Col < g.Cols.Count) {
				var col = g.Cols[cell.Col];
				if (col.Parent != null && col.Parent.Kind == BK.ColGroup) list.Add(col.Parent);
				list.Add(col);
			}
			if (grp != null && grp.Kind == BK.RowGroup) list.Add(grp);
			if (row != null && row.Kind == BK.Row) list.Add(row);
			foreach (var a in list) {
				if (!PrintBg(a.S)) continue;
				if (a.S.BackgroundColor.A > 0) { Fill(a.S.BackgroundColor); _c.Op(F.N(cell.X) + " " + F.N(cell.Y) + " " + F.N(cell.W) + " " + F.N(cell.H) + " re f"); }
				if (a.S.Backgrounds != null) {
					_c.Op("q " + F.N(cell.X) + " " + F.N(cell.Y) + " " + F.N(cell.W) + " " + F.N(cell.H) + " re W n");
					double ax = a.Kind == BK.Row || a.Kind == BK.RowGroup ? a.X : cell.X, ay = a.Kind == BK.Row || a.Kind == BK.RowGroup ? a.Y : cell.Y;
					double aw = a.Kind == BK.Row || a.Kind == BK.RowGroup ? a.W : cell.W, ah = a.Kind == BK.Row || a.Kind == BK.RowGroup ? a.H : cell.H;
					PaintBgLayers(a, a.S, ax, ay, aw, ah, new double[8], false);
					_c.Op("Q");
				}
			}
		}

		void PaintRepeatedHeaders(Box t) {
			var g = _L.GridOf(t);
			if (g == null) return;
			if (g.Head != null && t.RepeatHeaders != null) PaintRepeatedGroup(g.Head, t.RepeatHeaders);
			if (g.Foot != null && t.RepeatFooters != null) PaintRepeatedGroup(g.Foot, t.RepeatFooters);
		}

		void PaintRepeatedGroup(Box grp, List<(double y, double h)> at) {
			foreach (var (ry, rh) in at) {
				if (ry + rh < _pageTop || ry > _pageBottom) continue;
				double dy = ry - grp.Y;
				_L.Translate(grp, 0, dy);
				ComputeExtents(grp);
				try {
					foreach (var row in grp.Kids) {
						if (row.Kind != BK.Row) continue;
						PaintBoxDecor(row, null);
						foreach (var cell in row.Kids) {
							if (cell.Kind != BK.Cell) continue;
							PaintBoxDecor(cell, null);
							PaintSC(cell, true);
						}
					}
				}
				finally {
					_L.Translate(grp, 0, -dy);
					ComputeExtents(grp);
				}
			}
		}

		void PaintBackground(Box b, double x, double y, double w, double h, double[] r) {
			var s = b.S;
			if (s.BackgroundColor.A > 0) {
				byte clip = s.Backgrounds != null && s.Backgrounds.Count > 0 ? s.Backgrounds[s.Backgrounds.Count - 1].Clip : (byte)0;
				ClipBox(b, x, y, w, h, r, clip, out double cx, out double cy, out double cw, out double ch, out double[] cr);
				Fill(s.BackgroundColor);
				PathRoundRect(cx, cy, cw, ch, cr);
				_c.Op("f");
				if (s.BackgroundColor.A < 255) ResetAlpha();
			}
			if (s.Backgrounds != null) {
				_c.Op("q");
				PaintBgLayers(b, s, x, y, w, h, r, false);
				_c.Op("Q");
			}
		}

		void ResetAlpha() {
			string gs = _pdf.State(1, 1);
			_c.States.Add(gs);
			_c.Op("/" + gs + " gs");
		}

		void ClipBox(Box b, double x, double y, double w, double h, double[] r, byte clip, out double cx, out double cy, out double cw, out double ch, out double[] cr) {
			cx = x; cy = y; cw = w; ch = h; cr = r;
			if (clip == 1 || clip == 2) {
				cx += b.Bd[3]; cy += b.Bd[0]; cw -= b.Bd[1] + b.Bd[3]; ch -= b.Bd[0] + b.Bd[2];
				cr = InnerRadii(r, b.Bd);
				if (clip == 2) {
					cx += b.P[3]; cy += b.P[0]; cw -= b.P[1] + b.P[3]; ch -= b.P[0] + b.P[2];
					cr = InnerRadii(cr, b.P);
				}
			}
		}

		void PaintBgLayers(Box b, Style s, double x, double y, double w, double h, double[] r, bool canvas) {
			for (int li = s.Backgrounds.Count - 1; li >= 0; li--) {
				var layer = s.Backgrounds[li];
				if (layer.Url == null && layer.Grad == null) continue;
				ClipBox(b, x, y, w, h, r, canvas ? (byte)0 : layer.Clip == 3 ? (byte)0 : layer.Clip, out double cx, out double cy, out double cw, out double ch, out double[] cr);
				double ox = x, oy = y, ow = w, oh = h;
				if (!canvas && (layer.Origin == 0 || layer.Origin == 2)) {
					ox += b.Bd[3]; oy += b.Bd[0]; ow -= b.Bd[1] + b.Bd[3]; oh -= b.Bd[0] + b.Bd[2];
					if (layer.Origin == 2) { ox += b.P[3]; oy += b.P[0]; ow -= b.P[1] + b.P[3]; oh -= b.P[0] + b.P[2]; }
				}
				ImageData img = null;
				double iw, ih;
				if (layer.Url != null) {
					img = _ctx.LoadImage(layer.Url);
					if (img == null) continue;
					iw = img.W; ih = img.H;
				}
				else { iw = ow; ih = oh; }
				double tw, th;
				double ratio = iw > 0 && ih > 0 ? iw / ih : 1;
				if (layer.SizeKind == 1 || layer.SizeKind == 2) {
					double sc = layer.SizeKind == 1 ? Math.Max(ow / iw, oh / ih) : Math.Min(ow / iw, oh / ih);
					if (layer.Grad != null) { tw = ow; th = oh; }
					else { tw = iw * sc; th = ih * sc; }
				}
				else {
					bool wa = layer.SizeW.IsAuto, ha = layer.SizeH.IsAuto;
					double sw = wa ? 0 : layer.SizeW.Resolve(ow), sh = ha ? 0 : layer.SizeH.Resolve(oh);
					if (wa && ha) { tw = iw; th = ih; }
					else if (wa) { th = sh; tw = layer.Grad != null ? ow : sh * ratio; }
					else if (ha) { tw = sw; th = layer.Grad != null ? oh : sw / ratio; }
					else { tw = sw; th = sh; }
				}
				if (tw <= 0.01 || th <= 0.01) continue;
				if (layer.RepX == 3) { int n = Math.Max(1, (int)Math.Round(ow / tw)); double ntw = ow / n; if (layer.SizeH.IsAuto && layer.RepY != 3) th = th * ntw / tw; tw = ntw; }
				if (layer.RepY == 3) { int n = Math.Max(1, (int)Math.Round(oh / th)); th = oh / n; }
				double px = ox + layer.PosX.Px + (layer.PosX.HasPct ? layer.PosX.Pct / 100 * (ow - tw) : 0);
				double py = oy + layer.PosY.Px + (layer.PosY.HasPct ? layer.PosY.Pct / 100 * (oh - th) : 0);
				if (layer.PosX.Fn != null) px = ox + layer.PosX.Fn(ow - tw);
				if (layer.PosY.Fn != null) py = oy + layer.PosY.Fn(oh - th);
				_c.Op("q");
				PathRoundRect(cx, cy, cw, ch, cr);
				_c.Op("W n");
				double spaceX = 0, spaceY = 0;
				if (layer.RepX == 2 && tw <= ow) { int n = (int)Math.Floor(ow / tw); if (n > 1) { spaceX = (ow - n * tw) / (n - 1); px = ox; } }
				if (layer.RepY == 2 && th <= oh) { int n = (int)Math.Floor(oh / th); if (n > 1) { spaceY = (oh - n * th) / (n - 1); py = oy; } }
				double stepX = tw + spaceX, stepY = th + spaceY;
				double x0 = px, y0 = py;
				bool rx = layer.RepX != 1, ry = layer.RepY != 1;
				if (rx) while (x0 > cx) x0 -= stepX;
				if (ry) while (y0 > cy) y0 -= stepY;
				double yTop = Math.Max(cy, _pageTop - th), yBot = Math.Min(cy + ch, _pageBottom + th);
				int count = 0;
				for (double ty = y0; ty < cy + ch && count < 20000; ty += stepY) {
					if (ty + th >= yTop && ty <= yBot) {
						for (double tx = x0; tx < cx + cw && count < 20000; tx += stepX) {
							if (tx + tw >= cx) {
								if (img != null) DrawImage(img, tx, ty, tw, th);
								else PaintGradient(layer.Grad, tx, ty, tw, th);
								count++;
							}
							if (!rx) break;
						}
					}
					if (!ry) break;
				}
				_c.Op("Q");
			}
		}

		public void DrawImage(ImageData img, double x, double y, double w, double h) {
			if (img.SvgRoot != null) {
				_c.Op("q");
				_c.Op(F.N(x) + " " + F.N(y) + " " + F.N(w) + " " + F.N(h) + " re W n");
				Svg.Render(this, img.SvgRoot, x, y, w, h, _ctx);
				_c.Op("Q");
				return;
			}
			var res = _pdf.Image(img);
			_c.Images.Add(res.Name);
			double a, bb, c, d, e, f;
			switch (img.Orientation) {
				case 2: a = -w; bb = 0; c = 0; d = -h; e = x + w; f = y + h; break;
				case 3: a = -w; bb = 0; c = 0; d = h; e = x + w; f = y; break;
				case 4: a = w; bb = 0; c = 0; d = h; e = x; f = y; break;
				case 5: a = 0; bb = h; c = w; d = 0; e = x; f = y; break;
				case 6: a = 0; bb = h; c = -w; d = 0; e = x + w; f = y; break;
				case 7: a = 0; bb = -h; c = -w; d = 0; e = x + w; f = y + h; break;
				case 8: a = 0; bb = -h; c = w; d = 0; e = x; f = y + h; break;
				default: a = w; bb = 0; c = 0; d = -h; e = x; f = y + h; break;
			}
			_c.Op("q " + F.N(a) + " " + F.N(bb) + " " + F.N(c) + " " + F.N(d) + " " + F.N(e) + " " + F.N(f) + " cm /" + res.Name + " Do Q");
		}

		public PdfContent Content => _c;
		public bool SnapText = true;

		static double Rn(double v) => Math.Round(v, MidpointRounding.AwayFromZero);

		static void Snap(ref double x, ref double y, ref double w, ref double h) {
			double x1 = Rn(x + w), y1 = Rn(y + h);
			x = Rn(x); y = Rn(y);
			w = x1 - x; h = y1 - y;
		}
		public PdfContent SetContent(PdfContent c) { var old = _c; _c = c; return old; }

		public void PaintSnippet(PdfContent c, Box root) {
			_c = c;
			_page = 0;
			_pageTop = double.NegativeInfinity;
			_pageBottom = double.PositiveInfinity;
			PaintWrapped(root, true);
		}
		public PdfDoc Doc => _pdf;

		public void PaintGradient(Gradient g, double x, double y, double w, double h) {
			if (g == null || w <= 0 || h <= 0) return;
			var stops = ResolveStops(g, x, y, w, h, out double length, out string coords, out bool radialMatrix, out double[] matrix);
			if (stops == null) return;
			if (g.Type == 2) { PaintConic(g, stops, x, y, w, h); return; }
			string fn = BuildFunction(stops, out bool anyAlpha);
			string shType = g.Type == 0 ? "2" : "3";
			string sh = _pdf.AddShading("<< /ShadingType " + shType + " /ColorSpace /DeviceRGB /Coords [" + coords + "] /Function " + fn + " /Extend [true true] >>");
			_c.Shadings.Add(sh);
			_c.Op("q");
			_c.Op(F.N(x) + " " + F.N(y) + " " + F.N(w) + " " + F.N(h) + " re W n");
			if (anyAlpha) {
				var mask = new PdfContent();
				string alphaFn = BuildAlphaFunction(stops);
				string ash = _pdf.AddShading("<< /ShadingType " + shType + " /ColorSpace /DeviceGray /Coords [" + coords + "] /Function " + alphaFn + " /Extend [true true] >>");
				mask.Shadings.Add(ash);
				if (radialMatrix) mask.Op(string.Join(" ", matrix.Select(F.N)) + " cm");
				mask.Op("/" + ash + " sh");
				string form = _pdf.AddForm(mask, -1e5, -1e6, 1e5, 1e7, true, true);
				string gs = _pdf.State(1, 1, null, _pdf.FormObj(form));
				_c.States.Add(gs);
				_c.Op("/" + gs + " gs");
			}
			if (radialMatrix) _c.Op(string.Join(" ", matrix.Select(F.N)) + " cm");
			_c.Op("/" + sh + " sh");
			_c.Op("Q");
		}

		void PaintConic(Gradient g, List<(double pos, Rgba c)> stops, double x, double y, double w, double h) {
			double cx = x + g.PX.Resolve(w), cy = y + g.PY.Resolve(h);
			double rad = Math.Sqrt(w * w + h * h) * 2;
			int n = 180;
			_c.Op("q " + F.N(x) + " " + F.N(y) + " " + F.N(w) + " " + F.N(h) + " re W n");
			for (int i = 0; i < n; i++) {
				double t0 = (double)i / n, t1 = (double)(i + 1) / n;
				var col = Sample(stops, (t0 + t1) / 2, g.Repeating);
				double a0 = (g.FromAngle + t0 * 360 - 90) * Math.PI / 180, a1 = (g.FromAngle + t1 * 360 - 90) * Math.PI / 180 + 0.004;
				Fill(col);
				_c.Op(F.N(cx) + " " + F.N(cy) + " m " + F.N(cx + rad * Math.Cos(a0)) + " " + F.N(cy + rad * Math.Sin(a0)) + " l " + F.N(cx + rad * Math.Cos(a1)) + " " + F.N(cy + rad * Math.Sin(a1)) + " l h f");
			}
			_c.Op("Q");
		}

		static Rgba Sample(List<(double pos, Rgba c)> stops, double t, bool repeating) {
			if (repeating && stops.Count > 1) {
				double s0 = stops[0].pos, s1 = stops[stops.Count - 1].pos, len = s1 - s0;
				if (len > 0) t = s0 + ((t - s0) % len + len) % len;
			}
			if (t <= stops[0].pos) return stops[0].c;
			for (int i = 1; i < stops.Count; i++) {
				if (t <= stops[i].pos) {
					double span = stops[i].pos - stops[i - 1].pos;
					double k = span <= 0 ? 1 : (t - stops[i - 1].pos) / span;
					var a = stops[i - 1].c; var b = stops[i].c;
					return new Rgba((int)(a.R + (b.R - a.R) * k), (int)(a.G + (b.G - a.G) * k), (int)(a.B + (b.B - a.B) * k), (int)(a.A + (b.A - a.A) * k));
				}
			}
			return stops[stops.Count - 1].c;
		}

		List<(double pos, Rgba c)> ResolveStops(Gradient g, double x, double y, double w, double h, out double length, out string coords, out bool useMatrix, out double[] matrix) {
			useMatrix = false; matrix = null;
			double x0, y0, x1, y1;
			double cxp = x + w / 2, cyp = y + h / 2;
			if (g.Type == 0) {
				double ang;
				if (g.ToCorner) ang = Math.Atan2(g.CornerX * h, -g.CornerY * w) * 180 / Math.PI;
				else ang = g.Angle;
				double rad = ang * Math.PI / 180;
				double sin = Math.Sin(rad), cos = Math.Cos(rad);
				length = Math.Abs(w * sin) + Math.Abs(h * cos);
				x0 = cxp - sin * length / 2; y0 = cyp + cos * length / 2;
				x1 = cxp + sin * length / 2; y1 = cyp - cos * length / 2;
				var st = NormalizeStops(g, length);
				if (st == null) { coords = null; return null; }
				ExpandRepeating(g, ref st);
				double p0 = st[0].pos, p1 = st[st.Count - 1].pos;
				double X(double p) => x0 + (x1 - x0) * p;
				double Y(double p) => y0 + (y1 - y0) * p;
				coords = F.N(X(p0)) + " " + F.N(Y(p0)) + " " + F.N(X(p1)) + " " + F.N(Y(p1));
				if (p1 - p0 < 1e-9) { coords = F.N(X(p0)) + " " + F.N(Y(p0)) + " " + F.N(X(p0) + 0.001) + " " + F.N(Y(p0)); p1 = p0 + 1e-6; }
				return st.Select(s => ((s.pos - p0) / (p1 - p0), s.c)).ToList();
			}
			double pcx = x + g.PX.Resolve(w), pcy = y + g.PY.Resolve(h);
			double rx, ry;
			double dl = pcx - x, dr = x + w - pcx, dt = pcy - y, db = y + h - pcy;
			if (g.ExplicitSize) { rx = g.RX.Resolve(w); ry = g.Circle ? rx : g.RY.Resolve(h); }
			else {
				switch (g.SizeKw) {
					case 0: rx = Math.Min(Math.Abs(dl), Math.Abs(dr)); ry = Math.Min(Math.Abs(dt), Math.Abs(db)); if (g.Circle) rx = ry = Math.Min(rx, ry); break;
					case 2: rx = Math.Max(Math.Abs(dl), Math.Abs(dr)); ry = Math.Max(Math.Abs(dt), Math.Abs(db)); if (g.Circle) rx = ry = Math.Max(rx, ry); break;
					case 1: {
						double cxd = Math.Min(Math.Abs(dl), Math.Abs(dr)), cyd = Math.Min(Math.Abs(dt), Math.Abs(db));
						if (g.Circle) { rx = ry = Math.Sqrt(cxd * cxd + cyd * cyd); }
						else { rx = cxd * Math.Sqrt(2); ry = cyd * Math.Sqrt(2); }
						break;
					}
					default: {
						double fxd = Math.Max(Math.Abs(dl), Math.Abs(dr)), fyd = Math.Max(Math.Abs(dt), Math.Abs(db));
						if (g.Circle) { rx = ry = Math.Sqrt(fxd * fxd + fyd * fyd); }
						else { rx = fxd * Math.Sqrt(2); ry = fyd * Math.Sqrt(2); }
						break;
					}
				}
			}
			rx = Math.Max(rx, 0.001); ry = Math.Max(ry, 0.001);
			length = rx;
			var st2 = NormalizeStops(g, rx);
			if (st2 == null) { coords = null; return null; }
			ExpandRepeating(g, ref st2);
			double q0 = Math.Max(0, st2[0].pos), q1 = st2[st2.Count - 1].pos;
			if (q1 <= q0) q1 = q0 + 1e-6;
			useMatrix = true;
			matrix = new double[] { 1, 0, 0, ry / rx, 0, pcy - pcy * ry / rx };
			coords = F.N(pcx) + " " + F.N(pcy) + " " + F.N(q0 * rx) + " " + F.N(pcx) + " " + F.N(pcy) + " " + F.N(q1 * rx);
			return st2.Select(s => ((s.pos - q0) / (q1 - q0), s.c)).ToList();
		}

		static void ExpandRepeating(Gradient g, ref List<(double pos, Rgba c)> st) {
			if (!g.Repeating || st.Count < 2) return;
			double s0 = st[0].pos, s1 = st[st.Count - 1].pos, len = s1 - s0;
			if (len <= 1e-6) return;
			var res = new List<(double, Rgba)>();
			double start = s0 - Math.Ceiling(s0 / len + 1e-9) * len;
			if (start > 0) start -= len;
			int guard = 0;
			for (double off = start - s0; off + s0 < 1 + len && guard < 400; off += len, guard++) foreach (var s in st) res.Add((s.pos + off, s.c));
			st = res.Where(r => r.Item1 >= -len && r.Item1 <= 1 + len).ToList();
			if (st.Count < 2) st = res;
		}

		static List<(double pos, Rgba c)> NormalizeStops(Gradient g, double length) {
			var raw = g.Stops;
			var list = new List<(double? pos, Rgba c, bool hint)>();
			foreach (var s in raw) {
				double? p = null;
				if (s.Pos != null) p = length > 0 ? s.Pos.Value.Resolve(length) / length : 0;
				if (g.Type == 2 && s.Pos != null) p = s.Pos.Value.Pct / 100;
				list.Add((p, s.Color, s.Hint));
			}
			var colorStops = list.Where(x => !x.hint).ToList();
			if (colorStops.Count == 0) return null;
			if (colorStops[0].pos == null) colorStops[0] = (0, colorStops[0].c, false);
			if (colorStops[colorStops.Count - 1].pos == null) colorStops[colorStops.Count - 1] = (1, colorStops[colorStops.Count - 1].c, false);
			double maxSoFar = double.NegativeInfinity;
			for (int i = 0; i < colorStops.Count; i++) {
				if (colorStops[i].pos != null) {
					double v = Math.Max(colorStops[i].pos.Value, maxSoFar);
					colorStops[i] = (v, colorStops[i].c, false);
					maxSoFar = v;
				}
			}
			for (int i = 0; i < colorStops.Count; i++) {
				if (colorStops[i].pos != null) continue;
				int j = i;
				while (colorStops[j].pos == null) j++;
				double a = colorStops[i - 1].pos.Value, b = colorStops[j].pos.Value;
				for (int k = i; k < j; k++) colorStops[k] = (a + (b - a) * (k - i + 1) / (j - i + 1), colorStops[k].c, false);
				i = j;
			}
			return colorStops.Select(x => (x.pos.Value, x.c)).ToList();
		}

		string BuildFunction(List<(double pos, Rgba c)> stops, out bool anyAlpha) {
			anyAlpha = stops.Any(s => s.c.A < 255);
			return BuildFn(stops, c => F.N(c.R / 255.0) + " " + F.N(c.G / 255.0) + " " + F.N(c.B / 255.0));
		}

		string BuildAlphaFunction(List<(double pos, Rgba c)> stops) => BuildFn(stops, c => F.N(c.A / 255.0));

		string BuildFn(List<(double pos, Rgba c)> stops, Func<Rgba, string> col) {
			if (stops.Count == 1) stops = new List<(double, Rgba)> { (0, stops[0].c), (1, stops[0].c) };
			var fns = new List<int>();
			var bounds = new List<string>();
			var encode = new List<string>();
			var pts = stops.ToList();
			if (pts[0].pos > 0) pts.Insert(0, (0, pts[0].c));
			if (pts[pts.Count - 1].pos < 1) pts.Add((1, pts[pts.Count - 1].c));
			for (int i = 0; i + 1 < pts.Count; i++) {
				fns.Add(_pdf.AddFunction("<< /FunctionType 2 /Domain [0 1] /C0 [" + col(pts[i].c) + "] /C1 [" + col(pts[i + 1].c) + "] /N 1 >>"));
				if (i > 0) bounds.Add(F.N(Math.Clamp(pts[i].pos, 0, 1)));
				encode.Add("0 1");
			}
			if (fns.Count == 1) return fns[0] + " 0 R";
			for (int i = 1; i < bounds.Count; i++) if (double.Parse(bounds[i], CultureInfo.InvariantCulture) < double.Parse(bounds[i - 1], CultureInfo.InvariantCulture)) bounds[i] = bounds[i - 1];
			return "<< /FunctionType 3 /Domain [0 1] /Functions [" + string.Join(" ", fns.Select(f => f + " 0 R")) + "] /Bounds [" + string.Join(" ", bounds) + "] /Encode [" + string.Join(" ", encode) + "] >>";
		}

		void PaintShadows(Box b, double x, double y, double w, double h, double[] r, bool inset) {
			var list = b.S.BoxShadow;
			if (list == null) return;
			for (int i = list.Count - 1; i >= 0; i--) {
				var sh = list[i];
				if (sh.Inset != inset || sh.Color.A == 0) continue;
				if (!inset) {
					_c.Op("q");
					_c.Op("-100000 -1000000 200000 2000000 re");
					PathRoundRect(x, y, w, h, r);
					_c.Op("W* n");
					BlurredRect(x + sh.X, y + sh.Y, w, h, r, sh.Spread, sh.Blur, sh.Color, false, 0, 0, 0, 0, null);
					_c.Op("Q");
				}
				else {
					double px = x + b.Bd[3], py = y + b.Bd[0], pw = w - b.Bd[1] - b.Bd[3], ph = h - b.Bd[0] - b.Bd[2];
					var pr = InnerRadii(r, b.Bd);
					_c.Op("q");
					PathRoundRect(px, py, pw, ph, pr);
					_c.Op("W n");
					BlurredRect(px + sh.X, py + sh.Y, pw, ph, pr, -sh.Spread, sh.Blur, sh.Color, true, px, py, pw, ph, pr);
					_c.Op("Q");
				}
			}
		}

		void BlurredRect(double x, double y, double w, double h, double[] r, double spread, double blur, Rgba color, bool inset, double px, double py, double pw, double ph, double[] pr) {
			double ex = x - spread, ey = y - spread, ew = w + 2 * spread, eh = h + 2 * spread;
			var er = r.Select(v => v > 0 ? Math.Max(0, v + spread) : 0).ToArray();
			if (blur <= 0.01) {
				Fill(color);
				if (!inset) { if (ew > 0 && eh > 0) { PathRoundRect(ex, ey, ew, eh, er); _c.Op("f"); } }
				else {
					PathRoundRect(px - 1000, py - 1000, pw + 2000, ph + 2000, null);
					if (ew > 0 && eh > 0) PathRoundRect(ex, ey, ew, eh, er);
					_c.Op("f*");
				}
				if (color.A < 255) ResetAlpha();
				return;
			}
			double sigma = blur / 2;
			double pad = Math.Ceiling(sigma * 3);
			double ox, oy, ow, oh;
			if (!inset) { ox = ex - pad; oy = ey - pad; ow = Math.Max(0, ew) + 2 * pad; oh = Math.Max(0, eh) + 2 * pad; }
			else { ox = px; oy = py; ow = pw; oh = ph; }
			if (ow <= 0 || oh <= 0) return;
			double scale = Math.Min(3.125, Math.Sqrt(4e6 / Math.Max(1, ow * oh)));
			int W = Math.Max(1, (int)Math.Ceiling(ow * scale)), H = Math.Max(1, (int)Math.Ceiling(oh * scale));
			var mask = new float[W * H];
			for (int j = 0; j < H; j++) {
				double cy = oy + (j + 0.5) / scale;
				for (int i = 0; i < W; i++) {
					double cx = ox + (i + 0.5) / scale;
					double cov = RoundRectCoverage(cx, cy, ex, ey, ew, eh, er, 1 / scale);
					mask[j * W + i] = (float)(inset ? 1 - cov : cov);
				}
			}
			double s = sigma * scale;
			BoxBlur(mask, W, H, s, inset ? 1f : 0f);
			var alpha = new byte[W * H];
			for (int k = 0; k < mask.Length; k++) alpha[k] = (byte)Math.Round(Math.Clamp(mask[k], 0, 1) * 255 * color.A / 255.0);
			var img = new ImageData { PixW = W, PixH = H, W = ow, H = oh, Rgb = new byte[W * H * 3], Alpha = alpha };
			for (int k = 0; k < W * H; k++) { img.Rgb[k * 3] = color.R; img.Rgb[k * 3 + 1] = color.G; img.Rgb[k * 3 + 2] = color.B; }
			DrawImage(img, ox, oy, ow, oh);
		}

		static double RoundRectCoverage(double px, double py, double x, double y, double w, double h, double[] r, double pix) {
			if (w <= 0 || h <= 0) return 0;
			double dx = Math.Min(px - x, x + w - px), dy = Math.Min(py - y, y + h - py);
			double edge = Math.Min(dx, dy);
			if (edge < -pix) return 0;
			double cov = Math.Clamp(edge / pix + 0.5, 0, 1);
			bool left = px < x + w / 2, top = py < y + h / 2;
			double rx = top ? (left ? r[0] : r[2]) : (left ? r[6] : r[4]);
			double ry = top ? (left ? r[1] : r[3]) : (left ? r[7] : r[5]);
			if (rx > 0 && ry > 0) {
				double ccx = left ? x + rx : x + w - rx, ccy = top ? y + ry : y + h - ry;
				bool inCorner = (left ? px < ccx : px > ccx) && (top ? py < ccy : py > ccy);
				if (inCorner) {
					double nx = (px - ccx) / rx, ny = (py - ccy) / ry;
					double dist = (Math.Sqrt(nx * nx + ny * ny) - 1) * Math.Min(rx, ry);
					cov = Math.Clamp(0.5 - dist / pix, 0, 1);
				}
			}
			return cov;
		}

		static void BoxBlur(float[] m, int w, int h, double sigma, float outside) {
			if (sigma < 0.3) return;
			int n = 3;
			double wIdeal = Math.Sqrt(12 * sigma * sigma / n + 1);
			int wl = (int)Math.Floor(wIdeal);
			if (wl % 2 == 0) wl--;
			int wu = wl + 2;
			double mIdeal = (12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
			int mm = (int)Math.Round(mIdeal);
			var tmp = new float[m.Length];
			for (int pass = 0; pass < n; pass++) {
				int rad = ((pass < mm ? wl : wu) - 1) / 2;
				if (rad < 1) continue;
				BlurH(m, tmp, w, h, rad, outside);
				BlurV(tmp, m, w, h, rad, outside);
			}
		}

		static void BlurH(float[] src, float[] dst, int w, int h, int r, float outside) {
			float inv = 1f / (r + r + 1);
			for (int y = 0; y < h; y++) {
				int row = y * w;
				float acc = outside * r;
				for (int x = 0; x <= r; x++) acc += x < w ? src[row + x] : outside;
				for (int x = 0; x < w; x++) {
					dst[row + x] = acc * inv;
					int add = x + r + 1, sub = x - r;
					acc += (add < w ? src[row + add] : outside) - (sub >= 0 ? src[row + sub] : outside);
				}
			}
		}

		static void BlurV(float[] src, float[] dst, int w, int h, int r, float outside) {
			float inv = 1f / (r + r + 1);
			for (int x = 0; x < w; x++) {
				float acc = outside * r;
				for (int y = 0; y <= r; y++) acc += y < h ? src[y * w + x] : outside;
				for (int y = 0; y < h; y++) {
					dst[y * w + x] = acc * inv;
					int add = y + r + 1, sub = y - r;
					acc += (add < h ? src[add * w + x] : outside) - (sub >= 0 ? src[sub * w + x] : outside);
				}
			}
		}

		void PaintBorders(Box b, double x, double y, double w, double h) {
			var s = b.S;
			var bw = new double[4];
			var st = new BS[4];
			var co = new Rgba[4];
			bool any = false;
			for (int i = 0; i < 4; i++) {
				bw[i] = s.BorderStyle[i] == BS.None || s.BorderStyle[i] == BS.Hidden ? 0 : s.BorderWidth[i];
				st[i] = s.BorderStyle[i];
				co[i] = s.BorderColor[i];
				if (bw[i] > 0 && co[i].A > 0) any = true;
			}
			if (!any) return;
			var r = Radii(b, x, y, w, h);
			DrawBorder(x, y, w, h, bw, st, co, r);
		}

		public void DrawBorder(double x, double y, double w, double h, double[] bw, BS[] st, Rgba[] co, double[] r) {
			bool uniform = st.All(v => v == st[0]) && co.All(v => v.Equals(co[0])) && (st[0] == BS.Solid || st[0] == BS.Double && bw.All(v => v == bw[0]));
			if (uniform && st[0] == BS.Solid) {
				Fill(co[0]);
				PathRoundRect(x, y, w, h, r);
				PathRoundRect(x + bw[3], y + bw[0], w - bw[1] - bw[3], h - bw[0] - bw[2], InnerRadii(r, bw));
				_c.Op("f*");
				if (co[0].A < 255) ResetAlpha();
				return;
			}
			for (int side = 0; side < 4; side++) {
				if (bw[side] <= 0 || st[side] == BS.None || st[side] == BS.Hidden || co[side].A == 0) continue;
				_c.Op("q");
				double cx = x + w / 2, cy = y + h / 2;
				double ix0 = x + bw[3], iy0 = y + bw[0], ix1 = x + w - bw[1], iy1 = y + h - bw[2];
				string trap = side switch {
					0 => P(x, y) + " m " + P(x + w, y) + " l " + P(ix1, iy0) + " l " + P(ix0, iy0) + " l h",
					1 => P(x + w, y) + " m " + P(x + w, y + h) + " l " + P(ix1, iy1) + " l " + P(ix1, iy0) + " l h",
					2 => P(x + w, y + h) + " m " + P(x, y + h) + " l " + P(ix0, iy1) + " l " + P(ix1, iy1) + " l h",
					_ => P(x, y + h) + " m " + P(x, y) + " l " + P(ix0, iy0) + " l " + P(ix0, iy1) + " l h"
				};
				_c.Op(trap + " W n");
				_c.Op("q");
				PathRoundRect(x, y, w, h, r);
				_c.Op("W n");
				PaintSide(side, x, y, w, h, bw, st[side], co[side], r);
				_c.Op("Q");
				_c.Op("Q");
			}
		}

		static string P(double x, double y) => F.N(x) + " " + F.N(y);

		static double BestDashGap(double length, double dash, double gap) {
			double avail = length + gap;
			double minN = Math.Floor(avail / (dash + gap)), maxN = minN + 1;
			double minGaps = minN - 1, maxGaps = maxN - 1;
			double minGap = minGaps > 0 ? (length - minN * dash) / minGaps : double.PositiveInfinity;
			double maxGap = maxGaps > 0 ? (length - maxN * dash) / maxGaps : double.PositiveInfinity;
			return maxGap <= 0 || Math.Abs(minGap - gap) < Math.Abs(maxGap - gap) ? minGap : maxGap;
		}

		void PaintSide(int side, double x, double y, double w, double h, double[] bw, BS style, Rgba color, double[] r) {
			double t = bw[side];
			void Band(double inset0, double inset1, Rgba c) {
				Fill(c);
				var o = new[] { bw[0] * inset0, bw[1] * inset0, bw[2] * inset0, bw[3] * inset0 };
				var i = new[] { bw[0] * inset1, bw[1] * inset1, bw[2] * inset1, bw[3] * inset1 };
				PathRoundRect(x + o[3], y + o[0], w - o[1] - o[3], h - o[0] - o[2], InnerRadii(r, o));
				PathRoundRect(x + i[3], y + i[0], w - i[1] - i[3], h - i[0] - i[2], InnerRadii(r, i));
				_c.Op("f*");
			}
			bool topLeft = side == 0 || side == 3;
			switch (style) {
				case BS.Solid: Band(0, 1, color); break;
				case BS.Double: {
					int third = (int)((t + 1) / 3);
					if (third < 1) { Band(0, 1, color); break; }
					double f = third / t;
					Band(0, f, color);
					Band(1 - f, 1, color);
					break;
				}
				case BS.Inset: Band(0, 1, color.BorderStyleColor(topLeft)); break;
				case BS.Outset: Band(0, 1, color.BorderStyleColor(!topLeft)); break;
				case BS.Groove: case BS.Ridge: {
					bool groove = style == BS.Groove;
					Band(0, 0.5, color.BorderStyleColor(groove ? topLeft : !topLeft));
					Band(0.5, 1, color.BorderStyleColor(groove ? !topLeft : topLeft));
					break;
				}
				case BS.Dashed: case BS.Dotted: {
					bool dotted = style == BS.Dotted;
					Stroke(color);
					double len = side == 0 || side == 2 ? w : h;
					double half = t / 2;
					var sb = new StringBuilder();
					sb.Append(F.N(t)).Append(" w ");
					double sx0, sy0, sx1, sy1;
					switch (side) {
						case 0: sx0 = x; sy0 = y + half; sx1 = x + w; sy1 = sy0; break;
						case 1: sx0 = x + w - half; sy0 = y; sx1 = sx0; sy1 = y + h; break;
						case 2: sx0 = x; sy0 = y + h - half; sx1 = x + w; sy1 = sy0; break;
						default: sx0 = x + half; sy0 = y; sx1 = sx0; sy1 = y + h; break;
					}
					if (dotted) {
						double gap = BestDashGap(len, t, t);
						if (double.IsInfinity(gap) || gap < 0) gap = t;
						bool horiz = side == 0 || side == 2;
						if (horiz) { sx0 += half; sx1 -= half; } else { sy0 += half; sy1 -= half; }
						if (t > 3) sb.Append("1 J [0 ").Append(F.N(t + gap)).Append("] 0 d ");
						else sb.Append("0 J [").Append(F.N(t)).Append(' ').Append(F.N(gap)).Append("] ").Append(F.N(half)).Append(" d ");
					}
					else {
						double dash = t * (t >= 3 ? 2 : 3), gapLen = t * (t >= 3 ? 1 : 2);
						if (dash < len) {
							double gap = BestDashGap(len, dash, gapLen);
							if (double.IsInfinity(gap) || gap < 0) gap = gapLen;
							sb.Append("0 J [").Append(F.N(dash)).Append(' ').Append(F.N(gap)).Append("] 0 d ");
						}
					}
					sb.Append(P(sx0, sy0)).Append(" m ").Append(P(sx1, sy1)).Append(" l S [] 0 d 0 J");
					if (r.Any(v => v > 0)) {
						var mid = new[] { bw[0] / 2, bw[1] / 2, bw[2] / 2, bw[3] / 2 };
						_c.Op(F.N(t) + " w " + (dotted ? "1 J [0 " + F.N(t * 2) + "] 0 d" : "[" + F.N(t * 2) + " " + F.N(t) + "] 0 d"));
						PathRoundRect(x + mid[3], y + mid[0], w - mid[1] - mid[3], h - mid[0] - mid[2], InnerRadii(r, mid));
						_c.Op("S [] 0 d 0 J");
					}
					else _c.Op(sb.ToString());
					if (color.A < 255) ResetAlpha();
					break;
				}
			}
		}

		void PaintCollapsedBorders(Box cell) {
			var cb = cell.CB;
			double x = cell.X, y = cell.Y, w = cell.W, h = cell.H;
			for (int side = 0; side < 4; side++) {
				var (bw, st, co) = cb[side];
				if (bw <= 0 || co.A == 0) continue;
				double half = bw / 2;
				double rx, ry, rw, rh;
				switch (side) {
					case 0: rx = x - cb[3].w / 2; ry = y - half; rw = w + cb[3].w / 2 + cb[1].w / 2; rh = bw; break;
					case 1: rx = x + w - half; ry = y - cb[0].w / 2; rw = bw; rh = h + cb[0].w / 2 + cb[2].w / 2; break;
					case 2: rx = x - cb[3].w / 2; ry = y + h - half; rw = w + cb[3].w / 2 + cb[1].w / 2; rh = bw; break;
					default: rx = x - half; ry = y - cb[0].w / 2; rw = bw; rh = h + cb[0].w / 2 + cb[2].w / 2; break;
				}
				Snap(ref rx, ref ry, ref rw, ref rh);
				if (st == BS.Solid || st == BS.Inset || st == BS.Outset || st == BS.Groove || st == BS.Ridge) {
					bool tl = side == 0 || side == 3;
					Fill(st == BS.Inset ? co.BorderStyleColor(tl) : st == BS.Outset ? co.BorderStyleColor(!tl) : co);
					_c.Op(F.N(rx) + " " + F.N(ry) + " " + F.N(rw) + " " + F.N(rh) + " re f");
				}
				else {
					_c.Op("q " + F.N(rx) + " " + F.N(ry) + " " + F.N(rw) + " " + F.N(rh) + " re W n");
					var bws = new double[4]; bws[side] = bw;
					PaintSide(side, side == 1 ? rx + rw - Math.Max(rw, 1) : rx, side == 2 ? ry + rh - Math.Max(rh, 1) : ry, side == 0 || side == 2 ? rw : Math.Max(rw, 1), side == 1 || side == 3 ? rh : Math.Max(rh, 1), bws, st, co, new double[8]);
					_c.Op("Q");
				}
				if (co.A < 255) ResetAlpha();
			}
		}

		void PaintOutline(Box b, double x, double y, double w, double h) {
			var s = b.S;
			double o = s.OutlineOffset + s.OutlineWidth;
			var bw = new[] { s.OutlineWidth, s.OutlineWidth, s.OutlineWidth, s.OutlineWidth };
			var st = new[] { s.OutlineStyle, s.OutlineStyle, s.OutlineStyle, s.OutlineStyle };
			var co = new[] { s.OutlineColor, s.OutlineColor, s.OutlineColor, s.OutlineColor };
			var r = Radii(b, x, y, w, h).Select(v => v > 0 ? v + o : 0).ToArray();
			DrawBorder(x - o, y - o, w + 2 * o, h + 2 * o, bw, st, co, r);
		}

		void PaintReplaced(Box b) {
			var s = b.S;
			double cx = b.ContentX, cy = b.ContentY, cw = b.ContentW, ch = b.ContentHgt;
			Snap(ref cx, ref cy, ref cw, ref ch);
			if (b.Control != 0) { PaintControl(b, cx, cy, cw, ch); return; }
			if (b.SvgEl != null) {
				_c.Op("q");
				_c.Op(F.N(cx) + " " + F.N(cy) + " " + F.N(cw) + " " + F.N(ch) + " re W n");
				Svg.Render(this, b.SvgEl, cx, cy, cw, ch, _ctx);
				_c.Op("Q");
				return;
			}
			var img = b.Img;
			if (img == null || cw <= 0 || ch <= 0) return;
			double iw = img.W, ih = img.H;
			double dw = cw, dh = ch;
			switch (s.ObjectFit) {
				case 1: { double sc = Math.Min(cw / iw, ch / ih); dw = iw * sc; dh = ih * sc; break; }
				case 2: { double sc = Math.Max(cw / iw, ch / ih); dw = iw * sc; dh = ih * sc; break; }
				case 3: dw = iw; dh = ih; break;
				case 4: { double sc = Math.Min(1, Math.Min(cw / iw, ch / ih)); dw = iw * sc; dh = ih * sc; break; }
			}
			double dx = cx + s.ObjectPosX.Px + (s.ObjectPosX.HasPct ? s.ObjectPosX.Pct / 100 * (cw - dw) : 0);
			double dy = cy + s.ObjectPosY.Px + (s.ObjectPosY.HasPct ? s.ObjectPosY.Pct / 100 * (ch - dh) : 0);
			bool clip = dw > cw + 0.01 || dh > ch + 0.01 || dx < cx - 0.01 || dy < cy - 0.01 || b.S.RadiusH.Any(v => v.Resolve(b.W) > 0);
			if (clip) {
				_c.Op("q");
				var r = Radii(b, b.X, b.Y, b.W, b.H);
				var ir = InnerRadii(InnerRadii(r, b.Bd), b.P);
				PathRoundRect(cx, cy, cw, ch, ir);
				_c.Op("W n");
			}
			DrawImage(img, dx, dy, dw, dh);
			if (clip) _c.Op("Q");
		}

		void PaintControl(Box b, double x, double y, double w, double h) {
			var el = b.El;
			var s = b.S;
			if (b.Control == 1 || b.Control == 2) {
				bool on = el?.Attr("checked") != null;
				if (!s.BorderWidth.Any(v => v > 0) && s.BackgroundColor.A == 0) {
					Stroke(new Rgba(118, 118, 118));
					_c.Op("1 w");
					if (b.Control == 1) { PathRoundRect(x + 0.5, y + 0.5, w - 1, h - 1, Enumerable.Repeat(2.0, 8).ToArray()); _c.Op("S"); }
					else { PathRoundRect(x + 0.5, y + 0.5, w - 1, h - 1, Enumerable.Repeat(w / 2, 8).ToArray()); _c.Op("S"); }
				}
				if (on) {
					if (b.Control == 1) {
						Fill(new Rgba(0, 117, 255));
						PathRoundRect(x, y, w, h, Enumerable.Repeat(2.0, 8).ToArray());
						_c.Op("f");
						Stroke(Rgba.White);
						_c.Op(F.N(Math.Max(1.5, w / 7)) + " w 1 J 1 j " + P(x + w * 0.22, y + h * 0.52) + " m " + P(x + w * 0.42, y + h * 0.72) + " l " + P(x + w * 0.78, y + h * 0.3) + " l S");
					}
					else {
						Fill(new Rgba(0, 117, 255));
						PathRoundRect(x, y, w, h, Enumerable.Repeat(w / 2, 8).ToArray());
						_c.Op("f");
						Fill(Rgba.White);
						double r = w * 0.3;
						PathRoundRect(x + w / 2 - r, y + h / 2 - r, 2 * r, 2 * r, Enumerable.Repeat(r, 8).ToArray());
						_c.Op("f");
					}
				}
				return;
			}
			if (b.Control == 3) {
				double max = 1, val = 0;
				if (el != null) {
					double.TryParse(el.Attr("max") ?? "1", NumberStyles.Float, CultureInfo.InvariantCulture, out max);
					double.TryParse(el.Attr("value") ?? "0", NumberStyles.Float, CultureInfo.InvariantCulture, out val);
					if (el.Tag == "meter") { double.TryParse(el.Attr("min") ?? "0", NumberStyles.Float, CultureInfo.InvariantCulture, out double min); max -= min; val -= min; }
				}
				if (max <= 0) max = 1;
				double frac = Math.Clamp(val / max, 0, 1);
				var rr = Enumerable.Repeat(Math.Min(h / 2, 4), 8).ToArray();
				Fill(new Rgba(238, 238, 238));
				PathRoundRect(x, y, w, h, rr);
				_c.Op("f");
				Fill(el?.Tag == "meter" ? new Rgba(16, 124, 16) : new Rgba(0, 117, 255));
				_c.Op("q");
				PathRoundRect(x, y, w, h, rr);
				_c.Op("W n");
				_c.Op(F.N(s.Rtl ? x + w * (1 - frac) : x) + " " + F.N(y) + " " + F.N(w * frac) + " " + F.N(h) + " re f Q");
			}
		}

		void PaintMarker(Box li) {
			if (li.S.Visibility != 0) return;
			if (li.MarkerImg != null && li.MarkerImgW > 0) { DrawImage(li.MarkerImg, li.MarkerImgX, li.MarkerImgY, li.MarkerImgW, li.MarkerImgH); return; }
			if (li.MarkerShape != 0) {
				double x = li.MarkerSX, y = Rn(li.MarkerSY), w = li.MarkerSW;
				var r = new[] { w / 2, w / 2, w / 2, w / 2, w / 2, w / 2, w / 2, w / 2 };
				if (li.MarkerShape == 1) { Fill(li.MarkerColor); PathRoundRect(x, y, w, w, r); _c.Op("f"); }
				else if (li.MarkerShape == 2) { Stroke(li.MarkerColor); _c.Op("1 w"); PathRoundRect(x, y, w, w, r); _c.Op("S"); }
				else { Fill(li.MarkerColor); _c.Op(F.N(x) + " " + F.N(y) + " " + F.N(w) + " " + F.N(w) + " re f"); }
				if (li.MarkerColor.A < 255) ResetAlpha();
				return;
			}
			if (li.MarkerFrags == null) return;
			foreach (var g in li.MarkerFrags) PaintGlyphs(g, false);
		}

		void PaintLines(Box b) {
			if (b.Lines == null) return;
			foreach (var line in b.Lines) {
				if (line.Y >= _pageBottom - 0.01 || line.Y + line.H <= _pageTop + 0.01) {
					foreach (var a in line.Atomics) if (Visible(a)) PaintWrapped(a, false);
					continue;
				}
				foreach (var bf in line.Boxes) PaintInlineBox(bf);
				foreach (var g in line.Glyphs) {
					if (g.Hidden) continue;
					if (g.S.TextShadow != null) foreach (var sh in Enumerable.Reverse(g.S.TextShadow)) PaintGlyphs(g, false, sh);
				}
				foreach (var g in line.Glyphs) {
					if (g.Hidden) continue;
					PaintDecorations(g, true);
					PaintGlyphs(g, false);
					PaintDecorations(g, false);
				}
				foreach (var a in line.Atomics) PaintWrapped(a, false);
			}
		}

		void PaintInlineBox(InlineBoxFrag bf) {
			var b = bf.Box;
			var s = b.S;
			if (b.Href != null) { AddLink(b, bf.X, bf.Top + s.BorderWidth[0] + s.Padding[0].Resolve(0), bf.X + bf.W, bf.Bottom - s.BorderWidth[2] - s.Padding[2].Resolve(0)); _linked.Add(b); }
			RecordAnchor(b);
			if (s.Visibility != 0) return;
			bool hasBg = PrintBg(s) && (s.BackgroundColor.A > 0 || s.Backgrounds != null || s.BoxShadow != null);
			bool hasBorder = s.BorderWidth.Any(v => v > 0);
			if (!hasBg && !hasBorder && s.OutlineWidth == 0) return;
			double x = bf.X, y = bf.Top, w = bf.W, h = bf.Bottom - bf.Top;
			Snap(ref x, ref y, ref w, ref h);
			bool rtl = s.Rtl;
			var bw = (double[])s.BorderWidth.Clone();
			var st = (BS[])s.BorderStyle.Clone();
			if (!bf.First) { int side = rtl ? 1 : 3; bw[side] = 0; }
			if (!bf.Last) { int side = rtl ? 3 : 1; bw[side] = 0; }
			var r = new double[8];
			for (int i = 0; i < 4; i++) { r[i * 2] = s.RadiusH[i].Resolve(w); r[i * 2 + 1] = s.RadiusV[i].Resolve(h); }
			if (!bf.First) { if (rtl) { r[2] = r[3] = r[4] = r[5] = 0; } else { r[0] = r[1] = r[6] = r[7] = 0; } }
			if (!bf.Last) { if (rtl) { r[0] = r[1] = r[6] = r[7] = 0; } else { r[2] = r[3] = r[4] = r[5] = 0; } }
			if (hasBg) {
				if (s.BackgroundColor.A > 0) { Fill(s.BackgroundColor); PathRoundRect(x, y, w, h, r); _c.Op("f"); if (s.BackgroundColor.A < 255) ResetAlpha(); }
				if (s.Backgrounds != null) { _c.Op("q"); var tmp = new Box(BK.Block, null, s); tmp.Bd = bw; tmp.P = new double[4]; PaintBgLayers(tmp, s, x, y, w, h, r, false); _c.Op("Q"); }
			}
			if (hasBorder) DrawBorder(x, y, w, h, bw, st, s.BorderColor, r);
		}

		public void PaintGlyphs(GlyphFrag g, bool clipText, Shadow shadow = null) {
			var rf = g.RF;
			var font = _pdf.Font(rf.File);
			_c.Fonts.Add(font.Name);
			var color = shadow != null ? shadow.Color : g.Color;
			if (!PrintBg(g.S) && shadow == null) {
				int dr = 255 - color.R, dg = 255 - color.G, db = 255 - color.B;
				if (dr * dr + dg * dg + db * db <= 65025) color = color.Dark();
			}
			if (color.A == 0) return;
			double sx = shadow?.X ?? 0, sy = shadow?.Y ?? 0;
			if (_clipStack.Count > 0 && _transformDepth == 0) {
				double cx0 = double.NegativeInfinity, cx1 = double.PositiveInfinity, cy0 = double.NegativeInfinity, cy1 = double.PositiveInfinity;
				foreach (var r in _clipStack) { cx0 = Math.Max(cx0, r.x0); cx1 = Math.Min(cx1, r.x1); cy0 = Math.Max(cy0, r.y0); cy1 = Math.Min(cy1, r.y1); }
				var keep = new List<int>();
				for (int i = 0; i < g.G.Length; i++) {
					double gx = g.X[i] + sx, adv = rf.File.AdvanceUnits(g.G[i]) * rf.Size / rf.File.UnitsPerEm;
					if (gx >= cx1 || gx + adv <= cx0 || g.Y[i] + sy - rf.Asc >= cy1 || g.Y[i] + sy + rf.Desc <= cy0) continue;
					keep.Add(i);
				}
				if (keep.Count == 0) return;
				if (keep.Count < g.G.Length) g = new GlyphFrag { RF = g.RF, G = keep.Select(i => g.G[i]).ToArray(), X = keep.Select(i => g.X[i]).ToArray(), Y = keep.Select(i => g.Y[i]).ToArray(), U = keep.Select(i => g.U[i]).ToArray(), Color = g.Color, S = g.S, Baseline = g.Baseline, X0 = g.X0, X1 = g.X1, AnchorTop = g.AnchorTop };
			}
			if (SnapText && !double.IsNaN(g.AnchorTop)) sy += Math.Round(g.AnchorTop, MidpointRounding.AwayFromZero) - g.AnchorTop;
			else if (SnapText) sy += Math.Round(g.Baseline, MidpointRounding.AwayFromZero) - g.Baseline;
			for (int i = 0; i < g.G.Length; i++) {
				font.Used.Add(g.G[i]);
				if (g.U[i] != null && g.U[i].Length > 0 && !font.Uni.ContainsKey(g.G[i])) font.Uni[g.G[i]] = g.U[i];
			}
			var sb = new StringBuilder();
			sb.Append("BT\n");
			sb.Append(F.N(color.R / 255.0)).Append(' ').Append(F.N(color.G / 255.0)).Append(' ').Append(F.N(color.B / 255.0)).Append(" rg\n");
			if (color.A < 255) { string gs = _pdf.State(color.A / 255.0, color.A / 255.0); _c.States.Add(gs); sb.Append('/').Append(gs).Append(" gs\n"); }
			double size = rf.Size;
			if (rf.SynthBold) {
				double ratio = size <= 9 ? 1.0 / 24 : size >= 36 ? 1.0 / 32 : 1.0 / 24 + (1.0 / 32 - 1.0 / 24) * (size - 9) / 27;
				sb.Append(F.N(color.R / 255.0)).Append(' ').Append(F.N(color.G / 255.0)).Append(' ').Append(F.N(color.B / 255.0)).Append(" RG ").Append(F.N(size * ratio)).Append(" w 2 Tr\n");
			}
			sb.Append('/').Append(font.Name).Append(' ').Append(F.N(size)).Append(" Tf\n");
			double skew = rf.SynthItalic ? 0.25 : 0;
			int i0 = 0;
			while (i0 < g.G.Length) {
				double y0 = g.Y[i0];
				int i1 = i0 + 1;
				while (i1 < g.G.Length && Math.Abs(g.Y[i1] - y0) < 0.001 && g.X[i1] >= g.X[i1 - 1] - 0.001) i1++;
				sb.Append("1 0 ").Append(F.N(skew)).Append(" -1 ").Append(F.N(g.X[i0] + sx)).Append(' ').Append(F.N(y0 + sy)).Append(" Tm\n[");
				for (int i = i0; i < i1; i++) {
					sb.Append('<').Append(g.G[i].ToString("X4")).Append('>');
					if (i + 1 < i1) {
						double adv = rf.File.AdvanceUnits(g.G[i]) * size / rf.File.UnitsPerEm;
						double want = g.X[i + 1] - g.X[i];
						double adj = (adv - want) * 1000 / size;
						if (Math.Abs(adj) > 0.01) sb.Append(F.N(adj));
					}
				}
				sb.Append("] TJ\n");
				i0 = i1;
			}
			if (rf.SynthBold) sb.Append("0 Tr\n");
			sb.Append("ET");
			_c.Op(sb.ToString());
			if (color.A < 255) ResetAlpha();
		}

		void PaintDecorations(GlyphFrag g, bool under) {
			if (g.Decos == null || g.Decos.Count == 0) return;
			var rf = g.RF;
			var f = rf.File;
			double size = rf.Size;
			foreach (var d in g.Decos) {
				byte lines = d.Lines;
				if (lines == 0) continue;
				double thick = d.Thickness.IsAuto ? Math.Max(1, size / 10) : d.Thickness.Resolve(size);
				if (thick <= 0) continue;
				var col = d.Color;
				if (!PrintBg(g.S)) { int dr = 255 - col.R, dg = 255 - col.G, db = 255 - col.B; if (dr * dr + dg * dg + db * db <= 65025) col = col.Dark(); }
				foreach (int bit in new[] { 1, 2, 4 }) {
					if ((lines & bit) == 0) continue;
					if (under && bit == 4 || !under && bit != 4) continue;
					double y;
					if (bit == 1) {
						double off = f.UnderlinePos != 0 ? -f.UnderlinePos * size / f.UnitsPerEm : size / 10;
						if (!d.Offset.IsAuto) off = d.Offset.Resolve(size) ;
						y = g.Baseline + Math.Max(1, off) + thick / 2;
					}
					else if (bit == 2) y = g.Baseline - rf.Asc + thick / 2;
					else {
						double sp = f.StrikePos > 0 ? f.StrikePos * size / f.UnitsPerEm : rf.XH / 2;
						y = g.Baseline - sp + thick / 2;
					}
					DrawDecoLine(g.X0, g.X1, y, thick, d.Style, col);
				}
			}
		}

		void DrawDecoLine(double x0, double x1, double y, double t, byte style, Rgba c) {
			if (x1 <= x0) return;
			switch (style) {
				case 1:
					Fill(c);
					_c.Op(F.N(x0) + " " + F.N(y - t / 2) + " " + F.N(x1 - x0) + " " + F.N(t) + " re f");
					_c.Op(F.N(x0) + " " + F.N(y + t * 1.5) + " " + F.N(x1 - x0) + " " + F.N(t) + " re f");
					break;
				case 2: case 3:
					Stroke(c);
					_c.Op(F.N(t) + " w " + (style == 2 ? "1 J [0 " + F.N(t * 2) + "] 0 d" : "[" + F.N(t * 3) + " " + F.N(t * 3) + "] 0 d") + " " + P(x0, y) + " m " + P(x1, y) + " l S [] 0 d 0 J");
					break;
				case 4: {
					Stroke(c);
					double a = t * 1.5, step = t * 3;
					var sb = new StringBuilder(F.N(t) + " w " + P(x0, y) + " m");
					bool up = true;
					for (double x = x0; x < x1; x += step) {
						double xe = Math.Min(x1, x + step);
						sb.Append(' ').Append(P(x + step / 2, y + (up ? -a : a) * 2)).Append(' ').Append(P(x + step / 2, y + (up ? -a : a) * 2)).Append(' ').Append(P(xe, y)).Append(" c");
						up = !up;
					}
					sb.Append(" S");
					_c.Op(sb.ToString());
					break;
				}
				default:
					Fill(c);
					_c.Op(F.N(x0) + " " + F.N(y - t / 2) + " " + F.N(x1 - x0) + " " + F.N(t) + " re f");
					break;
			}
			if (c.A < 255) ResetAlpha();
		}

		double[] TransformMatrix(Box b) {
			var list = ParseTransform(b.S.Transform, b.W, b.H);
			if (list == null) return null;
			double ox = b.X + b.S.TransformOriginX.Resolve(b.W), oy = b.Y + b.S.TransformOriginY.Resolve(b.H);
			var m = Mul(new double[] { 1, 0, 0, 1, ox, oy }, Mul(list, new double[] { 1, 0, 0, 1, -ox, -oy }));
			return m;
		}

		public static double[] Mul(double[] a, double[] b) => new[] {
			a[0] * b[0] + a[2] * b[1], a[1] * b[0] + a[3] * b[1],
			a[0] * b[2] + a[2] * b[3], a[1] * b[2] + a[3] * b[3],
			a[0] * b[4] + a[2] * b[5] + a[4], a[1] * b[4] + a[3] * b[5] + a[5]
		};

		public static double[] ParseTransform(string t, double w, double h) {
			if (string.IsNullOrWhiteSpace(t)) return null;
			var m = new double[] { 1, 0, 0, 1, 0, 0 };
			int i = 0;
			string s = t.Trim();
			var ctx = new LenCtx { Em = 16, Rem = 16 };
			while (i < s.Length) {
				while (i < s.Length && (char.IsWhiteSpace(s[i]) || s[i] == ',')) i++;
				int st = i;
				while (i < s.Length && s[i] != '(') i++;
				if (i >= s.Length) break;
				string fn = s.Substring(st, i - st).Trim().ToLowerInvariant();
				int e = s.IndexOf(')', i);
				if (e < 0) break;
				var args = s.Substring(i + 1, e - i - 1).Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				i = e + 1;
				double L(int k, double basis) { if (k >= args.Length) return 0; var l = Val.ParseLen(args[k], ctx, true); return l?.Resolve(basis) ?? 0; }
				double A(int k) { if (k >= args.Length) return 0; double a = Val.ParseAngle(args[k]); return double.IsNaN(a) ? 0 : a * Math.PI / 180; }
				double N(int k, double def) => k < args.Length && Val.Num(args[k], out double v) ? v : k < args.Length && args[k].EndsWith("%") && Val.Num(args[k][..^1], out double pv) ? pv / 100 : def;
				double[] f;
				switch (fn) {
					case "matrix": f = new[] { N(0, 1), N(1, 0), N(2, 0), N(3, 1), N(4, 0), N(5, 0) }; break;
					case "matrix3d": f = new[] { N(0, 1), N(1, 0), N(4, 0), N(5, 1), N(12, 0), N(13, 0) }; break;
					case "translate": f = new[] { 1, 0, 0, 1, L(0, w), args.Length > 1 ? L(1, h) : 0 }; break;
					case "translate3d": f = new[] { 1, 0, 0, 1, L(0, w), L(1, h) }; break;
					case "translatex": f = new[] { 1, 0, 0, 1, L(0, w), 0.0 }; break;
					case "translatey": f = new[] { 1, 0, 0, 1, 0.0, L(0, h) }; break;
					case "scale": { double sx = N(0, 1), sy = args.Length > 1 ? N(1, 1) : sx; f = new[] { sx, 0, 0, sy, 0.0, 0.0 }; break; }
					case "scale3d": f = new[] { N(0, 1), 0, 0, N(1, 1), 0.0, 0.0 }; break;
					case "scalex": f = new[] { N(0, 1), 0, 0, 1, 0.0, 0.0 }; break;
					case "scaley": f = new[] { 1, 0, 0, N(0, 1), 0.0, 0.0 }; break;
					case "rotate": case "rotatez": { double a = A(args.Length == 4 ? 3 : 0); f = new[] { Math.Cos(a), Math.Sin(a), -Math.Sin(a), Math.Cos(a), 0, 0 }; break; }
					case "skew": f = new[] { 1, Math.Tan(args.Length > 1 ? A(1) : 0), Math.Tan(A(0)), 1, 0, 0 }; break;
					case "skewx": f = new[] { 1, 0, Math.Tan(A(0)), 1, 0, 0 }; break;
					case "skewy": f = new[] { 1, Math.Tan(A(0)), 0, 1, 0, 0 }; break;
					default: continue;
				}
				m = Mul(m, f);
			}
			return m;
		}

		void ApplyClipPath(Box b) {
			string cp = b.S.ClipPath.Trim();
			string lc = cp.ToLowerInvariant();
			double x = b.X, y = b.Y, w = b.W, h = b.H;
			var ctx = new LenCtx { Em = b.S.FontSize, Rem = 16 };
			int p = lc.IndexOf('(');
			if (p < 0) return;
			string fn = lc.Substring(0, p);
			string inner = cp.Substring(p + 1, cp.LastIndexOf(')') - p - 1);
			switch (fn) {
				case "inset": {
					var parts = Css.SplitWs(inner).Where(t => t != "," && !t.Equals("round", StringComparison.OrdinalIgnoreCase)).ToList();
					var v = parts.Select(t => Val.ParseLen(t, ctx)?.Resolve(w) ?? 0).ToList();
					double t0 = v.Count > 0 ? v[0] : 0, r0 = v.Count > 1 ? v[1] : t0, b0 = v.Count > 2 ? v[2] : t0, l0 = v.Count > 3 ? v[3] : r0;
					_c.Op(F.N(x + l0) + " " + F.N(y + t0) + " " + F.N(w - l0 - r0) + " " + F.N(h - t0 - b0) + " re W n");
					break;
				}
				case "circle": case "ellipse": {
					string shape = inner, at = null;
					int ai = inner.IndexOf(" at ", StringComparison.OrdinalIgnoreCase);
					if (ai >= 0) { shape = inner.Substring(0, ai); at = inner.Substring(ai + 4); }
					else if (inner.TrimStart().StartsWith("at ", StringComparison.OrdinalIgnoreCase)) { shape = ""; at = inner.TrimStart().Substring(3); }
					Len px = Len.PctV(50), py = Len.PctV(50);
					if (at != null) StyleResolver.ParsePosition(at, ctx, out px, out py);
					double cx = x + px.Resolve(w), cy = y + py.Resolve(h);
					var rs = Css.SplitWs(shape.Trim()).Where(t => t.Length > 0).ToList();
					double rx, ry;
					if (fn == "circle") { rx = ry = rs.Count > 0 ? Val.ParseLen(rs[0], ctx)?.Resolve(Math.Sqrt(w * w + h * h) / Math.Sqrt(2)) ?? Math.Min(w, h) / 2 : Math.Min(w, h) / 2; }
					else { rx = rs.Count > 0 ? Val.ParseLen(rs[0], ctx)?.Resolve(w) ?? w / 2 : w / 2; ry = rs.Count > 1 ? Val.ParseLen(rs[1], ctx)?.Resolve(h) ?? h / 2 : h / 2; }
					PathRoundRect(cx - rx, cy - ry, 2 * rx, 2 * ry, new[] { rx, ry, rx, ry, rx, ry, rx, ry });
					_c.Op("W n");
					break;
				}
				case "polygon": {
					var pts = Css.SplitTopLevel(inner, ',').Select(t => t.Trim()).Where(t => t.Length > 0 && t != "nonzero" && t != "evenodd").ToList();
					var sb = new StringBuilder();
					bool first = true;
					foreach (var pt in pts) {
						var xy = Css.SplitWs(pt);
						if (xy.Count < 2) continue;
						double px2 = x + (Val.ParseLen(xy[0], ctx)?.Resolve(w) ?? 0), py2 = y + (Val.ParseLen(xy[1], ctx)?.Resolve(h) ?? 0);
						sb.Append(P(px2, py2)).Append(first ? " m " : " l ");
						first = false;
					}
					if (!first) _c.Op(sb + "h W n");
					break;
				}
			}
		}
	}

	internal sealed class ImageData {
		public int PixW, PixH;
		public double W, H;
		public byte[] Jpeg;
		public int Components = 3;
		public bool AdobeInverted;
		public byte[] Rgb, Alpha;
		public bool Gray;
		public int Orientation = 1;
		public Element SvgRoot;
	}

	internal static class Images {
		public static ImageData Decode(byte[] d) {
			if (d == null || d.Length < 8) return null;
			try {
				if (d[0] == 0xFF && d[1] == 0xD8) return Jpeg(d);
				if (d[0] == 0x89 && d[1] == 'P' && d[2] == 'N' && d[3] == 'G') return Png(d);
				if (d[0] == 'G' && d[1] == 'I' && d[2] == 'F') return Gif(d);
				if (d[0] == 'B' && d[1] == 'M') return Bmp(d, 14, false);
				if (d[0] == 0 && d[1] == 0 && d[2] == 1 && d[3] == 0) return Ico(d);
				if (d[0] == 'R' && d[1] == 'I' && d[2] == 'F' && d[3] == 'F' && d.Length > 12 && d[8] == 'W' && d[9] == 'E' && d[10] == 'B' && d[11] == 'P') return WebP.Decode(d);
				int s = 0;
				while (s < d.Length && s < 512 && (char.IsWhiteSpace((char)d[s]) || d[s] == 0xEF || d[s] == 0xBB || d[s] == 0xBF)) s++;
				string head = Encoding.UTF8.GetString(d, s, Math.Min(d.Length - s, 2048));
				if (head.StartsWith("<?xml") || head.StartsWith("<svg") || head.StartsWith("<!DOCTYPE svg") || head.Contains("<svg")) return SvgImage(Encoding.UTF8.GetString(d));
			}
			catch { }
			return null;
		}

		static ImageData SvgImage(string text) {
			var doc = HtmlParser.Parse(text);
			var svg = doc.All.FirstOrDefault(e => e.Tag == "svg");
			if (svg == null) return null;
			Svg.IntrinsicSize(svg, out double w, out double h);
			return new ImageData { SvgRoot = svg, W = w, H = h, PixW = (int)w, PixH = (int)h };
		}

		static ImageData Jpeg(byte[] d) {
			int p = 2;
			var img = new ImageData { Jpeg = d };
			while (p + 4 <= d.Length) {
				if (d[p] != 0xFF) { p++; continue; }
				int m = d[p + 1];
				if (m == 0xD8 || m >= 0xD0 && m <= 0xD7 || m == 0x01 || m == 0xFF) { p += m == 0xFF ? 1 : 2; continue; }
				int len = d[p + 2] << 8 | d[p + 3];
				int seg = p + 4;
				if (m == 0xE1 && len > 8 && Encoding.ASCII.GetString(d, seg, 4) == "Exif") img.Orientation = ExifOrientation(d, seg + 6, len - 8);
				else if (m == 0xEE && len >= 12 && Encoding.ASCII.GetString(d, seg, 5) == "Adobe") img.AdobeInverted = true;
				else if (m >= 0xC0 && m <= 0xCF && m != 0xC4 && m != 0xC8 && m != 0xCC) {
					img.PixH = d[seg + 1] << 8 | d[seg + 2];
					img.PixW = d[seg + 3] << 8 | d[seg + 4];
					img.Components = d[seg + 5];
				}
				if (m == 0xDA) break;
				p += 2 + len;
			}
			if (img.PixW == 0 || img.PixH == 0) return null;
			if (img.Components == 4) img.AdobeInverted = true;
			bool swap = img.Orientation >= 5;
			img.W = swap ? img.PixH : img.PixW;
			img.H = swap ? img.PixW : img.PixH;
			return img;
		}

		static int ExifOrientation(byte[] d, int t, int len) {
			if (t + 8 > d.Length) return 1;
			bool le = d[t] == 'I';
			int U16(int o) => le ? d[o] | d[o + 1] << 8 : d[o] << 8 | d[o + 1];
			int U32(int o) => le ? d[o] | d[o + 1] << 8 | d[o + 2] << 16 | d[o + 3] << 24 : d[o] << 24 | d[o + 1] << 16 | d[o + 2] << 8 | d[o + 3];
			int ifd = t + U32(t + 4);
			if (ifd + 2 > d.Length) return 1;
			int n = U16(ifd);
			for (int i = 0; i < n; i++) {
				int e = ifd + 2 + i * 12;
				if (e + 12 > d.Length) break;
				if (U16(e) == 0x0112) { int v = U16(e + 8); return v >= 1 && v <= 8 ? v : 1; }
			}
			return 1;
		}

		static ImageData Png(byte[] d) {
			int p = 8;
			int w = 0, h = 0, depth = 8, ct = 0, interlace = 0;
			byte[] plte = null, trns = null;
			var idat = new MemoryStream();
			while (p + 8 <= d.Length) {
				int len = (int)BE.U32(d, p);
				string type = Encoding.ASCII.GetString(d, p + 4, 4);
				int data = p + 8;
				if (data + len > d.Length) len = d.Length - data;
				switch (type) {
					case "IHDR": w = (int)BE.U32(d, data); h = (int)BE.U32(d, data + 4); depth = d[data + 8]; ct = d[data + 9]; interlace = d[data + 12]; break;
					case "PLTE": plte = d.AsSpan(data, len).ToArray(); break;
					case "tRNS": trns = d.AsSpan(data, len).ToArray(); break;
					case "IDAT": idat.Write(d, data, len); break;
				}
				if (type == "IEND") break;
				p = data + len + 4;
			}
			if (w <= 0 || h <= 0) return null;
			byte[] raw;
			idat.Position = 0;
			using (var z = new ZLibStream(idat, CompressionMode.Decompress))
			using (var o = new MemoryStream()) { z.CopyTo(o); raw = o.ToArray(); }
			int channels = ct switch { 0 => 1, 2 => 3, 3 => 1, 4 => 2, 6 => 4, _ => 1 };
			int bpp = Math.Max(1, channels * depth / 8);
			var samples = new ushort[w * h * channels];
			int rp = 0;
			void DecodePass(int px0, int py0, int dx, int dy) {
				int pw = (w - px0 + dx - 1) / dx, ph = (h - py0 + dy - 1) / dy;
				if (pw <= 0 || ph <= 0) return;
				int stride = (pw * channels * depth + 7) / 8;
				var prev = new byte[stride];
				var cur = new byte[stride];
				for (int y = 0; y < ph; y++) {
					if (rp >= raw.Length) return;
					int ft = raw[rp++];
					int take = Math.Min(stride, raw.Length - rp);
					Buffer.BlockCopy(raw, rp, cur, 0, take);
					rp += stride;
					for (int i = 0; i < stride; i++) {
						int a = i >= bpp ? cur[i - bpp] : 0, b = prev[i], c = i >= bpp ? prev[i - bpp] : 0;
						int v = cur[i];
						switch (ft) {
							case 1: v += a; break;
							case 2: v += b; break;
							case 3: v += (a + b) >> 1; break;
							case 4: { int pp = a + b - c, pa = Math.Abs(pp - a), pb = Math.Abs(pp - b), pc = Math.Abs(pp - c); v += pa <= pb && pa <= pc ? a : pb <= pc ? b : c; break; }
						}
						cur[i] = (byte)v;
					}
					for (int x = 0; x < pw; x++) {
						int ox = px0 + x * dx, oy = py0 + y * dy;
						int di = (oy * w + ox) * channels;
						for (int ch = 0; ch < channels; ch++) {
							int si = x * channels + ch;
							ushort sv;
							if (depth == 8) sv = cur[si];
							else if (depth == 16) sv = (ushort)(cur[si * 2] << 8 | cur[si * 2 + 1]);
							else {
								int bitPos = si * depth;
								int byteV = cur[bitPos >> 3];
								int shift = 8 - depth - (bitPos & 7);
								sv = (ushort)(byteV >> shift & ((1 << depth) - 1));
							}
							samples[di + ch] = sv;
						}
					}
					(prev, cur) = (cur, prev);
				}
			}
			if (interlace == 1) {
				int[] sx = { 0, 4, 0, 2, 0, 1, 0 }, sy = { 0, 0, 4, 0, 2, 0, 1 }, dxs = { 8, 8, 4, 4, 2, 2, 1 }, dys = { 8, 8, 8, 4, 4, 2, 2 };
				for (int pass = 0; pass < 7; pass++) DecodePass(sx[pass], sy[pass], dxs[pass], dys[pass]);
			}
			else DecodePass(0, 0, 1, 1);
			int n = w * h;
			var img = new ImageData { PixW = w, PixH = h, W = w, H = h };
			byte To8(ushort v) => depth == 16 ? (byte)(v >> 8) : depth == 8 ? (byte)v : (byte)(v * 255 / ((1 << depth) - 1));
			bool gray = ct == 0 || ct == 4;
			var rgb = new byte[n * (gray ? 1 : 3)];
			byte[] alpha = null;
			bool anyAlpha = false;
			if (ct == 4 || ct == 6 || trns != null) alpha = new byte[n];
			for (int i = 0; i < n; i++) {
				switch (ct) {
					case 0: {
						ushort v = samples[i];
						rgb[i] = To8(v);
						if (trns != null && trns.Length >= 2) { int tv = trns[0] << 8 | trns[1]; alpha[i] = v == tv ? (byte)0 : (byte)255; }
						break;
					}
					case 2: {
						ushort r = samples[i * 3], g = samples[i * 3 + 1], b = samples[i * 3 + 2];
						rgb[i * 3] = To8(r); rgb[i * 3 + 1] = To8(g); rgb[i * 3 + 2] = To8(b);
						if (trns != null && trns.Length >= 6) { bool m = r == (trns[0] << 8 | trns[1]) && g == (trns[2] << 8 | trns[3]) && b == (trns[4] << 8 | trns[5]); alpha[i] = m ? (byte)0 : (byte)255; }
						break;
					}
					case 3: {
						int idx = samples[i];
						if (plte != null && idx * 3 + 2 < plte.Length) { rgb[i * 3] = plte[idx * 3]; rgb[i * 3 + 1] = plte[idx * 3 + 1]; rgb[i * 3 + 2] = plte[idx * 3 + 2]; }
						if (alpha != null) alpha[i] = trns != null && idx < trns.Length ? trns[idx] : (byte)255;
						break;
					}
					case 4: rgb[i] = To8(samples[i * 2]); alpha[i] = To8(samples[i * 2 + 1]); break;
					case 6: rgb[i * 3] = To8(samples[i * 4]); rgb[i * 3 + 1] = To8(samples[i * 4 + 1]); rgb[i * 3 + 2] = To8(samples[i * 4 + 2]); alpha[i] = To8(samples[i * 4 + 3]); break;
				}
				if (alpha != null && alpha[i] != 255) anyAlpha = true;
			}
			if (ct == 3) gray = false;
			img.Gray = gray;
			img.Rgb = rgb;
			img.Alpha = anyAlpha ? alpha : null;
			return img;
		}

		static ImageData Gif(byte[] d) {
			int w = d[6] | d[7] << 8, h = d[8] | d[9] << 8;
			int flags = d[10];
			int p = 13;
			byte[] gct = null;
			if ((flags & 0x80) != 0) { int sz = 3 * (1 << ((flags & 7) + 1)); gct = d.AsSpan(p, sz).ToArray(); p += sz; }
			int transparent = -1;
			while (p < d.Length) {
				byte b = d[p++];
				if (b == 0x21) {
					byte label = d[p++];
					if (label == 0xF9 && d[p] >= 4) { if ((d[p + 1] & 1) != 0) transparent = d[p + 4]; }
					while (p < d.Length && d[p] != 0) p += d[p] + 1;
					p++;
					continue;
				}
				if (b == 0x2C) {
					int fx = d[p] | d[p + 1] << 8, fy = d[p + 2] | d[p + 3] << 8, fw = d[p + 4] | d[p + 5] << 8, fh = d[p + 6] | d[p + 7] << 8;
					int lf = d[p + 8];
					p += 9;
					byte[] ct = gct;
					if ((lf & 0x80) != 0) { int sz = 3 * (1 << ((lf & 7) + 1)); ct = d.AsSpan(p, sz).ToArray(); p += sz; }
					bool inter = (lf & 0x40) != 0;
					int minCode = d[p++];
					var data = new MemoryStream();
					while (p < d.Length && d[p] != 0) { int len = d[p]; data.Write(d, p + 1, Math.Min(len, d.Length - p - 1)); p += len + 1; }
					var idx = Lzw(data.ToArray(), minCode, fw * fh);
					var rgb = new byte[w * h * 3];
					var alpha = new byte[w * h];
					int[] rows = new int[fh];
					if (inter) {
						int r = 0;
						foreach (var (start, step) in new[] { (0, 8), (4, 8), (2, 4), (1, 2) }) for (int y = start; y < fh; y += step) rows[r++] = y;
					}
					else for (int y = 0; y < fh; y++) rows[y] = y;
					bool anyAlpha = transparent >= 0 || fw != w || fh != h || fx != 0 || fy != 0;
					for (int i = 0; i < fw * fh && i < idx.Length; i++) {
						int y = rows[i / fw], x = i % fw;
						int ox = fx + x, oy = fy + y;
						if (ox >= w || oy >= h) continue;
						int c = idx[i];
						int o = oy * w + ox;
						if (c == transparent) continue;
						if (ct != null && c * 3 + 2 < ct.Length) { rgb[o * 3] = ct[c * 3]; rgb[o * 3 + 1] = ct[c * 3 + 1]; rgb[o * 3 + 2] = ct[c * 3 + 2]; }
						alpha[o] = 255;
					}
					return new ImageData { PixW = w, PixH = h, W = w, H = h, Rgb = rgb, Alpha = anyAlpha ? alpha : null };
				}
				if (b == 0x3B) break;
			}
			return null;
		}

		static byte[] Lzw(byte[] data, int minCode, int count) {
			var output = new byte[count];
			int clear = 1 << minCode, eoi = clear + 1;
			int codeSize = minCode + 1;
			var prefix = new int[4096];
			var suffix = new byte[4096];
			var length = new int[4096];
			for (int i = 0; i < clear; i++) { prefix[i] = -1; suffix[i] = (byte)i; length[i] = 1; }
			int next = eoi + 1, old = -1, op = 0;
			int bitPos = 0, total = data.Length * 8;
			var stack = new byte[4097];
			while (bitPos + codeSize <= total && op < count) {
				int code = 0;
				for (int i = 0; i < codeSize; i++) { if ((data[(bitPos + i) >> 3] >> ((bitPos + i) & 7) & 1) != 0) code |= 1 << i; }
				bitPos += codeSize;
				if (code == clear) { codeSize = minCode + 1; next = eoi + 1; old = -1; continue; }
				if (code == eoi) break;
				int cur = code;
				byte firstChar;
				int sp = 0;
				if (code >= next) {
					if (old < 0) break;
					int c2 = old;
					while (c2 >= 0) { stack[sp++] = suffix[c2]; c2 = prefix[c2]; }
					firstChar = stack[sp - 1];
					for (int i = sp - 1; i >= 0 && op < count; i--) output[op++] = stack[i];
					if (op < count) output[op++] = firstChar;
				}
				else {
					int c2 = cur;
					while (c2 >= 0) { stack[sp++] = suffix[c2]; c2 = prefix[c2]; }
					firstChar = stack[sp - 1];
					for (int i = sp - 1; i >= 0 && op < count; i--) output[op++] = stack[i];
				}
				if (old >= 0 && next < 4096) {
					prefix[next] = old;
					suffix[next] = firstChar;
					length[next] = length[old] + 1;
					next++;
					if (next == 1 << codeSize && codeSize < 12) codeSize++;
				}
				old = code;
			}
			return output;
		}

		static ImageData Bmp(byte[] d, int dib, bool icoMask) {
			int pixOff = dib == 14 ? (int)(d[10] | d[11] << 8 | d[12] << 16 | d[13] << 24) : -1;
			int hs = d[dib] | d[dib + 1] << 8 | d[dib + 2] << 16 | d[dib + 3] << 24;
			int w, h, bpp, comp = 0, colors = 0;
			if (hs == 12) { w = d[dib + 4] | d[dib + 5] << 8; h = (short)(d[dib + 6] | d[dib + 7] << 8); bpp = d[dib + 10] | d[dib + 11] << 8; }
			else {
				w = BitConverter.ToInt32(d, dib + 4); h = BitConverter.ToInt32(d, dib + 8);
				bpp = d[dib + 14] | d[dib + 15] << 8; comp = BitConverter.ToInt32(d, dib + 16); colors = BitConverter.ToInt32(d, dib + 32);
			}
			if (icoMask) h /= 2;
			bool topDown = h < 0;
			h = Math.Abs(h);
			int palOff = dib + hs;
			uint rm = 0x00FF0000, gm = 0x0000FF00, bm = 0x000000FF, am = 0;
			if (comp == 3 || comp == 6) {
				if (hs >= 52) { rm = BitConverter.ToUInt32(d, dib + 40); gm = BitConverter.ToUInt32(d, dib + 44); bm = BitConverter.ToUInt32(d, dib + 48); if (hs >= 56) am = BitConverter.ToUInt32(d, dib + 52); }
				else { rm = BitConverter.ToUInt32(d, palOff); gm = BitConverter.ToUInt32(d, palOff + 4); bm = BitConverter.ToUInt32(d, palOff + 8); palOff += 12; }
			}
			else if (bpp == 32) am = 0xFF000000;
			if (bpp == 16 && comp != 3) { rm = 0x7C00; gm = 0x03E0; bm = 0x001F; }
			int palSize = bpp <= 8 ? (colors > 0 ? colors : 1 << bpp) : 0;
			int entry = hs == 12 ? 3 : 4;
			if (pixOff < 0) pixOff = palOff + palSize * entry;
			int stride = (w * bpp + 31) / 32 * 4;
			var rgb = new byte[w * h * 3];
			var alpha = new byte[w * h];
			bool anyAlpha = false;
			static int Shift(uint m) { int s = 0; if (m == 0) return 0; while ((m & 1) == 0) { m >>= 1; s++; } return s; }
			static int Bits(uint m) { int b = 0; while (m != 0) { b += (int)(m & 1); m >>= 1; } return b; }
			byte Chan(uint v, uint m) { if (m == 0) return 255; uint x = (v & m) >> Shift(m); int bits = Bits(m); return (byte)(bits >= 8 ? x >> (bits - 8) : x * 255 / ((1u << bits) - 1)); }
			for (int y = 0; y < h; y++) {
				int row = pixOff + (topDown ? y : h - 1 - y) * stride;
				for (int x = 0; x < w; x++) {
					int o = y * w + x;
					byte r, g, b, a = 255;
					if (bpp <= 8) {
						int bit = x * bpp;
						int idx = d[row + (bit >> 3)] >> (8 - bpp - (bit & 7)) & ((1 << bpp) - 1);
						int pe = palOff + idx * entry;
						b = d[pe]; g = d[pe + 1]; r = d[pe + 2];
					}
					else if (bpp == 24) { int pp = row + x * 3; b = d[pp]; g = d[pp + 1]; r = d[pp + 2]; }
					else if (bpp == 16) { uint v = (uint)(d[row + x * 2] | d[row + x * 2 + 1] << 8); r = Chan(v, rm); g = Chan(v, gm); b = Chan(v, bm); }
					else { uint v = BitConverter.ToUInt32(d, row + x * 4); r = Chan(v, rm); g = Chan(v, gm); b = Chan(v, bm); if (am != 0) a = Chan(v, am); }
					rgb[o * 3] = r; rgb[o * 3 + 1] = g; rgb[o * 3 + 2] = b;
					alpha[o] = a;
					if (a != 255) anyAlpha = true;
				}
			}
			if (bpp == 32 && anyAlpha && alpha.All(v => v == 0)) { anyAlpha = false; }
			if (icoMask && !anyAlpha) {
				int mstride = (w + 31) / 32 * 4;
				int moff = pixOff + stride * h;
				if (moff + mstride * h <= d.Length) {
					for (int y = 0; y < h; y++) {
						int row = moff + (topDown ? y : h - 1 - y) * mstride;
						for (int x = 0; x < w; x++) if ((d[row + (x >> 3)] >> (7 - (x & 7)) & 1) != 0) { alpha[y * w + x] = 0; anyAlpha = true; }
					}
				}
			}
			return new ImageData { PixW = w, PixH = h, W = w, H = h, Rgb = rgb, Alpha = anyAlpha ? alpha : null };
		}

		static ImageData Ico(byte[] d) {
			int n = d[4] | d[5] << 8;
			int best = -1, bestSize = -1;
			for (int i = 0; i < n; i++) {
				int e = 6 + i * 16;
				int w = d[e] == 0 ? 256 : d[e];
				int bpp = d[e + 6] | d[e + 7] << 8;
				int score = w * 100 + bpp;
				if (score > bestSize) { bestSize = score; best = i; }
			}
			if (best < 0) return null;
			int be = 6 + best * 16;
			int size = BitConverter.ToInt32(d, be + 8), off = BitConverter.ToInt32(d, be + 12);
			var sub = d.AsSpan(off, Math.Min(size, d.Length - off)).ToArray();
			if (sub.Length > 8 && sub[0] == 0x89 && sub[1] == 'P') return Png(sub);
			return Bmp(sub, 0, true);
		}
	}

	internal sealed class SvgState {
		public string Fill = "black", Stroke = "none";
		public double FillOpacity = 1, StrokeOpacity = 1, StrokeWidth = 1, Miter = 4, DashOffset;
		public bool EvenOdd;
		public int Cap, Join;
		public double[] Dash;
		public Rgba Color = Rgba.Black;
		public double FontSize = 16;
		public string FontFamily = "sans-serif";
		public int FontWeight = 400;
		public byte FontStyle;
		public string Anchor = "start";
		public bool Hidden, Rtl;
		public double Opacity = 1;
		public bool ClipRule;
		public double VpW = 300, VpH = 150;
		public SvgState Clone() => (SvgState)MemberwiseClone();
	}

	internal static class Svg {
		public static void IntrinsicSize(Element svg, out double w, out double h) {
			var vb = ViewBox(svg);
			double? aw = Len(svg.Attr("width")), ah = Len(svg.Attr("height"));
			if (svg.Style != null) {
				if (svg.Style.Width.IsFixed) aw = svg.Style.Width.Px;
				if (svg.Style.Height.IsFixed) ah = svg.Style.Height.Px;
			}
			if (aw != null && ah != null) { w = aw.Value; h = ah.Value; return; }
			if (vb != null && vb[2] > 0 && vb[3] > 0) {
				if (aw != null) { w = aw.Value; h = w * vb[3] / vb[2]; return; }
				if (ah != null) { h = ah.Value; w = h * vb[2] / vb[3]; return; }
				w = vb[2]; h = vb[3];
				if (svg.Parent != null && svg.Parent.Tag != "html") { w = 300; h = 300 * vb[3] / vb[2]; }
				return;
			}
			w = aw ?? 300; h = ah ?? 150;
		}

		static double? Len(string v, double pctBase = double.NaN, double em = 16) {
			if (string.IsNullOrWhiteSpace(v)) return null;
			v = v.Trim();
			if (v.EndsWith("%")) { if (double.IsNaN(pctBase)) return null; return Val.Num(v[..^1], out double p) ? p * pctBase / 100 : null; }
			if (Val.Num(v, out double d)) return d;
			var l = Val.ParseLen(v, new LenCtx { Em = em, Rem = 16 });
			return l?.IsValue == true ? l.Value.Px : null;
		}

		static double[] ViewBox(Element e) {
			var v = e.Attr("viewBox");
			if (v == null) return null;
			var p = v.Split(new[] { ' ', ',', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			if (p.Length != 4) return null;
			var r = new double[4];
			for (int i = 0; i < 4; i++) if (!Val.Num(p[i], out r[i])) return null;
			return r;
		}

		static double[] ViewMatrix(Element e, double[] vb, double w, double h) {
			if (vb == null || vb[2] <= 0 || vb[3] <= 0) return new double[] { 1, 0, 0, 1, 0, 0 };
			string par = (e.Attr("preserveAspectRatio") ?? "xMidYMid meet").Trim();
			double sx = w / vb[2], sy = h / vb[3];
			if (par.StartsWith("none")) return new[] { sx, 0, 0, sy, -vb[0] * sx, -vb[1] * sy };
			bool slice = par.Contains("slice");
			double s = slice ? Math.Max(sx, sy) : Math.Min(sx, sy);
			double tx = -vb[0] * s, ty = -vb[1] * s;
			double ex = w - vb[2] * s, ey = h - vb[3] * s;
			if (par.Contains("xMid")) tx += ex / 2; else if (par.Contains("xMax")) tx += ex;
			if (par.Contains("YMid")) ty += ey / 2; else if (par.Contains("YMax")) ty += ey;
			return new[] { s, 0, 0, s, tx, ty };
		}

		sealed class Ctx {
			public Painter P;
			public RenderContext R;
			public Element Root;
			public Dictionary<string, Element> Ids = new();
			public List<CssRule> Rules = new();
			public int Depth;
		}

		public static void Render(Painter p, Element svg, double x, double y, double w, double h, RenderContext rc) {
			var ctx = new Ctx { P = p, R = rc, Root = svg };
			Index(svg, ctx);
			for (var anc = svg; anc != null; anc = anc.Parent) if (anc.Parent == null) Index(anc, ctx);
			foreach (var st in AllDesc(svg).Where(e => e.Tag == "style")) {
				var sheet = new StyleSheet();
				int order = 100000;
				Css.ParseSheet(st.TextContent(), sheet, 1, new MediaEnv(), ref order, null, null);
				ctx.Rules.AddRange(sheet.Rules);
			}
			ctx.Rules.AddRange(rc.AuthorRules);
			var state = new SvgState();
			if (svg.Style != null) { state.Color = svg.Style.Color; state.FontSize = svg.Style.FontSize; state.FontFamily = string.Join(",", svg.Style.FontFamily); state.FontWeight = svg.Style.FontWeight; state.Rtl = svg.Style.Rtl; }
			state.VpW = w; state.VpH = h;
			var c = p.Content;
			c.Op("q");
			c.Op("1 0 0 1 " + F.N(x) + " " + F.N(y) + " cm");
			var vb = ViewBox(svg);
			var m = ViewMatrix(svg, vb, w, h);
			c.Op(string.Join(" ", m.Select(F.N)) + " cm");
			if (vb != null) { state.VpW = vb[2]; state.VpH = vb[3]; }
			ApplyStyles(svg, state, ctx);
			foreach (var ch in svg.ChildElements()) RenderNode(ch, state, ctx);
			c.Op("Q");
		}

		static IEnumerable<Element> AllDesc(Element e) { foreach (var d in e.Descendants()) yield return d; }

		static void Index(Element e, Ctx ctx) {
			foreach (var d in e.Descendants()) if (d.Id != null && !ctx.Ids.ContainsKey(d.Id)) ctx.Ids[d.Id] = d;
		}

		static void ApplyStyles(Element e, SvgState s, Ctx ctx) {
			foreach (var kv in e.Attrs) Prop(s, kv.Key, kv.Value, ctx);
			var matched = new List<(CssDecl d, int spec, int order, bool imp)>();
			foreach (var r in ctx.Rules) {
				if (r.Sel.PseudoElement != null) continue;
				if (!Css.Matches(r.Sel, e)) continue;
				foreach (var d in r.Decls) matched.Add((d, r.Sel.Spec, r.Order, d.Important));
			}
			foreach (var m in matched.OrderBy(x => x.imp).ThenBy(x => x.spec).ThenBy(x => x.order)) Prop(s, m.d.Prop, m.d.Value, ctx);
			var style = e.Attr("style");
			if (style != null) foreach (var d in Css.ParseDeclarations(style)) Prop(s, d.Prop, d.Value, ctx);
		}

		static void Prop(SvgState s, string name, string v, Ctx ctx) {
			v = v.Trim();
			string lv = v.ToLowerInvariant();
			if (lv == "inherit") return;
			switch (name) {
				case "fill": s.Fill = v; break;
				case "stroke": s.Stroke = v; break;
				case "fill-opacity": s.FillOpacity = Op(lv); break;
				case "stroke-opacity": s.StrokeOpacity = Op(lv); break;
				case "opacity": s.Opacity = Op(lv); break;
				case "stroke-width": s.StrokeWidth = Len(v, Math.Sqrt((s.VpW * s.VpW + s.VpH * s.VpH) / 2), s.FontSize) ?? s.StrokeWidth; break;
				case "stroke-linecap": s.Cap = lv == "round" ? 1 : lv == "square" ? 2 : 0; break;
				case "stroke-linejoin": s.Join = lv == "round" ? 1 : lv == "bevel" ? 2 : 0; break;
				case "stroke-miterlimit": if (Val.Num(lv, out double ml)) s.Miter = ml; break;
				case "stroke-dasharray": s.Dash = lv == "none" ? null : lv.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(t => Len(t, s.VpW, s.FontSize) ?? 0).ToArray(); break;
				case "stroke-dashoffset": s.DashOffset = Len(v, s.VpW, s.FontSize) ?? 0; break;
				case "fill-rule": s.EvenOdd = lv == "evenodd"; break;
				case "clip-rule": s.ClipRule = lv == "evenodd"; break;
				case "color": { var c = Css.ParseColor(v); if (c != null && !c.Value.Current) s.Color = c.Value; break; }
				case "font-size": s.FontSize = Len(v, s.FontSize, s.FontSize) ?? s.FontSize; break;
				case "font-family": s.FontFamily = v; break;
				case "font-weight": s.FontWeight = Css.ParseWeightKw(lv, s.FontWeight); break;
				case "font-style": s.FontStyle = (byte)(lv == "italic" || lv == "oblique" ? 1 : 0); break;
				case "text-anchor": s.Anchor = lv; break;
				case "direction": s.Rtl = lv == "rtl"; break;
				case "display": if (lv == "none") s.Hidden = true; break;
				case "visibility": s.Hidden = lv == "hidden" || lv == "collapse"; break;
				case "font": {
					foreach (var (k, val) in Shorthands.Expand("font", v, false)) Prop(s, k, val, ctx);
					break;
				}
			}
		}

		static double Op(string v) {
			if (v.EndsWith("%") && Val.Num(v[..^1], out double p)) return Math.Clamp(p / 100, 0, 1);
			return Val.Num(v, out double d) ? Math.Clamp(d, 0, 1) : 1;
		}

		static void RenderNode(Element e, SvgState parent, Ctx ctx) {
			if (ctx.Depth > 40) return;
			string tag = e.Tag;
			switch (tag) {
				case "defs": case "clipPath": case "mask": case "linearGradient": case "radialGradient": case "pattern": case "marker": case "title": case "desc": case "metadata": case "style": case "script": case "filter": case "symbol": case "foreignObject":
					return;
			}
			var s = parent.Clone();
			s.Opacity = 1;
			s.Hidden = parent.Hidden && false;
			ApplyStyles(e, s, ctx);
			if (s.Hidden && tag != "g" && tag != "svg") return;
			if (s.Hidden) return;
			var c = ctx.P.Content;
			c.Op("q");
			var tr = e.Attr("transform");
			if (tr != null) { var m = ParseSvgTransform(tr); c.Op(string.Join(" ", m.Select(F.N)) + " cm"); }
			var cp = e.Attr("clip-path") ?? StyleProp(e, "clip-path");
			if (cp != null && cp.StartsWith("url(")) ApplyClip(cp, e, s, ctx);
			bool group = s.Opacity < 1;
			PdfContent outer = null;
			if (group) { outer = ctx.P.Content; SwapContent(ctx, new PdfContent()); }
			ctx.Depth++;
			try {
				switch (tag) {
					case "g": case "a": foreach (var ch in e.ChildElements()) RenderNode(ch, s, ctx); break;
					case "switch": { var first = e.ChildElements().FirstOrDefault(); if (first != null) RenderNode(first, s, ctx); break; }
					case "svg": {
						double x = Len(e.Attr("x"), s.VpW) ?? 0, y = Len(e.Attr("y"), s.VpH) ?? 0;
						double w = Len(e.Attr("width"), s.VpW) ?? s.VpW, h = Len(e.Attr("height"), s.VpH) ?? s.VpH;
						var vb = ViewBox(e);
						var m = ViewMatrix(e, vb, w, h);
						var cc = ctx.P.Content;
						cc.Op(F.N(x) + " " + F.N(y) + " " + F.N(w) + " " + F.N(h) + " re W n");
						cc.Op("1 0 0 1 " + F.N(x) + " " + F.N(y) + " cm " + string.Join(" ", m.Select(F.N)) + " cm");
						var s2 = s.Clone();
						if (vb != null) { s2.VpW = vb[2]; s2.VpH = vb[3]; } else { s2.VpW = w; s2.VpH = h; }
						foreach (var ch in e.ChildElements()) RenderNode(ch, s2, ctx);
						break;
					}
					case "use": {
						string href = e.Attr("href") ?? e.Attr("xlink:href");
						if (href == null || !href.StartsWith("#") || !ctx.Ids.TryGetValue(href.Substring(1), out var target)) break;
						double x = Len(e.Attr("x"), s.VpW) ?? 0, y = Len(e.Attr("y"), s.VpH) ?? 0;
						var cc = ctx.P.Content;
						cc.Op("1 0 0 1 " + F.N(x) + " " + F.N(y) + " cm");
						if (target.Tag == "symbol" || target.Tag == "svg") {
							double w = Len(e.Attr("width"), s.VpW) ?? Len(target.Attr("width"), s.VpW) ?? s.VpW;
							double h = Len(e.Attr("height"), s.VpH) ?? Len(target.Attr("height"), s.VpH) ?? s.VpH;
							var vb = ViewBox(target);
							var m = ViewMatrix(target, vb, w, h);
							cc.Op(string.Join(" ", m.Select(F.N)) + " cm");
							var s2 = s.Clone();
							ApplyStyles(target, s2, ctx);
							if (vb != null) { s2.VpW = vb[2]; s2.VpH = vb[3]; }
							foreach (var ch in target.ChildElements()) RenderNode(ch, s2, ctx);
						}
						else RenderNode(target, s, ctx);
						break;
					}
					case "path": {
						var path = PathData(e.Attr("d") ?? "");
						DrawPath(path, s, ctx, e);
						break;
					}
					case "rect": {
						double x = Len(e.Attr("x"), s.VpW) ?? 0, y = Len(e.Attr("y"), s.VpH) ?? 0, w = Len(e.Attr("width"), s.VpW) ?? 0, h = Len(e.Attr("height"), s.VpH) ?? 0;
						if (w <= 0 || h <= 0) break;
						double? rx0 = Len(e.Attr("rx"), s.VpW), ry0 = Len(e.Attr("ry"), s.VpH);
						double rx = rx0 ?? ry0 ?? 0, ry = ry0 ?? rx0 ?? 0;
						rx = Math.Min(rx, w / 2); ry = Math.Min(ry, h / 2);
						var path = new List<(char, double[])>();
						if (rx <= 0 || ry <= 0) {
							path.Add(('M', new[] { x, y })); path.Add(('L', new[] { x + w, y })); path.Add(('L', new[] { x + w, y + h })); path.Add(('L', new[] { x, y + h })); path.Add(('Z', null));
						}
						else {
							const double k = 0.5522847498;
							path.Add(('M', new[] { x + rx, y }));
							path.Add(('L', new[] { x + w - rx, y }));
							path.Add(('C', new[] { x + w - rx + rx * k, y, x + w, y + ry - ry * k, x + w, y + ry }));
							path.Add(('L', new[] { x + w, y + h - ry }));
							path.Add(('C', new[] { x + w, y + h - ry + ry * k, x + w - rx + rx * k, y + h, x + w - rx, y + h }));
							path.Add(('L', new[] { x + rx, y + h }));
							path.Add(('C', new[] { x + rx - rx * k, y + h, x, y + h - ry + ry * k, x, y + h - ry }));
							path.Add(('L', new[] { x, y + ry }));
							path.Add(('C', new[] { x, y + ry - ry * k, x + rx - rx * k, y, x + rx, y }));
							path.Add(('Z', null));
						}
						DrawPath(path, s, ctx, e);
						break;
					}
					case "circle": case "ellipse": {
						double cx = Len(e.Attr("cx"), s.VpW) ?? 0, cy = Len(e.Attr("cy"), s.VpH) ?? 0;
						double rx, ry;
						if (tag == "circle") rx = ry = Len(e.Attr("r"), Math.Sqrt((s.VpW * s.VpW + s.VpH * s.VpH) / 2)) ?? 0;
						else { rx = Len(e.Attr("rx"), s.VpW) ?? 0; ry = Len(e.Attr("ry"), s.VpH) ?? 0; }
						if (rx <= 0 || ry <= 0) break;
						const double k = 0.5522847498;
						var path = new List<(char, double[])> {
							('M', new[] { cx + rx, cy }),
							('C', new[] { cx + rx, cy + ry * k, cx + rx * k, cy + ry, cx, cy + ry }),
							('C', new[] { cx - rx * k, cy + ry, cx - rx, cy + ry * k, cx - rx, cy }),
							('C', new[] { cx - rx, cy - ry * k, cx - rx * k, cy - ry, cx, cy - ry }),
							('C', new[] { cx + rx * k, cy - ry, cx + rx, cy - ry * k, cx + rx, cy }),
							('Z', null)
						};
						DrawPath(path, s, ctx, e);
						break;
					}
					case "line": {
						double x1 = Len(e.Attr("x1"), s.VpW) ?? 0, y1 = Len(e.Attr("y1"), s.VpH) ?? 0, x2 = Len(e.Attr("x2"), s.VpW) ?? 0, y2 = Len(e.Attr("y2"), s.VpH) ?? 0;
						var s2 = s.Clone(); s2.Fill = "none";
						DrawPath(new List<(char, double[])> { ('M', new[] { x1, y1 }), ('L', new[] { x2, y2 }) }, s2, ctx, e);
						break;
					}
					case "polyline": case "polygon": {
						var nums = Numbers(e.Attr("points") ?? "");
						if (nums.Count < 4) break;
						var path = new List<(char, double[])> { ('M', new[] { nums[0], nums[1] }) };
						for (int i = 2; i + 1 < nums.Count; i += 2) path.Add(('L', new[] { nums[i], nums[i + 1] }));
						if (tag == "polygon") path.Add(('Z', null));
						DrawPath(path, s, ctx, e);
						break;
					}
					case "text": RenderText(e, s, ctx); break;
					case "image": {
						string href = e.Attr("href") ?? e.Attr("xlink:href");
						if (href == null) break;
						var img = ctx.R.LoadImage(href);
						if (img == null) break;
						double x = Len(e.Attr("x"), s.VpW) ?? 0, y = Len(e.Attr("y"), s.VpH) ?? 0;
						double w = Len(e.Attr("width"), s.VpW) ?? img.W, h = Len(e.Attr("height"), s.VpH) ?? img.H;
						string par = e.Attr("preserveAspectRatio") ?? "xMidYMid meet";
						double dw = w, dh = h, dx = x, dy = y;
						if (!par.StartsWith("none") && img.W > 0 && img.H > 0) {
							double sc = par.Contains("slice") ? Math.Max(w / img.W, h / img.H) : Math.Min(w / img.W, h / img.H);
							dw = img.W * sc; dh = img.H * sc;
							dx = x + (par.Contains("xMid") || !par.Contains("xM") ? (w - dw) / 2 : par.Contains("xMax") ? w - dw : 0);
							dy = y + (par.Contains("YMid") || !par.Contains("YM") ? (h - dh) / 2 : par.Contains("YMax") ? h - dh : 0);
						}
						var cc = ctx.P.Content;
						cc.Op("q " + F.N(x) + " " + F.N(y) + " " + F.N(w) + " " + F.N(h) + " re W n");
						ctx.P.DrawImage(img, dx, dy, dw, dh);
						cc.Op("Q");
						break;
					}
				}
			}
			finally { ctx.Depth--; }
			if (group) {
				var inner = ctx.P.Content;
				SwapContent(ctx, outer);
				string form = ctx.P.Doc.AddForm(inner, -1e5, -1e5, 1e5, 1e5, true);
				outer.XObjects.Add(form);
				string gs = ctx.P.Doc.State(s.Opacity, s.Opacity);
				outer.States.Add(gs);
				outer.Op("/" + gs + " gs /" + form + " Do");
			}
			ctx.P.Content.Op("Q");
		}

		static string StyleProp(Element e, string prop) {
			var st = e.Attr("style");
			if (st == null) return null;
			foreach (var d in Css.ParseDeclarations(st)) if (d.Prop == prop) return d.Value;
			return null;
		}

		static void SwapContent(Ctx ctx, PdfContent c) => ctx.P.SetContent(c);

		static void ApplyClip(string url, Element e, SvgState s, Ctx ctx) {
			string id = url.Substring(4).TrimEnd(')').Trim().Trim('"', '\'').TrimStart('#');
			if (!ctx.Ids.TryGetValue(id, out var clip)) return;
			var sb = new StringBuilder();
			var c = ctx.P.Content;
			bool any = false;
			foreach (var ch in clip.ChildElements()) {
				List<(char, double[])> path = null;
				double x = Len(ch.Attr("x"), s.VpW) ?? 0, y = Len(ch.Attr("y"), s.VpH) ?? 0;
				switch (ch.Tag) {
					case "path": path = PathData(ch.Attr("d") ?? ""); break;
					case "rect": {
						double w = Len(ch.Attr("width"), s.VpW) ?? 0, h = Len(ch.Attr("height"), s.VpH) ?? 0;
						path = new List<(char, double[])> { ('M', new[] { x, y }), ('L', new[] { x + w, y }), ('L', new[] { x + w, y + h }), ('L', new[] { x, y + h }), ('Z', null) };
						break;
					}
					case "circle": case "ellipse": {
						double cx = Len(ch.Attr("cx"), s.VpW) ?? 0, cy = Len(ch.Attr("cy"), s.VpH) ?? 0;
						double rx = ch.Tag == "circle" ? Len(ch.Attr("r"), s.VpW) ?? 0 : Len(ch.Attr("rx"), s.VpW) ?? 0;
						double ry = ch.Tag == "circle" ? rx : Len(ch.Attr("ry"), s.VpH) ?? 0;
						const double k = 0.5522847498;
						path = new List<(char, double[])> { ('M', new[] { cx + rx, cy }), ('C', new[] { cx + rx, cy + ry * k, cx + rx * k, cy + ry, cx, cy + ry }), ('C', new[] { cx - rx * k, cy + ry, cx - rx, cy + ry * k, cx - rx, cy }), ('C', new[] { cx - rx, cy - ry * k, cx - rx * k, cy - ry, cx, cy - ry }), ('C', new[] { cx + rx * k, cy - ry, cx + rx, cy - ry * k, cx + rx, cy }), ('Z', null) };
						break;
					}
					case "polygon": {
						var nums = Numbers(ch.Attr("points") ?? "");
						if (nums.Count < 4) break;
						path = new List<(char, double[])> { ('M', new[] { nums[0], nums[1] }) };
						for (int i = 2; i + 1 < nums.Count; i += 2) path.Add(('L', new[] { nums[i], nums[i + 1] }));
						path.Add(('Z', null));
						break;
					}
				}
				if (path == null) continue;
				var tr = ch.Attr("transform");
				if (tr != null) path = TransformPath(path, ParseSvgTransform(tr));
				sb.Append(PathOps(path)).Append('\n');
				any = true;
			}
			if (any) c.Op(sb + (s.ClipRule ? "W* n" : "W n"));
		}

		static List<(char, double[])> TransformPath(List<(char, double[])> p, double[] m) {
			var res = new List<(char, double[])>();
			foreach (var (cmd, a) in p) {
				if (a == null) { res.Add((cmd, null)); continue; }
				var b = new double[a.Length];
				for (int i = 0; i + 1 < a.Length; i += 2) { b[i] = m[0] * a[i] + m[2] * a[i + 1] + m[4]; b[i + 1] = m[1] * a[i] + m[3] * a[i + 1] + m[5]; }
				res.Add((cmd, b));
			}
			return res;
		}

		static string PathOps(List<(char, double[])> path) {
			var sb = new StringBuilder();
			foreach (var (cmd, a) in path) {
				switch (cmd) {
					case 'M': sb.Append(F.N(a[0])).Append(' ').Append(F.N(a[1])).Append(" m "); break;
					case 'L': sb.Append(F.N(a[0])).Append(' ').Append(F.N(a[1])).Append(" l "); break;
					case 'C': sb.Append(F.N(a[0])).Append(' ').Append(F.N(a[1])).Append(' ').Append(F.N(a[2])).Append(' ').Append(F.N(a[3])).Append(' ').Append(F.N(a[4])).Append(' ').Append(F.N(a[5])).Append(" c "); break;
					case 'Z': sb.Append("h "); break;
				}
			}
			return sb.ToString();
		}

		static (double x0, double y0, double x1, double y1) BBox(List<(char, double[])> path) {
			double x0 = double.PositiveInfinity, y0 = double.PositiveInfinity, x1 = double.NegativeInfinity, y1 = double.NegativeInfinity;
			foreach (var (_, a) in path) {
				if (a == null) continue;
				for (int i = 0; i + 1 < a.Length; i += 2) { x0 = Math.Min(x0, a[i]); x1 = Math.Max(x1, a[i]); y0 = Math.Min(y0, a[i + 1]); y1 = Math.Max(y1, a[i + 1]); }
			}
			if (double.IsInfinity(x0)) return (0, 0, 0, 0);
			return (x0, y0, x1, y1);
		}

		static void DrawPath(List<(char, double[])> path, SvgState s, Ctx ctx, Element e) {
			if (path.Count == 0) return;
			var c = ctx.P.Content;
			string ops = PathOps(path);
			var bbox = BBox(path);
			bool fill = s.Fill != null && s.Fill.Trim().ToLowerInvariant() != "none";
			bool stroke = s.Stroke != null && s.Stroke.Trim().ToLowerInvariant() != "none" && s.StrokeWidth > 0;
			if (fill) {
				string f = s.Fill.Trim();
				if (f.StartsWith("url(")) {
					string fallback = f.Contains(')') ? f.Substring(f.IndexOf(')') + 1).Trim() : "";
					if (!PaintServer(f, ops, s.EvenOdd, bbox, s.FillOpacity, s, ctx) && fallback.Length > 0) { var col = Color(fallback, s); if (col != null) { ctx.P.Fill(col.Value.WithAlpha(col.Value.Alpha * s.FillOpacity)); c.Op(ops + (s.EvenOdd ? "f*" : "f")); } }
				}
				else {
					var col = Color(f, s);
					if (col != null && col.Value.A > 0) {
						ctx.P.Fill(col.Value.WithAlpha(col.Value.Alpha * s.FillOpacity));
						c.Op(ops + (s.EvenOdd ? "f*" : "f"));
						if (col.Value.A < 255 || s.FillOpacity < 1) ResetAlpha(ctx);
					}
				}
			}
			if (stroke) {
				string st = s.Stroke.Trim();
				var col = st.StartsWith("url(") ? GradientFallbackColor(st, ctx) : Color(st, s);
				if (col != null && col.Value.A > 0) {
					ctx.P.Stroke(col.Value.WithAlpha(col.Value.Alpha * s.StrokeOpacity));
					var sb = new StringBuilder();
					sb.Append(F.N(s.StrokeWidth)).Append(" w ").Append(s.Cap).Append(" J ").Append(s.Join).Append(" j ").Append(F.N(s.Miter)).Append(" M ");
					if (s.Dash != null && s.Dash.Length > 0 && s.Dash.Any(v => v > 0)) {
						var d = s.Dash.Length % 2 == 1 ? s.Dash.Concat(s.Dash).ToArray() : s.Dash;
						sb.Append('[').Append(string.Join(" ", d.Select(F.N))).Append("] ").Append(F.N(s.DashOffset)).Append(" d ");
					}
					c.Op(sb + ops + "S");
					if (s.Dash != null) c.Op("[] 0 d");
					if (col.Value.A < 255 || s.StrokeOpacity < 1) ResetAlpha(ctx);
				}
			}
		}

		static void ResetAlpha(Ctx ctx) {
			string gs = ctx.P.Doc.State(1, 1);
			ctx.P.Content.States.Add(gs);
			ctx.P.Content.Op("/" + gs + " gs");
		}

		static Rgba? Color(string v, SvgState s) {
			string lv = v.Trim().ToLowerInvariant();
			if (lv == "currentcolor") return s.Color;
			if (lv == "none" || lv == "transparent") return null;
			var c = Css.ParseColor(v);
			if (c == null) return null;
			return c.Value.Current ? s.Color : c.Value;
		}

		static Rgba? GradientFallbackColor(string url, Ctx ctx) {
			string id = url.Substring(4, url.IndexOf(')') - 4).Trim().Trim('"', '\'').TrimStart('#');
			if (!ctx.Ids.TryGetValue(id, out var g)) return null;
			var stop = Stops(g, ctx).FirstOrDefault();
			return stop.c;
		}

		static List<(double off, Rgba c)> Stops(Element g, Ctx ctx) {
			var stops = new List<(double, Rgba)>();
			var src = g;
			for (int guard = 0; guard < 8 && src != null; guard++) {
				var st = src.ChildElements().Where(x => x.Tag == "stop").ToList();
				if (st.Count > 0) {
					foreach (var s in st) {
						string off = s.Attr("offset") ?? StyleProp(s, "offset") ?? "0";
						double o = off.EndsWith("%") ? (Val.Num(off[..^1], out double p) ? p / 100 : 0) : Val.Num(off, out double d) ? d : 0;
						string col = StyleProp(s, "stop-color") ?? s.Attr("stop-color") ?? "black";
						string op = StyleProp(s, "stop-opacity") ?? s.Attr("stop-opacity") ?? "1";
						var c = Css.ParseColor(col) ?? Rgba.Black;
						if (c.Current) c = Rgba.Black;
						c = c.WithAlpha(c.Alpha * Op(op.ToLowerInvariant()));
						stops.Add((Math.Clamp(o, 0, 1), c));
					}
					break;
				}
				string href = src.Attr("href") ?? src.Attr("xlink:href");
				if (href == null || !href.StartsWith("#") || !ctx.Ids.TryGetValue(href.Substring(1), out src)) break;
			}
			for (int i = 1; i < stops.Count; i++) if (stops[i].Item1 < stops[i - 1].Item1) stops[i] = (stops[i - 1].Item1, stops[i].Item2);
			return stops;
		}

		static string GAttr(Element g, string name, Ctx ctx) {
			var src = g;
			for (int guard = 0; guard < 8 && src != null; guard++) {
				var v = src.Attr(name);
				if (v != null) return v;
				string href = src.Attr("href") ?? src.Attr("xlink:href");
				if (href == null || !href.StartsWith("#") || !ctx.Ids.TryGetValue(href.Substring(1), out src)) break;
			}
			return null;
		}

		static bool PaintServer(string url, string ops, bool evenOdd, (double x0, double y0, double x1, double y1) bb, double opacity, SvgState s, Ctx ctx) {
			string id = url.Substring(4, url.IndexOf(')') - 4).Trim().Trim('"', '\'').TrimStart('#');
			if (!ctx.Ids.TryGetValue(id, out var g)) return false;
			var stops = Stops(g, ctx);
			if (stops.Count == 0) return false;
			if (g.Tag != "linearGradient" && g.Tag != "radialGradient") return false;
			bool userSpace = (GAttr(g, "gradientUnits", ctx) ?? "") == "userSpaceOnUse";
			double bw = bb.x1 - bb.x0, bh = bb.y1 - bb.y0;
			double Coord(string v, double def, bool horiz) {
				if (v == null) return def;
				if (v.EndsWith("%")) { double p = Val.Num(v[..^1], out double pp) ? pp / 100 : 0; return userSpace ? p * (horiz ? s.VpW : s.VpH) : p; }
				return Len(v, horiz ? s.VpW : s.VpH) ?? def;
			}
			var c = ctx.P.Content;
			string fnDict = BuildStopsFn(stops, ctx, out bool alpha, false);
			string coords;
			if (g.Tag == "linearGradient") {
				double x1 = Coord(GAttr(g, "x1", ctx), 0, true), y1 = Coord(GAttr(g, "y1", ctx), 0, false), x2 = Coord(GAttr(g, "x2", ctx), userSpace ? s.VpW : 1, true), y2 = Coord(GAttr(g, "y2", ctx), 0, false);
				coords = F.N(x1) + " " + F.N(y1) + " " + F.N(x2) + " " + F.N(y2);
			}
			else {
				double cx = Coord(GAttr(g, "cx", ctx), userSpace ? s.VpW / 2 : 0.5, true), cy = Coord(GAttr(g, "cy", ctx), userSpace ? s.VpH / 2 : 0.5, false);
				double r = Coord(GAttr(g, "r", ctx), userSpace ? Math.Sqrt((s.VpW * s.VpW + s.VpH * s.VpH) / 2) / 2 : 0.5, true);
				double fx = Coord(GAttr(g, "fx", ctx), cx, true), fy = Coord(GAttr(g, "fy", ctx), cy, false);
				coords = F.N(fx) + " " + F.N(fy) + " 0 " + F.N(cx) + " " + F.N(cy) + " " + F.N(r);
			}
			string type = g.Tag == "linearGradient" ? "2" : "3";
			string sh = ctx.P.Doc.AddShading("<< /ShadingType " + type + " /ColorSpace /DeviceRGB /Coords [" + coords + "] /Function " + fnDict + " /Extend [true true] >>");
			c.Shadings.Add(sh);
			c.Op("q");
			c.Op(ops + (evenOdd ? "W* n" : "W n"));
			if (opacity < 1) { string gs = ctx.P.Doc.State(opacity, opacity); c.States.Add(gs); c.Op("/" + gs + " gs"); }
			if (!userSpace) c.Op(F.N(bw) + " 0 0 " + F.N(bh) + " " + F.N(bb.x0) + " " + F.N(bb.y0) + " cm");
			var gt = GAttr(g, "gradientTransform", ctx);
			if (gt != null) c.Op(string.Join(" ", ParseSvgTransform(gt).Select(F.N)) + " cm");
			if (alpha) {
				var mask = new PdfContent();
				string afn = BuildStopsFn(stops, ctx, out _, true);
				string ash = ctx.P.Doc.AddShading("<< /ShadingType " + type + " /ColorSpace /DeviceGray /Coords [" + coords + "] /Function " + afn + " /Extend [true true] >>");
				mask.Shadings.Add(ash);
				mask.Op("/" + ash + " sh");
				string form = ctx.P.Doc.AddForm(mask, -1e5, -1e5, 1e5, 1e5, true, true);
				string gs = ctx.P.Doc.State(1, 1, null, ctx.P.Doc.FormObj(form));
				c.States.Add(gs);
				c.Op("/" + gs + " gs");
			}
			c.Op("/" + sh + " sh");
			c.Op("Q");
			return true;
		}

		static string BuildStopsFn(List<(double off, Rgba c)> stops, Ctx ctx, out bool anyAlpha, bool alphaOnly) {
			anyAlpha = stops.Any(s => s.c.A < 255);
			var pts = stops.ToList();
			if (pts.Count == 1) pts.Add((1, pts[0].c));
			if (pts[0].off > 0) pts.Insert(0, (0, pts[0].c));
			if (pts[pts.Count - 1].off < 1) pts.Add((1, pts[pts.Count - 1].c));
			string Col(Rgba c) => alphaOnly ? F.N(c.A / 255.0) : F.N(c.R / 255.0) + " " + F.N(c.G / 255.0) + " " + F.N(c.B / 255.0);
			var fns = new List<int>();
			var bounds = new List<double>();
			for (int i = 0; i + 1 < pts.Count; i++) {
				fns.Add(ctx.P.Doc.AddFunction("<< /FunctionType 2 /Domain [0 1] /C0 [" + Col(pts[i].c) + "] /C1 [" + Col(pts[i + 1].c) + "] /N 1 >>"));
				if (i > 0) bounds.Add(pts[i].off);
			}
			if (fns.Count == 1) return fns[0] + " 0 R";
			return "<< /FunctionType 3 /Domain [0 1] /Functions [" + string.Join(" ", fns.Select(f => f + " 0 R")) + "] /Bounds [" + string.Join(" ", bounds.Select(F.N)) + "] /Encode [" + string.Join(" ", fns.Select(_ => "0 1")) + "] >>";
		}

		static void RenderText(Element e, SvgState s, Ctx ctx) {
			var runs = new List<(string text, SvgState st, double? x, double? y, double dx, double dy)>();
			void Collect(Element el, SvgState st) {
				double? x = Len(FirstNum(el.Attr("x")), st.VpW), y = Len(FirstNum(el.Attr("y")), st.VpH);
				double dx = Len(FirstNum(el.Attr("dx")), st.VpW) ?? 0, dy = Len(FirstNum(el.Attr("dy")), st.VpH) ?? 0;
				bool pending = true;
				foreach (var n in el.Children) {
					if (n is TextNode t) {
						string txt = HtmlParser.CollapseWs(t.Text);
						if (txt.Length == 0) continue;
						runs.Add((txt, st, pending ? x : null, pending ? y : null, pending ? dx : 0, pending ? dy : 0));
						pending = false;
					}
					else if (n is Element ce && (ce.Tag == "tspan" || ce.Tag == "a" || ce.Tag == "textPath")) {
						var cs = st.Clone();
						ApplyStyles(ce, cs, ctx);
						if (!cs.Hidden) Collect(ce, cs);
					}
				}
			}
			Collect(e, s);
			if (runs.Count == 0) return;
			if (runs[0].text.Length > 0) runs[0] = (runs[0].text.TrimStart(), runs[0].st, runs[0].x, runs[0].y, runs[0].dx, runs[0].dy);
			int last = runs.Count - 1;
			runs[last] = (runs[last].text.TrimEnd(), runs[last].st, runs[last].x, runs[last].y, runs[last].dx, runs[last].dy);
			double penX = 0, penY = 0;
			var chunk = new List<GlyphFrag>();
			double chunkStart = 0;
			string anchor = s.Anchor;
			bool rtl = s.Rtl;
			void FlushChunk() {
				if (chunk.Count == 0) return;
				double width = penX - chunkStart;
				double shift = anchor == "middle" ? -width / 2 : (anchor == "end") != rtl ? -width : 0;
				foreach (var g in chunk) { if (shift != 0) g.Shift(shift, 0); ctx.P.PaintGlyphs(g, false); }
				chunk.Clear();
			}
			foreach (var (text, st, x, y, dx, dy) in runs) {
				if (x != null || y != null) { FlushChunk(); if (x != null) penX = x.Value; if (y != null) penY = y.Value; chunkStart = penX; anchor = st.Anchor; rtl = st.Rtl; }
				penX += dx; penY += dy;
				if (text.Length == 0) continue;
				var style = new Style { FontSize = st.FontSize, FontWeight = st.FontWeight, FontStyle = st.FontStyle, Ws = WSp.Pre };
				style.FontFamily = Css.SplitTopLevel(st.FontFamily, ',').Select(f => Css.Unquote(f.Trim())).Where(f => f.Length > 0).ToArray();
				if (style.FontFamily.Length == 0) style.FontFamily = new[] { "sans-serif" };
				var col = st.Fill.Trim().StartsWith("url(") ? GradientFallbackColor(st.Fill.Trim(), ctx) : Color(st.Fill, st);
				if (col == null) { continue; }
				style.Color = col.Value.WithAlpha(col.Value.Alpha * st.FillOpacity);
				var frags = ctx.R.Layout.TextRun(text, style, penX, penY, out double w);
				chunk.AddRange(frags);
				penX += w;
			}
			FlushChunk();
		}

		static string FirstNum(string v) {
			if (v == null) return null;
			var p = v.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
			return p.Length > 0 ? p[0] : null;
		}

		public static List<double> Numbers(string s) {
			var res = new List<double>();
			int i = 0;
			while (i < s.Length) {
				while (i < s.Length && (char.IsWhiteSpace(s[i]) || s[i] == ',')) i++;
				if (i >= s.Length) break;
				int st = i;
				if (s[i] == '-' || s[i] == '+') i++;
				bool dot = false, exp = false;
				while (i < s.Length) {
					char c = s[i];
					if (char.IsDigit(c)) { i++; continue; }
					if (c == '.' && !dot && !exp) { dot = true; i++; continue; }
					if ((c == 'e' || c == 'E') && !exp) { exp = true; i++; if (i < s.Length && (s[i] == '-' || s[i] == '+')) i++; continue; }
					break;
				}
				if (i == st) { i++; continue; }
				if (Val.Num(s.Substring(st, i - st), out double v)) res.Add(v);
			}
			return res;
		}

		public static List<(char, double[])> PathData(string d) {
			var res = new List<(char, double[])>();
			int i = 0;
			double cx = 0, cy = 0, sx = 0, sy = 0, lcx = 0, lcy = 0;
			char prev = ' ';
			char cmd = ' ';
			double Num() {
				while (i < d.Length && (char.IsWhiteSpace(d[i]) || d[i] == ',')) i++;
				int st = i;
				if (i < d.Length && (d[i] == '-' || d[i] == '+')) i++;
				bool dot = false, exp = false;
				while (i < d.Length) {
					char c = d[i];
					if (char.IsDigit(c)) { i++; continue; }
					if (c == '.' && !dot && !exp) { dot = true; i++; continue; }
					if ((c == 'e' || c == 'E') && !exp && i + 1 < d.Length && (char.IsDigit(d[i + 1]) || d[i + 1] == '-' || d[i + 1] == '+')) { exp = true; i++; if (d[i] == '-' || d[i] == '+') i++; continue; }
					break;
				}
				return Val.Num(d.Substring(st, i - st), out double v) ? v : 0;
			}
			double Flag() {
				while (i < d.Length && (char.IsWhiteSpace(d[i]) || d[i] == ',')) i++;
				if (i < d.Length && (d[i] == '0' || d[i] == '1')) { double v = d[i] - '0'; i++; return v; }
				return Num();
			}
			bool HasNum() {
				int j = i;
				while (j < d.Length && (char.IsWhiteSpace(d[j]) || d[j] == ',')) j++;
				return j < d.Length && (char.IsDigit(d[j]) || d[j] == '-' || d[j] == '+' || d[j] == '.');
			}
			while (i < d.Length) {
				while (i < d.Length && (char.IsWhiteSpace(d[i]) || d[i] == ',')) i++;
				if (i >= d.Length) break;
				if (char.IsLetter(d[i])) { cmd = d[i]; i++; }
				else if (cmd == ' ') break;
				bool rel = char.IsLower(cmd);
				char uc = char.ToUpperInvariant(cmd);
				switch (uc) {
					case 'M': {
						double x = Num(), y = Num();
						if (rel) { x += cx; y += cy; }
						res.Add(('M', new[] { x, y }));
						cx = sx = x; cy = sy = y;
						cmd = rel ? 'l' : 'L';
						prev = 'M';
						break;
					}
					case 'L': { double x = Num(), y = Num(); if (rel) { x += cx; y += cy; } res.Add(('L', new[] { x, y })); cx = x; cy = y; prev = 'L'; break; }
					case 'H': { double x = Num(); if (rel) x += cx; res.Add(('L', new[] { x, cy })); cx = x; prev = 'L'; break; }
					case 'V': { double y = Num(); if (rel) y += cy; res.Add(('L', new[] { cx, y })); cy = y; prev = 'L'; break; }
					case 'C': {
						double x1 = Num(), y1 = Num(), x2 = Num(), y2 = Num(), x = Num(), y = Num();
						if (rel) { x1 += cx; y1 += cy; x2 += cx; y2 += cy; x += cx; y += cy; }
						res.Add(('C', new[] { x1, y1, x2, y2, x, y }));
						lcx = x2; lcy = y2; cx = x; cy = y; prev = 'C';
						break;
					}
					case 'S': {
						double x2 = Num(), y2 = Num(), x = Num(), y = Num();
						if (rel) { x2 += cx; y2 += cy; x += cx; y += cy; }
						double x1 = prev == 'C' ? 2 * cx - lcx : cx, y1 = prev == 'C' ? 2 * cy - lcy : cy;
						res.Add(('C', new[] { x1, y1, x2, y2, x, y }));
						lcx = x2; lcy = y2; cx = x; cy = y; prev = 'C';
						break;
					}
					case 'Q': {
						double qx = Num(), qy = Num(), x = Num(), y = Num();
						if (rel) { qx += cx; qy += cy; x += cx; y += cy; }
						res.Add(('C', new[] { cx + 2.0 / 3 * (qx - cx), cy + 2.0 / 3 * (qy - cy), x + 2.0 / 3 * (qx - x), y + 2.0 / 3 * (qy - y), x, y }));
						lcx = qx; lcy = qy; cx = x; cy = y; prev = 'Q';
						break;
					}
					case 'T': {
						double x = Num(), y = Num();
						if (rel) { x += cx; y += cy; }
						double qx = prev == 'Q' ? 2 * cx - lcx : cx, qy = prev == 'Q' ? 2 * cy - lcy : cy;
						res.Add(('C', new[] { cx + 2.0 / 3 * (qx - cx), cy + 2.0 / 3 * (qy - cy), x + 2.0 / 3 * (qx - x), y + 2.0 / 3 * (qy - y), x, y }));
						lcx = qx; lcy = qy; cx = x; cy = y; prev = 'Q';
						break;
					}
					case 'A': {
						double rx = Math.Abs(Num()), ry = Math.Abs(Num()), rot = Num(), large = Flag(), sweep = Flag(), x = Num(), y = Num();
						if (rel) { x += cx; y += cy; }
						Arc(res, cx, cy, rx, ry, rot, large != 0, sweep != 0, x, y);
						cx = x; cy = y; prev = 'A';
						break;
					}
					case 'Z': res.Add(('Z', null)); cx = sx; cy = sy; prev = 'Z'; if (!HasNum()) { } break;
					default: i++; break;
				}
				if (uc == 'Z') continue;
				if (!HasNum() && i < d.Length && !char.IsLetter(d[Math.Min(i, d.Length - 1)])) i++;
			}
			return res;
		}

		static void Arc(List<(char, double[])> res, double x1, double y1, double rx, double ry, double phiDeg, bool large, bool sweep, double x2, double y2) {
			if (rx == 0 || ry == 0) { res.Add(('L', new[] { x2, y2 })); return; }
			if (x1 == x2 && y1 == y2) return;
			double phi = phiDeg * Math.PI / 180, cos = Math.Cos(phi), sin = Math.Sin(phi);
			double dx = (x1 - x2) / 2, dy = (y1 - y2) / 2;
			double x1p = cos * dx + sin * dy, y1p = -sin * dx + cos * dy;
			double lambda = x1p * x1p / (rx * rx) + y1p * y1p / (ry * ry);
			if (lambda > 1) { double s = Math.Sqrt(lambda); rx *= s; ry *= s; }
			double num = rx * rx * ry * ry - rx * rx * y1p * y1p - ry * ry * x1p * x1p;
			double den = rx * rx * y1p * y1p + ry * ry * x1p * x1p;
			double coef = den == 0 ? 0 : Math.Sqrt(Math.Max(0, num / den));
			if (large == sweep) coef = -coef;
			double cxp = coef * rx * y1p / ry, cyp = -coef * ry * x1p / rx;
			double cx = cos * cxp - sin * cyp + (x1 + x2) / 2, cy = sin * cxp + cos * cyp + (y1 + y2) / 2;
			double Ang(double ux, double uy, double vx, double vy) {
				double a = Math.Atan2(ux * vy - uy * vx, ux * vx + uy * vy);
				return a;
			}
			double t1 = Ang(1, 0, (x1p - cxp) / rx, (y1p - cyp) / ry);
			double dt = Ang((x1p - cxp) / rx, (y1p - cyp) / ry, (-x1p - cxp) / rx, (-y1p - cyp) / ry);
			if (!sweep && dt > 0) dt -= 2 * Math.PI;
			else if (sweep && dt < 0) dt += 2 * Math.PI;
			int segs = (int)Math.Ceiling(Math.Abs(dt) / (Math.PI / 2));
			double delta = dt / segs;
			double t = t1;
			for (int i = 0; i < segs; i++) {
				double a1 = t, a2 = t + delta;
				double k = 4.0 / 3 * Math.Tan((a2 - a1) / 4);
				double c1 = Math.Cos(a1), s1 = Math.Sin(a1), c2 = Math.Cos(a2), s2 = Math.Sin(a2);
				double ex1 = c1 - k * s1, ey1 = s1 + k * c1, ex2 = c2 + k * s2, ey2 = s2 - k * c2;
				(double, double) Map(double ux, double uy) => (cx + rx * ux * cos - ry * uy * sin, cy + rx * ux * sin + ry * uy * cos);
				var p1 = Map(ex1, ey1); var p2 = Map(ex2, ey2); var p3 = Map(c2, s2);
				res.Add(('C', new[] { p1.Item1, p1.Item2, p2.Item1, p2.Item2, p3.Item1, p3.Item2 }));
				t = a2;
			}
		}

		public static double[] ParseSvgTransform(string t) {
			var m = new double[] { 1, 0, 0, 1, 0, 0 };
			int i = 0;
			while (i < t.Length) {
				while (i < t.Length && (char.IsWhiteSpace(t[i]) || t[i] == ',')) i++;
				int st = i;
				while (i < t.Length && t[i] != '(') i++;
				if (i >= t.Length) break;
				string fn = t.Substring(st, i - st).Trim().ToLowerInvariant();
				int e = t.IndexOf(')', i);
				if (e < 0) break;
				var a = Numbers(t.Substring(i + 1, e - i - 1));
				i = e + 1;
				double A(int k, double def = 0) => k < a.Count ? a[k] : def;
				double[] f;
				switch (fn) {
					case "matrix": f = new[] { A(0, 1), A(1), A(2), A(3, 1), A(4), A(5) }; break;
					case "translate": f = new[] { 1, 0, 0, 1, A(0), A(1) }; break;
					case "scale": f = new[] { A(0, 1), 0, 0, a.Count > 1 ? a[1] : A(0, 1), 0, 0 }; break;
					case "rotate": {
						double r = A(0) * Math.PI / 180;
						f = new[] { Math.Cos(r), Math.Sin(r), -Math.Sin(r), Math.Cos(r), 0, 0 };
						if (a.Count >= 3) f = Painter.Mul(Painter.Mul(new double[] { 1, 0, 0, 1, a[1], a[2] }, f), new double[] { 1, 0, 0, 1, -a[1], -a[2] });
						break;
					}
					case "skewx": f = new[] { 1, 0, Math.Tan(A(0) * Math.PI / 180), 1, 0, 0 }; break;
					case "skewy": f = new[] { 1, Math.Tan(A(0) * Math.PI / 180), 0, 1, 0, 0 }; break;
					default: continue;
				}
				m = Painter.Mul(m, f);
			}
			return m;
		}
	}

	internal static class WebP {
		const string TablesZ = "eNqtk3dYU/cax9+jrEQQAgmBsEIgISGEqJG9oiCICAIaQRwoQqROBBwoco9bqatc18XHFhe2xL1wi4patLRSxIWIqFUvchVEZsav4URy+IM+ts9zf399zvu88/u+B8e/8tQbNOh1LYFXVn5Cr2pQz8fsvFeqh1UIaRFL6URNLwk8u/aDpq4RFWoxZt3djttnCF/sQhdqRUSG8zs7UNMLAqPnP0ZNbwjE0pSoRedQ8m2LqkmXl5td3voa6Ur8okFtOoffirrRB11n8llvUbVCl2G3CjXrHE6ub0WNDQTGrqxGN3TVMAz1DtROIiLHvO3yv/rrl/YXfbcneLXodt2RE8/2quqwMHHB/R2H6/ajehzb0YLqXhDtnN/cqH7+DN3RYtzMs59OXO26q7VjK9+pH976VK7FmTm/d1fcRne1DszJBz6VXOr4vcfh53bUqCs571QLettGJPNYVaGuLNONueajsrYKHdLitA0NyqdXCat16rG2i/uIati5LvTuPZEhd2sLetVIoEv0WeX1alTZ47BXq+9bIixn83vU8IpA0czLyptP0AtCByWq0fVwD+tEXwTYiJ1EL3SspnQqf7uPyrR43OxF6/ULXbe1XpE0Rd2+002HUSOOJXeoH5QSkR+TPndX65a1J/pm5+WbuilsPiqf1hLtVEhqUflFIu8oz6sfK56i1z0O15S9hesPdfW2szlX3w5mocerrm9Qk453kHvDXuqxo0yPzeQ+sXWq3gw1wd296DMV4X/33bWpeX/x2rOi5uMoj3fu3U/nXx9Dl/Agz/zaXaeaS9BjHMt4033nWscP2uTF816rqh+hY9owt+j9746caTvVs+7hz9pv3+l6psXcMddaS69312nRJmJVbWFJ67keh3MNXbXNxFp27HypelyJeo7Zfm7Jp9Ibmvoeh7g3ql/uEbtYLq/quFyO/qtFevJ/Wg8eRfd6xnzeiR7XEP3+eLxd9b6SmFSc/QhVP9DtIqdJ/eAhgZsn1Kgq7hI/pHtsUfP+C7qDIUX9TOKHPr/I196Jdj3Wt6h6sUut1ps/662NGk0vqvukaNdbP6r1qEF6RJ16VPax9nlkmFLTbwaNuvejm7Rr0N9/T0hsULWovwQ3drd0q79UJ7PVkz18UJNMTtHVf2VSk46/cNCbVf132b++fR309u4+xn+gw0WlPrBR+bk3UqlsVffKSjbZ1GctalKIPhvS9Fta0888f9HkP2n9//o6SezWqDT6FXfpBSb/gHYSVRryQDUq8jLIqD4zK/XURqKaFFWrqj6vul+lyDPp6mvW9HNz3V+buH+p3y4dMilr4dLdGbtPS/NKFnuGJx8ftTVWFj96zyxff6ri6RWa2e5MFu2Ax00G9WiOTexNVrCHVLYtgspT1GzjsEo8faw2FDn/GhFCN8nMvG9KeRnHHzT93AQHC9d1pZM2pqTNPrR99KXUVXsXsKNiR8oPzxwTzG/EBV6UUy2bWSZzxghZBZHFQ5l7R/C+KZIMd8rC08eZ2j1BCTTjhLAIj0yJ+UWmWDB4/rlSBu3GSJGt/NYVpqV1YuK/d7p7isorhTNKF8l4LqeD3Lx+TmAxhO8UvhxJeqbcYWq0m71M4X8oKUXKsc7+lckRFkfO8WazJFic0CnMdra1VOHakMmxYsjXXLYYmxtsGZMs8xs2RHI9zeWHRwsmeS4I3O50pPY1w3JBxsJo2WnEW5rJd8fK26qotvMShQ6bimu4wsKgEMc9oaIwrCo11kRYiElYEVtiYiwQzrmlEPAN5dWlBlidZBjtx6oypm18vpOrwq6Y5/FHaIL/T/FZrIDTIQFJZwKKp0YW8vmM7K1VHLfjYnOLUk79UFGpJ8251CjFnF3q52ltg2PeNO4IdmSWPc7EK/kDKVmSMkuj+gQuJSsgWsoQp/t4PKV4e616KU0Kr+QLmCw/b7+M84YCXnGNI4e1dnc2lb3OzcqrjpXJjkROlBGNA8yxxZFRbFNJGSoSShKdXELlMp+LMl8Gc0FApYhb5sMLz42UY5yUFSnuiemyRcfjkyd4TcJTF6aJosYtF0vwOAXmRzOMkPt5CYbynM1y/CJZmExADc2Pnp8kzsxiDKDKUTHTKM3BgirD08pZfpZUubyGQxcuopu7HsDDMJY8KILL8XJqLRsVGMGLXGL/nBsmwjmYeFCbIphmFV4g8wkO5wq5hakHZEVYCGXKBnspu0IWZU6hSNH5JRYTjewNpci2E/M1oa3ZiHyycX9zevx6P1YO/+Awp42Fa21dnkpmeaxevdzJ6PuJfOn67S9Epbg1i7a6uXAwViWyYEyWPjfGbvky3PYE5jPlNeNNzbzLUJjYXsaSyXCcXaCwHGQcsk1hYNDuYkctOKYwZeFXY11mnonN8xbJ8sQJJyQCOjstCv92cUE2LvMVmJ9oknEpwRGOtCWLEc0yP40XmO8TLmRtKXC1clSgGXb0fFFQcC0Wd1nKFtAX7dpCZxzIsDINuokPMbAxMDQyNqFQqYNMzQabW9BollZ0OoNhbc1k2tjasuzsHRyd2M4cF1cul+fGF7gLPUSenuIhQ4dJhnt5+/j6+QcEBgWHSEeMDA0bFR4xOnJM1Nix0THjYuPix0+QTUxInDR5avL0lNQ0efrsufMzMrMXL8vNw1etWbchf9PW77bvKtxrAIZgBMZgAhSgwiAwBTMYDOZgATSwBCugAwOsgQk2YAsssAN7cABHcAI2OAMHXMAVuMADN+CDANxBCB4gAk8QwxAYCsNAAsPBC7zBB3zBD/whEIJBCiMhDMJhNIyBsRADsRAPE2AiJEISTIFpMB1SIBXkkA6zYS7MhwzIhGxYAstgBeCwGtbBRtgEW6EAdsBu2APfwz44CIehBI7CSTgLF+AKXIdbUAGVUAU18ATqoAH+gEb4CG3QBRoYiJlgZpglxsTsMRtjpi3fyE1gzeK5+xr6+DHsvP25wtEGEZF0e6+A8DGuHomjooYHDkyYZOUwMclFFDZWNlkSlD5g1jeWjvLZHM8JU0Kj0+YMC16KLckZP5XmlDp3ZMziZc7iRcuHhsycFz8NsnNHjLNgp8wfkrUiLlmamTdjQezCf03P+BNQKV5u";
		static byte[] _coef, _update, _bmodes, _dc, _plane;
		static ushort[] _ac;
		static readonly object Sync = new();

		static void InitTables() {
			if (_coef != null) return;
			lock (Sync) {
				if (_coef != null) return;
				byte[] t;
				using (var ms = new MemoryStream(System.Convert.FromBase64String(TablesZ)))
				using (var z = new ZLibStream(ms, CompressionMode.Decompress))
				using (var o = new MemoryStream()) { z.CopyTo(o); t = o.ToArray(); }
				_update = t.AsSpan(1056, 1056).ToArray();
				_bmodes = t.AsSpan(2112, 900).ToArray();
				_dc = t.AsSpan(3012, 128).ToArray();
				var ac = new ushort[128];
				for (int i = 0; i < 128; i++) ac[i] = (ushort)(t[3140 + i * 2] | t[3141 + i * 2] << 8);
				_ac = ac;
				_plane = t.AsSpan(3396, 120).ToArray();
				_coef = t.AsSpan(0, 1056).ToArray();
			}
		}

		public static ImageData Decode(byte[] d) {
			try { return DecodeCore(d); } catch { return null; }
		}

		static ImageData DecodeCore(byte[] d) {
			InitTables();
			int p = 12;
			int aOff = -1, aLen = 0;
			while (p + 8 <= d.Length) {
				string tag = Encoding.ASCII.GetString(d, p, 4);
				int len = BitConverter.ToInt32(d, p + 4);
				int data = p + 8;
				if (len < 0 || data + len > d.Length) len = d.Length - data;
				if (tag == "ANMF") { p = data + 16; continue; }
				if (tag == "ALPH") { aOff = data; aLen = len; }
				else if (tag == "VP8L") return Lossless(d, data, len);
				else if (tag == "VP8 ") {
					var img = Lossy(d, data, len);
					if (img != null && aOff >= 0) {
						img.Alpha = DecodeAlpha(d, aOff, aLen, img.PixW, img.PixH);
						if (img.Alpha.All(v => v == 255)) img.Alpha = null;
					}
					return img;
				}
				p = data + len + (len & 1);
			}
			return null;
		}

		sealed class BitR {
			readonly byte[] d; int pos; readonly int end; ulong val; int bits;
			public BitR(byte[] d, int off, int len) { this.d = d; pos = off; end = off + len; }
			public int Read(int n) {
				if (n == 0) return 0;
				while (bits < n) { val |= (ulong)(pos < end ? d[pos] : 0) << bits; pos++; bits += 8; }
				int v = (int)(val & ((1UL << n) - 1));
				val >>= n; bits -= n;
				return v;
			}
		}

		sealed class Huff {
			public int Single = -1, MaxLen;
			public int[] First = new int[16], Count = new int[16], Offset = new int[16], Sorted;

			public static Huff Build(int[] lengths) {
				var h = new Huff();
				int nz = 0, last = 0;
				for (int s = 0; s < lengths.Length; s++) if (lengths[s] > 0) { nz++; last = s; }
				if (nz <= 1) { h.Single = last; return h; }
				foreach (var l in lengths) if (l > 0) { h.Count[l]++; if (l > h.MaxLen) h.MaxLen = l; }
				int c = 0, off = 0;
				for (int l = 1; l < 16; l++) { h.First[l] = c; h.Offset[l] = off; off += h.Count[l]; c = (c + h.Count[l]) << 1; }
				h.Sorted = new int[nz];
				var pos = (int[])h.Offset.Clone();
				for (int s = 0; s < lengths.Length; s++) if (lengths[s] > 0) h.Sorted[pos[lengths[s]]++] = s;
				return h;
			}

			public int Read(BitR br) {
				if (Single >= 0) return Single;
				int code = 0;
				for (int l = 1; l <= MaxLen; l++) {
					code = code << 1 | br.Read(1);
					int idx = code - First[l];
					if (idx >= 0 && idx < Count[l]) return Sorted[Offset[l] + idx];
				}
				return 0;
			}
		}

		static readonly int[] CodeLengthOrder = { 17, 18, 0, 1, 2, 3, 4, 5, 16, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

		static Huff ReadCode(BitR br, int alphabet) {
			var lengths = new int[alphabet];
			if (br.Read(1) == 1) {
				int num = br.Read(1) + 1;
				int s0 = br.Read(br.Read(1) == 1 ? 8 : 1);
				if (num == 1) return new Huff { Single = s0 };
				int s1 = br.Read(8);
				lengths[s0] = 1; lengths[s1] = 1;
				return Huff.Build(lengths);
			}
			var clen = new int[19];
			int n = 4 + br.Read(4);
			for (int i = 0; i < n; i++) clen[CodeLengthOrder[i]] = br.Read(3);
			var ch = Huff.Build(clen);
			int max = alphabet;
			if (br.Read(1) == 1) { int nb = 2 + 2 * br.Read(3); max = 2 + br.Read(nb); }
			int sym = 0, prev = 8;
			while (sym < alphabet) {
				if (max-- == 0) break;
				int c = ch.Read(br);
				if (c < 16) { lengths[sym++] = c; if (c != 0) prev = c; }
				else {
					int rep, val = 0;
					if (c == 16) { rep = 3 + br.Read(2); val = prev; }
					else if (c == 17) rep = 3 + br.Read(3);
					else rep = 11 + br.Read(7);
					for (int k = 0; k < rep && sym < alphabet; k++) lengths[sym++] = val;
				}
			}
			return Huff.Build(lengths);
		}

		static int Div(int n, int b) => (n + (1 << b) - 1) >> b;

		static int PrefixValue(BitR br, int prefix) {
			if (prefix < 4) return prefix + 1;
			int extra = (prefix - 2) >> 1;
			return ((2 + (prefix & 1)) << extra) + br.Read(extra) + 1;
		}

		static uint Add(uint a, uint b) => (((a & 0xff00ff00u) + (b & 0xff00ff00u)) & 0xff00ff00u) | (((a & 0x00ff00ffu) + (b & 0x00ff00ffu)) & 0x00ff00ffu);
		static uint Avg2(uint a, uint b) => (((a ^ b) & 0xfefefefeu) >> 1) + (a & b);
		static int Clip255(int v) => (v & ~0xff) == 0 ? v : v < 0 ? 0 : 255;
		static int Ch(uint v, int sh) => (int)(v >> sh & 0xff);

		static uint Select(uint t, uint l, uint tl) {
			int pa = 0;
			for (int sh = 0; sh < 32; sh += 8) pa += Math.Abs(Ch(l, sh) - Ch(tl, sh)) - Math.Abs(Ch(t, sh) - Ch(tl, sh));
			return pa <= 0 ? t : l;
		}

		static uint Full(uint a, uint b, uint c) {
			uint r = 0;
			for (int sh = 0; sh < 32; sh += 8) r |= (uint)Clip255(Ch(a, sh) + Ch(b, sh) - Ch(c, sh)) << sh;
			return r;
		}

		static uint Half(uint a, uint b) {
			uint r = 0;
			for (int sh = 0; sh < 32; sh += 8) { int x = Ch(a, sh), y = Ch(b, sh); r |= (uint)Clip255(x + (x - y) / 2) << sh; }
			return r;
		}

		sealed class Tr { public int Type, Bits, W; public uint[] Data; }

		static uint[] ImageStream(BitR br, int xsize, int ysize, bool level0) {
			var trs = new List<Tr>();
			int w = xsize;
			if (level0) {
				while (br.Read(1) == 1) {
					int type = br.Read(2);
					var tr = new Tr { Type = type, W = w };
					if (type == 0 || type == 1) { tr.Bits = br.Read(3) + 2; tr.Data = ImageStream(br, Div(w, tr.Bits), Div(ysize, tr.Bits), false); }
					else if (type == 3) {
						int n = br.Read(8) + 1;
						tr.Bits = n > 16 ? 0 : n > 4 ? 1 : n > 2 ? 2 : 3;
						var pal = ImageStream(br, n, 1, false);
						for (int i = 1; i < n; i++) pal[i] = Add(pal[i], pal[i - 1]);
						tr.Data = pal;
						w = Div(w, tr.Bits);
					}
					trs.Add(tr);
				}
			}
			int cacheBits = br.Read(1) == 1 ? br.Read(4) : 0;
			int hb = 0, groups = 1;
			uint[] himg = null;
			if (level0 && br.Read(1) == 1) {
				hb = br.Read(3) + 2;
				himg = ImageStream(br, Div(w, hb), Div(ysize, hb), false);
				foreach (var v in himg) groups = Math.Max(groups, (int)(v >> 8 & 0xffff) + 1);
			}
			int cacheSize = cacheBits > 0 ? 1 << cacheBits : 0;
			var codes = new Huff[groups, 5];
			for (int g = 0; g < groups; g++) {
				codes[g, 0] = ReadCode(br, 256 + 24 + cacheSize);
				codes[g, 1] = ReadCode(br, 256);
				codes[g, 2] = ReadCode(br, 256);
				codes[g, 3] = ReadCode(br, 256);
				codes[g, 4] = ReadCode(br, 40);
			}
			int total = w * ysize;
			var px = new uint[total];
			var cache = cacheSize > 0 ? new uint[cacheSize] : null;
			int hw = himg != null ? Div(w, hb) : 0;
			int pos = 0, lastCached = 0;
			while (pos < total) {
				int g = himg != null ? (int)(himg[(pos / w >> hb) * hw + (pos % w >> hb)] >> 8 & 0xffff) : 0;
				int s = codes[g, 0].Read(br);
				if (s < 256) {
					int r = codes[g, 1].Read(br), b = codes[g, 2].Read(br), a = codes[g, 3].Read(br);
					px[pos++] = (uint)(a << 24 | r << 16 | s << 8 | b);
				}
				else if (s < 280) {
					int length = PrefixValue(br, s - 256);
					int dcode = PrefixValue(br, codes[g, 4].Read(br));
					int dist;
					if (dcode > 120) dist = dcode - 120;
					else {
						int pc = _plane[dcode - 1];
						dist = (pc >> 4) * w + (8 - (pc & 0xf));
						if (dist < 1) dist = 1;
					}
					for (int k = 0; k < length && pos < total; k++, pos++) px[pos] = pos - dist >= 0 ? px[pos - dist] : 0;
				}
				else px[pos++] = cache != null && s - 280 < cacheSize ? cache[s - 280] : 0;
				if (cache != null) while (lastCached < pos) { uint c = px[lastCached++]; cache[(int)((0x1e35a7bdu * c) >> (32 - cacheBits))] = c; }
			}
			for (int ti = trs.Count - 1; ti >= 0; ti--) {
				var tr = trs[ti];
				int W = tr.W;
				switch (tr.Type) {
					case 2:
						for (int i = 0; i < px.Length; i++) {
							uint v = px[i]; uint gg = v >> 8 & 0xff;
							px[i] = (v & 0xff00ff00u) | (((v >> 16 & 0xff) + gg & 0xff) << 16) | ((v & 0xff) + gg & 0xff);
						}
						break;
					case 1: {
						int bw = Div(W, tr.Bits);
						for (int y = 0; y < ysize; y++) for (int x = 0; x < W; x++) {
							uint m = tr.Data[(y >> tr.Bits) * bw + (x >> tr.Bits)];
							sbyte g2r = (sbyte)(m & 0xff), g2b = (sbyte)(m >> 8 & 0xff), r2b = (sbyte)(m >> 16 & 0xff);
							int i = y * W + x;
							uint v = px[i];
							sbyte gg = (sbyte)(v >> 8 & 0xff);
							int nr = (int)(v >> 16 & 0xff) + (g2r * gg >> 5) & 0xff;
							int nb = (int)(v & 0xff) + (g2b * gg >> 5) + (r2b * (sbyte)nr >> 5) & 0xff;
							px[i] = (v & 0xff00ff00u) | (uint)(nr << 16) | (uint)nb;
						}
						break;
					}
					case 0: {
						int bw = Div(W, tr.Bits);
						for (int y = 0; y < ysize; y++) for (int x = 0; x < W; x++) {
							int i = y * W + x;
							uint pred;
							if (y == 0) pred = x == 0 ? 0xff000000u : px[i - 1];
							else if (x == 0) pred = px[i - W];
							else {
								int mode = (int)(tr.Data[(y >> tr.Bits) * bw + (x >> tr.Bits)] >> 8 & 0xf);
								uint L = px[i - 1], T = px[i - W], TL = px[i - W - 1], TR = px[i - W + 1];
								pred = mode switch {
									0 => 0xff000000u, 1 => L, 2 => T, 3 => TR, 4 => TL, 5 => Avg2(Avg2(L, TR), T), 6 => Avg2(L, TL), 7 => Avg2(L, T), 8 => Avg2(TL, T),
									9 => Avg2(T, TR), 10 => Avg2(Avg2(L, TL), Avg2(T, TR)), 11 => Select(T, L, TL), 12 => Full(L, T, TL), 13 => Half(Avg2(L, T), TL), _ => 0xff000000u
								};
							}
							px[i] = Add(px[i], pred);
						}
						break;
					}
					case 3: {
						var outp = new uint[W * ysize];
						int bitsPer = 8 >> tr.Bits, mask = (1 << bitsPer) - 1, ppb = 1 << tr.Bits;
						int pw = Div(W, tr.Bits);
						for (int y = 0; y < ysize; y++) for (int x = 0; x < W; x++) {
							int idx = (int)(px[y * pw + (x >> tr.Bits)] >> 8 & 0xff);
							if (tr.Bits > 0) idx = idx >> ((x & (ppb - 1)) * bitsPer) & mask;
							outp[y * W + x] = idx < tr.Data.Length ? tr.Data[idx] : 0;
						}
						px = outp;
						break;
					}
				}
			}
			return px;
		}

		static ImageData Lossless(byte[] d, int off, int len) {
			if (d[off] != 0x2f) return null;
			var br = new BitR(d, off + 1, len - 1);
			int w = br.Read(14) + 1, h = br.Read(14) + 1;
			br.Read(4);
			var px = ImageStream(br, w, h, true);
			var img = new ImageData { PixW = w, PixH = h, W = w, H = h, Rgb = new byte[w * h * 3], Alpha = new byte[w * h] };
			bool anyAlpha = false;
			for (int i = 0; i < w * h; i++) {
				uint v = px[i];
				img.Rgb[i * 3] = (byte)(v >> 16); img.Rgb[i * 3 + 1] = (byte)(v >> 8); img.Rgb[i * 3 + 2] = (byte)v;
				img.Alpha[i] = (byte)(v >> 24);
				if (img.Alpha[i] != 255) anyAlpha = true;
			}
			if (!anyAlpha) img.Alpha = null;
			return img;
		}

		static byte[] DecodeAlpha(byte[] d, int off, int len, int w, int h) {
			int hdr = d[off];
			int method = hdr & 3, filter = hdr >> 2 & 3;
			var a = new byte[w * h];
			if (method == 0) Buffer.BlockCopy(d, off + 1, a, 0, Math.Min(w * h, len - 1));
			else {
				var px = ImageStream(new BitR(d, off + 1, len - 1), w, h, true);
				for (int i = 0; i < w * h; i++) a[i] = (byte)(px[i] >> 8);
			}
			if (filter != 0)
				for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) {
					int i = y * w + x;
					int pred;
					if (x == 0 && y == 0) pred = 0;
					else if (y == 0) pred = a[i - 1];
					else if (x == 0) pred = a[i - w];
					else pred = filter switch { 1 => a[i - 1], 2 => a[i - w], _ => Clip255(a[i - 1] + a[i - w] - a[i - w - 1]) };
					a[i] = (byte)(a[i] + pred);
				}
			return a;
		}

		sealed class BoolDec {
			readonly byte[] d; int pos; readonly int end; uint value, range = 255; int bitCount;
			public BoolDec(byte[] d, int off, int len) { this.d = d; pos = off; end = off + len; value = (uint)(Next() << 8 | Next()); }
			int Next() => pos < end ? d[pos++] : 0;
			public int Bit(int prob) {
				uint split = 1 + ((range - 1) * (uint)prob >> 8);
				uint big = split << 8;
				int r;
				if (value >= big) { r = 1; range -= split; value -= big; }
				else { r = 0; range = split; }
				while (range < 128) {
					value <<= 1; range <<= 1;
					if (++bitCount == 8) { bitCount = 0; value |= (uint)Next(); }
				}
				return r;
			}
			public int Lit(int n) { int v = 0; while (n-- > 0) v = v << 1 | Bit(128); return v; }
			public int Signed(int n) { int v = Lit(n); return Bit(128) == 1 ? -v : v; }
		}

		static readonly int[] Zigzag = { 0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15 };
		static readonly int[] Bands = { 0, 1, 2, 3, 6, 4, 5, 6, 6, 6, 6, 6, 6, 6, 6, 7, 0 };
		static readonly int[] BModeTree = { 0, 1, -1, 2, -2, 3, 4, 6, -3, 5, -4, -5, -6, 7, -7, 8, -8, -9 };
		static readonly byte[][] Cat = { new byte[] { 173, 148, 140 }, new byte[] { 176, 155, 140, 135 }, new byte[] { 180, 157, 141, 134, 130 }, new byte[] { 254, 254, 243, 230, 196, 177, 153, 140, 133, 130, 129 } };

		static int Clip(int v, int max) => v < 0 ? 0 : v > max ? max : v;
		static int C8(int v) => v < 0 ? 0 : v > 255 ? 255 : v;

		static ImageData Lossy(byte[] d, int off, int len) {
			int bits = d[off] | d[off + 1] << 8 | d[off + 2] << 16;
			if ((bits & 1) != 0 || d[off + 3] != 0x9d || d[off + 4] != 0x01 || d[off + 5] != 0x2a) return null;
			int firstPart = bits >> 5;
			int width = (d[off + 6] | d[off + 7] << 8) & 0x3fff, height = (d[off + 8] | d[off + 9] << 8) & 0x3fff;
			int p0 = off + 10;
			var br = new BoolDec(d, p0, firstPart);
			br.Bit(128); br.Bit(128);
			bool useSeg = br.Bit(128) == 1, updateMap = false, absDelta = false;
			var segQ = new int[4]; var segF = new int[4]; var segP = new[] { 255, 255, 255 };
			if (useSeg) {
				updateMap = br.Bit(128) == 1;
				if (br.Bit(128) == 1) {
					absDelta = br.Bit(128) == 1;
					for (int s = 0; s < 4; s++) segQ[s] = br.Bit(128) == 1 ? br.Signed(7) : 0;
					for (int s = 0; s < 4; s++) segF[s] = br.Bit(128) == 1 ? br.Signed(6) : 0;
				}
				if (updateMap) for (int s = 0; s < 3; s++) segP[s] = br.Bit(128) == 1 ? br.Lit(8) : 255;
			}
			bool simple = br.Bit(128) == 1;
			int level = br.Lit(6), sharp = br.Lit(3);
			bool useLfDelta = br.Bit(128) == 1;
			var refD = new int[4]; var modeD = new int[4];
			if (useLfDelta && br.Bit(128) == 1) {
				for (int i = 0; i < 4; i++) if (br.Bit(128) == 1) refD[i] = br.Signed(6);
				for (int i = 0; i < 4; i++) if (br.Bit(128) == 1) modeD[i] = br.Signed(6);
			}
			int numParts = 1 << br.Lit(2);
			int partStart = p0 + firstPart;
			var parts = new BoolDec[numParts];
			int pp = partStart + 3 * (numParts - 1), endAll = off + len;
			for (int i = 0; i < numParts; i++) {
				int sz = i < numParts - 1 ? d[partStart + i * 3] | d[partStart + i * 3 + 1] << 8 | d[partStart + i * 3 + 2] << 16 : endAll - pp;
				sz = Math.Max(0, Math.Min(sz, endAll - pp));
				parts[i] = new BoolDec(d, pp, sz);
				pp += sz;
			}
			int baseQ = br.Lit(7);
			int dqY1dc = br.Bit(128) == 1 ? br.Signed(4) : 0, dqY2dc = br.Bit(128) == 1 ? br.Signed(4) : 0, dqY2ac = br.Bit(128) == 1 ? br.Signed(4) : 0;
			int dqUVdc = br.Bit(128) == 1 ? br.Signed(4) : 0, dqUVac = br.Bit(128) == 1 ? br.Signed(4) : 0;
			var qm = new int[4, 6];
			for (int s = 0; s < 4; s++) {
				int q = useSeg ? (absDelta ? segQ[s] : segQ[s] + baseQ) : baseQ;
				qm[s, 0] = _dc[Clip(q + dqY1dc, 127)];
				qm[s, 1] = _ac[Clip(q, 127)];
				qm[s, 2] = _dc[Clip(q + dqY2dc, 127)] * 2;
				qm[s, 3] = Math.Max(8, _ac[Clip(q + dqY2ac, 127)] * 101581 >> 16);
				qm[s, 4] = _dc[Clip(q + dqUVdc, 117)];
				qm[s, 5] = _ac[Clip(q + dqUVac, 127)];
			}
			br.Bit(128);
			var prob = (byte[])_coef.Clone();
			for (int i = 0; i < 1056; i++) if (br.Bit(_update[i]) == 1) prob[i] = (byte)br.Lit(8);
			bool useSkip = br.Bit(128) == 1;
			int skipP = useSkip ? br.Lit(8) : 0;
			int mbw = (width + 15) >> 4, mbh = (height + 15) >> 4;
			int yw = mbw * 16, uvw = mbw * 8;
			var Y = new byte[yw * mbh * 16];
			var U = new byte[uvw * mbh * 8];
			var V = new byte[uvw * mbh * 8];
			var intraT = new byte[mbw * 4];
			var nzT = new int[mbw * 4]; var nzTU = new int[mbw * 2]; var nzTV = new int[mbw * 2]; var nzTdc = new int[mbw];
			var mbSeg = new byte[mbw * mbh]; var mbI4 = new bool[mbw * mbh]; var mbInner = new bool[mbw * mbh];
			var coeffs = new short[384];
			var dcb = new short[16];
			var modes = new int[16];
			var bufY = new int[21 * 17];
			var bufC = new int[9 * 9];
			for (int my = 0; my < mbh; my++) {
				var intraL = new byte[4];
				var nzL = new int[4]; var nzLU = new int[2]; var nzLV = new int[2]; int nzLdc = 0;
				var tb = parts[my & (numParts - 1)];
				for (int mx = 0; mx < mbw; mx++) {
					int seg = updateMap ? (br.Bit(segP[0]) == 0 ? br.Bit(segP[1]) : br.Bit(segP[2]) + 2) : 0;
					bool skip = useSkip && br.Bit(skipP) == 1;
					bool i4 = br.Bit(145) == 0;
					int ymode = 0;
					if (!i4) {
						ymode = br.Bit(156) == 1 ? (br.Bit(128) == 1 ? 1 : 3) : (br.Bit(163) == 1 ? 2 : 0);
						for (int k = 0; k < 4; k++) { intraT[mx * 4 + k] = (byte)ymode; intraL[k] = (byte)ymode; }
					}
					else {
						for (int y = 0; y < 4; y++) {
							int lm = intraL[y];
							for (int x = 0; x < 4; x++) {
								int pb = (intraT[mx * 4 + x] * 10 + lm) * 9;
								int i = BModeTree[br.Bit(_bmodes[pb])];
								while (i > 0) i = BModeTree[2 * i + br.Bit(_bmodes[pb + i])];
								lm = -i;
								intraT[mx * 4 + x] = (byte)lm;
								modes[y * 4 + x] = lm;
							}
							intraL[y] = (byte)lm;
						}
					}
					int uvmode = br.Bit(142) == 0 ? 0 : br.Bit(114) == 0 ? 2 : br.Bit(183) == 1 ? 1 : 3;
					Array.Clear(coeffs);
					if (!skip) {
						int first = 0, acType = 3;
						if (!i4) {
							Array.Clear(dcb);
							int nz = Coeffs(tb, prob, 1, nzTdc[mx] + nzLdc, qm[seg, 2], qm[seg, 3], 0, dcb, 0);
							nzTdc[mx] = nzLdc = nz > 0 ? 1 : 0;
							if (nz > 1) Wht(dcb, coeffs);
							else { int dc0 = dcb[0] + 3 >> 3; for (int i = 0; i < 256; i += 16) coeffs[i] = (short)dc0; }
							first = 1; acType = 0;
						}
						for (int y = 0; y < 4; y++) for (int x = 0; x < 4; x++) {
							int nz = Coeffs(tb, prob, acType, nzL[y] + nzT[mx * 4 + x], qm[seg, 0], qm[seg, 1], first, coeffs, (y * 4 + x) * 16);
							nzL[y] = nzT[mx * 4 + x] = nz > first ? 1 : 0;
						}
						for (int ch = 0; ch < 2; ch++) {
							var tU = ch == 0 ? nzTU : nzTV; var lU = ch == 0 ? nzLU : nzLV;
							for (int y = 0; y < 2; y++) for (int x = 0; x < 2; x++) {
								int nz = Coeffs(tb, prob, 2, lU[y] + tU[mx * 2 + x], qm[seg, 4], qm[seg, 5], 0, coeffs, 256 + ch * 64 + (y * 2 + x) * 16);
								lU[y] = tU[mx * 2 + x] = nz > 0 ? 1 : 0;
								}
						}
					}
					else {
						for (int k = 0; k < 4; k++) { nzL[k] = 0; nzT[mx * 4 + k] = 0; }
						for (int k = 0; k < 2; k++) { nzLU[k] = nzLV[k] = 0; nzTU[mx * 2 + k] = nzTV[mx * 2 + k] = 0; }
						if (!i4) { nzLdc = 0; nzTdc[mx] = 0; }
					}
					int mi = my * mbw + mx;
					mbSeg[mi] = (byte)seg; mbI4[mi] = i4; mbInner[mi] = i4 || coeffs.Any(c => c != 0);
					ReconY(Y, yw, mx, my, mbw, i4, ymode, modes, coeffs, bufY);
					ReconUV(U, uvw, mx, my, uvmode, coeffs, 256, bufC);
					ReconUV(V, uvw, mx, my, uvmode, coeffs, 320, bufC);
				}
			}
			if (level > 0) LoopFilter(Y, U, V, yw, uvw, mbw, mbh, simple, level, sharp, useSeg, absDelta, segF, useLfDelta, refD, modeD, mbSeg, mbI4, mbInner);
			var img = new ImageData { PixW = width, PixH = height, W = width, H = height, Rgb = new byte[width * height * 3] };
			ToRgb(Y, U, V, yw, uvw, width, height, img.Rgb);
			return img;
		}

		static int Coeffs(BoolDec br, byte[] prob, int type, int ctx, int dcq, int acq, int n, short[] outp, int o) {
			int p = ((type * 8 + Bands[n]) * 3 + ctx) * 11;
			for (; n < 16; n++) {
				if (br.Bit(prob[p]) == 0) return n;
				while (br.Bit(prob[p + 1]) == 0) {
					if (++n == 16) return 16;
					p = (type * 8 + Bands[n]) * 3 * 11;
				}
				int v, next;
				if (br.Bit(prob[p + 2]) == 0) { v = 1; next = 1; }
				else {
					if (br.Bit(prob[p + 3]) == 0) v = br.Bit(prob[p + 4]) == 0 ? 2 : 3 + br.Bit(prob[p + 5]);
					else if (br.Bit(prob[p + 6]) == 0) {
						if (br.Bit(prob[p + 7]) == 0) v = 5 + br.Bit(159);
						else { v = 7 + 2 * br.Bit(165); v += br.Bit(145); }
					}
					else {
						int b1 = br.Bit(prob[p + 8]);
						int b0 = br.Bit(prob[p + 9 + b1]);
						int cat = 2 * b1 + b0;
						v = 0;
						foreach (var t in Cat[cat]) v += v + br.Bit(t);
						v += 3 + (8 << cat);
					}
					next = 2;
				}
				int sv = br.Bit(128) == 1 ? -v : v;
				outp[o + Zigzag[n]] = (short)(sv * (n > 0 ? acq : dcq));
				p = ((type * 8 + Bands[n + 1]) * 3 + next) * 11;
			}
			return 16;
		}

		static void Wht(short[] inp, short[] outp) {
			var tmp = new int[16];
			for (int i = 0; i < 4; i++) {
				int a0 = inp[i] + inp[12 + i], a1 = inp[4 + i] + inp[8 + i], a2 = inp[4 + i] - inp[8 + i], a3 = inp[i] - inp[12 + i];
				tmp[i] = a0 + a1; tmp[8 + i] = a0 - a1; tmp[4 + i] = a3 + a2; tmp[12 + i] = a3 - a2;
			}
			int o = 0;
			for (int i = 0; i < 4; i++) {
				int dc = tmp[i * 4] + 3;
				int a0 = dc + tmp[3 + i * 4], a1 = tmp[1 + i * 4] + tmp[2 + i * 4], a2 = tmp[1 + i * 4] - tmp[2 + i * 4], a3 = dc - tmp[3 + i * 4];
				outp[o] = (short)(a0 + a1 >> 3); outp[o + 16] = (short)(a3 + a2 >> 3); outp[o + 32] = (short)(a0 - a1 >> 3); outp[o + 48] = (short)(a3 - a2 >> 3);
				o += 64;
			}
		}

		static int Mul1(int a) => (a * 20091 >> 16) + a;
		static int Mul2(int a) => a * 35468 >> 16;

		static void Idct(short[] c, int o, int[] buf, int stride, int dst) {
			Span<int> tmp = stackalloc int[16];
			for (int i = 0; i < 4; i++) {
				int a = c[o + i] + c[o + 8 + i], b = c[o + i] - c[o + 8 + i];
				int cc = Mul2(c[o + 4 + i]) - Mul1(c[o + 12 + i]), dd = Mul1(c[o + 4 + i]) + Mul2(c[o + 12 + i]);
				tmp[i * 4] = a + dd; tmp[i * 4 + 1] = b + cc; tmp[i * 4 + 2] = b - cc; tmp[i * 4 + 3] = a - dd;
			}
			for (int i = 0; i < 4; i++) {
				int dc = tmp[i] + 4;
				int a = dc + tmp[8 + i], b = dc - tmp[8 + i];
				int cc = Mul2(tmp[4 + i]) - Mul1(tmp[12 + i]), dd = Mul1(tmp[4 + i]) + Mul2(tmp[12 + i]);
				int row = dst + i * stride;
				buf[row] = C8(buf[row] + (a + dd >> 3));
				buf[row + 1] = C8(buf[row + 1] + (b + cc >> 3));
				buf[row + 2] = C8(buf[row + 2] + (b - cc >> 3));
				buf[row + 3] = C8(buf[row + 3] + (a - dd >> 3));
			}
		}

		static int Avg3(int a, int b, int c) => a + 2 * b + c + 2 >> 2;
		static int Avg2i(int a, int b) => a + b + 1 >> 1;

		static void Predict4(int mode, int[] pr, int[] T, int[] L, int X) {
			int A = T[0], B = T[1], C = T[2], D = T[3], E = T[4], F = T[5], G = T[6], H = T[7];
			int I = L[0], J = L[1], K = L[2], LL = L[3];
			void P(int x, int y, int v) => pr[y * 4 + x] = v;
			switch (mode) {
				case 0: { int s = 4; for (int i = 0; i < 4; i++) s += T[i] + L[i]; s >>= 3; for (int i = 0; i < 16; i++) pr[i] = s; break; }
				case 1: for (int r = 0; r < 4; r++) for (int c = 0; c < 4; c++) pr[r * 4 + c] = C8(L[r] + T[c] - X); break;
				case 2: { int[] v = { Avg3(X, A, B), Avg3(A, B, C), Avg3(B, C, D), Avg3(C, D, E) }; for (int r = 0; r < 4; r++) for (int c = 0; c < 4; c++) pr[r * 4 + c] = v[c]; break; }
				case 3: { int[] v = { Avg3(X, I, J), Avg3(I, J, K), Avg3(J, K, LL), Avg3(K, LL, LL) }; for (int r = 0; r < 4; r++) for (int c = 0; c < 4; c++) pr[r * 4 + c] = v[r]; break; }
				case 4: {
					P(0, 3, Avg3(J, K, LL));
					int v = Avg3(I, J, K); P(1, 3, v); P(0, 2, v);
					v = Avg3(X, I, J); P(2, 3, v); P(1, 2, v); P(0, 1, v);
					v = Avg3(A, X, I); P(3, 3, v); P(2, 2, v); P(1, 1, v); P(0, 0, v);
					v = Avg3(B, A, X); P(3, 2, v); P(2, 1, v); P(1, 0, v);
					v = Avg3(C, B, A); P(3, 1, v); P(2, 0, v);
					P(3, 0, Avg3(D, C, B));
					break;
				}
				case 5: {
					int v = Avg2i(X, A); P(0, 0, v); P(1, 2, v);
					v = Avg2i(A, B); P(1, 0, v); P(2, 2, v);
					v = Avg2i(B, C); P(2, 0, v); P(3, 2, v);
					P(3, 0, Avg2i(C, D));
					P(0, 3, Avg3(K, J, I));
					P(0, 2, Avg3(J, I, X));
					v = Avg3(I, X, A); P(0, 1, v); P(1, 3, v);
					v = Avg3(X, A, B); P(1, 1, v); P(2, 3, v);
					v = Avg3(A, B, C); P(2, 1, v); P(3, 3, v);
					P(3, 1, Avg3(B, C, D));
					break;
				}
				case 6: {
					P(0, 0, Avg3(A, B, C));
					int v = Avg3(B, C, D); P(1, 0, v); P(0, 1, v);
					v = Avg3(C, D, E); P(2, 0, v); P(1, 1, v); P(0, 2, v);
					v = Avg3(D, E, F); P(3, 0, v); P(2, 1, v); P(1, 2, v); P(0, 3, v);
					v = Avg3(E, F, G); P(3, 1, v); P(2, 2, v); P(1, 3, v);
					v = Avg3(F, G, H); P(3, 2, v); P(2, 3, v);
					P(3, 3, Avg3(G, H, H));
					break;
				}
				case 7: {
					P(0, 0, Avg2i(A, B));
					int v = Avg2i(B, C); P(1, 0, v); P(0, 2, v);
					v = Avg2i(C, D); P(2, 0, v); P(1, 2, v);
					v = Avg2i(D, E); P(3, 0, v); P(2, 2, v);
					P(0, 1, Avg3(A, B, C));
					v = Avg3(B, C, D); P(1, 1, v); P(0, 3, v);
					v = Avg3(C, D, E); P(2, 1, v); P(1, 3, v);
					v = Avg3(D, E, F); P(3, 1, v); P(2, 3, v);
					P(3, 2, Avg3(E, F, G));
					P(3, 3, Avg3(F, G, H));
					break;
				}
				case 8: {
					int v = Avg2i(I, X); P(0, 0, v); P(2, 1, v);
					v = Avg2i(J, I); P(0, 1, v); P(2, 2, v);
					v = Avg2i(K, J); P(0, 2, v); P(2, 3, v);
					P(0, 3, Avg2i(LL, K));
					P(3, 0, Avg3(A, B, C));
					P(2, 0, Avg3(X, A, B));
					v = Avg3(I, X, A); P(1, 0, v); P(3, 1, v);
					v = Avg3(J, I, X); P(1, 1, v); P(3, 2, v);
					v = Avg3(K, J, I); P(1, 2, v); P(3, 3, v);
					P(1, 3, Avg3(LL, K, J));
					break;
				}
				default: {
					P(0, 0, Avg2i(I, J));
					int v = Avg2i(J, K); P(2, 0, v); P(0, 1, v);
					v = Avg2i(K, LL); P(2, 1, v); P(0, 2, v);
					P(1, 0, Avg3(I, J, K));
					v = Avg3(J, K, LL); P(3, 0, v); P(1, 1, v);
					v = Avg3(K, LL, LL); P(3, 1, v); P(1, 2, v);
					P(3, 2, LL); P(2, 2, LL); P(0, 3, LL); P(1, 3, LL); P(2, 3, LL); P(3, 3, LL);
					break;
				}
			}
		}

		static void ReconY(byte[] Y, int yw, int mx, int my, int mbw, bool i4, int ymode, int[] modes, short[] coeffs, int[] b) {
			const int S = 21;
			int x0 = mx * 16, y0 = my * 16;
			for (int c = -1; c < 20; c++) {
				int v;
				if (my == 0) v = 127;
				else if (c == -1) v = mx == 0 ? 129 : Y[(y0 - 1) * yw + x0 - 1];
				else if (c < 16) v = Y[(y0 - 1) * yw + x0 + c];
				else v = mx < mbw - 1 ? Y[(y0 - 1) * yw + x0 + c] : Y[(y0 - 1) * yw + x0 + 15];
				b[c + 1] = v;
			}
			if (my == 0) b[0] = 127;
			else if (mx == 0) b[0] = 129;
			for (int r = 0; r < 16; r++) b[(r + 1) * S] = mx == 0 ? 129 : Y[(y0 + r) * yw + x0 - 1];
			if (!i4) {
				if (ymode == 0) {
					int dc;
					if (mx == 0 && my == 0) dc = 128;
					else if (my == 0) { int s = 0; for (int r = 0; r < 16; r++) s += b[(r + 1) * S]; dc = s + 8 >> 4; }
					else if (mx == 0) { int s = 0; for (int c = 0; c < 16; c++) s += b[c + 1]; dc = s + 8 >> 4; }
					else { int s = 0; for (int k = 0; k < 16; k++) s += b[k + 1] + b[(k + 1) * S]; dc = s + 16 >> 5; }
					for (int r = 0; r < 16; r++) for (int c = 0; c < 16; c++) b[(r + 1) * S + c + 1] = dc;
				}
				else
					for (int r = 0; r < 16; r++) for (int c = 0; c < 16; c++)
						b[(r + 1) * S + c + 1] = ymode switch { 2 => b[c + 1], 3 => b[(r + 1) * S], _ => C8(b[(r + 1) * S] + b[c + 1] - b[0]) };
				for (int n = 0; n < 16; n++) Idct(coeffs, n * 16, b, S, ((n >> 2) * 4 + 1) * S + (n & 3) * 4 + 1);
			}
			else {
				var T = new int[8]; var L = new int[4]; var pr = new int[16];
				for (int n = 0; n < 16; n++) {
					int bx = (n & 3) * 4, by = (n >> 2) * 4;
					int dst = (by + 1) * S + bx + 1;
					for (int i = 0; i < 8; i++) T[i] = bx == 12 && i >= 4 ? b[16 + i - 4 + 1] : b[dst - S + i];
					for (int i = 0; i < 4; i++) L[i] = b[dst + i * S - 1];
					Predict4(modes[n], pr, T, L, b[dst - S - 1]);
					for (int r = 0; r < 4; r++) for (int c = 0; c < 4; c++) b[dst + r * S + c] = pr[r * 4 + c];
					Idct(coeffs, n * 16, b, S, dst);
				}
			}
			for (int r = 0; r < 16; r++) for (int c = 0; c < 16; c++) Y[(y0 + r) * yw + x0 + c] = (byte)b[(r + 1) * S + c + 1];
		}

		static void ReconUV(byte[] P, int w, int mx, int my, int mode, short[] coeffs, int co, int[] b) {
			const int S = 9;
			int x0 = mx * 8, y0 = my * 8;
			for (int c = -1; c < 8; c++) b[c + 1] = my == 0 ? 127 : c == -1 ? (mx == 0 ? 129 : P[(y0 - 1) * w + x0 - 1]) : P[(y0 - 1) * w + x0 + c];
			for (int r = 0; r < 8; r++) b[(r + 1) * S] = mx == 0 ? 129 : P[(y0 + r) * w + x0 - 1];
			int dc = 0;
			if (mode == 0) {
				if (mx == 0 && my == 0) dc = 128;
				else if (my == 0) { int s = 0; for (int r = 0; r < 8; r++) s += b[(r + 1) * S]; dc = s + 4 >> 3; }
				else if (mx == 0) { int s = 0; for (int c = 0; c < 8; c++) s += b[c + 1]; dc = s + 4 >> 3; }
				else { int s = 0; for (int k = 0; k < 8; k++) s += b[k + 1] + b[(k + 1) * S]; dc = s + 8 >> 4; }
			}
			for (int r = 0; r < 8; r++) for (int c = 0; c < 8; c++)
				b[(r + 1) * S + c + 1] = mode switch { 0 => dc, 2 => b[c + 1], 3 => b[(r + 1) * S], _ => C8(b[(r + 1) * S] + b[c + 1] - b[0]) };
			for (int n = 0; n < 4; n++) Idct(coeffs, co + n * 16, b, S, ((n >> 1) * 4 + 1) * S + (n & 1) * 4 + 1);
			for (int r = 0; r < 8; r++) for (int c = 0; c < 8; c++) P[(y0 + r) * w + x0 + c] = (byte)b[(r + 1) * S + c + 1];
		}

		static int SClip1(int v) => v < -128 ? -128 : v > 127 ? 127 : v;
		static int SClip2(int v) => v < -16 ? -16 : v > 15 ? 15 : v;
		static byte Clip1(int v) => (byte)(v < 0 ? 0 : v > 255 ? 255 : v);

		static void Filter2(byte[] p, int i, int s) {
			int p1 = p[i - 2 * s], p0 = p[i - s], q0 = p[i], q1 = p[i + s];
			int a = 3 * (q0 - p0) + SClip1(p1 - q1);
			int a1 = SClip2(a + 4 >> 3), a2 = SClip2(a + 3 >> 3);
			p[i - s] = Clip1(p0 + a2); p[i] = Clip1(q0 - a1);
		}

		static void Filter4(byte[] p, int i, int s) {
			int p1 = p[i - 2 * s], p0 = p[i - s], q0 = p[i], q1 = p[i + s];
			int a = 3 * (q0 - p0);
			int a1 = SClip2(a + 4 >> 3), a2 = SClip2(a + 3 >> 3), a3 = a1 + 1 >> 1;
			p[i - 2 * s] = Clip1(p1 + a3); p[i - s] = Clip1(p0 + a2); p[i] = Clip1(q0 - a1); p[i + s] = Clip1(q1 - a3);
		}

		static void Filter6(byte[] p, int i, int s) {
			int p2 = p[i - 3 * s], p1 = p[i - 2 * s], p0 = p[i - s], q0 = p[i], q1 = p[i + s], q2 = p[i + 2 * s];
			int a = SClip1(3 * (q0 - p0) + SClip1(p1 - q1));
			int a1 = 27 * a + 63 >> 7, a2 = 18 * a + 63 >> 7, a3 = 9 * a + 63 >> 7;
			p[i - 3 * s] = Clip1(p2 + a3); p[i - 2 * s] = Clip1(p1 + a2); p[i - s] = Clip1(p0 + a1);
			p[i] = Clip1(q0 - a1); p[i + s] = Clip1(q1 - a2); p[i + 2 * s] = Clip1(q2 - a3);
		}

		static bool Hev(byte[] p, int i, int s, int t) => Math.Abs(p[i - 2 * s] - p[i - s]) > t || Math.Abs(p[i + s] - p[i]) > t;
		static bool NeedsSimple(byte[] p, int i, int s, int t) => 4 * Math.Abs(p[i - s] - p[i]) + Math.Abs(p[i - 2 * s] - p[i + s]) <= t;

		static bool NeedsNormal(byte[] p, int i, int s, int t, int it) {
			int p3 = p[i - 4 * s], p2 = p[i - 3 * s], p1 = p[i - 2 * s], p0 = p[i - s], q0 = p[i], q1 = p[i + s], q2 = p[i + 2 * s], q3 = p[i + 3 * s];
			if (4 * Math.Abs(p0 - q0) + Math.Abs(p1 - q1) > t) return false;
			return Math.Abs(p3 - p2) <= it && Math.Abs(p2 - p1) <= it && Math.Abs(p1 - p0) <= it && Math.Abs(q3 - q2) <= it && Math.Abs(q2 - q1) <= it && Math.Abs(q1 - q0) <= it;
		}

		static void Edge(byte[] p, int i, int hs, int vs, int size, int th, int ith, int hev, bool mb) {
			int t2 = 2 * th + 1;
			for (int k = 0; k < size; k++, i += vs) {
				if (!NeedsNormal(p, i, hs, t2, ith)) continue;
				if (Hev(p, i, hs, hev)) Filter2(p, i, hs);
				else if (mb) Filter6(p, i, hs);
				else Filter4(p, i, hs);
			}
		}

		static void SimpleEdge(byte[] p, int i, int hs, int vs, int th) {
			int t2 = 2 * th + 1;
			for (int k = 0; k < 16; k++, i += vs) if (NeedsSimple(p, i, hs, t2)) Filter2(p, i, hs);
		}

		static void LoopFilter(byte[] Y, byte[] U, byte[] V, int yw, int uvw, int mbw, int mbh, bool simple, int lvl, int sharp, bool useSeg, bool absDelta, int[] segF, bool useLfDelta, int[] refD, int[] modeD, byte[] mbSeg, bool[] mbI4, bool[] mbInner) {
			var limit = new int[4, 2]; var ilev = new int[4, 2]; var hevT = new int[4, 2];
			for (int s = 0; s < 4; s++) {
				int baseL = useSeg ? (absDelta ? segF[s] : segF[s] + lvl) : lvl;
				for (int i4 = 0; i4 <= 1; i4++) {
					int level = baseL;
					if (useLfDelta) { level += refD[0]; if (i4 == 1) level += modeD[0]; }
					level = level < 0 ? 0 : level > 63 ? 63 : level;
					if (level == 0) continue;
					int il = level;
					if (sharp > 0) { il >>= sharp > 4 ? 2 : 1; if (il > 9 - sharp) il = 9 - sharp; }
					if (il < 1) il = 1;
					ilev[s, i4] = il;
					limit[s, i4] = 2 * level + il;
					hevT[s, i4] = level >= 40 ? 2 : level >= 15 ? 1 : 0;
				}
			}
			for (int my = 0; my < mbh; my++) for (int mx = 0; mx < mbw; mx++) {
				int mi = my * mbw + mx;
				int s = mbSeg[mi], i4 = mbI4[mi] ? 1 : 0;
				int lim = limit[s, i4];
				if (lim == 0) continue;
				int il = ilev[s, i4], hv = hevT[s, i4];
				bool inner = mbInner[mi];
				int yo = my * 16 * yw + mx * 16, uo = my * 8 * uvw + mx * 8;
				if (simple) {
					if (mx > 0) SimpleEdge(Y, yo, 1, yw, lim + 4);
					if (inner) for (int k = 4; k < 16; k += 4) SimpleEdge(Y, yo + k, 1, yw, lim);
					if (my > 0) SimpleEdge(Y, yo, yw, 1, lim + 4);
					if (inner) for (int k = 4; k < 16; k += 4) SimpleEdge(Y, yo + k * yw, yw, 1, lim);
					continue;
				}
				if (mx > 0) { Edge(Y, yo, 1, yw, 16, lim + 4, il, hv, true); Edge(U, uo, 1, uvw, 8, lim + 4, il, hv, true); Edge(V, uo, 1, uvw, 8, lim + 4, il, hv, true); }
				if (inner) {
					for (int k = 4; k < 16; k += 4) Edge(Y, yo + k, 1, yw, 16, lim, il, hv, false);
					Edge(U, uo + 4, 1, uvw, 8, lim, il, hv, false); Edge(V, uo + 4, 1, uvw, 8, lim, il, hv, false);
				}
				if (my > 0) { Edge(Y, yo, yw, 1, 16, lim + 4, il, hv, true); Edge(U, uo, uvw, 1, 8, lim + 4, il, hv, true); Edge(V, uo, uvw, 1, 8, lim + 4, il, hv, true); }
				if (inner) {
					for (int k = 4; k < 16; k += 4) Edge(Y, yo + k * yw, yw, 1, 16, lim, il, hv, false);
					Edge(U, uo + 4 * uvw, uvw, 1, 8, lim, il, hv, false); Edge(V, uo + 4 * uvw, uvw, 1, 8, lim, il, hv, false);
				}
			}
		}

		static int MultHi(int v, int c) => v * c >> 8;
		static byte YuvClip(int v) => (byte)((v & ~16383) == 0 ? v >> 6 : v < 0 ? 0 : 255);

		static void Put(byte[] rgb, int i, int y, int u, int v) {
			rgb[i] = YuvClip(MultHi(y, 19077) + MultHi(v, 26149) - 14234);
			rgb[i + 1] = YuvClip(MultHi(y, 19077) - MultHi(u, 6419) - MultHi(v, 13320) + 8708);
			rgb[i + 2] = YuvClip(MultHi(y, 19077) + MultHi(u, 33050) - 17685);
		}

		static void UpLine(byte[] Y, int topRow, int botRow, byte[] U, byte[] V, int tuRow, int cuRow, int yw, int uvw, int len, byte[] rgb, int w) {
			uint Ld(int row, int x) => (uint)(U[row * uvw + x] | V[row * uvw + x] << 16);
			void Emit(int row, int x, uint uv) => Put(rgb, (row * w + x) * 3, Y[row * yw + x], (int)(uv & 0xff), (int)(uv >> 16 & 0xff));
			uint tl = Ld(tuRow, 0), l = Ld(cuRow, 0);
			Emit(topRow, 0, 3 * tl + l + 0x00020002u >> 2);
			if (botRow >= 0) Emit(botRow, 0, 3 * l + tl + 0x00020002u >> 2);
			for (int x = 1; x <= (len - 1) >> 1; x++) {
				uint t = Ld(tuRow, x), uv = Ld(cuRow, x);
				uint avg = tl + t + l + uv + 0x00080008u;
				uint d12 = avg + 2 * (t + l) >> 3, d03 = avg + 2 * (tl + uv) >> 3;
				Emit(topRow, 2 * x - 1, (d12 + tl >> 1) & 0x00ff00ffu);
				Emit(topRow, 2 * x, (d03 + t >> 1) & 0x00ff00ffu);
				if (botRow >= 0) { Emit(botRow, 2 * x - 1, (d03 + l >> 1) & 0x00ff00ffu); Emit(botRow, 2 * x, (d12 + uv >> 1) & 0x00ff00ffu); }
				tl = t; l = uv;
			}
			if ((len & 1) == 0) {
				Emit(topRow, len - 1, 3 * tl + l + 0x00020002u >> 2);
				if (botRow >= 0) Emit(botRow, len - 1, 3 * l + tl + 0x00020002u >> 2);
			}
		}

		static void ToRgb(byte[] Y, byte[] U, byte[] V, int yw, int uvw, int w, int h, byte[] rgb) {
			UpLine(Y, 0, -1, U, V, 0, 0, yw, uvw, w, rgb, w);
			int y = 1;
			for (; y + 1 < h; y += 2) UpLine(Y, y, y + 1, U, V, (y - 1) >> 1, (y + 1) >> 1, yw, uvw, w, rgb, w);
			if (y < h) UpLine(Y, y, -1, U, V, (y - 1) >> 1, (y - 1) >> 1, yw, uvw, w, rgb, w);
		}
	}
}
