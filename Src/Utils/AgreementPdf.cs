using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SinaMN75U.Utils;

public static class AgreementPdf {
	private const string FontFamily = "Vazir";
	private const string DefaultBrand = "#2f2b8f";
	private const string Ink = "#1a1a1a";
	private const string Muted = "#555555";
	private const string Line = "#c9c9d4";

	private static readonly Lazy<byte[]> FallbackLogo = new(() => ReadResource("logo.jpg"));

	private static readonly Lazy<bool> Initialized = new(() => {
		QuestPDF.Settings.License = LicenseType.Community;
		using MemoryStream font = new(ReadResource("Vazir.ttf"));
		FontManager.RegisterFontWithCustomName(FontFamily, font);
		return true;
	});

	public static byte[] Build(AgreementTemplateEntity template, BrokerEntity broker, Dictionary<string, string> values, byte[]? signature) {
		_ = Initialized.Value;

		string brand = broker.JsonData.ThemeColor.IsNullOrEmpty() ? DefaultBrand : broker.JsonData.ThemeColor!;
		byte[] logo = Decode(broker.JsonData.LogoBase64) ?? FallbackLogo.Value;
		string title = template.JsonData.HeaderTitle.IsNullOrEmpty() ? AgreementContent.Title : template.JsonData.HeaderTitle!;

		return Document.Create(document => {
			document.Page(page => {
				page.Size(PageSizes.A4);
				page.Margin(24);
				page.DefaultTextStyle(x => x.FontFamily(FontFamily).FontSize(9).LineHeight(1.6f).FontColor(Ink));
				page.Content().ContentFromRightToLeft().Column(column => {
					column.Spacing(6);
					Header(column, values, title, brand, logo);
					Body(column, template, values, brand);
					Signatories(column, values, broker, signature);
				});
				page.Footer().AlignCenter().Text(x => {
					x.DefaultTextStyle(t => t.FontFamily(FontFamily).FontSize(8).FontColor(Muted));
					x.CurrentPageNumber();
					x.Span(" / ");
					x.TotalPages();
				});
			});
		}).GeneratePdf();
	}

	private static void Header(ColumnDescriptor column, Dictionary<string, string> values, string title, string brand, byte[] logo) {
		column.Item().Row(row => {
			row.ConstantItem(150).Column(meta => {
				meta.Item().Text($"تاریخ : {Value(values, "day")}/{Value(values, "month")}/{Value(values, "year")}").FontSize(8).FontColor(Muted);
				meta.Item().Text($"شماره : {Value(values, "number")}{values.GetValueOrDefault("contractNumberSuffix", "")}").FontSize(8).FontColor(Muted);
			});
			row.RelativeItem().AlignMiddle().Text(title).FontSize(12).Bold().FontColor(brand);
			row.ConstantItem(90).AlignLeft().Height(44).Image(logo).FitArea();
		});
		column.Item().PaddingBottom(6).LineHorizontal(2).LineColor(brand);
	}

	private static void Body(ColumnDescriptor column, AgreementTemplateEntity template, Dictionary<string, string> values, string brand) {
		foreach (AgreementTemplateBlock block in template.JsonData.Blocks.OrderBy(x => x.Order)) {
			string text = Fill(block.Text, values);
			switch (block.Type) {
				case AgreementBlockType.PageBreak:
					column.Item().PageBreak();
					break;
				case AgreementBlockType.Article:
					column.Item().PaddingTop(8).Text(text).FontSize(10).Bold().FontColor(brand);
					break;
				case AgreementBlockType.ArticleCenter:
					column.Item().PaddingTop(8).AlignCenter().Text(text).FontSize(10).Bold().FontColor(brand);
					break;
				case AgreementBlockType.Note:
					column.Item().PaddingRight(12).Text(text).FontSize(8).FontColor(Muted);
					break;
				case AgreementBlockType.NoteCenter:
					column.Item().PaddingTop(6).AlignCenter().Text(text).FontSize(8).FontColor(Muted);
					break;
				case AgreementBlockType.Sub:
					column.Item().PaddingRight(12).Text(text);
					break;
				default:
					column.Item().Text(text);
					break;
			}
		}
	}

	private static void Signatories(ColumnDescriptor column, Dictionary<string, string> values, BrokerEntity broker, byte[]? signature) {
		column.Item().PaddingTop(16).Row(row => {
			row.Spacing(12);
			row.RelativeItem().Border(1).BorderColor(Line).Padding(10).Column(representative => {
				representative.Item().Text(broker.JsonData.LegalName.IsNullOrEmpty() ? broker.Title : broker.JsonData.LegalName!).Bold();
				foreach (BrokerSignatory s in broker.JsonData.Signatories.OrderBy(x => x.Order ?? 0)) {
					representative.Item().PaddingTop(6).Text(s.Name ?? "");
					representative.Item().Text(s.Role ?? "").FontSize(8).FontColor(Muted);
					byte[]? stamp = Decode(s.SignatureBase64);
					if (stamp != null) representative.Item().PaddingTop(4).Height(40).AlignCenter().Image(stamp).FitArea();
				}
			});
			row.RelativeItem().Border(1).BorderColor(Line).Padding(10).Column(customer => {
				customer.Item().Text("نام و نام خانوادگی پذیرنده").Bold();
				customer.Item().PaddingTop(6).Text(Value(values, "fullName"));
				customer.Item().PaddingTop(6).Text("امضا و اثر انگشت").FontSize(8).FontColor(Muted);
				if (signature == null)
					customer.Item().Height(70);
				else
					customer.Item().PaddingTop(4).Height(70).AlignCenter().Image(signature).FitArea();
			});
		});
	}

	private static string Fill(string text, Dictionary<string, string> values) =>
		Regex.Replace(text, @"\{\{([A-Za-z0-9_]+)\}\}", m => values.GetValueOrDefault(m.Groups[1].Value, "---"));

	private static string Value(Dictionary<string, string> values, string key) => values.GetValueOrDefault(key, "---");

	private static byte[]? Decode(string? base64) {
		if (base64.IsNullOrEmpty()) return null;
		try {
			int i = base64!.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
			return Convert.FromBase64String(i >= 0 ? base64[(i + 7)..].Trim() : base64.Trim());
		}
		catch {
			return null;
		}
	}

	private static byte[] ReadResource(string endsWith) {
		Assembly assembly = typeof(AgreementPdf).Assembly;
		string name = assembly.GetManifestResourceNames().First(x => x.EndsWith(endsWith, StringComparison.OrdinalIgnoreCase));
		using Stream stream = assembly.GetManifestResourceStream(name)!;
		using MemoryStream memory = new();
		stream.CopyTo(memory);
		return memory.ToArray();
	}
}
